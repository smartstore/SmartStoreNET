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
using SmartStore.Services.Events;
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
using SmartStore.Services.Installation;
using SmartStore.Services.Localization;
using SmartStore.Services.Logging;
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
using SmartStore.Web.Framework.UI.Editor;
using SmartStore.Services.Filter;
using SmartStore.Web.Framework.WebApi.Routes;
using System.Web.Http.Controllers;
using System.Web.Http;
using SmartStore.Core.Data.Hooks;
using dotless.Core.Parameters;
using SmartStore.Core.Themes;
using SmartStore.Services.Themes;
using SmartStore.Core.Domain.Messages;
using SmartStore.Services.Stores;

namespace SmartStore.Web.Framework
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder)
        {

            var foundAssemblies = typeFinder.GetAssemblies().ToArray(); // codehint: sm-add

            // codehint: sm-edit
            builder.RegisterModule(new AutofacWebTypesModule());
            builder.Register(c =>
                //register FakeHttpContext when HttpContext is not available
                HttpContext.Current != null ?
                (new HttpContextWrapper(HttpContext.Current) as HttpContextBase) :
                (new FakeHttpContext("~/") as HttpContextBase))
                .As<HttpContextBase>()
                .InstancePerHttpRequest();

            //web helper
            builder.RegisterType<WebHelper>().As<IWebHelper>().InstancePerHttpRequest();

            //controllers
            builder.RegisterControllers(foundAssemblies);

            //// codehint: sm-add
            //// http controllers (web api)
            ////builder.RegisterWebApiFilterProvider(GlobalConfiguration.Configuration);
            ////builder.RegisterWebApiModelBinderProvider();
            ////builder.RegisterWebApiModelBinders(foundAssemblies);
            //builder.RegisterApiControllers(foundAssemblies).PropertiesAutowired();
           
            //data layer
            var dataSettingsManager = new DataSettingsManager();
            var dataProviderSettings = dataSettingsManager.LoadSettings();
            builder.Register(c => dataSettingsManager.LoadSettings()).As<DataSettings>().SingleInstance(); // xxx (leer)
            builder.Register(x => new EfDataProviderManager(x.Resolve<DataSettings>())).As<BaseDataProviderManager>().SingleInstance(); // xxx (perdep)


            builder.Register(x => (IEfDataProvider)x.Resolve<BaseDataProviderManager>().LoadDataProvider()).As<IDataProvider>().InstancePerDependency();
            builder.Register(x => (IEfDataProvider)x.Resolve<BaseDataProviderManager>().LoadDataProvider()).As<IEfDataProvider>().InstancePerDependency();

            string dbContextParam = string.Empty;
            if (dataProviderSettings != null && dataProviderSettings.IsValid())
            {
                var efDataProviderManager = new EfDataProviderManager(dataSettingsManager.LoadSettings());
                var dataProvider = (IEfDataProvider)efDataProviderManager.LoadDataProvider();
                dataProvider.InitConnectionFactory();

                builder.Register<IDbContext>(c => new SmartObjectContext(dataProviderSettings.DataConnectionString))
                    .InstancePerHttpRequest()
                    .PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies);
            }
            else
            {
                builder.Register<IDbContext>(c => new SmartObjectContext(dataSettingsManager.LoadSettings().DataConnectionString))
                    .InstancePerHttpRequest()
                    .PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies);
            }

            builder.RegisterGeneric(typeof(EfRepository<>)).As(typeof(IRepository<>)).InstancePerHttpRequest();

            //// register DB Hooks (codehint: sm-add)
            builder.RegisterType<LocalizedEntityPostDeleteHook>().As<IHook>();
            
            //plugins
            builder.RegisterType<PluginFinder>().As<IPluginFinder>().SingleInstance(); // xxx (http)

            //cache manager
            builder.RegisterType<StaticCache>().As<ICache>().Named<ICache>("static").SingleInstance();
            builder.RegisterType<RequestCache>().As<ICache>().Named<ICache>("request").InstancePerHttpRequest();
            
            builder.RegisterType<DefaultCacheManager>()
                .As<ICacheManager>()
                .Named<ICacheManager>("sm_cache_static")
                .WithParameter(ResolvedParameter.ForNamed<ICache>("static"))
                .InstancePerHttpRequest();
            builder.RegisterType<DefaultCacheManager>()
                .As<ICacheManager>()
                .Named<ICacheManager>("sm_cache_per_request")
                .WithParameter(ResolvedParameter.ForNamed<ICache>("request"))
                .InstancePerHttpRequest();

            //work context
            builder.RegisterType<WebWorkContext>().As<IWorkContext>()
                .WithParameter(ResolvedParameter.ForNamed<ICacheManager>("sm_cache_static"))
                .InstancePerHttpRequest();

            //services
            builder.RegisterType<BackInStockSubscriptionService>().As<IBackInStockSubscriptionService>().InstancePerHttpRequest();
            builder.RegisterType<CategoryService>().As<ICategoryService>().InstancePerHttpRequest();
            builder.RegisterType<CompareProductsService>().As<ICompareProductsService>().InstancePerHttpRequest();
            builder.RegisterType<RecentlyViewedProductsService>().As<IRecentlyViewedProductsService>().InstancePerHttpRequest();
            builder.RegisterType<ManufacturerService>().As<IManufacturerService>().InstancePerHttpRequest();
            builder.RegisterType<PriceCalculationService>().As<IPriceCalculationService>().InstancePerHttpRequest();
            builder.RegisterType<PriceFormatter>().As<IPriceFormatter>().InstancePerHttpRequest();
            builder.RegisterType<ProductAttributeFormatter>().As<IProductAttributeFormatter>().InstancePerLifetimeScope();
            builder.RegisterType<ProductAttributeParser>().As<IProductAttributeParser>().InstancePerHttpRequest();
            builder.RegisterType<ProductAttributeService>().As<IProductAttributeService>().InstancePerHttpRequest().PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies); // codehint: sm-edit (Autowiring)
            builder.RegisterType<ProductService>().As<IProductService>().InstancePerHttpRequest();
            builder.RegisterType<CopyProductService>().As<ICopyProductService>().InstancePerHttpRequest();
            builder.RegisterType<ProductTagService>().As<IProductTagService>().InstancePerHttpRequest();
            builder.RegisterType<SpecificationAttributeService>().As<ISpecificationAttributeService>().InstancePerHttpRequest();
            builder.RegisterType<ProductTemplateService>().As<IProductTemplateService>().InstancePerHttpRequest();
            builder.RegisterType<CategoryTemplateService>().As<ICategoryTemplateService>().InstancePerHttpRequest();
            builder.RegisterType<ManufacturerTemplateService>().As<IManufacturerTemplateService>().InstancePerHttpRequest();

            builder.RegisterType<AffiliateService>().As<IAffiliateService>().InstancePerHttpRequest();
            builder.RegisterType<AddressService>().As<IAddressService>().InstancePerHttpRequest();
            builder.RegisterType<GenericAttributeService>().As<IGenericAttributeService>().InstancePerHttpRequest();
            builder.RegisterType<FulltextService>().As<IFulltextService>().InstancePerHttpRequest();
            builder.RegisterType<MaintenanceService>().As<IMaintenanceService>().InstancePerHttpRequest();
 

            builder.RegisterGeneric(typeof(ConfigurationProvider<>)).As(typeof(IConfigurationProvider<>));
            builder.RegisterSource(new SettingsSource());
            
            builder.RegisterType<CustomerContentService>().As<ICustomerContentService>().InstancePerHttpRequest();
            builder.RegisterType<CustomerService>().As<ICustomerService>().InstancePerHttpRequest();
            builder.RegisterType<CustomerRegistrationService>().As<ICustomerRegistrationService>().InstancePerHttpRequest();
            builder.RegisterType<CustomerReportService>().As<ICustomerReportService>().InstancePerHttpRequest();

            //pass MemoryCacheManager to SettingService as cacheManager (cache settngs between requests)
            builder.RegisterType<PermissionService>().As<IPermissionService>()
                .WithParameter(ResolvedParameter.ForNamed<ICacheManager>("sm_cache_static"))
                .InstancePerHttpRequest();

            //pass MemoryCacheManager to SettingService as cacheManager (cache settings between requests)
            builder.RegisterType<AclService>().As<IAclService>()
                .WithParameter(ResolvedParameter.ForNamed<ICacheManager>("sm_cache_static"))
                .InstancePerHttpRequest();

            builder.RegisterType<GeoCountryLookup>().As<IGeoCountryLookup>().InstancePerHttpRequest();
            builder.RegisterType<CountryService>().As<ICountryService>().InstancePerHttpRequest();
            builder.RegisterType<CurrencyService>().As<ICurrencyService>().InstancePerHttpRequest();

            //codehint: sm-add
            builder.RegisterType<DeliveryTimeService>().As<IDeliveryTimeService>().InstancePerHttpRequest();

            builder.RegisterType<MeasureService>().As<IMeasureService>().InstancePerHttpRequest();
            builder.RegisterType<StateProvinceService>().As<IStateProvinceService>().InstancePerHttpRequest();

			builder.RegisterType<StoreService>().As<IStoreService>().InstancePerHttpRequest();
			//pass MemoryCacheManager to SettingService as cacheManager (cache settings between requests)
			builder.RegisterType<StoreMappingService>().As<IStoreMappingService>()
				.WithParameter(ResolvedParameter.ForNamed<ICacheManager>("sm_cache_static"))
				.InstancePerHttpRequest();

            builder.RegisterType<DiscountService>().As<IDiscountService>().InstancePerHttpRequest();


            //pass MemoryCacheManager to SettingService as cacheManager (cache settngs between requests)
            builder.RegisterType<SettingService>().As<ISettingService>()
                .WithParameter(ResolvedParameter.ForNamed<ICacheManager>("sm_cache_static"))
                .InstancePerHttpRequest();
            //pass MemoryCacheManager to LocalizationService as cacheManager (cache locales between requests)
            builder.RegisterType<LocalizationService>().As<ILocalizationService>()
                .WithParameter(ResolvedParameter.ForNamed<ICacheManager>("sm_cache_static"))
                .InstancePerHttpRequest();

            //pass MemoryCacheManager to LocalizedEntityService as cacheManager (cache locales between requests)
            builder.RegisterType<LocalizedEntityService>().As<ILocalizedEntityService>()
                .WithParameter(ResolvedParameter.ForNamed<ICacheManager>("sm_cache_static"))
                .InstancePerHttpRequest();
            builder.RegisterType<LanguageService>().As<ILanguageService>().InstancePerHttpRequest();

            builder.RegisterType<DownloadService>().As<IDownloadService>().InstancePerHttpRequest();
            builder.RegisterType<ImageCache>().As<IImageCache>().InstancePerHttpRequest(); // codehint: sm-add
            builder.RegisterType<ImageResizerService>().As<IImageResizerService>().SingleInstance(); // xxx (http) // codehint: sm-add
            builder.RegisterType<PictureService>().As<IPictureService>().InstancePerHttpRequest();

            builder.RegisterType<MessageTemplateService>().As<IMessageTemplateService>().InstancePerHttpRequest();
            builder.RegisterType<QueuedEmailService>().As<IQueuedEmailService>().InstancePerHttpRequest();
            builder.RegisterType<NewsLetterSubscriptionService>().As<INewsLetterSubscriptionService>().InstancePerHttpRequest();
            builder.RegisterType<CampaignService>().As<ICampaignService>().InstancePerHttpRequest();
            builder.RegisterType<EmailAccountService>().As<IEmailAccountService>().InstancePerHttpRequest();
            builder.RegisterType<WorkflowMessageService>().As<IWorkflowMessageService>().InstancePerHttpRequest();
            builder.RegisterType<MessageTokenProvider>().As<IMessageTokenProvider>().InstancePerHttpRequest();
            builder.RegisterType<Tokenizer>().As<ITokenizer>().InstancePerHttpRequest();
            builder.RegisterType<EmailSender>().As<IEmailSender>().SingleInstance(); // xxx (http)

            builder.RegisterType<CheckoutAttributeFormatter>().As<ICheckoutAttributeFormatter>().InstancePerHttpRequest();
            builder.RegisterType<CheckoutAttributeParser>().As<ICheckoutAttributeParser>().InstancePerHttpRequest();
            builder.RegisterType<CheckoutAttributeService>().As<ICheckoutAttributeService>().InstancePerHttpRequest();
            builder.RegisterType<GiftCardService>().As<IGiftCardService>().InstancePerHttpRequest();
            builder.RegisterType<OrderService>().As<IOrderService>().InstancePerHttpRequest();
            builder.RegisterType<OrderReportService>().As<IOrderReportService>().InstancePerHttpRequest();
            builder.RegisterType<OrderProcessingService>().As<IOrderProcessingService>().InstancePerHttpRequest();
            builder.RegisterType<OrderTotalCalculationService>().As<IOrderTotalCalculationService>().InstancePerHttpRequest();
            builder.RegisterType<ShoppingCartService>().As<IShoppingCartService>().InstancePerHttpRequest();

            builder.RegisterType<PaymentService>().As<IPaymentService>().InstancePerHttpRequest();

            builder.RegisterType<EncryptionService>().As<IEncryptionService>().InstancePerHttpRequest();
            builder.RegisterType<FormsAuthenticationService>().As<IAuthenticationService>().InstancePerHttpRequest();

            //pass MemoryCacheManager to UrlRecordService as cacheManager (cache settings between requests)
            builder.RegisterType<UrlRecordService>().As<IUrlRecordService>()
                .WithParameter(ResolvedParameter.ForNamed<ICacheManager>("sm_cache_static"))
                .InstancePerHttpRequest();

            builder.RegisterType<ShipmentService>().As<IShipmentService>().InstancePerHttpRequest();
            builder.RegisterType<ShippingService>().As<IShippingService>().InstancePerHttpRequest();

            builder.RegisterType<TaxCategoryService>().As<ITaxCategoryService>().InstancePerHttpRequest();
            builder.RegisterType<TaxService>().As<ITaxService>().InstancePerHttpRequest();
            builder.RegisterType<TaxCategoryService>().As<ITaxCategoryService>().InstancePerHttpRequest();

            builder.RegisterType<DefaultLogger>().As<ILogger>().InstancePerHttpRequest();
            builder.RegisterType<CustomerActivityService>().As<ICustomerActivityService>().InstancePerHttpRequest();

            builder.RegisterType<InstallationService>().As<IInstallationService>().InstancePerHttpRequest();

            builder.RegisterType<ForumService>().As<IForumService>().InstancePerHttpRequest();
            
            builder.RegisterType<PollService>().As<IPollService>().InstancePerHttpRequest();
            builder.RegisterType<BlogService>().As<IBlogService>().InstancePerHttpRequest();
            builder.RegisterType<WidgetService>().As<IWidgetService>().InstancePerHttpRequest();
            builder.RegisterType<TopicService>().As<ITopicService>().InstancePerHttpRequest();
            builder.RegisterType<NewsService>().As<INewsService>().InstancePerHttpRequest();

            builder.RegisterType<DateTimeHelper>().As<IDateTimeHelper>().InstancePerHttpRequest();
            builder.RegisterType<SitemapGenerator>().As<ISitemapGenerator>().InstancePerHttpRequest();
            builder.RegisterType<PageTitleBuilder>().As<IPageTitleBuilder>().InstancePerHttpRequest();

            builder.RegisterType<ScheduleTaskService>().As<IScheduleTaskService>().InstancePerHttpRequest();

            builder.RegisterType<TelerikLocalizationServiceFactory>().As<Telerik.Web.Mvc.Infrastructure.ILocalizationServiceFactory>().InstancePerHttpRequest();

            builder.RegisterType<ExportManager>().As<IExportManager>().InstancePerHttpRequest();
            builder.RegisterType<ImportManager>().As<IImportManager>().InstancePerHttpRequest();
            builder.RegisterType<MobileDeviceHelper>().As<IMobileDeviceHelper>().InstancePerHttpRequest();
            builder.RegisterType<PdfService>().As<IPdfService>().InstancePerHttpRequest();
            builder.RegisterType<DefaultThemeRegistry>().As<IThemeRegistry>().SingleInstance(); // codehint: sm-edit (InstancePerHttpRequest > SingleInstance)
            builder.RegisterType<ThemeContext>().As<IThemeContext>().InstancePerHttpRequest();

            builder.RegisterType<ExternalAuthorizer>().As<IExternalAuthorizer>().InstancePerHttpRequest();
            builder.RegisterType<OpenAuthenticationService>().As<IOpenAuthenticationService>().InstancePerHttpRequest();

			// codehint: sm-add
			builder.RegisterType<FilterService>().As<IFilterService>().InstancePerHttpRequest();          
                
            builder.RegisterType<EmbeddedViewResolver>().As<IEmbeddedViewResolver>().SingleInstance();
            builder.RegisterType<RoutePublisher>().As<IRoutePublisher>().SingleInstance();
            // codehint: sm-add
            builder.RegisterType<HttpRoutePublisher>().As<IHttpRoutePublisher>().SingleInstance();
            builder.RegisterType<BundlePublisher>().As<IBundlePublisher>().SingleInstance();

            //HTML Editor services
            builder.RegisterType<NetAdvDirectoryService>().As<INetAdvDirectoryService>().InstancePerHttpRequest();
            builder.RegisterType<NetAdvImageService>().As<INetAdvImageService>().SingleInstance(); // xxx (http)

            //Register event consumers
            var consumers = typeFinder.FindClassesOfType(typeof(IConsumer<>)).ToList();
            foreach (var consumer in consumers)
            {
                builder.RegisterType(consumer)
                    .As(consumer.FindInterfaces((type, criteria) =>
                    {
                        var isMatch = type.IsGenericType && ((Type)criteria).IsAssignableFrom(type.GetGenericTypeDefinition());
                        return isMatch;
                    }, typeof(IConsumer<>)))
                    .InstancePerHttpRequest();
            }
            builder.RegisterType<EventPublisher>().As<IEventPublisher>().SingleInstance();
            builder.RegisterType<SubscriptionService>().As<ISubscriptionService>().SingleInstance();

            // register theming services (codehint: sm-add)
            builder.RegisterType<DbParameterSource>().As<IParameterSource>()
                .WithParameter(ResolvedParameter.ForNamed<ICacheManager>("sm_cache_static"));
            builder.RegisterType<ThemeVariablesService>().As<IThemeVariablesService>().InstancePerHttpRequest();

            // register UI component renderers (codehint: sm-add)
            builder.RegisterType<TabStripRenderer>().As<ComponentRenderer<TabStrip>>();
            builder.RegisterType<PagerRenderer>().As<ComponentRenderer<Pager>>();
            builder.RegisterType<WindowRenderer>().As<ComponentRenderer<Window>>();

            //// codehint: sm-add (enable mvc action filter property injection) >>> CRASHES! :-(
            //builder.RegisterFilterProvider();

        }

        public int Order
        {
            get { return 0; }
        }
    }


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
                var buildMethod = BuildMethod.MakeGenericMethod(ts.ServiceType);
                yield return (IComponentRegistration)buildMethod.Invoke(null, null);
            }
        }

        static IComponentRegistration BuildRegistration<TSettings>() where TSettings : ISettings, new()
        {
            return RegistrationBuilder
                .ForDelegate((c, p) => c.Resolve<IConfigurationProvider<TSettings>>().Settings)
                .InstancePerHttpRequest()
                .CreateRegistration();
        }

        public bool IsAdapterForIndividualComponents { get { return false; } }
    }

}
