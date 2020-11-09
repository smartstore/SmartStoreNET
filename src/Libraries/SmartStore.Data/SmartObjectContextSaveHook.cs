using SmartStore.Data;

namespace SmartStore.Core.Data.Hooks
{
    public abstract class DbSaveHook<TEntity> : DbSaveHook<SmartObjectContext, TEntity>
        where TEntity : class
    {
    }
}