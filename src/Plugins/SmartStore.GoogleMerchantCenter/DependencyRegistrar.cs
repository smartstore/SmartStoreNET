using Autofac;
using Autofac.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Infrastructure.DependencyManagement;
using SmartStore.Data;
using SmartStore.GoogleMerchantCenter.Data;
using SmartStore.GoogleMerchantCenter.Domain;
using SmartStore.GoogleMerchantCenter.Services;

namespace SmartStore.GoogleMerchantCenter
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, bool isActiveModule)
        {
            builder.RegisterType<GoogleFeedService>().As<IGoogleFeedService>().InstancePerRequest();

            //register named context
            builder.Register<IDbContext>(c => new GoogleProductObjectContext(DataSettings.Current.DataConnectionString))
                .Named<IDbContext>(GoogleProductObjectContext.ALIASKEY)
                .InstancePerRequest();

            builder.Register<GoogleProductObjectContext>(c => new GoogleProductObjectContext(DataSettings.Current.DataConnectionString))
                .InstancePerRequest();

            //override required repository with our custom context
            builder.RegisterType<EfRepository<GoogleProductRecord>>()
                .As<IRepository<GoogleProductRecord>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>(GoogleProductObjectContext.ALIASKEY))
                .InstancePerRequest();
        }

        public int Order => 1;
    }
}
