using System;
using System.Linq;
using System.Threading.Tasks;
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

			var success = true;
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
			using (var scope = new DbContextScope(ctx: dbContext, autoDetectChanges: false, proxyCreation: false, validateOnSave: false, autoCommit: false))
			using (var transaction = dbContext.BeginTransaction())
			{
				try
				{
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
						pictures = new PagedList<Picture>(_pictureRepository.Table.OrderByDescending(x => x.Id), pageIndex++, PAGE_SIZE);

						foreach (var picture in pictures)
						{
							// move item from source to target
							source.MoveTo(target, context, picture);

							// explicitly attach modified entity to context because we disabled AutoCommit
							picture.UpdatedOnUtc = utcNow;
							_pictureRepository.Update(picture);

							++context.MovedItems;
						}

						// save the current batch to database
						dbContext.SaveChanges();
					}
					while (pictures.HasNextPage);

					transaction.Commit();
				}
				catch (Exception exception)
				{
					success = false;
					transaction.Rollback();

					// TODO: not here
					//_services.Settings.SetSetting("Media.Storage.Provider", sourceProvider.Metadata.SystemName);

					_services.Notifier.Error(exception.Message);
					_logger.Error(exception);
				}
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
