using System;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using System.Web.WebPages;
using AutoMapper;
using FluentValidation.Mvc;
using JavaScriptEngineSwitcher.Core;
using JavaScriptEngineSwitcher.Msie;
using JavaScriptEngineSwitcher.V8;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Events;
using SmartStore.Core.Infrastructure;
using SmartStore.Services.Customers;
using SmartStore.Services.Tasks; 
using SmartStore.Utilities;
using SmartStore.Web.Framework.Bundling;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Localization;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Framework.Plugins;
using SmartStore.Web.Framework.Routing;
using SmartStore.Web.Framework.Theming;
using SmartStore.Web.Framework.Validators;

namespace SmartStore.Web
{
	// Note: For instructions on enabling IIS6 or IIS7 classic mode, 
	// visit http://go.microsoft.com/?LinkId=9394801

	public class MvcApplication : System.Web.HttpApplication
	{
		public static void RegisterGlobalFilters(GlobalFilterCollection filters, IEngine engine)
		{
			var eventPublisher = engine.Resolve<IEventPublisher>();
			eventPublisher.Publish(new AppRegisterGlobalFiltersEvent
			{
				Filters = filters
			});
		}

		public static void RegisterRoutes(RouteCollection routes, IEngine engine, bool databaseInstalled = true)
		{
			//routes.IgnoreRoute("favicon.ico");
			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
			routes.IgnoreRoute("{resource}.ashx/{*pathInfo}");
			routes.IgnoreRoute(".db/{*virtualpath}");

			// register routes (core, admin, plugins, etc)
			var routePublisher = engine.Resolve<IRoutePublisher>();
			routePublisher.RegisterRoutes(routes);
		}

		public static void RegisterBundles(BundleCollection bundles, IEngine engine)
		{
			// register custom bundles
			var bundlePublisher = engine.Resolve<IBundlePublisher>();
			bundlePublisher.RegisterBundles(bundles);
		}

		public static void RegisterClassMaps(IEngine engine)
		{
			// register AutoMapper maps
			var profileTypes = engine.Resolve<ITypeFinder>().FindClassesOfType<Profile>();

			if (profileTypes.Any())
			{
				Mapper.Initialize(cfg => {
					foreach (var profileType in profileTypes)
					{
						cfg.AddProfile(profileType);
					}
				});
			}
		}

		public static void RegisterJsEngines()
		{
			JsEngineSwitcher engineSwitcher = JsEngineSwitcher.Instance;
			engineSwitcher.EngineFactories
				.AddV8()
				.AddMsie(new MsieSettings
				{
					UseEcmaScript5Polyfill = true,
					UseJson2Library = true
				});

			engineSwitcher.DefaultEngineName = V8JsEngine.EngineName;
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
				// Remove all view engines
				ViewEngines.Engines.Clear();
			}

			// Initialize engine context
			var engine = EngineContext.Initialize(false);

			// Model binders
			ModelBinders.Binders.DefaultBinder = new SmartModelBinder();

			// Add some functionality on top of the default ModelMetadataProvider
			ModelMetadataProviders.Current = new SmartMetadataProvider();

			// Register MVC areas
			AreaRegistration.RegisterAllAreas();

			// Fluent validation
			FluentValidationModelValidatorProvider.Configure(x =>
			{
				x.ValidatorFactory = new SmartValidatorFactory();
			});
			
			// Routes
			RegisterRoutes(RouteTable.Routes, engine, installed);

			// localize MVC resources
			ClientDataTypeModelValidatorProvider.ResourceClassKey = "MvcLocalization";
			DefaultModelBinder.ResourceClassKey = "MvcLocalization";
			ErrorMessageProvider.SetResourceClassKey("MvcLocalization");

			// Register JsEngine
			RegisterJsEngines();

			if (installed)
			{
				// register our themeable razor view engine we use
				ViewEngines.Engines.Add(new ThemeableRazorViewEngine());

				// Global filters
				RegisterGlobalFilters(GlobalFilters.Filters, engine);

				// Bundles
				RegisterBundles(BundleTable.Bundles, engine);

				// register virtual path provider for theming (file inheritance & variables handling)
				HostingEnvironment.RegisterVirtualPathProvider(new ThemingVirtualPathProvider(HostingEnvironment.VirtualPathProvider));
				
				// register plugin debug view virtual path provider
				if (HttpContext.Current.IsDebuggingEnabled && CommonHelper.IsDevEnvironment)
				{
					HostingEnvironment.RegisterVirtualPathProvider(new PluginDebugViewVirtualPathProvider());
				}

				BundleTable.VirtualPathProvider = HostingEnvironment.VirtualPathProvider;

				// "throw-away" filter for task scheduler initialization (the filter removes itself when processed)
				GlobalFilters.Filters.Add(new InitializeSchedulerFilter());

				// register AutoMapper class maps
				RegisterClassMaps(engine);
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

		public void AnonymousIdentification_Creating(object sender, AnonymousIdentificationEventArgs args)
		{
			try
			{
				if (DataSettings.DatabaseIsInstalled())
				{
					var customerService = EngineContext.Current.Resolve<ICustomerService>();
					var customer = customerService.FindGuestCustomerByClientIdent(maxAgeSeconds: 180);
					if (customer != null)
					{
						// We found our anonymous visitor: don't let ASP.NET create a new id.
						args.AnonymousID = customer.CustomerGuid.ToString();
					}
				}
			}
			catch { }
		}
	}
}
