namespace SmartStore.Data.Migrations
{
    using SmartStore.Core.Data;
    using SmartStore.Data.Setup;
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ShopIcons : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
            AddColumn("dbo.Store", "PngIconMediaFileId", c => c.Int(nullable: false));
            AddColumn("dbo.Store", "AppleTouchIconMediaFileId", c => c.Int(nullable: false));
            AddColumn("dbo.Store", "MsTileImageMediaFileId", c => c.Int(nullable: false));
            AddColumn("dbo.Store", "MsTileColor", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Store", "MsTileColor");
            DropColumn("dbo.Store", "MsTileImageMediaFileId");
            DropColumn("dbo.Store", "AppleTouchIconMediaFileId");
            DropColumn("dbo.Store", "PngIconMediaFileId");
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
            builder.AddOrUpdate("Admin.Configuration.Stores.Fields.PngIconMediaFileId",
                "PNG icon",
                "PNG-Icon",
                "Defines the icon for the store. This icon is used as favicon and when shortcuts to your store are created.",
                "Legt das Icon für den Shop fest. Dieses Icon wird als Favicon verwendet und wenn Shortcuts zu Ihrem Shop erstellt werden.");

            builder.AddOrUpdate("Admin.Configuration.Stores.Fields.AppleTouchIconMediaFileId",
                "Apple Touch icon",
                "Apple Touch Icon",
                "TODO",
                "TODO");

            builder.AddOrUpdate("Admin.Configuration.Stores.Fields.MsTileImageMediaFileId",
                "Microsoft tile icon",
                "Microsoft Kachelgrafik",
                "TODO",
                "TODO");

            builder.AddOrUpdate("Admin.Configuration.Stores.Fields.MsTileColor",
                "Microsoft tile color",
                "Microsoft Kachelfarbe",
                "TODO",
                "TODO");
        }
    }
}
