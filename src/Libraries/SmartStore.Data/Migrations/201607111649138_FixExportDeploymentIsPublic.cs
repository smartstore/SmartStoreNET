namespace SmartStore.Data.Migrations
{
	using System.Data.Entity.Migrations;
	using System.Web.Hosting;
	using Core.Data;

	public partial class FixExportDeploymentIsPublic : DbMigration
    {
        public override void Up()
        {
			if (HostingEnvironment.IsHosted && DataSettings.Current.IsSqlServer)
			{
				Sql("IF EXISTS(SELECT TOP 1 1 FROM sys.objects o INNER JOIN sys.columns c ON o.object_id = c.object_id WHERE o.name = 'ExportDeployment' AND c.name = 'IsPublic') ALTER TABLE [dbo].[ExportDeployment] DROP COLUMN [IsPublic];");
			}
		}

		public override void Down()
        {
        }
    }
}
