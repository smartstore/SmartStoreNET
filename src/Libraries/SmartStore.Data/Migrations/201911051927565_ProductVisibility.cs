namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    using System.Web.Hosting;
    using SmartStore.Core.Data;

    public partial class ProductVisibility : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Product", "Visibility", c => c.Int(nullable: false));

            if (HostingEnvironment.IsHosted && DataSettings.Current.IsSqlServer)
            {
                var hidden = (int)Core.Domain.Catalog.ProductVisibility.Hidden;
                Sql($"Update [dbo].[Product] Set [Visibility] = {hidden} Where [VisibleIndividually] = 0");
            }
        }
        
        public override void Down()
        {
            DropColumn("dbo.Product", "Visibility");
        }
    }
}
