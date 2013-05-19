using Autofac;
using Autofac.Integration.Mvc;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Infrastructure.DependencyManagement;
using SmartStore.Plugin.Payments.Sofortueberweisung.Core;

namespace SmartStore.Plugin.Payments.Sofortueberweisung
{
	public class DependencyRegistrar : IDependencyRegistrar
	{
		public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder) {
			builder.RegisterType<SofortueberweisungApi>().As<ISofortueberweisungApi>().InstancePerHttpRequest();
		}

		public int Order {
			get { return 1; }
		}
	}
}
