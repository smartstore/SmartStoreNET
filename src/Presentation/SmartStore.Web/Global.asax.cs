using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.WebPages;
using FluentValidation.Mvc;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Events;
using SmartStore.Core.Infrastructure;
using SmartStore.Services.Tasks;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Mvc;
using SmartStore.Web.Framework.Mvc.Bundles;
using SmartStore.Web.Framework.Mvc.Routes;
using SmartStore.Web.Framework.Plugins;
using SmartStore.Web.Framework.Themes;
using SmartStore.Web.Framework.Validators;


namespace SmartStore.Web
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {

		public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
			var eventPublisher = EngineContext.Current.Resolve<IEventPublisher>();
			eventPublisher.Publish(new AppRegisterGlobalFiltersEvent {
				Filters = filters
			});
        }

		public static void RegisterRoutes(RouteCollection routes, bool databaseInstalled = true)
        {
			//routes.IgnoreRoute("favicon.ico");
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
			routes.IgnoreRoute("{resource}.ashx/{*pathInfo}");
			routes.IgnoreRoute(".db/{*virtualpath}");

			// register routes (core, admin, plugins, etc)
			var routePublisher = EngineContext.Current.Resolve<IRoutePublisher>();
			routePublisher.RegisterRoutes(routes);
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
            
            // Register MVC areas
            AreaRegistration.RegisterAllAreas();
            
            // fluent validation
            DataAnnotationsModelValidatorProvider.AddImplicitRequiredAttributeForValueTypes = false;
            ModelValidatorProviders.Providers.Add(new FluentValidationModelValidatorProvider(new SmartValidatorFactory()));

			// Routes
			RegisterRoutes(RouteTable.Routes, installed);

			// localize MVC resources
			ClientDataTypeModelValidatorProvider.ResourceClassKey = "MvcLocalization";
			DefaultModelBinder.ResourceClassKey = "MvcLocalization";
			ErrorMessageProvider.SetResourceClassKey("MvcLocalization");

			if (installed)
			{
				// register our themeable razor view engine we use
				ViewEngines.Engines.Add(new ThemeableRazorViewEngine());

				// Global filters
				RegisterGlobalFilters(GlobalFilters.Filters); 
				
				// Bundles
				RegisterBundles(BundleTable.Bundles);

				// register virtual path provider for theming (file inheritance & variables handling)
				HostingEnvironment.RegisterVirtualPathProvider(new ThemingVirtualPathProvider(HostingEnvironment.VirtualPathProvider));
				BundleTable.VirtualPathProvider = HostingEnvironment.VirtualPathProvider;

				// register plugin debug view virtual path provider
				if (HttpContext.Current.IsDebuggingEnabled)
				{
					HostingEnvironment.RegisterVirtualPathProvider(new PluginDebugViewVirtualPathProvider());
				}

                // Install filter
                GlobalFilters.Filters.Add(new InitializeSchedulerFilter());
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
                custom = custom.ToLowerInvariant();
                
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

    }

}
