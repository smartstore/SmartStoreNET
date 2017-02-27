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
	using Core.Domain.Catalog;

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

            // Remove permission record
            var permissionRecords = context.Set<PermissionRecord>();
            var record = permissionRecords.Where(x => x.SystemName.Equals("ManageContentSlider")).FirstOrDefault();
            if (record != null)
			{
				permissionRecords.Remove(record);
			}         

			// Set new product template as default
			var newProductTemplate = context.Set<ProductTemplate>().FirstOrDefault(x => x.Name == "Default Product Template") ?? context.Set<ProductTemplate>().LastOrDefault();
			if (newProductTemplate != null)
			{
				context.ExecuteSqlCommand("Update [Product] Set [ProductTemplateId] = {0}", true, null, newProductTemplate.Id);
			}

            context.SaveChanges();
        }

		public void MigrateSettings(SmartObjectContext context)
		{
			// Change ProductSortingEnum.Position > Relevance
			var settings = context.Set<Setting>().Where(x => x.Name == "CatalogSettings.DefaultSortOrder" && x.Value.StartsWith("Position")).ToList();
			if (settings.Any())
			{
				settings.Each(x => x.Value = "Relevance");
				EngineContext.Current.Resolve<ICacheManager>().Clear();
			}

            context.MigrateSettings(x => {
                x.DeleteGroup("ContentSlider");
            });

			// Change MediaSettings.ProductThumbPictureSize to 250 if smaller
			var keys = new string[] { "MediaSettings.ProductThumbPictureSize", "MediaSettings.CategoryThumbPictureSize", "MediaSettings.ManufacturerThumbPictureSize", "MediaSettings.CartThumbPictureSize", "MediaSettings.MiniCartThumbPictureSize" };
			settings = context.Set<Setting>().Where(x => keys.Contains(x.Name)).ToList();
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

			// Change MediaSettings.ProductDetailsPictureSize to 600 if smaller
			var setting = context.Set<Setting>().FirstOrDefault(x => x.Name == "MediaSettings.ProductDetailsPictureSize" || x.Name == "MediaSettings.AssociatedProductPictureSize");
			if (setting != null && setting.Value.Convert<int>() < 600)
			{
				setting.Value = "600";
			}

			// Change MediaSettings.VariantValueThumbPictureSize to 70 if smaller
			setting = context.Set<Setting>().FirstOrDefault(x => x.Name == "MediaSettings.VariantValueThumbPictureSize");
			if (setting != null && setting.Value.Convert<int>() < 70)
			{
				setting.Value = "70";
			}

			// Add new product template
			// TODO: (mc) refactor depending code to reflect this change (ProductTemplate.Simple/Grouped are obsolete now)
			context.Set<ProductTemplate>().AddOrUpdate(x => x.ViewPath, new ProductTemplate
			{
				Name = "Default Product Template",
				ViewPath = "Product",
				DisplayOrder = 10
			});

			// Change CatalogSettings.PageShareCode (16px > 32px)
			setting = context.Set<Setting>().FirstOrDefault(x => x.Name == "CatalogSettings.PageShareCode");
			if (setting != null)
			{
				setting.Value = "<!-- AddThis Button BEGIN --><div class=\"addthis_toolbox addthis_default_style addthis_32x32_style\"><a class=\"addthis_button_preferred_1\"></a><a class=\"addthis_button_preferred_2\"></a><a class=\"addthis_button_preferred_3\"></a><a class=\"addthis_button_preferred_4\"></a><a class=\"addthis_button_compact\"></a><a class=\"addthis_counter addthis_bubble_style\"></a></div><script type=\"text/javascript\">var addthis_config = {\"data_track_addressbar\":false};</script><script type=\"text/javascript\" src=\"//s7.addthis.com/js/300/addthis_widget.js#pubid=ra-50f6c18f03ecbb2f\"></script><!-- AddThis Button END -->";
			}

			// [...]

			context.SaveChanges();

			context.MigrateSettings(x => 
			{
				x.Add<int>("MediaSettings.DefaultThumbnailAspectRatio", 1);
                x.Delete("ShoppingCartSettings.MiniShoppingCartProductNumber");
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
			builder.AddOrUpdate("Search.ResultFiltering", "Result filtering", "Ergebnisfilterung");
			builder.AddOrUpdate("Search.FilterBy", "Filter by", "Filtern nach");
			builder.AddOrUpdate("Search.TermInCategory", "in {0}", "in {0}");
			builder.AddOrUpdate("Search.TermFromBrand", "from {0}", "von {0}");
			builder.AddOrUpdate("Search.SearchBox.Tooltip", "What are you looking for?", "Wonach suchen Sie?");
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
				"Show brand in product lists",
				"Zeige Hersteller/Marke in Produktlisten",
				"Specifies whether the brand name should be displayed in grid style product lists",
				"Legt fest, ob der Markenname auch in Rasterstil Produktlisten angezeigt werden sollen");

			builder.AddOrUpdate("Admin.Configuration.Settings.Catalog.ShowManufacturerLogoInLists",
				"Show brand logo instead of name",
				"Zeige Marken-Logo statt -Name",
				"Specifies whether the brand logo should be displayed in line style product lists. Falls back to textual name if no logo has been uploaded.",
				"Legt fest, ob das Marken-Logo in Produktlisten dargestellt werden soll (nicht anwendbar in Rasteransicht). Wenn kein Logo hochgeladen wurde, wird grundsätzlich der Name angezeigt.");

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

			builder.AddOrUpdate("Products.SortByX", "Sort by {0}", "Sortiere nach {0}");
			builder.AddOrUpdate("Products.SwitchToGrid", "Show", "Zur Rasteransicht wechseln");
			builder.AddOrUpdate("Products.SwitchToList", "Show", "Zur Listenansicht wechseln");
			builder.AddOrUpdate("Products.ToFilter", "Filter", "Filtern");
			builder.AddOrUpdate("Products.ToSort", "Sort", "Sortieren");

			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Catalog.ProductSortingEnum.CreatedOn", "Newest Arrivals", "Neu eingetroffen");
			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Catalog.ProductSortingEnum.Initial", "Position", "Position");
			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Catalog.ProductSortingEnum.PriceAsc", "Price: Low to High", "Preis: aufsteigend");
			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Catalog.ProductSortingEnum.PriceDesc", "Price: High to Low", "Preis: absteigend");
			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Catalog.ProductSortingEnum.Relevance", "Relevance", "Beste Ergebnisse");

			builder.AddOrUpdate("Pager.PageX", "Page {0}", "Seite {0}");
			builder.AddOrUpdate("Pager.XPerPage", "{0} per Page", "{0} pro Seite");
			builder.AddOrUpdate("Pager.PageXOfY", "Page {0} of {1}", "Seite {0} von {1}");
			builder.AddOrUpdate("Pager.PageXOfYShort", "{0} of {1}", "{0} von {1}");

			builder.AddOrUpdate("Products.Price.OldPrice", "Regular", "Regulär");
			builder.AddOrUpdate("Products.Sku", "SKU", "Art.-Nr.");
			builder.AddOrUpdate("Products.ChooseColorX", "Choose {0}", "{0} auswählen");


			builder.AddOrUpdate("Tax.LegalInfoShort", "Prices {0}, plus <a href='{1}'>shipping</a>", "Preise {0}, zzgl. <a href='{1}'>Versandkosten</a>");
			builder.AddOrUpdate("Tax.LegalInfoShort2", "Prices {0}, plus shipping", "Preise {0}, zzgl. Versandkosten");

			builder.AddOrUpdate("Enums.SmartStore.Core.Search.SearchMode.ExactMatch", "Is equal to (term)", "Ist gleich (Term)");
			builder.AddOrUpdate("Enums.SmartStore.Core.Search.SearchMode.StartsWith", "Starts with (prefix)", "Beginnt mit (Prefix)");
			builder.AddOrUpdate("Enums.SmartStore.Core.Search.SearchMode.Contains", "Contains (wildcard)", "Beinhaltet (Wildcard)");

			builder.AddOrUpdate("Admin.Configuration.Settings.Search.WildcardSearchNote",
				"The wildcard mode can slow down the search for a large number of products.",
				"Der Wildcard-Modus kann bei einer großen Anzahl an Produkten die Suche verlangsamen.");

			builder.AddOrUpdate("Admin.Configuration.Settings.Search.SearchFieldsNote",
				"The standard search searches the fields name, SKU and short description. For more fields, a search plugin like <a href='http://community.smartstore.com/marketplace/file/' target='_blank'>Mega Search Plugin</a> is required.",
				"In der Standardsuche werden die Felder Name, SKU und Kurzbeschreibung durchsucht. Für weitere Felder ist ein Such-Plugin wie bspw. dem <a href='http://community.smartstore.com/marketplace/file/' target='_blank'>Mega Search Plugin</a> notwendig.");

			builder.AddOrUpdate("Admin.Configuration.Settings.Search.SearchMode",
				"Search mode",
				"Suchmodus",
				"Specifies the search mode. Please keep in mind that the search mode can - depending on catalog size - strongly affect search performance. 'Is equal to' is the fastest, 'Contains' the slowest.",
				"Legt den Suchmodus fest. Bitte beachten Sie, dass der Suchmodus die Geschwindigkeit der Suche (abhängig von der Produktanzahl) beeinflusst. 'Ist gleich' ist am schnellsten, 'Beinhaltet' am langsamsten.");

			builder.AddOrUpdate("Admin.Configuration.DeliveryTimes.CannotDeleteAssignedProducts",
				"The delivery time cannot be deleted. It has associated products or product variants.",
				"Die Lieferzeit kann nicht gelöscht werden. Ihr sind Produkte oder Produktvarianten zugeordnet.");

			builder.AddOrUpdate("Media.Manufacturer.ImageLinkTitleFormat", "All products from {0}", "Alle Produkte von {0}");
			builder.AddOrUpdate("Manufacturers.List", "All Brands", "Alle Marken");

			builder.AddOrUpdate("Admin.Configuration.Settings.Catalog.GridStyleListColumnSpan",
				"Products per row in grid style list",
				"Anzahl Produkte pro Zeile in Rasteransicht",
				"Sets the responsive behavior of the grid style product list. The wider the screen, the more products are arranged in a row.",
				"Legt das responsive Verhalten der Produktliste in der Rasteransicht fest. Je breiter der Bildschirm, desto mehr Produkte werden in einer Zeile angeordnet.");

			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Catalog.GridColumnSpan.Max2Cols", "Always 2 (mobile & desktop) -not recommended", "Immer 2 (Mobil & Desktop) - nicht empfohlen");
			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Catalog.GridColumnSpan.Max3Cols", "2 (mobile) to 3 (desktop)", "2 (Mobil) bis 3 (Desktop)");
			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Catalog.GridColumnSpan.Max4Cols", "2 (mobile) to 4 (desktop) - recommended", "2 (Mobil) bis 4 (Desktop) - empfohlen");
			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Catalog.GridColumnSpan.Max5Cols", "2 (mobile) to 5 (desktop)", "2 (Mobil) bis 5 (Desktop)");
			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Catalog.GridColumnSpan.Max6Cols", "2 (mobile) to 6 (desktop)", "2 (Mobil) bis 6 (Desktop)");

            builder.AddOrUpdate("Catalog.Manufacturerall.Numbers", "#", "#");

            builder.AddOrUpdate("Admin.Configuration.Settings.Catalog.SortManufacturersAlphabetically",
                "Sort manufacturers alphabetically",
                "Hersteller alphabetisch sortieren",
                "Specifies whether manufacturers on the manufacturer overview page will be displayed sorted alphabetically.",
                "Legt fest ob Hersteller auf der Herstellerübersichtsseite alphabetisch sortiert dargestellt werden.");

			builder.AddOrUpdate("Common.NoImageAvail", "No image available", "Bild wird nachgereicht");

			builder.AddOrUpdate("Products.Bundle.PriceWithoutDiscount.Note", "Instead of", "Statt");
			builder.AddOrUpdate("Products.Bundle.PriceWithDiscount.Note", "As bundle only", "Im Set nur");
			builder.AddOrUpdate("Products.Price", "Price", "Preis");
			builder.AddOrUpdate("Products.TierPrices", "Block pricing", "Staffelpreise");
			builder.AddOrUpdate("Products.ManufacturerPartNumber", "MPN", "MPN");
			builder.AddOrUpdate("Products.Details", "Description", "Beschreibung");
			builder.AddOrUpdate("Products.Specs", "Features", "Merkmale");
			builder.AddOrUpdate("Products.Availability.InStockWithQuantity", "{0} in stock", "{0} am Lager");
			builder.AddOrUpdate("Products.Availability.InStock", "In stock", "Vorrätig");
			builder.AddOrUpdate("Products.Availability.OutOfStock", "Out of stock", "Vergriffen");

			builder.AddOrUpdate("Reviews.Overview.First", "Be the first to review this item", "Geben Sie die erste Bewertung ab");
			builder.AddOrUpdate("Reviews.Overview.AddNew", "Write a review", "Bewertung schreiben");
			builder.AddOrUpdate("Reviews.Overview.ReadAll", "Read all reviews", "Alle Bewertungen lesen");
			builder.AddOrUpdate("Reviews.Empty", "There are no reviews yet", "Es liegen keine Bewertungen vor");
			builder.AddOrUpdate("Reviews.Fields.Rating", "Your rating?", "Ihre Bewertung?");
			builder.AddOrUpdate("Reviews.Fields.Title", "Headline for your review", "Titel Ihrer Bewertung");
			builder.AddOrUpdate("Reviews.Fields.ReviewText", "Your opinion on the product", "Ihre Meinung zum Produkt");
			builder.AddOrUpdate("Reviews.SubmitButton", "Submit review", "Bewertung absenden");

			builder.AddOrUpdate("Admin.Configuration.Settings.Search.FilterMinHitCount",
				"Minimum hit count for filters",
				"Minimale Trefferanzahl für Filter",
				"Specifies the minimum number of search hits from which to show a filter.",
				"Legt die minimale Anzahl an Suchtreffern fest, ab dem ein Filter angezeigt wird.");

			builder.AddOrUpdate("Admin.Configuration.Settings.Search.FilterMaxChoicesCount",
				"Maximum number of filters",
				"Maximale Anzahl an Filtern",
				"Specifies the maximum number of filters per group.",
				"Legt die maximale Anzahl an Filtern pro Gruppe fest.");

			builder.AddOrUpdate("Enums.SmartStore.Core.Search.Facets.FacetSorting.HitsDesc",
				"Hit count: highest first",
				"Trefferanzahl: Höchste zuerst");

			builder.AddOrUpdate("Enums.SmartStore.Core.Search.Facets.FacetSorting.ValueAsc",
				"Name: A to Z",
				"Name: A bis Z");

			builder.AddOrUpdate("Enums.SmartStore.Core.Search.Facets.FacetSorting.DisplayOrder",
				"According to display order",
				"Gemäß Reihenfolge");

			builder.AddOrUpdate("Admin.Catalog.Attributes.SpecificationAttributes.Fields.Alias",
				"Alias",
				"Alias",
				"An optional reference name for internal use.",
				"Ein optionaler Referenzwert für interne Zwecke.");

			builder.AddOrUpdate("Admin.Catalog.Attributes.SpecificationAttributes.Options.Fields.Alias",
				"Alias",
				"Alias",
				"An optional reference name for internal use.",
				"Ein optionaler Referenzwert für interne Zwecke.");

			builder.AddOrUpdate("Admin.Catalog.Attributes.SpecificationAttributes.Fields.FacetSorting",
				"Sorting of search filters",
				"Sortierung der Suchfilter",
				"Specifies the sorting of the search filters. This setting is only effective in accordance with the Mega-Search-Plus plugin. Changes will take effect after a renewal of the search index.",
				"Legt die Sortierung der Suchfilter fest. Diese Einstellung ist nur in Zusammenhang mit dem Mega-Search-Plus Plugin wirksam. Änderungen werden erst nach einer Erneuerung des Suchindex wirksam.");

			builder.AddOrUpdate("Search.Facet.Category", "Category", "Kategorie");
			builder.AddOrUpdate("Search.Facet.Manufacturer", "Brand", "Marke");
			builder.AddOrUpdate("Search.Facet.Price", "Price", "Preis");
			builder.AddOrUpdate("Search.Facet.Rating", "Rating", "Bewertung");
			builder.AddOrUpdate("Search.Facet.DeliveryTime", "Delivery Time", "Lieferzeit");

			builder.AddOrUpdate("Admin.Catalog.Products.ProductVariantAttributes.Attributes.Values.ViewLink",
				"Edit Options (Total: {0})",
				"Optionen bearbeiten (Anzahl: {0})");
			builder.AddOrUpdate("Admin.Catalog.Products.ProductVariantAttributes.Attributes.Values.EditAttributeDetails",
				"Options for attribute '{0}'. Product: {1}",
				"Optionen für Attribut '{0}'. Produkt: {1}");
			builder.AddOrUpdate("Admin.Catalog.Products.ProductVariantAttributes.Attributes.Values", "Options", "Optionen");
			builder.AddOrUpdate("Admin.Catalog.Attributes.CheckoutAttributes.Values", "Options", "Optionen");

			builder.AddOrUpdate("Search.Facet.PriceLabelTemplate", "up to {0}", "bis {0}");

			builder.AddOrUpdate("Common.CopyToClipboard.Failed", "Failed to copy.", "Kopieren ist fehlgeschlagen.");

            builder.AddOrUpdate("PDFPackagingSlip.Gtin", "EAN", "Gtin");

			builder.AddOrUpdate("Common.Error.AliasAlreadyExists",
				"An alias \"{0}\" already exists. Please use a different value.",
				"Ein Alias \"{0}\" existiert bereits. Bitte verwenden Sie einen anderen Wert.");

            builder.AddOrUpdate("Admin.Configuration.DeliveryTimes.Fields.IsDefault", 
                "Is default",
                "Ist Standard",
                "Specifies the default delivery time.",
                "Bestimmt die Standard-Lieferzeit");

            builder.AddOrUpdate("Admin.Configuration.Settings.Catalog.ShowDefaultDeliveryTime",
                "Show default delivery time",
                "Zeige Standard-Lieferzeit",
                "Specifies whether to show the default delivery time if there is none assigned to a product.",
                "Bestimmt ob die Standard-Lieferzeit für ein Produkt angezeigt wird, wenn dem Produkt keine Lieferzeit zugewiesen wurde.");
            
            builder.AddOrUpdate("Admin.Catalog.Products.Fields.QuantityStep",
                "Quantity step",
                "Schrittweite",
                "Specifies the incremental respectively decremental step on usage of +/-. Orderable quantities are limited to a multiple of this value.",
                "Bestimmt den Wert, um den die Bestellmenge erhöht bzw. vermindert wird, wenn ein Kunde die +/- Steuerelemente benutzt. Die Bestellmenge ist auf ein Vielfaches dieses Wertes beschränkt.");

            builder.AddOrUpdate("Admin.Catalog.Products.Fields.QuantiyControlType",
                "Control type",
                "Steuerelement",
                "Specifies the control type to enter the quantity.",
                "Bestimmt das Steuerelement für die Angabe der Bestellmenge.");

            builder.AddOrUpdate("Admin.Catalog.Products.Fields.HideQuantityControl",
                "Hide quantity control on product pages",
                "Angabe der Bestellmenge auf Produktseiten nicht anbieten",
                "Specifies whether to hide the quantity control on product pages.",
                "Bestimmt ob eine Element zur Angabe der Bestellmenge auf Produktdetailseiten dargestellt wird.");
            
            builder.AddOrUpdate("Admin.Configuration.Settings.Catalog.ShowManufacturerInProductDetail",
                "Display manufacturer",
                "Hersteller anzeigen.",
                "Specifies whether the product manufacturer will be displayed on product detail pages.",
                "Bestimmt ob der Hersteller eines Produktes auf Produktdetailseiten angezeigt wird.");
			builder.AddOrUpdate("Admin.Catalog.Products.Fields.CustomsTariffNumber",
				"Customs Tariff Number",
				"Zolltarifnummer",
				"Specifies the customs tariff number of the product.",
				"Legt die Zolltarifnummer des Produktes fest.");

			builder.AddOrUpdate("Admin.Catalog.Products.Fields.CountryOfOriginId",
				"Country of Origin",
				"Herkunftsland",
				"Specifies the country of origin of the product.",
				"Legt das Herkunftsland des Produktes fest.");

			builder.Delete(
				"Products.ProductHasBeenAddedToTheWishlist", 
				"Products.ProductHasBeenAddedToTheWishlist.Link",
				"Products.ProductHasBeenAddedToTheCart",
				"Products.ProductHasBeenAddedToTheCart.Link");

			builder.AddOrUpdate("ShoppingCart.AddToWishlist", "Add to wishlist", "Auf die Wunschliste");
			builder.AddOrUpdate("ShoppingCart.Mini.AddedItemToCart", "The product {0} has been successfully added to your cart", "Das Produkt {0} wurde erfolgreich in den Warenkorb gelegt");
			builder.AddOrUpdate("ShoppingCart.Mini.AddedItemToWishlist", "The product {0} has been added to your wishlist", "Das Produkt {0} wurde erfolgreich auf ihrer Wunschliste vermerkt");
			builder.AddOrUpdate("ShoppingCart.Mini.AddedItemToCompare", "The product {0} has been successfully added to your compare list", "Das Produkt {0} wurde der Vergleichsliste erfolgreich hinzugefügt");
			builder.AddOrUpdate("ShoppingCart.Mini.EmptyCart.Title", "Shopping cart empty", "Warenkorb ist leer");
			builder.AddOrUpdate("ShoppingCart.Mini.EmptyWishlist.Title", "Wishlist empty", "Wunschliste ist leer");
			builder.AddOrUpdate("ShoppingCart.Mini.EmptyCompare.Title", "Compare list empty", "Vergleichsliste ist leer");
			builder.AddOrUpdate("ShoppingCart.Mini.EmptyCart.Info",
				"You have not added any product to your cart yet. Use the <i class='{0}'></i> icon to add a product to your cart.", 
				"Sie haben noch keine Produkte in ihren Warenkorb gelegt.<br /> Benutzen Sie das <i class='{0}'></i> Symbol, um ein Produkt in den Warenkorb zu legen.");
			builder.AddOrUpdate("ShoppingCart.Mini.EmptyWishlist.Info",
				"You have not added any product to your wishlist yet. Use the <i class='{0}'></i> icon to add a product to your wishlist.",
				"Sie haben noch keine Produkte auf ihrer Wunschliste vermerkt.<br /> Benutzen Sie das <i class='{0}'></i> Symbol, um ein Produkt in ihrer Wunschliste zu vermerken.");
			builder.AddOrUpdate("ShoppingCart.Mini.EmptyCompare.Info",
				"You have not added any product to your compare list yet. Use the <i class='{0}'></i> icon to add a product to your compare list.",
				"Sie haben noch keine Produkte in ihrer Vergleichsliste.<br /> Benutzen Sie das <i class='{0}'></i> Symbol, um ein Produkt in die Vergleichsliste aufzunehmen.");

            builder.AddOrUpdate("PageTitle.Blog.Month", "Blog entries in {0}", "Blog Einträge des Monats {0}");
            builder.AddOrUpdate("PageTitle.Blog.Tag", "Blog entries for the tag {0}", "Blog-Einträge für das Stichwort {0}");
            builder.AddOrUpdate("Metadesc.Blog.Month", "Blog entries in {0}", "Blog Einträge des Monats {0}");
            builder.AddOrUpdate("Metadesc.Blog.Tag", "Blog entries for the tag {0}", "Blog-Einträge für das Stichwort {0}");

            builder.AddOrUpdate("Admin.Catalog.Products.ProductVariantAttributes.Attributes.Values.Fields.Picture",
                "Picture",
                "Bild",
                "Choose a picture which will be displayed as the selector for the attribute.",
                "Wählen Sie ein Bild, welches als Auswahlelement für das Attribut angezeigt werden soll.");

            builder.Delete(
                "Admin.Configuration.Settings.ShoppingCart.MiniShoppingCartProductNumber",
                "Admin.Configuration.Settings.ShoppingCart.MiniShoppingCartProductNumber.Hint");

            builder.AddOrUpdate("ShoppingCart.MoveToWishlist", "Move to wishlist", "In die Wunschliste verschieben");
            builder.AddOrUpdate("Products.Compare.CompareNow", "Compare now", "Jetzt vergleichen");
            builder.AddOrUpdate("ShoppingCart.PaymentButtonBar.Or", "Or", "Oder");

			builder.AddOrUpdate("Common.Error.OptionAlreadyExists",
				"The option \"{0}\" already exists.",
				"Die Option \"{0}\" existiert bereits.");

			builder.AddOrUpdate("Admin.Catalog.Attributes.ProductAttributes.Fields.AllowFiltering",
				"Allow filtering",
				"Filtern zulassen",
				"Specifies whether search results can be filtered by this attribute.",
				"Legt fest, ob Suchergebnisse nach diesem Attribut gefiltert werden können.");

			builder.AddOrUpdate("Common.Menu", "Menu", "Menü");

			builder.Delete(
				"Admin.Configuration.Settings.GeneralCommon.FullTextSettings",
				"Admin.Configuration.Settings.GeneralCommon.FullTextSettings.Supported",
				"Admin.Configuration.Settings.GeneralCommon.FullTextSettings.NotSupported",
				"Admin.Configuration.Settings.GeneralCommon.FullTextSettings.CurrenlyEnabled",
				"Admin.Configuration.Settings.GeneralCommon.FullTextSettings.CurrenlyDisabled",
				"Admin.Configuration.Settings.GeneralCommon.FullTextSettings.Disable",
				"Admin.Configuration.Settings.GeneralCommon.FullTextSettings.Disabled",
				"Admin.Configuration.Settings.GeneralCommon.FullTextSettings.Enable",
				"Admin.Configuration.Settings.GeneralCommon.FullTextSettings.Enabled",
				"Admin.Configuration.Settings.GeneralCommon.FullTextSettings.SearchMode",
				"Admin.Configuration.Settings.GeneralCommon.FullTextSettings.SearchMode.Hint",
				"Enums.SmartStore.Core.Domain.Common.FulltextSearchMode.ExactMatch",
				"Enums.SmartStore.Core.Domain.Common.FulltextSearchMode.And",
				"Enums.SmartStore.Core.Domain.Common.FulltextSearchMode.Or"
			);

			builder.AddOrUpdate("Common.Options.Count", "Number options", "Anzahl Optionen");
			builder.AddOrUpdate("Common.Options.Add", "Add option", "Option hinzufügen");
			builder.AddOrUpdate("Common.Options.Edit", "Edit option", "Option bearbeiten");

			builder.AddOrUpdate("Admin.Validation.RequiredField", "Please enter \"{0}\".", "Bitte \"{0}\" angeben.");
		}
	}
}
