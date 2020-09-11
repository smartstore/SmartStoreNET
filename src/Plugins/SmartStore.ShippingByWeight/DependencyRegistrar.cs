using Autofac;
using Autofac.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Infrastructure.DependencyManagement;
using SmartStore.Data;
using SmartStore.ShippingByWeight.Data;
using SmartStore.ShippingByWeight.Domain;
using SmartStore.ShippingByWeight.Services;

namespace SmartStore.ShippingByWeight
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, bool isActiveModule)
        {
            builder.RegisterType<ShippingByWeightService>().As<IShippingByWeightService>().InstancePerRequest();

            // data layer
            // register named context
            builder.Register<IDbContext>(c => new ShippingByWeightObjectContext(DataSettings.Current.DataConnectionString))
                .Named<IDbContext>(ShippingByWeightObjectContext.ALIASKEY)
                .InstancePerRequest();

            builder.Register<ShippingByWeightObjectContext>(c => new ShippingByWeightObjectContext(DataSettings.Current.DataConnectionString))
                .InstancePerRequest();

            // override required repository with our custom context
            builder.RegisterType<EfRepository<ShippingByWeightRecord>>()
                .As<IRepository<ShippingByWeightRecord>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>(ShippingByWeightObjectContext.ALIASKEY))
                .InstancePerRequest();
        }

        public int Order => 1;
    }
}
