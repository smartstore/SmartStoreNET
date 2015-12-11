using System.Collections.Generic;
using System.IO;
using System.Linq;
using SmartStore.Core.Domain;
using SmartStore.Utilities;

namespace SmartStore.Services.DataExchange.Import
{
	public static class ImportExtensions
	{
		/// <summary>
		/// Get folder for import files
		/// </summary>
		/// <param name="profile">Import profile</param>
		/// <returns>Folder path</returns>
		public static string GetImportFolder(this ImportProfile profile, bool content = false, bool create = false)
		{
			var path = CommonHelper.MapPath(string.Concat("~/App_Data/ImportProfiles/", profile.FolderName, content ? "/Content" : ""));

			if (create && !System.IO.Directory.Exists(path))
				System.IO.Directory.CreateDirectory(path);

			return path;
		}

		/// <summary>
		/// Gets import files for an import profile
		/// </summary>
		/// <param name="profile">Import profile</param>
		/// <returns>List of file paths</returns>
		public static List<string> GetImportFiles(this ImportProfile profile)
		{
			var result = new List<string>();
			var folder = profile.GetImportFolder(true);

			if (System.IO.Directory.Exists(folder))
			{
				var initialImportFile = Path.Combine(folder, profile.FileName);

				result.Add(initialImportFile);

				result.AddRange(System.IO.Directory.EnumerateFiles(folder, "*", SearchOption.TopDirectoryOnly)
					.Where(x => !x.IsCaseInsensitiveEqual(initialImportFile))
					.OrderBy(x => x)
					.ToList());
			}

			return result;
		}

		/// <summary>
		/// Get log file path for an import profile
		/// </summary>
		/// <param name="profile">Import profile</param>
		/// <returns>Log file path</returns>
		public static string GetImportLogPath(this ImportProfile profile)
		{
			return Path.Combine(profile.GetImportFolder(), "log.txt");
		}
	}
}
