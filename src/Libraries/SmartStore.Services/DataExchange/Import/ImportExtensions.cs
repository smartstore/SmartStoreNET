using System.Collections.Generic;
using System.IO;
using System.Linq;
using SmartStore.Core.Data;
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
        public static string GetImportFolder(
            this ImportProfile profile,
            bool content = false,
            bool create = false,
            bool absolutePath = true)
        {
            var path = string.Concat(
                DataSettings.Current.TenantPath,
                "/ImportProfiles/",
                profile.FolderName,
                content ? "/Content" : "");

            if (absolutePath)
            {
                path = CommonHelper.MapPath(path);

                if (create && !System.IO.Directory.Exists(path))
                {
                    System.IO.Directory.CreateDirectory(path);
                }
            }

            return path;
        }

        /// <summary>
        /// Gets import files for an import profile.
        /// </summary>
        /// <param name="profile">Import profile.</param>
        /// <param name="includeRelatedFiles">Whether to include related data files.</param>
        /// <returns>List of import files.</returns>
        public static List<ImportFile> GetImportFiles(this ImportProfile profile, bool includeRelatedFiles = true)
        {
            var result = new List<ImportFile>();
            var importFolder = profile.GetImportFolder(true);

            if (System.IO.Directory.Exists(importFolder))
            {
                var paths = System.IO.Directory.EnumerateFiles(importFolder, "*", SearchOption.TopDirectoryOnly);
                foreach (var path in paths)
                {
                    var file = new ImportFile(path);
                    if (!includeRelatedFiles && file.RelatedType.HasValue)
                    {
                        continue;
                    }

                    result.Add(file);
                }

                // Always main data files first.
                result = result.OrderBy(x => x.RelatedType).ThenBy(x => x.Path).ToList();
                return result;
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
