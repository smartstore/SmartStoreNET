namespace SmartStore.Data.Migrations
{
	using System;
	using System.Data.Entity.Migrations;
	using Setup;
	using Utilities;

	public partial class ProductMainPictureId : DbMigration, IDataSeeder<SmartObjectContext>
	{
		public override void Up()
        {
            AddColumn("dbo.Product", "MainPictureId", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Product", "MainPictureId");
        }

		public bool RollbackOnFailure
		{
			get { return true; }
		}

		public void Seed(SmartObjectContext context)
		{
			DataMigrator.FixProductMainPictureIds(context, true);
		}
	}
}
