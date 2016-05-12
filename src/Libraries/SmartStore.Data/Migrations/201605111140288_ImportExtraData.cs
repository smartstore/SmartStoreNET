namespace SmartStore.Data.Migrations
{
	using System.Data.Entity.Migrations;
	using Setup;

	public partial class ImportExtraData : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
	{
        public override void Up()
        {
            AddColumn("dbo.ImportProfile", "ExtraData", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ImportProfile", "ExtraData");
        }

		public bool RollbackOnFailure
		{
			get { return false; }
		}

		public void Seed(SmartObjectContext context)
		{
			context.MigrateLocaleResources(MigrateLocaleResources);

            context.MigrateSettings(x =>
            {
                x.Add("MediaSettings.VariantValueThumbPictureSize", "20");
                x.Delete("MediaSettings.AutoCompleteSearchThumbPictureSize");
            });
		}

		public void MigrateLocaleResources(LocaleResourcesBuilder builder)
		{
			builder.AddOrUpdate("Admin.DataExchange.Import.NumberOfPictures",
				"Number of pictures",
				"Anzahl der Bilder",
				"Specifies the number of images per object to be imported.",
				"Legt die Anzahl der zu importierenden Bilder pro Objekt fest.");
		}
	}
}
