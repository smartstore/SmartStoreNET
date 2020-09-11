using Autofac;
using Autofac.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Infrastructure.DependencyManagement;
using SmartStore.Data;
using SmartStore.Shipping.Data;
using SmartStore.Shipping.Domain;
using SmartStore.Shipping.Services;

namespace SmartStore.Shipping
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, bool isActiveModule)
        {
            builder.RegisterType<ShippingByTotalService>().As<IShippingByTotalService>().InstancePerRequest();

            //data layer
            //register named context
            builder.Register<IDbContext>(c => new ByTotalObjectContext(DataSettings.Current.DataConnectionString))
                .Named<IDbContext>(ByTotalObjectContext.ALIASKEY)
                .InstancePerRequest();

            builder.Register<ByTotalObjectContext>(c => new ByTotalObjectContext(DataSettings.Current.DataConnectionString))
                .InstancePerRequest();

            //override required repository with our custom context
            builder.RegisterType<EfRepository<ShippingByTotalRecord>>()
                .As<IRepository<ShippingByTotalRecord>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>(ByTotalObjectContext.ALIASKEY))
                .InstancePerRequest();
        }

        public int Order => 1;
    }
}
