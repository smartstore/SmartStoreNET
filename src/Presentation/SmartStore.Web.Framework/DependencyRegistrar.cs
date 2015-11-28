using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using Autofac;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Integration.Mvc;
using Autofac.Integration.WebApi;
using SmartStore.Core;
using SmartStore.Core.Caching;
using SmartStore.Core.Configuration;
using SmartStore.Core.Data;
using SmartStore.Core.Data.Hooks;
using SmartStore.Core.Email;
using SmartStore.Core.Events;
using SmartStore.Core.Fakes;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Infrastructure.DependencyManagement;
using SmartStore.Core.IO.Media;
using SmartStore.Core.IO.VirtualPath;
using SmartStore.Core.IO.WebSite;
using SmartStore.Core.Localization;
using SmartStore.Core.Logging;
using SmartStore.Core.Packaging;
using SmartStore.Core.Plugins;
using SmartStore.Core.Themes;
using SmartStore.Data;
using SmartStore.Services;
using SmartStore.Services.Affiliates;
using SmartStore.Services.Authentication;
using SmartStore.Services.Authentication.External;
using SmartStore.Services.Blogs;
using SmartStore.Services.Catalog;
using SmartStore.Services.Cms;
using SmartStore.Services.Common;
using SmartStore.Services.Configuration;
using SmartStore.Services.Customers;
using SmartStore.Services.DataExchange;
using SmartStore.Services.Directory;
using SmartStore.Services.Discounts;
using SmartStore.Services.Events;
using SmartStore.Services.ExportImport;
using SmartStore.Services.Filter;
using SmartStore.Services.Forums;
using SmartStore.Services.Helpers;
using SmartStore.Services.Localization;
using SmartStore.Services.Logging;
using SmartStore.Services.Media;
using SmartStore.Services.Messages;
using SmartStore.Services.News;
using SmartStore.Services.Orders;
using SmartStore.Services.Payments;
using SmartStore.Services.Pdf;
using SmartStore.Services.Polls;
using SmartStore.Services.Security;
using SmartStore.Services.Seo;
using SmartStore.Services.Shipping;
using SmartStore.Services.Stores;
using SmartStore.Services.Tasks;
using SmartStore.Services.Tax;
using SmartStore.Services.Themes;
using SmartStore.Services.Topics;
using SmartStore.Utilities;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Localization;
using SmartStore.Web.Framework.Mvc.Bundles;
using SmartStore.Web.Framework.Mvc.Routes;
using SmartStore.Web.Framework.Plugins;
using SmartStore.Web.Framework.Themes;
using SmartStore.Web.Framework.UI;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.Configuration;
using Module = Autofac.Module;

namespace SmartStore.Web.Framework
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
		public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, bool isActiveModule)
        {
			// plugins
			var pluginFinder = new PluginFinder();
			builder.RegisterInstance(pluginFinder).As<IPluginFinder>().SingleInstance();
			builder.RegisterType<PluginMediator>();
			
			// modules
			builder.RegisterModule(new DbModule(typeFinder));
			builder.RegisterModule(new CachingModule());
			builder.RegisterModule(new LocalizationModule());
			builder.RegisterModule(new LoggingModule());
			builder.RegisterModule(new EventModule(typeFinder, pluginFinder));
			builder.RegisterModule(new MessagingModule());
			builder.RegisterModule(new WebModule(typeFinder));
			builder.RegisterModule(new WebApiModule(typeFinder));
			builder.RegisterModule(new UiModule(typeFinder));
			builder.RegisterModule(new IOModule());
			builder.RegisterModule(new PackagingModule());
			builder.RegisterModule(new ProvidersModule(typeFinder, pluginFinder));
            builder.RegisterModule(new TasksModule(typeFinder));

			// sources
			builder.RegisterSource(new SettingsSource());

            // web helper
            builder.RegisterType<WebHelper>().As<IWebHelper>().InstancePerRequest(); 

            // work context
            builder.RegisterType<WebWorkContext>().As<IWorkContext>().WithStaticCache().InstancePerRequest();
			
			// store context
			builder.RegisterType<WebStoreContext>().As<IStoreContext>().InstancePerRequest();

            // services
			builder.RegisterType<CategoryService>().As<ICategoryService>().InstancePerRequest();
			builder.RegisterType<CategoryService>().Named<ICategoryService>("nocache")
				.WithNullCache()
				.InstancePerRequest();

			builder.RegisterType<ManufacturerService>().As<IManufacturerService>()
				.WithNullCache()
				.InstancePerRequest();
			builder.RegisterType<ManufacturerService>().Named<IManufacturerService>("nocache")
				.WithNullCache()
				.InstancePerRequest();

			builder.RegisterType<ProductService>().As<IProductService>().InstancePerRequest();
			builder.RegisterType<ProductService>().Named<IProductService>("nocache").InstancePerRequest();

            builder.RegisterType<BackInStockSubscriptionService>().As<IBackInStockSubscriptionService>().InstancePerRequest();
            builder.RegisterType<CompareProductsService>().As<ICompareProductsService>().InstancePerRequest();
            builder.RegisterType<RecentlyViewedProductsService>().As<IRecentlyViewedProductsService>().InstancePerRequest();
            builder.RegisterType<PriceCalculationService>().As<IPriceCalculationService>().InstancePerRequest();
            builder.RegisterType<PriceFormatter>().As<IPriceFormatter>().InstancePerRequest();
            builder.RegisterType<ProductAttributeFormatter>().As<IProductAttributeFormatter>().InstancePerLifetimeScope();
            builder.RegisterType<ProductAttributeParser>().As<IProductAttributeParser>().InstancePerRequest();
			builder.RegisterType<ProductAttributeService>().As<IProductAttributeService>().InstancePerRequest();
            builder.RegisterType<CopyProductService>().As<ICopyProductService>().InstancePerRequest();
			builder.RegisterType<SpecificationAttributeService>().As<ISpecificationAttributeService>().InstancePerRequest();
            builder.RegisterType<ProductTemplateService>().As<IProductTemplateService>().InstancePerRequest();
			builder.RegisterType<CategoryTemplateService>().As<ICategoryTemplateService>().InstancePerRequest();
			builder.RegisterType<ManufacturerTemplateService>().As<IManufacturerTemplateService>().InstancePerRequest();
			builder.RegisterType<ProductTagService>().As<IProductTagService>().WithStaticCache().InstancePerRequest();

            builder.RegisterType<AffiliateService>().As<IAffiliateService>().InstancePerRequest();
            builder.RegisterType<AddressService>().As<IAddressService>().InstancePerRequest();
			builder.RegisterType<GenericAttributeService>().As<IGenericAttributeService>().InstancePerRequest();
            builder.RegisterType<FulltextService>().As<IFulltextService>().InstancePerRequest();
            builder.RegisterType<MaintenanceService>().As<IMaintenanceService>().InstancePerRequest();

			builder.RegisterType<CustomerContentService>().As<ICustomerContentService>().InstancePerRequest();
			builder.RegisterType<CustomerService>().As<ICustomerService>().InstancePerRequest();
            builder.RegisterType<CustomerRegistrationService>().As<ICustomerRegistrationService>().InstancePerRequest();
            builder.RegisterType<CustomerReportService>().As<ICustomerReportService>().InstancePerRequest();

            builder.RegisterType<PermissionService>().As<IPermissionService>().WithStaticCache() .InstancePerRequest();

            builder.RegisterType<AclService>().As<IAclService>().WithStaticCache().InstancePerRequest();

            builder.RegisterType<GeoCountryLookup>().As<IGeoCountryLookup>().InstancePerRequest();
			builder.RegisterType<CountryService>().As<ICountryService>().InstancePerRequest();
			builder.RegisterType<CurrencyService>().As<ICurrencyService>().InstancePerRequest();

			builder.RegisterType<DeliveryTimeService>().As<IDeliveryTimeService>().InstancePerRequest();
            builder.RegisterType<QuantityUnitService>().As<IQuantityUnitService>().InstancePerRequest();
			builder.RegisterType<MeasureService>().As<IMeasureService>().InstancePerRequest();
			builder.RegisterType<StateProvinceService>().As<IStateProvinceService>().InstancePerRequest();

			builder.RegisterType<StoreService>().As<IStoreService>().InstancePerRequest();
			builder.RegisterType<StoreMappingService>().As<IStoreMappingService>().WithStaticCache().InstancePerRequest();

			builder.RegisterType<DiscountService>().As<IDiscountService>().InstancePerRequest();

            builder.RegisterType<SettingService>().As<ISettingService>().WithStaticCache().InstancePerRequest();

            builder.RegisterType<DownloadService>().As<IDownloadService>().InstancePerRequest();
            builder.RegisterType<ImageCache>().As<IImageCache>().InstancePerRequest();
            builder.RegisterType<ImageResizerService>().As<IImageResizerService>().SingleInstance();
            builder.RegisterType<PictureService>().As<IPictureService>().InstancePerRequest();

            builder.RegisterType<CheckoutAttributeFormatter>().As<ICheckoutAttributeFormatter>().InstancePerRequest();
            builder.RegisterType<CheckoutAttributeParser>().As<ICheckoutAttributeParser>().InstancePerRequest();
			builder.RegisterType<CheckoutAttributeService>().As<ICheckoutAttributeService>().InstancePerRequest();
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
			builder.RegisterType<ShippingService>().As<IShippingService>().InstancePerRequest();

			builder.RegisterType<TaxCategoryService>().As<ITaxCategoryService>().InstancePerRequest();
            builder.RegisterType<TaxService>().As<ITaxService>().InstancePerRequest();

			builder.RegisterType<ForumService>().As<IForumService>().InstancePerRequest();

			builder.RegisterType<PollService>().As<IPollService>().InstancePerRequest();
            builder.RegisterType<BlogService>().As<IBlogService>().InstancePerRequest();
            builder.RegisterType<WidgetService>().As<IWidgetService>().InstancePerRequest();
            builder.RegisterType<TopicService>().As<ITopicService>().InstancePerRequest();
			builder.RegisterType<NewsService>().As<INewsService>().InstancePerRequest();

            builder.RegisterType<DateTimeHelper>().As<IDateTimeHelper>().InstancePerRequest();
            builder.RegisterType<SitemapGenerator>().As<ISitemapGenerator>().InstancePerRequest();
            builder.RegisterType<PageAssetsBuilder>().As<IPageAssetsBuilder>().InstancePerRequest();

            builder.RegisterType<ScheduleTaskService>().As<IScheduleTaskService>().InstancePerRequest();

			builder.RegisterType<ExportManager>().As<IExportManager>()
				.WithParameter(ResolvedParameter.ForNamed<IProductService>("nocache"))
				.WithParameter(ResolvedParameter.ForNamed<ICategoryService>("nocache"))
				.WithParameter(ResolvedParameter.ForNamed<IManufacturerService>("nocache"))
				.InstancePerRequest();

            builder.RegisterType<ImportManager>().As<IImportManager>().InstancePerRequest();
			builder.RegisterType<SyncMappingService>().As<ISyncMappingService>().InstancePerRequest();

            builder.RegisterType<MobileDeviceHelper>().As<IMobileDeviceHelper>().InstancePerRequest();
			builder.RegisterType<UAParserUserAgent>().As<IUserAgent>().InstancePerRequest();
			builder.RegisterType<WkHtmlToPdfConverter>().As<IPdfConverter>().InstancePerRequest();

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

				var hooks = _typeFinder.FindClassesOfType<IHook>(ignoreInactivePlugins: true);
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

			builder.Register<DbQuerySettings>(c => 
			{
				var storeService = c.Resolve<IStoreService>();
				var aclService = c.Resolve<IAclService>();

				return new DbQuerySettings(!aclService.HasActiveAcl, storeService.IsSingleStoreMode());
			})
			.InstancePerRequest();
		}

		protected override void AttachToComponentRegistration(IComponentRegistry componentRegistry, IComponentRegistration registration)
		{
			var querySettingsProperty = FindQuerySettingsProperty(registration.Activator.LimitType);

			if (querySettingsProperty == null)
				return;

			registration.Activated += (sender, e) =>
			{
				if (DataSettings.DatabaseIsInstalled())
				{
					var querySettings = e.Context.Resolve<DbQuerySettings>();
					querySettingsProperty.SetValue(e.Instance, querySettings, null);
				}
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
			builder.RegisterType<CustomerActivityService>().As<ICustomerActivityService>().InstancePerRequest();
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
			builder.RegisterType<LanguageService>().As<ILanguageService>().InstancePerRequest();
			
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
				if (DataSettings.DatabaseIsInstalled())
				{
					Localizer localizer = e.Context.Resolve<IText>().Get;
					userProperty.SetValue(e.Instance, localizer, null);
				}
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
			builder.RegisterType<StaticCache>().Keyed<ICache>(typeof(StaticCache)).SingleInstance();
			builder.RegisterType<AspNetCache>().Keyed<ICache>(typeof(AspNetCache)).SingleInstance();
			builder.RegisterType<RequestCache>().Keyed<ICache>(typeof(RequestCache)).InstancePerRequest();

			builder.RegisterType<CacheManager<RequestCache>>()
				.As<ICacheManager>()
				.InstancePerRequest();
			builder.RegisterType<CacheManager<StaticCache>>()
				.Named<ICacheManager>("static")
				.SingleInstance();
			builder.RegisterType<CacheManager<AspNetCache>>()
				.Named<ICacheManager>("aspnet")
				.SingleInstance();
			builder.RegisterType<NullCache>()
				.Named<ICacheManager>("null")
				.SingleInstance();

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
		private readonly IPluginFinder _pluginFinder;

		public EventModule(ITypeFinder typeFinder, IPluginFinder pluginFinder)
		{
			_typeFinder = typeFinder;
			_pluginFinder = pluginFinder;
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

				var pluginDescriptor = _pluginFinder.GetPluginDescriptorByAssembly(consumerType.Assembly);
				var isActive = PluginManager.IsActivePluginAssembly(consumerType.Assembly);
				var shouldExecuteAsync = consumerType.GetAttribute<AsyncConsumerAttribute>(false) != null;

				registration.WithMetadata<EventConsumerMetadata>(m => {
					m.For(em => em.IsActive, isActive);
					m.For(em => em.ExecuteAsync, shouldExecuteAsync);
					m.For(em => em.PluginDescriptor, pluginDescriptor);
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
			builder.RegisterType<MessageTemplateService>().As<IMessageTemplateService>().InstancePerRequest();
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
			var foundAssemblies = _typeFinder.GetAssemblies(ignoreInactivePlugins: true).ToArray();

			builder.RegisterModule(new AutofacWebTypesModule());
			builder.Register(HttpContextBaseFactory).As<HttpContextBase>();

			// register all controllers
			builder.RegisterControllers(foundAssemblies);

			builder.RegisterType<RoutePublisher>().As<IRoutePublisher>().SingleInstance();
			builder.RegisterType<BundlePublisher>().As<IBundlePublisher>().SingleInstance();
			builder.RegisterType<BundleBuilder>().As<IBundleBuilder>().InstancePerRequest();
			builder.RegisterType<FileDownloadManager>().InstancePerRequest();

			builder.RegisterFilterProvider();

			// global exception handling
			if (DataSettings.DatabaseIsInstalled())
			{
				builder.RegisterType<HandleExceptionFilter>().AsActionFilterFor<Controller>();
			}
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
			var foundAssemblies = _typeFinder.GetAssemblies(ignoreInactivePlugins: true).ToArray();

			// register all api controllers
			builder.RegisterApiControllers(foundAssemblies);
			
			builder.RegisterType<WebApiConfigurationPublisher>().As<IWebApiConfigurationPublisher>();
		}

		protected override void AttachToComponentRegistration(IComponentRegistry componentRegistry, IComponentRegistration registration)
		{
			if (!DataSettings.DatabaseIsInstalled())
				return;
			
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
			builder.Register<DefaultThemeRegistry>(x => new DefaultThemeRegistry(x.Resolve<IEventPublisher>(), null, null, true)).As<IThemeRegistry>().SingleInstance();
			builder.RegisterType<ThemeFileResolver>().As<IThemeFileResolver>().SingleInstance();

			builder.RegisterType<ThemeContext>().As<IThemeContext>().InstancePerRequest();
			builder.RegisterType<ThemeVariablesService>().As<IThemeVariablesService>().InstancePerRequest();

			// register UI component renderers
			builder.RegisterType<TabStripRenderer>().As<ComponentRenderer<TabStrip>>();
			builder.RegisterType<PagerRenderer>().As<ComponentRenderer<Pager>>();
			builder.RegisterType<WindowRenderer>().As<ComponentRenderer<Window>>();

			builder.RegisterType<WidgetProvider>().As<IWidgetProvider>().InstancePerRequest();
			builder.RegisterType<MenuPublisher>().As<IMenuPublisher>().InstancePerRequest();
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

	public class ProvidersModule : Module
	{
		private readonly ITypeFinder _typeFinder;
		private readonly IPluginFinder _pluginFinder;

		public ProvidersModule(ITypeFinder typeFinder, IPluginFinder pluginFinder)
		{
			_typeFinder = typeFinder;
			_pluginFinder = pluginFinder;
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<ProviderManager>().As<IProviderManager>().InstancePerRequest();
			
			if (!DataSettings.DatabaseIsInstalled())
				return;

			var providerTypes = _typeFinder.FindClassesOfType<IProvider>(ignoreInactivePlugins: true).ToList();

			foreach (var type in providerTypes)
			{
				var pluginDescriptor = _pluginFinder.GetPluginDescriptorByAssembly(type.Assembly);
				var groupName = ProviderTypeToKnownGroupName(type);
				var systemName = GetSystemName(type, pluginDescriptor);
				var friendlyName = GetFriendlyName(type, pluginDescriptor);
				var displayOrder = GetDisplayOrder(type, pluginDescriptor);
				var dependentWidgets = GetDependentWidgets(type);
				var resPattern = (pluginDescriptor != null ? "Plugins" : "Providers") + ".{1}.{0}"; // e.g. Plugins.FriendlyName.MySystemName
				var settingPattern = (pluginDescriptor != null ? "Plugins" : "Providers") + ".{0}.{1}"; // e.g. Plugins.MySystemName.DisplayOrder
				var isConfigurable = typeof(IConfigurable).IsAssignableFrom(type);
				var isEditable = typeof(IUserEditable).IsAssignableFrom(type);

				var registration = builder.RegisterType(type).Named<IProvider>(systemName).InstancePerRequest().PropertiesAutowired(PropertyWiringOptions.None);
				registration.WithMetadata<ProviderMetadata>(m =>
				{
					m.For(em => em.PluginDescriptor, pluginDescriptor);
					m.For(em => em.GroupName, groupName);
					m.For(em => em.SystemName, systemName);
					m.For(em => em.ResourceKeyPattern, resPattern);
					m.For(em => em.SettingKeyPattern, settingPattern);
					m.For(em => em.FriendlyName, friendlyName.Item1);
					m.For(em => em.Description, friendlyName.Item2);
					m.For(em => em.DisplayOrder, displayOrder);
					m.For(em => em.DependentWidgets, dependentWidgets);
					m.For(em => em.IsConfigurable, isConfigurable);
					m.For(em => em.IsEditable, isEditable);
				});

				// register specific provider type
				RegisterAsSpecificProvider<ITaxProvider>(type, systemName, registration);
				RegisterAsSpecificProvider<IDiscountRequirementRule>(type, systemName, registration);
				RegisterAsSpecificProvider<IExchangeRateProvider>(type, systemName, registration);
				RegisterAsSpecificProvider<IShippingRateComputationMethod>(type, systemName, registration);
				RegisterAsSpecificProvider<IWidget>(type, systemName, registration);
				RegisterAsSpecificProvider<IExternalAuthenticationMethod>(type, systemName, registration);
				RegisterAsSpecificProvider<IPaymentMethod>(type, systemName, registration);
			}

		}

		#region Helpers

		private void RegisterAsSpecificProvider<T>(Type implType, string systemName, IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle> registration) where T : IProvider
		{
			if (typeof(T).IsAssignableFrom(implType))
			{
				try
				{
					registration.As<T>().Named<T>(systemName);
					registration.WithMetadata<ProviderMetadata>(m =>
					{
						m.For(em => em.ProviderType, typeof(T));
					});
				}
				catch (Exception) { }
			}
		}

		private string GetSystemName(Type type, PluginDescriptor descriptor)
		{
			var attr = type.GetAttribute<SystemNameAttribute>(false);
			if (attr != null)
			{
				return attr.Name;
			}

			if (typeof(IPlugin).IsAssignableFrom(type) && descriptor != null)
			{
				return descriptor.SystemName;
			}

			return type.FullName;
			//throw Error.Application("The 'SystemNameAttribute' must be applied to a provider type if the provider does not implement 'IPlugin' (provider type: {0}, plugin: {1})".FormatInvariant(type.FullName, descriptor != null ? descriptor.SystemName : "-"));
		}

		private int GetDisplayOrder(Type type, PluginDescriptor descriptor)
		{
			var attr = type.GetAttribute<DisplayOrderAttribute>(false);
			if (attr != null)
			{
				return attr.DisplayOrder;
			}

			if (typeof(IPlugin).IsAssignableFrom(type) && descriptor != null)
			{
				return descriptor.DisplayOrder;
			}

			return 0;
		}

		private Tuple<string/*Name*/, string/*Description*/> GetFriendlyName(Type type, PluginDescriptor descriptor)
		{
			string name = null;
			string description = name;

			var attr = type.GetAttribute<FriendlyNameAttribute>(false);
			if (attr != null)
			{
				name = attr.Name;
				description = attr.Description;
			}
			else if (typeof(IPlugin).IsAssignableFrom(type) && descriptor != null)
			{
				name = descriptor.FriendlyName;
				description = descriptor.Description;
			}
			else
			{
				name = Inflector.Titleize(type.Name);
				//throw Error.Application("The 'FriendlyNameAttribute' must be applied to a provider type if the provider does not implement 'IPlugin' (provider type: {0}, plugin: {1})".FormatInvariant(type.FullName, descriptor != null ? descriptor.SystemName : "-"));
			}

			return new Tuple<string, string>(name, description);
		}

		private string[] GetDependentWidgets(Type type)
		{
			if (!typeof(IWidget).IsAssignableFrom(type))
			{
				// don't let widgets depend on other widgets
				var attr = type.GetAttribute<DependentWidgetsAttribute>(false);
				if (attr != null)
				{
					return attr.WidgetSystemNames;
				}
			}

			return new string[] {};
		}

		private string ProviderTypeToKnownGroupName(Type implType)
		{
			if (typeof(ITaxProvider).IsAssignableFrom(implType))
			{
				return "Tax";
			}
			else if (typeof(IDiscountRequirementRule).IsAssignableFrom(implType))
			{
				return "Marketing";
			}
			else if (typeof(IExchangeRateProvider).IsAssignableFrom(implType))
			{
				return "Payment";
			}
			else if (typeof(IShippingRateComputationMethod).IsAssignableFrom(implType))
			{
				return "Shipping";
			}
			else if (typeof(IPaymentMethod).IsAssignableFrom(implType))
			{
				return "Payment";
			}
			else if (typeof(IExternalAuthenticationMethod).IsAssignableFrom(implType))
			{
				return "Security";
			}
			else if (typeof(IWidget).IsAssignableFrom(implType))
			{
				return "CMS";
			}

			return null;
		}

		#endregion

	}

    public class TasksModule : Module
    {
        private readonly ITypeFinder _typeFinder;

        public TasksModule(ITypeFinder typeFinder)
        {
            _typeFinder = typeFinder;
        }

        protected override void Load(ContainerBuilder builder)
        {
            if (!DataSettings.DatabaseIsInstalled())
                return;

            builder.RegisterType<DefaultTaskScheduler>().As<ITaskScheduler>().SingleInstance();
            builder.RegisterType<TaskExecutor>().As<ITaskExecutor>().InstancePerRequest();

            var taskTypes = _typeFinder.FindClassesOfType<ITask>(ignoreInactivePlugins: true).ToList();

            foreach (var type in taskTypes)
            {
                var typeName = type.FullName;
                builder.RegisterType(type).Named<ITask>(typeName).Keyed<ITask>(type).InstancePerRequest();
            }

            // Register resolving delegate
            builder.Register<Func<Type, ITask>>(c =>
            {
                var cc = c.Resolve<IComponentContext>();
                return keyed => cc.ResolveKeyed<ITask>(keyed);
            });

            builder.Register<Func<string, ITask>>(c =>
            {
                var cc = c.Resolve<IComponentContext>();
                return named => cc.ResolveNamed<ITask>(named);
            });

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

						////although it's better to connect to your database and execute the following SQL:
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
