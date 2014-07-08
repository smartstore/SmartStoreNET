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
using SmartStore.Web.Framework.Mvc;
using SmartStore.Web.Framework.Mvc.Bundles;
using SmartStore.Web.Framework.Mvc.Routes;
using SmartStore.Web.Framework.Themes;
using SmartStore.Core.Events;
using System.Web; 
using SmartStore.Core.Domain.Themes;
using SmartStore.Core.Infrastructure.DependencyManagement;
using Autofac;
using Autofac.Integration.Mvc;
using System.IO;
using System.Diagnostics;
using SmartStore.Web.Framework.Plugins;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Validators;
using SmartStore.Web.Controllers; 


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

			if (installed)
			{
				// register our themeable razor view engine we use
				ViewEngines.Engines.Add(new ThemeableRazorViewEngine());

				// Global filters
				RegisterGlobalFilters(GlobalFilters.Filters);
				
				// Bundles
				RegisterBundles(BundleTable.Bundles);

				// register virtual path provider for theme variables
				HostingEnvironment.RegisterVirtualPathProvider(new ThemeVarsVirtualPathProvider(HostingEnvironment.VirtualPathProvider));
				BundleTable.VirtualPathProvider = HostingEnvironment.VirtualPathProvider;

				// register plugin debug view virtual path provider
				if (HttpContext.Current.IsDebuggingEnabled)
				{
					HostingEnvironment.RegisterVirtualPathProvider(new PluginDebugViewVirtualPathProvider());
				}

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
		

        protected void Application_Error(object sender, EventArgs e)
        {
			var exception = Server.GetLastError();

			// TODO: make a setting and don't log error 404 if set
			LogException(exception);
			
			var httpException = exception as HttpException;

			// don't return 404 view if a static resource was requested
			if (httpException != null && httpException.GetHttpCode() == 404 && WebHelper.IsStaticResourceRequested(Request))
				return;

			var httpContext = ((MvcApplication)sender).Context;
			var currentController = " ";
			var currentAction = " ";
			var currentRouteData = RouteTable.Routes.GetRouteData(new HttpContextWrapper(httpContext));

			if (currentRouteData != null)
			{
				if (currentRouteData.Values["controller"] != null && !String.IsNullOrEmpty(currentRouteData.Values["controller"].ToString()))
					currentController = currentRouteData.Values["controller"].ToString();
				if (currentRouteData.Values["action"] != null && !String.IsNullOrEmpty(currentRouteData.Values["action"].ToString()))
					currentAction = currentRouteData.Values["action"].ToString();
			}

			var errorController = new ErrorController();
			var routeData = new RouteData();
			var errorAction = "Index";

			if (httpException != null)
			{
				switch (httpException.GetHttpCode())
				{
					case 404:
						errorAction = "NotFound";
						break;
					// TODO: more?
				}
			}			

			httpContext.ClearError();
			httpContext.Response.Clear();
			httpContext.Response.StatusCode = httpException != null ? httpException.GetHttpCode() : 500;
			httpContext.Response.TrySkipIisCustomErrors = true;

			routeData.Values["controller"] = "Error";
			routeData.Values["action"] = errorAction;

			errorController.ViewData.Model = new HandleErrorInfo(exception, currentController, currentAction);
			((IController)errorController).Execute(new RequestContext(new HttpContextWrapper(httpContext), routeData));
        }

        protected void LogException(Exception exception)
        {
            if (exception == null)
                return;
            
            if (!DataSettings.DatabaseIsInstalled())
                return;

			//// ignore 404 HTTP errors
			//var httpException = exception as HttpException;
			//if (httpException != null && httpException.GetHttpCode() == 404)
			//	return;

            try
            {
                var logger = EngineContext.Current.Resolve<ILogger>();
                var workContext = EngineContext.Current.Resolve<IWorkContext>();
                logger.Error(exception.Message, exception, workContext.CurrentCustomer);
            }
            catch
            {
                // don't throw new exception
            }
        }

    }

}
