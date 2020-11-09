using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Web;
using System.Web.Compilation;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using SmartStore.Core.Data;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Infrastructure.DependencyManagement;
using SmartStore.Core.Logging;
using SmartStore.Core.Packaging;
using SmartStore.Core.Plugins;
using SmartStore.Utilities;

// Contributor: Umbraco (http://www.umbraco.com). Thanks a lot!
// SEE THIS POST for full details of what this does
//http://shazwazza.com/post/Developing-a-plugin-framework-in-ASPNET-with-medium-trust.aspx

[assembly: PreApplicationStartMethod(typeof(PluginManager), "Initialize")]
namespace SmartStore.Core.Plugins
{
    /// <summary>
    /// Sets the application up for plugin referencing
    /// </summary>
    public partial class PluginManager
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetDllDirectory(string lpPathName);

        private static readonly object _lock = new object();
        private static DirectoryInfo _shadowCopyDir;
        private static readonly ConcurrentDictionary<string, PluginDescriptor> _referencedPlugins = new ConcurrentDictionary<string, PluginDescriptor>(StringComparer.OrdinalIgnoreCase);
        private static readonly HashSet<Assembly> _inactiveAssemblies = new HashSet<Assembly>();

        private static ILogger Logger { get; set; } = NullLogger.Instance;

        private static IDisposable ActivateLogger()
        {
            var logger = new TraceLogger();
            Logger = logger;
            return new ActionDisposable(() =>
            {
                logger.Dispose();
                Logger = NullLogger.Instance;
            });
        }

        /// <summary>
        /// Returns the virtual path of the plugins folder relative to the application
        /// </summary>
        public static string PluginsLocation { get; } = "~/Plugins";

        /// <summary> 
        /// Returns a collection of all referenced plugin assemblies that have been shadow copied
        /// </summary>
        public static IEnumerable<PluginDescriptor> ReferencedPlugins
        {
            get => _referencedPlugins.Values;
            // for unit testing purposes
            internal set
            {
                foreach (var x in value)
                {
                    if (!_referencedPlugins.ContainsKey(x.SystemName))
                    {
                        _referencedPlugins[x.SystemName] = x;
                    }
                }
            }
        }

        /// <summary>
        /// Returns a collection of all plugins which are not compatible with the current version
        /// </summary>
        public static IEnumerable<string> IncompatiblePlugins
        {
            get;
            // for unit testing purposes
            internal set;
        }

        /// <summary>
        /// Initialize
        /// </summary>
        public static void Initialize()
        {
            using (ActivateLogger())
            {
                Logger.Debug("===================== APPINIT START");
                InitializeCore();
                Logger.Debug("===================== APPINIT END");
            }
        }

        private static void InitializeCore()
        {
            var isFullTrust = WebHelper.GetTrustLevel() == AspNetHostingPermissionLevel.Unrestricted;
            if (!isFullTrust)
            {
                throw new ApplicationException("Smartstore requires Full Trust mode. Please enable Full Trust for your web site or contact your hosting provider.");
            }

            using (var updater = new AppUpdater())
            {
                // update from NuGet package, if it exists and is valid
                if (updater.TryUpdateFromPackage())
                {
                    // [...]
                }

                // execute migrations
                updater.ExecuteMigrations();
            }

            // adding a process-specific environment path (either bin/x86 or bin/x64)
            // ensures that unmanaged native dependencies can be resolved successfully.
            SetNativeDllPath();

            DynamicModuleUtility.RegisterModule(typeof(AutofacRequestLifetimeHttpModule));

            var incompatiblePlugins = (new HashSet<string>(StringComparer.OrdinalIgnoreCase)).AsSynchronized();
            var inactiveAssemblies = _inactiveAssemblies.AsSynchronized();
            var dirty = false;

            var watch = Stopwatch.StartNew();

            _shadowCopyDir = new DirectoryInfo(AppDomain.CurrentDomain.DynamicDirectory);

            var plugins = LoadPluginDescriptors().ToArray();
            var compatiblePlugins = plugins.Where(x => !x.Incompatible).ToArray();
            var hasher = new PluginsHasher(compatiblePlugins);

            Logger.DebugFormat("Loaded plugin descriptors. {0} total, {1} incompatible.", plugins.Length, plugins.Length - compatiblePlugins.Length);

            var ms = watch.ElapsedMilliseconds;
            Logger.DebugFormat("INIT PLUGINS (LoadPluginDescriptors). Time elapsed: {0} ms.", ms);

            // If plugins state is dirty, we copy files over to the dynamic folder,
            // otherwise we just reference the previously copied file.
            dirty = DetectAndCleanStalePlugins(compatiblePlugins, hasher);

            // Perf: Initialize/probe all plugins in parallel
            plugins.AsParallel().ForAll(x =>
            {
                // Deploy to ASP.NET dynamic folder.
                DeployPlugin(x, dirty);

                // Finalize
                FinalizePlugin(x);
            });

            //// Retry when failed, because during parallel execution assembly loading MAY fail.
            //// Therefore we retry initialization for failed plugins, but sequentially this time.
            //foreach (var p in plugins)
            //{
            //	// INFO: this seems redundant, but it's ok: 
            //	// DeployPlugin() only probes assemblies that are not loaded yet.
            //	DeployPlugin(p, dirty);

            //	// Finalize
            //	FinalizePlugin(p);
            //}

            if (dirty && DataSettings.DatabaseIsInstalled())
            {
                // Save current hash of all deployed plugins to disk
                hasher.Persist();

                // Save names of all deployed assemblies to disk (so we can nuke them later)
                SavePluginsAssemblies(_referencedPlugins.Values);
            }

            IncompatiblePlugins = incompatiblePlugins.AsReadOnly();

            ms = watch.ElapsedMilliseconds;
            Logger.DebugFormat("INIT PLUGINS (Deployment complete). Time elapsed: {0} ms.", ms);

            void DeployPlugin(PluginDescriptor p, bool shadowCopy)
            {
                if (p.Incompatible)
                {
                    // Do nothing if plugin is incompatible
                    return;
                }

                // First copy referenced local assemblies (if any)
                for (int i = 0; i < p.ReferencedLocalAssemblies.Length; ++i)
                {
                    var refAr = p.ReferencedLocalAssemblies[i];
                    if (refAr.Assembly == null)
                    {
                        Probe(refAr, p, shadowCopy);
                    }
                }

                // Then copy main plugin assembly
                var ar = p.Assembly;
                if (ar.Assembly == null)
                {
                    Probe(ar, p, shadowCopy);
                    if (ar.Assembly != null)
                    {
                        // Activate (even if uninstalled): Find IPlugin, IPreApplicationStart, IConfigurable etc.
                        ActivatePlugin(p);
                    }
                }
            }

            void FinalizePlugin(PluginDescriptor p)
            {
                _referencedPlugins[p.SystemName] = p;

                if (p.Incompatible)
                {
                    incompatiblePlugins.Add(p.SystemName);
                    return;
                }

                var firstFailedAssembly = p.ReferencedLocalAssemblies.FirstOrDefault(x => x.ActivationException != null);
                if (firstFailedAssembly == null && p.Assembly.ActivationException != null)
                {
                    firstFailedAssembly = p.Assembly;
                }
                if (firstFailedAssembly != null)
                {
                    Logger.ErrorFormat("Assembly probing failed for '{0}': {1}", firstFailedAssembly.File?.Name.EmptyNull(), firstFailedAssembly.ActivationException.Message);
                    p.Incompatible = true;
                    incompatiblePlugins.Add(p.SystemName);
                }

                if ((!p.Installed || firstFailedAssembly != null) && p.Assembly.Assembly != null)
                {
                    inactiveAssemblies.Add(p.Assembly.Assembly);
                }
            }
        }

        /// <summary>
        /// Loads and parses the descriptors of all installed plugins
        /// </summary>
        /// <returns>All descriptors</returns>
        private static IEnumerable<PluginDescriptor> LoadPluginDescriptors()
        {
            // TODO: Add verbose exception handling / raising here since this is happening on app startup and could
            // prevent app from starting altogether

            var pluginsDir = new DirectoryInfo(CommonHelper.MapPath(PluginsLocation));

            if (!pluginsDir.Exists)
            {
                pluginsDir.Create();
                return Enumerable.Empty<PluginDescriptor>();
            }

            // Determine all plugin folders: ~/Plugins/{SystemName}
            var allPluginDirs = pluginsDir.EnumerateDirectories().ToArray()
                .Where(x => !x.Name.IsMatch("bin") && !x.Name.IsMatch("_Backup"))
                .OrderBy(x => x.Name)
                .ToArray();

            var installedPluginSystemNames = PluginFileParser.ParseInstalledPluginsFile().AsSynchronized();

            // Load/activate all plugins
            return allPluginDirs
                .AsParallel()
                .AsOrdered()
                .Select(d => LoadPluginDescriptor(d, installedPluginSystemNames))
                .Where(x => x != null);
        }

        private static PluginDescriptor LoadPluginDescriptor(DirectoryInfo d, ICollection<string> installedPluginSystemNames)
        {
            var descriptionFile = new FileInfo(Path.Combine(d.FullName, "Description.txt"));
            if (!descriptionFile.Exists)
            {
                return null;
            }

            // Load descriptor file (Description.txt)
            var descriptor = PluginFileParser.ParsePluginDescriptionFile(descriptionFile.FullName);

            // Some validation
            if (descriptor.SystemName.IsEmpty())
            {
                throw new SmartException("The plugin descriptor '{0}' does not define a plugin system name. Try assigning the plugin a unique name and recompile.".FormatInvariant(descriptionFile.FullName));
            }

            if (descriptor.PluginFileName.IsEmpty())
            {
                throw new SmartException("The plugin descriptor '{0}' does not define a plugin assembly file name. Try assigning the plugin a file name and recompile.".FormatInvariant(descriptionFile.FullName));
            }

            descriptor.VirtualPath = PluginsLocation + "/" + descriptor.FolderName;

            // Set 'Installed' property
            descriptor.Installed = installedPluginSystemNames.Contains(descriptor.SystemName);

            // Ensure that version of plugin is valid
            if (!IsAssumedCompatible(descriptor))
            {
                // Set 'Incompatible' property and return
                descriptor.Incompatible = true;
                return descriptor;
            }

            var skipDlls = new HashSet<string>(new[] { "log4net.dll" }, StringComparer.OrdinalIgnoreCase);

            // Get list of all DLLs in plugin folders (not in 'bin' or '_Backup'!)
            var pluginBinaries = descriptionFile.Directory.EnumerateFiles("*.dll", SearchOption.AllDirectories).ToArray()
                .Where(x => IsPackagePluginFolder(x.Directory) && !skipDlls.Contains(x.Name))
                .OrderBy(x => x.Name)
                .ToDictionarySafe(x => x.Name, StringComparer.OrdinalIgnoreCase);

            // Set 'OriginalAssemblyFile' property
            descriptor.Assembly.OriginalFile = pluginBinaries.Get(descriptor.PluginFileName);

            if (descriptor.Assembly.OriginalFile == null)
            {
                throw new SmartException("The main assembly '{0}' for plugin '{1}' could not be found.".FormatInvariant(descriptor.PluginFileName, descriptor.SystemName));
            }

            // Load all other referenced local assemblies now
            var otherAssemblyFiles = pluginBinaries
                .Where(x => !x.Key.IsCaseInsensitiveEqual(descriptor.PluginFileName))
                .Select(x => x.Value);

            descriptor.ReferencedLocalAssemblies = otherAssemblyFiles.Select(x => new AssemblyReference { OriginalFile = x }).ToArray();

            return descriptor;
        }

        private static void ActivatePlugin(PluginDescriptor plugin)
        {
            // Init plugin type (only one plugin per assembly is allowed)
            bool pluginFound = false;
            bool preStarterFound = !plugin.Installed;
            var exportedTypes = plugin.Assembly.Assembly.GetExportedTypes();

            foreach (var t in exportedTypes)
            {
                if (typeof(IPlugin).IsAssignableFrom(t) && !t.IsInterface && t.IsClass && !t.IsAbstract)
                {
                    plugin.PluginClrType = t;
                    plugin.IsConfigurable = typeof(IConfigurable).IsAssignableFrom(t);
                    pluginFound = true;
                }
                else if (plugin.Installed && typeof(IPreApplicationStart).IsAssignableFrom(t) && !t.IsInterface && t.IsClass && !t.IsAbstract && t.HasDefaultConstructor())
                {
                    try
                    {
                        var preStarter = Activator.CreateInstance(t) as IPreApplicationStart;
                        preStarter.Start();
                    }
                    catch { }
                    preStarterFound = true;
                }

                if (pluginFound && preStarterFound)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Mark plugin as installed
        /// </summary>
        /// <param name="systemName">Plugin system name</param>
        public static void MarkPluginAsInstalled(string systemName)
        {
            Guard.NotEmpty(systemName, nameof(systemName));

            var installedPluginSystemNames = GetInstalledPluginNames();

            bool installed = installedPluginSystemNames.Contains(systemName);
            if (!installed)
            {
                installedPluginSystemNames.Add(systemName);
            }

            PluginFileParser.SaveInstalledPluginsFile(installedPluginSystemNames);
        }

        /// <summary>
        /// Mark plugin as uninstalled
        /// </summary>
        /// <param name="systemName">Plugin system name</param>
        public static void MarkPluginAsUninstalled(string systemName)
        {
            Guard.NotEmpty(systemName, nameof(systemName));

            var installedPluginSystemNames = GetInstalledPluginNames();
            bool installed = installedPluginSystemNames.Contains(systemName);
            if (installed)
            {
                installedPluginSystemNames.Remove(systemName);
            }

            PluginFileParser.SaveInstalledPluginsFile(installedPluginSystemNames);
        }

        /// <summary>
        /// Mark plugin as uninstalled
        /// </summary>
        public static void MarkAllPluginsAsUninstalled()
        {
            var filePath = PluginFileParser.InstalledPluginsFilePath;
            if (File.Exists(filePath))
                File.Delete(filePath);
        }

        private static ICollection<string> GetInstalledPluginNames()
        {
            var filePath = PluginFileParser.InstalledPluginsFilePath;
            if (!File.Exists(filePath))
            {
                using (File.Create(filePath))
                {
                    // We use 'using' to close the file after it's created
                }
            }

            var installedPluginSystemNames = PluginFileParser.ParseInstalledPluginsFile();
            return installedPluginSystemNames;
        }

        /// <summary>
        /// Gets a value indicating whether a plugin is assumed
        /// to be compatible with the current app version
        /// </summary>
        /// <remarks>
        /// A plugin is generally compatible when both app version and plugin's 
        /// <c>MinorAppVersion</c> are equal, OR - when app version is greater - it is 
        /// assumed to be compatible when no breaking changes occured since <c>MinorAppVersion</c>.
        /// </remarks>
        /// <param name="descriptor">The plugin to check</param>
        /// <returns><c>true</c> when the plugin is assumed to be compatible</returns>
        public static bool IsAssumedCompatible(PluginDescriptor descriptor)
        {
            Guard.NotNull(descriptor, nameof(descriptor));

            return IsAssumedCompatible(descriptor.MinAppVersion);
        }

        /// <summary>
        /// Gets a value indicating whether the given min. required app version is assumed
        /// to be compatible with the current app version
        /// </summary>
        /// <remarks>
        /// A plugin is generally compatible when both app version and plugin's 
        /// <c>MinorAppVersion</c> are equal, OR - when app version is greater - it is 
        /// assumed to be compatible when no breaking changes occured since <c>MinorAppVersion</c>.
        /// </remarks>
        /// <param name="minAppVersion">The min. app version to check for</param>
        /// <returns><c>true</c> when the extension's version is assumed to be compatible</returns>
        public static bool IsAssumedCompatible(Version minAppVersion)
        {
            Guard.NotNull(minAppVersion, nameof(minAppVersion));

            if (SmartStoreVersion.Version == minAppVersion)
            {
                return true;
            }

            if (SmartStoreVersion.Version < minAppVersion)
            {
                return false;
            }

            bool compatible = true;

            foreach (var version in SmartStoreVersion.BreakingChangesHistory)
            {
                if (version > minAppVersion)
                {
                    // there was a breaking change in a version greater
                    // than plugin's MinorAppVersion.
                    compatible = false;
                    break;
                }

                if (version <= minAppVersion)
                {
                    break;
                }
            }

            return compatible;
        }

        /// <summary>
        /// Gets a value indicating whether a plugin
        /// is registered and installed.
        /// </summary>
        /// <param name="systemName">The system name of the plugin to check for</param>
        /// <returns><c>true</c> if the plugin exists, <c>false</c> otherwise</returns>
        public static bool PluginExists(string systemName)
        {
            Guard.NotEmpty(systemName, nameof(systemName));
            return _referencedPlugins.ContainsKey(systemName);
        }

        /// <summary>
        /// Gets a value indicating whether the plugin assembly
        /// was properly installed and is active.
        /// </summary>
        /// <param name="assembly">The assembly to check for</param>
        /// <returns><c>true</c> when the assembly is installed and active</returns>
        public static bool IsActivePluginAssembly(Assembly assembly)
        {
            return !_inactiveAssemblies.Contains(assembly);
        }

        private static void SetPrivateEnvPath()
        {
            string envPath = Environment.GetEnvironmentVariable("PATH");

            if (Environment.Is64BitProcess)
            {
                envPath = envPath.EnsureEndsWith(";") + Path.Combine(AppDomain.CurrentDomain.RelativeSearchPath, "amd64");
                envPath = envPath.EnsureEndsWith(";") + Path.Combine(AppDomain.CurrentDomain.RelativeSearchPath, "x64");
            }
            else
            {
                envPath = envPath.EnsureEndsWith(";") + Path.Combine(AppDomain.CurrentDomain.RelativeSearchPath, "x86");
            }


            Environment.SetEnvironmentVariable("PATH", envPath, EnvironmentVariableTarget.Process);
        }

        private static void SetNativeDllPath()
        {
            var currentDomain = AppDomain.CurrentDomain;
            var privateBinPath = currentDomain.SetupInformation.PrivateBinPath.NullEmpty() ?? currentDomain.BaseDirectory;
            var dir = Path.Combine(privateBinPath, Environment.Is64BitProcess ? "x64" : "x86");

            Logger.DebugFormat("Setting native DLL path to '{0}'.", dir);

            SetDllDirectory(dir);
        }

        /// <summary>
        /// Perform file deploy
        /// </summary>
        /// <param name="ar">Assembly reference to probe</param>
		/// <returns>Reference to the shadow copied Assembly</returns>
        private static Assembly Probe(AssemblyReference ar, PluginDescriptor d, bool shadowCopy)
        {
            var file = ar.OriginalFile;

            try
            {
                if (file.Directory == null || file.Directory.Parent == null)
                {
                    throw new InvalidOperationException("The plugin directory for the " + file.Name +
                                                        " file exists in a folder outside of the allowed SmartStore folder hierarchy");
                }

                ar.File = InitializeFullTrust(file, shadowCopy);

                // Load assembly locked, because concurrent load calls - even with different assemblies -
                // will result in strange app init behaviour.
                lock (_lock)
                {
                    // We can now register the plugin definition
                    ar.Assembly = Assembly.Load(AssemblyName.GetAssemblyName(ar.File.FullName));

                    // Add the reference to the build manager
                    if (ar.Assembly != null)
                    {
                        // Loading assembly can fail in parallel loops.
                        // In this case, we'll probe again later in a sequential loop.
                        BuildManager.AddReferencedAssembly(ar.Assembly);
                    }
                }

                ar.ActivationException = null;
            }
            catch (UnauthorizedAccessException ex)
            {
                Logger.Error(ex.Message);

                // Throw the exception if its UnauthorizedAccessException as this will 
                // be because we most likely cannot copy to the dynamic folder.
                throw;
            }
            catch (ReflectionTypeLoadException ex)
            {
                var msg = string.Empty;
                foreach (var e in ex.LoaderExceptions)
                {
                    msg += e.Message + Environment.NewLine;
                }

                ar.ActivationException = CreateException(msg, ex);
            }
            catch (Exception ex)
            {
                var msg = string.Empty;
                for (var e = ex; e != null; e = e.InnerException)
                {
                    msg += e.Message + Environment.NewLine;
                }

                ar.ActivationException = CreateException(msg, ex);
            }

            return ar.Assembly;

            Exception CreateException(string message, Exception innerException)
            {
                return new SmartException(
                    "Error loading plugin '{0}'".FormatInvariant(d.SystemName) + Environment.NewLine + message,
                    innerException);
            }
        }

        /// <summary>
        /// Used to initialize plugins when running in Full Trust
        /// </summary>
        /// <param name="dll">Plugin dll file</param>
        /// <returns>Shadow copied file</returns>
        private static FileInfo InitializeFullTrust(FileInfo dll, bool shadowCopy)
        {
            var probedDll = new FileInfo(Path.Combine(_shadowCopyDir.FullName, dll.Name));

            // If instructed to not perform the copy, just return the path to where it is supposed to be
            if (!shadowCopy && probedDll.Exists)
                return probedDll;

            try
            {
                File.Copy(dll.FullName, probedDll.FullName, true);
            }
            catch (UnauthorizedAccessException)
            {
                throw new UnauthorizedAccessException(string.Format("Access to the path '{0}' is denied, ensure that read, write and modify permissions are allowed.", probedDll.Directory.FullName));
            }
            catch (IOException)
            {
                Logger.WarnFormat("'{0}' is locked, attempting to rename.", probedDll.FullName);

                // This occurs when the files are locked,
                // For some reason devenv locks plugin files some times and for another crazy reason you are allowed to rename them
                // Which releases the lock, so that it what we are doing here, once it's renamed, we can re-shadow copy
                try
                {
                    // If all else fails during the cleanup and we cannot copy over so we need to rename with a GUID
                    var deleteName = GetNewDeleteName(probedDll);
                    File.Move(probedDll.FullName, deleteName);
                }
                catch (UnauthorizedAccessException)
                {
                    throw new UnauthorizedAccessException(string.Format("Access to the path '{0}' is denied, ensure that read, write and modify permissions are allowed.", probedDll.Directory.FullName));
                }
                catch (IOException exc)
                {
                    throw new IOException(probedDll.FullName + " rename failed, cannot initialize plugin", exc);
                }

                // OK, we've made it this so far, now retry the shadow copy
                File.Copy(dll.FullName, probedDll.FullName, true);
            }

            return probedDll;
        }

        /// <summary>
        /// Determines if the folder is a bin plugin folder for a package
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        private static bool IsPackagePluginFolder(DirectoryInfo folder)
        {
            if (folder == null) return false;
            if (folder.Parent == null) return false;
            if (!folder.Parent.Name.Equals("Plugins", StringComparison.InvariantCultureIgnoreCase)) return false;
            return true;
        }
    }
}
