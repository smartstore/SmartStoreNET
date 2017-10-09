namespace SmartStore.Data.Migrations
{
    using System.Data.Entity.Migrations;
    using Setup;

    public partial class CurrencyRounding : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
            AddColumn("dbo.Currency", "RoundDuringCalculation", c => c.Boolean(nullable: false));
            AddColumn("dbo.Currency", "RoundDuringCalculationDecimals", c => c.Int(nullable: false, defaultValue: 2));
            AddColumn("dbo.Currency", "RoundOrderTotal", c => c.Boolean(nullable: false));
            AddColumn("dbo.Currency", "RoundOrderTotalToValue", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.Currency", "RoundOrderTotalRule", c => c.Int(nullable: false));
            AddColumn("dbo.PaymentMethod", "RoundOrderTotal", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.PaymentMethod", "RoundOrderTotal");
            DropColumn("dbo.Currency", "RoundOrderTotalRule");
            DropColumn("dbo.Currency", "RoundOrderTotalToValue");
            DropColumn("dbo.Currency", "RoundOrderTotal");
            DropColumn("dbo.Currency", "RoundDuringCalculationDecimals");
            DropColumn("dbo.Currency", "RoundDuringCalculation");
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
                "Round midpoint down (e.g. 0.05 rounding, 9.225 will round to 9.20)",
                "Mittelwert abrunden (z.B. 0,05 Rundung, 9,225 wird auf 9,20 gerundet)");
            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Directory.CurrencyRoundingRule.RoundMidpointUp",
                "Round midpoint up (e.g. 0.05 rounding, 9.225 will round to 9.25)",
                "Mittelwert aufrunden (z.B. 0,05 Rundung, 9,225 wird auf 9,25 gerundet)");
            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Directory.CurrencyRoundingRule.AlwaysRoundDown",
                "Always round down (e.g. 0.05 rounding, 9.24 will round to 9.20)",
                "Immer abrunden (z.B. 0,05 Rundung, 9,24 wird auf 9,20 gerundet)");
            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Directory.CurrencyRoundingRule.AlwaysRoundUp",
                "Always round up (e.g. 0.05 rounding, 9.26 will round to 9.30)",
                "Immer abrunden (z.B. 0,05 Rundung, 9,26 wird auf 9,30 gerundet)");

            builder.AddOrUpdate("Admin.Configuration.Currencies.NoPaymentMethodsEnabledRounding",
                "The rounding of the order value is not activated for any payment methid. Click <a href='{0}'>edit</a> for the desired payment methods and activate the rounding of the order total.",
                "Die Rundung des Auftragswertes ist bei keiner Zahlart aktiviert. Klicken Sie bei den gewünschten Zahlarten auf <a href='{0}'>Bearbeiten</a> und aktivieren Sie die Rundung des Auftragswertes.");

            builder.AddOrUpdate("Admin.Configuration.Currencies.PaymentMethodsEnabledRoundingList",
                "Payment methods for which rounding of the order total is enabled",
                "Zahlarten, bei denen die Rundung des Auftragswertes aktiviert ist");

            builder.AddOrUpdate("Admin.Configuration.Currencies.Fields.RoundDuringCalculation",
                "Round during price calculation",
                "Während der Preisberechnung runden",
                "Specifies whether to round during price calculation.",
                "Legt fest, ob bereits während der Preisberechnung gerundet werden soll.");

            builder.AddOrUpdate("Admin.Configuration.Currencies.Fields.RoundDuringCalculationDecimals",
                "Number of decimal places",
                "Anzahl der Dezimalstellen",
                "Specifies the number of decimal places to round to. 2 is the default value.",
                "Legt fest, auf wieviele Dezimalstellen gerundet werden soll. 2 ist der Standardwert.");

            builder.AddOrUpdate("Admin.Configuration.Currencies.Fields.RoundOrderTotal",
                "Round order total",
                "Auftragswert runden",
                "Specifies whether to round the order total.",
                "Legt fest, ob der Gesamtwert eines Auftrages gerundet werden soll.");

            builder.AddOrUpdate("Admin.Configuration.Currencies.Fields.RoundOrderTotalToValue",
                "Round to",
                "Runden nach",
                "Specifies the nearest multiple of the smallest, chosen amount to round the order total to. For 0.05 for example 9.43 will be rounded to 9.45.",
                "Legt das nächste Vielfache des kleinsten, gewählten Betrages fest, auf den der Aufragswert gerundet werden soll. Bei 0,05 wird z.B. 9.43 auf 9.45 gerundet.");

            builder.AddOrUpdate("Admin.Configuration.Currencies.Fields.RoundOrderTotalRule",
                "Rounding rule",
                "Rundungsregel",
                "Specifies the rule for rounding the order total.",
                "Legt die Rundungsregel für das Runden des Auftragswertes fest.");

            builder.AddOrUpdate("Admin.Configuration.Payment.Methods.RoundOrderTotal",
                "Apply rounding, if enabled",
                "Runden anwenden, sofern aktiviert",
                "Specifies whether to round the order total for this payment method if rounding for the chosen currency is enabled.",
                "Legt fest, ob die Rundung des Auftragswertes für diese Zahlart angewendet werden soll, vorausgesetzt Runden ist für die gewählte Währung aktiviert.");

            builder.AddOrUpdate("Admin.Configuration.Currencies.Fields.RoundDuringCalculationDecimals.Validation",
                "The number of decimal places must be between 0 and 8.",
                "Die Anzahl der Dezimalstellen muss zwischen 0 und 8 liegen.");

            builder.Delete(
                "Admin.Configuration.Settings.ShoppingCart.RoundPricesDuringCalculation",
                "Admin.Configuration.Settings.ShoppingCart.RoundPricesDuringCalculation.Hint");
        }
    }
}
