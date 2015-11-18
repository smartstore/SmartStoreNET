using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using SmartStore.Core.Domain;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Plugins;
using SmartStore.Services.Localization;
using SmartStore.Utilities;

namespace SmartStore.Services.DataExchange
{
	public static class ExportExtensions
	{
		/// <summary>
		/// Returns a value indicating whether the export provider is valid
		/// </summary>
		/// <param name="provider">Export provider</param>
		/// <returns><c>true</c> provider is valid, <c>false</c> provider is invalid.</returns>
		public static bool IsValid(this Provider<IExportProvider> provider)
		{
			return provider != null;
		}

		/// <summary>
		/// Gets the localized friendly name or the system name as fallback
		/// </summary>
		/// <param name="provider">Export provider</param>
		/// <param name="localizationService">Localization service</param>
		/// <returns>Provider name</returns>
		public static string GetName(this Provider<IExportProvider> provider, ILocalizationService localizationService)
		{
			var systemName = provider.Metadata.SystemName;
			var resourceName = provider.Metadata.ResourceKeyPattern.FormatInvariant(systemName, "FriendlyName");
			var name = localizationService.GetResource(resourceName, 0, false, systemName, true);

			return (name.IsEmpty() ? systemName : name);
		}

		/// <summary>
		/// Get temporary folder for an export profile
		/// </summary>
		/// <param name="profile">Export profile</param>
		/// <returns>Folder path</returns>
		public static string GetExportFolder(this ExportProfile profile, bool content = false, bool create = false)
		{
			var path = CommonHelper.MapPath(string.Concat("~/App_Data/ExportProfiles/", profile.FolderName, content ? "/Content" : ""));

			if (create && !System.IO.Directory.Exists(path))
				System.IO.Directory.CreateDirectory(path);

			return path;
		}

		/// <summary>
		/// Get log file path for an export profile
		/// </summary>
		/// <param name="profile">Export profile</param>
		/// <returns>Log file path</returns>
		public static string GetExportLogPath(this ExportProfile profile)
		{
			return Path.Combine(profile.GetExportFolder(), "log.txt");
		}

		/// <summary>
		/// Gets the ZIP path for a profile
		/// </summary>
		/// <param name="profile">Export profile</param>
		/// <returns>ZIP file path</returns>
		public static string GetExportZipPath(this ExportProfile profile)
		{
			return Path.Combine(profile.GetExportFolder(), profile.FolderName + ".zip");
		}

		/// <summary>
		/// Gets existing export files for an export profile
		/// </summary>
		/// <param name="profile">Export profile</param>
		/// <returns>List of file names</returns>
		public static List<string> GetExportFiles(this ExportProfile profile)
		{
			var exportFolder = profile.GetExportFolder(true);

			if (System.IO.Directory.Exists(exportFolder))
			{
				return System.IO.Directory.EnumerateFiles(exportFolder, "*", SearchOption.TopDirectoryOnly)
					.OrderBy(x => x)
					.ToList();
			}

			return new List<string>();
        }

		/// <summary>
		/// Resolves the file name pattern for an export profile
		/// </summary>
		/// <param name="profile">Export profile</param>
		/// <param name="store">Store</param>
		/// <param name="fileIndex">One based file index</param>
		/// <param name="maxFileNameLength">The maximum length of the file name</param>
		/// <returns>Resolved file name pattern</returns>
		public static string ResolveFileNamePattern(this ExportProfile profile, Store store, int fileIndex, int maxFileNameLength)
		{
			var sb = new StringBuilder(profile.FileNamePattern);

			sb.Replace("%Profile.Id%", profile.Id.ToString());
			sb.Replace("%Profile.FolderName%", profile.FolderName);
			sb.Replace("%Store.Id%", store.Id.ToString());
			sb.Replace("%File.Index%", fileIndex.ToString("D4"));

			if (profile.FileNamePattern.Contains("%Profile.SeoName%"))
				sb.Replace("%Profile.SeoName%", SeoHelper.GetSeName(profile.Name, true, false).Replace("/", "").Replace("-", ""));		

			if (profile.FileNamePattern.Contains("%Store.SeoName%"))
				sb.Replace("%Store.SeoName%", profile.PerStore ? SeoHelper.GetSeName(store.Name, true, false) : "allstores");

			if (profile.FileNamePattern.Contains("%Random.Number%"))
				sb.Replace("%Random.Number%", CommonHelper.GenerateRandomInteger().ToString());

			if (profile.FileNamePattern.Contains("%Timestamp%"))
				sb.Replace("%Timestamp%", DateTime.UtcNow.ToString("s", CultureInfo.InvariantCulture));

			var result = sb.ToString()
				.ToValidFileName("")
				.Truncate(maxFileNameLength);

			return result;
		}
	}
}
