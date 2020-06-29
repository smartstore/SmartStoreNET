namespace SmartStore.Data.Migrations
{
    using SmartStore.Core.Domain.Common;
    using SmartStore.Core.Domain.Customers;
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
            // Perf
            var numDeletedAttrs = DataMigrator.DeleteGuestCustomerGenericAttributes(context, TimeSpan.FromDays(30));
            var numDeletedCustomers = DataMigrator.DeleteGuestCustomers(context, TimeSpan.FromDays(30));

            var candidates = new[] { "Gender", "VatNumberStatusId", "TimeZoneId", "TaxDisplayTypeId", "LastForumVisit", "LastUserAgent", "LastUserDeviceType" };
            var numUpdatedCustomers = DataMigrator.MoveCustomerFields(context, UpdateCustomer, candidates);
        }

        private static void UpdateCustomer(Customer customer, GenericAttribute attr)
        {
            switch (attr.Key)
            {
                case "Gender":
                    customer.Gender = attr.Value?.Truncate(100);
                    break;
                case "VatNumberStatusId":
                    customer.VatNumberStatusId = attr.Value.Convert<int>();
                    break;
                case "TimeZoneId":
                    customer.TimeZoneId = attr.Value?.Truncate(255);
                    break;
                case "TaxDisplayTypeId":
                    customer.TaxDisplayTypeId = attr.Value.Convert<int>();
                    break;
                case "LastForumVisit":
                    customer.LastForumVisit = attr.Value.Convert<DateTime>();
                    break;
                case "LastUserAgent":
                    customer.LastUserAgent = attr.Value.Convert<string>();
                    break;
                case "LastUserDeviceType":
                    // TODO: split
                    customer.LastUserDeviceType = attr.Value.Convert<string>();
                    break;
            }
        }
    }
}
