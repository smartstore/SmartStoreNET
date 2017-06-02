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
				var tenantName = DataSettings.Current.TenantName;
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

				Sql($"UPDATE [dbo].[LocalizedProperty] SET [LocaleValue] = REPLACE([LocaleValue],'/Media/Uploaded/','{uploadedPath}') WHERE [LocaleKeyGroup] = 'Category' Or [LocaleKeyGroup] = 'Manufacturer' Or [LocaleKeyGroup] = 'ShippingMethod' Or [LocaleKeyGroup] = 'PaymentMethod' Or [LocaleKeyGroup] = 'BlogPost' Or [LocaleKeyGroup] = 'News' Or [LocaleKeyGroup] = 'Campaign' Or [LocaleKeyGroup] = 'QueuedEmail' Or [LocaleKeyGroup] = 'Topic' Or [LocaleKeyGroup] = 'Product'");

				Sql($"UPDATE [dbo].[Product] SET [FullDescription] = REPLACE([FullDescription],'/Media/Uploaded/','{uploadedPath}')");
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
