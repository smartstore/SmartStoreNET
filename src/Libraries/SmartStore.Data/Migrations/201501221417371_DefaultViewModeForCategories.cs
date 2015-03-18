namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    using SmartStore.Core.Domain.Topics;
	using SmartStore.Data.Setup;
	using System.Linq;

    public partial class DefaultViewModeForCategories : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
            AddColumn("dbo.Category", "DefaultViewMode", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Category", "DefaultViewMode");
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
            builder.AddOrUpdate("Admin.Configuration.Settings.ShoppingCart.Checkout",
                "Checkout",
                "Checkout");
            builder.AddOrUpdate("Admin.Configuration.Settings.CustomerUser.ValidateEmailAddress",
                "Validate customer email address",
                "Prüfung der Kunden-Email-Adresse");
            builder.AddOrUpdate("Admin.Configuration.Settings.CustomerUser.ValidateEmailAddress.Hint",
                "Determines whether the customers email address is validated during the checkout process",
                "Bestimmt ob die Email-Adresse des Kunden im Checkout validiert wird.");
            builder.AddOrUpdate("Admin.Address.Fields.EmailMatch",
                "Reenter email address",
                "Bestätigung der E-Mail");
            builder.AddOrUpdate("Address.Fields.EmailMatch",
                "Reenter email address",
                "Bestätigung der E-Mail");
            builder.AddOrUpdate("Admin.Address.Fields.EmailMatch.Required",
                "Please reenter your email address",
                "Bitte wiederholen Sie die Eingabe Ihrer E-Mail-Adresse");
            builder.AddOrUpdate("Admin.Address.Fields.EmailMatch.MustMatchEmail",
                "The email address has to match the email address entered before",
                "Die E-Mail-Adresse muss mit der zuvor eingegebenen E-Mail-Adresse übereinstimmen");
        }
    }
}
