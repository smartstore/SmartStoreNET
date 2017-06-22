namespace SmartStore.Data.Migrations
{
	using System;
	using System.Linq;
	using System.Data.Entity.Migrations;
	using Core.Domain.Catalog;
	using Setup;
	using Core.Domain.Configuration;

	public partial class ListPaging : DbMigration, IDataSeeder<SmartObjectContext>
	{
		public override void Up()
        {
            AlterColumn("dbo.Category", "PageSize", c => c.Int());
            AlterColumn("dbo.Category", "AllowCustomersToSelectPageSize", c => c.Boolean());
            AlterColumn("dbo.Manufacturer", "PageSize", c => c.Int());
            AlterColumn("dbo.Manufacturer", "AllowCustomersToSelectPageSize", c => c.Boolean());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Manufacturer", "AllowCustomersToSelectPageSize", c => c.Boolean(nullable: false));
            AlterColumn("dbo.Manufacturer", "PageSize", c => c.Int(nullable: false));
            AlterColumn("dbo.Category", "AllowCustomersToSelectPageSize", c => c.Boolean(nullable: false));
            AlterColumn("dbo.Category", "PageSize", c => c.Int(nullable: false));
        }

		public bool RollbackOnFailure
		{
			get { return false; }
		}


		public void Seed(SmartObjectContext context)
		{
			// Set Category paging stuff to null
			try
			{
				var categories = context.Set<Category>().ToList();
				foreach (var c in categories)
				{
					c.PageSize = null;
					c.AllowCustomersToSelectPageSize = null;
					c.PageSizeOptions = null;
				}

				context.SaveChanges();
				context.DetachEntities<Category>();
			}
			catch { }


			// Set Manufacturer paging stuff to null
			try
			{
				var manufacturers = context.Set<Manufacturer>().ToList();
				foreach (var m in manufacturers)
				{
					m.PageSize = null;
					m.AllowCustomersToSelectPageSize = null;
					m.PageSizeOptions = null;
				}

				context.SaveChanges();
				context.DetachEntities<Manufacturer>();
			}
			catch { }


			// Set new default paging global settings
			try
			{
				var settings = context.Set<Setting>().Where(x => x.Name.StartsWith("CatalogSettings.")).ToList();
				foreach (var s in settings)
				{
					switch (s.Name.ToLowerInvariant())
					{
						case "catalogsettings.recentlyaddedproductsnumber":
							s.Value = "100";
							break;
						case "catalogsettings.recentlyviewedproductsnumber":
							s.Value = "8";
							break;
						case "catalogsettings.numberofbestsellersonhomepage":
							s.Value = "12";
							break;
						case "catalogsettings.productsalsopurchasednumber":
							s.Value = "12";
							break;
						case "catalogsettings.defaultproductlistpagesize":
							s.Value = "24";
							break;
						case "catalogsettings.defaultpagesizeoptions":
						case "catalogsettings.productsbytagpagesizeoptions":
							s.Value = "12,24,36,48,72,120";
							break;
						case "catalogsettings.allowcustomerstoselectpagesize":
							s.Value = "true";
							break;
						case "catalogsettings.manufacturersblockitemstodisplay":
							s.Value = "8";
							break;
						case "catalogsettings.usesmallproductboxonhomepage":
							s.Value = "false";
							break;
					}
				}

				context.SaveChanges();
			}
			catch { }
		}
	}
}
