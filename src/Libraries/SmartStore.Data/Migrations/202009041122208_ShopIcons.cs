namespace SmartStore.Data.Migrations
{
    using System.Data.Entity.Migrations;
    using SmartStore.Core.Data;
    using SmartStore.Data.Setup;

    public partial class ShopIcons : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
            AddColumn("dbo.Store", "FavIconMediaFileId", c => c.Int());
            AddColumn("dbo.Store", "PngIconMediaFileId", c => c.Int());
            AddColumn("dbo.Store", "AppleTouchIconMediaFileId", c => c.Int());
            AddColumn("dbo.Store", "MsTileImageMediaFileId", c => c.Int());
            AddColumn("dbo.Store", "MsTileColor", c => c.String());
        }

        public override void Down()
        {
            DropColumn("dbo.Store", "MsTileColor");
            DropColumn("dbo.Store", "MsTileImageMediaFileId");
            DropColumn("dbo.Store", "AppleTouchIconMediaFileId");
            DropColumn("dbo.Store", "PngIconMediaFileId");
            DropColumn("dbo.Store", "FavIconMediaFileId");
        }

        public bool RollbackOnFailure => true;

        public void Seed(SmartObjectContext context)
        {
            if (!DataSettings.DatabaseIsInstalled())
                return;

            // Add resources.
            context.MigrateLocaleResources(MigrateLocaleResources);

            context.SaveChanges();
        }

        public void MigrateLocaleResources(LocaleResourcesBuilder builder)
        {
            builder.AddOrUpdate("Admin.Configuration.Stores.Fields.FavIconMediaFileId",
                "Favicon",
                "Favicon",
                "Defines the favicon for the store. This icon should contain graphics in the dimensions 16×16, 32×32 and 48×48.",
                "Legt das Favicon für den Shop fest. Dieses Icon sollte Grafiken in den Abmessungen 16×16, 32×32 und 48×48 beinhalten.");

            builder.AddOrUpdate("Admin.Configuration.Stores.Fields.PngIconMediaFileId",
                "PNG icon",
                "PNG-Icon",
                "Defines the PNG icon for the store. This icon is recognized as favicon by up-to-date devices and used when shortcuts to your store are created. Optimal dimensions for this icon are 196x196.",
                "Legt das PNG-Icon für den Shop fest. Dieses Icon wird von aktuellen Geräten als Favicon erkannt und verwendet, wenn Shortcuts zu Ihrem Shop erstellt werden. Optimale Abmessungen für das Icon sind 196x196.");

            builder.AddOrUpdate("Admin.Configuration.Stores.Fields.AppleTouchIconMediaFileId",
                "Apple Touch icon",
                "Apple Touch Icon",
                "Defines the icon used by Apple devices as shortcut icon. Optimal dimensions for this icon are 180x180.",
                "Legt das Icon fest, das von Apple-Geräten als Shortcut-Icon verwendet wird. Optimale Abmessungen für das Icon sind 180x180.");

            builder.AddOrUpdate("Admin.Configuration.Stores.Fields.MsTileImageMediaFileId",
                "Microsoft tile picture",
                "Microsoft Kachelgrafik",
                "Defines the icon that is used by Microsoft devices as a tile picture when shortcuts to your store are created. Optimal dimensions for the picture are 310x310.",
                "Legt das Icon fest, das von Microsoft-Geräten als Kachelgrafik verwendet wird, wenn Shortcuts zu Ihrem Shop erstellt werden. Optimale Abmessungen für das Bild sind 310x310.");

            builder.AddOrUpdate("Admin.Configuration.Stores.Fields.MsTileColor",
                "Microsoft tile color",
                "Microsoft Kachelfarbe",
                "Defines the color used by Microsoft devices as tile color when shortcuts to your store are created.",
                "Legt die Farbe fest, die von Microsoft-Geräten als Kachelfarbe verwendet wird, wenn Shortcuts zu Ihrem Shop erstellt werden.");
        }
    }
}
