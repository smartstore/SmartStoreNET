using Autofac;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Infrastructure.DependencyManagement;
using SmartStore.PayPal.Services;

namespace SmartStore.PayPal
{
	public class DependencyRegistrar : IDependencyRegistrar
	{
		public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, bool isActiveModule)
		{
            builder.RegisterType<PayPalExpressApiService>().As<IPayPalExpressApiService>().InstancePerRequest();
            builder.RegisterType<PayPalStandardService>().As<IPayPalStandardService>().InstancePerRequest();
		}

		public int Order
		{
			get { return 1; }
		}
	}
}
