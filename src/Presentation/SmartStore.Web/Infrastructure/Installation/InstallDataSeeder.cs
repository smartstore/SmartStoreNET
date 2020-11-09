using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using SmartStore.Core;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Configuration;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Security;
using SmartStore.Core.Domain.Seo;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Domain.Tax;
using SmartStore.Core.Domain.Themes;
using SmartStore.Core.Events;
using SmartStore.Core.Logging;
using SmartStore.Data;
using SmartStore.Data.Setup;
using SmartStore.Data.Utilities;
using SmartStore.Services.Common;
using SmartStore.Services.Configuration;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
using SmartStore.Services.Media.Storage;
using SmartStore.Services.Security;
using SmartStore.Services.Seo;
using SmartStore.Services.Stores;
using SmartStore.Utilities;
using SmartStore.Web.Framework;

namespace SmartStore.Web.Infrastructure.Installation
{
    public partial class InstallDataSeeder : IDataSeeder<SmartObjectContext>
    {
        #region Fields & Constants

        private ILogger _logger;
        private SmartObjectContext _ctx;
        private SeedDataConfiguration _config;
        private InvariantSeedData _data;
        private ISettingService _settingService;
        private IGenericAttributeService _gaService;
        private ILocalizationService _locService;
        private IUrlRecordService _urlRecordService;
        private int _defaultStoreId;

        #endregion Fields & Constants

        #region Ctor

        public InstallDataSeeder(SeedDataConfiguration configuration, ILogger logger)
        {
            Guard.NotNull(configuration, nameof(configuration));
            Guard.NotNull(configuration.Language, "Language");
            Guard.NotNull(configuration.Data, "SeedData");

            _config = configuration;
            _data = configuration.Data;
            _logger = logger;
        }

        #endregion Ctor

        #region Populate

        private void PopulateStores()
        {
            SaveRange(_data.Stores());
            _defaultStoreId = _data.Stores().First().Id;
        }

        private void PopulateTaxCategories()
        {
            SaveRange(_data.TaxCategories());

            // add tax rates to fixed rate provider
            var taxCategories = _ctx.Set<TaxCategory>().ToList();
            int i = 0;
            var taxIds = taxCategories.OrderBy(x => x.Id).Select(x => x.Id).ToList();
            foreach (var id in taxIds)
            {
                decimal rate = 0;
                if (_data.FixedTaxRates.Any() && _data.FixedTaxRates.Length > i)
                {
                    rate = _data.FixedTaxRates[i];
                }
                i++;
                this.SettingService.SetSetting(string.Format("Tax.TaxProvider.FixedRate.TaxCategoryId{0}", id), rate);
            }

            _ctx.SaveChanges();
        }

        private void PopulateLanguage(Language primaryLanguage)
        {
            primaryLanguage.Published = true;
            Save(primaryLanguage);
        }

        private void PopulateLocaleResources()
        {
            // Default primary language
            var language = _ctx.Set<Language>().First();

            var locPath = CommonHelper.MapPath("~/App_Data/Localization/App/" + language.LanguageCulture);
            if (!Directory.Exists(locPath))
            {
                // Fallback to neutral language folder (de, en etc.)
                locPath = CommonHelper.MapPath("~/App_Data/Localization/App/" + language.UniqueSeoCode);
            }

            var localizationService = this.LocalizationService;

            // Perf
            _ctx.DetachAll(false);

            // save resources
            foreach (var filePath in Directory.EnumerateFiles(locPath, "*.smres.xml", SearchOption.TopDirectoryOnly))
            {
                var doc = new XmlDocument();
                doc.Load(filePath);

                doc = localizationService.FlattenResourceFile(doc);

                // now we have a parsed XML file (the same structure as exported language packs)
                // let's save resources
                localizationService.ImportResourcesFromXml(language, doc);

                // no need to call SaveChanges() here, as the above call makes it
                // already without AutoDetectChanges(), so it's fast.

                // Perf
                _ctx.DetachAll(false);
            }

            MigratorUtils.ExecutePendingResourceMigrations(locPath, _ctx);
        }

        private void PopulateCurrencies()
        {
            SaveRange(_data.Currencies().Where(x => x != null));
        }

        private void PopulateCountriesAndStates()
        {
            SaveRange(_data.Countries().Where(x => x != null));
            DataMigrator.ImportAddressFormats(_ctx);
        }

        private void PopulateShippingMethods()
        {
            SaveRange(_data.ShippingMethods(_config.SeedSampleData).Where(x => x != null));
        }

        private void PopulateCustomersAndUsers(string defaultUserEmail, string defaultUserPassword)
        {
            var customerRoles = _data.CustomerRoles(_config.SeedSampleData);
            SaveRange(customerRoles.Where(x => x != null));

            //admin user
            var adminUser = new Customer
            {
                CustomerGuid = Guid.NewGuid(),
                Email = defaultUserEmail,
                Username = defaultUserEmail,
                Password = defaultUserPassword,
                PasswordFormat = PasswordFormat.Clear,
                PasswordSalt = "",
                Active = true,
                CreatedOnUtc = DateTime.UtcNow,
                LastActivityDateUtc = DateTime.UtcNow,
            };

            var adminAddress = _data.AdminAddress();

            adminUser.Addresses.Add(adminAddress);
            adminUser.BillingAddress = adminAddress;
            adminUser.ShippingAddress = adminAddress;

            var adminRole = customerRoles.First(x => x.SystemName == SystemCustomerRoleNames.Administrators);
            var forumRole = customerRoles.First(x => x.SystemName == SystemCustomerRoleNames.ForumModerators);
            var registeredRole = customerRoles.First(x => x.SystemName == SystemCustomerRoleNames.Registered);

            adminUser.CustomerRoleMappings.Add(new CustomerRoleMapping { CustomerId = adminUser.Id, CustomerRoleId = adminRole.Id });
            adminUser.CustomerRoleMappings.Add(new CustomerRoleMapping { CustomerId = adminUser.Id, CustomerRoleId = forumRole.Id });
            adminUser.CustomerRoleMappings.Add(new CustomerRoleMapping { CustomerId = adminUser.Id, CustomerRoleId = registeredRole.Id });
            Save(adminUser);

            // Set default customer name
            var firstAddress = adminUser.Addresses.FirstOrDefault();
            GenericAttributeService.InsertAttribute(new GenericAttribute
            {
                EntityId = adminUser.Id,
                Key = "FirstName",
                KeyGroup = "Customer",
                Value = firstAddress.FirstName,
                StoreId = 0
            });
            GenericAttributeService.InsertAttribute(new GenericAttribute
            {
                EntityId = adminUser.Id,
                Key = "LastName",
                KeyGroup = "Customer",
                Value = firstAddress.LastName,
                StoreId = 0
            });
            _ctx.SaveChanges();

            // Built-in user for search engines (crawlers)
            var guestRole = customerRoles.FirstOrDefault(x => x.SystemName == SystemCustomerRoleNames.Guests);

            var customer = _data.SearchEngineUser();
            customer.CustomerRoleMappings.Add(new CustomerRoleMapping { CustomerId = customer.Id, CustomerRoleId = guestRole.Id });
            Save(customer);

            // Built-in user for background tasks
            customer = _data.BackgroundTaskUser();
            customer.CustomerRoleMappings.Add(new CustomerRoleMapping { CustomerId = customer.Id, CustomerRoleId = guestRole.Id });
            Save(customer);

            // Built-in user for the PDF converter
            customer = _data.PdfConverterUser();
            customer.CustomerRoleMappings.Add(new CustomerRoleMapping { CustomerId = customer.Id, CustomerRoleId = guestRole.Id });
            Save(customer);
        }

        private void HashDefaultCustomerPassword(string defaultUserEmail, string defaultUserPassword)
        {
            var encryptionService = new EncryptionService(new SecuritySettings());
            var saltKey = encryptionService.CreateSaltKey(5);
            var adminUser = _ctx.Set<Customer>().FirstOrDefault(x => x.Email == _config.DefaultUserName);

            adminUser.PasswordSalt = saltKey;
            adminUser.PasswordFormat = PasswordFormat.Hashed;
            adminUser.Password = encryptionService.CreatePasswordHash(defaultUserPassword, saltKey, new CustomerSettings().HashedPasswordFormat);

            _ctx.SaveChanges();
        }

        private void PopulateSettings()
        {
            var method = typeof(ISettingService).GetMethods().FirstOrDefault(x =>
            {
                if (x.Name == "SaveSetting")
                {
                    var parameters = x.GetParameters();
                    return parameters[0].ParameterType.Name == "T" && parameters[1].ParameterType.Equals(typeof(int));
                }
                return false;
            });

            var settings = _data.Settings();
            foreach (var setting in settings)
            {
                Type settingType = setting.GetType();
                Type settingServiceType = typeof(ISettingService);

                var settingService = this.SettingService;
                if (settingService != null)
                {
                    var genericMethod = method.MakeGenericMethod(settingType);
                    int storeId = settingType.Equals(typeof(ThemeSettings)) ? _defaultStoreId : 0;

                    genericMethod.Invoke(settingService, new object[] { setting, storeId });
                }
            }

            _ctx.SaveChanges();
        }

        private void PopulateMessageTemplates()
        {
            var converter = new MessageTemplateConverter(_ctx);
            converter.ImportAll(_config.Language);
        }

        private void PopulateBlogPosts()
        {
            var converter = new BlogPostConverter(_ctx);
            var blogPosts = converter.ImportAll(_config.Language);            
            PopulateUrlRecordsFor(blogPosts);
        }

        private void PopulateNewsItems()
        {
            var converter = new NewsItemConverter(_ctx);
            var newsItems = converter.ImportAll(_config.Language);
            PopulateUrlRecordsFor(newsItems);
        }

        private void PopulateCategories()
        {
            var categoriesFirstLevel = _data.CategoriesFirstLevel();
            SaveRange(categoriesFirstLevel);
            PopulateUrlRecordsFor(categoriesFirstLevel);

            var categoriesSecondLevel = _data.CategoriesSecondLevel();
            SaveRange(categoriesSecondLevel);
            PopulateUrlRecordsFor(categoriesSecondLevel);
        }

        private void PopulateManufacturers()
        {
            var manufacturers = _data.Manufacturers();
            SaveRange(manufacturers);
            PopulateUrlRecordsFor(manufacturers);
        }

        private void PopulateProducts()
        {
            var products = _data.Products();
            SaveRange(products);

            _data.AddDownloads(products);

            // Fix MainPictureId
            DataMigrator.FixProductMainPictureIds(_ctx);

            PopulateUrlRecordsFor(products);

            _data.AssignGroupedProducts(products);
        }

        private void PopulateManufacturerTemplates()
        {
            var manufacturerTemplates = new List<ManufacturerTemplate>
                                {
                                    new ManufacturerTemplate
                                        {
                                            Name = "Products in Grid or Lines",
                                            ViewPath = "ManufacturerTemplate.ProductsInGridOrLines",
                                            DisplayOrder = 1
                                        },
                                };
            SaveRange(_data.ManufacturerTemplates());
        }

        private void PopulateTopics()
        {
            var topics = _data.Topics();
            SaveRange(topics);
            PopulateUrlRecordsFor(topics);
        }

        private void PopulateMenus()
        {
            DataMigrator.CreateSystemMenus(_ctx);
        }

        private void MoveMedia()
        {
            if (!_config.StoreMediaInDB)
            {
                // All pictures have initially been stored in the DB. Move the binaries to disk.
                var fileSystemStorageProvider = new FileSystemMediaStorageProvider(new MediaFileSystem());
                var mediaStorages = _ctx.Set<MediaStorage>();

                using (var scope = new DbContextScope(ctx: _ctx, autoDetectChanges: true, autoCommit: false))
                {
                    var mediaFiles = _ctx.Set<MediaFile>()
                        .Expand(x => x.MediaStorage)
                        .Where(x => x.MediaStorageId != null)
                        .ToList();

                    foreach (var mediaFile in mediaFiles)
                    {
                        if (mediaFile.MediaStorage?.Data?.LongLength > 0)
                        {
                            fileSystemStorageProvider.Save(mediaFile, MediaStorageItem.FromStream(mediaFile.MediaStorage.Data.ToStream()));
                            mediaFile.MediaStorageId = null;
                            mediaFile.MediaStorage = null;
                        }
                    }

                    scope.Commit();
                }
            }
        }

        #endregion

        #region Properties

        protected SeedDataConfiguration Configuration => _config;

        protected SmartObjectContext DataContext => _ctx;


        protected ISettingService SettingService
        {
            get
            {
                if (_settingService == null)
                {
                    var rs = new EfRepository<Setting>(_ctx);
                    rs.AutoCommitEnabled = false;

                    _settingService = new SettingService(NullCache.Instance, rs);
                }

                return _settingService;
            }
        }

        protected IGenericAttributeService GenericAttributeService
        {
            get
            {
                if (_gaService == null)
                {
                    var rs = new EfRepository<GenericAttribute>(_ctx);
                    rs.AutoCommitEnabled = false;

                    var rsOrder = new EfRepository<Order>(_ctx);
                    rs.AutoCommitEnabled = false;

                    _gaService = new GenericAttributeService(rs, NullEventPublisher.Instance, rsOrder);
                }

                return _gaService;
            }
        }

        protected ILocalizationService LocalizationService
        {
            get
            {
                if (_locService == null)
                {
                    var rsLanguage = new EfRepository<Language>(_ctx);
                    rsLanguage.AutoCommitEnabled = false;

                    var rsResources = new EfRepository<LocaleStringResource>(_ctx);
                    rsResources.AutoCommitEnabled = false;

                    var rsStore = new EfRepository<Store>(_ctx);
                    rsStore.AutoCommitEnabled = false;

                    var storeMappingService = new StoreMappingService(NullCache.Instance, null, null, null);
                    var storeService = new StoreService(rsStore);
                    var storeContext = new WebStoreContext(new Lazy<IRepository<Store>>(() => rsStore), null, NullCache.Instance);

                    var locSettings = new LocalizationSettings();

                    var languageService = new LanguageService(
                        NullRequestCache.Instance,
                        NullCache.Instance,
                        rsLanguage,
                        this.SettingService,
                        locSettings,
                        NullEventPublisher.Instance,
                        storeMappingService,
                        storeService,
                        storeContext);

                    _locService = new LocalizationService(
                        NullCache.Instance,
                        NullLogger.Instance,
                        null /* IWorkContext: not needed during install */,
                        rsResources,
                        languageService);
                }

                return _locService;
            }
        }

        #endregion Properties

        #region Methods

        public virtual void Seed(SmartObjectContext context)
        {
            Guard.NotNull(context, nameof(context));

            _ctx = context;
            _data.Initialize(_ctx);

            _ctx.Configuration.AutoDetectChangesEnabled = false;
            _ctx.Configuration.ValidateOnSaveEnabled = false;
            _ctx.HooksEnabled = false;

            _config.ProgressMessageCallback("Progress.CreatingRequiredData");

            // special mandatory (non-visible) settings
            _ctx.MigrateSettings(x =>
            {
                x.Add("Media.Storage.Provider", _config.StoreMediaInDB ? DatabaseMediaStorageProvider.SystemName : FileSystemMediaStorageProvider.SystemName);
            });

            Populate("PopulatePictures", _data.Pictures());
            Populate("PopulateCurrencies", PopulateCurrencies);
            Populate("PopulateStores", PopulateStores);
            Populate("InstallLanguages", () => PopulateLanguage(_config.Language));
            Populate("PopulateMeasureDimensions", _data.MeasureDimensions());
            Populate("PopulateMeasureWeights", _data.MeasureWeights());
            Populate("PopulateTaxCategories", PopulateTaxCategories);
            Populate("PopulateCountriesAndStates", PopulateCountriesAndStates);
            Populate("PopulateShippingMethods", PopulateShippingMethods);
            Populate("PopulateDeliveryTimes", _data.DeliveryTimes());
            Populate("PopulateQuantityUnits", _data.QuantityUnits());
            Populate("PopulateCustomersAndUsers", () => PopulateCustomersAndUsers(_config.DefaultUserName, _config.DefaultUserPassword));
            Populate("PopulateEmailAccounts", _data.EmailAccounts());
            Populate("PopulateMessageTemplates", PopulateMessageTemplates);
            Populate("PopulateTopics", PopulateTopics);
            Populate("PopulateSettings", PopulateSettings);
            Populate("PopulateActivityLogTypes", _data.ActivityLogTypes());
            Populate("PopulateCustomersAndUsers", () => HashDefaultCustomerPassword(_config.DefaultUserName, _config.DefaultUserPassword));
            Populate("PopulateProductTemplates", _data.ProductTemplates());
            Populate("PopulateCategoryTemplates", _data.CategoryTemplates());
            Populate("PopulateManufacturerTemplates", PopulateManufacturerTemplates);
            Populate("PopulateScheduleTasks", _data.ScheduleTasks());
            Populate("PopulateLocaleResources", PopulateLocaleResources);
            Populate("PopulateMenus", PopulateMenus);

            if (_config.SeedSampleData)
            {
                _logger.Info("Seeding sample data");

                _config.ProgressMessageCallback("Progress.CreatingSampleData");

                Populate("PopulateSpecificationAttributes", _data.SpecificationAttributes());
                Populate("PopulateProductAttributes", _data.ProductAttributes());
                Populate("PopulateProductAttributeOptionsSets", _data.ProductAttributeOptionsSets());
                Populate("PopulateProductAttributeOptions", _data.ProductAttributeOptions());
                Populate("PopulateCampaigns", _data.Campaigns());
                Populate("PopulateRuleSets", _data.RuleSets());
                Populate("PopulateDiscounts", _data.Discounts());
                Populate("PopulateCategories", PopulateCategories);
                Populate("PopulateManufacturers", PopulateManufacturers);
                Populate("PopulateProducts", PopulateProducts);
                Populate("PopulateProductBundleItems", _data.ProductBundleItems());
                Populate("PopulateProductVariantAttributes", _data.ProductVariantAttributes());
                Populate("ProductVariantAttributeCombinations", _data.ProductVariantAttributeCombinations());
                Populate("PopulateProductTags", _data.ProductTags());
                Populate("PopulateForumsGroups", _data.ForumGroups());
                Populate("PopulateForums", _data.Forums());
                Populate("PopulateBlogPosts", PopulateBlogPosts);
                Populate("PopulateNews", PopulateNewsItems);
                Populate("PopulatePolls", _data.Polls());
                Populate("FinalizeSamples", () => _data.FinalizeSamples());
            }

            Populate("MovePictures", MoveMedia);

            // Perf
            _ctx.DetachAll();
        }

        public bool RollbackOnFailure => false;

        #endregion

        #region Utils

        private void PopulateUrlRecordsFor<T>(IEnumerable<T> entities) where T : BaseEntity, ISlugSupported, new()
        {
            foreach (var entity in entities)
            {
                var ur = _data.CreateUrlRecordFor(entity);
                if (ur != null)
                {
                    ur.Slug = ValidateSeName(entity, ur.Slug);
                    Save(ur);
                }
            }
        }

        private string ValidateSeName<TEntity>(TEntity entity, string name)
            where TEntity : BaseEntity, ISlugSupported
        {
            var seoSettings = new SeoSettings { LoadAllUrlAliasesOnStartup = false };
            var perfSettings = new PerformanceSettings();

            if (_urlRecordService == null)
            {
                _urlRecordService = new UrlRecordService(
                    NullCache.Instance,
                    new EfRepository<UrlRecord>(_ctx) { AutoCommitEnabled = false },
                    seoSettings,
                    perfSettings);
            }

            return entity.ValidateSeName<TEntity>("", name, true, _urlRecordService, seoSettings);
        }

        private void Populate<TEntity>(string stage, IEnumerable<TEntity> entities)
            where TEntity : BaseEntity
        {
            try
            {
                _logger.DebugFormat("Populate: {0}", stage);
                SaveRange(entities);
            }
            catch (Exception ex)
            {
                var ex2 = new SeedDataException(stage, ex);
                _logger.Error(ex2);
                throw ex2;
            }
        }

        private void Populate(string stage, Action populateAction)
        {
            try
            {
                _logger.DebugFormat("Populate: {0}", stage);
                populateAction();
            }
            catch (Exception ex)
            {
                var ex2 = new SeedDataException(stage, ex);
                _logger.Error(ex2);
                throw ex2;
            }
        }

        private void Save<TEntity>(TEntity entity) where TEntity : BaseEntity
        {
            _ctx.Set<TEntity>().Add(entity);
            _ctx.SaveChanges();
        }

        private void SaveRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : BaseEntity
        {
            _ctx.Set<TEntity>().AddRange(entities);
            _ctx.SaveChanges();
        }

        #endregion

    }

}
