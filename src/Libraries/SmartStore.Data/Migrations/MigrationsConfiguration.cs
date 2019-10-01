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
            context.Set<ScheduleTask>().AddOrUpdate(x => x.Type,
                new ScheduleTask
                {
                    Name = "Rebuild XML Sitemap",
                    CronExpression = "45 3 * * *",
                    Type = "SmartStore.Services.Seo.RebuildXmlSitemapTask, SmartStore.Services",
                    Enabled = true,
                    StopOnError = false
                }
            );

            context.SaveChanges();

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
            builder.AddOrUpdate("Admin.Permissions.UnknownPermission", "Unknown permission \"{0}\".", "Unbekanntes Zugriffsrecht \"{0}\".");

            builder.AddOrUpdate("Admin.AccessDenied.DetailedDescription",
                "<div>You do not have permission to perform the selected operation.</div><div>Access right: {0}</div><div>System name: {1}</div>",
                "<div>Sie haben keine Berechtigung, diesen Vorgang durchzuführen.</div><div>Zugriffsrecht: {0}</div><div>Systemname: {1}</div>");

            builder.Delete(
                "Admin.Configuration.Measures.Weights.Fields.MarkAsPrimaryWeight",
                "Admin.Configuration.Measures.Dimensions.Fields.MarkAsPrimaryDimension",
                "Admin.Customers.Customers.Addresses.AddButton",
                "Admin.Address",
                "Admin.Catalog.Products.Acl",
                "Admin.Configuration.ACL.Updated");

            builder.AddOrUpdate("Admin.ContentManagement.Blog.BlogPosts.Fields.Tags.Hint",
                "Tags are keywords that this blog post can also be identified by. Enter a comma separated list of the tags to be associated with this blog post.",
                "Dieser Blog-Eintrag kann durch die Verwendung von Tags (Stichwörter) gekennzeichnet und kategorisiert werden. Mehrere Tags können als kommagetrennte Liste eingegeben werden.");
        }
    }
}
