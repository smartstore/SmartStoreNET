namespace SmartStore.Data.Migrations
{
    using System.Data.Entity.Migrations;
    using System.Linq;
    using SmartStore.Core.Data;
    using SmartStore.Core.Domain.Cms;
    using SmartStore.Data.Setup;

    public partial class CookieManager : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
        }

        public override void Down()
        {
        }

        public bool RollbackOnFailure => true;

        public void Seed(SmartObjectContext context)
        {
            if (!DataSettings.DatabaseIsInstalled())
                return;

            // Add resources.
            context.MigrateLocaleResources(MigrateLocaleResources);

            // Add menu item to footer.
            var menuSet = context.Set<MenuRecord>();
            var menuItemSet = context.Set<MenuItemRecord>();
            var footerService = menuSet.Where(x => x.SystemName.Equals("FooterService")).FirstOrDefault();

            menuItemSet.Add(new MenuItemRecord
            {
                MenuId = footerService.Id,
                ProviderName = "route",
                Model = "{{\"routename\":\"{0}\"}}".FormatInvariant("CookieManager"),
                Title = "Cookie Manager",
                DisplayOrder = 100,
                Published = true,
                CssClass = "cookie-manager"
            });

            context.SaveChanges();
        }

        public void MigrateLocaleResources(LocaleResourcesBuilder builder)
        {
            builder.AddOrUpdate("CookieManager.MyAccount.Button", "Cookie Manager", "Cookie Manager");
            builder.AddOrUpdate("CookieManager.Dialog.Title", "Cookie settings", "Cookie-Einstellungen");
            builder.AddOrUpdate("CookieManager.Dialog.Heading", "Yes, we use cookies", "Ja, wir verwenden Cookies");
            builder.AddOrUpdate("CookieManager.Dialog.Intro",
                "You decide which cookies you allow or reject. You can change your decision at any time in your <a href='{0}'>My Account area</a>. Further information can also be found in our <a href='{1}'>privacy policy</a>.",
                "Sie entscheiden, welche Cookies Sie zulassen oder ablehnen. Ihre Entscheidung können Sie jederzeit in Ihrem <a href='{0}'>My-Account-Bereich</a> ändern. Weitere Infos auch in unserer <a href='{1}'>Datenschutzerklärung</a>.");

            builder.AddOrUpdate("CookieManager.Dialog.Required.Heading",
                "Required cookies",
                "Notwendige Cookies");
            builder.AddOrUpdate("CookieManager.Dialog.Required.Intro",
                "Technically required cookies help us to make the operation of the website possible. They provide basic functions such as the display of products or login and are therefore a prerequisite for using the site.",
                "Technisch erforderliche Cookies helfen uns dabei, die Bedienung der Webseite zu ermöglichen. Sie stellen Grundfunktionen, wie die Darstellung von Produkten oder den Login sicher und sind daher eine Voraussetzung zur Nutzung der Seite.");

            builder.AddOrUpdate("CookieManager.Dialog.Analytics.Heading",
                "Analytical cookies",
                "Analytische Cookies");
            builder.AddOrUpdate("CookieManager.Dialog.Analytics.Intro",
                "These cookies help us to improve our website by anonymously understanding the performance and use of our site.",
                "Diese Cookies helfen uns, unsere Website zu verbessern, indem wir anonym die Leistung und die Verwendung unserer Seite verstehen.");

            builder.AddOrUpdate("CookieManager.Dialog.ThirdParty.Heading",
                "Third party cookies",
                "Cookies von Drittanbietern");
            builder.AddOrUpdate("CookieManager.Dialog.ThirdParty.Intro",
                "These cookies help us to use comfort functions of other service providers and integrate them into our shop.",
                "Diese Cookies helfen uns, Komfort-Funktionen von anderen Dienstanbietern zu nutzen und in unseren Shop einzubinden.");

            builder.AddOrUpdate("CookieManager.Dialog.Show", "Show", "Anzeigen");
            builder.AddOrUpdate("CookieManager.Dialog.ReadMore", "Read more", "Weiterlesen");
            builder.AddOrUpdate("CookieManager.Dialog.Button.AcceptAll", "Accept all", "Alle auswählen");
            builder.AddOrUpdate("CookieManager.Dialog.Button.AcceptSelected", "Accept selected", "Auswahl bestätigen");

            // Remove deleted resources from consent badge.
            builder.Delete("CookieConsent.Button");
            builder.Delete("CookieConsent.BadgeText");
            builder.Delete("Admin.Configuration.Settings.CustomerUser.Privacy.CookieConsentBadgetext");
            builder.Delete("Admin.Configuration.Settings.CustomerUser.Privacy.CookieConsentBadgetext.Hint");
        }
    }
}
