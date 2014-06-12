namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
	using SmartStore.Data.Setup;

	public partial class ContentDeliveryNetwork : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
            AddColumn("dbo.Store", "ContentDeliveryNetwork", c => c.String(maxLength: 400));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Store", "ContentDeliveryNetwork");
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
			builder.AddOrUpdate("Admin.Configuration.Stores.Fields.ContentDeliveryNetwork",
				"Content Delivery Network URL",
				"Content Delivery Network URL");

			builder.AddOrUpdate("Admin.Configuration.Stores.Fields.ContentDeliveryNetwork.Hint",
				"The URL of your CDN, e.g. https://xxx.cloudfront.net or http://xxx.cloudflare.net. Setting this value will allow the site to serve static content like media through the CDN.",
				"Die URL eines CDN, z.B. https://xxx.cloudfront.net oder http://xxx.cloudflare.net. Diese Einstellung bewirkt, dass statische Mediendateien durch das CDN bereitgestellt werden.");
		}
    }
}
