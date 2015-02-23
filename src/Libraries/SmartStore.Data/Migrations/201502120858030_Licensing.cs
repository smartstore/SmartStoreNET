namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
	using SmartStore.Data.Setup;

	public partial class Licensing : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
            CreateTable(
                "dbo.License",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
						LicenseKey = c.String(nullable: false, maxLength: 400),
                        SystemName = c.String(nullable: false, maxLength: 400),
						MajorVersion = c.Int(nullable: false),
                        StoreId = c.Int(nullable: false),
						ActivatedOnUtc = c.DateTime()
                    })
                .PrimaryKey(t => t.Id);            
        }
        
        public override void Down()
        {
            DropTable("dbo.License");
        }

		public bool RollbackOnFailure
		{
			get { return false; }
		}

		public void Seed(SmartObjectContext context)
		{
			context.MigrateLocaleResources(MigrateLocaleResources);
		}

		public void MigrateLocaleResources(LocaleResourcesBuilder builder)
		{
			builder.AddOrUpdate("Admin.Common.License",
				"License",
				"Lizenzieren");

			builder.AddOrUpdate("Admin.Common.Licensed",
				"Licensed",
				"Lizenziert");

			builder.AddOrUpdate("Admin.Common.ResourceNotFound",
				"The resource was not found.",
				"Die gewünschte Ressource wurde nicht gefunden.");

			builder.AddOrUpdate("Admin.Configuration.Plugins.LicenseActivated",
				"The plugin has been successfully licensed.",
				"Das Plugin wurde erfolgreich lizenziert.");

			builder.AddOrUpdate("Admin.Configuration.Plugins.LicenseKey",
				"License key",
				"Lizenzschlüssel",
				"Please enter the license key for the plugin.",
				"Bitte den Lizenzschlüssel für das Plugin eingeben.");

			builder.AddOrUpdate("Admin.Plugins.AddOnLicensing",
				"Add-on licensing",
				"Add-on Lizenzierung");

			builder.AddOrUpdate("Admin.Plugins.NoLicenseFound",
				"No active license has been found for {0}. Please click 'License' in the plugin list and enter your license key.",
				"Es wurde keine aktive Lizenz für {0} gefunden. Bitte klicken Sie in der Plugin-Liste auf 'Lizenzieren' und geben Sie Ihren Lizenzschlüssel ein.");
		}
    }
}
