namespace SmartStore.Data.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class TaskHistoryErrorLength : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.ScheduleTaskHistory", "Error", c => c.String());
        }

        public override void Down()
        {
            AlterColumn("dbo.ScheduleTaskHistory", "Error", c => c.String(maxLength: 1000));
        }
    }
}
