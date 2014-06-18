using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using Autofac;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Integration.Mvc;
using Autofac.Integration.WebApi;
using SmartStore.Core;
using SmartStore.Core.Caching;
using SmartStore.Core.Configuration;
using SmartStore.Core.Data;
using SmartStore.Core.Events;
using SmartStore.Core.Fakes;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Infrastructure.DependencyManagement;
using SmartStore.Core.Plugins;
using SmartStore.Data;
using SmartStore.Services.Affiliates;
using SmartStore.Services.Authentication;
using SmartStore.Services.Authentication.External;
using SmartStore.Services.Blogs;
using SmartStore.Services.Catalog;
using SmartStore.Services.Cms;
using SmartStore.Services.Common;
using SmartStore.Services.Configuration;
using SmartStore.Services.Customers;
using SmartStore.Services.Directory;
using SmartStore.Services.Discounts;
using SmartStore.Services.ExportImport;
using SmartStore.Services.Forums;
using SmartStore.Services.Helpers;
using SmartStore.Services.Localization;
using SmartStore.Core.Logging;
using SmartStore.Services.Media;
using SmartStore.Services.Messages;
using SmartStore.Services.News;
using SmartStore.Services.Orders;
using SmartStore.Services.Payments;
using SmartStore.Services.Polls;
using SmartStore.Services.Security;
using SmartStore.Services.Seo;
using SmartStore.Services.Shipping;
using SmartStore.Services.Tasks;
using SmartStore.Services.Tax;
using SmartStore.Services.Topics;
using SmartStore.Web.Framework.EmbeddedViews;
using SmartStore.Web.Framework.Mvc.Routes;
using SmartStore.Web.Framework.Mvc.Bundles;
using SmartStore.Web.Framework.Themes;
using SmartStore.Web.Framework.UI;
using SmartStore.Services.Filter;
using SmartStore.Core.Data.Hooks;
using SmartStore.Core.Themes;
using SmartStore.Services.Themes;
using SmartStore.Services.Stores;
using SmartStore.Web.Framework.WebApi.Configuration;
using SmartStore.Services;
using Module = Autofac.Module;
using SmartStore.Core.Localization;
using SmartStore.Web.Framework.Localization;
using SmartStore.Core.Email;
using Autofac.Features.Metadata;
using SmartStore.Services.Events;
using System.Diagnostics;
using SmartStore.Services.Logging;
using SmartStore.Core.Packaging;
using SmartStore.Core.IO.Media;
using SmartStore.Core.IO.VirtualPath;
using SmartStore.Core.IO.WebSite;
using SmartStore.Web.Framework.WebApi;

namespace SmartStore.Web.Framework
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder)
        {
			// modules
			builder.RegisterModule(new DbModule(typeFinder));
			builder.RegisterModule(new CachingModule());
			builder.RegisterModule(new LocalizationModule());
			builder.RegisterModule(new LoggingModule());
			builder.RegisterModule(new EventModule(typeFinder));
			builder.RegisterModule(new MessagingModule());
			builder.RegisterModule(new WebModule(typeFinder));
			builder.RegisterModule(new WebApiModule(typeFinder));
			builder.RegisterModule(new UiModule(typeFinder));
			builder.RegisterModule(new IOModule());
			builder.RegisterModule(new PackagingModule());

			// sources
			builder.RegisterSource(new SettingsSource());

            // web helper
            builder.RegisterType<WebHelper>().As<IWebHelper>().InstancePerRequest();
            
            // plugins
            builder.RegisterType<PluginFinder>().As<IPluginFinder>().SingleInstance(); // xxx (http)

            // work context
            builder.RegisterType<WebWorkContext>().As<IWorkContext>().WithStaticCache().InstancePerRequest();
			
			// store context
			builder.RegisterType<WebStoreContext>().As<IStoreContext>().InstancePerRequest();

            // services
            builder.RegisterType<BackInStockSubscriptionService>().As<IBackInStockSubscriptionService>().InstancePerRequest();
            builder.RegisterType<CategoryService>().As<ICategoryService>().WithRequestCache().InstancePerRequest();
            builder.RegisterType<CompareProductsService>().As<ICompareProductsService>().InstancePerRequest();
            builder.RegisterType<RecentlyViewedProductsService>().As<IRecentlyViewedProductsService>().InstancePerRequest();
			builder.RegisterType<ManufacturerService>().As<IManufacturerService>().WithRequestCache().InstancePerRequest();
            builder.RegisterType<PriceCalculationService>().As<IPriceCalculationService>().InstancePerRequest();
            builder.RegisterType<PriceFormatter>().As<IPriceFormatter>().InstancePerRequest();
            builder.RegisterType<ProductAttributeFormatter>().As<IProductAttributeFormatter>().InstancePerLifetimeScope();
            builder.RegisterType<ProductAttributeParser>().As<IProductAttributeParser>().InstancePerRequest();
			builder.RegisterType<ProductAttributeService>().As<IProductAttributeService>().WithRequestCache().InstancePerRequest();
			builder.RegisterType<ProductService>().As<IProductService>().WithRequestCache().InstancePerRequest();
            builder.RegisterType<CopyProductService>().As<ICopyProductService>().InstancePerRequest();
			builder.RegisterType<SpecificationAttributeService>().As<ISpecificationAttributeService>().WithRequestCache().InstancePerRequest();
            builder.RegisterType<ProductTemplateService>().As<IProductTemplateService>().InstancePerRequest();
			builder.RegisterType<CategoryTemplateService>().As<ICategoryTemplateService>().WithRequestCache().InstancePerRequest();
			builder.RegisterType<ManufacturerTemplateService>().As<IManufacturerTemplateService>().WithRequestCache().InstancePerRequest();
			builder.RegisterType<ProductTagService>().As<IProductTagService>().WithStaticCache().InstancePerRequest();

            builder.RegisterType<AffiliateService>().As<IAffiliateService>().InstancePerRequest();
            builder.RegisterType<AddressService>().As<IAddressService>().InstancePerRequest();
			builder.RegisterType<GenericAttributeService>().As<IGenericAttributeService>().WithRequestCache().InstancePerRequest();
            builder.RegisterType<FulltextService>().As<IFulltextService>().InstancePerRequest();
            builder.RegisterType<MaintenanceService>().As<IMaintenanceService>().InstancePerRequest();

			builder.RegisterType<CustomerContentService>().As<ICustomerContentService>().WithRequestCache().InstancePerRequest();
			builder.RegisterType<CustomerService>().As<ICustomerService>().WithRequestCache().InstancePerRequest();
            builder.RegisterType<CustomerRegistrationService>().As<ICustomerRegistrationService>().InstancePerRequest();
            builder.RegisterType<CustomerReportService>().As<ICustomerReportService>().InstancePerRequest();

            builder.RegisterType<PermissionService>().As<IPermissionService>().WithStaticCache() .InstancePerRequest();

            builder.RegisterType<AclService>().As<IAclService>().WithStaticCache().InstancePerRequest();

            builder.RegisterType<GeoCountryLookup>().As<IGeoCountryLookup>().InstancePerRequest();
			builder.RegisterType<CountryService>().As<ICountryService>().WithRequestCache().InstancePerRequest();
			builder.RegisterType<CurrencyService>().As<ICurrencyService>().WithRequestCache().InstancePerRequest();

			builder.RegisterType<DeliveryTimeService>().As<IDeliveryTimeService>().WithRequestCache().InstancePerRequest();

			builder.RegisterType<MeasureService>().As<IMeasureService>().WithRequestCache().InstancePerRequest();
			builder.RegisterType<StateProvinceService>().As<IStateProvinceService>().WithRequestCache().InstancePerRequest();

			builder.RegisterType<StoreService>().As<IStoreService>().WithRequestCache().InstancePerRequest();
			builder.RegisterType<StoreMappingService>().As<IStoreMappingService>().WithStaticCache().InstancePerRequest();

			builder.RegisterType<DiscountService>().As<IDiscountService>().WithRequestCache().InstancePerRequest();

            builder.RegisterType<SettingService>().As<ISettingService>().WithStaticCache().InstancePerRequest();

            builder.RegisterType<DownloadService>().As<IDownloadService>().InstancePerRequest();
            builder.RegisterType<ImageCache>().As<IImageCache>().InstancePerRequest();
            builder.RegisterType<ImageResizerService>().As<IImageResizerService>().SingleInstance();
            builder.RegisterType<PictureService>().As<IPictureService>().InstancePerRequest();

            builder.RegisterType<CheckoutAttributeFormatter>().As<ICheckoutAttributeFormatter>().InstancePerRequest();
            builder.RegisterType<CheckoutAttributeParser>().As<ICheckoutAttributeParser>().InstancePerRequest();
			builder.RegisterType<CheckoutAttributeService>().As<ICheckoutAttributeService>().WithRequestCache().InstancePerRequest();
            builder.RegisterType<GiftCardService>().As<IGiftCardService>().InstancePerRequest();
            builder.RegisterType<OrderService>().As<IOrderService>().InstancePerRequest();
            builder.RegisterType<OrderReportService>().As<IOrderReportService>().InstancePerRequest();
            builder.RegisterType<OrderProcessingService>().As<IOrderProcessingService>().InstancePerRequest();
            builder.RegisterType<OrderTotalCalculationService>().As<IOrderTotalCalculationService>().InstancePerRequest();
            builder.RegisterType<ShoppingCartService>().As<IShoppingCartService>().InstancePerRequest();

            builder.RegisterType<PaymentService>().As<IPaymentService>().InstancePerRequest();

            builder.RegisterType<EncryptionService>().As<IEncryptionService>().InstancePerRequest();
            builder.RegisterType<FormsAuthenticationService>().As<IAuthenticationService>().InstancePerRequest();

			builder.RegisterType<UrlRecordService>().As<IUrlRecordService>().WithStaticCache().InstancePerRequest();

            builder.RegisterType<ShipmentService>().As<IShipmentService>().InstancePerRequest();
			builder.RegisterType<ShippingService>().As<IShippingService>().WithRequestCache().InstancePerRequest();

			builder.RegisterType<TaxCategoryService>().As<ITaxCategoryService>().WithRequestCache().InstancePerRequest();
            builder.RegisterType<TaxService>().As<ITaxService>().InstancePerRequest();

			builder.RegisterType<ForumService>().As<IForumService>().WithRequestCache().InstancePerRequest();

			builder.RegisterType<PollService>().As<IPollService>().WithRequestCache().InstancePerRequest();
            builder.RegisterType<BlogService>().As<IBlogService>().WithRequestCache().InstancePerRequest();
            builder.RegisterType<WidgetService>().As<IWidgetService>().InstancePerRequest();
            builder.RegisterType<TopicService>().As<ITopicService>().InstancePerRequest();
			builder.RegisterType<NewsService>().As<INewsService>().WithRequestCache().InstancePerRequest();

            builder.RegisterType<DateTimeHelper>().As<IDateTimeHelper>().InstancePerRequest();
            builder.RegisterType<SitemapGenerator>().As<ISitemapGenerator>().InstancePerRequest();
            builder.RegisterType<PageAssetsBuilder>().As<IPageAssetsBuilder>().InstancePerRequest();

            builder.RegisterType<ScheduleTaskService>().As<IScheduleTaskService>().InstancePerRequest();

            builder.RegisterType<ExportManager>().As<IExportManager>().InstancePerRequest();
            builder.RegisterType<ImportManager>().As<IImportManager>().InstancePerRequest();
            builder.RegisterType<MobileDeviceHelper>().As<IMobileDeviceHelper>().InstancePerRequest();
            builder.RegisterType<PdfService>().As<IPdfService>().InstancePerRequest();

            builder.RegisterType<ExternalAuthorizer>().As<IExternalAuthorizer>().InstancePerRequest();
            builder.RegisterType<OpenAuthenticationService>().As<IOpenAuthenticationService>().InstancePerRequest();

			builder.RegisterType<FilterService>().As<IFilterService>().InstancePerRequest();          
			builder.RegisterType<CommonServices>().As<ICommonServices>().WithStaticCache().InstancePerRequest();
        }

        public int Order
        {
            get { return -100; }
        }
    }

	#region Modules

	public class DbModule : Module
	{
		private readonly ITypeFinder _typeFinder;

		public DbModule(ITypeFinder typeFinder)
		{
			_typeFinder = typeFinder;
		}
		
		protected override void Load(ContainerBuilder builder)
		{
			builder.Register(c => DataSettings.Current).As<DataSettings>().InstancePerDependency();
			builder.Register(x => new EfDataProviderFactory(x.Resolve<DataSettings>())).As<DataProviderFactory>().InstancePerDependency();

			builder.Register(x => x.Resolve<DataProviderFactory>().LoadDataProvider()).As<IDataProvider>().InstancePerDependency();
			builder.Register(x => (IEfDataProvider)x.Resolve<DataProviderFactory>().LoadDataProvider()).As<IEfDataProvider>().InstancePerDependency();

			if (DataSettings.Current.IsValid())
			{
				// register DB Hooks (only when app was installed properly)

				Func<Type, Type> findHookedType = (t) => 
				{
					var x = t;
					while (x != null)
					{
						if (x.IsGenericType)
						{
							return x.GetGenericArguments()[0];
						}
						x = x.BaseType;
					}

					return typeof(object);
				};

				var hooks = _typeFinder.FindClassesOfType(typeof(IHook));
				foreach (var hook in hooks)
				{
					var hookedType = findHookedType(hook);

					var registration = builder.RegisterType(hook)
						.As(typeof(IPreActionHook).IsAssignableFrom(hook) ? typeof(IPreActionHook) : typeof(IPostActionHook))
						.InstancePerRequest();

					registration.WithMetadata<HookMetadata>(m => 
					{ 
						m.For(em => em.HookedType, hookedType); 
					});
				}

				builder.Register<IDbContext>(c => new SmartObjectContext(DataSettings.Current.DataConnectionString))
					.PropertiesAutowired(PropertyWiringOptions.None)
					.InstancePerRequest();
			}
			else
			{
				builder.Register<IDbContext>(c => new SmartObjectContext(DataSettings.Current.DataConnectionString)).InstancePerRequest();
			}

			builder.Register<Func<string, IDbContext>>(c =>
			{
				var cc = c.Resolve<IComponentContext>();
				return named => cc.ResolveNamed<IDbContext>(named);
			});

			builder.RegisterGeneric(typeof(EfRepository<>)).As(typeof(IRepository<>)).InstancePerRequest();

			builder.Register<DbQuerySettings>(c => {
				var storeService = c.Resolve<IStoreService>();
				var aclService = c.Resolve<IAclService>();

				return new DbQuerySettings(!aclService.HasActiveAcl, storeService.IsSingleStoreMode());
			}).InstancePerRequest();
		}

		protected override void AttachToComponentRegistration(IComponentRegistry componentRegistry, IComponentRegistration registration)
		{
			var querySettingsProperty = FindQuerySettingsProperty(registration.Activator.LimitType);

			if (querySettingsProperty == null)
				return;

			registration.Activated += (sender, e) =>
			{
				var querySettings = e.Context.Resolve<DbQuerySettings>();
				querySettingsProperty.SetValue(e.Instance, querySettings, null);
			};
		}

		private static PropertyInfo FindQuerySettingsProperty(Type type)
		{
			return type.GetProperty("QuerySettings", typeof(DbQuerySettings));
		}
	}

	public class LoggingModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<Notifier>().As<INotifier>().InstancePerRequest();
			builder.RegisterType<DefaultLogger>().As<ILogger>().InstancePerRequest();
			builder.RegisterType<CustomerActivityService>().As<ICustomerActivityService>().WithRequestCache().InstancePerRequest();
		}

		protected override void AttachToComponentRegistration(IComponentRegistry componentRegistry, IComponentRegistration registration)
		{
			if (!DataSettings.DatabaseIsInstalled())
				return;
			
			var implementationType = registration.Activator.LimitType;

			// build an array of actions on this type to assign loggers to member properties
			var injectors = BuildLoggerInjectors(implementationType).ToArray();

			// if there are no logger properties, there's no reason to hook the activated event
			if (!injectors.Any())
				return;

			// otherwise, whan an instance of this component is activated, inject the loggers on the instance
			registration.Activated += (s, e) =>
			{
				foreach (var injector in injectors)
					injector(e.Context, e.Instance);
			};
		}

		private IEnumerable<Action<IComponentContext, object>> BuildLoggerInjectors(Type componentType)
		{
			if (DataSettings.DatabaseIsInstalled())
			{
				// Look for settable properties of type "ILogger" 
				var loggerProperties = componentType
					.GetProperties(BindingFlags.SetProperty | BindingFlags.Public | BindingFlags.Instance)
					.Select(p => new
					{
						PropertyInfo = p,
						p.PropertyType,
						IndexParameters = p.GetIndexParameters(),
						Accessors = p.GetAccessors(false)
					})
					.Where(x => x.PropertyType == typeof(ILogger)) // must be a logger
					.Where(x => x.IndexParameters.Count() == 0) // must not be an indexer
					.Where(x => x.Accessors.Length != 1 || x.Accessors[0].ReturnType == typeof(void)); //must have get/set, or only set

				// Return an array of actions that resolve a logger and assign the property
				foreach (var entry in loggerProperties)
				{
					var propertyInfo = entry.PropertyInfo;

					yield return (ctx, instance) =>
					{
						string component = componentType.ToString();
						var logger = ctx.Resolve<ILogger>();
						propertyInfo.SetValue(instance, logger, null);
					};
				}
			}
		}
	}

	public class LocalizationModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<LanguageService>().As<ILanguageService>().WithRequestCache().InstancePerRequest();
			
			builder.RegisterType<TelerikLocalizationServiceFactory>().As<Telerik.Web.Mvc.Infrastructure.ILocalizationServiceFactory>().InstancePerRequest();
			builder.RegisterType<LocalizationService>().As<ILocalizationService>()
				.WithStaticCache() // pass StaticCache as ICache (cache settings between requests)
				.InstancePerRequest();

			builder.RegisterType<Text>().As<IText>().InstancePerRequest();

			builder.RegisterType<LocalizedEntityService>().As<ILocalizedEntityService>()
				.WithStaticCache() // pass StaticCache as ICache (cache settings between requests)
				.InstancePerRequest();
		}

		protected override void AttachToComponentRegistration(IComponentRegistry componentRegistry, IComponentRegistration registration)
		{
			var userProperty = FindUserProperty(registration.Activator.LimitType);
			
			if (userProperty == null)
				return;

			registration.Activated += (sender, e) =>
			{
				Localizer localizer = e.Context.Resolve<IText>().Get;
				userProperty.SetValue(e.Instance, localizer, null);
			};
		}

		private static PropertyInfo FindUserProperty(Type type)
		{
			return type.GetProperty("T", typeof(Localizer));
		}
	}

	public class CachingModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<StaticCache>().As<ICache>().Keyed<ICache>(typeof(StaticCache)).SingleInstance();
			builder.RegisterType<AspNetCache>().As<ICache>().Keyed<ICache>(typeof(AspNetCache)).InstancePerRequest();
			builder.RegisterType<RequestCache>().As<ICache>().Keyed<ICache>(typeof(RequestCache)).InstancePerRequest();

			builder.RegisterType<CacheManager<StaticCache>>()
				.As<ICacheManager>()
				.Named<ICacheManager>("static")
				.SingleInstance();
			builder.RegisterType<CacheManager<AspNetCache>>()
				.As<ICacheManager>()
				.Named<ICacheManager>("aspnet")
				.InstancePerRequest();
			builder.RegisterType<CacheManager<RequestCache>>()
				.As<ICacheManager>()
				.Named<ICacheManager>("request")
				.InstancePerRequest();

			// Register resolving delegate
			builder.Register<Func<Type, ICache>>(c =>
			{
				var cc = c.Resolve<IComponentContext>();
				return keyed => cc.ResolveKeyed<ICache>(keyed);
			});

			builder.Register<Func<string, ICacheManager>>(c =>
			{
				var cc = c.Resolve<IComponentContext>();
				return named => cc.ResolveNamed<ICacheManager>(named);
			});

			builder.Register<Func<string, Lazy<ICacheManager>>>(c =>
			{
				var cc = c.Resolve<IComponentContext>();
				return named => cc.ResolveNamed<Lazy<ICacheManager>>(named);
			});
		}
	}

	public class EventModule : Module
	{
		private readonly ITypeFinder _typeFinder;

		public EventModule(ITypeFinder typeFinder)
		{
			_typeFinder = typeFinder;
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<EventPublisher>().As<IEventPublisher>().SingleInstance();
			builder.RegisterGeneric(typeof(DefaultConsumerFactory<>)).As(typeof(IConsumerFactory<>)).InstancePerDependency();

			// Register event consumers
			var consumerTypes = _typeFinder.FindClassesOfType(typeof(IConsumer<>));
			foreach (var consumerType in consumerTypes)
			{
				Type[] implementedInterfaces = consumerType.FindInterfaces(IsConsumerInterface, typeof(IConsumer<>));

				var registration = builder.RegisterType(consumerType).As(implementedInterfaces);

				var isActive = PluginManager.IsActivePluginAssembly(consumerType.Assembly);
				var shouldExecuteAsync = consumerType.GetAttribute<AsyncConsumerAttribute>(false) != null;

				registration.WithMetadata<EventConsumerMetadata>(m => {
					m.For(em => em.IsActive, isActive);
					m.For(em => em.ExecuteAsync, shouldExecuteAsync);
				});

				if (!shouldExecuteAsync)
					registration.InstancePerRequest();

			}
		}

		private static bool IsConsumerInterface(Type type, object criteria)
		{
			var isMatch = type.IsGenericType && ((Type)criteria).IsAssignableFrom(type.GetGenericTypeDefinition());
			return isMatch;
		}
	}

	public class MessagingModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<MessageTemplateService>().As<IMessageTemplateService>().WithRequestCache().InstancePerRequest();
			builder.RegisterType<QueuedEmailService>().As<IQueuedEmailService>().InstancePerRequest();
			builder.RegisterType<NewsLetterSubscriptionService>().As<INewsLetterSubscriptionService>().InstancePerRequest();
			builder.RegisterType<CampaignService>().As<ICampaignService>().InstancePerRequest();
			builder.RegisterType<EmailAccountService>().As<IEmailAccountService>().InstancePerRequest();
			builder.RegisterType<WorkflowMessageService>().As<IWorkflowMessageService>().InstancePerRequest();
			builder.RegisterType<MessageTokenProvider>().As<IMessageTokenProvider>().InstancePerRequest();
			builder.RegisterType<Tokenizer>().As<ITokenizer>().InstancePerRequest();
			builder.RegisterType<DefaultEmailSender>().As<IEmailSender>().SingleInstance(); // xxx (http)
		}
	}

	public class WebModule : Module
	{
		private readonly ITypeFinder _typeFinder;

		public WebModule(ITypeFinder typeFinder)
		{
			_typeFinder = typeFinder;
		}

		protected override void Load(ContainerBuilder builder)
		{
			var foundAssemblies = _typeFinder.GetAssemblies().ToArray();

			builder.RegisterModule(new AutofacWebTypesModule());
			builder.Register(HttpContextBaseFactory).As<HttpContextBase>().InstancePerRequest();

			// register all controllers
			builder.RegisterControllers(foundAssemblies);

			builder.RegisterType<EmbeddedViewResolver>().As<IEmbeddedViewResolver>().SingleInstance();
			builder.RegisterType<RoutePublisher>().As<IRoutePublisher>().SingleInstance();
			builder.RegisterType<BundlePublisher>().As<IBundlePublisher>().SingleInstance();
			builder.RegisterType<BundleBuilder>().As<IBundleBuilder>().InstancePerRequest();

			builder.RegisterFilterProvider();
		}

		static HttpContextBase HttpContextBaseFactory(IComponentContext ctx)
		{
			if (IsRequestValid())
			{
				return new HttpContextWrapper(HttpContext.Current);
			}

			// TODO: determine store url

			// register FakeHttpContext when HttpContext is not available
			return new FakeHttpContext("~/");
		}

		static bool IsRequestValid()
		{
			if (HttpContext.Current == null)
				return false;

			try
			{
				// The "Request" property throws at application startup on IIS integrated pipeline mode
				var req = HttpContext.Current.Request;
			}
			catch (Exception)
			{
				return false;
			}

			return true;
		}
	}

	public class WebApiModule : Module
	{
		private readonly ITypeFinder _typeFinder;

		public WebApiModule(ITypeFinder typeFinder)
		{
			_typeFinder = typeFinder;
		}

		protected override void Load(ContainerBuilder builder)
		{
			var foundAssemblies = _typeFinder.GetAssemblies().ToArray();

			// register all api controllers
			builder.RegisterApiControllers(foundAssemblies);
			
			builder.RegisterType<WebApiConfigurationPublisher>().As<IWebApiConfigurationPublisher>().SingleInstance();
		}

		protected override void AttachToComponentRegistration(IComponentRegistry componentRegistry, IComponentRegistration registration)
		{
			var baseType = typeof(WebApiEntityController<,>);
			var type = registration.Activator.LimitType;
			Type implementingType;

			if (!type.IsSubClass(baseType, out implementingType))
				return;

			var repoProperty = FindRepositoryProperty(type, implementingType.GetGenericArguments()[0]);
			var serviceProperty = FindServiceProperty(type, implementingType.GetGenericArguments()[1]);

			if (repoProperty != null || serviceProperty != null)
			{
				registration.Activated += (sender, e) =>
				{
					if (repoProperty != null)
					{
						var repo = e.Context.Resolve(repoProperty.PropertyType);
						repoProperty.SetValue(e.Instance, repo, null);
					}

					if (serviceProperty != null)
					{
						var service = e.Context.Resolve(serviceProperty.PropertyType);
						serviceProperty.SetValue(e.Instance, service, null);
					}
				};
			}
		}

		private static PropertyInfo FindRepositoryProperty(Type type, Type entityType)
		{
			var pi = type.GetProperty("Repository", typeof(IRepository<>).MakeGenericType(entityType));
			return pi;
		}

		private static PropertyInfo FindServiceProperty(Type type, Type serviceType)
		{
			var pi = type.GetProperty("Service", serviceType);
			return pi;
		}

	}

	public class UiModule : Module
	{
		private readonly ITypeFinder _typeFinder;

		public UiModule(ITypeFinder typeFinder)
		{
			_typeFinder = typeFinder;
		}

		protected override void Load(ContainerBuilder builder)
		{
			// register theming services
			builder.RegisterType<DefaultThemeRegistry>().As<IThemeRegistry>().SingleInstance();
			builder.RegisterType<ThemeContext>().As<IThemeContext>().InstancePerRequest();
			builder.RegisterType<ThemeVariablesService>().As<IThemeVariablesService>().InstancePerRequest();

			// register UI component renderers
			builder.RegisterType<TabStripRenderer>().As<ComponentRenderer<TabStrip>>();
			builder.RegisterType<PagerRenderer>().As<ComponentRenderer<Pager>>();
			builder.RegisterType<WindowRenderer>().As<ComponentRenderer<Window>>();

			builder.RegisterType<WidgetProvider>().As<IWidgetProvider>().InstancePerRequest();

			// Register simple (code) widgets
			var widgetTypes = _typeFinder.FindClassesOfType(typeof(IWidget)).Where(x => !typeof(IWidgetPlugin).IsAssignableFrom(x));
			foreach (var widgetType in widgetTypes)
			{
				if (PluginManager.IsActivePluginAssembly(widgetType.Assembly))
				{
					builder.RegisterType(widgetType).As<IWidget>().Named<IWidget>(widgetType.FullName).InstancePerRequest();
				}
			}
		}
	}

	public class IOModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<FileSystemStorageProvider>().As<IStorageProvider>().InstancePerRequest();
			builder.RegisterType<DefaultVirtualPathProvider>().As<IVirtualPathProvider>().InstancePerRequest();
			builder.RegisterType<WebSiteFolder>().As<IWebSiteFolder>().InstancePerRequest();
		}
	}

	public class PackagingModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<PackageBuilder>().As<IPackageBuilder>().InstancePerRequest();
			builder.RegisterType<PackageInstaller>().As<IPackageInstaller>().InstancePerRequest();
			builder.RegisterType<PackageManager>().As<IPackageManager>().InstancePerRequest();
			builder.RegisterType<FolderUpdater>().As<IFolderUpdater>().InstancePerRequest();
		}
	}

	//public class PluginsModule : Module
	//{
	//	private readonly ITypeFinder _typeFinder;

	//	public PluginsModule(ITypeFinder typeFinder)
	//	{
	//		_typeFinder = typeFinder;
	//	}

	//	protected override void Load(ContainerBuilder builder)
	//	{
	//		// Register payment methods
	//		var types = _typeFinder.FindClassesOfType(typeof(IPaymentMethod));
	//		foreach (var type in types)
	//		{
	//			if (PluginManager.IsActivePluginAssembly(type.Assembly))
	//			{
	//				builder.RegisterType(type).As<IPaymentMethod>().Named<IPaymentMethod>(type.FullName).InstancePerRequest();
	//			}
	//		}
	//	}
	//}

	#endregion

	#region Sources

	public class SettingsSource : IRegistrationSource
    {
        static readonly MethodInfo BuildMethod = typeof(SettingsSource).GetMethod(
            "BuildRegistration",
            BindingFlags.Static | BindingFlags.NonPublic);

        public IEnumerable<IComponentRegistration> RegistrationsFor(
                Service service,
                Func<Service, IEnumerable<IComponentRegistration>> registrations)
        {
            var ts = service as TypedService;
            if (ts != null && typeof(ISettings).IsAssignableFrom(ts.ServiceType))
            {
                //var buildMethod = BuildMethod.MakeGenericMethod(ts.ServiceType);
                //yield return (IComponentRegistration)buildMethod.Invoke(null, null);

				// Perf with Fasterflect
				yield return (IComponentRegistration)Fasterflect.TryInvokeWithValuesExtensions.TryCallMethodWithValues(
					typeof(SettingsSource),
					null, 
					"BuildRegistration", 
					new Type[] { ts.ServiceType }, 
					BindingFlags.Static | BindingFlags.NonPublic);
            }
        }

        static IComponentRegistration BuildRegistration<TSettings>() where TSettings : ISettings, new()
        {
            return RegistrationBuilder
				.ForDelegate((c, p) =>
				{
					int currentStoreId = 0;
					IStoreContext storeContext;
					if (c.TryResolve<IStoreContext>(out storeContext))
					{
						var store = storeContext.CurrentStore;

						currentStoreId = store.Id;
						//uncomment the code below if you want load settings per store only when you have two stores installed.
						//var currentStoreId = c.Resolve<IStoreService>().GetAllStores().Count > 1
						//    c.Resolve<IStoreContext>().CurrentStore.Id : 0;

						//although it's better to connect to your database and execute the following SQL:
						//DELETE FROM [Setting] WHERE [StoreId] > 0

						return c.Resolve<ISettingService>().LoadSetting<TSettings>(currentStoreId);
					}

					// Unit tests
					return new TSettings();
				})
                .InstancePerRequest()
                .CreateRegistration();
        }

        public bool IsAdapterForIndividualComponents { get { return false; } }
    }

	#endregion

}
