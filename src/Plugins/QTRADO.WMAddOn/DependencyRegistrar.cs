using Autofac;
using Autofac.Core;
using Autofac.Integration.Mvc;

using QTRADO.WMAddOn.Data;
using QTRADO.WMAddOn.Domain;
using QTRADO.WMAddOn.Services;

using SmartStore.Core.Data;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Infrastructure.DependencyManagement;
using SmartStore.Data;


namespace QTRADO.WMAddOn
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, bool isActiveModule)
        {
            builder.RegisterType<WMAddOnService>().As<IWMAddOnService>().InstancePerRequest();

            //register named context
            builder.Register<IDbContext>(c => new WMAddOnObjectContext(DataSettings.Current.DataConnectionString))
                .Named<IDbContext>(WMAddOnObjectContext.ALIASKEY)
                .InstancePerRequest();

            builder.Register(c => new WMAddOnObjectContext(DataSettings.Current.DataConnectionString))
                .InstancePerRequest();

            //override required repository with our custom context
            builder.RegisterType<EfRepository<Grossist>>()
                .As<IRepository<Grossist>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>(WMAddOnObjectContext.ALIASKEY))
                .InstancePerRequest();

        }

        public int Order
        {
            get { return 1; }
        }
    }
}
