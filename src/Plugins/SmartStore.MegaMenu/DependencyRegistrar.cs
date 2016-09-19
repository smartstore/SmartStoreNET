using Autofac;
using Autofac.Core;
using Autofac.Integration.Mvc;
using SmartStore.Core.Data;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Infrastructure.DependencyManagement;
using SmartStore.Core.Plugins;
using SmartStore.Data;
using SmartStore.MegaMenu.Data;
using SmartStore.MegaMenu.Domain;
using SmartStore.MegaMenu.Filters;
using SmartStore.MegaMenu.Services;
using SmartStore.Web.Controllers;

namespace SmartStore.MegaMenu
{
	public class DependencyRegistrar : IDependencyRegistrar
	{
		public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, bool isActiveModule)
		{
			if (isActiveModule)
			{
                builder.RegisterType<MegaMenuService>().As<IMegaMenuService>().InstancePerRequest();

                builder.RegisterType<MegaMenuFilter>().AsActionFilterFor<CatalogController>(x => x.Megamenu(0, 0)).InstancePerRequest();
			}

            //register named context
            builder.Register<IDbContext>(c => new MegaMenuObjectContext(DataSettings.Current.DataConnectionString))
                .Named<IDbContext>(MegaMenuObjectContext.ALIASKEY)
                .InstancePerRequest();

            builder.Register<MegaMenuObjectContext>(c => new MegaMenuObjectContext(DataSettings.Current.DataConnectionString))
                .InstancePerRequest();

            //override required repository with our custom context
            builder.RegisterType<EfRepository<MegaMenuRecord>>()
                .As<IRepository<MegaMenuRecord>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>(MegaMenuObjectContext.ALIASKEY))
                .InstancePerRequest();

        }

		public int Order
		{
			get { return 1; }
		}
	}
}
