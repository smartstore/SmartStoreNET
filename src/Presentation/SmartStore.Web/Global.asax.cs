using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.WebPages;
using System.Web.Optimization;
using FluentValidation.Mvc;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Logging;
using SmartStore.Services.Tasks;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.EmbeddedViews;
using SmartStore.Web.Framework.Mvc;
using SmartStore.Web.Framework.Mvc.Bundles;
using SmartStore.Web.Framework.Mvc.Routes;
using SmartStore.Web.Framework.Themes;
using StackExchange.Profiling;
using StackExchange.Profiling.MVCHelpers;
using SmartStore.Core.Events;
using System.Web;
using SmartStore.Core.Domain.Themes;
using SmartStore.Core.Infrastructure.DependencyManagement;
using Autofac;
using Autofac.Integration.Mvc;
using System.IO;
using System.Diagnostics;


namespace SmartStore.Web
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
		private bool _profilingEnabled = false;

		public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
			var eventPublisher = EngineContext.Current.Resolve<IEventPublisher>();
			eventPublisher.Publish(new AppRegisterGlobalFiltersEvent {
				Filters = filters
			});
        }

		public static void RegisterRoutes(RouteCollection routes, bool databaseInstalled = true)
        {
            routes.IgnoreRoute("favicon.ico");
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
			routes.IgnoreRoute(".db/{*virtualpath}");

			if (databaseInstalled)
			{
				// register custom routes (plugins, etc)
				var routePublisher = EngineContext.Current.Resolve<IRoutePublisher>();
				routePublisher.RegisterRoutes(routes);
			}
            
            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                new[] { "SmartStore.Web.Controllers" }
            );
        }

        public static void RegisterBundles(BundleCollection bundles)
        {               
            // register custom bundles
            var bundlePublisher = EngineContext.Current.Resolve<IBundlePublisher>();
            bundlePublisher.RegisterBundles(bundles);
        }

        protected void Application_Start()
        {	
			// we use our own mobile devices support (".Mobile" is reserved). that's why we disable it.
			var mobileDisplayMode = DisplayModeProvider.Instance.Modes.FirstOrDefault(x => x.DisplayModeId == DisplayModeProvider.MobileDisplayModeId);
            if (mobileDisplayMode != null)
                DisplayModeProvider.Instance.Modes.Remove(mobileDisplayMode);

			bool installed = DataSettings.DatabaseIsInstalled();

			if (installed)
			{
				// remove all view engines
				ViewEngines.Engines.Clear();
			}

            // initialize engine context
            EngineContext.Initialize(false);        

            // model binders
            ModelBinders.Binders.DefaultBinder = new SmartModelBinder();

            // Add some functionality on top of the default ModelMetadataProvider
            ModelMetadataProviders.Current = new SmartMetadataProvider();

            // Registering some regular mvc stuff
            AreaRegistration.RegisterAllAreas();
            
            // fluent validation
            DataAnnotationsModelValidatorProvider.AddImplicitRequiredAttributeForValueTypes = false;
            ModelValidatorProviders.Providers.Add(new FluentValidationModelValidatorProvider(new SmartValidatorFactory()));

			// Routes
			RegisterRoutes(RouteTable.Routes, installed);

			if (installed)
			{
				var profilingEnabled = this.ProfilingEnabled;
				
				// register our themeable razor view engine we use
				IViewEngine viewEngine = new ThemeableRazorViewEngine();
				if (profilingEnabled)
				{
					// ...and wrap, if profiling is active
					viewEngine = new ProfilingViewEngine(viewEngine);
					GlobalFilters.Filters.Add(new ProfilingActionFilter());
				}
				ViewEngines.Engines.Add(viewEngine);

				// Global filters
				RegisterGlobalFilters(GlobalFilters.Filters);
				
				// Bundles
				RegisterBundles(BundleTable.Bundles);

				// register virtual path provider for theme variables
				HostingEnvironment.RegisterVirtualPathProvider(new ThemeVarsVirtualPathProvider(HostingEnvironment.VirtualPathProvider));
				BundleTable.VirtualPathProvider = HostingEnvironment.VirtualPathProvider;

				// register virtual path provider for embedded views
				var embeddedViewResolver = EngineContext.Current.Resolve<IEmbeddedViewResolver>();
				var embeddedProvider = new EmbeddedViewVirtualPathProvider(embeddedViewResolver.GetEmbeddedViews());
				HostingEnvironment.RegisterVirtualPathProvider(embeddedProvider);

				// start scheduled tasks
				TaskManager.Instance.Initialize();
				TaskManager.Instance.Start();
			}
			else
			{
				// app not installed

				// Install filter
				GlobalFilters.Filters.Add(new HandleInstallFilter());
			}

        }

        public override string GetVaryByCustomString(HttpContext context, string custom)
        {
            string result = string.Empty;
            
            if (DataSettings.DatabaseIsInstalled())
            {
                custom = custom.ToLower();
                
                switch (custom) 
                {
                    case "theme":
                        result = EngineContext.Current.Resolve<IThemeContext>().CurrentTheme.ThemeName;
                        break;
                    case "store":
                        result = EngineContext.Current.Resolve<IStoreContext>().CurrentStore.Id.ToString();
                        break;
                    case "theme_store":
                        result = "{0}-{1}".FormatInvariant(
                            EngineContext.Current.Resolve<IThemeContext>().CurrentTheme.ThemeName,
                            EngineContext.Current.Resolve<IStoreContext>().CurrentStore.Id.ToString());
                        break;
                }
            }

            if (result.HasValue())
            {
                return result;
            }

            return base.GetVaryByCustomString(context, custom);
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
			//var installed = DataSettings.DatabaseIsInstalled();

			// ignore static resources
			if (WebHelper.IsStaticResourceRequested(this.Request))
				return;

			_profilingEnabled = this.ProfilingEnabled;

			if (_profilingEnabled)
			{
				MiniProfiler.Start();
			}
        }

        protected void Application_EndRequest(object sender, EventArgs e)
        {
			// Don't resolve dependencies from now on.
			
			// ignore static resources
			if (WebHelper.IsStaticResourceRequested(this.Request))
				return;

			if (_profilingEnabled)
			{
				// stop mini profiler
				MiniProfiler.Stop();
			}
        }
		
        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {
            // [...]
        }

        protected void Application_Error(Object sender, EventArgs e)
        {
            //disable compression (if enabled). More info - http://stackoverflow.com/questions/3960707/asp-net-mvc-weird-characters-in-error-page
            //log error
            LogException(Server.GetLastError());
        }

        protected void LogException(Exception exc)
        {
            if (exc == null)
                return;
            
            if (!DataSettings.DatabaseIsInstalled())
                return;
            
            try
            {
                var logger = EngineContext.Current.Resolve<ILogger>();
                var workContext = EngineContext.Current.Resolve<IWorkContext>();
                logger.Error(exc.Message, exc, workContext.CurrentCustomer);
            }
            catch (Exception)
            {
                //don't throw new exception if occurs
            }
        }

		protected bool ProfilingEnabled
		{
			get
			{
				return DataSettings.DatabaseIsInstalled() && EngineContext.Current.Resolve<StoreInformationSettings>().DisplayMiniProfilerInPublicStore;
			}
		}

    }

}
