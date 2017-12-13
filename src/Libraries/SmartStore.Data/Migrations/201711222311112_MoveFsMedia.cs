namespace SmartStore.Data.Migrations
{
	using System;
	using System.Data.Entity.Migrations;
	using System.Web.Hosting;
	using System.Linq;
	using System.Collections.Generic;
	using SmartStore.Core.Domain.Configuration;
	using SmartStore.Data.Setup;
	using SmartStore.Utilities;
	using Core.Infrastructure;
	using Core.IO;
	using System.Text.RegularExpressions;
	using System.IO;
	using Core.Data;

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

			// Check whether FS storage provider is active...
			var setting = context.Set<Setting>().FirstOrDefault(x => x.Name == "Media.Storage.Provider");
			if (setting == null || !setting.Value.IsCaseInsensitiveEqual("MediaStorage.SmartStoreFileSystem"))
			{
				// DB provider is active: no need to move anything.
				return;
			}

			// What a huge, fucking hack! > IMediaFileSystem is defined in an
			// assembly which we don't reference from here. But it also implements
			// IFileSystem, which we can cast to.
			var fsType = Type.GetType("SmartStore.Services.Media.IMediaFileSystem, SmartStore.Services");
			var fs = EngineContext.Current.Resolve(fsType) as IFileSystem;

			// Pattern for file matching. E.g. matches 0000234-0.png
			var rg = new Regex(@"^([0-9]{7})-0[.](.{3,4})$", RegexOptions.Compiled | RegexOptions.Singleline);

			var subfolders = new Dictionary<string, string>();

			// Get root files
			var files = fs.ListFiles("").ToList();
			foreach (var chunk in files.Slice(500))
			{
				foreach (var file in chunk)
				{
					var match = rg.Match(file.Name);
					if (match.Success)
					{
						var name = match.Groups[1].Value;
						var ext = match.Groups[2].Value;
						// The new file name without trailing -0
						var newName = string.Concat(name, ".", ext);
						// The subfolder name, e.g. 0024, when file name is 0024893.png
						var dirName = name.Substring(0, DirMaxLength);

						string subfolder = null;
						if (!subfolders.TryGetValue(dirName, out subfolder))
						{
							// Create subfolder "Storage/0000"
							subfolder = fs.Combine("Storage", dirName);
							fs.TryCreateFolder(subfolder);
							subfolders[dirName] = subfolder;
						}

						// Build destination path
						var destinationPath = fs.Combine(subfolder, newName);

						// Move the file now!
						fs.RenameFile(file.Path, destinationPath);
					}
				}
			}
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
