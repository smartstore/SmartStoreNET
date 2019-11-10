namespace SmartStore.Data.Migrations
{
	using System;
	using System.Data.Entity.Migrations;
	using Setup;
    using SmartStore.Core.Data;
    using SmartStore.Core.Domain.Catalog;
	using SmartStore.Core.Domain.Common;
    using SmartStore.Core.Domain.Tasks;
    using SmartStore.Utilities;

	public sealed class MigrationsConfiguration : DbMigrationsConfiguration<SmartObjectContext>
	{
		public MigrationsConfiguration()
		{
			AutomaticMigrationsEnabled = false;
			AutomaticMigrationDataLossAllowed = true;
			ContextKey = "SmartStore.Core";

            if (DataSettings.Current.IsSqlServer)
            {
                var commandTimeout = CommonHelper.GetAppSetting<int?>("sm:EfMigrationsCommandTimeout");
                if (commandTimeout.HasValue)
                {
                    CommandTimeout = commandTimeout.Value;
                }

                CommandTimeout = 9999999;
            }
		}

		public void SeedDatabase(SmartObjectContext context)
		{
			using (var scope = new DbContextScope(context, hooksEnabled: false))
			{
				Seed(context);
				scope.Commit();
			}		
		}

		protected override void Seed(SmartObjectContext context)
		{
            context.MigrateLocaleResources(MigrateLocaleResources);
			MigrateSettings(context);
        }

		public void MigrateSettings(SmartObjectContext context)
		{

		}

		public void MigrateLocaleResources(LocaleResourcesBuilder builder)
		{
            builder.AddOrUpdate("Admin.Configuration.Measures.Weights.AddWeight", "Add weight", "Gewicht hinzufügen");
            builder.AddOrUpdate("Admin.Configuration.Measures.Weights.EditWeight", "Edit weight", "Gewicht bearbeiten");

            builder.AddOrUpdate("Admin.Configuration.Measures.Dimensions.AddDimension", "Add dimension", "Abmessung hinzufügen");
            builder.AddOrUpdate("Admin.Configuration.Measures.Dimensions.EditDimension", "Edit dimension", "Abmessung bearbeiten");

            builder.AddOrUpdate("Admin.Configuration.QuantityUnit.AddQuantityUnit", "Add quantity unit", "Verpackungseinheit hinzufügen");
            builder.AddOrUpdate("Admin.Configuration.QuantityUnit.EditQuantityUnit", "Edit quantity unit", "Verpackungseinheit bearbeiten");

            builder.AddOrUpdate("Admin.Configuration.Settings.Catalog.ApplyPercentageDiscountOnTierPrice",
                "Apply percentage discounts on tier prices",
                "Prozentuale Rabatte auf Staffelpreise anwenden",
                "Specifies whether to apply percentage discounts also on tier prices.",
                "Legt fest, ob prozentuale Rabatte auch auf Staffelpreise angewendet werden sollen.");

            builder.AddOrUpdate("Admin.Configuration.Settings.ShoppingCart.ShowProductBundleImagesOnShoppingCart",
                "Show product images of bundle items",
                "Produktbilder von Bundle-Bestandteilen anzeigen",
                "Specifies whether to show product images of bundle items.",
                "Legt fest, ob Produktbilder von Bundle-Bestandteilen angezeigt werden sollen.");

            builder.AddOrUpdate("Admin.Promotions.Discounts.DiscountRequirementsCount",
                "Number of requirements",
                "Anzahl an Voraussetzungen");

            builder.AddOrUpdate("Admin.Common.CustomerRole.LimitedTo",
                "Limited to customer roles",
                "Auf Kundengruppen begrenzt",
                "Specifies whether the object is only available to certain customer groups.",
                "Legt fest, ob das Objekt nur für bestimmte Kundengruppen verfügbar ist.");

            builder.AddOrUpdate("Admin.Permissions.AllowInherited", "Allow (inherited)", "Erlaubt (geerbt)");
            builder.AddOrUpdate("Admin.Permissions.DenyInherited", "Deny (inherited)", "Verweigert (geerbt)");

            builder.AddOrUpdate("Admin.AccessDenied.DetailedDescription",
                "<div>You do not have permission to perform the selected operation.</div><div>Access right: {0}</div><div>System name: {1}</div>",
                "<div>Sie haben keine Berechtigung, diesen Vorgang durchzuführen.</div><div>Zugriffsrecht: {0}</div><div>Systemname: {1}</div>");

            builder.Delete(
                "Admin.Configuration.Measures.Weights.Fields.MarkAsPrimaryWeight",
                "Admin.Configuration.Measures.Dimensions.Fields.MarkAsPrimaryDimension",
                "Admin.Customers.Customers.Addresses.AddButton",
                "Admin.Address",
                "Admin.Catalog.Products.Acl",
                "Admin.Configuration.ACL.Updated",
                "Admin.Configuration.Stores.NoStoresDefined",
                "Admin.Configuration.Acl.NoRolesDefined",
                "Admin.Common.Acl.AvailableFor");

            builder.AddOrUpdate("Admin.ContentManagement.Blog.BlogPosts.Fields.Tags.Hint",
                "Tags are keywords that this blog post can also be identified by. Enter a comma separated list of the tags to be associated with this blog post.",
                "Dieser Blog-Eintrag kann durch die Verwendung von Tags (Stichwörter) gekennzeichnet und kategorisiert werden. Mehrere Tags können als kommagetrennte Liste eingegeben werden.");

            // Granular permission.
            builder.AddOrUpdate("Common.Read", "Read", "Lesen");
            builder.AddOrUpdate("Common.Create", "Create", "Erstellen");
            builder.AddOrUpdate("Common.Notify", "Notify", "Benachrichtigen");
            builder.AddOrUpdate("Common.Approve", "Approve", "Genehmigen");
            builder.AddOrUpdate("Common.Rules", "Rules", "Regeln");

            builder.AddOrUpdate("Common.Allow", "Allow", "Erlaubt");
            builder.AddOrUpdate("Common.Deny", "Deny", "Verweigert");

            builder.AddOrUpdate("Common.ExpandCollapseAll", "Expand\\collapse all", "Alle auf\\zuklappen");

            builder.AddOrUpdate("Admin.Customers.PermissionViewNote",
                "The view shows the permissions that apply to this customer based on the customer roles assigned to him. To change permissions, switch to the relevant <a class=\"alert-link\" href=\"{0}\">customer role</a>.",
                "Die Ansicht zeigt die Rechte, die für diesen Kunden auf Basis der ihm zugeordneten Kundengruppen gelten. Um Rechte zu ändern, wechseln Sie bitte zur betreffenden <a class=\"alert-link\" href=\"{0}\">Kundengruppe</a>.");

            builder.AddOrUpdate("Permissions.DisplayName.DisplayPrice", "Display prices", "Preise anzeigen");
            builder.AddOrUpdate("Permissions.DisplayName.AccessShop", "Access shop", "Zugang zum Shop");
            builder.AddOrUpdate("Permissions.DisplayName.AccessShoppingCart", "Access shoppping cart", "Auf Warenkorb zugreifen");
            builder.AddOrUpdate("Permissions.DisplayName.AccessWishlist", "Access wishlist", "Auf Wunschliste zugreifen");
            builder.AddOrUpdate("Permissions.DisplayName.AccessBackend", "Access backend", "Auf Backend zugreifen");
            builder.AddOrUpdate("Permissions.DisplayName.EditOrderItem", "Edit order items", "Auftragspositionen bearbeiten");
            builder.AddOrUpdate("Permissions.DisplayName.EditShipment", "Edit shipment", "Sendungen bearbeiten");
            builder.AddOrUpdate("Permissions.DisplayName.EditAnswer", "Edit answers", "Antworten bearbeiten");
            builder.AddOrUpdate("Permissions.DisplayName.EditOptionSet", "Edit options sets", "Options-Sets bearbeiten");
            builder.AddOrUpdate("Permissions.DisplayName.EditCategory", "Edit categories", "Warengruppen bearbeiten");
            builder.AddOrUpdate("Permissions.DisplayName.EditManufacturer", "Edit manufacturers", "Hersteller bearbeiten");
            builder.AddOrUpdate("Permissions.DisplayName.EditAssociatedProduct", "Edit associated products", "Verknüpfte Produkte bearbeiten");
            builder.AddOrUpdate("Permissions.DisplayName.EditBundle", "Edit bundles", "Bundles bearbeiten");
            builder.AddOrUpdate("Permissions.DisplayName.EditPromotion", "Edit promotion", "Promotion bearbeiten");
            builder.AddOrUpdate("Permissions.DisplayName.EditPicture", "Edit pictures", "Bilder bearbeiten");
            builder.AddOrUpdate("Permissions.DisplayName.EditTag", "Edit tags", "Tags bearbeiten");
            builder.AddOrUpdate("Permissions.DisplayName.EditAttribute", "Edit attributes", "Attribute bearbeiten");
            builder.AddOrUpdate("Permissions.DisplayName.EditVariant", "Edit variants", "Varianten bearbeiten");
            builder.AddOrUpdate("Permissions.DisplayName.EditTierPrice", "Edit tier prices", "Staffelpreise bearbeiten");
            builder.AddOrUpdate("Permissions.DisplayName.EditRecurringPayment", "Edit recurring payment", "Wiederkehrende Zahlungen bearbeiten");
            builder.AddOrUpdate("Permissions.DisplayName.EditOption", "Edit options", "Optionen bearbeiten");
            builder.AddOrUpdate("Permissions.DisplayName.EditAddress", "Edit addresses", "Adressen bearbeiten");
            builder.AddOrUpdate("Permissions.DisplayName.EditCustomerRole", "Edit customer roles", "Kundengruppen bearbeiten");
            builder.AddOrUpdate("Permissions.DisplayName.SendPM", "Send private messages", "Private Nachrichten senden");
            builder.AddOrUpdate("Permissions.DisplayName.EditProduct", "Edit products", "Produkte bearbeiten");
            builder.AddOrUpdate("Permissions.DisplayName.EditComment", "Edit comments", "Kommentare bearbeiten");
            builder.AddOrUpdate("Permissions.DisplayName.EditResource", "Edit resources", "Ressourcen bearbeiten");

            builder.AddOrUpdate("Admin.ContentManagement.Blog.Heading.Publish", "Publishing", "Veröffentlichung");
            builder.AddOrUpdate("Admin.ContentManagement.Blog.Heading.Display", "Display", "Darstellung");
            builder.AddOrUpdate("Admin.ContentManagement.News.Heading.Publish", "Publishing", "Veröffentlichung");

            builder.AddOrUpdate("Admin.Validation.Url", "Please enter a valid URL.", "Bitte geben Sie eine gültige URL ein.");

            builder.AddOrUpdate("Admin.Catalog.Products.Fields.Visibility",
                "Visibility",
                "Sichtbarkeit",
                "Limits the visibility of the product. In the case of \"Not visible\", the product only appears as an associated product on the parent product detail page, but without a link to an individual page.",
                "Schränkt die Sichtbarkeit des Produktes ein. Bei \"Nicht sichtbar\" erscheint das Produkt nur noch als verknüpftes Produkt auf der übergeordneten Produktdetailseite, jedoch ohne Verlinkung auf eine eigenständige Seite.");

            builder.AddOrUpdate("Admin.DataExchange.Export.Filter.Visibility",
                "Visibility",
                "Sichtbarkeit",
                "Filter by visibility. \"In search results\" includes fully visible products.",
                "Nach Sichtbarkeit filter. \"In Suchergebnissen\" schließt überall sichtbare Produkte mit ein.");

            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Catalog.ProductVisibility.Full", "Fully visible", "Überall sichtbar");
            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Catalog.ProductVisibility.SearchResults", "In search results", "In Suchergebnissen");
            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Catalog.ProductVisibility.ProductPage", "On product detail pages", "Auf Produktdetailseiten");
            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Catalog.ProductVisibility.Hidden", "Not visible", "Nicht sichtbar");

            builder.Delete(
                "Admin.Catalog.Products.Fields.VisibleIndividually",
                "Admin.Catalog.Products.Fields.VisibleIndividually.Hint");
        }
    }
}
