namespace SmartStore.Data.Migrations
{
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
                var fullStr = ((int)Core.Domain.Catalog.ProductVisibility.Full).ToString();

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

                Sql("ALTER PROCEDURE [dbo].[ProductTagCountLoadAll]\r\n" +
                    "(\r\n" +
                    "	@StoreId int,\r\n" +
                    "   @IncludeHidden bit = 0\r\n" +
                    ")\r\n" +
                    "AS\r\n" +
                    "BEGIN\r\n" +
                    "    SET NOCOUNT ON\r\n" +
                    "    SELECT pt.Id as [ProductTagId], COUNT(p.Id) as [ProductCount]\r\n" +
                    "    FROM ProductTag pt with (NOLOCK)\r\n" +
                    "	LEFT JOIN Product_ProductTag_Mapping pptm with (NOLOCK) ON pt.[Id] = pptm.[ProductTag_Id]\r\n" +
                    "	LEFT JOIN Product p with (NOLOCK) ON pptm.[Product_Id] = p.[Id]\r\n" +
                    "	WHERE\r\n" +
                    "		p.[Deleted] = 0\r\n" +
                    "		AND p.Published = 1\r\n" +
                    "		AND p.Visibility = " + fullStr + "\r\n" +
                    "		AND (@IncludeHidden = 1 Or pt.Published = 1)\r\n" +
                    "		AND (@StoreId = 0 or (p.LimitedToStores = 0 OR EXISTS (\r\n" +
                    "			SELECT 1 FROM [StoreMapping] sm\r\n" +
                    "			WHERE [sm].EntityId = p.Id AND [sm].EntityName = 'Product' and [sm].StoreId=@StoreId\r\n" +
                    "			)))\r\n" +
                    "	GROUP BY pt.Id\r\n" +
                    "	ORDER BY pt.Id\r\n" +
                    "END\r\n");
            }
        }

        public override void Down()
        {
            if (DataSettings.Current.IsSqlServer)
            {
                Sql("IF EXISTS (SELECT * FROM sys.indexes WHERE name='IX_SeekExport1' AND object_id = OBJECT_ID('[dbo].[Product]')) DROP INDEX [IX_SeekExport1] ON [dbo].[Product];");

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

            DropIndex("dbo.Product", new[] { "IsSystemProduct" });
            DropIndex("dbo.Product", new[] { "Visibility" });
            DropColumn("dbo.Product", "Visibility");
        }
    }
}
