namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using Core.Caching;
    using Core.Domain.Configuration;
    using Core.Infrastructure;
    using Setup;
    using Core.Domain.Security;

    public sealed class MigrationsConfiguration : DbMigrationsConfiguration<SmartObjectContext>
	{
		public MigrationsConfiguration()
		{
			AutomaticMigrationsEnabled = false;
			AutomaticMigrationDataLossAllowed = true;
			ContextKey = "SmartStore.Core";
		}

		public void SeedDatabase(SmartObjectContext context)
		{
			Seed(context);
		}

		protected override void Seed(SmartObjectContext context)
		{
			// TODO: (mc) Temp only. Put this in a seeding migration right before release.
			context.MigrateLocaleResources(MigrateLocaleResources);
			MigrateSettings(context);

            // remove permission record
            var permissionRecords = context.Set<PermissionRecord>();
            var record = permissionRecords.Where(x => x.SystemName.Equals("ManageContentSlider")).FirstOrDefault();
            if (record != null)
                permissionRecords.Remove(record);

            context.SaveChanges();
        }

		public void MigrateSettings(SmartObjectContext context)
		{
			// Change ProductSortingEnum.Position > Relevance
			var settings = context.Set<Setting>().Where(x => x.Name == "CatalogSettings.DefaultSortOrder" && x.Value == "Position").ToList();
			if (settings.Any())
			{
				settings.Each(x => x.Value = "Relevance");
				EngineContext.Current.Resolve<ICacheManager>().Clear();
			}

            context.MigrateSettings(x => {
                x.DeleteGroup("ContentSlider");
            });
			// Change MediaSettings.ProductThumbPictureSize to 250 if smaller
			settings = context.Set<Setting>().Where(x => x.Name == "MediaSettings.ProductThumbPictureSize").ToList();
			if (settings.Any())
			{
				settings.Each(x =>
				{
					var size = x.Value.Convert<int>();
					if (size < 250)
					{
						x.Value = "250";
					}
				});
			}

            // [...]
            
            context.SaveChanges();

			context.MigrateSettings(x => 
			{
				x.Add<int>("MediaSettings.DefaultThumbnailAspectRatio", 1);
			});
		}

		public void MigrateLocaleResources(LocaleResourcesBuilder builder)
		{
			builder.AddOrUpdate("Admin.Catalog.Categories.Fields.BadgeText",
				"Badge text",
				"Badge-Text",
				"Gets or sets the text of the badge which will be displayed next to the category link within menus.",
				"Legt den Text der Badge fest, die innerhalb von Menus neben den Menueinträgen dargestellt wird.");

			builder.AddOrUpdate("Admin.Catalog.Categories.Fields.BadgeStyle",
				"Badge style",
				"Badge-Style",
				"Gets or sets the type of the badge which will be displayed next to the category link within menus.",
				"Legt den Stil der Badge fest, die innerhalb von Menus neben den Menueinträgen dargestellt wird.");

			builder.AddOrUpdate("Admin.Header.ClearDbCache",
				"Clear database cache",
				"Datenbank Cache löschen");

			builder.AddOrUpdate("Admin.System.Warnings.TaskScheduler.OK",
				"The task scheduler can poll and execute tasks.",
				"Der Task-Scheduler kann Hintergrund-Aufgaben planen und ausführen.");

			builder.AddOrUpdate("Admin.System.Warnings.TaskScheduler.Fail",
				"The task scheduler cannot poll and execute tasks. Base URL: {0}, Status: {1}. Please specify a working base url in web.config, setting 'sm:TaskSchedulerBaseUrl'.",
				"Der Task-Scheduler kann keine Hintergrund-Aufgaben planen und ausführen. Basis-URL: {0}, Status: {1}. Bitte legen Sie eine vom Webserver erreichbare Basis-URL in der web.config Datei fest, Einstellung: 'sm:TaskSchedulerBaseUrl'.");

			builder.AddOrUpdate("Products.NotFound",
				"The product with ID {0} was not found.",
				"Das Produkt mit der ID {0} wurde nicht gefunden.");

			builder.AddOrUpdate("Products.Deleted",
				"The product with ID {0} has been deleted.",
				"Das Produkt mit der ID {0} wurde gelöscht.");

			builder.AddOrUpdate("Common.ShowLess", "Show less", "Weniger anzeigen");
			builder.AddOrUpdate("Menu.ServiceMenu", "Help & Services", "Hilfe & Service");

			// Search
			builder.Delete("PageTitle.Search");
			builder.Delete("Search.ResultFor");

			builder.AddOrUpdate("Search", "Search", "Suchen");
			builder.AddOrUpdate("Search.Title", "Search", "Suche");
			builder.AddOrUpdate("Search.PageTitle", "Search result for {0}", "Suchergebnis für {0}");
			builder.AddOrUpdate("Search.PagingInfo", "{0} of {1}", "{0} von {1}");
			builder.AddOrUpdate("Search.DidYouMean", "Did you mean?", "Meinten Sie?");
			builder.AddOrUpdate("Search.Hits", "Hits", "Treffer");
			builder.AddOrUpdate("Search.NoResultsText", "Your search did not match any products.", "Ihre Suche ergab leider keine Produkttreffer.");
			builder.AddOrUpdate("Search.NumHits", "{0} Hits", "{0} Treffer");
			builder.AddOrUpdate("Search.InstantSearch", "Instant Search", "Instantsuche");
			builder.AddOrUpdate("Search.GlobalFilters", "Global Filters", "Globale Filter");
			builder.AddOrUpdate("Search.FilterBy", "Filter by", "Filtern nach");
			builder.AddOrUpdate("Search.SearchTermMinimumLengthIsNCharacters",
				"The minimum length for the search term is {0} characters.",
				"Die Mindestlänge für den Suchbegriff beträgt {0} Zeichen.");
			builder.AddOrUpdate("Search.TermCorrectedHint",
				"Displaying results for {0}. Your search for {1} did not match any results.",
				"Ergebnisse für {0} werden angezeigt. Ihre Suche nach {1} ergab leider keine Treffer.");

			builder.Delete("Enums.SmartStore.Core.Domain.Catalog.ProductSortingEnum.Position");
			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Catalog.ProductSortingEnum.Relevance", "Relevance", "Relevanz");

			builder.Delete(
				"Admin.Configuration.Settings.Catalog.ProductSearchAutoCompleteEnabled",
				"Admin.Configuration.Settings.Catalog.ProductSearchAutoCompleteEnabled.Hint",
				"Admin.Configuration.Settings.Catalog.ProductSearchAutoCompleteNumberOfProducts",
				"Admin.Configuration.Settings.Catalog.ProductSearchAutoCompleteNumberOfProducts.Hint",
				"Admin.Configuration.Settings.Catalog.SearchPageProductsPerPage",
				"Admin.Configuration.Settings.Catalog.SearchPageProductsPerPage.Hint",
				"Admin.Configuration.Settings.Catalog.ProductSearchAllowCustomersToSelectPageSize",
				"Admin.Configuration.Settings.Catalog.ProductSearchAllowCustomersToSelectPageSize.Hint",
				"Admin.Configuration.Settings.Catalog.ProductSearchPageSizeOptions",
				"Admin.Configuration.Settings.Catalog.ProductSearchPageSizeOptions.Hint",
				"Admin.Configuration.Settings.Catalog.ShowProductImagesInSearchAutoComplete",
				"Admin.Configuration.Settings.Catalog.ShowProductImagesInSearchAutoComplete.Hint",
				"Admin.Configuration.Settings.Catalog.SuppressSkuSearch",
				"Admin.Configuration.Settings.Catalog.SuppressSkuSearch.Hint",
				"Admin.Configuration.Settings.Catalog.SearchDescriptions",
				"Admin.Configuration.Settings.Catalog.SearchDescriptions.Hint",
				"Admin.Configuration.Settings.Catalog.ProductSearchSettings",
				"Admin.Configuration.Settings.Catalog.ProductsByTagPageSize",
				"Admin.Configuration.Settings.Catalog.ProductsByTagPageSize.Hint"
			);

			builder.AddOrUpdate("Admin.Configuration.Settings.Search.InstantSearchEnabled",
				"Enable Instant Search",
				"Instant-Suche aktivieren",
				"Activates Instant Search (Search-As-You-Type). Search hits and suggestions are already displayed before user finishes typing the search term.",
				"Aktiviert die Instant-Suche (Search-As-You-Type). Suchtreffer und -Vorschläge werden schon während der Eingabe des Suchbegriffs angezeigt.");

			builder.AddOrUpdate("Admin.Configuration.Settings.Search.ShowProductImagesInInstantSearch",
				"Show product images",
				"Produktbilder anzeigen",
				"Specifies whether to display product images in instant search.",
				"Legt fest, ob Produktbilder in der Instantsuche angezeigt werden sollen.");

			builder.AddOrUpdate("Admin.Configuration.Settings.Search.InstantSearchNumberOfProducts",
				"Number of products",
				"Produktanzahl",
				"Specifies the number of product hits displayed in instant search.",
				"Legt die Anzahl der angezeigten Produkt-Treffer in der Instantsuche fest.");

			builder.AddOrUpdate("Admin.Configuration.Settings.Search.InstantSearchTermMinLength",
				"Minimum search term length",
				"Minimale Suchbegrifflänge",
				"Specifies the minimum length of a search term from which to show the result of instant search.",
				"Legt die minimale Länge eines Suchbegriffs fest, ab dem das Ergebnis der Instantsuche angezeigt wird.");

			builder.AddOrUpdate("Admin.Configuration.Settings.Search.SearchFields",
				"Search fields",
				"Suchfelder",
				"Specifies additional search fields. The product name is always searched.",
				"Legt zusätzlich zu durchsuchende Felder fest. Der Produktname wird grundsätzlich immer durchsucht.");

			builder.AddOrUpdate("Admin.Configuration.Settings.Catalog.DefaultProductListPageSize",
				"Number of products displayed per page",
				"Anzahl pro Seite angezeigter Produkte",
				"Specifies the number of products displayed per page in a product list.",
				"Legt die Anzahl der pro Seite angezeigten Produkte in einer Produktliste fest.");

			builder.AddOrUpdate("Admin.Validation.ValueRange",
				"The value must be between {0} and {1}.",
				"Der Wert muss zwischen {0} und {1} liegen.");

			builder.AddOrUpdate("Admin.Validation.ValueGreaterThan",
				"The value must be greater than {0}.",
				"Der Wert muss größer als {0} sein.");

			builder.AddOrUpdate("Common.AdditionalShippingSurcharge",
				"zzgl. <b>{0}</b> zusätzlicher Versandgebühr",
				"Plus <b>{0}</b> shipping surcharge");

			builder.DeleteFor("Admin.Configuration.ContentSlider");
			builder.DeleteFor("Admin.ContentManagement.ContentSlider");
			builder.DeleteFor("Admin.ContentSlider.Slide");
			builder.Delete("Admin.Themes.ContentSlider");

			builder.AddOrUpdate("Admin.Configuration.Settings.Catalog.ShowShortDescriptionInGridStyleLists",
				"Show short description in product lists",
				"Zeige Kurzbeschreibung in Produktlisten",
				"Specifies whether the product short description should be displayed in product lists",
				"Legt fest, ob die Produkt-Kurzbeschreibung auch in Produktlisten angezeigt werden sollen");

			builder.AddOrUpdate("Admin.Configuration.Settings.Catalog.ShowManufacturerInGridStyleLists",
				"Show manufacturer in product lists",
				"Zeige Hersteller in Produktlisten",
				"Specifies whether the manufacturer name should be displayed in grid style product lists",
				"Legt fest, ob der Hersteller-Name auch in Rasterstil Produktlisten angezeigt werden sollen");

			builder.AddOrUpdate("Admin.Configuration.Settings.Catalog.ShowProductOptionsInLists",
				"Show variant names in product lists",
				"Zeige Variantnamen in Produktlisten",
				"Specifies whether variant names should be displayed in product lists",
				"Legt fest, ob Variantnamen in Produktlisten angezeigt werden sollen");

			builder.AddOrUpdate("Products.PlusOption", "Further option", "Weitere Option");
			builder.AddOrUpdate("Products.PlusOptions", "More options", "Weitere Optionen");

			builder.AddOrUpdate("Products.Longdesc.More", "Show more", "Mehr anzeigen");
			builder.AddOrUpdate("Products.Longdesc.Less", "Show less", "Weniger anzeigen");

			builder.AddOrUpdate("Products.Tags.ProductsTaggedWith", "Products tagged with {0}", "Produkte markiert mit {0}");

			builder.AddOrUpdate("Admin.Configuration.Settings.Catalog.DefaultPageSizeOptions",
				"Page size options",
				"Auswahlmöglichkeiten für Seitengröße",
				"Comma-separated page size options that a customer can select in product lists.",
				"Kommagetrennte Liste mit Optionen für Seitengröße, die ein Kunde in Produktlisten wählen kann.");

			builder.AddOrUpdate("Common.ListIsEmpty", "The list is empty.", "Die Liste ist leer.");

			builder.AddOrUpdate("Products.SortBy", "Sort by", "Sortiere nach");
			builder.AddOrUpdate("Products.PageSize", "Show", "Zeige");
			builder.AddOrUpdate("Products.SwitchToGrid", "Show", "Zur Rasteransicht wechseln");
			builder.AddOrUpdate("Products.SwitchToList", "Show", "Zur Listenansicht wechseln");

			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Catalog.ProductSortingEnum.CreatedOn", "Newest Arrivals", "Neu eingetroffen");
			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Catalog.ProductSortingEnum.Initial", "Position", "Position");
			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Catalog.ProductSortingEnum.PriceAsc", "Price: Low to High", "Preis: aufsteigend");
			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Catalog.ProductSortingEnum.PriceDesc", "Price: High to Low", "Preis: absteigend");
			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Catalog.ProductSortingEnum.Relevance", "Relevance", "Beste Ergebnisse");
		}
	}
}
