using Autofac;
using Autofac.Core;
using Autofac.Integration.Mvc;
using SmartStore.Core.Data;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Infrastructure.DependencyManagement;
using SmartStore.Data;
using SmartStore.Plugin.Shipping.ByTotal.Data;
using SmartStore.Plugin.Shipping.ByTotal.Domain;
using SmartStore.Plugin.Shipping.ByTotal.Services;

namespace SmartStore.Plugin.Shipping.ByTotal
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder)
        {
			builder.RegisterType<ShippingByTotalService>().As<IShippingByTotalService>().WithRequestCache().InstancePerHttpRequest();

            //data layer
            //register named context
			builder.Register<IDbContext>(c => new ShippingByTotalObjectContext(DataSettings.Current.DataConnectionString))
                .Named<IDbContext>(ShippingByTotalObjectContext.ALIASKEY)
                .InstancePerHttpRequest();

			builder.Register<ShippingByTotalObjectContext>(c => new ShippingByTotalObjectContext(DataSettings.Current.DataConnectionString))
                .InstancePerHttpRequest();

            //override required repository with our custom context
            builder.RegisterType<EfRepository<ShippingByTotalRecord>>()
                .As<IRepository<ShippingByTotalRecord>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>(ShippingByTotalObjectContext.ALIASKEY))
                .InstancePerHttpRequest();
        }

        public int Order
        {
            get { return 1; }
        }
    }
}
