using System;
using System.Collections.Generic;
using SmartStore.Core.Configuration;
using SmartStore.Core.Infrastructure.DependencyManagement;

namespace SmartStore.Core.Infrastructure
{
    /// <summary>
    /// Classes implementing this interface can serve as a portal for the 
    /// various services composing the SmartStore engine. Edit functionality, modules
    /// and implementations access most SmartStore functionality through this 
    /// interface.
    /// </summary>
    public interface IEngine
    {
        ContainerManager ContainerManager { get; }
        
        /// <summary>
        /// Initialize components and plugins in the SmartStore environment.
        /// </summary>
        /// <param name="config">Config</param>
        void Initialize();

        T Resolve<T>(string name = null) where T : class;

        object Resolve(Type type, string name = null);

        T[] ResolveAll<T>();
    }
}
