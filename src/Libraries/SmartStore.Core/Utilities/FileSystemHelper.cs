using System;
using System.IO;
using System.Linq;
using SmartStore.Core.Data;

namespace SmartStore.Utilities
{
    public static class FileSystemHelper
    {
        private static readonly string _pathTemp = CommonHelper.MapPath(CommonHelper.GetAppSetting("sm:TempDirectory", "~/App_Data/_temp"));
        private static readonly string _pathTempTenant = CommonHelper.MapPath(DataSettings.Current.TenantPath + "/_temp");

        /// <summary>
        /// Returns physical path to application temp directory
        /// </summary>
        /// <param name="subDirectory">Name of a sub directory to be created and returned (optional)</param>
        public static string TempDir(string subDirectory = null)
        {
            return TempDirInternal(_pathTemp, subDirectory);
        }

        /// <summary>
        /// Returns physical path to current tenant temp directory
        /// </summary>
        /// <param name="subDirectory">Name of a sub directory to be created and returned (optional)</param>
        public static string TempDirTenant(string subDirectory = null)
        {
            return TempDirInternal(_pathTempTenant, subDirectory);
        }

        private static string TempDirInternal(string path, string subDirectory = null)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            if (subDirectory.HasValue())
            {
                path = Path.Combine(path, subDirectory);

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }

            return path;
        }

        /// <summary>
        /// Safe way to cleanup temporary directories. Should be called via scheduled task.
        /// </summary>
        public static void ClearTempDirectories()
        {
            var olderThan = TimeSpan.FromHours(5);

            ClearDirectory(new DirectoryInfo(TempDir()), false, olderThan);
            ClearDirectory(new DirectoryInfo(TempDirTenant()), false, olderThan);
        }

        /// <summary>
        /// Safe way to delete all directory content
        /// </summary>
        /// <param name="path">Directory path</param>
        /// <param name="deleteIfEmpfy">Delete dir too if it doesn't contain any entries after deletion anymore</param>
        /// <param name="olderThan">Delete only files older than this TimeSpan</param>
        /// <param name="exceptFileNames">Name of files not to be deleted</param>
        public static void ClearDirectory(string path, bool deleteIfEmpfy, TimeSpan olderThan, params string[] exceptFileNames)
        {
            if (path.IsEmpty())
                return;

            ClearDirectory(new DirectoryInfo(path), deleteIfEmpfy, olderThan, exceptFileNames);
        }

        /// <summary>
        /// Safe way to delete all directory content
        /// </summary>
        /// <param name="path">Directory path</param>
        /// <param name="deleteIfEmpfy">Delete dir too if it doesn't contain any entries after deletion anymore</param>
        /// <param name="exceptFileNames">Name of files not to be deleted</param>
        public static void ClearDirectory(string path, bool deleteIfEmpfy, params string[] exceptFileNames)
        {
            if (path.IsEmpty())
                return;

            ClearDirectory(new DirectoryInfo(path), deleteIfEmpfy, TimeSpan.Zero, exceptFileNames);
        }

        /// <summary>
        /// Safe way to delete all directory content
        /// </summary>
        /// <param name="dir">Directory info object</param>
        /// <param name="deleteIfEmpfy">Delete dir too if it doesn't contain any entries after deletion anymore</param>
        /// <param name="exceptFileNames">Name of files not to be deleted</param>
        public static void ClearDirectory(DirectoryInfo dir, bool deleteIfEmpfy, params string[] exceptFileNames)
        {
            Guard.NotNull(dir, nameof(dir));

            ClearDirectory(dir, deleteIfEmpfy, TimeSpan.Zero, exceptFileNames);
        }

        /// <summary>
        /// Safe way to delete all directory content
        /// </summary>
        /// <param name="dir">Directory info object</param>
        /// <param name="deleteIfEmpfy">Delete dir too if it doesn't contain any entries after deletion anymore</param>
        /// <param name="olderThan">Delete only files older than this TimeSpan</param>
        /// <param name="exceptFileNames">Name of files not to be deleted</param>
        public static void ClearDirectory(DirectoryInfo dir, bool deleteIfEmpfy, TimeSpan olderThan, params string[] exceptFileNames)
        {
            Guard.NotNull(dir, nameof(dir));

            if (!dir.Exists)
                return;

            var olderThanDate = DateTime.UtcNow.Subtract(olderThan);

            for (int i = 0; i < 10; i++)
            {
                try
                {
                    foreach (var fsi in dir.EnumerateFileSystemInfos())
                    {
                        if (fsi is FileInfo file)
                        {
                            if (file.LastWriteTimeUtc >= olderThanDate)
                                continue;

                            if (exceptFileNames.Any(x => x.IsCaseInsensitiveEqual(file.Name)))
                                continue;

                            if (file.IsReadOnly)
                            {
                                file.IsReadOnly = false;
                            }

                            file.Delete();
                        }
                        else if (fsi is DirectoryInfo subDir)
                        {
                            ClearDirectory(subDir, true, olderThan, exceptFileNames);
                        }

                    }

                    break;
                }
                catch (Exception ex)
                {
                    ex.Dump();
                }
            }

            if (deleteIfEmpfy)
            {
                try
                {
                    if (!dir.EnumerateFileSystemInfos().Any())
                    {
                        dir.Delete(true);
                    }
                }
                catch (Exception ex)
                {
                    ex.Dump();
                }
            }
        }

        /// <summary>
        /// Safe way to delete a file.
        /// </summary>
        public static bool DeleteFile(string path)
        {
            if (path.IsEmpty())
                return true;

            try
            {
                if (Directory.Exists(path))
                {
                    throw new UnauthorizedAccessException("Deleting folders not possible due to security reasons: {0}".FormatWith(path));
                }

                // No exception, if file doesn't exists
                File.Delete(path);
                return true;
            }
            catch (Exception ex)
            {
                ex.Dump();
                return false;
            }
        }

        /// <summary>
        /// Safe way to copy a file.
        /// </summary>
        public static bool CopyFile(string sourcePath, string destinationPath, bool overwrite = true, bool deleteSource = false)
        {
            bool result = true;
            try
            {
                File.Copy(sourcePath, destinationPath, overwrite);

                if (deleteSource)
                    DeleteFile(sourcePath);
            }
            catch (Exception exc)
            {
                result = false;
                exc.Dump();
            }

            return result;
        }

        /// <summary>
        /// Safe way to copy a directory and all content.
        /// </summary>
        /// <param name="source">Source directory</param>
        /// <param name="target">Target directory</param>
        /// <param name="overwrite">Whether to overwrite existing files</param>
        public static bool CopyDirectory(DirectoryInfo source, DirectoryInfo target, bool overwrite = true)
        {
            if (target.FullName.EnsureEndsWith("\\").StartsWith(source.FullName.EnsureEndsWith("\\"), StringComparison.CurrentCultureIgnoreCase))
            {
                // Cannot copy a folder into itself.
                return false;
            }

            var result = true;

            foreach (FileInfo fi in source.GetFiles())
            {
                try
                {
                    fi.CopyTo(Path.Combine(target.ToString(), fi.Name), overwrite);
                }
                catch (Exception ex)
                {
                    result = false;
                    ex.Dump();
                }
            }

            foreach (DirectoryInfo sourceSubDir in source.GetDirectories())
            {
                try
                {
                    DirectoryInfo targetSubDir = target.CreateSubdirectory(sourceSubDir.Name);
                    CopyDirectory(sourceSubDir, targetSubDir, overwrite);
                }
                catch (Exception ex)
                {
                    result = false;
                    ex.Dump();
                }
            }

            return result;
        }

        /// <summary>
        /// Creates a non existing directory name
        /// </summary>
        /// <param name="directoryPath">Path of a directory</param>
        /// <param name="defaultName">Default name for directory. <c>null</c> to use a guid.</param>
        /// <returns>Non existing directory name</returns>
        public static string CreateNonExistingDirectoryName(string directoryPath, string defaultName)
        {
            if (defaultName.IsEmpty())
                defaultName = Guid.NewGuid().ToString();

            if (directoryPath.IsEmpty() || !Directory.Exists(directoryPath))
                return defaultName;

            var newName = defaultName;

            for (int i = 1; i < 999999 && Directory.Exists(Path.Combine(directoryPath, newName)); ++i)
            {
                newName = defaultName + i.ToString();
            }

            return newName;
        }

        /// <summary>
        /// Safe way to count files in a directory
        /// </summary>
        /// <param name="directoryPath">A directory path</param>
        /// <returns>File count</returns>
        public static int CountFiles(string directoryPath)
        {
            try
            {
                return Directory.GetFiles(directoryPath).Length;
            }
            catch { }

            return 0;
        }

        /// <summary>
        /// Safe way to empty a file
        /// </summary>
        /// <param name="path">File path</param>
        public static void ClearFile(string path)
        {
            try
            {
                if (path.HasValue()) File.WriteAllText(path, "");
            }
            catch { }
        }
    }
}
