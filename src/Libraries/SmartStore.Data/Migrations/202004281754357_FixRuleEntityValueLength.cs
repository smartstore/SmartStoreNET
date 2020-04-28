namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FixRuleEntityValueLength : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Rule", "Value", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Rule", "Value", c => c.String(maxLength: 400));
        }
    }
}
