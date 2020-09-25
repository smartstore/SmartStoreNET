using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Hosting;
using SmartStore.Core.Logging;
using SmartStore.Core.Plugins;
using SmartStore.Utilities;

namespace SmartStore.Core.Infrastructure
{
    /// <summary>
    /// Provides information about types in the current web application. 
    /// Optionally this class can look at all assemblies in the bin folder.
    /// </summary>
    public class WebAppTypeFinder : AppDomainTypeFinder
    {
        private readonly Lazy<IEnumerable<Assembly>> _allAssembliesResolver;
        private readonly Lazy<IEnumerable<Assembly>> _activeAssembliesResolver;

        public WebAppTypeFinder()
        {
            EnsureBinFolderAssembliesLoaded = CommonHelper.GetAppSetting<bool>("sm:EnableDynamicDiscovery", true);
            _allAssembliesResolver = new Lazy<IEnumerable<Assembly>>(() => this.GetAssembliesInternal(false));
            _activeAssembliesResolver = new Lazy<IEnumerable<Assembly>>(() => this.GetAssembliesInternal(true));
        }

        /// <summary>
        /// Gets or sets wether assemblies in the bin folder of the web application should be explicitly checked for being loaded on application load. 
        /// This is needed in situations where plugins need to be loaded into the AppDomain after the application has been reloaded.
        /// </summary>
        public bool EnsureBinFolderAssembliesLoaded { get; set; }

        /// <summary>
        /// Gets a physical disk path of \Bin directory
        /// </summary>
        /// <returns>The physical path. E.g. "c:\inetpub\wwwroot\bin"</returns>
        public virtual string GetBinDirectory()
        {
            if (HostingEnvironment.IsHosted)
            {
                // Hosted
                return HttpRuntime.BinDirectory;
            }
            else
            {
                // Not hosted. For example, run either in unit tests
                return AppDomain.CurrentDomain.BaseDirectory;
            }
        }

        public override IEnumerable<Assembly> GetAssemblies(bool ignoreInactivePlugins = false)
        {
            if (ignoreInactivePlugins)
            {
                return _activeAssembliesResolver.Value;
            }
            else
            {
                return _allAssembliesResolver.Value;
            }
        }

        private IEnumerable<Assembly> GetAssembliesInternal(bool ignoreInactivePlugins)
        {
            if (ignoreInactivePlugins)
            {
                var allAssemblies = _allAssembliesResolver.Value;
                return allAssemblies.Where(x => PluginManager.IsActivePluginAssembly(x)).AsReadOnly();
            }

            if (this.EnsureBinFolderAssembliesLoaded)
            {
                LoadPluginAssemblies();
            }

            return base.GetAssemblies(false);
        }

        /// <summary>
        /// Makes sure matching plugin assemblies are loaded into the app domain.
        /// </summary>
        protected virtual void LoadPluginAssemblies()
        {
            var alreadyLoadedAssemblyNames = new HashSet<string>(
                AppDomain.CurrentDomain.GetAssemblies().Select(x => x.FullName),
                StringComparer.OrdinalIgnoreCase);

            var pluginAssemblies = new HashSet<Assembly>(PluginManager.ReferencedPlugins.Select(x => x.Assembly.Assembly));

            foreach (var assembly in pluginAssemblies)
            {
                try
                {
                    if (assembly == null)
                        continue;

                    if (!alreadyLoadedAssemblyNames.Contains(assembly.FullName))
                    {
                        App.Load(assembly.FullName);
                        Logger.DebugFormat("Loaded plugin assembly '{0}'", assembly.FullName);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }
        }

        #region Obsolete

        ///// <summary>
        ///// Makes sure matching assemblies in the supplied folder are loaded into the app domain.
        ///// </summary>
        ///// <param name="path">The physical path to a directory containing dlls to load into the app domain.</param>
        //protected virtual void LoadCandidateAssembliesFromFolder(string path)
        //{
        //	var alreadyLoadedAssemblyNames = new HashSet<string>();
        //	var appDomainAssemblies = base.GetAssemblies();

        //	foreach (var a in appDomainAssemblies)
        //	{
        //		alreadyLoadedAssemblyNames.Add(a.FullName);
        //	}

        //	if (!Directory.Exists(path))
        //	{
        //		return;
        //	}

        //	var dlls = Directory.GetFiles(path, "*.dll");

        //	foreach (string dllPath in dlls)
        //	{
        //		try
        //		{
        //			var an = AssemblyName.GetAssemblyName(dllPath);
        //			if (!alreadyLoadedAssemblyNames.Contains(an.FullName) && Matches(an.FullName))
        //			{
        //				App.Load(an);
        //			}
        //		}
        //		catch (BadImageFormatException ex)
        //		{
        //			Trace.TraceError(ex.ToString());
        //		}
        //	}
        //}

        #endregion
    }
}
