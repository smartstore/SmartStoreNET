namespace SmartStore.Data.Migrations
{
    using SmartStore.Data.Setup;
    using SmartStore.Data.Utilities;
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MoveFurtherCustomerFields : DbMigration, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
            AddColumn("dbo.Customer", "Gender", c => c.String());
            AddColumn("dbo.Customer", "ZipPostalCode", c => c.String());
            AddColumn("dbo.Customer", "VatNumberStatusId", c => c.Int(nullable: false));
            AddColumn("dbo.Customer", "TimeZoneId", c => c.String());
            AddColumn("dbo.Customer", "TaxDisplayTypeId", c => c.Int(nullable: false));
            AddColumn("dbo.Customer", "CountryId", c => c.Int(nullable: false));
            AddColumn("dbo.Customer", "CurrencyId", c => c.Int());
            AddColumn("dbo.Customer", "LanguageId", c => c.Int(nullable: false));
            AddColumn("dbo.Customer", "LastForumVisit", c => c.DateTime());
            AddColumn("dbo.Customer", "LastUserAgent", c => c.String());
            AddColumn("dbo.Customer", "LastUserDeviceType", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Customer", "LastUserDeviceType");
            DropColumn("dbo.Customer", "LastUserAgent");
            DropColumn("dbo.Customer", "LastForumVisit");
            DropColumn("dbo.Customer", "LanguageId");
            DropColumn("dbo.Customer", "CurrencyId");
            DropColumn("dbo.Customer", "CountryId");
            DropColumn("dbo.Customer", "TaxDisplayTypeId");
            DropColumn("dbo.Customer", "TimeZoneId");
            DropColumn("dbo.Customer", "VatNumberStatusId");
            DropColumn("dbo.Customer", "ZipPostalCode");
            DropColumn("dbo.Customer", "Gender");
        }

        public bool RollbackOnFailure => true;

        public void Seed(SmartObjectContext context)
        {
            DataMigrator.MoveFurtherCustomerFields(context);
        }
    }
}
