namespace SmartStore.Data.Migrations
{
	using System.Data.Entity.Migrations;
	using System.Web.Hosting;
	using Core.Data;
	using Setup;

	public partial class UpdateMediaPath : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
	{
        public override void Up()
        {
			if (HostingEnvironment.IsHosted && DataSettings.Current.IsSqlServer)
			{
				var tenantName = DataSettings.Current.TenantName.NullEmpty() ?? "Default";
				var uploadedPath = $"/Media/{tenantName}/Uploaded/";
				var thumbsPath = $"/Media/{tenantName}/Thumbs/";

				Sql($"UPDATE [dbo].[Category] SET [Description] = REPLACE([Description],'/Media/Uploaded/','{uploadedPath}')");
				Sql($"UPDATE [dbo].[Category] SET [BottomDescription] = REPLACE([BottomDescription],'/Media/Uploaded/','{uploadedPath}')");
				Sql($"UPDATE [dbo].[Manufacturer] SET [Description] = REPLACE([Description],'/Media/Uploaded/','{uploadedPath}')");
				Sql($"UPDATE [dbo].[ShippingMethod] SET [Description] = REPLACE([Description],'/Media/Uploaded/','{uploadedPath}')");
				Sql($"UPDATE [dbo].[PaymentMethod] SET [FullDescription] = REPLACE([FullDescription],'/Media/Uploaded/','{uploadedPath}')");
				Sql($"UPDATE [dbo].[BlogPost] SET [Body] = REPLACE([Body],'/Media/Uploaded/','{uploadedPath}')");
				Sql($"UPDATE [dbo].[News] SET [Full] = REPLACE([Full],'/Media/Uploaded/','{uploadedPath}')");
				Sql($"UPDATE [dbo].[Campaign] SET [Body] = REPLACE([Body],'/Media/Uploaded/','{uploadedPath}')");
				Sql($"UPDATE [dbo].[QueuedEmail] SET [Body] = REPLACE([Body],'/Media/Uploaded/','{uploadedPath}')");
				Sql($"UPDATE [dbo].[QueuedEmail] SET [Body] = REPLACE([Body],'/Media/Thumbs/','{thumbsPath}')");
				Sql($"UPDATE [dbo].[Topic] SET [Body] = REPLACE([Body],'/Media/Uploaded/','{uploadedPath}')");
				Sql($"UPDATE [dbo].[MessageTemplate] SET [Body] = REPLACE([Body],'/Media/Uploaded/','{uploadedPath}')");
				Sql($"UPDATE [dbo].[Product] SET [FullDescription] = REPLACE([FullDescription],'/Media/Uploaded/','{uploadedPath}')");

				// LocalizedProperty
				Sql($"UPDATE [dbo].[LocalizedProperty] SET [LocaleValue] = REPLACE([LocaleValue],'/Media/Thumbs/','{thumbsPath}') WHERE [LocaleKey] = 'Body' AND [LocaleKeyGroup] = 'QueuedEmail'");
				Sql($@"
UPDATE [dbo].[LocalizedProperty] SET [LocaleValue] = REPLACE([LocaleValue],'/Media/Uploaded/','{uploadedPath}') 
WHERE 
	([LocaleKey] = 'BottomDescription' AND [LocaleKeyGroup] = 'Category') OR
	([LocaleKey] = 'Full' AND [LocaleKeyGroup] = 'News') OR
	([LocaleKey] = 'Description' AND [LocaleKeyGroup] IN ('Category', 'Manufacturer', 'ShippingMethod')) OR
	([LocaleKey] = 'FullDescription' AND [LocaleKeyGroup] IN ('PaymentMethod', 'Product')) OR
	([LocaleKey] = 'Body' AND [LocaleKeyGroup] IN ('BlogPost', 'Campaign', 'QueuedEmail', 'MessageTemplate', 'Topic'))");
			}
		}
        
        public override void Down()
        {
        }

		public bool RollbackOnFailure
		{
			get { return false; }
		}

		public void Seed(SmartObjectContext context)
		{
			context.MigrateLocaleResources(MigrateLocaleResources);

			context.SaveChanges();
		}

		public void MigrateLocaleResources(LocaleResourcesBuilder builder)
		{
			builder.AddOrUpdate("Common.For", "For: {0}", "Für: {0}");
			builder.AddOrUpdate("Products.Sorting.Featured", "Featured", "Empfehlung");
		}
	}
}
