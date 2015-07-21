namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class WebScheduler : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ScheduleTask", "NextRunUtc", c => c.DateTime());
            CreateIndex("dbo.ScheduleTask", new[] { "NextRunUtc", "Enabled" }, name: "IX_NextRun_Enabled");
        }
        
        public override void Down()
        {
            DropIndex("dbo.ScheduleTask", "IX_NextRun_Enabled");
            DropColumn("dbo.ScheduleTask", "NextRunUtc");
        }
    }
}
