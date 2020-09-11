using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using SmartStore.Core.Data;
using SmartStore.Core.IO;
using SmartStore.Core.Logging;
using SmartStore.Utilities;

namespace SmartStore.Core.Plugins
{
    public partial class PluginManager
    {
        #region PluginsHasher

        class PluginsHasher : DirectoryHasher
        {
            private readonly IEnumerable<PluginDescriptor> _plugins;

            public PluginsHasher(IEnumerable<PluginDescriptor> plugins)
                : base(CommonHelper.MapPath(PluginsLocation), null, false, CommonHelper.MapPath(DataSettings.Current.TenantPath))
            {
                _plugins = plugins;
            }

            protected override int ComputeHash()
            {
                var hashCombiner = new HashCodeCombiner();

                // Add each *.dll, Description.txt, web.config to the hash
                var arrPlugins = _plugins.OrderBy(x => x.FolderName).ToArray();
                foreach (var p in arrPlugins)
                {
                    if (p.Assembly.OriginalFile != null)
                    {
                        hashCombiner.Add(p.Assembly.OriginalFile);
                    }

                    p.ReferencedLocalAssemblies.Each(x => hashCombiner.Add(x.OriginalFile));

                    hashCombiner.Add(Path.Combine(p.PhysicalPath, "Description.txt"));
                    hashCombiner.Add(Path.Combine(p.PhysicalPath, "web.config"));
                    hashCombiner.Add(p.Installed);
                }

                return hashCombiner.CombinedHash;
            }
        }

        #endregion

        private static ICollection<string> _lastPluginAssemblies;

        /// <summary>
        /// <c>true</c> when any plugin file has been changed since previous application start.
        /// </summary>
        public static bool PluginChangeDetected { get; private set; }

        /// <summary>
        /// This checks if any of our plugins have changed, if so it will go 
        /// </summary>
        /// <returns>
        /// Returns true if there were changes to plugins and a cleanup was required, otherwise false is returned.
        /// </returns>
        private static bool DetectAndCleanStalePlugins(IEnumerable<PluginDescriptor> plugins, PluginsHasher hasher)
        {
            // Check if anything has been changed, or if we are in debug mode then always perform cleanup
            if (hasher.HasChanged)
            {
                PluginChangeDetected = true;
                Logger.DebugFormat("Plugin changes detected in hash.");

                var lastAssemblies = GetLastPluginsAssemblies();

                // We need to read the old assembly list and clean them out from the shadow copy folder 
                var staleFiles = lastAssemblies
                    .Select(x => new FileInfo(Path.Combine(_shadowCopyDir.FullName, x)))
                    .ToArray();

                // If that fails, then we will try to remove it the hard way by renaming it to .delete if it doesn't delete nicely
                if (staleFiles.Length > 5)
                {
                    staleFiles.AsParallel().ForAll(f => TryDeleteStaleFile(f));
                }
                else
                {
                    staleFiles.Each(f => TryDeleteStaleFile(f));
                }

                return true;
            }

            // No plugin changes found
            return false;
        }

        /// <summary>
        /// Loads the file names of the last shadow copied plugin assemblies from disk
        /// </summary>
        /// <returns></returns>
        private static ICollection<string> GetLastPluginsAssemblies()
        {
            if (_lastPluginAssemblies == null)
            {
                var filePath = Path.Combine(CommonHelper.MapPath(DataSettings.Current.TenantPath), "plugins.assemblies");

                if (!File.Exists(filePath))
                {
                    _lastPluginAssemblies = new List<string>();
                }
                else
                {
                    var list = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                    var file = File.ReadAllText(filePath, Encoding.UTF8);
                    var sr = new StringReader(file);
                    while (true)
                    {
                        var f = sr.ReadLine();
                        if (f != null && f.HasValue())
                        {
                            list.Add(f);
                        }
                        else
                        {
                            break;
                        }
                    }

                    _lastPluginAssemblies = list;
                }
            }

            return _lastPluginAssemblies;
        }

        private static void SavePluginsAssemblies(IEnumerable<PluginDescriptor> plugins)
        {
            var list = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var p in plugins)
            {
                p.ReferencedLocalAssemblies.Each(x => list.Add(x.OriginalFile.Name));

                if (p.Assembly.OriginalFile != null)
                {
                    list.Add(p.Assembly.OriginalFile.Name);
                }
            }

            var sb = new StringBuilder();
            foreach (var f in list)
            {
                sb.AppendLine(f);
            }

            var filePath = Path.Combine(CommonHelper.MapPath(DataSettings.Current.TenantPath), "plugins.assemblies");
            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);

            _lastPluginAssemblies = null;
        }

        /// <summary>
        /// Tries to delete the file. If it doesn't go nicely, we'll force rename it to .delete, if that already exists and we cannot
        /// remove the .delete file then we rename to Guid + .delete
        /// </summary>
        /// <param name="f"></param>
        /// <returns>If it deletes nicely return true, otherwise false</returns>
        private static bool TryDeleteStaleFile(FileInfo f)
        {
            Debug.WriteLine("Trying to delete shadow copied file " + f.FullName);

            // This is a special case: people may have been usign the CodeGen folder before so we need to cleanup those
            // files too, even if we are no longer using it
            if (CleanupDeletePluginFiles(new FileInfo(f.FullName + ".delete")))
                return true;

            try
            {
                f.Delete();
                return true;
            }
            catch { }

            // If the file doesn't delete, then create the .delete file for it, hopefully the BuildManager will clean up next time
            var newName = GetNewDeleteName(f);

            try
            {
                File.Move(f.FullName, newName);
            }
            catch (UnauthorizedAccessException)
            {
                var ex = new UnauthorizedAccessException(string.Format("Access to the path '{0}' is denied, ensure that read, write and modify permissions are allowed.", Path.GetDirectoryName(newName)));
                Logger.Error(ex.Message);
                throw ex;
            }
            catch (IOException)
            {
                Logger.Error(f.FullName + " rename failed, cannot remove stale plugin");
                throw;
            }

            // Make it an empty file
            try
            {
                (new StreamWriter(newName)).Close();
            }
            catch { }

            Logger.Error("Stale plugin " + f.FullName + " failed to cleanup successfully. A .delete file has been created for it.");

            return false;
        }

        /// <summary>
        /// Returns the .delete file name for a particular file. If just the file name + .delete already exists and is locked,
        /// then this returns a Guid + .delete format.
        /// </summary>
        /// <param name="pluginFile"></param>
        /// <returns></returns>
        private static string GetNewDeleteName(FileInfo pluginFile)
        {
            var deleteName = pluginFile.FullName + ".delete";

            try
            {
                if (File.Exists(deleteName))
                    File.Delete(deleteName);
            }
            catch
            {
                Logger.Error("Cannot remove file " + deleteName + ".It is locked, renaming to.delete file with GUID");
                // Cannot delete, so we will need to use a GUID
                deleteName = pluginFile.FullName + Guid.NewGuid().ToString("N") + ".delete";
            }

            return deleteName;
        }

        /// <summary>
        /// When given a .delete file, this will attempt to delete the base file that the .delete was created from
        /// and then the .delete file itself. if both are removed, then it will return true.
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        private static bool CleanupDeletePluginFiles(FileInfo f)
        {
            if (f.Extension != ".delete")
                return false;

            var baseFileName = Path.GetDirectoryName(f.FullName) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(f.FullName);

            // NOTE: this is to remove our old GUID named files... we shouldn't have any of these but sometimes things are just locked
            var base64Files = f.Directory.GetFiles(Path.GetFileName(baseFileName) + "???????????????????????????????????" + ".delete").ToList();
            Func<string, bool> delFile = x =>
            {
                if (x.IsEmpty())
                    return true;

                if (File.Exists(x))
                {
                    try
                    {
                        File.Delete(x);
                    }
                    catch
                    {
                        return false;
                    }
                }
                return true;
            };

            // Always try deleting old guid named files
            foreach (var x in base64Files)
            {
                delFile(x.FullName);
            }

            // Try to delete the .dll file
            if (!delFile(baseFileName))
            {
                return false;
            }

            try
            {
                // Now try to remove the .delete file
                f.Delete();
            }
            catch { }

            return true;
        }
    }
}
