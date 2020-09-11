using System;
using SmartStore.Core.Data;
using SmartStore.Core.Data.Hooks;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Security;

namespace SmartStore.Core.Tests.Data.Hooks
{
    internal class Hook_Entity_Inserted_Deleted_Update : DbSaveHook<IDbContext, BaseEntity>
    {
        protected override void OnInserted(BaseEntity entity, IHookedEntity entry) { }
        protected override void OnDeleted(BaseEntity entity, IHookedEntity entry) { }
        protected override void OnUpdating(BaseEntity entity, IHookedEntity entry) { }
        protected override void OnUpdated(BaseEntity entity, IHookedEntity entry) { }
    }

    internal class Hook_Acl_Deleted : DbSaveHook<IDbContext, IAclSupported>
    {
        protected override void OnDeleted(IAclSupported entity, IHookedEntity entry) { }
    }

    [Important]
    internal class Hook_Auditable_Inserting_Updating_Important : DbSaveHook<IDbContext, IAuditable>
    {
        protected override void OnInserting(IAuditable entity, IHookedEntity entry) { }
        protected override void OnUpdating(IAuditable entity, IHookedEntity entry) { }
    }

    internal class Hook_SoftDeletable_Updating_ChangingState : DbSaveHook<IDbContext, ISoftDeletable>
    {
        protected override void OnUpdating(ISoftDeletable entity, IHookedEntity entry)
        {
            entry.State = EntityState.Unchanged;
        }
    }

    internal class Hook_LocalizedEntity_Deleted : DbSaveHook<IDbContext, ILocalizedEntity>
    {
        protected override void OnDeleted(ILocalizedEntity entity, IHookedEntity entry) { }
    }

    internal class Hook_Product_Post : IDbSaveHook
    {
        public void OnBeforeSave(IHookedEntity entry)
        {
            throw new NotImplementedException();
        }

        public void OnAfterSave(IHookedEntity entry)
        {
            if (entry.EntityType != typeof(Product))
                throw new NotSupportedException();
        }

        public void OnBeforeSaveCompleted() { }
        public void OnAfterSaveCompleted() { }
    }

    internal class Hook_Category_Pre : IDbSaveHook
    {
        public void OnBeforeSave(IHookedEntity entry)
        {
            if (entry.EntityType != typeof(Category))
                throw new NotSupportedException();
        }

        public void OnAfterSave(IHookedEntity entry)
        {
            throw new NotImplementedException();
        }

        public void OnBeforeSaveCompleted() { }
        public void OnAfterSaveCompleted() { }
    }
}
