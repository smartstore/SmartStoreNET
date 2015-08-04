namespace SmartStore.Data.Migrations
{
	using System;
	using System.Linq;
	using System.Data.Entity.Migrations;
	using SmartStore.Core.Domain.Tasks;
	using SmartStore.Data.Setup;
	using SmartStore.Core.Domain.Localization;

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

			builder.AddOrUpdate("admin.help.nopcommercenote",
				"SmartStore.NET is a fork of the ASP.NET open source e-commerce solution {0}.",
				"SmartStore.NET ist ein Fork der ASP.NET Open-Source E-Commerce-Lösung {0}.");

			builder.AddOrUpdate("Payment.ExpirationDate",
				"Valid until",
				"Gültig bis");

			builder.Update("Plugins.Payment.CashOnDelivery.PaymentInfoDescription")
				.Value("en", "Once your order is placed, you will be contacted by our staff to confirm the order.")
				.Value("de", "Sobald Ihre Bestellung abgeschloßen ist, werden Sie persönlich von einem unserer Mitarbeiter kontaktiert, um die Bestellung zu bestätigen.");

			builder.Update("Plugins.Payment.Invoice.PaymentInfoDescription")
				.Value("en", "Once your order is placed, you will be contacted by our staff to confirm the order.")
				.Value("de", "Sobald Ihre Bestellung abgeschloßen ist, werden Sie persönlich von einem unserer Mitarbeiter kontaktiert, um die Bestellung zu bestätigen.");

			builder.Update("Plugins.Payment.DirectDebit.PaymentInfoDescription")
				.Value("en", "Once your order is placed, you will be contacted by our staff to confirm the order.")
				.Value("de", "Sobald Ihre Bestellung abgeschloßen ist, werden Sie persönlich von einem unserer Mitarbeiter kontaktiert, um die Bestellung zu bestätigen.");

			builder.Update("Plugins.Payment.PayInStore.PaymentInfoDescription")
				.Value("en", "Reserve items at your local store, and pay in store when you pick up your order.")
				.Value("de", "Reservieren Sie Produkte und zahlen Sie an der Kasse in unserem Ladenlokal.");

			builder.Update("Plugins.Payment.Prepayment.PaymentInfoDescription")
				.Value("en", "Once your order is placed, you will be contacted by our staff to confirm the order.")
				.Value("de", "Sobald Ihre Bestellung abgeschloßen ist, werden Sie persönlich von einem unserer Mitarbeiter kontaktiert, um die Bestellung zu bestätigen.");

		}
	}
}
