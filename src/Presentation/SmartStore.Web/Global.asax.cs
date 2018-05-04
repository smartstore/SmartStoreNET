using System;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
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
using SmartStore.Core.Themes;
using SmartStore.Services.Tasks;
using SmartStore.Web.Framework.Bundling;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Localization;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Framework.Routing;
using SmartStore.Web.Framework.Theming;
using SmartStore.Web.Framework.Theming.Assets;
using SmartStore.Web.Framework.Validators;
using System.Net;

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
			// SSL & TLS
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
			ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, errors) => true;
			
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

			// VPPs
			RegisterVirtualPathProviders();

			if (installed)
			{
				// register our themeable razor view engine we use
				ViewEngines.Engines.Add(new ThemeableRazorViewEngine());

				// Global filters
				RegisterGlobalFilters(GlobalFilters.Filters, engine);

				// Bundles
				RegisterBundles(BundleTable.Bundles, engine);

				// "throw-away" filter for task scheduler initialization (the filter removes itself when processed)
				GlobalFilters.Filters.Add(new InitializeSchedulerFilter(), int.MinValue);

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

		private void RegisterVirtualPathProviders()
		{
			var vppSystem = HostingEnvironment.VirtualPathProvider;

			// register virtual path provider for bundling (Sass & variables handling)
			BundleTable.VirtualPathProvider = new BundlingVirtualPathProvider(vppSystem);

			if (DataSettings.DatabaseIsInstalled())
			{
				var vppTheme = new ThemingVirtualPathProvider(vppSystem);

				// register virtual path provider for theming (file inheritance handling etc.)
				HostingEnvironment.RegisterVirtualPathProvider(vppTheme);
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
