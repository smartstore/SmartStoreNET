using System.Data.Entity;
using SmartStore.Core.Infrastructure;

namespace SmartStore.Plugin.Feed.Froogle.Data
{
    public class EfStartUpTask : IStartupTask
    {
        public void Execute()
        {
            //It's required to set initializer to null (for SQL Server Compact).
            //otherwise, you'll get something like "The model backing the 'your context name' context has changed since the database was created. Consider using Code First Migrations to update the database"
            Database.SetInitializer<GoogleProductObjectContext>(null);
        }

        public int Order
        {
            get { return 0; }
        }
    }
}
