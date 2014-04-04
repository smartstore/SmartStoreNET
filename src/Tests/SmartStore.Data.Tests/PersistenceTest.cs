using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using SmartStore.Core;
using NUnit.Framework;
using SmartStore.Data.Migrations;
using SmartStore.Data.Setup;

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
			context.Database.Delete();
			Database.SetInitializer(new TestDatabaseInitializer<SmartObjectContext, MigrationsConfiguration>(GetTestDbName()));
			context.Database.Initialize(true);
        }

        protected string GetTestDbName()
        {
            string testDbName = "Data Source=" + (System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)) + @"\\SmartStore.Data.Tests.Db.sdf;Persist Security Info=False";
            return testDbName;
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
