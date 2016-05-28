namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CustomerTablePerf : DbMigration
    {
        public override void Up()
        {
			// without dropping the indexes we cannot adjust column lengths
			DropIndex("dbo.Customer", "IX_Customer_Email");
			DropIndex("dbo.Customer", "IX_Customer_Username");
			
			AlterColumn("dbo.Customer", "Username", c => c.String(maxLength: 500));
            AlterColumn("dbo.Customer", "Email", c => c.String(maxLength: 500));
            AlterColumn("dbo.Customer", "Password", c => c.String(maxLength: 500));
            AlterColumn("dbo.Customer", "PasswordSalt", c => c.String(maxLength: 500));
            AlterColumn("dbo.Customer", "LastIpAddress", c => c.String(maxLength: 100));

			// recreate previously dropped indexes
			CreateIndex("dbo.Customer", "Email", name: "IX_Customer_Email");
			CreateIndex("dbo.Customer", "Username", name: "IX_Customer_Username");
        }
        
        public override void Down()
        {
			//// INFO: (mc) Unnecessary
			//AlterColumn("dbo.Customer", "LastIpAddress", c => c.String());
			//AlterColumn("dbo.Customer", "PasswordSalt", c => c.String());
			//AlterColumn("dbo.Customer", "Password", c => c.String());
			//AlterColumn("dbo.Customer", "Email", c => c.String(maxLength: 1000));
			//AlterColumn("dbo.Customer", "Username", c => c.String(maxLength: 1000));
        }
    }
}
