using Autofac;
using Autofac.Integration.Mvc;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Infrastructure.DependencyManagement;
using SmartStore.Core.Plugins;
using SmartStore.AmazonPay.Api;
using SmartStore.AmazonPay.Filters;
using SmartStore.AmazonPay.Services;
using SmartStore.Web.Controllers;

namespace SmartStore.AmazonPay
{
	public class DependencyRegistrar : IDependencyRegistrar
	{
		public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, bool isActiveModule)
		{
			builder.RegisterType<AmazonPayService>().As<IAmazonPayService>().InstancePerRequest();
			builder.RegisterType<AmazonPayApi>().As<IAmazonPayApi>().InstancePerRequest();

			if (isActiveModule)
			{
				builder.RegisterType<AmazonPayCheckoutFilter>().AsActionFilterFor<CheckoutController>().InstancePerRequest();
			}
		}

		public int Order
		{
			get { return 1; }
		}
	}
}
