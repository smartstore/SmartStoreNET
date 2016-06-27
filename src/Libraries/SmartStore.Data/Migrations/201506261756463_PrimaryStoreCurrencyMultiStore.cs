namespace SmartStore.Data.Migrations
{
	using System.Data.Entity.Migrations;
	using System.Linq;
	using System.Web.Hosting;
	using SmartStore.Core.Data;
	using SmartStore.Core.Domain.Configuration;
	using SmartStore.Core.Domain.Directory;
	using SmartStore.Core.Domain.Stores;
	using SmartStore.Data.Setup;

	public partial class PrimaryStoreCurrencyMultiStore : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
			AddColumn("dbo.Store", "PrimaryStoreCurrencyId", c => c.Int(nullable: false, defaultValue: 1));
			AddColumn("dbo.Store", "PrimaryExchangeRateCurrencyId", c => c.Int(nullable: false, defaultValue: 1));

			// avoid conflicts with foreign key constraint
			if (HostingEnvironment.IsHosted && DataSettings.Current.IsSqlServer)
			{
				// what sql-server compact does not support here:
				// - Update Set with a select sub-query
				// - Select in variable via declare
				// - Alter table to check/nocheck a constraint
				// so the the foreign key check constraint fails here (and therefore this migration) if there's no currency with id 1.

				Sql("Update dbo.Store Set PrimaryStoreCurrencyId = (Select Min(Id) From dbo.Currency)");
				Sql("Update dbo.Store Set PrimaryExchangeRateCurrencyId = (Select Min(Id) From dbo.Currency)");
			}

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
			var primaryStoreCurrencySetting = settings.FirstOrDefault(x => x.Name == "CurrencySettings.PrimaryStoreCurrencyId");
			var primaryExchangeRateCurrencySetting = settings.FirstOrDefault(x => x.Name == "CurrencySettings.PrimaryExchangeRateCurrencyId");

			int primaryStoreCurrencyId = primaryStoreCurrencySetting.Value.ToInt();
			int primaryExchangeRateCurrencyId = primaryExchangeRateCurrencySetting.Value.ToInt();

			if (primaryStoreCurrencyId == 0)
				primaryStoreCurrencyId = context.Set<Currency>().First().Id;

			if (primaryExchangeRateCurrencyId == 0)
				primaryExchangeRateCurrencyId = primaryStoreCurrencyId;

			var stores = context.Set<Store>().ToList();

			stores.ForEach(x =>
			{
				x.PrimaryStoreCurrencyId = primaryStoreCurrencyId;
				x.PrimaryExchangeRateCurrencyId = primaryExchangeRateCurrencyId;
			});

			settings.Remove(primaryStoreCurrencySetting);
			settings.Remove(primaryExchangeRateCurrencySetting);

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
				"A list of stores where the currency is primary store currency.",
				"Eine Liste mit Shops, in denen die Währung Leitwährung ist.");

			builder.AddOrUpdate("Admin.Configuration.Currencies.Fields.PrimaryExchangeRateCurrencyStores",
				"Is exchange rate currency for",
				"Ist Umrechnungswährung für",
				"A list of stores where the currency is primary exchange rate currency.",
				"Eine Liste mit Shops, in denen die Währung Umrechnungswährung ist.");

			builder.AddOrUpdate("Admin.Configuration.Stores.Fields.SslEnabled",
				"SSL",
				"SSL",
				"Specifies whether the store should be SSL secured.",
				"Legt fest, ob der Shop SSL gesichert werden soll.");

			builder.AddOrUpdate("Admin.Configuration.Settings.News.MaxAgeInDays",
				"Maximum age (in days)",
				"Maximales Alter (in Tagen)",
				"Specifies the maximum news age in days. Older news are not exported in the RSS feed.",
				"Legt das maximale News-Alter in Tagen fest. Ältere News werden im RSS-Feed nicht exportiert.");

			builder.AddOrUpdate("Admin.Configuration.Settings.Blog.MaxAgeInDays",
				"Maximum age (in days)",
				"Maximales Alter (in Tagen)",
				"Specifies the maximum news age in days. Older blog posts are not exported in the RSS feed.",
				"Legt das maximale Blog-Alter in Tagen fest. Ältere Blog-Einträge werden im RSS-Feed nicht exportiert.");


			builder.AddOrUpdate("Admin.Common.Deleted",
				"Deleted",
				"Gelöscht");


			builder.Delete("Admin.Configuration.Currencies.CantDeletePrimary");
			builder.Delete("Admin.Configuration.Currencies.CantDeleteExchange");
			builder.Delete("Admin.Configuration.Currencies.Fields.MarkAsPrimaryStoreCurrency");
			builder.Delete("Admin.Configuration.Currencies.Fields.MarkAsPrimaryExchangeRateCurrency");
			builder.Delete("Forum.ForumFeedTitle");
		}
    }
}
