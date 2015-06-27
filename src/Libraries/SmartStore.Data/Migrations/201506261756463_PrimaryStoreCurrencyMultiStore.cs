namespace SmartStore.Data.Migrations
{
	using System.Data.Entity.Migrations;
	using System.Linq;
	using SmartStore.Core.Domain.Configuration;
	using SmartStore.Core.Domain.Stores;
	using SmartStore.Data.Setup;

	public partial class PrimaryStoreCurrencyMultiStore : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
            AddColumn("dbo.Store", "PrimaryStoreCurrencyId", c => c.Int(nullable: false));
            AddColumn("dbo.Store", "PrimaryExchangeRateCurrencyId", c => c.Int(nullable: false));
            CreateIndex("dbo.Store", "PrimaryStoreCurrencyId");
            CreateIndex("dbo.Store", "PrimaryExchangeRateCurrencyId");
            AddForeignKey("dbo.Store", "PrimaryExchangeRateCurrencyId", "dbo.Currency", "Id");
            AddForeignKey("dbo.Store", "PrimaryStoreCurrencyId", "dbo.Currency", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Store", "PrimaryStoreCurrencyId", "dbo.Currency");
            DropForeignKey("dbo.Store", "PrimaryExchangeRateCurrencyId", "dbo.Currency");
            DropIndex("dbo.Store", new[] { "PrimaryExchangeRateCurrencyId" });
            DropIndex("dbo.Store", new[] { "PrimaryStoreCurrencyId" });
            DropColumn("dbo.Store", "PrimaryExchangeRateCurrencyId");
            DropColumn("dbo.Store", "PrimaryStoreCurrencyId");
        }

		public bool RollbackOnFailure
		{
			get { return false; }
		}

		public void Seed(SmartObjectContext context)
		{
			context.MigrateLocaleResources(MigrateLocaleResources);

			var settings = context.Set<Setting>();
			var primaryStoreCurrency = settings.FirstOrDefault(x => x.Name == "CurrencySettings.PrimaryStoreCurrencyId");
			var primaryExchangeRateCurrency = settings.FirstOrDefault(x => x.Name == "CurrencySettings.PrimaryExchangeRateCurrencyId");

			var stores = context.Set<Store>().ToList();

			foreach (var store in stores)
			{
				int id = primaryStoreCurrency.Value.ToInt();
				if (id != 0)
					store.PrimaryStoreCurrencyId = id;

				id = primaryExchangeRateCurrency.Value.ToInt();
				if (id != 0)
					store.PrimaryExchangeRateCurrencyId = id;
			}

			settings.Remove(primaryStoreCurrency);
			settings.Remove(primaryExchangeRateCurrency);

			context.SaveChanges();
		}

		public void MigrateLocaleResources(LocaleResourcesBuilder builder)
		{
			builder.AddOrUpdate("Admin.Configuration.Currencies.NoDeleteOrDeactivate",
				"The currency cannot be deleted or deactivated because it is attached to the store \"{0}\" as primary or exchange rate currency.",
				"Die Währung kann nicht gelöscht oder deaktiviert werden, weil sie dem Shop \"{0}\" als Leit- oder Umrechnungswährung zugeordnet ist.");



			builder.Delete("Admin.Configuration.Currencies.CantDeletePrimary");
			builder.Delete("Admin.Configuration.Currencies.CantDeleteExchange");
		}
    }
}
