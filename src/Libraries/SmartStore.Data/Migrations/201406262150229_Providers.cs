namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
	using SmartStore.Data.Setup;

	public partial class Providers : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
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
		}

		public void MigrateLocaleResources(LocaleResourcesBuilder builder)
		{
			builder.AddOrUpdate("Providers.FriendlyName.CurrencyExchange.ECB",
				"ECB exchange rate provider",
				"EZB-Wechselkursdienst");

			builder.AddOrUpdate("Providers.CurrencyExchange.ECB.SetCurrencyToEURO",
				"You can use ECB (European central bank) exchange rate provider only when exchange rate currency code is set to EURO",
				"Der EZB-Wechselkursdienst kann nur genutzt werden, wenn der Wechselkurs-Währungscode auf EUR gesetzt ist.");

			builder.AddOrUpdate("Providers.FriendlyName.CurrencyExchange.MoneyConverter",
				"Money converter exchange rate provider",
				"Money Converter Wechselkursdienst");
		}
    }
}
