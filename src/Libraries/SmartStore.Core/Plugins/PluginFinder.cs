using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SmartStore.Core.Plugins
{
    public class PluginFinder : IPluginFinder
    {
        private IList<PluginDescriptor> _plugins;
        private readonly IDictionary<string, PluginDescriptor> _nameMap = new Dictionary<string, PluginDescriptor>(StringComparer.InvariantCultureIgnoreCase);
        private readonly IDictionary<Assembly, PluginDescriptor> _assemblyMap = new Dictionary<Assembly, PluginDescriptor>();

        private static readonly object _lock = new object();

        private PluginFinder()
        {
            lock (_lock)
            {
                LoadPlugins();
            }
        }

        public static IPluginFinder Current { get; } = new PluginFinder();

        private void LoadPlugins()
        {
            if (_plugins != null)
                return;

            var plugins = PluginManager.ReferencedPlugins.ToList();
            plugins.Sort(); //sort
            _plugins = plugins;

            foreach (var plugin in plugins)
            {
                _nameMap[plugin.SystemName] = plugin;
                if (plugin.Assembly.Assembly != null)
                {
                    _assemblyMap[plugin.Assembly.Assembly] = plugin;
                }
            }
        }

        /// <summary>
        /// Gets plugins
        /// </summary>
        /// <typeparam name="T">The type of plugins to get.</typeparam>
        /// <param name="installedOnly">A value indicating whether to load only installed plugins</param>
        /// <returns>Plugins</returns>
        public virtual IEnumerable<T> GetPlugins<T>(bool installedOnly = true) where T : class, IPlugin
        {
            foreach (var plugin in _plugins)
                if (typeof(T).IsAssignableFrom(plugin.PluginClrType))
                    if (!installedOnly || plugin.Installed)
                        yield return plugin.Instance<T>();
        }

        /// <summary>
        /// Get plugin descriptors
        /// </summary>
        /// <param name="installedOnly">A value indicating whether to load only installed plugins</param>
        /// <returns>Plugin descriptors</returns>
        public virtual IEnumerable<PluginDescriptor> GetPluginDescriptors(bool installedOnly = true)
        {
            foreach (var plugin in _plugins)
                if (!installedOnly || plugin.Installed)
                    yield return plugin;
        }

        /// <summary>
        /// Get plugin descriptors
        /// </summary>
        /// <typeparam name="T">The type of plugin to get.</typeparam>
        /// <param name="installedOnly">A value indicating whether to load only installed plugins</param>
        /// <returns>Plugin descriptors</returns>
        public virtual IEnumerable<PluginDescriptor> GetPluginDescriptors<T>(bool installedOnly = true)
            where T : class, IPlugin
        {
            foreach (var plugin in _plugins)
                if (typeof(T).IsAssignableFrom(plugin.PluginClrType))
                    if (!installedOnly || plugin.Installed)
                        yield return plugin;
        }

        public virtual PluginDescriptor GetPluginDescriptorByAssembly(Assembly assembly, bool installedOnly = true)
        {
            if (assembly != null && _assemblyMap.TryGetValue(assembly, out var descriptor))
            {
                if (!installedOnly || descriptor.Installed)
                    return descriptor;
            }

            return null;
        }

        /// <summary>
        /// Get a plugin descriptor by its system name
        /// </summary>
        /// <param name="systemName">Plugin system name</param>
        /// <param name="installedOnly">A value indicating whether to load only installed plugins</param>
        /// <returns>>Plugin descriptor</returns>
        public virtual PluginDescriptor GetPluginDescriptorBySystemName(string systemName, bool installedOnly = true)
        {
            if (systemName.HasValue() && _nameMap.TryGetValue(systemName, out var descriptor))
            {
                if (!installedOnly || descriptor.Installed)
                    return descriptor;
            }

            return null;
        }

        /// <summary>
        /// Get a plugin descriptor by its system name
        /// </summary>
        /// <typeparam name="T">The type of plugin to get.</typeparam>
        /// <param name="systemName">Plugin system name</param>
        /// <param name="installedOnly">A value indicating whether to load only installed plugins</param>
        /// <returns>>Plugin descriptor</returns>
        public virtual PluginDescriptor GetPluginDescriptorBySystemName<T>(string systemName, bool installedOnly = true) where T : class, IPlugin
        {
            if (systemName.HasValue() && _nameMap.TryGetValue(systemName, out var descriptor))
            {
                if (!installedOnly || descriptor.Installed)
                {
                    if (typeof(T).IsAssignableFrom(descriptor.PluginClrType))
                        return descriptor;
                }
            }

            return null;
        }

    }
}