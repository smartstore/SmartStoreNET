using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Hosting;
using Autofac;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Registration;
using Autofac.Integration.Mvc;
using Autofac.Integration.WebApi;
using SmartStore.ComponentModel;
using SmartStore.Core;
using SmartStore.Core.Async;
using SmartStore.Core.Caching;
using SmartStore.Core.Configuration;
using SmartStore.Core.Data;
using SmartStore.Core.Data.Hooks;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Email;
using SmartStore.Core.Events;
using SmartStore.Core.Fakes;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Infrastructure.DependencyManagement;
using SmartStore.Core.IO;
using SmartStore.Core.Localization;
using SmartStore.Core.Logging;
using SmartStore.Core.Packaging;
using SmartStore.Core.Plugins;
using SmartStore.Core.Search;
using SmartStore.Core.Security;
using SmartStore.Core.Themes;
using SmartStore.Data;
using SmartStore.Data.Caching;
using SmartStore.Rules;
using SmartStore.Services;
using SmartStore.Services.Affiliates;
using SmartStore.Services.Authentication;
using SmartStore.Services.Authentication.External;
using SmartStore.Services.Blogs;
using SmartStore.Services.Cart.Rules;
using SmartStore.Services.Catalog;
using SmartStore.Services.Catalog.Extensions;
using SmartStore.Services.Catalog.Importer;
using SmartStore.Services.Catalog.Modelling;
using SmartStore.Services.Catalog.Rules;
using SmartStore.Services.Cms;
using SmartStore.Services.Common;
using SmartStore.Services.Configuration;
using SmartStore.Services.Customers;
using SmartStore.Services.Customers.Importer;
using SmartStore.Services.DataExchange;
using SmartStore.Services.DataExchange.Export;
using SmartStore.Services.DataExchange.Import;
using SmartStore.Services.Directory;
using SmartStore.Services.Discounts;
using SmartStore.Services.Events;
using SmartStore.Services.Forums;
using SmartStore.Services.Helpers;
using SmartStore.Services.Localization;
using SmartStore.Services.Logging;
using SmartStore.Services.Media;
using SmartStore.Services.Media.Storage;
using SmartStore.Services.Messages;
using SmartStore.Services.Messages.Importer;
using SmartStore.Services.News;
using SmartStore.Services.Orders;
using SmartStore.Services.Payments;
using SmartStore.Services.Pdf;
using SmartStore.Services.Polls;
using SmartStore.Services.Rules;
using SmartStore.Services.Search;
using SmartStore.Services.Search.Extensions;
using SmartStore.Services.Search.Modelling;
using SmartStore.Services.Search.Rendering;
using SmartStore.Services.Security;
using SmartStore.Services.Seo;
using SmartStore.Services.Shipping;
using SmartStore.Services.Stores;
using SmartStore.Services.Tasks;
using SmartStore.Services.Tax;
using SmartStore.Services.Themes;
using SmartStore.Services.Topics;
using SmartStore.Templating;
using SmartStore.Templating.Liquid;
using SmartStore.Utilities;
using SmartStore.Web.Framework.Bundling;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Localization;
using SmartStore.Web.Framework.Plugins;
using SmartStore.Web.Framework.Routing;
using SmartStore.Web.Framework.Theming;
using SmartStore.Web.Framework.Theming.Assets;
using SmartStore.Web.Framework.UI;
using SmartStore.Web.Framework.WebApi.Configuration;
using SmartStore.Web.Framework.WebApi.OData;
using Module = Autofac.Module;

namespace SmartStore.Web.Framework
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, bool isActiveModule)
        {
            // plugins
            var pluginFinder = PluginFinder.Current;
            builder.RegisterInstance(pluginFinder);
            builder.RegisterType<PluginMediator>();

            // modules
            builder.RegisterModule(new CoreModule(typeFinder));
            builder.RegisterModule(new MediaModule(typeFinder));
            builder.RegisterModule(new DbModule(typeFinder));
            builder.RegisterModule(new CachingModule());
            builder.RegisterModule(new SearchModule());
            builder.RegisterModule(new LocalizationModule());
            builder.RegisterModule(new MessagingModule());
            builder.RegisterModule(new WebModule(typeFinder));
            builder.RegisterModule(new WebApiModule(typeFinder));
            builder.RegisterModule(new UiModule(typeFinder));
            builder.RegisterModule(new IOModule());
            builder.RegisterModule(new PackagingModule());
            builder.RegisterModule(new ProvidersModule(typeFinder, pluginFinder));
            builder.RegisterModule(new TasksModule(typeFinder));
            builder.RegisterModule(new DataExchangeModule(typeFinder));
            builder.RegisterModule(new EventModule(typeFinder, pluginFinder));
            builder.RegisterModule(new RuleModule(typeFinder));
        }

        public int Order => -100;
    }

    #region Modules

    internal class CoreModule : Module
    {
        private readonly ITypeFinder _typeFinder;

        public CoreModule(ITypeFinder typeFinder)
        {
            _typeFinder = typeFinder;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ApplicationEnvironment>().As<IApplicationEnvironment>().SingleInstance();

            // sources
            builder.RegisterGeneric(typeof(WorkValues<>)).InstancePerRequest();
            builder.RegisterSource(new SettingsSource());
            builder.RegisterSource(new WorkSource());

            // Logging stuff
            builder.RegisterType<Notifier>().As<INotifier>().InstancePerRequest();
            builder.RegisterType<DbLogService>().As<ILogService>().InstancePerRequest();
            builder.RegisterType<CustomerActivityService>().As<ICustomerActivityService>().InstancePerRequest();

            // web helper
            builder.RegisterType<WebHelper>().As<IWebHelper>().InstancePerRequest();

            // work context
            builder.RegisterType<WebWorkContext>().As<IWorkContext>().InstancePerRequest();

            // store context
            builder.RegisterType<WebStoreContext>().As<IStoreContext>().InstancePerRequest();

            // services
            builder.RegisterType<CategoryService>().As<ICategoryService>().As<IXmlSitemapPublisher>().InstancePerRequest();

            builder.RegisterType<ManufacturerService>().As<IManufacturerService>()
                .As<IXmlSitemapPublisher>()
                .WithNullCache()
                .InstancePerRequest();

            builder.RegisterType<ProductService>().As<IProductService>().InstancePerRequest();

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
            builder.RegisterType<ProductTagService>().As<IProductTagService>().InstancePerRequest();
            builder.RegisterType<ProductVariantQueryFactory>().As<IProductVariantQueryFactory>().InstancePerRequest();
            builder.RegisterType<ProductUrlHelper>().InstancePerRequest();

            builder.RegisterType<AffiliateService>().As<IAffiliateService>().InstancePerRequest();
            builder.RegisterType<AddressService>().As<IAddressService>().InstancePerRequest();
            builder.RegisterType<GenericAttributeService>().As<IGenericAttributeService>().InstancePerRequest();
            builder.RegisterType<MaintenanceService>().As<IMaintenanceService>().InstancePerRequest();

            builder.RegisterType<CustomerContentService>().As<ICustomerContentService>().InstancePerRequest();
            builder.RegisterType<CustomerService>().As<ICustomerService>().InstancePerRequest();
            builder.RegisterType<CustomerRegistrationService>().As<ICustomerRegistrationService>().InstancePerRequest();
            builder.RegisterType<CustomerReportService>().As<ICustomerReportService>().InstancePerRequest();

            builder.RegisterType<PermissionService>().As<IPermissionService>().InstancePerRequest();
            builder.RegisterType<AclService>().As<IAclService>().InstancePerRequest();
            builder.RegisterType<GdprTool>().As<IGdprTool>().InstancePerRequest();
            builder.RegisterType<CookieManager>().As<ICookieManager>().InstancePerRequest();

            builder.RegisterType<GeoCountryLookup>().As<IGeoCountryLookup>().SingleInstance();
            builder.RegisterType<CountryService>().As<ICountryService>().InstancePerRequest();
            builder.RegisterType<CurrencyService>().As<ICurrencyService>().InstancePerRequest();

            builder.RegisterType<DeliveryTimeService>().As<IDeliveryTimeService>().InstancePerRequest();
            builder.RegisterType<QuantityUnitService>().As<IQuantityUnitService>().InstancePerRequest();
            builder.RegisterType<MeasureService>().As<IMeasureService>().InstancePerRequest();
            builder.RegisterType<StateProvinceService>().As<IStateProvinceService>().InstancePerRequest();

            builder.RegisterType<StoreService>().As<IStoreService>().InstancePerRequest();
            builder.RegisterType<StoreMappingService>().As<IStoreMappingService>().InstancePerRequest();

            builder.RegisterType<DiscountService>().As<IDiscountService>().InstancePerRequest();

            builder.RegisterType<SettingService>().As<ISettingService>().InstancePerRequest();

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

            builder.RegisterType<UrlRecordService>().As<IUrlRecordService>().InstancePerRequest();

            builder.RegisterType<ShipmentService>().As<IShipmentService>().InstancePerRequest();
            builder.RegisterType<ShippingService>().As<IShippingService>().InstancePerRequest();

            builder.RegisterType<TaxCategoryService>().As<ITaxCategoryService>().InstancePerRequest();
            builder.RegisterType<TaxService>().As<ITaxService>().InstancePerRequest();

            builder.RegisterType<ForumService>().As<IForumService>().As<IXmlSitemapPublisher>().InstancePerRequest();

            builder.RegisterType<PollService>().As<IPollService>().InstancePerRequest();
            builder.RegisterType<BlogService>().As<IBlogService>().As<IXmlSitemapPublisher>().InstancePerRequest();
            builder.RegisterType<TopicService>().As<ITopicService>().As<IXmlSitemapPublisher>().InstancePerRequest();
            builder.RegisterType<NewsService>().As<INewsService>().As<IXmlSitemapPublisher>().InstancePerRequest();

            builder.RegisterType<WidgetService>().As<IWidgetService>().InstancePerRequest();
            builder.RegisterType<MenuStorage>().As<IMenuStorage>().InstancePerRequest();
            builder.RegisterType<LinkResolver>().As<ILinkResolver>().InstancePerRequest();

            builder.RegisterType<DateTimeHelper>().As<IDateTimeHelper>().InstancePerRequest();
            builder.RegisterType<XmlSitemapGenerator>().As<IXmlSitemapGenerator>().InstancePerRequest();
            builder.RegisterType<PageAssetsBuilder>().As<IPageAssetsBuilder>().InstancePerRequest();

            builder.RegisterType<ScheduleTaskService>().As<IScheduleTaskService>().InstancePerRequest();
            builder.RegisterType<SyncMappingService>().As<ISyncMappingService>().InstancePerRequest();

            builder.RegisterType<MobileDeviceHelper>().As<IMobileDeviceHelper>().InstancePerRequest();
            builder.RegisterType<UAParserUserAgent>().As<IUserAgent>().InstancePerRequest();
            builder.RegisterType<WkHtmlToPdfConverter>().As<IPdfConverter>().InstancePerRequest();

            builder.RegisterType<ExternalAuthorizer>().As<IExternalAuthorizer>().InstancePerRequest();
            builder.RegisterType<OpenAuthenticationService>().As<IOpenAuthenticationService>().InstancePerRequest();

            builder.RegisterType<CommonServices>().As<ICommonServices>().InstancePerRequest();
        }

        protected override void AttachToComponentRegistration(IComponentRegistryBuilder componentRegistry, IComponentRegistration registration)
        {
            // Look for first settable property of type "ICommonServices" and inject
            var servicesProperty = FindCommonServicesProperty(registration.Activator.LimitType);

            if (servicesProperty == null)
                return;

            registration.Metadata.Add("Property.ICommonServices", FastProperty.Create(servicesProperty));

            registration.Activated += (sender, e) =>
            {
                if (DataSettings.DatabaseIsInstalled())
                {
                    var prop = e.Component.Metadata.Get("Property.ICommonServices") as FastProperty;
                    var services = e.Context.Resolve<ICommonServices>();
                    prop.SetValue(e.Instance, services);
                }
            };
        }

        private static PropertyInfo FindCommonServicesProperty(Type type)
        {
            var prop = type
                .GetProperties(BindingFlags.SetProperty | BindingFlags.Public | BindingFlags.Instance)
                .Select(p => new
                {
                    PropertyInfo = p,
                    p.PropertyType,
                    IndexParameters = p.GetIndexParameters(),
                    Accessors = p.GetAccessors(false)
                })
                .Where(x => x.PropertyType == typeof(ICommonServices)) // must be ICommonServices
                .Where(x => x.IndexParameters.Count() == 0) // must not be an indexer
                .Where(x => x.Accessors.Length != 1 || x.Accessors[0].ReturnType == typeof(void)) //must have get/set, or only set
                .Select(x => x.PropertyInfo)
                .FirstOrDefault();

            return prop;
        }

        private IEnumerable<Action<IComponentContext, object>> BuildLoggerInjectors(Type componentType)
        {
            // Look for first settable property of type "ICommonServices" 
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
                .Where(x => x.Accessors.Length != 1 || x.Accessors[0].ReturnType == typeof(void)) //must have get/set, or only set
                .Select(x => FastProperty.Create(x.PropertyInfo));

            // Return an array of actions that resolve a logger and assign the property
            foreach (var prop in loggerProperties)
            {
                yield return (ctx, instance) =>
                {
                    string component = componentType.ToString();
                    var logger = ctx.Resolve<ILogger>();
                    prop.SetValue(instance, logger);
                };
            }
        }
    }

    internal class DbModule : Module
    {
        private readonly ITypeFinder _typeFinder;

        public DbModule(ITypeFinder typeFinder)
        {
            _typeFinder = typeFinder;
        }

        private static (Type ContextType, Type EntityType) DiscoverHookTypes(Type type)
        {
            var x = type.BaseType;
            while (x != null && x != typeof(object))
            {
                if (x.IsGenericType)
                {
                    var gtd = x.GetGenericTypeDefinition();
                    if (gtd == typeof(DbSaveHook<>))
                    {
                        return (typeof(SmartObjectContext), x.GetGenericArguments()[0]);
                    }
                    if (gtd == typeof(DbSaveHook<,>))
                    {
                        var args = x.GetGenericArguments();
                        return (args[0], args[1]);
                    }
                }

                x = x.BaseType;
            }

            foreach (var intface in type.GetInterfaces())
            {
                if (intface.IsGenericType)
                {
                    var gtd = intface.GetGenericTypeDefinition();
                    if (gtd == typeof(IDbSaveHook<>))
                    {
                        return (intface.GetGenericArguments()[0], typeof(BaseEntity));
                    }
                }
            }

            return (typeof(SmartObjectContext), typeof(BaseEntity));
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(c => DataSettings.Current).As<DataSettings>().InstancePerDependency();
            builder.Register(x => new EfDataProviderFactory(x.Resolve<DataSettings>())).As<DataProviderFactory>().InstancePerDependency();

            builder.Register(x => x.Resolve<DataProviderFactory>().LoadDataProvider()).As<IDataProvider>().InstancePerDependency();
            builder.Register(x => (IEfDataProvider)x.Resolve<DataProviderFactory>().LoadDataProvider()).As<IEfDataProvider>().InstancePerDependency();

            builder.RegisterType<DefaultDbHookHandler>().As<IDbHookHandler>().InstancePerRequest();

            builder.RegisterType<EfDbCache>().As<IDbCache>().SingleInstance();

            if (DataSettings.DatabaseIsInstalled())
            {
                // Register DB Hooks (only when app was installed properly)

                var hookTypes = _typeFinder.FindClassesOfType<IDbSaveHook>(ignoreInactivePlugins: true);
                foreach (var hookType in hookTypes)
                {
                    var types = DiscoverHookTypes(hookType);

                    var registration = builder.RegisterType(hookType)
                        .As<IDbSaveHook>()
                        .InstancePerRequest()
                        .WithMetadata<HookMetadata>(m =>
                        {
                            m.For(em => em.HookedType, types.EntityType);
                            m.For(em => em.ImplType, hookType);
                            m.For(em => em.DbContextType, types.ContextType ?? typeof(SmartObjectContext));
                            m.For(em => em.Important, hookType.HasAttribute<ImportantAttribute>(false));
                        });
                }

                builder.Register<IDbContext>(c => new SmartObjectContext(DataSettings.Current.DataConnectionString))
                    //.PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies)
                    .PropertiesAutowired(new DbContextPropertySelector(), false)
                    .InstancePerRequest();
            }
            else
            {
                builder.Register<IDbContext>(c =>
                    {
                        try
                        {
                            return new SmartObjectContext(DataSettings.Current.DataConnectionString);
                        }
                        catch
                        {
                            // return new SmartObjectContext();
                            return null;
                        }

                    })
                    .PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies)
                    .InstancePerRequest();
            }

            builder.Register<Func<string, IDbContext>>(c =>
            {
                var cc = c.Resolve<IComponentContext>();
                return named => cc.ResolveNamed<IDbContext>(named);
            });

            builder.RegisterGeneric(typeof(EfRepository<>)).As(typeof(IRepository<>)).InstancePerRequest();

            builder.Register(c =>
            {
                var storeService = c.Resolve<IStoreService>();
                var aclService = c.Resolve<IAclService>();

                return new DbQuerySettings(!aclService.HasActiveAcl, storeService.IsSingleStoreMode());
            })
            .InstancePerRequest();
        }

        protected override void AttachToComponentRegistration(IComponentRegistryBuilder componentRegistry, IComponentRegistration registration)
        {
            var querySettingsProperty = FindQuerySettingsProperty(registration.Activator.LimitType);

            if (querySettingsProperty == null)
                return;

            registration.Metadata.Add("Property.DbQuerySettings", FastProperty.Create(querySettingsProperty));

            registration.Activated += (sender, e) =>
            {
                if (DataSettings.DatabaseIsInstalled())
                {
                    if (e.Component.Metadata.Get("Property.DbQuerySettings") is FastProperty prop)
                    {
                        var querySettings = e.Context.Resolve<DbQuerySettings>();
                        prop.SetValue(e.Instance, querySettings);
                    }
                }
            };
        }

        private static PropertyInfo FindQuerySettingsProperty(Type type)
        {
            return type.GetProperty("QuerySettings", typeof(DbQuerySettings));
        }

        private class DbContextPropertySelector : IPropertySelector
        {
            public bool InjectProperty(PropertyInfo propertyInfo, object instance)
            {
                // Prevent Autofac circularity exception & never trigger hooks during tooling or tests
                return HostingEnvironment.IsHosted && typeof(IDbHookHandler).IsAssignableFrom(propertyInfo.PropertyType);
            }
        }
    }

    internal class LocalizationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {

            builder.RegisterType<LanguageService>().As<ILanguageService>().InstancePerRequest();
            builder.RegisterType<TelerikLocalizationServiceFactory>().As<Telerik.Web.Mvc.Infrastructure.ILocalizationServiceFactory>().InstancePerRequest();
            builder.RegisterType<LocalizationService>().As<ILocalizationService>().InstancePerRequest();

            builder.RegisterType<Text>().As<IText>().InstancePerRequest();
            builder.Register<Localizer>(c => c.Resolve<IText>().Get).InstancePerRequest();
            builder.Register<LocalizerEx>(c => c.Resolve<IText>().GetEx).InstancePerRequest();

            builder.RegisterType<LocalizationFileResolver>().As<ILocalizationFileResolver>().InstancePerRequest();
            builder.RegisterType<LocalizedEntityService>().As<ILocalizedEntityService>().InstancePerRequest();
            builder.RegisterType<LocalizedEntityHelper>().InstancePerRequest();
        }

        protected override void AttachToComponentRegistration(IComponentRegistryBuilder componentRegistry, IComponentRegistration registration)
        {
            var userProperty = FindUserProperty(registration.Activator.LimitType);

            if (userProperty == null)
                return;

            registration.Metadata.Add("Property.T", FastProperty.Create(userProperty));

            registration.Activated += (sender, e) =>
            {
                if (DataSettings.DatabaseIsInstalled() && e.Context.Resolve<IEngine>().IsFullyInitialized)
                {
                    if (e.Component.Metadata.Get("Property.T") is FastProperty prop)
                    {
                        try
                        {
                            var iText = e.Context.Resolve<IText>();
                            if (prop.Property.PropertyType == typeof(Localizer))
                            {
                                Localizer localizer = e.Context.Resolve<IText>().Get;
                                prop.SetValue(e.Instance, localizer);
                            }
                            else
                            {
                                LocalizerEx localizerEx = e.Context.Resolve<IText>().GetEx;
                                prop.SetValue(e.Instance, localizerEx);
                            }
                        }
                        catch { }
                    }
                }
            };
        }

        private static PropertyInfo FindUserProperty(Type type)
        {
            return type.GetProperty("T", typeof(Localizer)) ?? type.GetProperty("T", typeof(LocalizerEx));
        }
    }

    internal class CachingModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // Output cache
            builder.RegisterType<DisplayControl>().As<IDisplayControl>().InstancePerRequest();
            builder.Register(c => NullOutputCacheInvalidationObserver.Instance).SingleInstance();
            builder.RegisterType<NullCacheableRouteRegistrar>().As<ICacheableRouteRegistrar>().InstancePerRequest();

            // Request cache
            builder.RegisterType<RequestCache>().As<IRequestCache>().InstancePerRequest();

            // Model/Business cache (application scoped)
            builder.RegisterType<CacheScopeAccessor>().As<ICacheScopeAccessor>().InstancePerRequest();
            builder.RegisterType<MemoryCacheManager>().As<ICacheManager>().SingleInstance();
            builder.RegisterType<NullCache>().Named<ICacheManager>("null").SingleInstance();

            // Register MemoryCacheManager twice, this time explicitly named.
            // We may need this later in decorator classes as a kind of fallback.
            builder.RegisterType<MemoryCacheManager>().Named<ICacheManager>("memory").SingleInstance();

            // Asset cache
            if (DataSettings.DatabaseIsInstalled())
            {
                builder.RegisterType<DefaultAssetCache>().As<IAssetCache>().InstancePerRequest();
            }
            else
            {
                builder.Register<IAssetCache>(c => DefaultAssetCache.Null).SingleInstance();
            }
        }
    }

    internal class SearchModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // General.
            builder.RegisterType<DefaultIndexManager>().As<IIndexManager>().InstancePerRequest();
            builder.RegisterType<FacetUrlHelper>().InstancePerRequest();
            builder.RegisterType<FacetTemplateProvider>().As<IFacetTemplateProvider>().InstancePerRequest();

            // Catalog.
            builder.RegisterType<CatalogSearchService>().As<ICatalogSearchService>().As<IXmlSitemapPublisher>().InstancePerRequest();
            builder.RegisterType<LinqCatalogSearchService>().Named<ICatalogSearchService>("linq").InstancePerRequest();
            builder.RegisterType<CatalogSearchQueryFactory>().As<ICatalogSearchQueryFactory>().InstancePerRequest();
            builder.RegisterType<CatalogSearchQueryAliasMapper>().As<ICatalogSearchQueryAliasMapper>().InstancePerRequest();

            // Forum.
            builder.RegisterType<ForumSearchService>().As<IForumSearchService>().InstancePerRequest();
            builder.RegisterType<LinqForumSearchService>().Named<IForumSearchService>("linq").InstancePerRequest();
            builder.RegisterType<ForumSearchQueryFactory>().As<IForumSearchQueryFactory>().InstancePerRequest();
            builder.RegisterType<ForumSearchQueryAliasMapper>().As<IForumSearchQueryAliasMapper>().InstancePerRequest();
        }
    }

    internal class EventModule : Module
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
            builder.RegisterType<DefaultMessageBus>().As<IMessageBus>().SingleInstance();

            builder.RegisterType<EventPublisher>().As<IEventPublisher>().SingleInstance();
            builder.RegisterType<ConsumerRegistry>().As<IConsumerRegistry>().SingleInstance();
            builder.RegisterType<ConsumerResolver>().As<IConsumerResolver>().SingleInstance();
            builder.RegisterType<ConsumerInvoker>().As<IConsumerInvoker>().SingleInstance();

            var consumerTypes = _typeFinder.FindClassesOfType(typeof(IConsumer));
            foreach (var type in consumerTypes)
            {
                var registration = builder
                    .RegisterType(type)
                    .As<IConsumer>()
                    .Keyed<IConsumer>(type)
                    .InstancePerRequest();

                var pluginDescriptor = _pluginFinder.GetPluginDescriptorByAssembly(type.Assembly);
                var isActive = PluginManager.IsActivePluginAssembly(type.Assembly);

                registration.WithMetadata<EventConsumerMetadata>(m =>
                {
                    m.For(em => em.IsActive, isActive);
                    m.For(em => em.ContainerType, type);
                    m.For(em => em.PluginDescriptor, pluginDescriptor);
                });
            }
        }
    }

    internal class MessagingModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // Templating
            builder.RegisterType<LiquidTemplateEngine>().As<ITemplateEngine>().SingleInstance();
            builder.RegisterType<DefaultTemplateManager>().As<ITemplateManager>().SingleInstance();

            builder.RegisterType<MessageModelProvider>().As<IMessageModelProvider>().InstancePerRequest();
            builder.RegisterType<MessageFactory>().As<IMessageFactory>().InstancePerRequest();

            builder.RegisterType<MessageTemplateService>().As<IMessageTemplateService>().InstancePerRequest();
            builder.RegisterType<QueuedEmailService>().As<IQueuedEmailService>().InstancePerRequest();
            builder.RegisterType<NewsLetterSubscriptionService>().As<INewsLetterSubscriptionService>().InstancePerRequest();
            builder.RegisterType<CampaignService>().As<ICampaignService>().InstancePerRequest();
            builder.RegisterType<EmailAccountService>().As<IEmailAccountService>().InstancePerRequest();
            builder.RegisterType<DefaultEmailSender>().As<IEmailSender>().InstancePerRequest();
            builder.RegisterType<LocalAsyncState>().As<IAsyncState>().SingleInstance();
        }
    }

    internal class WebModule : Module
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

            // Filter provider
            builder.RegisterFilterProvider();

            // Model binding
            builder.RegisterModelBinders(foundAssemblies);
            builder.RegisterModelBinderProvider();

            var pageHelperRegistration = builder.RegisterType<WebViewPageHelper>().InstancePerRequest();

            // Global exception handling
            if (DataSettings.DatabaseIsInstalled())
            {
                pageHelperRegistration.PropertiesAutowired(PropertyWiringOptions.None);

                builder.RegisterType<HandleExceptionFilter>()
                    .AsExceptionFilterFor<SmartController>(-100)
                    .AsActionFilterFor<SmartController>(int.MaxValue)
                    .InstancePerRequest();

                builder.RegisterType<CookieConsentFilter>()
                    .AsActionFilterFor<PublicControllerBase>()
                    .InstancePerRequest();
            }
        }

        static HttpContextBase HttpContextBaseFactory(IComponentContext ctx)
        {
            if (IsRequestValid())
            {
                return new HttpContextWrapper(HttpContext.Current);
            }

            // register FakeHttpContext when HttpContext is not available
            return new FakeHttpContext("~/");
        }

        static bool IsRequestValid()
        {
            if (HttpContext.Current == null)
            {
                return false;
            }

            return HttpContext.Current.SafeGetHttpRequest() != null;
        }
    }

    internal class WebApiModule : Module
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

        protected override void AttachToComponentRegistration(IComponentRegistryBuilder componentRegistry, IComponentRegistration registration)
        {
            if (!DataSettings.DatabaseIsInstalled())
                return;

            var baseType = typeof(WebApiEntityController<,>);
            var type = registration.Activator.LimitType;

            if (!type.IsSubClass(baseType, out var implementingType))
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

    internal class UiModule : Module
    {
        private readonly ITypeFinder _typeFinder;

        public UiModule(ITypeFinder typeFinder)
        {
            _typeFinder = typeFinder;
        }

        protected override void Load(ContainerBuilder builder)
        {
            // register theming services
            builder.Register(x => new DefaultThemeRegistry(x.Resolve<IEventPublisher>(), x.Resolve<IApplicationEnvironment>(), null, null, true)).As<IThemeRegistry>().SingleInstance();
            builder.RegisterType<DefaultThemeFileResolver>().As<IThemeFileResolver>().SingleInstance();

            builder.RegisterType<ThemeContext>().As<IThemeContext>().InstancePerRequest();
            builder.RegisterType<ThemeVariablesService>().As<IThemeVariablesService>().InstancePerRequest();

            // register UI component renderers
            builder.RegisterType<TabStripRenderer>().As<ComponentRenderer<TabStrip>>();
            builder.RegisterType<PagerRenderer>().As<ComponentRenderer<Pager>>();
            builder.RegisterType<WindowRenderer>().As<ComponentRenderer<Window>>();

            builder.RegisterType<WidgetProvider>().As<IWidgetProvider>().InstancePerRequest();
            builder.RegisterType<MenuPublisher>().As<IMenuPublisher>().InstancePerRequest();
            builder.RegisterType<DefaultBreadcrumb>().As<IBreadcrumb>().InstancePerRequest();
            builder.RegisterType<IconExplorer>().As<IIconExplorer>().SingleInstance();

            // Menus
            builder.RegisterType<MenuService>().As<IMenuService>().InstancePerRequest();

            var menuResolverTypes = _typeFinder.FindClassesOfType<IMenuResolver>(ignoreInactivePlugins: true);
            foreach (var type in menuResolverTypes)
            {
                builder.RegisterType(type).As<IMenuResolver>().PropertiesAutowired(PropertyWiringOptions.None).InstancePerRequest();
            }

            builder.RegisterType<DatabaseMenu>().Named<IMenu>("database").InstancePerDependency();

            var menuTypes = _typeFinder.FindClassesOfType<IMenu>(ignoreInactivePlugins: true);
            foreach (var type in menuTypes)
            {
                builder.RegisterType(type).As<IMenu>().PropertiesAutowired(PropertyWiringOptions.None).InstancePerRequest();
            }

            var menuItemProviderTypes = _typeFinder.FindClassesOfType<IMenuItemProvider>(ignoreInactivePlugins: true);
            foreach (var type in menuItemProviderTypes)
            {
                var attribute = type.GetAttribute<MenuItemProviderAttribute>(false);
                var registration = builder.RegisterType(type).As<IMenuItemProvider>().PropertiesAutowired(PropertyWiringOptions.None).InstancePerRequest();
                registration.WithMetadata<MenuItemProviderMetadata>(m =>
                {
                    m.For(em => em.ProviderName, attribute.ProviderName);
                    m.For(em => em.AppendsMultipleItems, attribute.AppendsMultipleItems);
                });
            }

            if (DataSettings.DatabaseIsInstalled())
            {
                // We have to register two classes, otherwise the filters would be called twice.
                builder.RegisterType<MenuActionFilter>().AsActionFilterFor<SmartController>(0);
                builder.RegisterType<MenuResultFilter>().AsResultFilterFor<SmartController>(0);
            }
        }
    }

    internal class IOModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<LocalFileSystem>().As<IFileSystem>().SingleInstance();
            builder.RegisterType<MediaFileSystem>().As<IMediaFileSystem>().SingleInstance();

            // Register IFileSystem twice, this time explicitly named.
            // We may need this later in decorator classes as a kind of fallback.
            builder.RegisterType<LocalFileSystem>().Named<IFileSystem>("local").SingleInstance();
            builder.RegisterType<MediaFileSystem>().Named<IMediaFileSystem>("local").SingleInstance();

            builder.RegisterType<DefaultVirtualPathProvider>().As<IVirtualPathProvider>().SingleInstance();
            builder.RegisterType<LockFileManager>().As<ILockFileManager>().SingleInstance();
        }
    }

    internal class PackagingModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<PackageBuilder>().As<IPackageBuilder>().InstancePerRequest();
            builder.RegisterType<PackageInstaller>().As<IPackageInstaller>().InstancePerRequest();
            builder.RegisterType<PackageManager>().As<IPackageManager>().InstancePerRequest();
            builder.RegisterType<FolderUpdater>().As<IFolderUpdater>().InstancePerRequest();
        }
    }

    internal class ProvidersModule : Module
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
                var isHidden = GetIsHidden(type);
                var exportFeature = GetExportFeature(type);

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
                    m.For(em => em.IsHidden, isHidden);
                    m.For(em => em.ExportFeatures, exportFeature);
                });

                // Register specific provider type.
                RegisterAsSpecificProvider<ITaxProvider>(type, systemName, registration);
                RegisterAsSpecificProvider<IExchangeRateProvider>(type, systemName, registration);
                RegisterAsSpecificProvider<IShippingRateComputationMethod>(type, systemName, registration);
                RegisterAsSpecificProvider<IWidget>(type, systemName, registration);
                RegisterAsSpecificProvider<IExternalAuthenticationMethod>(type, systemName, registration);
                RegisterAsSpecificProvider<IPaymentMethod>(type, systemName, registration);
                RegisterAsSpecificProvider<IExportProvider>(type, systemName, registration);
                RegisterAsSpecificProvider<IOutputCacheProvider>(type, systemName, registration);
                RegisterAsSpecificProvider<IMediaStorageProvider>(type, systemName, registration);
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

        private bool GetIsHidden(Type type)
        {
            var attr = type.GetAttribute<IsHiddenAttribute>(false);
            if (attr != null)
            {
                return attr.IsHidden;
            }

            return false;
        }

        private ExportFeatures GetExportFeature(Type type)
        {
            var attr = type.GetAttribute<ExportFeaturesAttribute>(false);

            if (attr != null)
            {
                return attr.Features;
            }

            return ExportFeatures.None;
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

            return new string[] { };
        }

        private string ProviderTypeToKnownGroupName(Type implType)
        {
            if (typeof(ITaxProvider).IsAssignableFrom(implType))
            {
                return "Tax";
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
            else if (typeof(IExportProvider).IsAssignableFrom(implType))
            {
                return "Exporting";
            }
            else if (typeof(IOutputCacheProvider).IsAssignableFrom(implType))
            {
                return "OutputCache";
            }

            return null;
        }

        #endregion
    }

    internal class TasksModule : Module
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

    internal class DataExchangeModule : Module
    {
        private readonly ITypeFinder _typeFinder;

        public DataExchangeModule(ITypeFinder typeFinder)
        {
            _typeFinder = typeFinder;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ExportProfileService>().As<IExportProfileService>().InstancePerRequest();
            builder.RegisterType<ImportProfileService>().As<IImportProfileService>().InstancePerRequest();

            if (!DataSettings.DatabaseIsInstalled())
                return;

            builder.RegisterType<DataExporter>().As<IDataExporter>().InstancePerRequest();
            builder.RegisterType<DataImporter>().As<IDataImporter>().InstancePerRequest();

            // IEntityImporter implementations
            builder.RegisterType<ProductImporter>().Keyed<IEntityImporter>(ImportEntityType.Product).InstancePerRequest();
            builder.RegisterType<CategoryImporter>().Keyed<IEntityImporter>(ImportEntityType.Category).InstancePerRequest();
            builder.RegisterType<CustomerImporter>().Keyed<IEntityImporter>(ImportEntityType.Customer).InstancePerRequest();
            builder.RegisterType<NewsLetterSubscriptionImporter>().Keyed<IEntityImporter>(ImportEntityType.NewsLetterSubscription).InstancePerRequest();

            // Register resolving delegate
            builder.Register<Func<ImportEntityType, IEntityImporter>>(c =>
            {
                var cc = c.Resolve<IComponentContext>();
                return keyed => cc.ResolveKeyed<IEntityImporter>(keyed);
            });
        }
    }

    internal class RuleModule : SmartStore.Rules.RuleModule
    {
        private readonly ITypeFinder _typeFinder;

        public RuleModule(ITypeFinder typeFinder)
        {
            _typeFinder = typeFinder;
        }

        protected override void Load(ContainerBuilder builder)
        {
            var cartRuleTypes = _typeFinder.FindClassesOfType<IRule>(ignoreInactivePlugins: true).ToList();
            foreach (var ruleType in cartRuleTypes)
            {
                builder.RegisterType(ruleType).Keyed<IRule>(ruleType).InstancePerRequest();
            }

            builder.RegisterType<CartRuleProvider>()
                .As<ICartRuleProvider>()
                .Keyed<IRuleProvider>(RuleScope.Cart)
                .InstancePerRequest();

            builder.RegisterType<TargetGroupService>()
                .As<ITargetGroupService>()
                .Keyed<IRuleProvider>(RuleScope.Customer)
                .InstancePerRequest();

            builder.RegisterType<ProductRuleProvider>()
                .As<IProductRuleProvider>()
                .Keyed<IRuleProvider>(RuleScope.Product)
                .InstancePerRequest();

            builder.RegisterType<DefaultRuleOptionsProvider>().As<IRuleOptionsProvider>().InstancePerRequest();

            base.Load(builder);
        }
    }

    #endregion

    #region Sources

    class SettingsSource : IRegistrationSource
    {
        static readonly MethodInfo BuildMethod = typeof(SettingsSource).GetMethod(
            "BuildRegistration",
            BindingFlags.Static | BindingFlags.NonPublic);

        public IEnumerable<IComponentRegistration> RegistrationsFor(Service service, Func<Service, IEnumerable<IComponentRegistration>> registrations)
        {
            if (service is TypedService ts && typeof(ISettings).IsAssignableFrom(ts.ServiceType))
            {
                var buildMethod = BuildMethod.MakeGenericMethod(ts.ServiceType);
                yield return (IComponentRegistration)buildMethod.Invoke(null, null);
            }
        }

        [SuppressMessage("CodeQuality", "IDE0051", Justification = "Called by reflection")]
        static IComponentRegistration BuildRegistration<TSettings>() where TSettings : ISettings, new()
        {
            return RegistrationBuilder
                .ForDelegate((c, p) =>
                {
                    int currentStoreId = 0;
                    if (EngineContext.Current.IsFullyInitialized)
                    {
                        try
                        {
                            if (c.TryResolve(out IStoreContext storeContext))
                            {
                                currentStoreId = storeContext.CurrentStore.Id;
                            }
                        }
                        catch { }
                    }

                    try
                    {
                        return c.Resolve<ISettingService>().LoadSetting<TSettings>(currentStoreId);
                    }
                    catch
                    {
                        // Unit tests & tooling
                        return new TSettings();
                    }
                })
                .InstancePerRequest()
                .CreateRegistration();
        }

        public bool IsAdapterForIndividualComponents => false;
    }

    class WorkSource : IRegistrationSource
    {
        static readonly MethodInfo CreateMetaRegistrationMethod = typeof(WorkSource).GetMethod(
            "CreateMetaRegistration", BindingFlags.Static | BindingFlags.NonPublic);

        private static bool IsClosingTypeOf(Type type, Type openGenericType)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == openGenericType;
        }

        public IEnumerable<IComponentRegistration> RegistrationsFor(Service service, Func<Service, IEnumerable<IComponentRegistration>> registrationAccessor)
        {
            if (!(service is IServiceWithType swt) || !IsClosingTypeOf(swt.ServiceType, typeof(Work<>)))
                return Enumerable.Empty<IComponentRegistration>();

            var valueType = swt.ServiceType.GetGenericArguments()[0];

            var valueService = swt.ChangeType(valueType);

            var registrationCreator = CreateMetaRegistrationMethod.MakeGenericMethod(valueType);

            return registrationAccessor(valueService)
                .Select(v => registrationCreator.Invoke(null, new object[] { service, v }))
                .Cast<IComponentRegistration>();
        }

        public bool IsAdapterForIndividualComponents => true;

        [SuppressMessage("CodeQuality", "IDE0051", Justification = "Called by reflection")]
        static IComponentRegistration CreateMetaRegistration<T>(Service providedService, IComponentRegistration valueRegistration) where T : class
        {
            var rb = RegistrationBuilder.ForDelegate(
                (c, p) =>
                {
                    var accessor = c.Resolve<ILifetimeScopeAccessor>();
                    return new Work<T>(w =>
                    {
                        var scope = accessor.GetLifetimeScope(null);
                        if (scope == null)
                            return default(T);

                        var workValues = scope.Resolve<WorkValues<T>>();

                        if (!workValues.Values.TryGetValue(w, out T value))
                        {
                            var request = new ResolveRequest(providedService, valueRegistration, p);
                            value = (T)workValues.ComponentContext.ResolveComponent(request);
                            workValues.Values[w] = value;
                        }

                        return value;

                        ////T value = default(T); // accessor.GetLifetimeScope(null).Resolve<T>();
                        //return accessor.GetLifetimeScope(null).Resolve<T>();
                    });
                })
                .As(providedService)
                .Targeting(valueRegistration, false)
                .SingleInstance();

            return rb.CreateRegistration();
        }
    }

    class WorkValues<T> where T : class
    {
        public WorkValues(IComponentContext componentContext)
        {
            ComponentContext = componentContext;
            Values = new Dictionary<Work<T>, T>();
        }

        public IComponentContext ComponentContext { get; private set; }
        public IDictionary<Work<T>, T> Values { get; private set; }
    }

    #endregion

}
