namespace SmartStore.Data.Migrations
{
    using System.Data.Entity.Migrations;
    using System.Web.Hosting;
    using SmartStore.Core.Data;
    using SmartStore.Data.Setup;

    public partial class ProductTagPublished : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
            AddColumn("dbo.ProductTag", "Published", c => c.Boolean(nullable: false, defaultValue: true));
            CreateIndex("dbo.ProductTag", "Published", name: "IX_ProductTag_Published");

            if (HostingEnvironment.IsHosted && DataSettings.Current.IsSqlServer)
            {
                Sql(GetAlterTagCountProcedureSql(true));
            }
        }

        public override void Down()
        {
            DropIndex("dbo.ProductTag", "IX_ProductTag_Published");
            DropColumn("dbo.ProductTag", "Published");

            if (HostingEnvironment.IsHosted && DataSettings.Current.IsSqlServer)
            {
                Sql(GetAlterTagCountProcedureSql(false));
            }
        }

        public bool RollbackOnFailure => true;

        public void Seed(SmartObjectContext context)
        {
            context.MigrateLocaleResources(MigrateLocaleResources);
            context.SaveChanges();
        }

        public void MigrateLocaleResources(LocaleResourcesBuilder builder)
        {
            builder.AddOrUpdate(
                "Admin.Catalog.ProductTags.Published",
                "Published",
                "Veröffentlicht",
                "Tags that have not been published are not visible in the shop, but are taken into account in the product search.",
                "Nicht veröffentlichte Tags sind im Shop nicht sichtbar, werden aber bei der Produktsuche berücksichtigt.");
        }

        private string GetAlterTagCountProcedureSql(bool newVersion)
        {
            string result = null;

            if (newVersion)
            {
                result =
                    "ALTER PROCEDURE [dbo].[ProductTagCountLoadAll]\r\n" +
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
                    "		AND p.VisibleIndividually = 1\r\n" +
                    "		AND (@IncludeHidden = 1 Or pt.Published = 1)\r\n" +
                    "		AND (@StoreId = 0 or (p.LimitedToStores = 0 OR EXISTS (\r\n" +
                    "			SELECT 1 FROM [StoreMapping] sm\r\n" +
                    "			WHERE [sm].EntityId = p.Id AND [sm].EntityName = 'Product' and [sm].StoreId=@StoreId\r\n" +
                    "			)))\r\n" +
                    "	GROUP BY pt.Id\r\n" +
                    "	ORDER BY pt.Id\r\n" +
                    "END\r\n";
            }
            else
            {
                result =
                    "ALTER PROCEDURE [dbo].[ProductTagCountLoadAll]\r\n" +
                    "(\r\n" +
                    "	@StoreId int\r\n" +
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
                    "		AND (@StoreId = 0 or (p.LimitedToStores = 0 OR EXISTS (\r\n" +
                    "			SELECT 1 FROM [StoreMapping] sm\r\n" +
                    "			WHERE [sm].EntityId = p.Id AND [sm].EntityName = 'Product' and [sm].StoreId=@StoreId\r\n" +
                    "			)))\r\n" +
                    "	GROUP BY pt.Id\r\n" +
                    "	ORDER BY pt.Id\r\n" +
                    "END\r\n";
            }

            return result;
        }
    }
}
