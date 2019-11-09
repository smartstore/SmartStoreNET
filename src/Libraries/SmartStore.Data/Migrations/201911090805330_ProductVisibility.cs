namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    using System.Web.Hosting;
    using SmartStore.Core.Data;

    public partial class ProductVisibility : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Product", "Visibility", c => c.Int(nullable: false));
            CreateIndex("dbo.Product", "Visibility");
            CreateIndex("dbo.Product", "IsSystemProduct");

            if (HostingEnvironment.IsHosted && DataSettings.Current.IsSqlServer)
            {
                var hidden = (int)Core.Domain.Catalog.ProductVisibility.Hidden;
                Sql($"Update [dbo].[Product] Set [Visibility] = {hidden} Where [VisibleIndividually] = 0");
            }

            if (DataSettings.Current.IsSqlServer)
            {
                Sql("IF EXISTS (SELECT * FROM sys.indexes WHERE name='IX_SeekExport1' AND object_id = OBJECT_ID('[dbo].[Product]')) DROP INDEX [IX_SeekExport1] ON [dbo].[Product];");

				Sql(@"CREATE NONCLUSTERED INDEX [IX_SeekExport1] ON [dbo].[Product]
                (
	                [Published] ASC,
	                [Id] ASC,
	                [Visibility] ASC,
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
            DropIndex("dbo.Product", new[] { "IsSystemProduct" });
            DropIndex("dbo.Product", new[] { "Visibility" });
            DropColumn("dbo.Product", "Visibility");
        }
    }
}
