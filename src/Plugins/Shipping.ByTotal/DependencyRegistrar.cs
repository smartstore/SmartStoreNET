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
            builder.RegisterType<ShippingByTotalService>().As<IShippingByTotalService>().InstancePerHttpRequest();

            //data layer
            var dataSettingsManager = new DataSettingsManager();
            var dataProviderSettings = dataSettingsManager.LoadSettings();

            if (dataProviderSettings != null && dataProviderSettings.IsValid())
            {
                //register named context
                builder.Register<IDbContext>(c => new ShippingByTotalObjectContext(dataProviderSettings.DataConnectionString))
                    .Named<IDbContext>(ShippingByTotalObjectContext.ALIASKEY)
                    .InstancePerHttpRequest();

                builder.Register<ShippingByTotalObjectContext>(c => new ShippingByTotalObjectContext(dataProviderSettings.DataConnectionString))
                    .InstancePerHttpRequest();
            }
            else
            {
                //register named context
                builder.Register<IDbContext>(c => new ShippingByTotalObjectContext(c.Resolve<DataSettings>().DataConnectionString))
                    .Named<IDbContext>(ShippingByTotalObjectContext.ALIASKEY)
                    .InstancePerHttpRequest();

                builder.Register<ShippingByTotalObjectContext>(c => new ShippingByTotalObjectContext(c.Resolve<DataSettings>().DataConnectionString))
                    .InstancePerHttpRequest();
            }

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
