namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    using SmartStore.Data.Setup;

    public partial class CheckoutCommmentBox : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
    {
        public void Seed(SmartObjectContext context)
        {
            context.MigrateLocaleResources(MigrateLocaleResources);
        }

        public override void Up()
        {
            AddColumn("dbo.Order", "CustomerOrderComment", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Order", "CustomerOrderComment");
        }

        public void MigrateLocaleResources(LocaleResourcesBuilder builder)
        {
            // add or update resources
            builder.AddOrUpdate("ShoppingCart.CommentBox")
                .Value("Do you want to tell us something regarding this order?")
                .Value("de", "Möchten Sie uns etwas Wichtiges zu Ihrer Bestellung mitteilen?");

            builder.AddOrUpdate("Admin.Order.CustomerComment.Heading")
                .Value("The customer has added the following comment to his order")
                .Value("de", "Der Kunde hat folgenden Kommentar für diese Bestellung hinterlassen");

            builder.AddOrUpdate("Admin.Configuration.Settings.ShoppingCart.ShowCommentBox")
                .Value("Show comment box on confirm order page")
                .Value("de", "Zeige Kommentarbox auf Bestellabschlussseite");

            builder.AddOrUpdate("Admin.Configuration.Settings.ShoppingCart.ShowCommentBox.Hint")
                .Value("Determines whether comment box is displayed on confirm order page")
                .Value("de", "Legt fest ob der Kunde auf der Bestellabschlussseite einen Kommentar zu seiner Bestellung hinterlegen kann.");
        }

        public bool RollbackOnFailure
        {
            get { return false; }
        }
    }
}
