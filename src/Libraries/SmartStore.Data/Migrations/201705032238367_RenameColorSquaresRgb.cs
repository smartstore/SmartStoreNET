namespace SmartStore.Data.Migrations
{
	using System.Data.Entity.Migrations;
	using System.Web.Hosting;
	using Core.Data;
	using Setup;

	public partial class RenameColorSquaresRgb : DbMigration, IDataSeeder<SmartObjectContext>
	{
		private bool IsSqlServer
		{
			get
			{
				return HostingEnvironment.IsHosted && DataSettings.Current.IsSqlServer;
			}
		}

		public override void Up()
        {
			if (IsSqlServer)
			{
				RenameColumn("dbo.ProductAttributeOption", "ColorSquaresRgb", "Color");
				RenameColumn("dbo.ProductVariantAttributeValue", "ColorSquaresRgb", "Color");
			}
			else
			{
				AddColumn("dbo.ProductAttributeOption", "Color", c => c.String(maxLength: 100));
				AddColumn("dbo.ProductVariantAttributeValue", "Color", c => c.String(maxLength: 100));
			}
		}
        
        public override void Down()
        {
			if (IsSqlServer)
			{
				RenameColumn("dbo.ProductVariantAttributeValue", "Color", "ColorSquaresRgb");
				RenameColumn("dbo.ProductAttributeOption", "Color", "ColorSquaresRgb");
			}
			else
			{
				DropColumn("dbo.ProductVariantAttributeValue", "Color");
				DropColumn("dbo.ProductAttributeOption", "Color");
			}
		}

		public void Seed(SmartObjectContext context)
		{
			if (!IsSqlServer)
			{
				context.Execute("Update [dbo].[ProductAttributeOption] Set [Color] = [ColorSquaresRgb]");
				context.Execute("Update [dbo].[ProductVariantAttributeValue] Set [Color] = [ColorSquaresRgb]");

				context.Execute("Alter Table [dbo].[ProductAttributeOption] Drop Column [ColorSquaresRgb]");
				context.Execute("Alter Table [dbo].[ProductVariantAttributeValue] Drop Column [ColorSquaresRgb]");
			}
		}

		public bool RollbackOnFailure
		{
			get { return false; }
		}
	}
}
