using System;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Domain.Messages;
using SmartStore.Core.Localization;
using SmartStore.Core.Logging;
using SmartStore.Core.Plugins;
using SmartStore.Data.Utilities;

namespace SmartStore.Services.Media.Storage
{
    public class MediaMover : IMediaMover
    {
        private const int PAGE_SIZE = 50;

        private readonly IRepository<MediaFile> _mediaFileRepo;
        private readonly ICommonServices _services;
        private readonly ILogger _logger;

        public MediaMover(IRepository<MediaFile> mediaFileRepo, ICommonServices services, ILogger logger)
        {
            _mediaFileRepo = mediaFileRepo;
            _services = services;
            _logger = logger;
        }

        public Localizer T { get; set; } = NullLocalizer.Instance;

        public virtual bool Move(Provider<IMediaStorageProvider> sourceProvider, Provider<IMediaStorageProvider> targetProvider)
        {
            Guard.NotNull(sourceProvider, nameof(sourceProvider));
            Guard.NotNull(targetProvider, nameof(targetProvider));

            var success = false;
            var utcNow = DateTime.UtcNow;
            var context = new MediaMoverContext(sourceProvider.Metadata.SystemName, targetProvider.Metadata.SystemName);

            var source = sourceProvider.Value as ISupportsMediaMoving;
            var target = targetProvider.Value as ISupportsMediaMoving;

            // Source and target must support media storage moving
            if (source == null)
            {
                throw new ArgumentException(T("Admin.Media.StorageMovingNotSupported", sourceProvider.Metadata.SystemName));
            }

            if (target == null)
            {
                throw new ArgumentException(T("Admin.Media.StorageMovingNotSupported", targetProvider.Metadata.SystemName));
            }

            // Source and target provider must not be equal
            if (sourceProvider.Metadata.SystemName.IsCaseInsensitiveEqual(targetProvider.Metadata.SystemName))
            {
                throw new ArgumentException(T("Admin.Media.CannotMoveToSameProvider"));
            }

            // We are about to process data in chunks but want to commit ALL at once after ALL chunks have been processed successfully.
            // AutoDetectChanges true required for newly inserted binary data.
            using (var scope = new DbContextScope(ctx: _services.DbContext,
                autoDetectChanges: true,
                proxyCreation: false,
                validateOnSave: false,
                autoCommit: false))
            {
                using (var transaction = scope.DbContext.BeginTransaction())
                {
                    try
                    {
                        var pager = new FastPager<MediaFile>(_mediaFileRepo.Table, PAGE_SIZE);
                        while (pager.ReadNextPage(out var files))
                        {
                            foreach (var file in files)
                            {
                                // Move item from source to target
                                source.MoveTo(target, context, file);

                                file.UpdatedOnUtc = utcNow;
                                ++context.MovedItems;
                            }

                            scope.DbContext.SaveChanges();

                            // Detach all entities from previous page to save memory
                            scope.DbContext.DetachEntities(files, deep: true);
                        }

                        transaction.Commit();
                        success = true;
                    }
                    catch (Exception exception)
                    {
                        success = false;
                        transaction.Rollback();

                        _services.Notifier.Error(exception);
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
