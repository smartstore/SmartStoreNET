using System.IO;
using System.Linq;
using SmartStore.Core.Domain;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Plugins;
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
			return (
				provider != null &&
				provider.Value.FileExtension.HasValue()
			);
		}

		/// <summary>
		/// Returns a value indicating whether the export provider supports a projection type
		/// </summary>
		/// <param name="provider">Export provider</param>
		/// <param name="type">The type to check</param>
		/// <returns><c>true</c> provider supports type, <c>false</c> provider does not support type.</returns>
		public static bool Supports(this Provider<IExportProvider> provider, ExportProjectionSupport type)
		{
			if (provider != null)
				return provider.Metadata.ExportProjectionSupport.Contains(type);
			return false;
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
	}
}
