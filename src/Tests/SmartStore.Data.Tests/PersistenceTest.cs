using NUnit.Framework;
using SmartStore.Core;

namespace SmartStore.Data.Tests
{
    [TestFixture]
    public abstract class PersistenceTest
    {
        protected SmartObjectContext context;

        [SetUp]
        public virtual void SetUp()
        {
            context = new SmartObjectContext(GetTestDbName());
        }

        protected string GetTestDbName()
        {
            return GlobalSetup.GetTestDbName();
        }

        /// <summary>
        /// Persistance test helper
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <param name="entity">Entity</param>
        /// <param name="disposeContext">A value indicating whether to dispose context</param>
        protected T SaveAndLoadEntity<T>(T entity, bool disposeContext = true) where T : BaseEntity
        {

            context.Set<T>().Add(entity);
            context.SaveChanges();

            object id = entity.Id;

            if (disposeContext)
            {
                context.Dispose();
                context = new SmartObjectContext(GetTestDbName());
            }

            var fromDb = context.Set<T>().Find(id);
            return fromDb;
        }

        protected void ReloadContext()
        {
            context.Dispose();
            context = new SmartObjectContext(GetTestDbName());
        }
    }
}
