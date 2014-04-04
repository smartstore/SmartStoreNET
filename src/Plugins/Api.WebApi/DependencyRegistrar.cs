using Autofac;
using Autofac.Integration.Mvc;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Infrastructure.DependencyManagement;
using SmartStore.Plugin.Api.WebApi.Services;

namespace SmartStore.Plugin.Api.WebApi
{
	public class DependencyRegistrar : IDependencyRegistrar
	{
		public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder)
		{
			builder.RegisterType<WebApiPluginService>().As<IWebApiPluginService>().InstancePerHttpRequest();
		}

		public int Order
		{
			get { return 1; }
		}
	}
}
