namespace SmartStore.Data.Migrations
{
    using System.Data.Entity.Migrations;
    using Setup;

    public partial class CurrencyRounding : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
            AddColumn("dbo.Order", "OrderTotalRounding", c => c.Decimal(nullable: false, precision: 18, scale: 4));
            AddColumn("dbo.Currency", "RoundOrderItemsEnabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.Currency", "RoundNumDecimals", c => c.Int(nullable: false, defaultValue: 2));
            AddColumn("dbo.Currency", "RoundOrderTotalEnabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.Currency", "RoundOrderTotalDenominator", c => c.Decimal(nullable: false, precision: 18, scale: 4));
            AddColumn("dbo.Currency", "RoundOrderTotalRule", c => c.Int(nullable: false));
            AddColumn("dbo.PaymentMethod", "RoundOrderTotalEnabled", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.PaymentMethod", "RoundOrderTotalEnabled");
            DropColumn("dbo.Currency", "RoundOrderTotalRule");
            DropColumn("dbo.Currency", "RoundOrderTotalDenominator");
            DropColumn("dbo.Currency", "RoundOrderTotalEnabled");
            DropColumn("dbo.Currency", "RoundNumDecimals");
            DropColumn("dbo.Currency", "RoundOrderItemsEnabled");
            DropColumn("dbo.Order", "OrderTotalRounding");
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
            builder.AddOrUpdate("Common.Round", "Round", "Runden");
            builder.AddOrUpdate("ShoppingCart.Totals.Rounding", "Rounding", "Rundung");

            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Directory.CurrencyRoundingRule.RoundMidpointDown",
                "Round midpoint down (e.g. 0.05 rounding: 9.225 will round to 9.20)",
                "Mittelwert abrunden (z.B. 0,05 Rundung: 9,225 wird auf 9,20 gerundet)");
            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Directory.CurrencyRoundingRule.RoundMidpointUp",
                "Round midpoint up (e.g. 0.05 rounding: 9.225 will round to 9.25)",
                "Mittelwert aufrunden (z.B. 0,05 Rundung: 9,225 wird auf 9,25 gerundet)");
            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Directory.CurrencyRoundingRule.AlwaysRoundDown",
                "Always round down (e.g. 0.05 rounding: 9.24 will round to 9.20)",
                "Immer abrunden (z.B. 0,05 Rundung: 9,24 wird auf 9,20 gerundet)");
            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Directory.CurrencyRoundingRule.AlwaysRoundUp",
                "Always round up (e.g. 0.05 rounding, 9.26 will round to 9.30)",
                "Immer aufrunden (z.B. 0,05 Rundung: 9,26 wird auf 9,30 gerundet)");

            builder.AddOrUpdate("Admin.Configuration.Currencies.NoPaymentMethodsEnabledRounding",
                "Regardless of the currency configuration, the order totals are only rounded if it has been enabled for the selected payment method. Click <a href='{0}'>Edit</a> for the desired payment methods and activate the rounding option.",
                "Unabhängig von der Währungs-Konfiguration werden Bestellsummen erst gerundet, wenn das für die gewählte Zahlart auch vorgesehen ist. Klicken Sie bei den gewünschten Zahlarten auf <a href='{0}'>Bearbeiten</a> und aktivieren Sie die Rundungs-Option.");

            builder.AddOrUpdate("Admin.Configuration.Currencies.PaymentMethodsEnabledRoundingList",
                "Payment methods for which order total rounding is enabled",
                "Zahlarten, bei denen Bestellsummen-Rundung aktiviert ist");

            builder.AddOrUpdate("Admin.Configuration.Currencies.Fields.RoundOrderItemsEnabled",
                "Round all order item amounts",
                "Beträge aller Bestellpositionen runden",
                "Specifies whether to round all order item amounts (products, fees, tax etc.)",
                "Legt fest, ob die Beträge aller Bestellpositionen gerundet werden sollen (Produkte, Gebühren, Steuern etc.)");

            builder.AddOrUpdate("Admin.Configuration.Currencies.Fields.RoundNumDecimals",
                "Number of decimal digits",
                "Anzahl Dezimalstellen",
                "Specifies the number of decimal digits to round to (Default: 2)",
                "Legt fest, auf wieviele Dezimalstellen gerundet werden soll (Standard: 2)");

            builder.AddOrUpdate("Admin.Configuration.Currencies.Fields.RoundOrderTotalEnabled",
                "Round order total amount",
                "Bestellsumme runden",
                "Specifies whether to round the order total amount.",
                "Legt fest, ob die Bestellsumme gerundet werden soll.");

            builder.AddOrUpdate("Admin.Configuration.Currencies.Fields.RoundOrderTotalDenominator",
                "Round to",
                "Runden nach",
                "Specifies the nearest multiple of the smallest chosen amount to round the order total to. 0.05 for example will round 9.43 up to 9.45.",
                "Legt das nächste Vielfache des kleinsten, gewählten Betrages fest, auf den die Bestellsumme gerundet werden soll. Bei 0,05 wird z.B. 9,43 auf 9,45 gerundet.");

            builder.AddOrUpdate("Admin.Configuration.Currencies.Fields.RoundOrderTotalRule",
                "Rounding rule",
                "Rundungsregel",
                "Specifies the rule for rounding the order total amount.",
                "Legt die Regel für das Runden der Bestellsumme fest.");

            builder.AddOrUpdate("Admin.Configuration.Payment.Methods.RoundOrderTotalEnabled",
                "Round order total amount (if enabled)",
                "Bestellsumme runden, sofern aktiviert",
                "Specifies whether to round the order total in accordance with currency configuration if this payment method was selected in checkout.",
                "Legt fest, ob die Bestellsumme gemäß Währungs-Konfiguration gerundet werden soll, wenn diese Zahlart im Checkout gewählt wurde.");

            builder.AddOrUpdate("Admin.Configuration.Currencies.Fields.RoundOrderItemsEnabled.Validation",
                "The number of decimal digits must be between 0 and 8.",
                "Die Anzahl der Dezimalstellen muss zwischen 0 und 8 liegen.");

			builder.Delete(
				"Admin.Configuration.Settings.ShoppingCart.RoundPricesDuringCalculation",
				"Admin.Configuration.Settings.ShoppingCart.RoundPricesDuringCalculation.Hint");

			builder.AddOrUpdate("Admin.Orders.Fields.OrderTotalRounding",
                "Rounding",
                "Rundung",
                "The amount by which the order total was rounded up or down.",
                "Der Betrag, um den der Auftragswert auf- bzw. abgerundet wurde.");
        }
    }
}
