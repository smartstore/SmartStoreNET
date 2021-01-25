using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NuGet;
using SmartStore.Collections;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Payments;

namespace SmartStore.Services.Media
{
    public partial class DownloadService : IDownloadService
    {
        private readonly IRepository<Download> _downloadRepository;
        private readonly IMediaService _mediaService;

        public DownloadService(IRepository<Download> downloadRepository, IMediaService mediaService)
        {
            _downloadRepository = downloadRepository;
            _mediaService = mediaService;
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

        public virtual IList<Download> GetDownloadsFor<TEntity>(TEntity entity, bool versionedFilesOnly = false) where TEntity : BaseEntity
        {
            Guard.NotNull(entity, nameof(entity));

            return GetDownloadsFor(entity.Id, entity.GetUnproxiedType().Name, versionedFilesOnly);
        }

        public virtual IList<Download> GetDownloadsFor(int entityId, string entityName, bool versionedFilesOnly = false)
        {
            if (entityId > 0)
            {
                var downloads = (from x in _downloadRepository.Table.Expand(x => x.MediaFile)
                                 where x.EntityId == entityId && x.EntityName == entityName
                                 select x).ToList();

                if (versionedFilesOnly)
                    downloads = downloads.Where(x => !string.IsNullOrEmpty(x.FileVersion)).ToList();

                if (downloads.Any())
                {
                    var idsOrderedByVersion = downloads
                        .Select(x => new { x.Id, Version = new SemanticVersion(x.FileVersion.HasValue() ? x.FileVersion : "1.0.0.0") })
                        .OrderByDescending(x => x.Version)
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
            {
                return null;
            }

            var query = from x in _downloadRepository.Table.Expand(x => x.MediaFile)
                        where x.DownloadGuid == downloadGuid
                        select x;

            var download = query.FirstOrDefault();
            return download;
        }

        public virtual void DeleteDownload(Download download)
        {
            Guard.NotNull(download, nameof(download));

            _downloadRepository.Delete(download);
        }

        public virtual void InsertDownload(Download download)
        {
            Guard.NotNull(download, nameof(download));

            _downloadRepository.Insert(download);
        }

        public virtual MediaFileInfo InsertDownload(Download download, Stream stream, string fileName)
        {
            Guard.NotNull(download, nameof(download));

            var path = _mediaService.CombinePaths(SystemAlbumProvider.Downloads, fileName);
            var file = _mediaService.SaveFile(path, stream, dupeFileHandling: DuplicateFileHandling.Rename);
            file.File.Hidden = true;
            download.MediaFile = file.File;

            _downloadRepository.Insert(download);

            return file;
        }

        public virtual void UpdateDownload(Download download)
        {
            Guard.NotNull(download, nameof(download));

            download.UpdatedOnUtc = DateTime.UtcNow;

            _downloadRepository.Update(download);
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
            return _mediaService.StorageProvider.Load(download.MediaFile);
        }
    }
}
