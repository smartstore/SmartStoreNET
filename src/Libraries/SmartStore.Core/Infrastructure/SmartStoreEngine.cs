using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Autofac;
using Autofac.Integration.Mvc;
using SmartStore.Core.Data;
using SmartStore.Core.Infrastructure.DependencyManagement;
using SmartStore.Core.Logging;
using SmartStore.Core.Plugins;

namespace SmartStore.Core.Infrastructure
{
    public class SmartStoreEngine : IEngine
    {
        private ContainerManager _containerManager;

        public SmartStoreEngine()
        {
            Logger = NullLogger.Instance;
        }

        protected ILogger Logger
        {
            get;
            private set;
        }

        protected virtual void RunStartupTasks()
        {
            var typeFinder = _containerManager.Resolve<ITypeFinder>();
            var startUpTaskTypes = typeFinder.FindClassesOfType<IApplicationStart>(ignoreInactivePlugins: true);
            var startUpTasks = new List<IApplicationStart>();

            foreach (var startUpTaskType in startUpTaskTypes)
            {
                startUpTasks.Add((IApplicationStart)Activator.CreateInstance(startUpTaskType));
            }

            // execute tasks async grouped by order
            var groupedTasks = startUpTasks.OrderBy(st => st.Order).ToLookup(x => x.Order);
            foreach (var tasks in groupedTasks)
            {
                Parallel.ForEach(tasks, task =>
                {
                    try
                    {
                        Logger.DebugFormat("Executing startup task '{0}'", task.GetType().FullName);
                        task.Start();
                    }
                    catch (Exception ex)
                    {
                        Logger.ErrorFormat(ex, "Error while executing startup task '{0}'", task.GetType().FullName);
                    }
                });
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
            var typeFinder = CreateTypeFinder();

            // core dependencies
            var builder = new ContainerBuilder();
            builder.RegisterInstance(this).As<IEngine>();
            builder.RegisterInstance(typeFinder).As<ITypeFinder>();

            // Autofac
            builder.Register(x => new DefaultLifetimeScopeAccessor(x.Resolve<ILifetimeScope>())).As<ILifetimeScopeAccessor>().SingleInstance();
            builder.Register(x => new DefaultLifetimeScopeProvider(x.Resolve<ILifetimeScopeAccessor>())).As<ILifetimeScopeProvider>().SingleInstance();
            builder.Register(x => new AutofacDependencyResolver(x.Resolve<ILifetimeScope>(), x.Resolve<ILifetimeScopeProvider>())).As<IDependencyResolver>().SingleInstance();

            // Logging dependencies should be available very early
            builder.RegisterModule(new LoggingModule());

            // Register dependencies provided by other assemblies
            var registrarTypes = typeFinder.FindClassesOfType<IDependencyRegistrar>();
            var registrarInstances = new List<IDependencyRegistrar>();
            foreach (var type in registrarTypes)
            {
                registrarInstances.Add((IDependencyRegistrar)Activator.CreateInstance(type));
            }

            // Sort
            registrarInstances = registrarInstances.OrderBy(t => t.Order).ToList();
            foreach (var registrar in registrarInstances)
            {
                var type = registrar.GetType();
                Debug.WriteLine("Executing dependency registrar '{0}'.".FormatInvariant(type.FullName));
                registrar.Register(builder, typeFinder, PluginManager.IsActivePluginAssembly(type.Assembly));
            }

            var container = builder.Build();
            _containerManager = new ContainerManager(container);

            // MVC dependency resolver
            DependencyResolver.SetResolver(container.Resolve<IDependencyResolver>());

            // Logger
            this.Logger = container.Resolve<ILoggerFactory>().GetLogger("SmartStore.Bootstrapper");
            ((AppDomainTypeFinder)typeFinder).Logger = this.Logger;

            return _containerManager;
        }

        /// <summary>
        /// Initialize components and plugins
        /// </summary>
        public void Initialize()
        {
            RegisterDependencies();

            if (DataSettings.DatabaseIsInstalled())
            {
                RunStartupTasks();
            }
        }

        public bool IsFullyInitialized
        {
            get;
            set;
        }

        [DebuggerStepThrough]
        public T Resolve<T>(string name = null) where T : class
        {
            if (name.HasValue())
            {
                return ContainerManager.ResolveNamed<T>(name);
            }
            return ContainerManager.Resolve<T>();
        }

        [DebuggerStepThrough]
        public object Resolve(Type type, string name = null)
        {
            if (name.HasValue())
            {
                return ContainerManager.ResolveNamed(name, type);
            }
            return ContainerManager.Resolve(type);
        }

        [DebuggerStepThrough]
        public T[] ResolveAll<T>()
        {
            return ContainerManager.ResolveAll<T>();
        }

        public IContainer Container => _containerManager.Container;

        public ContainerManager ContainerManager => _containerManager;
    }
}
