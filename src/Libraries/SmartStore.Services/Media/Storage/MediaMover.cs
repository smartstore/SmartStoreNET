using System;
using System.Linq;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Domain.Messages;
using SmartStore.Core.Localization;
using SmartStore.Core.Logging;
using SmartStore.Core.Plugins;

namespace SmartStore.Services.Media.Storage
{
	public class MediaMover : IMediaMover
	{
		private const int PAGE_SIZE = 100;

		private readonly IRepository<MediaFile> _pictureRepository;
		private readonly ICommonServices _services;
		private readonly ILogger _logger;

		public MediaMover(
			IRepository<MediaFile> pictureRepository,
			ICommonServices services,
			ILogger logger)
		{
			_pictureRepository = pictureRepository;
			_services = services;
			_logger = logger;
		}

		public Localizer T { get; set; } = NullLocalizer.Instance;

		protected virtual void PageEntities<TEntity>(IOrderedQueryable<TEntity> query, Action<TEntity> moveEntity) where TEntity : BaseEntity, IHasMedia
		{
			var pageIndex = 0;
			IPagedList<TEntity> entities = null;

			do
			{
				if (entities != null)
				{
					// detach all entities from previous page to save memory
					_services.DbContext.DetachEntities(entities);
					entities.Clear();
					entities = null;
				}

				// load max 100 entities at once
				entities = new PagedList<TEntity>(query, pageIndex++, PAGE_SIZE);

				entities.Each(x => moveEntity(x));

				// save the current batch to database
				_services.DbContext.SaveChanges();
			}
			while (entities.HasNextPage);
		}

		public virtual bool Move(Provider<IMediaStorageProvider> sourceProvider, Provider<IMediaStorageProvider> targetProvider)
		{
			Guard.NotNull(sourceProvider, nameof(sourceProvider));
			Guard.NotNull(targetProvider, nameof(targetProvider));

			var success = false;
			var utcNow = DateTime.UtcNow;
			var context = new MediaMoverContext(sourceProvider.Metadata.SystemName, targetProvider.Metadata.SystemName);

			var source = sourceProvider.Value as ISupportsMediaMoving;
			var target = targetProvider.Value as ISupportsMediaMoving;

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
			// autoDetectChanges true required for newly inserted binary data.
			using (var scope = new DbContextScope(ctx: _services.DbContext, 
				autoDetectChanges: true, 
				proxyCreation: false, 
				validateOnSave: false, 
				autoCommit: false))
			{
				using (var transaction = _services.DbContext.BeginTransaction())
				{
					try
					{
						// Files
						var queryFiles = _pictureRepository.Table
							.Expand(x => x.MediaStorage)
							.OrderBy(x => x.Id);

						PageEntities(queryFiles, picture =>
						{
							// move item from source to target
							source.MoveTo(target, context, picture);

							picture.UpdatedOnUtc = utcNow;
							++context.MovedItems;
						});

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
			}


			if (success)
			{
				_services.Settings.SetSetting("Media.Storage.Provider", targetProvider.Metadata.SystemName);
			}

			// inform both provider about ending
			source.OnCompleted(context, success);
			target.OnCompleted(context, success);

			if (success && context.ShrinkDatabase)
			{
				_services.DbContext.ShrinkDatabase();
			}

			return success;
		}
	}
}
