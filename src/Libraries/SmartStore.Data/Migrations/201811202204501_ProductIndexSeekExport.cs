namespace SmartStore.Data.Migrations
{
    using System.Data.Entity.Migrations;
    using SmartStore.Core.Data;

    public partial class ProductIndexSeekExport : DbMigration
    {
        public override void Up()
        {
            if (DataSettings.Current.IsSqlServer)
            {
                Sql(@"CREATE NONCLUSTERED INDEX [IX_SeekExport1] ON [dbo].[Product]
(
	[Published] ASC,
	[Id] ASC,
	[VisibleIndividually] ASC,
	[Deleted] ASC,
	[IsSystemProduct] ASC,
	[AvailableStartDateTimeUtc] ASC,
	[AvailableEndDateTimeUtc] ASC
)
INCLUDE ([UpdatedOnUtc]) WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]");
            }
        }

        public override void Down()
        {
            if (DataSettings.Current.IsSqlServer)
            {
                DropIndex("dbo.Product", "IX_SeekExport1");
            }
        }
    }
}
