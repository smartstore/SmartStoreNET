using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Caching;
using SmartStore.Core.Domain;
using SmartStore.Core.Domain.DataExchange;
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
			return (provider != null);
		}

		/// <summary>
		/// Returns a value indicating whether the export provider supports a projection type
		/// </summary>
		/// <param name="provider">Export provider</param>
		/// <param name="type">The type to check</param>
		/// <returns><c>true</c> provider supports type, <c>false</c> provider does not support type.</returns>
		public static bool Supports(this Provider<IExportProvider> provider, ExportSupport type)
		{
			if (provider != null)
				return provider.Metadata.ExportSupport.Contains(type);
			return false;
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
		public static string GetExportFolder(this ExportProfile profile, bool content = false)
		{
			var path = Path.Combine(FileSystemHelper.TempDir(), @"Profile\Export\{0}{1}".FormatInvariant(profile.FolderName, content ? @"\Content" : ""));
			return path;
		}

		/// <summary>
		/// Get log file path for an export profile
		/// </summary>
		/// <param name="profile">Export profile</param>
		/// <returns>Log file path</returns>
		public static string GetExportLogFilePath(this ExportProfile profile)
		{
			var path = Path.Combine(profile.GetExportFolder(), "log.txt");
			return path;
		}

		/// <summary>
		/// Resolves the file name pattern for an export profile
		/// </summary>
		/// <param name="profile">Export profile</param>
		/// <param name="store">Store</param>
		/// <param name="fileNumber">File number</param>
		/// <param name="maxFileNameLength">The maximum length of the file name</param>
		/// <returns>Resolved file name pattern</returns>
		public static string ResolveFileNamePattern(this ExportProfile profile, Store store, int fileNumber, int maxFileNameLength)
		{
			var result = profile.FileNamePattern
				.Replace("%ExportProfile.Id%", profile.Id.ToString())
				.Replace("%ExportProfile.SeoName%", SeoHelper.GetSeName(profile.Name, true, false).Replace("/", "").Replace("-", ""))
				.Replace("%ExportProfile.FolderName%", profile.FolderName)
				.Replace("%Store.Id%", store.Id.ToString())
				.Replace("%Store.SeoName%", profile.PerStore ? SeoHelper.GetSeName(store.Name, true, false) : "allstores")
				.Replace("%Misc.FileNumber%", fileNumber.ToString("D4"))
				.ToValidFileName("")
				.Truncate(maxFileNameLength);

			return result;
		}

		/// <summary>
		/// Gets the cache key for selected entity identifiers
		/// </summary>
		/// <param name="profile">Export profile</param>
		/// <returns>Cache key for selected entity identifiers</returns>
		public static string GetSelectedEntityIdsCacheKey(this ExportProfile profile)
		{
			// do not use profile.Id because it can be 0
			return "ExportProfile_SelectedEntityIds_" + profile.ProviderSystemName;
		}

		/// <summary>
		/// Caches selected entity identifiers to be considered during following export
		/// </summary>
		/// <param name="profile">Export profile</param>
		/// <param name="selectedIds">Comma separated entity identifiers</param>
		public static void CacheSelectedEntityIds(this ExportProfile profile, string selectedIds)
		{
			var selectedIdsCacheKey = profile.GetSelectedEntityIdsCacheKey();

			if (selectedIds.HasValue())
				HttpRuntime.Cache.Add(selectedIdsCacheKey, selectedIds, null, DateTime.UtcNow.AddMinutes(5), Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
			else
				HttpRuntime.Cache.Remove(selectedIdsCacheKey);
		}
	}
}
