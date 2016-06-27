using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using SmartStore.Core;
using SmartStore.Core.Domain;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Plugins;
using SmartStore.Services.Localization;
using SmartStore.Utilities;

namespace SmartStore.Services.DataExchange.Export
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
			return provider != null && provider.Value != null;
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
			var path = CommonHelper.MapPath(string.Concat(profile.FolderName, content ? "/Content" : ""));

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
			var name = (new DirectoryInfo(profile.FolderName)).Name;
			if (name.IsEmpty())
				name = "ExportData";

			return Path.Combine(profile.GetExportFolder(), name.ToValidFileName() + ".zip");
		}

		/// <summary>
		/// Gets existing export files for an export profile
		/// </summary>
		/// <param name="profile">Export profile</param>
		/// <param name="provider">Export provider</param>
		/// <returns>List of file names</returns>
		public static IEnumerable<string> GetExportFiles(this ExportProfile profile, Provider<IExportProvider> provider)
		{
			var exportFolder = profile.GetExportFolder(true);

			if (System.IO.Directory.Exists(exportFolder) && provider.Value.FileExtension.HasValue())
			{
				var filter = "*.{0}".FormatInvariant(provider.Value.FileExtension.ToLower());

				return System.IO.Directory.EnumerateFiles(exportFolder, filter, SearchOption.AllDirectories).OrderBy(x => x);
			}

			return Enumerable.Empty<string>();
		}

		/// <summary>
		/// Get number of existing export files
		/// </summary>
		/// <param name="profile">Export profile</param>
		/// <param name="provider">Export provider</param>
		/// <returns>Number of export files</returns>
		public static int GetExportFileCount(this ExportProfile profile, Provider<IExportProvider> provider)
		{
			var result = profile.GetExportFiles(provider).Count();

			if (File.Exists(profile.GetExportZipPath()))
				++result;

			return result;
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

		/// <summary>
		/// Get path of the deployment folder
		/// </summary>
		/// <param name="deployment">Export deployment</param>
		/// <returns>Deployment folder path</returns>
		public static string GetDeploymentFolder(this ExportDeployment deployment, bool create = false)
		{
			if (deployment == null)
				return null;

			string path = null;

			if (deployment.DeploymentType == ExportDeploymentType.PublicFolder)
			{
				if (deployment.SubFolder.HasValue())
					path = Path.Combine(HttpRuntime.AppDomainAppPath, DataExporter.PublicFolder, deployment.SubFolder);
				else
					path = Path.Combine(HttpRuntime.AppDomainAppPath, DataExporter.PublicFolder);
			}
			else if (deployment.DeploymentType == ExportDeploymentType.FileSystem)
			{
				if (deployment.FileSystemPath.IsEmpty())
					return null;

				if (Path.IsPathRooted(deployment.FileSystemPath))
				{
					path = deployment.FileSystemPath;
				}
				else
				{
					path = FileSystemHelper.ValidateRootPath(deployment.FileSystemPath);
					path = CommonHelper.MapPath(path);
				}
			}

			if (create && !System.IO.Directory.Exists(path))
				System.IO.Directory.CreateDirectory(path);

			return path;
		}

		/// <summary>
		/// Get url of the public folder and take filtering and projection into consideration
		/// </summary>
		/// <param name="deployment">Export deployment</param>
		/// <param name="services">Common services</param>
		/// <param name="store">Store entity</param>
		/// <returns>Absolute URL of the public folder (always ends with /) or <c>null</c></returns>
		public static string GetPublicFolderUrl(this ExportDeployment deployment, ICommonServices services, Store store = null)
		{
			if (deployment != null && deployment.DeploymentType == ExportDeploymentType.PublicFolder)
			{
				if (store == null)
				{
					var filter = XmlHelper.Deserialize<ExportFilter>(deployment.Profile.Filtering);
					var storeId = filter.StoreId;

					if (storeId == 0)
					{
						var projection = XmlHelper.Deserialize<ExportProjection>(deployment.Profile.Projection);
						storeId = (projection.StoreId ?? 0);
					}

					store = (storeId == 0 ? services.StoreContext.CurrentStore : services.StoreService.GetStoreById(storeId));
				}

				var url = string.Concat(
					store.Url.EnsureEndsWith("/"),
					DataExporter.PublicFolder.EnsureEndsWith("/"),
					deployment.SubFolder.HasValue() ? deployment.SubFolder.EnsureEndsWith("/") : ""
				);

				return url;
			}

			return null;
		}

		/// <summary>
		/// Get icon class for a deployment type
		/// </summary>
		/// <param name="type">Deployment type</param>
		/// <returns>Icon class</returns>
		public static string GetIconClass(this ExportDeploymentType type)
		{
			switch (type)
			{
				case ExportDeploymentType.FileSystem:
					return "fa-folder-open-o";
				case ExportDeploymentType.Email:
					return "fa-envelope-o";
				case ExportDeploymentType.Http:
					return "fa-globe";
				case ExportDeploymentType.Ftp:
					return "fa-files-o";
				case ExportDeploymentType.PublicFolder:
					return "fa-unlock";
				default:
					return "fa-question";
			}
		}
	}
}
