using Autofac;
using Autofac.Integration.Mvc;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Infrastructure.DependencyManagement;
using SmartStore.Plugin.Payments.PayPalStandard.Services;

namespace SmartStore.Plugin.Payments.PayPalStandard
{
	public class DependencyRegistrar : IDependencyRegistrar
	{
		public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder)
		{
			builder.RegisterType<PayPalStandardService>().As<IPayPalStandardService>().InstancePerHttpRequest();
		}

		public int Order
		{
			get { return 1; }
		}
	}
}
