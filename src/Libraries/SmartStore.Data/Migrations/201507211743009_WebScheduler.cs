namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class WebScheduler : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ScheduleTask", "Alias", c => c.String(maxLength: 4000));
            AddColumn("dbo.ScheduleTask", "NextRunUtc", c => c.DateTime());
            AddColumn("dbo.ScheduleTask", "IsHidden", c => c.Boolean(nullable: false));
            CreateIndex("dbo.ScheduleTask", new[] { "NextRunUtc", "Enabled" }, name: "IX_NextRun_Enabled");
        }
        
        public override void Down()
        {
            DropIndex("dbo.ScheduleTask", "IX_NextRun_Enabled");
            DropColumn("dbo.ScheduleTask", "IsHidden");
            DropColumn("dbo.ScheduleTask", "NextRunUtc");
            DropColumn("dbo.ScheduleTask", "Alias");
        }
    }
}
