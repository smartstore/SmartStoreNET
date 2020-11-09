using System.Data.Entity;

namespace SmartStore.Data.Setup
{
    public sealed class SeedingDbMigrationEvent
    {
        public string MigrationId { get; internal set; }
        public string MigrationName { get; internal set; }
        public DbContext DbContext { get; internal set; }
    }

    public sealed class SeededDbMigrationEvent
    {
        public string MigrationId { get; internal set; }
        public string MigrationName { get; internal set; }
        public DbContext DbContext { get; internal set; }
    }
}
