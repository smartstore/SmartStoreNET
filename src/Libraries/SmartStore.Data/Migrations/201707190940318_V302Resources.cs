namespace SmartStore.Data.Migrations
{
    using Setup;
    using System;
    using System.Data.Entity.Migrations;

    public partial class V302Resources : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
        }
        
        public override void Down()
        {
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
            builder.AddOrUpdate("Admin.Configuration.Settings.Search.DefaultSortOrderMode",
                "Default product sort order",
                "Standardsortierreihenfolge für Produkte",
                "Specifies the default product sort order in search results.",
                "Legt die Standardsortierreihenfolge für Produkte in den Suchergebnissen fest.");

            builder.AddOrUpdate("Common.Recommended", "Recommended", "Empfohlen");

            builder.AddOrUpdate("Admin.Configuration.Themes.Option.AssetCachingEnabled",
                "Enable asset caching",
                "Asset Caching aktivieren",
                "Determines whether compiled asset files should be cached in file system in order to speed up application restarts. Select 'Auto', if caching should depend on the debug setting in web.config.",
                "Legt fest, ob kompilierte JS- und CSS-Dateien wie bspw. 'Sass' im Dateisystem zwischengespeichert werden sollen, um den Programmstart zu beschleunigen. Wählen Sie 'Automatisch', wenn das Caching von der Debug-Einstellung in der web.config abhängig sein soll.");

            builder.AddOrUpdate("Admin.Configuration.Themes.ClearAssetCache",
                "Clear asset cache",
                "Asset Cache leeren");

            builder.AddOrUpdate("Search.Facet.ExcludeOutOfStock", "Exclude Out of Stock", "Nicht verfügbare Artikel ausschließen");

            builder.AddOrUpdate("Admin.Configuration.Settings.Search.IncludeNotAvailable",
                "Include out of stock products",
                "Nicht verfügbare Produkte einschließen",
                "Specifies whether to include or exclude products that are out of stock by default.",
                "Legt fest, ob nicht verfügbare Produkte in Suchergebnissen standardmäßig angezeigt werden sollen oder nicht.");
        }
    }
}
