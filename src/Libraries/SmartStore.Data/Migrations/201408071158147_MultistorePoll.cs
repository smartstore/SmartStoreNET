namespace SmartStore.Data.Migrations
{
	using System;
	using System.Linq;
	using System.Data.Entity.Migrations;
	using SmartStore.Core.Domain.Stores;
	using SmartStore.Data.Setup;

	public partial class MultistorePoll : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
			AddColumn("dbo.Poll", "LimitedToStores", c => c.Boolean(nullable: false));
			AddColumn("dbo.NewsLetterSubscription", "StoreId", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
			DropColumn("dbo.Poll", "LimitedToStores");
			DropColumn("dbo.NewsLetterSubscription", "StoreId");
        }

		public bool RollbackOnFailure
		{
			get { return false; }
		}

		public void Seed(SmartObjectContext context)
		{
			context.MigrateLocaleResources(MigrateLocaleResources);

			int storeId = context.SqlQuery<int>("Select Top 1 Id From Store").FirstOrDefault();
			context.Execute("Update [dbo].[NewsLetterSubscription] Set StoreId = {0} Where StoreId = 0".FormatWith(storeId));
		}

		public void MigrateLocaleResources(LocaleResourcesBuilder builder)
		{
		}
    }
}
