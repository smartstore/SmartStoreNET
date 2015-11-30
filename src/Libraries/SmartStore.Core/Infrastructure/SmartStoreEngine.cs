using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Autofac;
using Autofac.Integration.Mvc;
using SmartStore.Core.Data;
using SmartStore.Core.Infrastructure.DependencyManagement;
using SmartStore.Core.Plugins;

namespace SmartStore.Core.Infrastructure
{
    public class SmartStoreEngine : IEngine
    {
        private ContainerManager _containerManager;

        #region Utilities

        protected virtual void RunStartupTasks()
        {
			var typeFinder = _containerManager.Resolve<ITypeFinder>();
            var startUpTaskTypes = typeFinder.FindClassesOfType<IStartupTask>(ignoreInactivePlugins: true);
            var startUpTasks = new List<IStartupTask>();

            foreach (var startUpTaskType in startUpTaskTypes)
            {
				startUpTasks.Add((IStartupTask)Activator.CreateInstance(startUpTaskType));
            }

			// execute tasks async grouped by order
			var groupedTasks = startUpTasks.OrderBy(st => st.Order).ToLookup(x => x.Order);
			foreach (var tasks in groupedTasks)
			{
				Parallel.ForEach(tasks, task => { task.Execute(); });
			}
        }

		protected virtual ITypeFinder CreateTypeFinder()
		{
			return new WebAppTypeFinder();
		}

		protected virtual object CreateDependencyResolver(IContainer container)
		{
			var scopeProvider = container.Resolve<ILifetimeScopeProvider>();
			var dependencyResolver = new AutofacDependencyResolver(container, scopeProvider);
			return dependencyResolver;
		}

		protected virtual ContainerManager RegisterDependencies()
		{
			var builder = new ContainerBuilder();
			var container = builder.Build();
			var typeFinder = CreateTypeFinder();

			// core dependencies
			builder = new ContainerBuilder();
			builder.RegisterInstance(this).As<IEngine>();
			builder.RegisterInstance(typeFinder).As<ITypeFinder>();

			// Autofac
			var lifetimeScopeAccessor = new DefaultLifetimeScopeAccessor(container);
			var lifetimeScopeProvider = new DefaultLifetimeScopeProvider(lifetimeScopeAccessor);
			builder.RegisterInstance(lifetimeScopeAccessor).As<ILifetimeScopeAccessor>();
			builder.RegisterInstance(lifetimeScopeProvider).As<ILifetimeScopeProvider>();

			var dependencyResolver = new AutofacDependencyResolver(container, lifetimeScopeProvider);
			builder.RegisterInstance(dependencyResolver);
			DependencyResolver.SetResolver(dependencyResolver);

			builder.Update(container);

			// register dependencies provided by other assemblies
			builder = new ContainerBuilder();
			var registrarTypes = typeFinder.FindClassesOfType<IDependencyRegistrar>();
			var registrarInstances = new List<IDependencyRegistrar>();
			foreach (var type in registrarTypes)
			{
				registrarInstances.Add((IDependencyRegistrar)Activator.CreateInstance(type));
			}
			// sort
			registrarInstances = registrarInstances.AsQueryable().OrderBy(t => t.Order).ToList();
			foreach (var registrar in registrarInstances)
			{
				registrar.Register(builder, typeFinder, PluginManager.IsActivePluginAssembly(registrar.GetType().Assembly));
			}
			builder.Update(container);

			return new ContainerManager(container);
		}

        #endregion

        #region Methods
        
        /// <summary>
        /// Initialize components and plugins in the sm environment.
        /// </summary>
        public void Initialize()
        {
			_containerManager = RegisterDependencies();
			if (DataSettings.DatabaseIsInstalled())
			{
				RunStartupTasks();
			}
        }

        public T Resolve<T>(string name = null) where T : class
		{
            if (name.HasValue())
            {
                return ContainerManager.ResolveNamed<T>(name);
            }
            return ContainerManager.Resolve<T>();
		}

        public object Resolve(Type type, string name = null)
        {
            if (name.HasValue())
            {
                return ContainerManager.ResolveNamed(name, type);
            }
            return ContainerManager.Resolve(type);
        }


        public T[] ResolveAll<T>()
        {
            return ContainerManager.ResolveAll<T>();
        }

		#endregion

        #region Properties

        public IContainer Container
        {
            get { return _containerManager.Container; }
        }

        public ContainerManager ContainerManager
        {
            get { return _containerManager; }
        }

        #endregion

	}
}
