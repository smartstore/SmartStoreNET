namespace SmartStore.Data.Migrations
{
	using System;
	using System.Data.Entity;
	using System.Data.Entity.Migrations;
	using System.Linq;
	using Setup;

	public sealed class MigrationsConfiguration : DbMigrationsConfiguration<SmartObjectContext>
	{
		public MigrationsConfiguration()
		{
			AutomaticMigrationsEnabled = false;
			AutomaticMigrationDataLossAllowed = true;
			ContextKey = "SmartStore.Core";
		}

		public void SeedDatabase(SmartObjectContext context)
		{
			Seed(context);
		}

		protected override void Seed(SmartObjectContext context)
		{
			context.MigrateLocaleResources(MigrateLocaleResources);
			MigrateSettings(context);

			context.SaveChanges();
        }

		public void MigrateSettings(SmartObjectContext context)
		{

		}

		public void MigrateLocaleResources(LocaleResourcesBuilder builder)
		{
			builder.AddOrUpdate("Admin.ReturnRequests.MaxRefundAmount",
				"Maximum refund amount",
				"Maximaler Erstattungsbetrag",
				"The maximum amount that can be refunded for this return request.",
				"Der maximale Betrag, der für diesen Rücksendewunsch erstattet werden kann.");
		}
	}
}
