namespace SmartStore.Data.Migrations
{
	using System.Data.Entity.Migrations;
	using System.Web.Hosting;
	using Core.Data;

	public partial class MoreIndexes : DbMigration
	{
		public override void Up()
		{
			if (HostingEnvironment.IsHosted && DataSettings.Current.IsSqlServer)
			{
				// Avoid "Column 'Name' in table 'dbo.ProductVariantAttributeValue' is of a type that is invalid for use as a key column in an index".
				Sql("If -1 = (SELECT CHARACTER_MAXIMUM_LENGTH FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ProductVariantAttributeValue' AND COLUMN_NAME = 'Name') ALTER TABLE [dbo].[ProductVariantAttributeValue] ALTER COLUMN [Name] nvarchar(4000) NULL;");
			}

			CreateIndex("dbo.Product_Category_Mapping", "IsFeaturedProduct");
			CreateIndex("dbo.Product_Manufacturer_Mapping", "IsFeaturedProduct");
			CreateIndex("dbo.SpecificationAttribute", "AllowFiltering");
			CreateIndex("dbo.Product_ProductAttribute_Mapping", "AttributeControlTypeId");
			CreateIndex("dbo.ProductAttribute", "AllowFiltering");
			CreateIndex("dbo.ProductVariantAttributeValue", "Name");
			CreateIndex("dbo.ProductVariantAttributeValue", "ValueTypeId");
		}

		public override void Down()
		{
			DropIndex("dbo.ProductVariantAttributeValue", new[] { "ValueTypeId" });
			DropIndex("dbo.ProductVariantAttributeValue", new[] { "Name" });
			DropIndex("dbo.ProductAttribute", new[] { "AllowFiltering" });
			DropIndex("dbo.Product_ProductAttribute_Mapping", new[] { "AttributeControlTypeId" });
			DropIndex("dbo.SpecificationAttribute", new[] { "AllowFiltering" });
			DropIndex("dbo.Product_Manufacturer_Mapping", new[] { "IsFeaturedProduct" });
			DropIndex("dbo.Product_Category_Mapping", new[] { "IsFeaturedProduct" });
		}
	}
}
