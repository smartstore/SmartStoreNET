namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    using SmartStore.Data.Setup;

    public partial class ProductBulkEditInfo : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
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
        }

        public void MigrateLocaleResources(LocaleResourcesBuilder builder)
        {
            builder.AddOrUpdate("Admin.Catalog.BulkEdit.Info",
                "Click on the values of the products you want to change within the grid and change them as desired. Accept the changes with a click on [Save changes] or discard them by clicking [Cancel changes]",
                "Klicken Sie in der Tabelle auf die Werte der Produkte, die Sie bearbeiten wollen. Ändern Sie die Werte wie gewünscht und übernehmen Sie Ihre Änderungen mit einem Klick auf [Änderungen speichern] oder verwerfen Sie sie mit einem Klick auf [Änderungen verwerfen]");
        }
    }
}
