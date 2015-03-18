using Autofac;
using Autofac.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Infrastructure.DependencyManagement;
using SmartStore.Data;
using SmartStore.Tax.Data;
using SmartStore.Tax.Domain;
using SmartStore.Tax.Services;

namespace SmartStore.Tax
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
		public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, bool isActiveModule)
        {
			builder.RegisterType<TaxRateService>().As<ITaxRateService>().InstancePerRequest();

			//register named context
			builder.Register<IDbContext>(c => new TaxRateObjectContext(DataSettings.Current.DataConnectionString))
				.Named<IDbContext>(TaxRateObjectContext.ALIASKEY)
				.InstancePerRequest();

			builder.Register<TaxRateObjectContext>(c => new TaxRateObjectContext(DataSettings.Current.DataConnectionString))
				.InstancePerRequest();

            //override required repository with our custom context
            builder.RegisterType<EfRepository<TaxRate>>()
                .As<IRepository<TaxRate>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>(TaxRateObjectContext.ALIASKEY))
                .InstancePerRequest();
        }

        public int Order
        {
            get { return 1; }
        }
    }
}
