namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
	using SmartStore.Data.Setup;
	using SmartStore.Data.Utilities;

	public partial class AddressFormat : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
	{
        public override void Up()
        {
            AddColumn("dbo.Country", "AddressFormat", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Country", "AddressFormat");
        }

		public void Seed(SmartObjectContext context)
		{
			context.MigrateLocaleResources(MigrateLocaleResources);
			DataMigrator.ImportAddressFormats(context);

			context.SaveChanges();
		}

		public void MigrateLocaleResources(LocaleResourcesBuilder builder)
		{
			builder.AddOrUpdate("Admin.Configuration.Countries.Fields.AddressFormat",
				"Address format",
				"Adressenformat",
				"The address format according to the countries mailing address format rules.",
				"Das Addressenformat gemäß der Landes-Richtlinien.");
		}

		public bool RollbackOnFailure => false;
	}
}
