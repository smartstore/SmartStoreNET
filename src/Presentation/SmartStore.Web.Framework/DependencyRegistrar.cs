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
            builder.RegisterType<WebHelper>().As<IWebHelper>().InstancePerHttpRequest();
            
            // plugins
            builder.RegisterType<PluginFinder>().As<IPluginFinder>().SingleInstance(); // xxx (http)

            // work context
            builder.RegisterType<WebWorkContext>().As<IWorkContext>().WithStaticCache().InstancePerHttpRequest();
			
			// store context
			builder.RegisterType<WebStoreContext>().As<IStoreContext>().InstancePerHttpRequest();

            // services
            builder.RegisterType<BackInStockSubscriptionService>().As<IBackInStockSubscriptionService>().InstancePerHttpRequest();
            builder.RegisterType<CategoryService>().As<ICategoryService>().WithRequestCache().InstancePerHttpRequest();
            builder.RegisterType<CompareProductsService>().As<ICompareProductsService>().InstancePerHttpRequest();
            builder.RegisterType<RecentlyViewedProductsService>().As<IRecentlyViewedProductsService>().InstancePerHttpRequest();
			builder.RegisterType<ManufacturerService>().As<IManufacturerService>().WithRequestCache().InstancePerHttpRequest();
            builder.RegisterType<PriceCalculationService>().As<IPriceCalculationService>().InstancePerHttpRequest();
            builder.RegisterType<PriceFormatter>().As<IPriceFormatter>().InstancePerHttpRequest();
            builder.RegisterType<ProductAttributeFormatter>().As<IProductAttributeFormatter>().InstancePerLifetimeScope();
            builder.RegisterType<ProductAttributeParser>().As<IProductAttributeParser>().InstancePerHttpRequest();
			builder.RegisterType<ProductAttributeService>().As<IProductAttributeService>().WithRequestCache().InstancePerHttpRequest();
			builder.RegisterType<ProductService>().As<IProductService>().WithRequestCache().InstancePerHttpRequest();
            builder.RegisterType<CopyProductService>().As<ICopyProductService>().InstancePerHttpRequest();
			builder.RegisterType<SpecificationAttributeService>().As<ISpecificationAttributeService>().WithRequestCache().InstancePerHttpRequest();
            builder.RegisterType<ProductTemplateService>().As<IProductTemplateService>().InstancePerHttpRequest();
			builder.RegisterType<CategoryTemplateService>().As<ICategoryTemplateService>().WithRequestCache().InstancePerHttpRequest();
			builder.RegisterType<ManufacturerTemplateService>().As<IManufacturerTemplateService>().WithRequestCache().InstancePerHttpRequest();
			builder.RegisterType<ProductTagService>().As<IProductTagService>().WithStaticCache().InstancePerHttpRequest();

            builder.RegisterType<AffiliateService>().As<IAffiliateService>().InstancePerHttpRequest();
            builder.RegisterType<AddressService>().As<IAddressService>().InstancePerHttpRequest();
			builder.RegisterType<GenericAttributeService>().As<IGenericAttributeService>().WithRequestCache().InstancePerHttpRequest();
            builder.RegisterType<FulltextService>().As<IFulltextService>().InstancePerHttpRequest();
            builder.RegisterType<MaintenanceService>().As<IMaintenanceService>().InstancePerHttpRequest();

			builder.RegisterType<CustomerContentService>().As<ICustomerContentService>().WithRequestCache().InstancePerHttpRequest();
			builder.RegisterType<CustomerService>().As<ICustomerService>().WithRequestCache().InstancePerHttpRequest();
            builder.RegisterType<CustomerRegistrationService>().As<ICustomerRegistrationService>().InstancePerHttpRequest();
            builder.RegisterType<CustomerReportService>().As<ICustomerReportService>().InstancePerHttpRequest();

            builder.RegisterType<PermissionService>().As<IPermissionService>().WithStaticCache() .InstancePerHttpRequest();

            builder.RegisterType<AclService>().As<IAclService>().WithStaticCache().InstancePerHttpRequest();

            builder.RegisterType<GeoCountryLookup>().As<IGeoCountryLookup>().InstancePerHttpRequest();
			builder.RegisterType<CountryService>().As<ICountryService>().WithRequestCache().InstancePerHttpRequest();
			builder.RegisterType<CurrencyService>().As<ICurrencyService>().WithRequestCache().InstancePerHttpRequest();

			builder.RegisterType<DeliveryTimeService>().As<IDeliveryTimeService>().WithRequestCache().InstancePerHttpRequest();

			builder.RegisterType<MeasureService>().As<IMeasureService>().WithRequestCache().InstancePerHttpRequest();
			builder.RegisterType<StateProvinceService>().As<IStateProvinceService>().WithRequestCache().InstancePerHttpRequest();

			builder.RegisterType<StoreService>().As<IStoreService>().WithRequestCache().InstancePerHttpRequest();
			builder.RegisterType<StoreMappingService>().As<IStoreMappingService>().WithStaticCache().InstancePerHttpRequest();

			builder.RegisterType<DiscountService>().As<IDiscountService>().WithRequestCache().InstancePerHttpRequest();

            builder.RegisterType<SettingService>().As<ISettingService>().WithStaticCache().InstancePerHttpRequest();

            builder.RegisterType<DownloadService>().As<IDownloadService>().InstancePerHttpRequest();
            builder.RegisterType<ImageCache>().As<IImageCache>().InstancePerHttpRequest();
            builder.RegisterType<ImageResizerService>().As<IImageResizerService>().SingleInstance();
            builder.RegisterType<PictureService>().As<IPictureService>().InstancePerHttpRequest();

            builder.RegisterType<CheckoutAttributeFormatter>().As<ICheckoutAttributeFormatter>().InstancePerHttpRequest();
            builder.RegisterType<CheckoutAttributeParser>().As<ICheckoutAttributeParser>().InstancePerHttpRequest();
			builder.RegisterType<CheckoutAttributeService>().As<ICheckoutAttributeService>().WithRequestCache().InstancePerHttpRequest();
            builder.RegisterType<GiftCardService>().As<IGiftCardService>().InstancePerHttpRequest();
            builder.RegisterType<OrderService>().As<IOrderService>().InstancePerHttpRequest();
            builder.RegisterType<OrderReportService>().As<IOrderReportService>().InstancePerHttpRequest();
            builder.RegisterType<OrderProcessingService>().As<IOrderProcessingService>().InstancePerHttpRequest();
            builder.RegisterType<OrderTotalCalculationService>().As<IOrderTotalCalculationService>().InstancePerHttpRequest();
            builder.RegisterType<ShoppingCartService>().As<IShoppingCartService>().InstancePerHttpRequest();

            builder.RegisterType<PaymentService>().As<IPaymentService>().InstancePerHttpRequest();

            builder.RegisterType<EncryptionService>().As<IEncryptionService>().InstancePerHttpRequest();
            builder.RegisterType<FormsAuthenticationService>().As<IAuthenticationService>().InstancePerHttpRequest();

			builder.RegisterType<UrlRecordService>().As<IUrlRecordService>().WithStaticCache().InstancePerHttpRequest();

            builder.RegisterType<ShipmentService>().As<IShipmentService>().InstancePerHttpRequest();
			builder.RegisterType<ShippingService>().As<IShippingService>().WithRequestCache().InstancePerHttpRequest();

			builder.RegisterType<TaxCategoryService>().As<ITaxCategoryService>().WithRequestCache().InstancePerHttpRequest();
            builder.RegisterType<TaxService>().As<ITaxService>().InstancePerHttpRequest();

			builder.RegisterType<ForumService>().As<IForumService>().WithRequestCache().InstancePerHttpRequest();

			builder.RegisterType<PollService>().As<IPollService>().WithRequestCache().InstancePerHttpRequest();
            builder.RegisterType<BlogService>().As<IBlogService>().WithRequestCache().InstancePerHttpRequest();
            builder.RegisterType<WidgetService>().As<IWidgetService>().InstancePerHttpRequest();
            builder.RegisterType<TopicService>().As<ITopicService>().InstancePerHttpRequest();
			builder.RegisterType<NewsService>().As<INewsService>().WithRequestCache().InstancePerHttpRequest();

            builder.RegisterType<DateTimeHelper>().As<IDateTimeHelper>().InstancePerHttpRequest();
            builder.RegisterType<SitemapGenerator>().As<ISitemapGenerator>().InstancePerHttpRequest();
            builder.RegisterType<PageAssetsBuilder>().As<IPageAssetsBuilder>().InstancePerHttpRequest();

            builder.RegisterType<ScheduleTaskService>().As<IScheduleTaskService>().InstancePerHttpRequest();

            builder.RegisterType<ExportManager>().As<IExportManager>().InstancePerHttpRequest();
            builder.RegisterType<ImportManager>().As<IImportManager>().InstancePerHttpRequest();
            builder.RegisterType<MobileDeviceHelper>().As<IMobileDeviceHelper>().InstancePerHttpRequest();
            builder.RegisterType<PdfService>().As<IPdfService>().InstancePerHttpRequest();

            builder.RegisterType<ExternalAuthorizer>().As<IExternalAuthorizer>().InstancePerHttpRequest();
            builder.RegisterType<OpenAuthenticationService>().As<IOpenAuthenticationService>().InstancePerHttpRequest();

			builder.RegisterType<FilterService>().As<IFilterService>().InstancePerHttpRequest();          
			builder.RegisterType<CommonServices>().As<ICommonServices>().WithStaticCache().InstancePerHttpRequest();

            //// codehint: sm-add (enable mvc action filter property injection) >>> CRASHES! :-(
            //builder.RegisterFilterProvider();

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
						.InstancePerHttpRequest();

					registration.WithMetadata<HookMetadata>(m => 
					{ 
						m.For(em => em.HookedType, hookedType); 
					});
				}

				builder.Register<IDbContext>(c => new SmartObjectContext(DataSettings.Current.DataConnectionString))
					.PropertiesAutowired(PropertyWiringOptions.None)
					.InstancePerHttpRequest();
			}
			else
			{
				builder.Register<IDbContext>(c => new SmartObjectContext(DataSettings.Current.DataConnectionString)).InstancePerHttpRequest();
			}

			builder.RegisterGeneric(typeof(EfRepository<>)).As(typeof(IRepository<>)).InstancePerHttpRequest();

			builder.Register<DbQuerySettings>(c => {
				var storeService = c.Resolve<IStoreService>();
				var aclService = c.Resolve<IAclService>();

				return new DbQuerySettings(!aclService.HasActiveAcl, storeService.IsSingleStoreMode());
			}).InstancePerHttpRequest();
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
			builder.RegisterType<Notifier>().As<INotifier>().InstancePerHttpRequest();
			builder.RegisterType<DefaultLogger>().As<ILogger>().InstancePerHttpRequest();
			builder.RegisterType<CustomerActivityService>().As<ICustomerActivityService>().WithRequestCache().InstancePerHttpRequest();
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
			builder.RegisterType<LanguageService>().As<ILanguageService>().WithRequestCache().InstancePerHttpRequest();
			
			builder.RegisterType<TelerikLocalizationServiceFactory>().As<Telerik.Web.Mvc.Infrastructure.ILocalizationServiceFactory>().InstancePerHttpRequest();
			builder.RegisterType<LocalizationService>().As<ILocalizationService>()
				.WithStaticCache() // pass StaticCache as ICache (cache settings between requests)
				.InstancePerHttpRequest();

			builder.RegisterType<Text>().As<IText>().InstancePerHttpRequest();

			builder.RegisterType<LocalizedEntityService>().As<ILocalizedEntityService>()
				.WithStaticCache() // pass StaticCache as ICache (cache settings between requests)
				.InstancePerHttpRequest();
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
			builder.RegisterType<AspNetCache>().As<ICache>().Keyed<ICache>(typeof(AspNetCache)).InstancePerHttpRequest();
			builder.RegisterType<RequestCache>().As<ICache>().Keyed<ICache>(typeof(RequestCache)).InstancePerHttpRequest();

			builder.RegisterType<CacheManager<StaticCache>>()
				.As<ICacheManager>()
				.Named<ICacheManager>("static")
				.SingleInstance();
			builder.RegisterType<CacheManager<AspNetCache>>()
				.As<ICacheManager>()
				.Named<ICacheManager>("aspnet")
				.InstancePerHttpRequest();
			builder.RegisterType<CacheManager<RequestCache>>()
				.As<ICacheManager>()
				.Named<ICacheManager>("request")
				.InstancePerHttpRequest();

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
					registration.InstancePerHttpRequest();

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
			builder.RegisterType<MessageTemplateService>().As<IMessageTemplateService>().WithRequestCache().InstancePerHttpRequest();
			builder.RegisterType<QueuedEmailService>().As<IQueuedEmailService>().InstancePerHttpRequest();
			builder.RegisterType<NewsLetterSubscriptionService>().As<INewsLetterSubscriptionService>().InstancePerHttpRequest();
			builder.RegisterType<CampaignService>().As<ICampaignService>().InstancePerHttpRequest();
			builder.RegisterType<EmailAccountService>().As<IEmailAccountService>().InstancePerHttpRequest();
			builder.RegisterType<WorkflowMessageService>().As<IWorkflowMessageService>().InstancePerHttpRequest();
			builder.RegisterType<MessageTokenProvider>().As<IMessageTokenProvider>().InstancePerHttpRequest();
			builder.RegisterType<Tokenizer>().As<ITokenizer>().InstancePerHttpRequest();
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
			builder.Register(c =>
				//register FakeHttpContext when HttpContext is not available
				HttpContext.Current != null ?
				(new HttpContextWrapper(HttpContext.Current) as HttpContextBase) :
				(new FakeHttpContext("~/") as HttpContextBase))
				.As<HttpContextBase>()
				.InstancePerHttpRequest();

			// register all controllers
			builder.RegisterControllers(foundAssemblies);

			builder.RegisterType<EmbeddedViewResolver>().As<IEmbeddedViewResolver>().SingleInstance();
			builder.RegisterType<RoutePublisher>().As<IRoutePublisher>().SingleInstance();
			builder.RegisterType<BundlePublisher>().As<IBundlePublisher>().SingleInstance();
			builder.RegisterType<BundleBuilder>().As<IBundleBuilder>().InstancePerHttpRequest();
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
			builder.RegisterType<ThemeContext>().As<IThemeContext>().InstancePerHttpRequest();
			builder.RegisterType<ThemeVariablesService>().As<IThemeVariablesService>().InstancePerHttpRequest();

			// register UI component renderers
			builder.RegisterType<TabStripRenderer>().As<ComponentRenderer<TabStrip>>();
			builder.RegisterType<PagerRenderer>().As<ComponentRenderer<Pager>>();
			builder.RegisterType<WindowRenderer>().As<ComponentRenderer<Window>>();

			// Register simple (code) widgets
			var widgetTypes = _typeFinder.FindClassesOfType(typeof(IWidget)).Where(x => !typeof(IWidgetPlugin).IsAssignableFrom(x));
			foreach (var widgetType in widgetTypes)
			{
				builder.RegisterType(widgetType).As<IWidget>().Named<IWidget>(widgetType.FullName).InstancePerHttpRequest();
			}
		}
	}

	public class IOModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<FileSystemStorageProvider>().As<IStorageProvider>().InstancePerHttpRequest();
			builder.RegisterType<DefaultVirtualPathProvider>().As<IVirtualPathProvider>().InstancePerHttpRequest();
			builder.RegisterType<WebSiteFolder>().As<IWebSiteFolder>().InstancePerHttpRequest();
		}
	}

	public class PackagingModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<PackageBuilder>().As<IPackageBuilder>().InstancePerHttpRequest();
			builder.RegisterType<PackageInstaller>().As<IPackageInstaller>().InstancePerHttpRequest();
			builder.RegisterType<PackageManager>().As<IPackageManager>().InstancePerHttpRequest();
			builder.RegisterType<FolderUpdater>().As<IFolderUpdater>().InstancePerHttpRequest();
		}
	}

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
                .InstancePerHttpRequest()
                .CreateRegistration();
        }

        public bool IsAdapterForIndividualComponents { get { return false; } }
    }

	#endregion

}
