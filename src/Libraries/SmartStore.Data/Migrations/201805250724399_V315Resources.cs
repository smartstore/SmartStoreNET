namespace SmartStore.Data.Migrations
{
    using System.Data.Entity.Migrations;
    using System.Linq;
    using SmartStore.Core.Domain.Catalog;
    using SmartStore.Core.Domain.Configuration;
    using SmartStore.Core.Domain.Seo;
    using SmartStore.Data.Setup;
    using SmartStore.Utilities;

    public partial class V315Resources : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
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

            MigrateSettings(context);
            context.SaveChanges();
        }

        public void MigrateSettings(SmartObjectContext context)
        {
            // SeoSettings.RedirectLegacyTopicUrls should be true when migrating (it is false by default after fresh install)
            var name = TypeHelper.NameOf<SeoSettings>(y => y.RedirectLegacyTopicUrls, true);
            context.MigrateSettings(x => x.Add(name, true));

            // remove setting which were moved from customer settings to privacy settings and have new default values which should be applied immediately 
            var settings = context.Set<Setting>();
            var storeLastIpAddressSetting = settings.FirstOrDefault(x => x.Name == "CustomerSettings.StoreLastIpAddress");
            if (storeLastIpAddressSetting != null) settings.Remove(storeLastIpAddressSetting);

            var displayPrivacyAgreementOnContactUs = settings.FirstOrDefault(x => x.Name == "CustomerSettings.DisplayPrivacyAgreementOnContactUs");
            if (displayPrivacyAgreementOnContactUs != null) settings.Remove(displayPrivacyAgreementOnContactUs);


            var showShareButtonName = TypeHelper.NameOf<CatalogSettings>(y => y.ShowShareButton, true);
            var showShareButtonSetting = context.Set<Setting>().FirstOrDefault(x => x.Name == showShareButtonName);
            if (showShareButtonSetting != null)
            {
                showShareButtonSetting.Value = "False";
            }

            var allowAnonymousUsersToEmailAFriendSetting = context.Set<Setting>().FirstOrDefault(x => x.Name == "CatalogSettings.AllowAnonymousUsersToEmailAFriend");
            if (allowAnonymousUsersToEmailAFriendSetting != null)
            {
                allowAnonymousUsersToEmailAFriendSetting.Value = "False";
            }

            var allowAnonymousUsersToReviewProductSetting = context.Set<Setting>().FirstOrDefault(x => x.Name == "CatalogSettings.AllowAnonymousUsersToReviewProduct");
            if (allowAnonymousUsersToReviewProductSetting != null)
            {
                allowAnonymousUsersToReviewProductSetting.Value = "False";
            }

            var allowAnonymousUsersToEmailWishlistSetting = context.Set<Setting>().FirstOrDefault(x => x.Name == "ShoppingCartSettings.AllowAnonymousUsersToEmailWishlist");
            if (allowAnonymousUsersToEmailWishlistSetting != null)
            {
                allowAnonymousUsersToEmailWishlistSetting.Value = "False";
            }
        }

        public void MigrateLocaleResources(LocaleResourcesBuilder builder)
        {
            builder.AddOrUpdate("Admin.Configuration.Settings.ShoppingCart.ThirdPartyEmailHandOver.Hint",
                "Specifies whether customers can agree to a transferring of their email address to third parties when ordering, and whether the checkbox is enabled by default during checkout. Please note that the 'Show activated' option isn't legally compliant in line with the GDPR.",
                "Legt fest, ob Kunden bei einer Bestellung der Weitergabe ihrer E-Mail Adresse an Dritte zustimmen können und ob die Checkbox dafür standardmäßig aktiviert ist. Bitte beachten Sie, dass die Option 'Aktiviert anzeigen' im Rahmen der DSVGO nicht rechtskonform ist.");

            builder.AddOrUpdate("Admin.Configuration.Settings.CustomerUser.Privacy", "Privacy", "Datenschutz");

            builder.AddOrUpdate("Admin.Configuration.Settings.CustomerUser.Privacy.EnableCookieConsent",
                "Enable cookie consent",
                "Cookie-Hinweis aktivieren",
                "Specifies whether the cookie consent box will be displayed in the frontend.",
                "Legt fest, ob ein Element für die Zustimmung zur Nutzung von Cookies im Frontend angezeigt wird.");

            builder.AddOrUpdate("Admin.Configuration.Settings.CustomerUser.Privacy.CookieConsentBadgetext",
                "Cookie consent display text",
                "Cookie-Hinweistext",
                "Specifies the text, that will be displayed to your customers if they havn't agreed to the usage of cookis yet.",
                "Bestimmt den Text, der Ihren Kunden beim Besuch der Seite angezeigt wird, sofern Sie ihre Zustimmung zur Nutzung von Cookies noch nicht gegeben haben.");

            builder.AddOrUpdate("CookieConsent.BadgeText",
                "{0} is using cookies, to guarantee the best shopping experience. Partially cookies will be set by third parties. <a href='{1}'>Privacy Info</a>",
                "{0} benutzt Cookies, um Ihnen das beste Einkaufserlebnis zu ermöglichen. Zum Teil werden Cookies auch von Drittanbietern gesetzt. <a href='{1}'>Datenschutzerklärung</a>");

            builder.AddOrUpdate("CookieConsent.Button", "Okay, got it", "Ok, verstanden");

            builder.Delete("ContactUs.PrivacyAgreement");

            builder.Delete("Admin.Configuration.Settings.CustomerUser.StoreLastIpAddress");
            builder.Delete("Admin.Configuration.Settings.CustomerUser.StoreLastIpAddress.Hint");
            builder.AddOrUpdate("Admin.Configuration.Settings.CustomerUser.Privacy.StoreLastIpAddress",
                "Store IP address",
                "IP-Adresse speichern",
                "Specifies whether to store the IP address in the customer data set.",
                "Legt fest, ob die IP-Adresse im Kundendatensatz gespeichert werden soll.");

            builder.Delete("Admin.Configuration.Settings.CustomerUser.DisplayPrivacyAgreementOnContactUs");
            builder.Delete("Admin.Configuration.Settings.CustomerUser.DisplayPrivacyAgreementOnContactUs.Hint");
            builder.AddOrUpdate("Admin.Configuration.Settings.CustomerUser.Privacy.DisplayGdprConsentOnForms",
                "Get privacy consent for form submissions",
                "Einwilligungserklärung in Formularen fordern",
                "Specifies whether a checkbox is displayed in forms that prompts the user to agree to the processing of his data.",
                "Bestimmt ob in Formularen eine Checkbox angezeigt wird, die den Benutzer auffordert der Verarbeitung seiner Daten zuzustimmen.");

            builder.AddOrUpdate("Gdpr.Consent.ValidationMessage",
                "Please agree to the processing of your data.",
                "Bitte stimmen Sie der Verarbeitung Ihrer Daten zu.");

            builder.Delete("ContactUs.PrivacyAgreement.MustBeAccepted");
            builder.Delete("ContactUs.PrivacyAgreement.DetailText");
            builder.AddOrUpdate("Gdpr.Consent.DetailText",
                "Yes I've read the <a href=\"{0}\">privacy terms</a> and agree that my data given by me can be stored electronically. My data will thereby only be used to process my inquiry.",
                "Ja, ich habe die <a href=\"{0}\">Datenschutzerklärung</a> zur Kenntnis genommen und bin damit einverstanden, dass die von mir angegebenen Daten elektronisch erhoben und gespeichert werden. Meine Daten werden dabei nur zur Bearbeitung meiner Anfrage genutzt.");

            builder.AddOrUpdate("Gdpr.Anonymous", "Anonymous", "Anonym");
            builder.AddOrUpdate("Gdpr.Anonymize", "Anonymize", "Anonymisieren");
            builder.AddOrUpdate("Gdpr.DeletedText", "Deleted", "Gelöscht");
            builder.AddOrUpdate("Gdpr.DeletedLongText",
                "This content was deleted by the author.",
                "Dieser Inhalt wurde vom Autor gelöscht.");
            builder.AddOrUpdate("Gdpr.Anonymize.Success",
                "The customer record '{0}' has been anonymized.",
                "Der Kundendatensatz '{0}' wurde anonymisiert.");

            builder.AddOrUpdate("Admin.Configuration.Languages.Fields.LastResourcesImportOn",
                "Last import",
                "Letzter Import",
                "The date on which resources were last downloaded and imported.",
                "Das Datum, an dem zuletzt Ressourcen heruntergeladen und importiert worden sind.");

            builder.AddOrUpdate("Admin.Configuration.Settings.CustomerUser.CustomerFormFields", "Registration", "Registrierung");
            builder.AddOrUpdate("Admin.Configuration.Settings.CustomerUser.AddressFormFields", "Addresses", "Adressen");

            builder.AddOrUpdate("Admin.Configuration.Settings.CustomerUser.FirstNameRequired",
                "First name required",
                "Vorname ist erforderlich",
                "Check the box if 'First name' is required.",
                "Legt fest, ob die Angabe des Vornamens erforderlich ist.");
            builder.AddOrUpdate("Admin.Configuration.Settings.CustomerUser.LastNameRequired",
                "Last name required",
                "Nachname ist erforderlich",
                "Check the box if 'Last name' is required.",
                "Legt fest, ob die Angabe des Nachnamens erforderlich ist.");
            builder.AddOrUpdate("Admin.Configuration.Settings.CustomerUser.Privacy.FullNameOnContactUsRequired",
                "Name in the contact form is required",
                "Name im Kontaktformular ist erforderlich",
                "Specifies whether the name is required in the contact form.",
                "Legt fest, ob die Angabe des Namens im Kontaktformular erforderlich ist.");
            builder.AddOrUpdate("Admin.Configuration.Settings.CustomerUser.Privacy.FullNameOnProductRequestRequired",
                "Name in the product request is required",
                "Name im Produktanfrage-Formular ist erforderlich",
                "Specifies whether the name is required in the product request form.",
                "Legt fest, ob die Angabe des Namens im Produktanfrage-Formular erforderlich ist.");

            builder.AddOrUpdate("Checkout.TermsOfService.IAccept",
                "I agree with the {0}terms of service{1} and I adhere to them unconditionally. I've read the {3}privacy terms{1} and agree that my data given by me can be stored electronically.",
                "Ich habe {0}die AGB{1} und {2}das Widerrufsrecht{1} gelesen und bin mit der Geltung einverstanden. Ich habe die {3}Datenschutzerklärung{1} zur Kenntnis genommen und bin damit einverstanden, dass die von mir angegebenen Daten elektronisch erhoben und gespeichert werden.");

            builder.AddOrUpdate("Admin.Customers.Customers.List.SearchDeletedOnly", "Only deactivated customers", "Nur deaktivierte Kunden");

            builder.AddOrUpdate("Admin.Common.Global", "Global", "Global");
            builder.AddOrUpdate("Admin.Common.News", "News", "News");
            builder.AddOrUpdate("Admin.Common.Navigation", "Navigation", "Navigation");
            builder.AddOrUpdate("Admin.Common.PDF", "PDF", "PDF");
            builder.AddOrUpdate("Admin.Common.Footer", "Footer", "Footer");

            builder.AddOrUpdate("Gdpr.Consent.DetailText.Small", "I agree to the <a href=\"{0}\">Privacy policy</a>.", "Mit den Bestimmungen zum <a href=\"{0}\">Datenschutz</a> bin ich einverstanden");

            builder.AddOrUpdate("Account.Fields.Newsletter",
                "I would like to subscribe to the newsletter. I agree to the <a href=\"{0}\" Privacy policy</a>. Unsubscription is possible at any time.",
                "Ich möchte den Newsletter abonnieren. Mit den Bestimmungen zum <a href=\"{0}\">Datenschutz</a> bin ich einverstanden. Eine Abmeldung ist jederzeit möglich.");

            builder.AddOrUpdate("Admin.Configuration.Settings.GeneralCommon.EnableHoneypotProtection",
                "Enable Honeypot protection",
                "Honeypot aktivieren",
                "Honeypot is a simple but reliable bot detection method that does not require any captcha. If active, registration and contact forms are protected against bots and attackers.",
                "Honeypot ist eine simple aber zuverlässige Bot-Erkennungsmethode, die ganz ohne Captcha auskommt. Wenn aktiv, werden Registrierungs- und Kontaktformular vor Bots und Angreifern geschützt.");
        }
    }
}
