namespace SmartStore.Data.Migrations
{
    using System.Data.Entity.Migrations;
    using SmartStore.Core.Domain.Catalog;
    using SmartStore.Core.Domain.Common;
    using SmartStore.Data.Setup;
    using SmartStore.Utilities;

    public partial class V320Resources : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
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
            context.MigrateSettings(x =>
            {
                x.Add(TypeHelper.NameOf<PerformanceSettings>(y => y.CacheSegmentSize, true), 500);
                x.Add(TypeHelper.NameOf<PerformanceSettings>(y => y.AlwaysPrefetchTranslations, true), false);
                x.Add(TypeHelper.NameOf<PerformanceSettings>(y => y.AlwaysPrefetchUrlSlugs, true), false);

                // New CatalogSettings properties
                x.Add(TypeHelper.NameOf<CatalogSettings>(y => y.ShowSubCategoriesInSubPages, true), false);
                x.Add(TypeHelper.NameOf<CatalogSettings>(y => y.ShowDescriptionInSubPages, true), false);
                x.Add(TypeHelper.NameOf<CatalogSettings>(y => y.IncludeFeaturedProductsInSubPages, true), false);
            });
        }

        public void MigrateLocaleResources(LocaleResourcesBuilder builder)
        {
            builder.AddOrUpdate("Admin.ReturnRequests.MaxRefundAmount",
                 "Maximum refund amount",
                 "Maximaler Erstattungsbetrag",
                 "The maximum amount that can be refunded for this return request.",
                 "Der maximale Betrag, der für diesen Rücksendewunsch erstattet werden kann.");

            builder.AddOrUpdate("Admin.Customers.Customers.Fields.Title",
                "Title",
                "Titel",
                "Specifies the title.",
                "Legt den Titel fest.");

            builder.AddOrUpdate("Admin.DataExchange.Export.FolderName.Validate",
                "Please enter a valid, relative folder path for the export data. The path must be at least 3 characters long and not the application folder.",
                "Bitte einen gültigen, relativen Ordnerpfad für die zu exportierenden Daten eingeben. Der Pfad muss mindestens 3 Zeichen lang und nicht der Anwendungsordner sein.");

            builder.AddOrUpdate("Admin.Catalog.Customers.CustomerSearchType", "Search in:", "Suche in:");

            // Fix some FluentValidation german translations
            builder.AddOrUpdate("Validation.LengthValidator")
                .Value("de", "'{PropertyName}' muss zwischen {MinLength} und {MaxLength} Zeichen lang sein. Sie haben {TotalLength} Zeichen eingegeben.");
            builder.AddOrUpdate("Validation.MinimumLengthValidator")
                .Value("de", "'{PropertyName}' muss mind. {MinLength} Zeichen lang sein. Sie haben {TotalLength} Zeichen eingegeben.");
            builder.AddOrUpdate("Validation.MaximumLengthValidator")
                .Value("de", "'{PropertyName}' darf max. {MaxLength} Zeichen lang sein. Sie haben {TotalLength} Zeichen eingegeben.");
            builder.AddOrUpdate("Validation.ExactLengthValidator")
                .Value("de", "'{PropertyName}' muss genau {MaxLength} lang sein. Sie haben {TotalLength} Zeichen eingegeben.");
            builder.AddOrUpdate("Validation.ExclusiveBetweenValidator")
                .Value("de", "'{PropertyName}' muss größer als {From} und kleiner als {To} sein. Sie haben '{Value}' eingegeben.");
            builder.AddOrUpdate("Validation.InclusiveBetweenValidator")
                .Value("de", "'{PropertyName}' muss zwischen {From} and {To} liegen. Sie haben '{Value}' eingegeben.");
            builder.AddOrUpdate("Validation.NotNullValidator")
                .Value("de", "'{PropertyName}' ist erforderlich.");
            builder.AddOrUpdate("Validation.NotEmptyValidator")
                .Value("de", "'{PropertyName}' ist erforderlich.");
            builder.AddOrUpdate("Validation.LessThanValidator")
                .Value("de", "'{PropertyName}' muss kleiner sein als '{ComparisonValue}'.");
            builder.AddOrUpdate("Validation.RegularExpressionValidator")
                .Value("de", "'{PropertyName}' entspricht nicht dem erforderlichen Muster.");
            builder.AddOrUpdate("Validation.ScalePrecisionValidator")
                .Value("de", "'{PropertyName}' darf insgesamt nicht mehr als {expectedPrecision} Ziffern enthalten, unter Berücksichtigung von {expectedScale} Dezimalstellen. {digits} Ziffern und {actualScale} Dezimalstellen wurden gefunden.");

            // Some new resources for custom validators
            builder.AddOrUpdate("Validation.CreditCardCvvNumberValidator",
                "'{PropertyName}' is invalid.",
                "'{PropertyName}' ist ungültig.");

            // Get rid of duplicate validator resource entries
            builder.Delete(
                "Admin.Catalog.Products.Fields.Name.Required",
                "Admin.Catalog.Categories.Fields.Name.Required",
                "Admin.Catalog.Manufacturers.Fields.Name.Required",
                "Admin.Validation.RequiredField",
                "Admin.Catalog.Attributes.ProductAttributes.Fields.Name.Required",
                "Admin.Catalog.ProductReviews.Fields.Title.Required",
                "Admin.Catalog.ProductReviews.Fields.ReviewText.Required",
                "Admin.Catalog.ProductTags.Fields.Name.Required",
                "Admin.Catalog.Products.ProductVariantAttributes.Attributes.Values.Fields.Name.Required",
                "Admin.Catalog.Products.ProductVariantAttributes.Attributes.Values.Fields.Quantity.GreaterOrEqualToOne",
                "Admin.Catalog.Attributes.SpecificationAttributes.Options.Fields.Name.Required",
                "Admin.Catalog.Attributes.SpecificationAttributes.Fields.Name.Required",
                "Admin.ContentManagement.Blog.BlogPosts.Fields.Title.Required",
                "Admin.ContentManagement.Blog.BlogPosts.Fields.Body.Required",
                "Admin.Common.GenericAttributes.Fields.Name.Required",
                "Admin.Customers.CustomerRoles.Fields.Name.Required",
                "Admin.Configuration.Countries.Fields.Name.Required",
                "Admin.Configuration.Countries.Fields.TwoLetterIsoCode.Required",
                "Admin.Configuration.Countries.Fields.TwoLetterIsoCode.Length",
                "Admin.Configuration.Countries.Fields.ThreeLetterIsoCode.Required",
                "Admin.Configuration.Countries.Fields.ThreeLetterIsoCode.Length",
                "Admin.Configuration.Measures.Dimensions.Fields.Name.Required",
                "Admin.Configuration.Measures.Dimensions.Fields.SystemKeyword.Required",
                "Admin.Configuration.Measures.Weights.Fields.Name.Required",
                "Admin.Configuration.Measures.Weights.Fields.SystemKeyword.Required",
                "Admin.Configuration.Countries.States.Fields.Name.Required",
                "Admin.Configuration.DeliveryTimes.Fields.Name.Required",
                "Admin.Configuration.DeliveryTimes.Fields.ColorHexValue.Required",
                "Admin.Configuration.DeliveryTimes.Fields.ColorHexValue.Range",
                "Admin.Configuration.DeliveryTimes.Fields.Name.Range",
                "Admin.Configuration.Currencies.Fields.Name.Required",
                "Admin.Configuration.Currencies.Fields.Name.Range",
                "Admin.Configuration.Currencies.Fields.CurrencyCode.Required",
                "Admin.Configuration.Currencies.Fields.CurrencyCode.Range",
                "Admin.Configuration.Currencies.Fields.Rate.Range",
                "Admin.Configuration.Currencies.Fields.CustomFormatting.Validation",
                "Admin.Promotions.Discounts.Fields.Name.Required",
                "Admin.ContentManagement.Forums.ForumGroup.Fields.Name.Required",
                "Admin.ContentManagement.Forums.Forum.Fields.Name.Required",
                "Admin.ContentManagement.Forums.Forum.Fields.ForumGroupId.Required",
                "Admin.Configuration.Languages.Resources.Fields.Name.Required",
                "Admin.Configuration.Languages.Resources.Fields.Value.Required",
                "Admin.Configuration.Languages.Fields.Name.Required",
                "Admin.Configuration.Languages.Fields.UniqueSeoCode.Required",
                "Admin.Configuration.Languages.Fields.UniqueSeoCode.Length",
                "Admin.Promotions.Campaigns.Fields.Name.Required",
                "Admin.Promotions.Campaigns.Fields.Subject.Required",
                "Admin.Promotions.Campaigns.Fields.Body.Required",
                "Admin.ContentManagement.MessageTemplates.Fields.Subject.Required",
                "Admin.ContentManagement.MessageTemplates.Fields.Body.Required",
                "Admin.Promotions.NewsLetterSubscriptions.Fields.Email.Required",
                "Admin.System.QueuedEmails.Fields.Priority.Required",
                "Admin.System.QueuedEmails.Fields.From.Required",
                "Admin.System.QueuedEmails.Fields.To.Required",
                "Admin.System.QueuedEmails.Fields.SentTries.Required",
                "Admin.System.QueuedEmails.Fields.Priority.Range",
                "Admin.System.QueuedEmails.Fields.SentTries.Range",
                "Admin.ContentManagement.News.NewsItems.Fields.Title.Required",
                "Admin.ContentManagement.News.NewsItems.Fields.Short.Required",
                "Admin.ContentManagement.News.NewsItems.Fields.Full.Required",
                "Admin.Catalog.Attributes.CheckoutAttributes.Fields.Name.Required",
                "Admin.Catalog.Attributes.CheckoutAttributes.Values.Fields.Name.Required",
                "Admin.Configuration.Plugins.Fields.FriendlyName.Required",
                "Admin.ContentManagement.Polls.Answers.Fields.Name.Required",
                "Admin.ContentManagement.Polls.Fields.Name.Required",
                "Admin.Configuration.Shipping.Methods.Fields.Name.Required",
                "Admin.Configuration.Stores.Fields.Name.Required",
                "Admin.Configuration.Stores.Fields.Url.Required",
                "Admin.Configuration.Settings.AllSettings.Fields.Name.Required",
                "Admin.System.ScheduleTasks.Name.Required",
                "Admin.Configuration.Tax.Categories.Fields.Name.Required",
                "Admin.ContentManagement.Topics.Fields.SystemName.Required",
                "Admin.Address.Fields.FirstName.Required",
                "Admin.Address.Fields.LastName.Required",
                "Admin.Address.Fields.Email.Required",
                "Admin.Address.Fields.Company.Required",
                "Admin.Address.Fields.City.Required",
                "Admin.Address.Fields.Address1.Required",
                "Admin.Address.Fields.Address2.Required",
                "Admin.Address.Fields.ZipPostalCode.Required",
                "Admin.Address.Fields.PhoneNumber.Required",
                "Admin.Address.Fields.FaxNumber.Required",
                "Admin.Address.Fields.EmailMatch.Required",
                "Admin.Customers.Customers.Fields.FirstName.Required",
                "Admin.Customers.Customers.Fields.LastName.Required",
                "Admin.Customers.Customers.Fields.Company.Required",
                "Admin.Customers.Customers.Fields.StreetAddress.Required",
                "Admin.Customers.Customers.Fields.StreetAddress2.Required",
                "Admin.Customers.Customers.Fields.ZipPostalCode.Required",
                "Admin.Customers.Customers.Fields.City.Required",
                "Admin.Customers.Customers.Fields.Phone.Required",
                "Admin.Customers.Customers.Fields.Fax.Required",
                "Admin.Validation.Name",
                "Admin.Validation.EmailAddress",
                "Admin.Validation.UsernamePassword",
                "Admin.DataExchange.Export.FileNamePattern.Validate",
                "Admin.DataExchange.Export.Partition.Validate",
                "Admin.Common.WrongEmail",
                "Payment.CardCode.Wrong"
            );

            // Get rid of duplicate CreatedOn resources also
            builder.Delete(
                "Admin.Affiliates.Orders.CreatedOn",
                "Admin.ContentManagement.Blog.Comments.Fields.CreatedOn",
                "Admin.ContentManagement.Blog.BlogPosts.Fields.CreatedOn",
                "Admin.ContentManagement.Blog.BlogPosts.Fields.CreatedOn",
                "Admin.Catalog.ProductReviews.Fields.CreatedOn",
                "Admin.Customers.Customers.Fields.CreatedOn",
                "Admin.Customers.Customers.Orders.CreatedOn",
                "Admin.Customers.Customers.ActivityLog.CreatedOn",
                "Admin.Orders.Fields.CreatedOn",
                "Admin.Customers.Customers.Fields.CreatedOn",
                "Admin.Promotions.NewsLetterSubscriptions.Fields.CreatedOn",
                "Admin.Configuration.Currencies.Fields.CreatedOn",
                "Admin.Promotions.Discounts.History.CreatedOn",
                "Admin.ContentManagement.Forums.ForumGroup.Fields.CreatedOn",
                "Admin.ContentManagement.Forums.Forum.Fields.CreatedOn",
                "Admin.Configuration.ActivityLog.ActivityLog.Fields.CreatedOn",
                "Admin.System.Log.Fields.CreatedOn",
                "Admin.Promotions.Campaigns.Fields.CreatedOn",
                "Admin.Promotions.NewsLetterSubscriptions.Fields.CreatedOn",
                "Admin.System.QueuedEmails.Fields.CreatedOn",
                "Admin.ContentManagement.News.Comments.Fields.CreatedOn",
                "Admin.ContentManagement.News.NewsItems.Fields.CreatedOn",
                "Admin.GiftCards.Fields.CreatedOn",
                "Admin.GiftCards.History.CreatedOn",
                "Admin.Orders.Fields.CreatedOn",
                "Admin.Orders.OrderNotes.Fields.CreatedOn",
                "Admin.RecurringPayments.History.CreatedOn",
                "Admin.ReturnRequests.Fields.CreatedOn"
            );

            // duplicate validator resource entries in frontend
            builder.Delete(
                "Blog.Comments.CommentText.Required",
                "Forum.TextCannotBeEmpty",
                "Forum.TopicSubjectCannotBeEmpty",
                "Forum.TextCannotBeEmpty",
                "Account.Fields.Email.Required",
                "Products.AskQuestion.Question.Required",
                "Account.Fields.FullName.Required",
                "Products.EmailAFriend.FriendEmail.Required",
                "Products.EmailAFriend.YourEmailAddress.Required",
                "Reviews.Fields.Title.Required",
                "Reviews.Fields.Title.MaxLengthValidation",
                "Reviews.Fields.ReviewText.Required",
                "Address.Fields.FirstName.Required",
                "Address.Fields.LastName.Required",
                "Address.Fields.Email.Required",
                "Account.Fields.Company.Required",
                "Account.Fields.StreetAddress.Required",
                "Account.Fields.StreetAddress2.Required",
                "Account.Fields.ZipPostalCode.Required",
                "Account.Fields.City.Required",
                "Account.Fields.Phone.Required",
                "Account.Fields.Fax.Required",
                "Admin.Address.Fields.EmailMatch.Required",
                "ContactUs.Email.Required",
                "ContactUs.Enquiry.Required",
                "ContactUs.FullName.Required",
                "Account.ChangePassword.Fields.OldPassword.Required",
                "Account.ChangePassword.Fields.NewPassword.Required",
                "Account.ChangePassword.Fields.NewPassword.LengthValidation",
                "Account.ChangePassword.Fields.ConfirmNewPassword.Required",
                "Account.ChangePassword.Fields.NewPassword.LengthValidation",
                "Account.Fields.Email.Required",
                "Account.Fields.FirstName.Required",
                "Account.Fields.LastName.Required",
                "Account.Fields.Company.Required",
                "Account.Fields.StreetAddress.Required",
                "Account.Fields.StreetAddress2.Required",
                "Account.Fields.ZipPostalCode.Required",
                "Account.Fields.City.Required",
                "Account.Fields.Phone.Required",
                "Account.Fields.Fax.Required",
                "Account.Fields.Password.Required",
                "Account.Fields.Vat.Required",
                "Account.PasswordRecovery.NewPassword.Required",
                "Account.PasswordRecovery.NewPassword.LengthValidation",
                "Account.PasswordRecovery.ConfirmNewPassword.Required",
                "Account.PasswordRecovery.Email.Required",
                "News.Comments.CommentTitle.Required",
                "News.Comments.CommentTitle.MaxLengthValidation",
                "News.Comments.CommentText.Required",
                "PrivateMessages.SubjectCannotBeEmpty",
                "PrivateMessages.MessageCannotBeEmpty",
                "Wishlist.EmailAFriend.FriendEmail.Required",
                "Wishlist.EmailAFriend.YourEmailAddress.Required"
            );

            // remove duplicate resources for display order
            builder.Delete(
                "Admin.Catalog.Categories.Fields.DisplayOrder",
                "Admin.Catalog.Categories.Products.Fields.DisplayOrder",
                "Admin.Catalog.Manufacturers.Fields.DisplayOrder",
                "Admin.Catalog.Manufacturers.Products.Fields.DisplayOrder",
                "Admin.Catalog.Products.ProductVariantAttributes.Attributes.Values.Fields.DisplayOrder",
                "Admin.Catalog.Products.BundleItems.Fields.DisplayOrder",
                "Admin.Catalog.Products.Fields.HomePageDisplayOrder",
                "Admin.Catalog.Products.SpecificationAttributes.Fields.DisplayOrder",
                "Admin.Catalog.Products.Pictures.Fields.DisplayOrder",
                "Admin.Catalog.Products.Categories.Fields.DisplayOrder",
                "Admin.Catalog.Products.Manufacturers.Fields.DisplayOrder",
                "Admin.Catalog.Products.RelatedProducts.Fields.DisplayOrder",
                "Admin.Catalog.Products.AssociatedProducts.Fields.DisplayOrder",
                "Admin.Catalog.Products.BundleItems.Fields.DisplayOrder",
                "Admin.Catalog.Products.ProductVariantAttributes.Attributes.Fields.DisplayOrder",
                "Admin.Catalog.Products.ProductVariantAttributes.Attributes.Values.Fields.DisplayOrder",
                "Admin.Catalog.Products.SpecificationAttributes.Fields.DisplayOrder",
                "Admin.Catalog.Attributes.SpecificationAttributes.Fields.DisplayOrder",
                "Admin.Catalog.Attributes.SpecificationAttributes.Options.Fields.DisplayOrder",
                "Admin.Catalog.Categories.Fields.DisplayOrder",
                "Admin.Catalog.Manufacturers.Fields.DisplayOrder",
                "Admin.Configuration.Countries.Fields.DisplayOrder",
                "Admin.Configuration.Currencies.Fields.DisplayOrder",
                "Admin.Configuration.DeliveryTimes.Fields.DisplayOrder",
                "Admin.Configuration.Measures.Dimensions.Fields.DisplayOrder",
                "Admin.Configuration.Measures.Weights.Fields.DisplayOrder",
                "Admin.Configuration.Countries.States.Fields.DisplayOrder",
                "Admin.ContentManagement.Forums.ForumGroup.Fields.DisplayOrder",
                "Admin.ContentManagement.Forums.Forum.Fields.DisplayOrder",
                "Admin.Configuration.Languages.Fields.DisplayOrder",
                "Admin.Catalog.Attributes.CheckoutAttributes.Fields.DisplayOrder",
                "Admin.Catalog.Attributes.CheckoutAttributes.Values.Fields.DisplayOrder",
                "Admin.Configuration.Plugins.Fields.DisplayOrder",
                "Admin.ContentManagement.Polls.Answers.Fields.DisplayOrder",
                "Admin.ContentManagement.Polls.Fields.DisplayOrder",
                "Admin.Configuration.Shipping.Methods.Fields.DisplayOrder",
                "Admin.Configuration.Stores.Fields.DisplayOrder",
                "Admin.Configuration.Tax.Categories.Fields.DisplayOrder"
            );

            builder.AddOrUpdate("Common.DisplayOrder.Hint",
                "Specifies display order. 1 represents the top of the list.",
                "Legt die Anzeige-Priorität fest. 1 steht bspw. für das erste Element in der Liste.");

            builder.AddOrUpdate("Admin.Configuration.Settings.GeneralCommon.UseInvisibleReCaptcha",
                "Use invisible reCAPTCHA",
                "Unsichtbaren reCAPTCHA verwenden",
                "Does not require the user to click on a checkbox, instead it is invoked directly when the user submits a form. By default only the most suspicious traffic will be prompted to solve a captcha.",
                "Der Benutzer muss nicht auf ein Kontrollkästchen klicken, sondern die Validierung erfolgt direkt beim Absenden eines Formulars. Nur bei 'verdächtigem' Traffic wird der Benutzer aufgefordert, ein Captcha zu lösen.");

            builder.AddOrUpdate("Admin.ContentManagement.Topics.Fields.ShortTitle",
                "Short title",
                "Kurztitel",
                "Optional. Used as link text. If empty, 'Title' sets the link text.",
                "Optional. Wird u.A. als Linktext verwendet. Wenn leer, stellt 'Titel' den Linktext.");

            builder.AddOrUpdate("Admin.ContentManagement.Topics.Fields.Intro",
                "Intro",
                "Intro",
                "Optional. Short introduction / teaser.",
                "Optional. Einleitung / Teaser.");

            builder.AddOrUpdate("Common.Download.Versions", "Versions", "Versionen");
            builder.AddOrUpdate("Common.Download.Version", "Version", "Version");
            builder.AddOrUpdate("Common.Download.Delete", "Delete download", "Download löschen");
            builder.AddOrUpdate("Common.Downloads", "Downloads", "Downloads");

            builder.AddOrUpdate("Admin.Catalog.Products.Fields.NewVersionDownloadId",
                "New download version",
                "Neue Version des Downloads",
                "Upload a new version of the download file here.",
                "Laden Sie hier eine neue Version der Download-Datei hoch.");

            builder.AddOrUpdate("Admin.Catalog.Products.Download.VersionDelete", "Delete this file version.", "Diese Dateiversion löschen.");
            builder.AddOrUpdate("Admin.Catalog.Products.Download.AddChangelog", "Edit changelog", "Änderungshistorie bearbeiten");
            builder.AddOrUpdate("Customer.Downloads.NoChangelogAvailable", "No changelog available.", "Keine Änderungshistorie verfügbar.");

            builder.AddOrUpdate("Admin.Catalog.Products.Download.SemanticVersion.NotValid",
                "The specified version information is not valid. Please enter the version number in the correct format (e.g.: 1.0.0.0, 2.0 or 3.1.5).",
                "Die angegebenen Versionsinformationen sind nicht gültig. Bitte geben Sie die Versionsnummer in korrektem Format an (z.B.: 1.0.0.0, 2.0 oder 3.1.5).");

            builder.AddOrUpdate("Admin.Catalog.Products.Fields.HasPreviewPicture",
                "Exclude first image from gallery",
                "Erstes Bild aus Gallerie ausschließen",
                "Activate this option if the first image should be displayed as a preview in product lists but not in the product detail gallery.",
                "Aktivieren Sie diese Option, wenn das erste Bild als Vorschau in Produktlisten, nicht aber in der Produktdetail-Gallerie angezeigt werden soll.");

            builder.AddOrUpdate("Products.Free", "Free", "Kostenlos");

            builder.AddOrUpdate("Admin.Catalog.Products.Fields.ProductTags.Hint",
                "Product tags are keywords that this product can also be identified by. Enter a list of the tags to be associated with this product. The more products associated with a particular tag, the larger it will show on the tag cloud.",
                "Eine Liste von Schlüsselwörtern, die das Produkt taxonomisch charakterisieren. Je mehr Produkte einem Schlüsselwort (Tag) zugeordnet sind, desto mehr visuelles Gewicht erhält das Tag.");

            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Forums.ForumTopicSorting.Initial", "Position", "Position");
            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Forums.ForumTopicSorting.Relevance", "Relevance", "Beste Ergebnisse");
            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Forums.ForumTopicSorting.SubjectAsc", "Title: A to Z", "Titel: A bis Z");
            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Forums.ForumTopicSorting.SubjectDesc", "Title: Z to A", "Titel: Z bis A");
            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Forums.ForumTopicSorting.UserNameAsc", "User name: A to Z", "Benutzername: A bis Z");
            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Forums.ForumTopicSorting.UserNameDesc", "User name: Z to A", "Benutzername: Z bis A");
            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Forums.ForumTopicSorting.CreatedOnAsc", "Created on: Oldest first", "Erstellt am: ältere zuerst");
            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Forums.ForumTopicSorting.CreatedOnDesc", "Created on: Newest first", "Erstellt am: neuere zuerst");
            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Forums.ForumTopicSorting.PostsAsc", "Post number: ascending", "Anzahl Beiträge: aufsteigend");
            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Forums.ForumTopicSorting.PostsDesc", "Post number: descending", "Anzahl Beiträge: absteigend");

            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Forums.ForumDateFilter.LastVisit", "Since last visit", "Seit dem letzten Besuch");
            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Forums.ForumDateFilter.Yesterday", "Yesterday", "Gestern");
            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Forums.ForumDateFilter.LastWeek", "Last week", "In der letzten Woche");
            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Forums.ForumDateFilter.LastTwoWeeks", "Last 2 weeks", "In den letzten 2 Wochen");
            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Forums.ForumDateFilter.LastMonth", "Last month", "Im letzten Monat");
            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Forums.ForumDateFilter.LastThreeMonths", "Last 3 months", "In den letzten 3 Monaten");
            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Forums.ForumDateFilter.LastSixMonths", "Last 6 months", "In den letzten 6 Monaten");
            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Forums.ForumDateFilter.LastYear", "Last year", "Im letzten Jahr");

            builder.AddOrUpdate("Search.Facet.Forum", "Forum", "Forum");
            builder.AddOrUpdate("Search.Facet.Customer", "User name", "Benutzername");
            builder.AddOrUpdate("Search.Facet.Date", "Period", "Zeitraum");
            builder.AddOrUpdate("Search.Facet.Date.Newer", "and newer", "und neuer");
            builder.AddOrUpdate("Search.Facet.Date.Older", "and older", "und älter");

            builder.AddOrUpdate("Forum.PostText", "Post text", "Beitragstext");
            builder.AddOrUpdate("Forum.Sticky", "Sticky topic", "Festes Thema");

            builder.AddOrUpdate("Search.HitsFor", "{0} hits for {1}", "{0} Treffer für {1}");
            builder.AddOrUpdate("Search.NoMoreHitsFound", "There were no more hits found.", "Es wurden keine weiteren Treffer gefunden.");

            builder.AddOrUpdate("Admin.Configuration.Settings.Search.WildcardSearchNote",
                "The wildcard mode can slow down the search for a large number of objects.",
                "Der Wildcard-Modus kann bei einer großen Anzahl an Objekten die Suche verlangsamen.");

            builder.AddOrUpdate("Admin.Configuration.Settings.Search.SearchMode",
                "Search mode",
                "Suchmodus",
                "Specifies the search mode. Please keep in mind that the search mode can - depending on number of objects - strongly affect search performance. 'Is equal to' is the fastest, 'Contains' the slowest.",
                "Legt den Suchmodus fest. Bitte beachten Sie, dass der Suchmodus die Geschwindigkeit der Suche (abhängig von der Objektanzahl) beeinflusst. 'Ist gleich' ist am schnellsten, 'Beinhaltet' am langsamsten.");

            builder.AddOrUpdate("Admin.Configuration.Settings.Search.Forum.SearchFields",
                "Search fields",
                "Suchfelder",
                "Specifies additional search fields. The topic title is always searched.",
                "Legt zusätzlich zu durchsuchende Felder fest. Der Thementitel wird grundsätzlich immer durchsucht.");

            builder.AddOrUpdate("Admin.Configuration.Settings.Search.DefaultSortOrder",
                "Default sort order",
                "Standardsortierreihenfolge",
                "Specifies the default sort order in search results.",
                "Legt die Standardsortierreihenfolge in den Suchergebnissen fest.");

            builder.AddOrUpdate("Admin.Configuration.Settings.Search.InstantSearchNumberOfHits",
                "Number of hits",
                "Anzahl der Treffer",
                "Specifies the number of hits displayed in instant search.",
                "Legt die Anzahl der angezeigten Suchtreffer in der Instantsuche fest.");

            builder.AddOrUpdate("Admin.Configuration.Settings.Forums.AllowSorting",
                "Allow sorting",
                "Sortierung zulassen",
                "Specifies whether forum posts can be sorted.",
                "Legt fest, ob Forenbeiträge sortiert werden können.");

            builder.AddOrUpdate("Admin.Common.DefaultPageSizeOptions",
                "Page size options",
                "Auswahlmöglichkeiten für Seitengröße",
                "Comma-separated page size options that a customer can select in lists.",
                "Kommagetrennte Liste mit Optionen für Seitengröße, die ein Kunde in Listen wählen kann.");

            builder.AddOrUpdate("Admin.Common.AllowCustomersToSelectPageSize",
                "Allow customers to select page size",
                "Kunde kann Listengröße ändern",
                "Whether customers are allowed to select the page size from a predefined list of options.",
                "Kunden können die Listengröße mit Hilfe einer vorgegebenen Optionsliste ändern.");


            builder.Delete(
                "Admin.Configuration.Settings.Search.DefaultSortOrderMode",
                "Admin.Configuration.Settings.Search.InstantSearchNumberOfProducts",
                "Admin.Configuration.Settings.CustomerUser.DefaultAvatarEnabled",
                "Forum.Search.LimitResultsToPrevious.AllResults",
                "Forum.Search.LimitResultsToPrevious.1day",
                "Forum.Search.LimitResultsToPrevious.7days",
                "Forum.Search.LimitResultsToPrevious.2weeks",
                "Forum.Search.LimitResultsToPrevious.1month",
                "Forum.Search.LimitResultsToPrevious.3months",
                "Forum.Search.LimitResultsToPrevious.6months",
                "Forum.Search.LimitResultsToPrevious.1year",
                "Forum.Search.SearchInForum.All",
                "Forum.Search.SearchWithin.All",
                "Forum.Search.SearchWithin.TopicTitlesOnly",
                "Forum.Search.SearchWithin.PostTextOnly",
                "Forum.SearchTermMinimumLengthIsNCharacters",
                "Enums.SmartStore.Core.Domain.Forums.ForumSearchType.All",
                "Enums.SmartStore.Core.Domain.Forums.ForumSearchType.PostTextOnly",
                "Enums.SmartStore.Core.Domain.Forums.ForumSearchType.TopicTitlesOnly",
                "Forum.AdvancedSearch",
                "Forum.SearchButton",
                "Forum.PageTitle.Search");

            builder.AddOrUpdate("Admin.Configuration.Settings.Catalog.PriceDisplayStyle",
                "Price display style",
                "Preisdarstellung",
                "Specifies the form in which prices are displayed in product lists and on the product detail page.",
                "Bestimmt die Darstellungform von Preisen in Produktlisten und auf der Produktdetailseite.");

            builder.AddOrUpdate("Admin.Configuration.Settings.Catalog.DisplayTextForZeroPrices",
                "Display text when prices are 0,00",
                "Zeige Text wenn Preise 0,00 sind",
                "Specifies whether to display a textual resource (free) instead of the value 0.00.",
                "Bestimmt, ob statt dem Wert 0,00 eine textuelle Resource (kostenlos) angezeigt werden soll.");

            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Catalog.PriceDisplayStyle.Default", "Default", "Standard");
            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Catalog.PriceDisplayStyle.BadgeAll", "In bagdes", "Markiert");
            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Catalog.PriceDisplayStyle.BadgeFreeProductsOnly", "Badge free products only", "Nur kostenlose Produkte markieren");

            builder.AddOrUpdate("Admin.DataExchange.Export.Filter.WorkingLanguageId",
                "Language",
                "Sprache",
                "Filter by language",
                "Nach Sprache filtern");


            builder.AddOrUpdate("Admin.Configuration.Settings.GeneralCommon.CaptchaShowOnForumPage",
                "Show on forum pages",
                "Auf Forenseiten anzeigen",
                "Specifies whether to display a CAPTCHA on forum pages when creating or replying to a topic.",
                "Legt fest, ob ein CAPTCHA auf Forenseiten angezeigt werden soll, wenn ein Thema erstellt oder darauf geantwortet wird.");

            builder.AddOrUpdate("Admin.Catalog.Products.BundleItems.NoProductLinkageForBundleItem",
                "The product \"{0}\" cannot be assigned an attribute of the type \"product\" because it is bundle item of a product bundle.",
                "Dem Produkt \"{0}\" kann kein Attribut vom Typ \"Produkt\" zugeordnet werden, weil es auf der Stückliste eines Produkt-Bundle steht.");

            builder.AddOrUpdate("Search.RelatedSearchTerms",
                "Related search terms",
                "Verwandte Suchbegriffe");

            builder.AddOrUpdate("Plugins.CannotLoadModule",
                "The plugin or provider \"{0}\" cannot be loaded.",
                "Das Plugin oder der Provider \"{0}\" kann nicht geladen werden.");

            builder.AddOrUpdate("Admin.System.ScheduleTasks.RunPerMachine",
                "Run per machine",
                "Pro Maschine ausführen",
                "Indicates whether the task is executed decidedly on each machine of a web farm.",
                "Gibt an, ob die Aufgabe auf jeder Maschine einer Webfarm dezidiert ausgeführt wird.");

            builder.Delete("Address.Fields.Required.Hint");

            builder.AddOrUpdate("Common.FormFields.Required.Hint",
                "* Input elements with asterisk are required and have to be filled out.",
                "* Eingabefelder mit Sternchen sind Pflichfelder und müssen ausgefüllt werden.");

            builder.AddOrUpdate("Forum.Post.Vote.OnlyRegistered",
                "Only registered users can vote for posts.",
                "Nur registrierte Benutzer können Beiträge bewerten.");

            builder.AddOrUpdate("Forum.Post.Vote.OwnPostNotAllowed",
                "You cannot vote for your own post.",
                "Sie können nicht Ihren eigenen Beitrag bewerten.");

            builder.AddOrUpdate("Forum.Post.Vote.SuccessfullyVoted",
                "Thank you for your vote.",
                "Danke für Ihre Bewertung.");

            builder.AddOrUpdate("Common.Liked", "Liked", "Gefällt");
            builder.AddOrUpdate("Common.LikeIt", "I like it", "Gefällt mir");
            builder.AddOrUpdate("Common.DoNotLikeIt", "I do not like it anymore", "Gefällt mir nicht mehr");

            builder.AddOrUpdate("Admin.Configuration.Settings.Forums.AllowCustomersToVoteOnPosts",
                "Allow customers to vote on posts",
                "Benutzer können Beiträge bewerten",
                "Specifies whether customers can vote on posts.",
                "Legt fest, ob Benutzer Beiträge bewerten können.");

            builder.AddOrUpdate("Admin.Configuration.Settings.Forums.AllowGuestsToVoteOnPosts",
                "Allow guests to vote on posts",
                "Gäste können Beiträge bewerten",
                "Specifies whether guests can vote on posts.",
                "Legt fest, ob Gäste Beiträge bewerten können.");

            // Typos.
            builder.AddOrUpdate("Admin.Promotions.Discounts.Requirements")
                .Value("de", "Voraussetzungen");
            builder.AddOrUpdate("Admin.Promotions.Discounts.Requirements.DiscountRequirementType")
                .Value("de", "Typ der Voraussetzung");
            builder.AddOrUpdate("Admin.Promotions.Discounts.Requirements.DiscountRequirementType.Hint")
                .Value("de", "Voraussetzungen für den Rabatt");
            builder.AddOrUpdate("Admin.Promotions.Discounts.Requirements.Remove")
                .Value("de", "Voraussetzung für den Rabatt entfernen");
            builder.AddOrUpdate("Admin.Promotions.Discounts.Requirements.SaveBeforeEdit")
                .Value("de", "Sie müssen den Rabatt zunächst speichern, bevor Sie Voraussetzungen für seine Anwendung festlegen können");

            builder.AddOrUpdate("Common.Voting", "Voting", "Abstimmung");
            builder.AddOrUpdate("Common.Answer", "Answer", "Antwort");
            builder.AddOrUpdate("Common.Size", "Size", "Größe");

            builder.AddOrUpdate("Admin.Configuration.Settings.CustomerUser.CustomerFormFields.Description",
                "Manage form fields that are displayed during registration.",
                "Verwalten Sie Formularfelder, die während der Registrierung angezeigt werden.");

            builder.AddOrUpdate("Admin.Configuration.Settings.CustomerUser.AddressFormFields.Description",
                "Manage form fields that are displayed during checkout and on \"My account\" page.",
                "Verwalten Sie Formularfelder, die während des Checkout-Prozesses und im \"Mein Konto\" Bereich angezeigt werden.");

            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.DataExchange.RelatedEntityType.TierPrice", "Tier price", "Staffelpreis");
            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.DataExchange.RelatedEntityType.ProductVariantAttributeValue", "Attribute option", "Attribut-Option");
            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.DataExchange.RelatedEntityType.ProductVariantAttributeCombination", "Attribute combination", "Attribut-Kombination");

            builder.AddOrUpdate("Admin.DataExchange.Export.ExportRelatedData.Validate",
                "Related data cannot be exported if the option \"Export attribute combinations\" is activated.",
                "Zugehörige Daten können nicht exportiert werden, wenn die Option \"Attributkombinationen exportieren\" aktiviert ist.");

            builder.AddOrUpdate("Admin.Common.ProcessingInfo",
                "{0}: {1} of {2} processed",
                "{0}: {1} von {2} verarbeitet");

            builder.AddOrUpdate("Admin.Configuration.Settings.Catalog.ShowSubCategoriesInSubPages",
                "Show subcategories also in subpages",
                "Unterwarengruppen auch in Unterseiten anzeigen",
                "Subpage: List index greater than 1 or any active filter.",
                "Unterseite: Listenindex größer 1 oder mind. ein aktiver Filter.");

            builder.AddOrUpdate("Admin.Configuration.Settings.Catalog.ShowDescriptionInSubPages",
                "Show page description also in subpages",
                "Seitenbeschreibungen auch in Unterseiten anzeigen",
                "Subpage: List index greater than 1 or any active filter.",
                "Unterseite: Listenindex größer 1 oder mind. ein aktiver Filter.");

            builder.AddOrUpdate("Admin.Configuration.Settings.Catalog.IncludeFeaturedProductsInSubPages",
                "Show featured products also in subpages",
                "Top-Produkte auch in Unterseiten anzeigen",
                "Subpage: List index greater than 1 or any active filter.",
                "Unterseite: Listenindex größer 1 oder mind. ein aktiver Filter.");

            builder.AddOrUpdate("Admin.Common.CopyOf", "Copy of {0}", "Kopie von {0}");

            builder.AddOrUpdate("Admin.Configuration.Languages.DefaultLanguage.Note",
                "The default language of the shop is <b class=\"font-weight-medium\">{0}</b>. The default is always the first published language.",
                "Die Standardsprache des Shops ist <b class=\"font-weight-medium\">{0}</b>. Standard ist stets die erste veröffentlichte Sprache.");

            builder.AddOrUpdate("Admin.Configuration.Languages.AvailableLanguages.Note",
                "Click <b class=\"font-weight-medium\">Download</b> to install a new language including all localized resources. On <a class=\"font-weight-medium\" href=\"https://translate.smartstore.com/\" target=\"_blank\">translate.smartstore.com</a> you will find more details about available resources.",
                "Klicken Sie auf <b class=\"font-weight-medium\">Download</b>, um eine neue Sprache mit allen lokalisierten Ressourcen zu installieren. Auf <a class=\"font-weight-medium\" href=\"https://translate.smartstore.com/\" target=\"_blank\">translate.smartstore.com</a> finden Sie weitere Details zu verfügbaren Ressourcen.");

            builder.AddOrUpdate("Common.BrowseFiles", "Browse", "Durchsuchen");
            builder.AddOrUpdate("Common.Url", "URL", "URL");
            builder.AddOrUpdate("Common.File", "File", "Datei");
            builder.AddOrUpdate("Common.Entity.Product", "Product", "Produkt");
            builder.AddOrUpdate("Common.Entity.Category", "Category", "Warengruppe");
            builder.AddOrUpdate("Common.Entity.Manufacturer", "Manufacturer", "Hersteller");
            builder.AddOrUpdate("Common.Entity.Topic", "Topic", "Seite");

            builder.AddOrUpdate("Common.Entity.SelectProduct", "Select product", "Produkt auswählen");
            builder.AddOrUpdate("Common.Entity.SelectCategory", "Select category", "Warengruppe auswählen");
            builder.AddOrUpdate("Common.Entity.SelectManufacturer", "Select manufacturer", "Hersteller auswählen");
            builder.AddOrUpdate("Common.Entity.SelectTopic", "Select topic", "Seite auswählen");

            builder.Delete("Admin.Customers.Customers.List.SearchDeletedOnly");
            builder.AddOrUpdate("Admin.Customers.Customers.List.SearchActiveOnly", "Only activated customers", "Nur aktivierte Kunden");

            builder.AddOrUpdate("Products.LoginForPrice",
                "Prices will be displayed after login.",
                "Preise werden nach Anmeldung angezeigt.");

            builder.AddOrUpdate("Admin.Configuration.Settings.Catalog.ShowLoginForPriceNote",
                "Show login for price note",
                "Hinweis \"Preis nach Anmeldung\" anzeigen",
                "Specifies whether to display a message stating that prices will not be displayed until login.",
                "Legt fest, ob ein Hinweis erscheinen soll, dass Preise erst nach Anmeldung angezeigt werden.");

            builder.AddOrUpdate("Products.EmailAFriend.LoginNote",
                "Please log in to use this function. <a href='{0}'>Login now</a>",
                "Bitte melden Sie sich an, um diese Funktion nutzen zu können. <a href='{0}'>Jetzt anmelden</a>");

            builder.AddOrUpdate("Account.Login.Fields.UsernameOrEmail",
                "Username or email",
                "Benutzername oder Email");

            builder.AddOrUpdate("Admin.Configuration.Settings.CustomerUser.CustomerLoginType",
                "Customer login type",
                "Art des Kundenlogins",
                "Specifies the customer login type.",
                "Legt die Art des Kundenlogins fest.");

            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Customers.CustomerLoginType.Username", "Username", "Benutzername");
            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Customers.CustomerLoginType.Email", "Email", "Email");
            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Customers.CustomerLoginType.UsernameOrEmail", "Username or email", "Benutzername oder Email");

            builder.AddOrUpdate("Admin.ContentManagement.Topics.Fields.HtmlId",
                "Html Id",
                "Html-ID",
                "Specifies the Html Id of the page.",
                "Legt die Html-ID der Seite fest.");

            builder.AddOrUpdate("Admin.ContentManagement.Topics.Fields.BodyCssClass",
                "Body CSS class",
                "Body CSS-Klasse",
                "Specifies the CSS class of the body element.",
                "Legt die CSS-Klasse des Body-Elements fest.");

            builder.AddOrUpdate("Admin.ContentManagement.Topics.Validation.NoWhiteSpace",
                "Whitespace isn't allowed.",
                "Leerzeichen sind nicht erlaubt.");

            builder.AddOrUpdate("Admin.Configuration.Settings.Tax.VatRequired",
                "Customers must enter a VAT number",
                "Kunden müssen eine Steuernummer angeben",
                "Specifies whether customers must enter a VAT identification number.",
                "Legt fest, ob Kunden bei der Registrierung eine Steuernummer angeben müssen.");

            builder.AddOrUpdate("Common.Top", "Top", "Oben");
            builder.AddOrUpdate("Common.Bottom", "Bottom", "Unten");
            builder.AddOrUpdate("Common.Left", "Left", "Links");
            builder.AddOrUpdate("Common.Right", "Right", "Rechts");
            builder.AddOrUpdate("Common.TopLeft", "Top left", "Links oben");
            builder.AddOrUpdate("Common.TopRight", "Top right", "Rechts oben");
            builder.AddOrUpdate("Common.BottomLeft", "Bottom left", "Links unten");
            builder.AddOrUpdate("Common.BottomRight", "Bottom right", "Rechts unten");
            builder.AddOrUpdate("Common.Center", "Center", "Mitte");
            builder.AddOrUpdate("Common.NoTitle", "No title", "Ohne Titel");
            builder.AddOrUpdate("Common.MoveUp", "Move up", "Nach oben");
            builder.AddOrUpdate("Common.MoveDown", "Move down", "Nach unten");

            builder.AddOrUpdate("Common.IncreaseValue", "Increase value", "Wert erhöhen");
            builder.AddOrUpdate("Common.DecreaseValue", "Decrease value", "Wert verringern");
            builder.AddOrUpdate("Common.QueryString", "Query string", "Query String");

            builder.AddOrUpdate("Admin.ContentManagement.Menus", "Menus", "Menüs");
            builder.AddOrUpdate("Admin.ContentManagement.AddMenu", "Add menu", "Menü hinzufügen");
            builder.AddOrUpdate("Admin.ContentManagement.EditMenu", "Edit menu", "Menü bearbeiten");

            builder.AddOrUpdate("Admin.ContentManagement.MenuLinks", "Menu items", "Menü Links");
            builder.AddOrUpdate("Admin.ContentManagement.AddMenuItem", "Add menu item", "Menü Link hinzufügen");
            builder.AddOrUpdate("Admin.ContentManagement.EditMenuItem", "Edit menu item", "Menü Link bearbeiten");

            builder.AddOrUpdate("Admin.ContentManagement.Menus.NoMenuItemsAvailable",
                "There are no menu links available.",
                "Es sind keine Menü Links vorhanden.");

            builder.AddOrUpdate("Admin.ContentManagement.Menus.CannotBeDeleted",
                "This menu is required by your shop and can therefore not be deleted.",
                "Dieses Menü wird von Ihrem Shop benötigt und kann daher nicht gelöscht werden.");

            builder.AddOrUpdate("Admin.ContentManagement.Menus.CatalogNote",
                "The category tree is dynamically integrated into the menu.",
                "Der Warengruppenbaum wird dynamisch in das Menü eingebunden.");

            builder.AddOrUpdate("Admin.ContentManagement.Menus.SpecifyLinkTarget",
                "Please specify link target",
                "Bitte Link Ziel angeben");

            builder.AddOrUpdate("Providers.MenuItems.FriendlyName.Entity", "Internal object or URL", "Internes Objekt oder URL");
            builder.AddOrUpdate("Providers.MenuItems.FriendlyName.Route", "Route", "Route");
            builder.AddOrUpdate("Providers.MenuItems.FriendlyName.Catalog", "Category tree", "Warengruppenbaum");

            builder.AddOrUpdate("Admin.ContentManagement.Menus.RouteName", "Route name", "Name der Route");
            builder.AddOrUpdate("Admin.ContentManagement.Menus.RouteValues", "Route values (JSON)", "Route Werte (JSON)");

            builder.AddOrUpdate("Admin.ContentManagement.Menus.SystemName",
                "System name",
                "Systemname",
                "The system name of the menu.",
                "Der Systemname des Menüs.");

            builder.AddOrUpdate("Admin.ContentManagement.Menus.Template",
                "Design template",
                "Design Vorlage",
                "The template defines the way how the menu is displayed.",
                "Über die Vorlage wird die Darstellungsart des Menüs festgelegt.");

            builder.AddOrUpdate("Admin.ContentManagement.Menus.WidgetZone",
                "Widget zone",
                "Widget Zone",
                "Specifies widget zones in which the menu should be displayed.",
                "Legt Widget Zonen fest, in denen das Menü dargestellt werden soll.");

            builder.AddOrUpdate("Admin.ContentManagement.Menus.Title",
                "Title",
                "Titel",
                "Specifies the title.",
                "Legt den Titel fest.");

            builder.AddOrUpdate("Admin.ContentManagement.Menus.Published",
                "Published",
                "Veröffentlicht",
                "Specifies whether the menu is visible in the shop.",
                "Legt fest, ob das Menü im Shop sichtbar ist.");

            builder.AddOrUpdate("Admin.ContentManagement.Menus.DisplayOrder",
                "Display order",
                "Reihenfolge",
                "Specifies the display order of the widget zones.",
                "Legt die Darstellungreihenfolge der Widget Zonen fest.");

            builder.AddOrUpdate("Admin.ContentManagement.Menus.Item.ParentItem",
                "Parent menu item",
                "Übergeordnetes Menüelement",
                "Specifies the parent menu item. Leave the field empty to create a first-level menu item.",
                "Legt das übergeordnete Menüelement fest. Lassen Sie das Feld leer, um ein Menüelement erster Ebene zu erzeugen.");

            builder.AddOrUpdate("Admin.ContentManagement.Menus.Item.LinkTarget",
                "Target",
                "Ziel",
                "Specifies the link target.",
                "Legt das Ziel des Links fest.");

            builder.AddOrUpdate("Admin.ContentManagement.Menus.Item.ShortDescription",
                "Short description",
                "Kurzbeschreibung",
                "Specifies a short description. Used as the 'title' attribute for the menu link.",
                "Legt eine Kurzbeschreibung fest. Wird als 'title' Attribut für das Menüelement verwendet.");

            builder.AddOrUpdate("Admin.ContentManagement.Menus.Item.PermissionNames",
                "Required permissions",
                "Erforderliche Rechte",
                "Specifies access permissions that are required to display the menu item (at least 1 permission must be granted).",
                "Legt Zugriffsrechte fest, die für die Anzeige des Menüelementes erforderlich sind (mind. 1 Recht muss gewährt sein).");

            builder.AddOrUpdate("Admin.ContentManagement.Menus.Item.Published",
                "Published",
                "Veröffentlicht",
                "Specifies whether the menu item is visible in the shop.",
                "Legt fest, ob das Menüelement im Shop sichtbar ist.");

            builder.AddOrUpdate("Admin.ContentManagement.Menus.Item.DisplayOrder",
                "Display order",
                "Reihenfolge",
                "Specifies the order of the menu item within a menu level.",
                "Legt die Reihenfolge des Menüelements innerhalb einer Menüebene fest.");

            builder.AddOrUpdate("Admin.ContentManagement.Menus.Item.BeginGroup",
                "Begin group",
                "Gruppe beginnen",
                "Inserts a separator before the link and optionally a heading (short description).",
                "Fügt vor den Link ein Trennelement sowie optional eine Überschrift ein (Kurzbeschreibung).");

            builder.AddOrUpdate("Admin.ContentManagement.Menus.Item.ShowExpanded",
                "Show expanded",
                "Geöffnet anzeigen",
                "If selected and this menu item has children, the menu will initially appear expanded.",
                "Legt fest, ob das Menü anfänglich geöffnet ist, sofern es Kindelemente besitzt.");

            builder.AddOrUpdate("Admin.ContentManagement.Menus.Item.NoFollow",
                "nofollow",
                "nofollow",
                "Sets the HTML attribute rel='nofollow'.",
                "Gibt das HTML-Attribut rel='nofollow' aus.");

            builder.AddOrUpdate("Admin.ContentManagement.Menus.Item.NewWindow",
                "Open in new browser tab",
                "In neuem Browsertab öffnen");

            builder.AddOrUpdate("Admin.ContentManagement.Menus.Item.Icon",
                "Icon",
                "Icon",
                "Specifies an optional icon.",
                "Legt ein optionales Icon fest.");

            builder.AddOrUpdate("Admin.ContentManagement.Menus.Item.HtmlId",
                "HTML ID",
                "HTML ID",
                "Sets the HTML ID attribute for the menu link.",
                "Legt das HTML ID Attribut für das Menüelement fest.");

            builder.AddOrUpdate("Admin.ContentManagement.Menus.Item.CssClass",
                "CSS class",
                "CSS Klasse",
                "Sets a CSS class for the menu link.",
                "Legt eine CSS Klasse für das Menüelement fest.");

            builder.Delete("Admin.Configuration.Settings.GeneralCommon.SocialSettings.GooglePlusLink");
            builder.Delete("Admin.Configuration.Settings.GeneralCommon.SocialSettings.GooglePlusLink.Hint");

            builder.AddOrUpdate("Products.BasePriceInfo.LanguageInsensitive",
                "{0} / {1} {2}",
                "{0} / {1} {2}");

            builder.AddOrUpdate("Common.Advanced",
                "Advanced",
                "Erweitert");

            builder.AddOrUpdate("ShoppingCart.DiscountCouponCode.NoMoreDiscount",
                "Further discounts are not possible.",
                "Eine weitere Rabattierung ist nicht möglich.");
        }
    }
}
