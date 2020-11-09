namespace SmartStore.Data.Migrations
{
    using System.Data.Entity.Migrations;
    using System.Linq;
    using System.Web.Hosting;
    using SmartStore.Core.Data;
    using SmartStore.Core.Domain.DataExchange;
    using SmartStore.Core.Domain.Localization;
    using SmartStore.Data.Setup;

    public partial class RemoveDiscountRequirements : DbMigration, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
            //DropForeignKey("dbo.DiscountRequirement", "DiscountId", "dbo.Discount");
            //DropIndex("dbo.DiscountRequirement", new[] { "DiscountId" });
            //DropTable("dbo.DiscountRequirement");
        }

        public override void Down()
        {
            //CreateTable(
            //    "dbo.DiscountRequirement",
            //    c => new
            //        {
            //            Id = c.Int(nullable: false, identity: true),
            //            DiscountId = c.Int(nullable: false),
            //            DiscountRequirementRuleSystemName = c.String(),
            //            SpentAmount = c.Decimal(nullable: false, precision: 18, scale: 4),
            //            BillingCountryId = c.Int(nullable: false),
            //            ShippingCountryId = c.Int(nullable: false),
            //            RestrictedToCustomerRoleId = c.Int(),
            //            RestrictedProductIds = c.String(),
            //            RestrictedPaymentMethods = c.String(),
            //            RestrictedShippingOptions = c.String(),
            //            RestrictedToStoreId = c.Int(),
            //            ExtraData = c.String(),
            //        })
            //    .PrimaryKey(t => t.Id);

            //CreateIndex("dbo.DiscountRequirement", "DiscountId");
            //AddForeignKey("dbo.DiscountRequirement", "DiscountId", "dbo.Discount", "Id", cascadeDelete: true);
        }

        public bool RollbackOnFailure => false;

        public void Seed(SmartObjectContext context)
        {
            if (!HostingEnvironment.IsHosted || !DataSettings.Current.IsSqlServer)
            {
                return;
            }

            // Remove discount requirement.
            // Data has been migrated through 202001301039020_DiscountRuleSets.cs.
            context.ExecuteSqlCommandSafe("ALTER TABLE [dbo].[DiscountRequirement] DROP CONSTRAINT [FK_dbo.DiscountRequirement_dbo.Discount_DiscountId]");
            context.ExecuteSqlCommandSafe("DROP INDEX [IX_DiscountId] ON [dbo].[DiscountRequirement]");
            context.ExecuteSqlCommandSafe("DROP TABLE [dbo].[DiscountRequirement]");

            // Remove data of obsolete filter plugins.
            var syncMappingsSet = context.Set<SyncMapping>();
            var obsoleteSyncMappings = syncMappingsSet.Where(x => x.ContextName == "SmartStore.ShippingFilter" || x.ContextName == "SmartStore.PaymentFilter").ToList();
            if (obsoleteSyncMappings.Any())
            {
                syncMappingsSet.RemoveRange(obsoleteSyncMappings);
                context.SaveChanges();
            }

            // Remove obsolete string resources.
            var resourceRanges = new string[]
            {
                "Plugins.Payment.PaymentFilter.",
                "Plugins.Shipping.ShippingFilter.",
                "Plugins.DiscountRequirement.MustBeAssignedToCustomerRole",
                "Plugins.DiscountRules.",
                "Plugins.SmartStore.DiscountRules.",
                "Plugins.FriendlyName.DiscountRequirement.",
                "Plugins.FriendlyName.SmartStore.DiscountRules.",
                "Admin.Promotions.Discounts.Requirements."
            };

            var resourceNames = new string[]
            {
                "Plugins.FriendlyName.SmartStore.PaymentFilter",
                "Plugins.FriendlyName.SmartStore.ShippingFilter",
                "Plugins.FriendlyName.SmartStore.DiscountRules",
                "Plugins.Description.SmartStore.DiscountRules",
                "Plugins.Description.SmartStore.PaymentFilter",
                "Plugins.Description.SmartStore.ShippingFilter",
                "Enums.SmartStore.PaymentFilter.PaymentFilterCountryContext.BillingAddress",
                "Enums.SmartStore.PaymentFilter.PaymentFilterCountryContext.ShippingAddress",
                "Enums.SmartStore.PaymentFilter.PaymentFilterAmountContext.SubtotalAmount",
                "Enums.SmartStore.PaymentFilter.PaymentFilterAmountContext.TotalAmount",
                "Enums.SmartStore.ShippingFilter.ShippingFilterCountryContext.BillingAddress",
                "Enums.SmartStore.ShippingFilter.ShippingFilterCountryContext.ShippingAddress",
                "Admin.Common.Restrictions",
                "Admin.Configuration.Restriction.SaveBeforeEdit",
                "Admin.Configuration.Shipping.Methods.RestrictionNote",
                "Admin.Configuration.Payment.Methods.RestrictionNote"
            };

            var resourceSet = context.Set<LocaleStringResource>();

            foreach (var range in resourceRanges)
            {
                var resources = resourceSet.Where(x => x.ResourceName.StartsWith(range)).ToList();
                if (resources.Any())
                {
                    resourceSet.RemoveRange(resources);
                    context.SaveChanges();
                }
            }

            var moreResources = resourceSet.Where(x => resourceNames.Contains(x.ResourceName)).ToList();
            if (moreResources.Any())
            {
                resourceSet.RemoveRange(moreResources);
                context.SaveChanges();
            }
        }
    }
}
