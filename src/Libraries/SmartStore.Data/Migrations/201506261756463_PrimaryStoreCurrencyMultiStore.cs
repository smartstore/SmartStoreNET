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

			// avoid conflicts with foreign key constraint
			Sql("Update dbo.Store Set PrimaryStoreCurrencyId = (Select Top(1) [Id] From dbo.Currency)");
			Sql("Update dbo.Store Set PrimaryExchangeRateCurrencyId = (Select Top(1) [Id] From dbo.Currency)");

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
			builder.AddOrUpdate("Admin.Configuration.Currencies.DeleteOrPublishStoreConflict",
				"The currency cannot be deleted or deactivated because it is attached to the store \"{0}\" as primary or exchange rate currency.",
				"Die Währung kann nicht gelöscht oder deaktiviert werden, weil sie dem Shop \"{0}\" als Leit- oder Umrechnungswährung zugeordnet ist.");

			//builder.AddOrUpdate("Admin.Configuration.Currencies.StoreLimitationConflict",
			//	"The store limitations must include store \"{0}\" because the currency is attached to it as primary or exchange rate currency.",
			//	"Die Shop-Eingrenzungen müssen den Shop \"{0}\" enthalten, da ihm die Währung als Leit- oder Umrechnungswährung zugeordnet ist.");

			builder.AddOrUpdate("Admin.Configuration.Stores.Fields.PrimaryStoreCurrencyId",
				"Primary store currency",
				"Leitwährung",
				"Specifies the the primary store currency.",
				"Legt die Leitwährung des Shops fest.");

			builder.AddOrUpdate("Admin.Configuration.Stores.Fields.PrimaryExchangeRateCurrencyId",
				"Exchange rate currency",
				"Umrechnungswährung",
				"Specifies the primary exchange rate currency for this store.",
				"Legt die Umrechnungswährung für diesen Shop fest.");

			builder.AddOrUpdate("Admin.Configuration.Currencies.Fields.IsPrimaryStoreCurrency",
				"Primary currency",
				"Leitwährung");

			builder.AddOrUpdate("Admin.Configuration.Currencies.Fields.IsPrimaryExchangeRateCurrency",
				"Exchange rate currency",
				"Umrechnungswährung");

			builder.AddOrUpdate("Admin.Configuration.Currencies.Fields.PrimaryStoreCurrencyStores",
				"Is primary store currency for",
				"Ist Leitwährung für",
				"A list of shores where the currency is primary store currency.",
				"Eine Liste mit Shops, in denen die Währung Leitwährung ist.");

			builder.AddOrUpdate("Admin.Configuration.Currencies.Fields.PrimaryExchangeRateCurrencyStores",
				"Is exchange rate currency for",
				"Ist Umrechnungswährung für",
				"A list of shores where the currency is primary exchange rate currency.",
				"Eine Liste mit Shops, in denen die Währung Umrechnungswährung ist.");

			builder.AddOrUpdate("Admin.Configuration.Stores.Fields.SslEnabled",
				"SSL",
				"SSL",
				"Specifies whether the store should be SSL secured.",
				"Legt fest, ob der Shop SSL gesichert werden soll.");

			builder.Delete("Admin.Configuration.Currencies.CantDeletePrimary");
			builder.Delete("Admin.Configuration.Currencies.CantDeleteExchange");
			builder.Delete("Admin.Configuration.Currencies.Fields.MarkAsPrimaryStoreCurrency");
			builder.Delete("Admin.Configuration.Currencies.Fields.MarkAsPrimaryExchangeRateCurrency");
		}
    }
}
