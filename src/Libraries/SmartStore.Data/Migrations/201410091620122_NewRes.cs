namespace SmartStore.Data.Migrations
{
	using System;
	using System.Linq;
	using System.Data.Entity.Migrations;
	using SmartStore.Core.Domain.Tasks;
	using SmartStore.Data.Setup;

	public partial class NewRes : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
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

			// update scheduled task types
			var table = context.Set<ScheduleTask>();

			ScheduleTask task;

			task = table.FirstOrDefault(x => x.Name.StartsWith("PromotionFeed.Froogle"));
			if (task != null)
			{
				task.Name = "SmartStore.GoogleMerchantCenter feed file generation";
				task.Type = "SmartStore.GoogleMerchantCenter.StaticFileGenerationTask, SmartStore.GoogleMerchantCenter";
			}

			task = table.FirstOrDefault(x => x.Name.StartsWith("PromotionFeed.Billiger"));
			if (task != null)
			{
				task.Name = "SmartStore.Billiger feed file generation";
				task.Type = "SmartStore.Billiger.StaticFileGenerationTask, SmartStore.Billiger";
			}

			task = table.FirstOrDefault(x => x.Name.StartsWith("PromotionFeed.Guenstiger"));
			if (task != null)
			{
				task.Name = "SmartStore.Guenstiger feed file generation";
				task.Type = "SmartStore.Guenstiger.StaticFileGenerationTask, SmartStore.Guenstiger";
			}

			task = table.FirstOrDefault(x => x.Name.StartsWith("PromotionFeed.ElmarShopinfo"));
			if (task != null)
			{
				task.Name = "SmartStore.ElmarShopinfo feed file generation";
				task.Type = "SmartStore.ElmarShopinfo.StaticFileGenerationTask, SmartStore.ElmarShopinfo";
			}

			task = table.FirstOrDefault(x => x.Name.StartsWith("PromotionFeed.Shopwahl"));
			if (task != null)
			{
				task.Name = "SmartStore.Shopwahl feed file generation";
				task.Type = "SmartStore.Shopwahl.StaticFileGenerationTask, SmartStore.Shopwahl";
			}

			task = table.FirstOrDefault(x => x.Name.StartsWith("MailChimp"));
			if (task != null)
			{
				task.Type = "SmartStore.MailChimp.MailChimpSynchronizationTask, SmartStore.MailChimp";
			}

			context.SaveChanges();
		}

		public void MigrateLocaleResources(LocaleResourcesBuilder builder)
		{
			builder.AddOrUpdate("Admin.Promotions").Value("de", "Marketing");
			builder.AddOrUpdate("Admin.Plugins.Manage",
				"Manage plugins",
				"Plugins verwalten");
		}
	}
}
