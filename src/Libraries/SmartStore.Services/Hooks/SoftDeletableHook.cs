using System;
using System.Linq;
using Autofac;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Data.Hooks;
using SmartStore.Core.Domain.Security;
using SmartStore.Core.Domain.Seo;
using SmartStore.Services.Security;
using SmartStore.Services.Seo;
using SmartStore.Data;
using SmartStore.Core.Domain.Catalog;
using System.Collections.Generic;
using SmartStore.Services.Catalog;
using SmartStore.Services.Media;
using SmartStore.Core.Domain.Media;

namespace SmartStore.Services.Hooks
{
    public class SoftDeletablePreUpdateHook : DbSaveHook<ObjectContextBase, ISoftDeletable>
    {
        private readonly IComponentContext _ctx;

        public SoftDeletablePreUpdateHook(IComponentContext ctx)
        {
            _ctx = ctx;
        }

        protected override void OnUpdating(ISoftDeletable entity, IHookedEntity entry)
        {
            var baseEntity = entry.Entity;

            var prop = entry.Entry.Property(nameof(ISoftDeletable.Deleted));
            var deletedModified = !prop.CurrentValue.Equals(prop.OriginalValue);
            if (!deletedModified)
                return;

            var entityType = entry.EntityType;

            // Mark orphaned ACL records as idle
            if (baseEntity is IAclSupported aclSupported && aclSupported.SubjectToAcl)
            {
                var shouldSetIdle = entity.Deleted;
                var aclService = _ctx.Resolve<IAclService>();
                var records = aclService.GetAclRecordsFor(entityType.Name, baseEntity.Id);
                foreach (var record in records)
                {
                    record.IsIdle = shouldSetIdle;
                    aclService.UpdateAclRecord(record);
                }
            }

            if (!entity.Deleted)
                return; // Nothing to process from here on.

            // Delete orphaned inactive UrlRecords.
            // We keep the active ones on purpose in order to be able to fully restore a soft deletable entity once we implemented the "recycle bin" feature
            if (baseEntity is ISlugSupported)
            {
                var urlRecordService = _ctx.Resolve<IUrlRecordService>();
                var activeRecords = urlRecordService.GetUrlRecordsFor(entityType.Name, baseEntity.Id);
                using (urlRecordService.BeginScope())
                {
                    foreach (var record in activeRecords)
                    {
                        if (!record.IsActive)
                        {
                            urlRecordService.DeleteUrlRecord(record);
                        }
                    }
                }
            }

            // Unassign any media file and let the media tracker reliably clean up orphaned files later.
            // TODO: Once we implement "recycle bin" feature for catalog entities, don't unassign anymore.
            if (baseEntity is Product product)
            {
                product.MainPictureId = null;

                var productService = _ctx.Resolve<IProductService>();
                var mediaTracker = _ctx.Resolve<IMediaTracker>();
                var tracks = new List<MediaTrack>();

                foreach (var pm in product.ProductPictures.ToList())
                {
                    productService.DeleteProductPicture(pm);

                    tracks.Add(new MediaTrack
                    {
                        EntityId = pm.Id,
                        EntityName = nameof(ProductMediaFile),
                        MediaFileId = pm.MediaFileId,
                        Property = nameof(pm.MediaFileId),
                        Operation = MediaTrackOperation.Untrack
                    });
                }

                // We have to untrack manually, because the tracker hook will not run in the scope of another hook.
                mediaTracker.TrackMany(SystemAlbumProvider.Catalog, tracks);
            }

            // Although nested hooks do not run, the following stuff will still trigger the tracker hook,
            // because this hook runs before the tracker hook.

            if (baseEntity is Category category)
            {
                category.MediaFileId = null;
            }

            if (baseEntity is Manufacturer manufacturer)
            {
                manufacturer.MediaFileId = null;
            }
        }
    }
}
