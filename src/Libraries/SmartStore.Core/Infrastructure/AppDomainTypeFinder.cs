using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using SmartStore.Core.Logging;
using SmartStore.Core.Plugins;

namespace SmartStore.Core.Infrastructure
{
    /// <summary>
    /// A class that finds types needed by SmartStore by looping assemblies in the 
    /// currently executing AppDomain. Only assemblies whose names matches
    /// certain patterns are investigated and an optional list of assemblies
    /// referenced by <see cref="CustomAssemblyNames"/> are always investigated.
    /// </summary>
    public class AppDomainTypeFinder : ITypeFinder
    {
        private static readonly object s_lock = new object();

        private readonly Regex _systemAssembliesPattern = new Regex(@"^System|^mscorlib|^Microsoft", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

        private readonly bool _ignoreReflectionErrors = true;
        private bool _loadAppDomainAssemblies = true;
        private IList<string> _customAssemblyNames = new List<string>();

        public AppDomainTypeFinder()
        {
            Logger = NullLogger.Instance;
        }

        public ILogger Logger
        {
            get;
            set;
        }

        public virtual AppDomain App => AppDomain.CurrentDomain;

        /// <summary>
        /// Gets or sets wether SmartStore should iterate assemblies in the app domain when loading SmartStore types. Loading patterns are applied when loading these assemblies.
        /// </summary>
        public bool LoadAppDomainAssemblies
        {
            get => _loadAppDomainAssemblies;
            set => _loadAppDomainAssemblies = value;
        }

        /// <summary>
		/// Gets or sets assemblies to load at startup in addition to those loaded in the AppDomain.
		/// </summary>
        public IList<string> CustomAssemblyNames
        {
            get => _customAssemblyNames;
            set => _customAssemblyNames = value;
        }

        public IEnumerable<Type> FindClassesOfType(Type assignTypeFrom, IEnumerable<Assembly> assemblies, bool onlyConcreteClasses = true)
        {
            var result = new List<Type>();

            try
            {
                foreach (var a in assemblies)
                {
                    Type[] types = null;
                    try
                    {
                        types = a.GetExportedTypes();
                    }
                    catch
                    {
                        // Entity Framework 6 doesn't allow getting types (throws an exception)
                        if (!_ignoreReflectionErrors)
                        {
                            throw;
                        }
                    }

                    if (types == null)
                        continue;

                    foreach (var t in types)
                    {
                        if (assignTypeFrom.IsAssignableFrom(t) || (assignTypeFrom.IsGenericTypeDefinition && DoesTypeImplementOpenGeneric(t, assignTypeFrom)))
                        {
                            if (!t.IsInterface)
                            {
                                if (onlyConcreteClasses)
                                {
                                    if (t.IsClass && !t.IsAbstract)
                                    {
                                        result.Add(t);
                                    }
                                }
                                else
                                {
                                    result.Add(t);
                                }
                            }
                        }
                    }
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                Logger.Error(ex);

                var msg = string.Empty;
                foreach (var e in ex.LoaderExceptions)
                {
                    msg += e.Message + Environment.NewLine;
                }

                var fail = new Exception(msg, ex);
                throw fail;
            }
            return result;
        }

        public virtual IEnumerable<Assembly> GetAssemblies(bool ignoreInactivePlugins = false)
        {
            var assemblies = new HashSet<Assembly>();

            var addedAssemblyNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (_loadAppDomainAssemblies)
            {
                AddAppDomainAssemblies(addedAssemblyNames, assemblies);
            }

            AddCustomAssemblies(addedAssemblyNames, assemblies);

            if (ignoreInactivePlugins)
            {
                return assemblies.Where(x => PluginManager.IsActivePluginAssembly(x)).AsReadOnly();
            }

            return assemblies.AsReadOnly();
        }

        /// <summary>
        /// Iterates all assemblies in the AppDomain and if its name matches the configured patterns adds it to our list.
        /// </summary>
        /// <param name="addedAssemblyNames"></param>
        /// <param name="assemblies"></param>
        private void AddAppDomainAssemblies(HashSet<string> addedAssemblyNames, HashSet<Assembly> assemblies)
        {
            var domainAssemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in domainAssemblies)
            {
                if (Matches(assembly))
                {
                    if (!addedAssemblyNames.Contains(assembly.FullName))
                    {
                        assemblies.Add(assembly);
                        addedAssemblyNames.Add(assembly.FullName);
                    }
                }
            }
        }

        /// <summary>
		/// Explicitly adds custom assemblies.
		/// </summary>
        protected virtual void AddCustomAssemblies(HashSet<string> addedAssemblyNames, HashSet<Assembly> assemblies)
        {
            foreach (string assemblyName in CustomAssemblyNames)
            {
                if (!addedAssemblyNames.Contains(assemblyName))
                {
                    try
                    {
                        var assembly = Assembly.Load(assemblyName);
                        assemblies.Add(assembly);
                        Logger.DebugFormat("Loaded custom assembly '{0}'", assemblyName);
                    }
                    finally
                    {
                        addedAssemblyNames.Add(assemblyName);
                    }
                }
            }
        }

        /// <summary>
        /// Checks whether an assembly is one of the shipped system or 3rd party assemblies that don't need to be investigated.
        /// </summary>
        /// <param name="assembly">The assembly to check.</param>
        /// <returns>True if the assembly should be loaded into SmartStore.</returns>
        public virtual bool Matches(Assembly assembly)
        {
            var name = assembly.FullName;

            // Check if passed assembly is a system assembly
            if (_systemAssembliesPattern.IsMatch(name))
            {
                return false;
            }

            // Check if the passed assembly starts with "SmartStore."
            if (name.StartsWith("SmartStore.", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // Check if any of the referenced assemblies of the passed assembly start with "SmartStore."
            // In this case it's obviously a SmartStore plugin.
            if (assembly.GetReferencedAssemblies().Any(x => x.FullName.StartsWith("SmartStore.", StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }

            return false;
        }

        protected virtual bool DoesTypeImplementOpenGeneric(Type type, Type openGeneric)
        {
            try
            {
                var genericTypeDefinition = openGeneric.GetGenericTypeDefinition();
                foreach (var implementedInterface in type.FindInterfaces((objType, objCriteria) => true, null))
                {
                    if (!implementedInterface.IsGenericType)
                        continue;

                    var isMatch = genericTypeDefinition.IsAssignableFrom(implementedInterface.GetGenericTypeDefinition());
                    return isMatch;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

    }
}