namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    using SmartStore.Data.Setup;

    public partial class AclRecordCustomerRole : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
            CreateIndex("dbo.AclRecord", "CustomerRoleId");
            AddForeignKey("dbo.AclRecord", "CustomerRoleId", "dbo.CustomerRole", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AclRecord", "CustomerRoleId", "dbo.CustomerRole");
            DropIndex("dbo.AclRecord", new[] { "CustomerRoleId" });
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
            builder.AddOrUpdate("Admin.Configuration.Category.Acl.AssignToSubCategoriesAndProducts",
                "Assign this ACL configuration to all subcategories and products included in this category",
                "Diese Konfiguration für alle Unterwarengruppen und Produkte übernehmen");

            builder.AddOrUpdate("Admin.Configuration.Category.Acl.AssignToSubCategoriesAndProducts.Hint",
                @"Please keep in mind you have to save changes in the ACL configuration <br/> 
                    before you can assign them to all subcategories and products. <br/>
                    <b>Attention:</b> Please keep in mind that <b>existing acl records will be deleted</b>",
                @"Bitte beachten Sie, dass die Änderungen der Zugriffsrechte zunächst gespeichert werden müssen, <br />
                    bevor diese für Unterkategorien und Produkte übernommen werden können. <br />
                    <b>Vorsicht:</b> Bitte beachten Sie, <b>dass vorhandene Zugriffsrechte überschrieben bzw. gelöscht werden</b>.");

            builder.AddOrUpdate("Admin.Configuration.Category.Stores.AssignToSubCategoriesAndProducts",
                "Assign this store configuration to all subcategories and products included in this category",
                "Diese Konfiguration für alle Unterwarengruppen und Produkte übernehmen");

            builder.AddOrUpdate("Admin.Configuration.Category.Stores.AssignToSubCategoriesAndProducts.Hint",
                @"Please keep in mind you have to save changes in the store configuration <br/> 
                    before you can assign them to all subcategories and products. <br/>
                    <b>Attention:</b> Please keep in mind that <b>existing store mappings will be deleted</b>",
                @"Bitte beachten Sie, dass die Änderungen an der Store-Konfiguration zunächst gespeichert werden müssen, <br />
                    bevor diese für Unterkategorien und Produkte übernommen werden können. <br />
                    <b>Vorsicht:</b> Bitte beachten Sie, <b>dass vorhandene Store-Konfiguration überschrieben bzw. gelöscht werden</b>.");

            builder.AddOrUpdate("Admin.Configuration.Acl.NoRolesDefined",
                "No customer roles defined",
                "Es sind keine Kundengruppen definiert");
            
        }
    }
}
