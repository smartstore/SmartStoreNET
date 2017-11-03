using Autofac;
using Autofac.Integration.Mvc;
using SmartStore.Core.Data;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Infrastructure.DependencyManagement;
using SmartStore.Core.Logging;
using SmartStore.DevTools.Filters;
using SmartStore.DevTools.Services;
using SmartStore.Web.Controllers;
using SmartStore.Web.Framework.Controllers;

namespace SmartStore.DevTools
{
	public class DependencyRegistrar : IDependencyRegistrar
    {
		public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, bool isActiveModule)
        {
			if (isActiveModule)
			{
				builder.RegisterType<MiniProfilerChronometer>().As<IChronometer>().InstancePerRequest();

				if (DataSettings.DatabaseIsInstalled())
				{
					// intercept ALL public store controller actions
					builder.RegisterType<ProfilerFilter>().AsActionFilterFor<SmartController>();
					builder.RegisterType<WidgetZoneFilter>().AsActionFilterFor<SmartController>();
					builder.RegisterType<MachineNameFilter>().AsResultFilterFor<SmartController>();

                    // Add an action to product detail offer actions
                    //builder.RegisterType<SampleProductDetailActionFilter>()
                    //    .AsActionFilterFor<ProductController>(x => x.ProductDetails(default(int), default(string), null))
                    //    .InstancePerRequest();
                    
                    //// intercept CatalogController's Product action
                    //builder.RegisterType<SampleResultFilter>().AsResultFilterFor<CatalogController>(x => x.Product(default(int), default(string))).InstancePerRequest();
                    //builder.RegisterType<SampleActionFilter>().AsActionFilterFor<PublicControllerBase>().InstancePerRequest();
                    //// intercept CheckoutController's Index action (to hijack the checkout or payment workflow)
                    //builder.RegisterType<SampleCheckoutFilter>().AsActionFilterFor<CheckoutController>(x => x.Index()).InstancePerRequest();
                }
			}
		}

        public int Order
        {
            get { return 1; }
        }
    }
}
