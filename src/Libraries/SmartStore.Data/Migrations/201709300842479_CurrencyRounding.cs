namespace SmartStore.Data.Migrations
{
    using Setup;
    using System.Data.Entity.Migrations;

    public partial class CurrencyRounding : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
            AddColumn("dbo.Currency", "RoundingMethod", c => c.Int(nullable: false));
        }

        public override void Down()
        {
            DropColumn("dbo.Currency", "RoundingMethod");
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
			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Directory.CurrencyRoundingMethod.Default",
                "Default",
                "Standard");
            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Directory.CurrencyRoundingMethod.Down005",
                "Round down to nearest 0.05",
                "Auf nächste 0,05 abrunden");
            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Directory.CurrencyRoundingMethod.Up005",
				"Round up to nearest 0.05",
                "Auf nächste 0,05 aufrunden");
            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Directory.CurrencyRoundingMethod.Down01",
				"Round down to nearest 0.10",
                "Auf nächste 0,10 abrunden");
            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Directory.CurrencyRoundingMethod.Up01",
				"Round up to nearest 0.10",
                "Auf nächste 0,10 aufrunden");
            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Directory.CurrencyRoundingMethod.Interval05",
				"Round in 0.50 intervals",
                "In 0,50 Intervalle runden");
            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Directory.CurrencyRoundingMethod.Interval1",
				"Round in 1.00 intervals",
                "In 1,00 Intervalle runden");
            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Directory.CurrencyRoundingMethod.Up1",
                "Round up to nearest 1.00",
                "Auf nächste 1,00 aufrunden");

            builder.AddOrUpdate("Admin.Configuration.Currencies.Fields.RoundingMethod",
                "Round",
                "Runden",
                "Specifies how to round currency values.",
                "Legt fest, wie Währungswerte gerundet werden sollen.");

			builder.AddOrUpdate("Admin.Configuration.Currencies.Fields.RoundingMethod.Default.Hint",
				"Examples:<br />9.4548 round down to 9.45<br />9.4568 round up to 9.46",
				"Beispiele:<br />9,4548 abrunden nach 9,45<br />9,4568 aufrunden nach 9,46");

			builder.AddOrUpdate("Admin.Configuration.Currencies.Fields.RoundingMethod.Down005.Hint",
				"Examples:<br />9.43 round down to 9.40<br />9.46 round down to 9.45<br />9.496 round up to 9.50",
				"Beispiele:<br />9,43 abrunden nach 9,40<br />9,46 abrunden nach 9,45<br />9,496 aufrunden nach 9,50");

			builder.AddOrUpdate("Admin.Configuration.Currencies.Fields.RoundingMethod.Up005.Hint",
				"Examples:<br />9.001 round down to 9.00<br />9.02 round up to 9.05<br />9.08 round up to 9.10",
				"Beispiele:<br />9,001 abrunden nach 9,00<br />9,02 aufrunden nach 9,05<br />9,08 aufrunden nach 9,10");

			builder.AddOrUpdate("Admin.Configuration.Currencies.Fields.RoundingMethod.Down01.Hint",
				"Examples:<br />9.43 round down to 9.40<br />9.46 round up to 9.50",
				"Beispiele:<br />9,43 abrunden nach 9,40<br />9,46 aufrunden nach 9,50");

			builder.AddOrUpdate("Admin.Configuration.Currencies.Fields.RoundingMethod.Up01.Hint",
				"Examples:<br />9.02 round down to 9.00<br />9.08 round up to 9.10",
				"Beispiele:<br />9,02 abrunden nach 9,00<br />9,08 aufrunden nach 9,10");

			builder.AddOrUpdate("Admin.Configuration.Currencies.Fields.RoundingMethod.Interval05.Hint",
				"0.01 - 0.24: round down to 0.00<br />0.25 - 0.49: round up to 0.50<br />0.51 - 0.74: round down to 0.50<br />0.75 - 0.99: round up to next integer",
				"0,01 - 0,24: abrunden nach 0,00<br />0,25 - 0,49: aufrunden nach 0,50<br />0,51 - 0,74: abrunden nach 0,50<br />0,75 - 0,99: aufrunden zur nächsten Ganzzahl");

			builder.AddOrUpdate("Admin.Configuration.Currencies.Fields.RoundingMethod.Interval1.Hint",
				"0.01 - 0.49: round down to 0.00<br />0.50 - 0.99: round up to next integer",
				"0,01 - 0,49: abrunden nach 0,00<br />0,50 - 0,99: aufrunden zur nächsten Ganzzahl");

			builder.AddOrUpdate("Admin.Configuration.Currencies.Fields.RoundingMethod.Up1.Hint",
				"Always round up decimals to next integer",
				"Dezimalzahlen immer zur nächsten Ganzzahl aufrunden");
		}
    }
}
