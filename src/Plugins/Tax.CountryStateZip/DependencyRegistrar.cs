using Autofac;
using Autofac.Core;
using Autofac.Integration.Mvc;
using SmartStore.Core.Data;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Infrastructure.DependencyManagement;
using SmartStore.Data;
using SmartStore.Plugin.Tax.CountryStateZip.Data;
using SmartStore.Plugin.Tax.CountryStateZip.Domain;
using SmartStore.Plugin.Tax.CountryStateZip.Services;

namespace SmartStore.Plugin.Tax.CountryStateZip
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder)
        {
            builder.RegisterType<TaxRateService>().As<ITaxRateService>().InstancePerHttpRequest();

            //data layer
            var dataSettingsManager = new DataSettingsManager();
            var dataProviderSettings = dataSettingsManager.LoadSettings();

            if (dataProviderSettings != null && dataProviderSettings.IsValid())
            {
                //register named context
                builder.Register<IDbContext>(c => new TaxRateObjectContext(dataProviderSettings.DataConnectionString))
                    .Named<IDbContext>(TaxRateObjectContext.ALIASKEY)
                    .InstancePerHttpRequest();

                builder.Register<TaxRateObjectContext>(c => new TaxRateObjectContext(dataProviderSettings.DataConnectionString))
                    .InstancePerHttpRequest();
            }
            else
            {
                //register named context
                builder.Register<IDbContext>(c => new TaxRateObjectContext(c.Resolve<DataSettings>().DataConnectionString))
                    .Named<IDbContext>(TaxRateObjectContext.ALIASKEY)
                    .InstancePerHttpRequest();

                builder.Register<TaxRateObjectContext>(c => new TaxRateObjectContext(c.Resolve<DataSettings>().DataConnectionString))
                    .InstancePerHttpRequest();
            }

            //override required repository with our custom context
            builder.RegisterType<EfRepository<TaxRate>>()
                .As<IRepository<TaxRate>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>(TaxRateObjectContext.ALIASKEY))
                .InstancePerHttpRequest();
        }

        public int Order
        {
            get { return 1; }
        }
    }
}
