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
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using SmartStore.Core.Infrastructure.DependencyManagement;
using SmartStore.Core.Plugins;
using SmartStore.Core.Packaging;
using SmartStore.Utilities;
using SmartStore.Utilities.Threading;
using System.Runtime.InteropServices;

// Contributor: Umbraco (http://www.umbraco.com). Thanks a lot!
// SEE THIS POST for full details of what this does
//http://shazwazza.com/post/Developing-a-plugin-framework-in-ASPNET-with-medium-trust.aspx

[assembly: PreApplicationStartMethod(typeof(PluginManager), "Initialize")]

namespace SmartStore.Core.Plugins
{
    /// <summary>
    /// Sets the application up for the plugin referencing
    /// </summary>
    public class PluginManager
    {
		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool SetDllDirectory(string lpPathName);

		private static readonly ReaderWriterLockSlim Locker = new ReaderWriterLockSlim();
        private static DirectoryInfo _shadowCopyFolder;
        private static readonly string _pluginsPath = "~/Plugins";
        private static readonly string _shadowCopyPath = "~/Plugins/bin";
        private static bool _clearShadowDirectoryOnStartup;
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
			
			using (Locker.GetWriteLock())
            {
                // TODO: Add verbose exception handling / raising here since this is happening on app startup and could
                // prevent app from starting altogether
                var pluginFolderPath = CommonHelper.MapPath(_pluginsPath);
				_shadowCopyFolder = new DirectoryInfo(CommonHelper.MapPath(_shadowCopyPath));

                var incompatiblePlugins = new List<string>();
				_clearShadowDirectoryOnStartup = CommonHelper.GetAppSetting<bool>("sm:ClearPluginsShadowDirectoryOnStartup", true);
                try
                {	
                    Debug.WriteLine("Creating shadow copy folder and querying for dlls");
                    //ensure folders are created
                    Directory.CreateDirectory(pluginFolderPath);
                    Directory.CreateDirectory(_shadowCopyFolder.FullName);

                    // get list of all files in bin
                    var binFiles = _shadowCopyFolder.GetFiles("*", SearchOption.AllDirectories);
                    if (_clearShadowDirectoryOnStartup)
                    {
                        // clear out shadow copied plugins
                        foreach (var f in binFiles)
                        {
                            Debug.WriteLine("Deleting " + f.Name);
                            try
                            {
                                File.Delete(f.FullName);
                            }
                            catch (Exception exc)
                            {
                                Debug.WriteLine("Error deleting file " + f.Name + ". Exception: " + exc);
                            }
                        }
                    }

					// determine all plugin folders
					var pluginPaths = from x in Directory.EnumerateDirectories(pluginFolderPath)
									  where !x.IsMatch("bin") && !x.IsMatch("_Backup")
									  select Path.Combine(pluginFolderPath, x);

					var installedPluginSystemNames = PluginFileParser.ParseInstalledPluginsFile();
					
					// now activate all plugins
					foreach (var pluginPath in pluginPaths)
					{
						var result = LoadPluginFromFolder(pluginPath, installedPluginSystemNames);
						if (result != null)
						{
							if (result.IsIncompatible)
							{
								incompatiblePlugins.Add(result.Descriptor.SystemName);
							}
							else if (result.Success)
							{
								_referencedPlugins[result.Descriptor.SystemName] = result.Descriptor;
							}
						}
					}
                }
                catch (Exception ex)
                {
                    var msg = string.Empty;
					for (var e = ex; e != null; e = e.InnerException)
					{
						msg += e.Message + Environment.NewLine;
					}

                    var fail = new Exception(msg, ex);
                    Debug.WriteLine(fail.Message, fail);

                    throw fail;
                }

                IncompatiblePlugins = incompatiblePlugins.AsReadOnly();
            }
        }

		private static LoadPluginResult LoadPluginFromFolder(string pluginFolderPath, ICollection<string> installedPluginSystemNames)
		{
			Guard.NotEmpty(pluginFolderPath, nameof(pluginFolderPath));

			var folder = new DirectoryInfo(pluginFolderPath);
			if (!folder.Exists)
			{
				return null;
			}

			var descriptionFile = new FileInfo(Path.Combine(pluginFolderPath, "Description.txt"));
			if (!descriptionFile.Exists)
			{
				return null;
			}

			// load descriptor file (Description.txt)
			var descriptor = PluginFileParser.ParsePluginDescriptionFile(descriptionFile.FullName);

			// some validation
			if (descriptor.SystemName.IsEmpty())
			{
				throw new Exception("The plugin descriptor '{0}' does not define a plugin system name. Try assigning the plugin a unique name and recompile.".FormatInvariant(descriptionFile.FullName));
			}
			if (descriptor.PluginFileName.IsEmpty())
			{
				throw new Exception("The plugin descriptor '{0}' does not define a plugin assembly file name. Try assigning the plugin a file name and recompile.".FormatInvariant(descriptionFile.FullName));
			}

			var result = new LoadPluginResult
			{
				DescriptionFile = descriptionFile,
				Descriptor = descriptor
			};

			//ensure that version of plugin is valid
			if (!IsAssumedCompatible(descriptor))
			{
				result.IsIncompatible = true;
				return result;
			}

			if (_referencedPlugins.ContainsKey(descriptor.SystemName))
			{
				throw new Exception(string.Format("A plugin with system name '{0}' is already defined", descriptor.SystemName));
			}

			if (installedPluginSystemNames == null)
			{
				installedPluginSystemNames = PluginFileParser.ParseInstalledPluginsFile();
			}

			// set 'Installed' property
			descriptor.Installed = installedPluginSystemNames.Contains(descriptor.SystemName);

			try
			{
				// get list of all DLLs in plugin folders (not in 'bin' or '_Backup'!)
				var pluginBinaries = descriptionFile.Directory.GetFiles("*.dll", SearchOption.AllDirectories)
					// just make sure we're not registering shadow copied plugins
					.Where(x => IsPackagePluginFolder(x.Directory))
					.ToList();

				// other plugin description info
				var mainPluginFile = pluginBinaries.Where(x => x.Name.IsCaseInsensitiveEqual(descriptor.PluginFileName)).FirstOrDefault();
				descriptor.OriginalAssemblyFile = mainPluginFile;

				// shadow copy main plugin file
				descriptor.ReferencedAssembly = Probe(mainPluginFile);

				if (!descriptor.Installed)
				{
					_inactiveAssemblies.Add(descriptor.ReferencedAssembly);
				}

				// load all other referenced assemblies now
				var otherAssemblies = from x in pluginBinaries
									  where !x.Name.IsCaseInsensitiveEqual(mainPluginFile.Name)
									  select x;

				foreach (var assemblyFile in otherAssemblies)
				{
					if (!IsAlreadyLoaded(assemblyFile))
					{
						Probe(assemblyFile);
					}
				}

				// init plugin type (only one plugin per assembly is allowed)
				var exportedTypes = descriptor.ReferencedAssembly.ExportedTypes;
				bool pluginFound = false;
				bool preStarterFound = !descriptor.Installed;
				foreach (var t in exportedTypes)
				{
					if (typeof(IPlugin).IsAssignableFrom(t) && !t.IsInterface && t.IsClass && !t.IsAbstract)
					{
						descriptor.PluginType = t;
						descriptor.IsConfigurable = typeof(IConfigurable).IsAssignableFrom(t);
						pluginFound = true;
					}
					else if (descriptor.Installed && typeof(IPreApplicationStart).IsAssignableFrom(t) && !t.IsInterface && t.IsClass && !t.IsAbstract && t.HasDefaultConstructor())
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

				result.Success = true;
			}
			catch (ReflectionTypeLoadException ex)
			{
				var msg = string.Empty;
				foreach (var e in ex.LoaderExceptions)
				{
					msg += e.Message + Environment.NewLine;
				}

				var fail = new Exception(msg, ex);
				Debug.WriteLine(fail.Message, fail);

				throw fail;
			}

			return result;
		}

        /// <summary>
        /// Mark plugin as installed
        /// </summary>
        /// <param name="systemName">Plugin system name</param>
        public static void MarkPluginAsInstalled(string systemName)
        {
			if (String.IsNullOrEmpty(systemName))
				throw new ArgumentNullException("systemName");

			var installedPluginSystemNames = GetInstalledPluginNames();
			bool alreadyMarkedAsInstalled = installedPluginSystemNames.Contains(systemName);
			if (!alreadyMarkedAsInstalled)
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
			bool alreadyMarkedAsInstalled = installedPluginSystemNames.Contains(systemName);
			if (alreadyMarkedAsInstalled)
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
					//we use 'using' to close the file after it's created
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
		/// Indicates whether assembly file is already loaded
		/// </summary>
		/// <param name="fileInfo">File info</param>
		/// <returns>Result</returns>
		private static bool IsAlreadyLoaded(FileInfo fileInfo)
        {
            // do not compare the full assembly name, just filename
            try
            {
                string fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileInfo.FullName);
				var assemblies = AppDomain.CurrentDomain.GetAssemblies();
				foreach (var a in assemblies)
                {
                    string assemblyName = a.FullName.Split(new[] { ',' }).FirstOrDefault();
                    if (fileNameWithoutExt.Equals(assemblyName, StringComparison.InvariantCultureIgnoreCase))
                        return true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Cannot validate whether an assembly is already loaded. " + ex);
            }
            return false;
        }

        /// <summary>
        /// Perform file deply
        /// </summary>
        /// <param name="plug">Plugin file info</param>
		/// <returns>Reference to the shadow copied Assembly</returns>
        private static Assembly Probe(FileInfo plug)
        {
			if (plug.Directory == null || plug.Directory.Parent == null)
				throw new InvalidOperationException("The plugin directory for the " + plug.Name +
													" file exists in a folder outside of the allowed SmartStore folder hierarchy");

			FileInfo shadowCopiedPlug;

			if (WebHelper.GetTrustLevel() != AspNetHostingPermissionLevel.Unrestricted)
			{
				// TODO: (mc) SMNET does not support Medium Trust, so this code is actually obsolete!

				// all plugins will need to be copied to ~/Plugins/bin/
				// this is aboslutely required because all of this relies on probingPaths being set statically in the web.config

				// were running in med trust, so copy to custom bin folder
				var shadowCopyPlugFolder = Directory.CreateDirectory(_shadowCopyFolder.FullName);
				shadowCopiedPlug = InitializeMediumTrust(plug, shadowCopyPlugFolder);
			}
			else
			{
				var directory = AppDomain.CurrentDomain.DynamicDirectory;
				//Debug.WriteLine(plug.FullName + " to " + directory);	// codehint: sm-edit
				// we're running in full trust so copy to standard dynamic folder
				shadowCopiedPlug = InitializeFullTrust(plug, new DirectoryInfo(directory));
			}

			// we can now register the plugin definition
			var shadowCopiedAssembly = Assembly.Load(AssemblyName.GetAssemblyName(shadowCopiedPlug.FullName));

			// add the reference to the build manager
			//Debug.WriteLine("Adding to BuildManager: '{0}'", shadowCopiedAssembly.FullName);	// codehint: sm-edit
			BuildManager.AddReferencedAssembly(shadowCopiedAssembly);

			return shadowCopiedAssembly;
        }

        /// <summary>
        /// Used to initialize plugins when running in Full Trust
        /// </summary>
        /// <param name="plug"></param>
        /// <param name="shadowCopyPlugFolder"></param>
        /// <returns></returns>
        private static FileInfo InitializeFullTrust(FileInfo plug, DirectoryInfo shadowCopyPlugFolder)
        {
            var shadowCopiedPlug = new FileInfo(Path.Combine(shadowCopyPlugFolder.FullName, plug.Name));
            try
            {
                File.Copy(plug.FullName, shadowCopiedPlug.FullName, true);
            }
            catch (IOException)
            {
                Debug.WriteLine(shadowCopiedPlug.FullName + " is locked, attempting to rename");
                //this occurs when the files are locked,
                //for some reason devenv locks plugin files some times and for another crazy reason you are allowed to rename them
                //which releases the lock, so that it what we are doing here, once it's renamed, we can re-shadow copy
                try
                {
                    var oldFile = shadowCopiedPlug.FullName + Guid.NewGuid().ToString("N") + ".old";
                    File.Move(shadowCopiedPlug.FullName, oldFile);
                }
                catch (IOException exc)
                {
                    throw new IOException(shadowCopiedPlug.FullName + " rename failed, cannot initialize plugin", exc);
                }
                //ok, we've made it this far, now retry the shadow copy
                File.Copy(plug.FullName, shadowCopiedPlug.FullName, true);
            }
            return shadowCopiedPlug;
        }

        /// <summary>
        /// Used to initialize plugins when running in Medium Trust
        /// </summary>
        /// <param name="plug"></param>
        /// <param name="shadowCopyPlugFolder"></param>
        /// <returns></returns>
        private static FileInfo InitializeMediumTrust(FileInfo plug, DirectoryInfo shadowCopyPlugFolder)
        {
            var shouldCopy = true;
            var shadowCopiedPlug = new FileInfo(Path.Combine(shadowCopyPlugFolder.FullName, plug.Name));

            //check if a shadow copied file already exists and if it does, check if it's updated, if not don't copy
            if (shadowCopiedPlug.Exists)
            {
                //it's better to use LastWriteTimeUTC, but not all file systems have this property
                //maybe it is better to compare file hash?
                var areFilesIdentical = shadowCopiedPlug.CreationTimeUtc.Ticks >= plug.CreationTimeUtc.Ticks;
                if (areFilesIdentical)
                {
                    Debug.WriteLine("Not copying; files appear identical: '{0}'", shadowCopiedPlug.Name);
                    shouldCopy = false;
                }
                else
                {
                    //delete an existing file
                    Debug.WriteLine("New plugin found; Deleting the old file: '{0}'", shadowCopiedPlug.Name);
                    File.Delete(shadowCopiedPlug.FullName);
                }
            }

            if (shouldCopy)
            {
                try
                {
                    File.Copy(plug.FullName, shadowCopiedPlug.FullName, true);
                }
                catch (IOException)
                {
                    Debug.WriteLine(shadowCopiedPlug.FullName + " is locked, attempting to rename");
                    //this occurs when the files are locked,
                    //for some reason devenv locks plugin files some times and for another crazy reason you are allowed to rename them
                    //which releases the lock, so that it what we are doing here, once it's renamed, we can re-shadow copy
                    try
                    {
                        var oldFile = shadowCopiedPlug.FullName + Guid.NewGuid().ToString("N") + ".old";
                        File.Move(shadowCopiedPlug.FullName, oldFile);
                    }
                    catch (IOException exc)
                    {
                        throw new IOException(shadowCopiedPlug.FullName + " rename failed, cannot initialize plugin", exc);
                    }
                    //ok, we've made it this far, now retry the shadow copy
                    File.Copy(plug.FullName, shadowCopiedPlug.FullName, true);
                }
            }

            return shadowCopiedPlug;
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
