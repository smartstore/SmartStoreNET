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
using SmartStore.Services.Logging;
using SmartStore.Services.Tasks;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.EmbeddedViews;
using SmartStore.Web.Framework.Mvc;
using SmartStore.Web.Framework.Mvc.Bundles;
using SmartStore.Web.Framework.Mvc.Routes;
using SmartStore.Web.Framework.Themes;
using StackExchange.Profiling;
using StackExchange.Profiling.MVCHelpers;
using SmartStore.Services.Events;
using SmartStore.Core.Events;
using System.Web;
using dotless.Core.configuration;
using SmartStore.Core.Domain.Themes;


namespace SmartStore.Web
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            //do not register HandleErrorAttribute. use classic error handling mode
            //filters.Add(new HandleErrorAttribute());

			var eventPublisher = EngineContext.Current.Resolve<IEventPublisher>();	// codehint: sm-add
			eventPublisher.Publish(new AppRegisterGlobalFiltersEvent {
				Filters = filters
			});
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("favicon.ico");
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            
            //register custom routes (plugins, etc)
            var routePublisher = EngineContext.Current.Resolve<IRoutePublisher>();
            routePublisher.RegisterRoutes(routes);
            
            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                new[] { "SmartStore.Web.Controllers" }
            );
        }

        /// <summary>
        /// <remarks>codehint: sm-add</remarks>
        /// </summary>
        public static void RegisterBundles(BundleCollection bundles, bool databaseInstalled = true)
        {               
            // register custom bundles
            var bundlePublisher = EngineContext.Current.Resolve<IBundlePublisher>();
            bundlePublisher.RegisterBundles(bundles);

            var lessConfig = new WebConfigConfigurationLoader().GetConfiguration();

            // it's way better to depend minifying on DebugMode than 
            // the explicit definition in web.config, so overwrite here!
            lessConfig.MinifyOutput = HttpContext.Current.IsDebuggingEnabled;

            // handle theme settings
            if (databaseInstalled)
            {
                var themeSettings = EngineContext.Current.Resolve<ThemeSettings>();

                // dotless config
                if (themeSettings.CssCacheEnabled > 0 || themeSettings.CssMinifyEnabled > 0)
                {
                    if (themeSettings.CssCacheEnabled > 0)
                        lessConfig.CacheEnabled = themeSettings.CssCacheEnabled == 2;
                    if (themeSettings.CssMinifyEnabled > 0)
                        lessConfig.MinifyOutput = themeSettings.CssMinifyEnabled == 2;
                }

                // bundling config
                if (themeSettings.BundleOptimizationEnabled > 0)
                    BundleTable.EnableOptimizations = themeSettings.BundleOptimizationEnabled == 2;
            }
        }

        protected void Application_Start()
        {
            
            // codehint: sm-add
            //MiniProfilerEF.Initialize_EF42();

            //we use our own mobile devices support (".Mobile" is reserved). that's why we disable it.
			var mobileDisplayMode = DisplayModeProvider.Instance.Modes
				.FirstOrDefault(x => x.DisplayModeId == DisplayModeProvider.MobileDisplayModeId);
            if (mobileDisplayMode != null)
                DisplayModeProvider.Instance.Modes.Remove(mobileDisplayMode);

            
            //initialize engine context
            EngineContext.Initialize(false);

            bool databaseInstalled = DataSettingsHelper.DatabaseIsInstalled();

            //set dependency resolver
            var dependencyResolver = new SmartDependencyResolver();
            DependencyResolver.SetResolver(dependencyResolver);

            //model binders
            ModelBinders.Binders.DefaultBinder = new SmartModelBinder();

            if (databaseInstalled)
            {
                //remove all view engines
                ViewEngines.Engines.Clear();
                //except the themeable razor view engine we use
                ViewEngines.Engines.Add(new ThemeableRazorViewEngine());
            }

            //Add some functionality on top of the default ModelMetadataProvider
            ModelMetadataProviders.Current = new SmartMetadataProvider();

            //Registering some regular mvc stuff
            AreaRegistration.RegisterAllAreas();

            // codehint: sm-add
            RegisterGlobalFilters(GlobalFilters.Filters);

            RegisterRoutes(RouteTable.Routes);

            // codehint: sm-add
            RegisterBundles(BundleTable.Bundles, databaseInstalled);

            if (!databaseInstalled)
            {
                GlobalFilters.Filters.Add(new HandleInstallFilter());
            }

            //StackExchange profiler
            if (databaseInstalled && EngineContext.Current.Resolve<StoreInformationSettings>().DisplayMiniProfilerInPublicStore)
            {
                GlobalFilters.Filters.Add(new ProfilingActionFilter());
            }
            
            //fluent validation
            DataAnnotationsModelValidatorProvider.AddImplicitRequiredAttributeForValueTypes = false;
            ModelValidatorProviders.Providers.Add(new FluentValidationModelValidatorProvider(new SmartValidatorFactory()));

            //register virtual path provider for embedded views
            var embeddedViewResolver = EngineContext.Current.Resolve<IEmbeddedViewResolver>();
            var embeddedProvider = new EmbeddedViewVirtualPathProvider(embeddedViewResolver.GetEmbeddedViews());
            HostingEnvironment.RegisterVirtualPathProvider(embeddedProvider);

            //start scheduled tasks
            if (databaseInstalled)
            {
                TaskManager.Instance.Initialize();
                TaskManager.Instance.Start();
            }
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            // must be at head, because the BizUrlMapper must also handle static html files
            EngineContext.Current.Resolve<IEventPublisher>().Publish(new AppBeginRequestEvent
            {
                Context = HttpContext.Current
            });
            
            // ignore static resources
			var webHelper = EngineContext.Current.Resolve<IWebHelper>();
			if (webHelper.IsStaticResource(this.Request))
				return;

            if (DataSettingsHelper.DatabaseIsInstalled() && 
                EngineContext.Current.Resolve<StoreInformationSettings>().DisplayMiniProfilerInPublicStore)
            {
                MiniProfiler.Start();
            }
        }

        protected void Application_EndRequest(object sender, EventArgs e)
        {
			// ignore static resources
			var webHelper = EngineContext.Current.Resolve<IWebHelper>();
			if (webHelper.IsStaticResource(this.Request))
				return;

            if (DataSettingsHelper.DatabaseIsInstalled() && EngineContext.Current.Resolve<StoreInformationSettings>().DisplayMiniProfilerInPublicStore)
            {
                // stop as early as you can, even earlier with MvcMiniProfiler.MiniProfiler.Stop(discardResults: true);
                MiniProfiler.Stop();
            }

			// codehint: sm-add
			EngineContext.Current.Resolve<IEventPublisher>().Publish(new AppEndRequestEvent {
				Context = HttpContext.Current
			});

            //////dispose registered resources
            //////we do not register AutofacRequestLifetimeHttpModule as IHttpModule 
            //////because it disposes resources before this Application_EndRequest method is called
            //////and this case the code above will throw an exception
            //try
            //{
            //    AutofacRequestLifetimeHttpModule.ContextEndRequest(sender, e);
            //}
            //catch { }
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
            
            if (!DataSettingsHelper.DatabaseIsInstalled())
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
    }

}
