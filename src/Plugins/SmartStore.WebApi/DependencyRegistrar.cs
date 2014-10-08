using Autofac;
using Autofac.Integration.Mvc;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Infrastructure.DependencyManagement;
using SmartStore.WebApi.Services;

namespace SmartStore.WebApi
{
	public class DependencyRegistrar : IDependencyRegistrar
	{
		public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, bool isActiveModule)
		{
			builder.RegisterType<WebApiPluginService>().As<IWebApiPluginService>().InstancePerRequest();
		}

		public int Order
		{
			get { return 1; }
		}
	}
}
