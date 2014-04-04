namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
	using SmartStore.Data.Setup;

	public partial class CurrencyMorePrecision : DbMigration, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
            AlterColumn("dbo.Currency", "Rate", c => c.Decimal(nullable: false, precision: 18, scale: 8));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Currency", "Rate", c => c.Decimal(nullable: false, precision: 18, scale: 4));
        }

		public void Seed(SmartObjectContext context)
		{
			context.MigrateSettings(x => {
				x.Add("catalogsettings.showvariantcombinationpriceadjustment", true);
				x.Add("catalogsettings.enabledynamicpriceupdate", true);
			});
		}

		public bool RollbackOnFailure
		{
			get { return false; }
		}
	}
}
