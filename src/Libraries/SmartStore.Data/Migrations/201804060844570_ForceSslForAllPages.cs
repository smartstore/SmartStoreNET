namespace SmartStore.Data.Migrations
{
	using SmartStore.Core.Domain.Configuration;
	using SmartStore.Core.Domain.Stores;
	using SmartStore.Data.Setup;
	using System;
    using System.Data.Entity.Migrations;
	using System.Linq;

	public partial class ForceSslForAllPages : DbMigration, IDataSeeder<SmartObjectContext>
	{
        public override void Up()
        {
            AddColumn("dbo.Store", "ForceSslForAllPages", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Store", "ForceSslForAllPages");
        }

		public bool RollbackOnFailure
		{
			get { return false; }
		}

		public void Seed(SmartObjectContext context)
		{
			try
			{
				var stores = context.Set<Store>();
				var settings = context.Set<Setting>();
				var defaultSetting = settings.FirstOrDefault(x => x.Name == "SecuritySettings.ForceSslForAllPages" && x.StoreId == 0);

				foreach (var store in stores)
				{
					var setting = settings.FirstOrDefault(x => x.Name == "SecuritySettings.ForceSslForAllPages" && x.StoreId == store.Id);

					if (setting != null)
					{
						store.ForceSslForAllPages = setting.Value.ToBool(true);

						// remove multistore setting because it's not used anymore
						settings.Remove(setting);
					}
					else if (defaultSetting != null)
					{
						store.ForceSslForAllPages = defaultSetting.Value.ToBool(true);
					}
					else
					{
						store.ForceSslForAllPages = false;
					}
				}

				// remove setting because it's not used anymore
				if (defaultSetting != null)
				{
					settings.Remove(defaultSetting);
				}

				context.MigrateLocaleResources(MigrateLocaleResources);
				
				context.SaveChanges();
			}
			catch { }
		}

		public void MigrateLocaleResources(LocaleResourcesBuilder builder)
		{
			builder.Delete("Admin.Configuration.Settings.GeneralCommon.ForceSslForAllPages");

			builder.AddOrUpdate("Admin.Configuration.Stores.Fields.ForceSslForAllPages",
				"Always use SSL",
				"Immer SSL verwenden",
				"Specifies whether to SSL secure all request.",
				"Legt fest, dass alle Anfragen SSL gesichert werden sollen.");
		}
	}
}
