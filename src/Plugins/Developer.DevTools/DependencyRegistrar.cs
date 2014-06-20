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
using SmartStore.Plugin.Developer.DevTools.Services;
using SmartStore.Web.Framework.Controllers;

namespace SmartStore.Plugin.Developer.DevTools
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder)
        {
			builder.RegisterType<ProfilerService>().As<IProfilerService>().InstancePerRequest();
			
			if (PluginManager.IsActivePluginAssembly(this.GetType().Assembly))
			{
				// intercept ALL public store controller actions
				builder.RegisterType<ProfilerFilter>().AsActionFilterFor<PublicControllerBase>();

				//// intercept CatalogController's Product action
				//builder.RegisterType<SampleResultFilter>().AsResultFilterFor<CatalogController>(x => x.Product(default(int), default(string))).InstancePerRequest();
				//builder.RegisterType<SampleActionFilter>().AsActionFilterFor<PublicControllerBase>().InstancePerRequest();
				//// intercept CheckoutController's Index action (to hijack the checkout or payment workflow)
				//builder.RegisterType<SampleCheckoutFilter>().AsActionFilterFor<CheckoutController>(x => x.Index()).InstancePerRequest();
			}
        }

        public int Order
        {
            get { return 1; }
        }
    }
}
