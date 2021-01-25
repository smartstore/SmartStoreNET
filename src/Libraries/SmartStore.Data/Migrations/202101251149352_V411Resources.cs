namespace SmartStore.Data.Migrations
{
    using System.Data.Entity.Migrations;
    using SmartStore.Data.Setup;

    public partial class V411Resources : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
        }
        
        public override void Down()
        {
        }

        public bool RollbackOnFailure => false;

        public void Seed(SmartObjectContext context)
        {
            context.MigrateLocaleResources(MigrateLocaleResources);
            context.SaveChanges();
        }

        public void MigrateLocaleResources(LocaleResourcesBuilder builder)
        {
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.CartItemQuantity",
                "Product quantity is in range",
                "Produktmenge liegt in folgendem Bereich");

            builder.AddOrUpdate("Newsletter.SubscriptionFailed",
                "The subscription or unsubscription has failed.",
                "Die Abonnierung bzw. Abbestellung ist fehlgeschlagen.");

            builder.AddOrUpdate("Common.UnsupportedBrowser",
                "You are using an unsupported browser! Please consider switching to a modern browser such as Google Chrome, Firefox or Opera to fully enjoy your shopping experience.",
                "Sie verwenden einen nicht unterstützten Browser! Bitte ziehen Sie in Betracht, zu einem modernen Browser wie Google Chrome, Firefox oder Opera zu wechseln, um Ihr Einkaufserlebnis in vollen Zügen genießen zu können.");

            builder.Delete("Admin.Configuration.Settings.Order.ApplyToSubtotal");
            builder.Delete("Checkout.MaxOrderTotalAmount");
            builder.Delete("Checkout.MinOrderTotalAmount");

            builder.AddOrUpdate("Checkout.MaxOrderSubtotalAmount",
                "Your maximum order total allowed is {0}.",
                "Ihr zulässiger Höchstbestellwert beträgt {0}.");

            builder.AddOrUpdate("Checkout.MinOrderSubtotalAmount",
                "Your minimum order total allowed is {0}.",
                "Ihr zulässiger Mindestbestellwert beträgt {0}.");

            builder.Delete("Admin.Configuration.Settings.Order.OrderTotalRestrictionType");

            builder.AddOrUpdate("Admin.Configuration.Settings.Order.MultipleOrderTotalRestrictionsExpandRange",
                "Customer groups extend the value range",
                "Kundengruppen erweitern den Wertebereich",
                "Specifies whether multiple order total restrictions through customer group assignments extend the allowed order value range.",
                "Legt fest, ob mehrfache Bestellwertbeschränkungen durch Kundengruppenzuordnungen den erlaubten Bestellwertbereich erweitern.");

            builder.AddOrUpdate("ActivityLog.EditOrder",
                "Edited order {0}",
                "Auftrag {0} bearbeitet");

            builder.AddOrUpdate("Admin.ContentManagement.Blog.BlogPosts.Fields.Language",
                "Regional relevance",
                "Regionale Relevanz",
                "Specifies the language for which the post is displayed. If limited to one language, blog contents need only be edited in that language (no multilingualism).",
                "Legt fest, für welche Sprache der Beitrag angezeigt wird. Bei einer Begrenzung auf eine Sprache brauchen Blog-Inhalte nur in dieser Sprache eingegeben zu werden (keine Mehrsprachigkeit).");

            builder.AddOrUpdate("Admin.ContentManagement.News.NewsItems.Fields.Language",
                "Regional relevance",
                "Regionale Relevanz",
                "Specifies the language for which the news is displayed. If limited to one language, news contents need only be edited in that language (no multilingualism).",
                "Legt fest, für welche Sprache die News angezeigt wird. Bei einer Begrenzung auf eine Sprache brauchen News-Inhalte nur in dieser Sprache eingegeben zu werden (keine Mehrsprachigkeit).");

            builder.AddOrUpdate("Common.International", "International", "International");

            builder.AddOrUpdate("Admin.Plugins.KnownGroup.B2B", "B2B", "B2B");

            builder.AddOrUpdate("Admin.Configuration.Settings.GeneralCommon.ReCaptchaTypeChangeWarning",
                "When changing the reCAPTCHA type, the public and private key must also be changed.",
                "Beim Ändern des reCAPTCHA Typs müssen auch die beiden Zugangsschlüssel (public\\private key) geändert werden.");

            builder.AddOrUpdate("Admin.Configuration.Settings.GeneralCommon.ExtraRobotsAllows",
                "Extra Allows for robots.txt",
                "Extra Allows für robots.txt",
                "Enter additional paths that should be included as Allow entries in your robots.txt. Each entry has to be entered in a new line.",
                "Geben Sie hier zusätzliche Pfade an, die als Allow-Einträge zur robots.txt hinzugefügt werden sollen. Jeder Eintrag muss in einer neuen Zeile erfolgen.");

            builder.AddOrUpdate("Admin.Configuration.Settings.GeneralCommon.DisplayAllows", "Show items for 'Allow'", "Einträge für 'Allow' anzeigen");
            builder.AddOrUpdate("Admin.Configuration.Settings.GeneralCommon.DisplayDisallows", "Show items for 'Disallow'", "Einträge für 'Disallow' anzeigen");
        }
    }
}
