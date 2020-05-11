namespace SmartStore.Data.Migrations
{
    using Setup;
    using System;
    using System.Data.Entity.Migrations;

    public partial class TierPriceCalcMethod : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
            AddColumn("dbo.TierPrice", "CalculationMethod", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.TierPrice", "CalculationMethod");
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
            builder.AddOrUpdate("Admin.Product.Price.Tierprices.Fixed", "Fixed Value", "Sabit Dəyər");
            builder.AddOrUpdate("Admin.Product.Price.Tierprices.Percental", "Percental", "Faiz");
            builder.AddOrUpdate("Admin.Product.Price.Tierprices.Adjustment", "Adjustment", "Tənzimləmə");
            builder.AddOrUpdate("Admin.Catalog.Products.TierPrices.Fields.CalculationMethod", "Calculation Method", "Hesablama metodu");
            builder.AddOrUpdate("Admin.Catalog.Products.TierPrices.Fields.Price", "Value", "Dəyər");

            // settings
            builder.AddOrUpdate("Admin.Configuration.Settings.Catalog.ApplyTierPricePercentageToAttributePriceAdjustments",
                "Apply tierprice percentage to attribute price adjustments",
                "Qiymət düzəlişlərini təyin etmək üçün pillə faizini tətbiq edin",
                "Specifies whether to apply tierprice percentage to attribute price adjustments",
                "Qiymət düzəlişlərinə atribut faizini tətbiq edib-etməməyinizi müəyyənləşdirir");

			builder.AddOrUpdate("Admin.Header.Account", "Account", "Hesab");
        }
    }
}
