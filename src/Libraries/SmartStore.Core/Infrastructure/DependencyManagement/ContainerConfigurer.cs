using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Autofac.Integration.Mvc;
using SmartStore.Core.Configuration;

namespace SmartStore.Core.Infrastructure.DependencyManagement
{
    /// <summary>
    /// Configures the inversion of control container with services used by SmartStore.
    /// </summary>
    public class ContainerConfigurer
    {
        public virtual void Configure(IEngine engine, ContainerManager containerManager, EventBroker broker, SmartStoreConfig configuration)
        {
            //other dependencies
            containerManager.AddComponentInstance<SmartStoreConfig>(configuration, "sm.configuration");
            containerManager.AddComponentInstance<IEngine>(engine, "sm.engine");
            containerManager.AddComponentInstance<ContainerConfigurer>(this, "sm.containerConfigurer");

            //type finder
            containerManager.AddComponent<ITypeFinder, WebAppTypeFinder>("sm.typeFinder");

            //register dependencies provided by other assemblies
            var typeFinder = containerManager.Resolve<ITypeFinder>();
            containerManager.UpdateContainer(x =>
            {
                var drTypes = typeFinder.FindClassesOfType<IDependencyRegistrar>();
                var drInstances = new List<IDependencyRegistrar>();
                foreach (var drType in drTypes)
                    drInstances.Add((IDependencyRegistrar)Activator.CreateInstance(drType));
                //sort
                drInstances = drInstances.AsQueryable().OrderBy(t => t.Order).ToList();
                foreach (var dependencyRegistrar in drInstances)
                    dependencyRegistrar.Register(x, typeFinder);
            });

            // event broker
            containerManager.AddComponentInstance(broker);

			// AutofacDependencyResolver
			var scopeProvider = new AutofacLifetimeScopeProvider(containerManager.Container);
			var dependencyResolver = new AutofacDependencyResolver(containerManager.Container, scopeProvider);
			DependencyResolver.SetResolver(dependencyResolver);
        }
    }
}
