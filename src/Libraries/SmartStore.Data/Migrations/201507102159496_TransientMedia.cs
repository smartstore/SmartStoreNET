namespace SmartStore.Data.Migrations
{
	using System;
	using System.Data.Entity.Migrations;
	using SmartStore.Core.Domain.Tasks;
	using SmartStore.Data.Setup;

	public partial class TransientMedia : DbMigration, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
            AddColumn("dbo.Download", "IsTransient", c => c.Boolean(nullable: false));
            AddColumn("dbo.Download", "UpdatedOnUtc", c => c.DateTime(nullable: false));
            AddColumn("dbo.Picture", "IsTransient", c => c.Boolean(nullable: false));
            AddColumn("dbo.Picture", "UpdatedOnUtc", c => c.DateTime(nullable: false));
            CreateIndex("dbo.Download", new[] { "UpdatedOnUtc", "IsTransient" }, name: "IX_UpdatedOn_IsTransient");
            CreateIndex("dbo.Picture", new[] { "UpdatedOnUtc", "IsTransient" }, name: "IX_UpdatedOn_IsTransient");
        }
        
        public override void Down()
        {
			DropIndex("dbo.Picture", "IX_UpdatedOn_IsTransient");
			DropIndex("dbo.Download", "IX_UpdatedOn_IsTransient");
			DropColumn("dbo.Picture", "UpdatedOnUtc");
			DropColumn("dbo.Picture", "IsTransient");
			DropColumn("dbo.Download", "UpdatedOnUtc");
			DropColumn("dbo.Download", "IsTransient");
        }

		public bool RollbackOnFailure
		{
			get { return false; }
		}

		public void Seed(SmartObjectContext context)
		{
			context.Set<ScheduleTask>().AddOrUpdate(x => x.Type,
				new ScheduleTask
				{
					Name = "Clear transient uploads",
					CronExpression = "30 1,13 * * *",
					Type = "SmartStore.Services.Media.TransientMediaClearTask, SmartStore.Services",
					Enabled = true,
					StopOnError = false,
				}
			);

			context.SaveChanges();
		}
	}
}
