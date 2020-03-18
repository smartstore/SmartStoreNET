using System;
using System.Collections.Generic;
using System.Linq;
using NuGet;
using SmartStore.Collections;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Payments;
using SmartStore.Core.Events;
using SmartStore.Core.Plugins;
using SmartStore.Services.Configuration;
using SmartStore.Services.Media.Storage;

namespace SmartStore.Services.Media
{
    public partial class DownloadService : IDownloadService
    {
        private readonly IRepository<Download> _downloadRepository;
        private readonly IEventPublisher _eventPubisher;
        private readonly IPictureService _mediaService;
		private readonly Provider<IMediaStorageProvider> _storageProvider;

		public DownloadService(
			IRepository<Download> downloadRepository,
			IEventPublisher eventPubisher,
            IPictureService mediaService,
            ISettingService settingService,
			IProviderManager providerManager)
        {
            _downloadRepository = downloadRepository;
            _eventPubisher = eventPubisher;
            _mediaService = mediaService;

			var systemName = settingService.GetSettingByKey("Media.Storage.Provider", DatabaseMediaStorageProvider.SystemName);

			_storageProvider = providerManager.GetProvider<IMediaStorageProvider>(systemName);
		}

		private void UpdateDownloadCore(Download download, byte[] downloadBinary, bool updateDataStorage)
		{
			download.UpdatedOnUtc = DateTime.UtcNow;

			_downloadRepository.Update(download);

			if (updateDataStorage)
			{
				// save to storage
				_storageProvider.Value.Save(download.MediaFile, downloadBinary.ToStream());
			}
		}

		public virtual Download GetDownloadById(int downloadId)
        {
            if (downloadId == 0)
                return null;
            
            var download = _downloadRepository.Table.Expand(x => x.MediaFile).FirstOrDefault(x => x.Id == downloadId);
            return download;
        }

		public virtual IList<Download> GetDownloadsByIds(int[] downloadIds)
		{
			if (downloadIds == null || downloadIds.Length == 0)
				return new List<Download>();

			var query = from dl in _downloadRepository.Table.Expand(x => x.MediaFile)
                        where downloadIds.Contains(dl.Id)
						select dl;

			var downloads = query.ToList();

			// sort by passed identifier sequence
			return downloads.OrderBySequence(downloadIds).ToList();
		}
        
		public virtual IList<Download> GetDownloadsFor<TEntity>(TEntity entity) where TEntity : BaseEntity
		{
			Guard.NotNull(entity, nameof(entity));

			return GetDownloadsFor(entity.Id, entity.GetUnproxiedType().Name);
		}

		public virtual IList<Download> GetDownloadsFor(int entityId, string entityName)
		{
			if (entityId > 0)
			{
				var downloads = (from x in _downloadRepository.Table.Expand(x => x.MediaFile)
                                 where x.EntityId == entityId && x.EntityName == entityName
								 select x).ToList();

				if (downloads.Any())
				{
					var idsOrderedByVersion = downloads
						.Select(x => new { x.Id, Version = new SemanticVersion(x.FileVersion.HasValue() ? x.FileVersion : "1.0.0.0") })
						.OrderBy(x => x.Version)
						.Select(x => x.Id);

					downloads = new List<Download>(downloads.OrderBySequence(idsOrderedByVersion));

					return downloads;
				}
			}

			return new List<Download>();
		}

        public virtual Download GetDownloadByVersion(int entityId, string entityName, string fileVersion)
        {
            if (entityId > 0 && fileVersion.HasValue() && entityName.HasValue())
            {
                var download = (from x in _downloadRepository.Table.Expand(x => x.MediaFile)
                                where x.EntityId == entityId && x.EntityName.Equals(entityName) && x.FileVersion.Equals(fileVersion)
                                select x).FirstOrDefault();
                
                return download;
            }

            return null;
        }

        public virtual Multimap<int, Download> GetDownloadsByEntityIds(int[] entityIds, string entityName)
        {
            Guard.NotNull(entityIds, nameof(entityIds));
            Guard.NotEmpty(entityName, nameof(entityName));

            var query = _downloadRepository.TableUntracked.Expand(x => x.MediaFile)
                .Where(x => entityIds.Contains(x.EntityId) && x.EntityName == entityName)
				.OrderBy(x => x.FileVersion);

            var map = query
                .ToList()
                .ToMultimap(x => x.EntityId, x => x);

            return map;
        }

        public virtual Download GetDownloadByGuid(Guid downloadGuid)
        {
            if (downloadGuid == Guid.Empty)
                return null;

            var query = from o in _downloadRepository.Table.Expand(x => x.MediaFile)
                        where o.DownloadGuid == downloadGuid
                        select o;

            var order = query.FirstOrDefault();
            return order;
        }

        public virtual void DeleteDownload(Download download)
        {
			Guard.NotNull(download, nameof(download));

			// Delete entity
			_downloadRepository.Delete(download);
        }

        public virtual void InsertDownload(Download download)
        {
			Guard.NotNull(download, nameof(download));

            _downloadRepository.Insert(download);
        }

        public virtual void InsertDownload(Download download, byte[] downloadBinary, string fileName, string mimeType = null)
        {
            Guard.NotNull(download, nameof(download));

            var file = _mediaService.InsertPicture(downloadBinary, mimeType, fileName, true, false, SystemAlbumProvider.Downloads);
            file.Hidden = true;
            download.MediaFile = file;

            _downloadRepository.Insert(download);

            // Save to storage
            _storageProvider.Value.Save(download.MediaFile, downloadBinary.ToStream());
        }

        public virtual void UpdateDownload(Download download)
		{
			Guard.NotNull(download, nameof(download));

			// we use an overload because a byte array cannot be nullable
			UpdateDownloadCore(download, null, false);
		}

		public virtual void UpdateDownload(Download download, byte[] downloadBinary)
        {
			Guard.NotNull(download, nameof(download));

			UpdateDownloadCore(download, downloadBinary, true);
        }

		public virtual bool IsDownloadAllowed(OrderItem orderItem)
        {
            if (orderItem == null)
                return false;

            var order = orderItem.Order;
            if (order == null || order.Deleted)
                return false;

            //order status
            if (order.OrderStatus == OrderStatus.Cancelled)
                return false;

            var product = orderItem.Product;
            if (product == null || !product.IsDownload)
                return false;

            //payment status
            switch (product.DownloadActivationType)
            {
                case DownloadActivationType.WhenOrderIsPaid:
                    {
                        if (order.PaymentStatus == PaymentStatus.Paid && order.PaidDateUtc.HasValue)
                        {
                            //expiration date
                            if (product.DownloadExpirationDays.HasValue)
                            {
                                if (order.PaidDateUtc.Value.AddDays(product.DownloadExpirationDays.Value) > DateTime.UtcNow)
                                {
                                    return true;
                                }
                            }
                            else
                            {
                                return true;
                            }
                        }
                    }
                    break;
                case DownloadActivationType.Manually:
                    {
                        if (orderItem.IsDownloadActivated)
                        {
                            //expiration date
                            if (product.DownloadExpirationDays.HasValue)
                            {
                                if (order.CreatedOnUtc.AddDays(product.DownloadExpirationDays.Value) > DateTime.UtcNow)
                                {
                                    return true;
                                }
                            }
                            else
                            {
                                return true;
                            }
                        }
                    }
                    break;
                default:
                    break;
            }

            return false;
        }

        public virtual bool IsLicenseDownloadAllowed(OrderItem orderItem)
        {
            if (orderItem == null)
                return false;

            return IsDownloadAllowed(orderItem) &&
                orderItem.LicenseDownloadId.HasValue &&
                orderItem.LicenseDownloadId > 0;
        }

		public virtual byte[] LoadDownloadBinary(Download download)
		{
			Guard.NotNull(download, nameof(download));

			return _storageProvider.Value.Load(download.MediaFile);
		}
	}
}
