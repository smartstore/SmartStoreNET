using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.Compilation;
using System.Runtime.InteropServices;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using SmartStore.Core.Infrastructure.DependencyManagement;
using SmartStore.Core.Plugins;
using SmartStore.Core.Packaging;
using SmartStore.Utilities;
using SmartStore.Utilities.Threading;
using SmartStore.Core.Data;

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

		private static readonly ReaderWriterLockSlim Locker = new ReaderWriterLockSlim();
        private static readonly string _pluginsPath = "~/Plugins";
		private static DirectoryInfo _shadowCopyDir;
		private static readonly ConcurrentDictionary<string, PluginDescriptor> _referencedPlugins = new ConcurrentDictionary<string, PluginDescriptor>(StringComparer.OrdinalIgnoreCase);
        private static HashSet<Assembly> _inactiveAssemblies = new HashSet<Assembly>();

		/// <summary>
		/// Returns the virtual path of the plugins folder relative to the application
		/// </summary>
		public static string PluginsLocation
		{
			get { return _pluginsPath; }
		}

		/// <summary> 
		/// Returns a collection of all referenced plugin assemblies that have been shadow copied
		/// </summary>
		public static IEnumerable<PluginDescriptor> ReferencedPlugins
		{
			get
			{
				return _referencedPlugins.Values;
			}
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
			var isFullTrust = WebHelper.GetTrustLevel() == AspNetHostingPermissionLevel.Unrestricted;
			if (!isFullTrust)
			{
				throw new ApplicationException("SmartStore.NET requires Full Trust mode. Please enable Full Trust for your web site or contact your hosting provider.");
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
			//SetPrivateEnvPath();

			DynamicModuleUtility.RegisterModule(typeof(AutofacRequestLifetimeHttpModule));

			#region Plugins

			var incompatiblePlugins = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

			using (Locker.GetWriteLock())
            {
				_shadowCopyDir = new DirectoryInfo(AppDomain.CurrentDomain.DynamicDirectory);

				var plugins = LoadPluginDescriptors().ToArray();
				var compatiblePlugins = plugins.Where(x => !x.Incompatible).ToArray();

				// If plugins state is dirty, we copy files over to the dynamic folder,
				// otherwise we just reference the previously copied file.
				var dirty = DetectAndCleanStalePlugins(compatiblePlugins);

				foreach (var plugin in plugins)
				{
					if (plugin.Incompatible)
					{
						incompatiblePlugins.Add(plugin.SystemName);
						continue;
					}
					else
					{
						_referencedPlugins[plugin.SystemName] = plugin;
					}

					try
					{
						// Shadow copy main plugin assembly
						plugin.ReferencedAssembly = Probe(plugin.OriginalAssemblyFile, dirty);

						// Shadow copy other referenced plugin local assemblies
						if (plugin.ReferencedLocalAssemblyFiles != null)
						{
							foreach (var assemblyFile in plugin.ReferencedLocalAssemblyFiles)
							{
								Probe(assemblyFile, dirty);
							}
						}

						if (!plugin.Installed)
						{
							_inactiveAssemblies.Add(plugin.ReferencedAssembly);
						}

						// Initialize: Find IPlugin, IPreApplicationStart, IConfigurable etc.
						ActivatePlugin(plugin);
					}
					catch (UnauthorizedAccessException)
					{
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

						HandlePluginActivationException(ex, plugin, msg, incompatiblePlugins);
					}
					catch (Exception ex)
					{
						var msg = string.Empty;
						for (var e = ex; e != null; e = e.InnerException)
						{
							msg += e.Message + Environment.NewLine;
						}

						HandlePluginActivationException(ex, plugin, msg, incompatiblePlugins);
					}
				}

				if (dirty && DataSettings.DatabaseIsInstalled())
				{
					// Save current hash of all deployed plugins to disk
					var hash = ComputePluginsHash(_referencedPlugins.Values.OrderBy(x => x.FolderName).ToArray());
					SavePluginsHash(hash);

					// Save names of all deployed assemblies to disk (so we can nuke them later)
					SavePluginsAssemblies(_referencedPlugins.Values);
				}

				IncompatiblePlugins = incompatiblePlugins.AsReadOnly();
			}

			#endregion
		}

		/// <summary>
		/// Loads and parses the descriptors of all installed plugins
		/// </summary>
		/// <returns>All descriptors</returns>
		private static IEnumerable<PluginDescriptor> LoadPluginDescriptors()
		{
			// TODO: Add verbose exception handling / raising here since this is happening on app startup and could
			// prevent app from starting altogether

			var pluginsDir = new DirectoryInfo(CommonHelper.MapPath(_pluginsPath));

			if (!pluginsDir.Exists)
			{
				pluginsDir.Create();
				yield break;
			}

			// Determine all plugin folders: ~/Plugins/{SystemName}
			var allPluginDirs = pluginsDir.EnumerateDirectories().ToArray()
				.Where(x => !x.Name.IsMatch("bin") && !x.Name.IsMatch("_Backup"))
				.OrderBy(x => x.Name)
				.ToArray();

			var installedPluginSystemNames = PluginFileParser.ParseInstalledPluginsFile();

			// Load/activate all plugins
			foreach (var d in allPluginDirs)
			{
				var descriptor = LoadPluginDescriptor(d, installedPluginSystemNames);
				if (descriptor != null)
				{
					yield return descriptor;
				}
			}
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

			descriptor.VirtualPath = _pluginsPath + "/" + descriptor.FolderName;

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
			descriptor.OriginalAssemblyFile = pluginBinaries.Get(descriptor.PluginFileName);

			if (descriptor.OriginalAssemblyFile == null)
			{
				throw new SmartException("The main assembly '{0}' for plugin '{1}' could not be found.".FormatInvariant(descriptor.PluginFileName, descriptor.SystemName));
			}

			// Load all other referenced local assemblies now
			var otherAssemblyFiles = pluginBinaries
				.Where(x => !x.Key.IsCaseInsensitiveEqual(descriptor.PluginFileName))
				.Select(x => x.Value);

			descriptor.ReferencedLocalAssemblyFiles = otherAssemblyFiles.ToArray();

			return descriptor;
		}

		private static void ActivatePlugin(PluginDescriptor plugin)
		{
			// Init plugin type (only one plugin per assembly is allowed)
			var exportedTypes = plugin.ReferencedAssembly.ExportedTypes;
			bool pluginFound = false;
			bool preStarterFound = !plugin.Installed;

			foreach (var t in exportedTypes)
			{
				if (typeof(IPlugin).IsAssignableFrom(t) && !t.IsInterface && t.IsClass && !t.IsAbstract)
				{
					plugin.PluginType = t;
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

		private static void HandlePluginActivationException(Exception ex, PluginDescriptor plugin, string msg, ICollection<string> incompatiblePlugins)
		{
			msg = "Error loading plugin '{0}'".FormatInvariant(plugin.SystemName) + Environment.NewLine + msg;

			var fail = new SmartException(msg, ex);
			Debug.WriteLine(fail.Message);

			if (plugin.ReferencedAssembly != null)
			{
				_inactiveAssemblies.Add(plugin.ReferencedAssembly);
			}

			plugin.ActivationException = fail;
			incompatiblePlugins.Add(plugin.SystemName);
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

			SetDllDirectory(dir);
		}

        /// <summary>
        /// Perform file deploy
        /// </summary>
        /// <param name="plugin">Plugin main dll file info</param>
		/// <returns>Reference to the shadow copied Assembly</returns>
        private static Assembly Probe(FileInfo plugin, bool performShadowCopy)
        {
			if (plugin.Directory == null || plugin.Directory.Parent == null)
			{
				throw new InvalidOperationException("The plugin directory for the " + plugin.Name +
													" file exists in a folder outside of the allowed SmartStore folder hierarchy");
			}

			var probedPlugin = InitializeFullTrust(plugin, performShadowCopy);

			// We can now register the plugin definition
			var probedAssembly = Assembly.Load(AssemblyName.GetAssemblyName(probedPlugin.FullName));

			// Add the reference to the build manager
			BuildManager.AddReferencedAssembly(probedAssembly);

			return probedAssembly;
        }

        /// <summary>
        /// Used to initialize plugins when running in Full Trust
        /// </summary>
        /// <param name="plugin">Plugin main dll file</param>
        /// <returns>Shadow copied file</returns>
        private static FileInfo InitializeFullTrust(FileInfo plugin, bool performShadowCopy)
        {
            var probedPlugin = new FileInfo(Path.Combine(_shadowCopyDir.FullName, plugin.Name));

			// If instructed to not perform the copy, just return the path to where it is supposed to be
			if (!performShadowCopy && probedPlugin.Exists)
				return probedPlugin;

			try
            {
                File.Copy(plugin.FullName, probedPlugin.FullName, true);
            }
			catch (UnauthorizedAccessException)
			{
				throw new UnauthorizedAccessException(string.Format("Access to the path '{0}' is denied, ensure that read, write and modify permissions are allowed.", probedPlugin.Directory.FullName));
			}
			catch (IOException)
            {
                Debug.WriteLine(probedPlugin.FullName + " is locked, attempting to rename");

                // This occurs when the files are locked,
                // For some reason devenv locks plugin files some times and for another crazy reason you are allowed to rename them
                // Which releases the lock, so that it what we are doing here, once it's renamed, we can re-shadow copy
                try
                {
					// If all else fails during the cleanup and we cannot copy over so we need to rename with a GUID
					var deleteName = GetNewDeleteName(probedPlugin);
					File.Move(probedPlugin.FullName, deleteName);
                }
				catch (UnauthorizedAccessException)
				{
					throw new UnauthorizedAccessException(string.Format("Access to the path '{0}' is denied, ensure that read, write and modify permissions are allowed.", probedPlugin.Directory.FullName));
				}
				catch (IOException exc)
                {
                    throw new IOException(probedPlugin.FullName + " rename failed, cannot initialize plugin", exc);
                }

                // OK, we've made it this so far, now retry the shadow copy
                File.Copy(plugin.FullName, probedPlugin.FullName, true);
            }

            return probedPlugin;
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
