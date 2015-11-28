using Autofac;
using Autofac.Integration.Mvc;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Infrastructure.DependencyManagement;
using SmartStore.Core.Plugins;
using SmartStore.Web.Controllers;
using SmartStore.PayPal.Filters;

namespace SmartStore.PayPal
{
	public class DependencyRegistrar : IDependencyRegistrar
	{
		public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, bool isActiveModule)
		{
			if (isActiveModule)
			{
				builder.RegisterType<PayPalExpressCheckoutFilter>().AsActionFilterFor<CheckoutController>(x => x.PaymentMethod()).InstancePerRequest();
			}
		}

		public int Order
		{
			get { return 1; }
		}
	}
}
