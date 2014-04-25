using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Autofac;
using Autofac.Integration.Mvc;
using SmartStore.Core.Configuration;
using SmartStore.Core.Data;
using SmartStore.Core.Infrastructure.DependencyManagement;
using SmartStore.Core.Plugins;

namespace SmartStore.Core.Infrastructure
{
    public class SmartStoreEngine : IEngine
    {
        #region Fields

        private ContainerManager _containerManager;

        #endregion

        #region Ctor

        /// <summary>
		/// Creates an instance of the content engine using default settings and configuration.
		/// </summary>
		public SmartStoreEngine() 
            : this(EventBroker.Instance, new ContainerConfigurer())
		{
		}

		public SmartStoreEngine(EventBroker broker, ContainerConfigurer configurer)
		{
            var config = ConfigurationManager.GetSection("SmartStoreConfig") as SmartStoreConfig;
            InitializeContainer(configurer, broker, config);
		}
        
        #endregion

        #region Utilities

        private void RunStartupTasks()
        {
            var typeFinder = _containerManager.Resolve<ITypeFinder>();
            var startUpTaskTypes = typeFinder.FindClassesOfType<IStartupTask>();
            var startUpTasks = new List<IStartupTask>();

            foreach (var startUpTaskType in startUpTaskTypes)
            {
                if (PluginManager.IsActivePluginAssembly(startUpTaskType.Assembly))
                {
                    startUpTasks.Add((IStartupTask)Activator.CreateInstance(startUpTaskType));
                }
            }

			// execute tasks async grouped by order
			var groupedTasks = startUpTasks.OrderBy(st => st.Order).ToLookup(x => x.Order);
			foreach (var tasks in groupedTasks)
			{
				Parallel.ForEach(tasks, task => { task.Execute(); });
			}
			
        }
        
        private void InitializeContainer(ContainerConfigurer configurer, EventBroker broker, SmartStoreConfig config)
        {
            var builder = new ContainerBuilder();
			_containerManager = new ContainerManager(builder.Build());
            configurer.Configure(this, _containerManager, broker, config);
        }

        #endregion

        #region Methods
        
        /// <summary>
        /// Initialize components and plugins in the sm environment.
        /// </summary>
        /// <param name="config">Config</param>
        public void Initialize(SmartStoreConfig config)
        {
            bool databaseInstalled = DataSettings.DatabaseIsInstalled();
			if (databaseInstalled)
			{
				//startup tasks
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
