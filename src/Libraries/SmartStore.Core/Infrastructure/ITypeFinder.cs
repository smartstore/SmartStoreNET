using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SmartStore.Core.Infrastructure
{
    /// <summary>
    /// Classes implementing this interface provide information about types 
    /// to various services in the SmartStore engine.
    /// </summary>
    public interface ITypeFinder
    {
        ///// <summary>
        ///// Gets all SmartStore specific assemblies (core & all plugins)
        ///// </summary>
        ///// <param name="ignoreInactivePlugins">Indicates whether uninstalled plugin's assemblies should be ignored</param>
        ///// <returns>A list of assemblies that should be loaded by the SmartStore factory.</returns>
        IEnumerable<Assembly> GetAssemblies(bool ignoreInactivePlugins = false);
        IEnumerable<Type> FindClassesOfType(Type assignTypeFrom, IEnumerable<Assembly> assemblies, bool onlyConcreteClasses = true);
    }

    public class NullTypeFinder : ITypeFinder
    {
        private static readonly ITypeFinder s_instance = new NullTypeFinder();

        public static ITypeFinder Instance => s_instance;

        public IEnumerable<Assembly> GetAssemblies(bool ignoreInactivePlugins = false)
        {
            return Enumerable.Empty<Assembly>();
        }

        public IEnumerable<Type> FindClassesOfType(Type assignTypeFrom, IEnumerable<Assembly> assemblies, bool onlyConcreteClasses = true)
        {
            return Enumerable.Empty<Type>();
        }
    }

    public static class ITypeFinderExtensions
    {
        public static IEnumerable<Type> FindClassesOfType<T>(this ITypeFinder finder, bool onlyConcreteClasses = true, bool ignoreInactivePlugins = false)
        {
            return finder.FindClassesOfType(typeof(T), finder.GetAssemblies(ignoreInactivePlugins), onlyConcreteClasses);
        }

        public static IEnumerable<Type> FindClassesOfType(this ITypeFinder finder, Type assignTypeFrom, bool onlyConcreteClasses = true, bool ignoreInactivePlugins = false)
        {
            return finder.FindClassesOfType(assignTypeFrom, finder.GetAssemblies(ignoreInactivePlugins), onlyConcreteClasses);
        }

        public static IEnumerable<Type> FindClassesOfType<T>(this ITypeFinder finder, IEnumerable<Assembly> assemblies, bool onlyConcreteClasses = true)
        {
            return finder.FindClassesOfType(typeof(T), assemblies, onlyConcreteClasses);
        }
    }
}
