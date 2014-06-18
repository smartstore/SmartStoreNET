using System;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Reflection;
using Autofac;
using Autofac.Core;
using Autofac.Integration.Mvc;
using SmartStore.Core.Plugins;
using SmartStore.Core.Data;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Infrastructure.DependencyManagement;
using SmartStore.Plugin.Developer.DevTools.Filters;
using SmartStore.Web.Controllers;
using SmartStore.Plugin.Developer.DevTools.Services;

namespace SmartStore.Plugin.Developer.DevTools
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder)
        {
			builder.RegisterType<ProfilerService>().As<IProfilerService>().InstancePerRequest();
			
			if (PluginManager.IsActivePluginAssembly(this.GetType().Assembly))
			{
				builder.RegisterType<TestResultFilter>().AsResultFilterFor<CatalogController>(x => x.Product(default(int), default(string))).InstancePerRequest();
				builder.RegisterType<HideBannerFilter>().AsResultFilterFor<CommonController>(x => x.Footer()).InstancePerRequest();
				builder.RegisterType<TestActionFilter>().AsActionFilterFor<PublicControllerBase>().InstancePerRequest();
				builder.RegisterType<MyCheckoutFilter>().AsActionFilterFor<CheckoutController>(x => x.Index()).InstancePerRequest();

				builder.RegisterType<ProfilerFilter>().AsActionFilterFor<PublicControllerBase>();
			}
        }

        public int Order
        {
            get { return 1; }
        }
    }
}
