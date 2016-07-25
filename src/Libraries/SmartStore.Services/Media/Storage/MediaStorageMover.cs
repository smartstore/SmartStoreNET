using System;
using System.Linq;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Localization;
using SmartStore.Core.Logging;
using SmartStore.Core.Plugins;

namespace SmartStore.Services.Media.Storage
{
	public class MediaStorageMover : IMediaStorageMover
	{
		private const int PAGE_SIZE = 100;

		private readonly IRepository<Picture> _pictureRepository;
		private readonly ICommonServices _services;
		private readonly ILogger _logger;

		public MediaStorageMover(
			IRepository<Picture> pictureRepository,
			ICommonServices services,
			ILogger logger)
		{
			_pictureRepository = pictureRepository;
			_services = services;
			_logger = logger;

			T = NullLocalizer.Instance;
		}

		public Localizer T { get; set; }

		public virtual bool Move(Provider<IMediaStorageProvider> sourceProvider, Provider<IMediaStorageProvider> targetProvider)
		{
			Guard.ArgumentNotNull(() => sourceProvider);
			Guard.ArgumentNotNull(() => targetProvider);

			var success = false;
			var pageIndex = 0;
			var utcNow = DateTime.UtcNow;
			IPagedList<Picture> pictures = null;
			var context = new MediaStorageMoverContext(sourceProvider.Metadata.SystemName, targetProvider.Metadata.SystemName);

			var dbContext = _pictureRepository.Context;
			var source = sourceProvider.Value as IMovableMediaSupported;
			var target = targetProvider.Value as IMovableMediaSupported;

			// source and target must support media storage moving
			if (source == null)
			{
				throw new ArgumentException(T("Admin.Media.StorageMovingNotSupported", sourceProvider.Metadata.SystemName));
			}

			if (target == null)
			{
				throw new ArgumentException(T("Admin.Media.StorageMovingNotSupported", targetProvider.Metadata.SystemName));
			}

			// source and target provider must not be equal
			if (sourceProvider.Metadata.SystemName.IsCaseInsensitiveEqual(targetProvider.Metadata.SystemName))
			{
				throw new ArgumentException(T("Admin.Media.CannotMoveToSameProvider"));
			}

			// we are about to process data in chunks but want to commit ALL at once when ALL chunks have been processed successfully.
			// set autoDetectChanges to true for newly inserted binary data.
			using (var scope = new DbContextScope(ctx: dbContext, autoDetectChanges: true, proxyCreation: false, validateOnSave: false, autoCommit: false))
			using (var transaction = dbContext.BeginTransaction())
			{
				try
				{
					var query = _pictureRepository.Table
						.Expand(x => x.BinaryData)
						.OrderBy(x => x.Id);

					do
					{
						if (pictures != null)
						{
							// detach all entities from previous page to save memory
							dbContext.DetachEntities(pictures);
							pictures.Clear();
							pictures = null;
						}

						// load max 100 picture entities at once
						pictures = new PagedList<Picture>(query, pageIndex++, PAGE_SIZE);

						foreach (var picture in pictures)
						{
							// move item from source to target
							source.MoveTo(target, context, new MediaStorageItem
							{
								Entity = picture,
								MimeType = picture.MimeType
							});

							picture.UpdatedOnUtc = utcNow;

							++context.MovedItems;
						}

						// save the current batch to database
						dbContext.SaveChanges();
					}
					while (pictures.HasNextPage);

					transaction.Commit();
					success = true;
				}
				catch (Exception exception)
				{
					success = false;
					transaction.Rollback();

					_services.Notifier.Error(exception.Message);
					_logger.Error(exception);
				}
			}

			if (success)
			{
				_services.Settings.SetSetting("Media.Storage.Provider", targetProvider.Metadata.SystemName);
			}

			// inform both provider about ending
			source.OnMoved(context, success);
			target.OnMoved(context, success);

			if (success && context.ShrinkDatabase)
			{
				dbContext.ShrinkDatabase();
			}

			return success;
		}
	}
}
