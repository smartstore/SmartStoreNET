namespace SmartStore.Data.Migrations
{
	using System;
	using System.Data.Entity.Migrations;
	using System.Web.Hosting;
	using SmartStore.Data.Setup;
	using SmartStore.Utilities;
	using System.IO;
	using Core.Data;
	using SmartStore.Data.Utilities;

	public partial class MoveFsMedia : DbMigration, IDataSeeder<SmartObjectContext>
	{
		const int DirMaxLength = 4;

		public override void Up()
        {
        }
        
        public override void Down()
        {
        }

		public bool RollbackOnFailure
		{
			get { return true; }
		}

		public void Seed(SmartObjectContext context)
		{
			if (!HostingEnvironment.IsHosted)
				return;

			// Move the whole media folder to new location at first
			MoveMediaFolder();

			// Reorganize files (root > Storage/{subfolder})
			DataMigrator.MoveFsMedia(context);
		}

		private void MoveMediaFolder()
		{
			// Moves "~/Media/{Tenant}" to "~/App_Data/Tenants/{Tenant}/Media"
			// This is relevant for local file system only. For cloud storages (like Azure)
			// we don't need to move the main folder.

			var sourceDir = new DirectoryInfo(CommonHelper.MapPath("~/Media/" + DataSettings.Current.TenantName, false));
			var destinationDir = new DirectoryInfo(CommonHelper.MapPath("~/App_Data/Tenants/" + DataSettings.Current.TenantName + "/Media", false));

			if (!sourceDir.Exists)
			{
				// Source (legacy media folder) does not exist, for whatever reasons. Nothing to move here.
				return;
			}

			//if (!destinationDir.Exists)
			//{
			//	destinationDir.Create();
			//}

			sourceDir.MoveTo(destinationDir.FullName);
		}
	}
}
