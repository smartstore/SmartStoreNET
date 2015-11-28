using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using SmartStore.Core;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Domain.Security;
using SmartStore.Core.Domain.Tax;
using SmartStore.Services.Configuration;
using SmartStore.Core.Domain.Seo;
using SmartStore.Core.Domain.Stores;
using SmartStore.Data;
using SmartStore.Core.Caching;
using SmartStore.Core.Domain.Themes;
using SmartStore.Utilities;
using SmartStore.Core.Domain.Configuration;
using SmartStore.Data.Setup;
using SmartStore.Core.Events;
using SmartStore.Services.Common;
using SmartStore.Services.Media;
using SmartStore.Core.Logging;
using SmartStore.Services.Localization;
using SmartStore.Services.Security;
using SmartStore.Services.Seo;
using System.Data.Entity.Migrations;
using SmartStore.Data.Migrations;
using SmartStore.Services.Stores;
using SmartStore.Core.Domain.Orders;
using SmartStore.Web.Framework;

namespace SmartStore.Web.Infrastructure.Installation
{
    public partial class InstallDataSeeder : IDataSeeder<SmartObjectContext>
    {
        #region Fields & Constants

		private SmartObjectContext _ctx;
        private SeedDataConfiguration _config;
        private InvariantSeedData _data;
		private ISettingService _settingService;
		private IGenericAttributeService _gaService;
		private IPictureService _pictureService;
		private ILocalizationService _locService;
		private IUrlRecordService _urlRecordService;
		private int _defaultStoreId;

        #endregion Fields & Constants

        #region Ctor

		public InstallDataSeeder(SeedDataConfiguration configuration)
        {
			Guard.ArgumentNotNull(() => configuration);

			Guard.ArgumentNotNull(configuration.Language, "Language");
			Guard.ArgumentNotNull(configuration.Data, "SeedData");

			_config = configuration;
			_data = configuration.Data;
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
            var language = _ctx.Set<Language>().Single();

            var locPath = CommonHelper.MapPath("~/App_Data/Localization/App/" + language.LanguageCulture);
            if (!System.IO.Directory.Exists(locPath))
            {
                // Fallback to neutral language folder (de, en etc.)
				locPath = CommonHelper.MapPath("~/App_Data/Localization/App/" + language.UniqueSeoCode);
            }

			var localizationService = this.LocalizationService;

			// save resources
			foreach (var filePath in System.IO.Directory.EnumerateFiles(locPath, "*.smres.xml", SearchOption.TopDirectoryOnly))
			{
				var doc = new XmlDocument();
				doc.Load(filePath);

				doc = localizationService.FlattenResourceFile(doc);

				// now we have a parsed XML file (the same structure as exported language packs)
				// let's save resources
				localizationService.ImportResourcesFromXml(language, doc);

				// no need to call SaveChanges() here, as the above call makes it
				// already without AutoDetectChanges(), so it's fast.
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
        }

		private void PopulateShippingMethods()
        {
			SaveRange(_data.ShippingMethods().Where(x => x != null));
        }

		private void PopulateCustomersAndUsers(string defaultUserEmail, string defaultUserPassword)
        {
            var customerRoles = _data.CustomerRoles();
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
            adminUser.CustomerRoles.Add(customerRoles.SingleOrDefault(x => x.SystemName == SystemCustomerRoleNames.Administrators));
            adminUser.CustomerRoles.Add(customerRoles.SingleOrDefault(x => x.SystemName == SystemCustomerRoleNames.ForumModerators));
            adminUser.CustomerRoles.Add(customerRoles.SingleOrDefault(x => x.SystemName == SystemCustomerRoleNames.Registered));
            Save(adminUser);

			// Set default customer name
			this.GenericAttributeService.SaveAttribute(adminUser, SystemCustomerAttributeNames.FirstName, adminUser.Addresses.FirstOrDefault().FirstName);
			this.GenericAttributeService.SaveAttribute(adminUser, SystemCustomerAttributeNames.LastName, adminUser.Addresses.FirstOrDefault().LastName);
			_ctx.SaveChanges();

			// Built-in user for search engines (crawlers)
            var customer = _data.SearchEngineUser();
            customer.CustomerRoles.Add(customerRoles.SingleOrDefault(x => x.SystemName == SystemCustomerRoleNames.Guests));
            Save(customer);

            // Built-in user for background tasks
            customer = _data.BackgroundTaskUser();
            customer.CustomerRoles.Add(customerRoles.SingleOrDefault(x => x.SystemName == SystemCustomerRoleNames.Guests));
            Save(customer);

			// Built-in user for the PDF converter
			customer = _data.PdfConverterUser();
			customer.CustomerRoles.Add(customerRoles.SingleOrDefault(x => x.SystemName == SystemCustomerRoleNames.Guests));
			Save(customer);
        }

		private void HashDefaultCustomerPassword(string defaultUserEmail, string defaultUserPassword)
        {
			var adminUser = _ctx.Set<Customer>().Where(x => x.Email == _config.DefaultUserName).Single();

			var encryptionService = new EncryptionService(new SecuritySettings());

			string saltKey = encryptionService.CreateSaltKey(5);
			adminUser.PasswordSalt = saltKey;
			adminUser.PasswordFormat = PasswordFormat.Hashed;
			adminUser.Password = encryptionService.CreatePasswordHash(defaultUserPassword, saltKey, new CustomerSettings().HashedPasswordFormat);

			SetModified(adminUser);
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
					int storeId = (settingType.Equals(typeof(ThemeSettings)) ? _defaultStoreId : 0);

					genericMethod.Invoke(settingService, new object[] { setting, storeId });
				}
			}

			_ctx.SaveChanges();
        }

		private void PopulateCategories()
        {
            var categoriesFirstLevel = _data.CategoriesFirstLevel();
			SaveRange(categoriesFirstLevel);
            //search engine names
            categoriesFirstLevel.Each(x =>
            {
                Save(new UrlRecord()
                {
                    EntityId = x.Id,
                    EntityName = "Category",
                    LanguageId = 0,
                    Slug = ValidateSeName(x, x.Name),
                    IsActive = true
                });
            });

            var categoriesSecondLevel = _data.CategoriesSecondLevel();
			SaveRange(categoriesSecondLevel);
            //search engine names
            categoriesSecondLevel.Each(x =>
            {
                Save(new UrlRecord()
                {
                    EntityId = x.Id,
                    EntityName = "Category",
                    LanguageId = 0,
					Slug = ValidateSeName(x, x.Name),
                    IsActive = true
                });
            });
        }

		private void PopulateManufacturers()
        {
            var manufacturers = _data.Manufacturers();
			SaveRange(manufacturers);
            //search engine names
            manufacturers.Each(x =>
            {
                Save(new UrlRecord()
                {
                    EntityId = x.Id,
                    EntityName = "Manufacturer",
                    LanguageId = 0,
					Slug = ValidateSeName(x, x.Name),
                    IsActive = true
                });
            });
        }

		private void PopulateProducts()
        {
            var products = _data.Products();
			SaveRange(products);
            //search engine names
            products.Each(x =>
            {
                Save(new UrlRecord()
                {
                    EntityId = x.Id,
                    EntityName = "Product",
                    LanguageId = 0,
					Slug = ValidateSeName(x, x.Name),
                    IsActive = true
                });
            });

			_data.AssignGroupedProducts(products);
        }

        private void PopulateBlogPosts()
        {
            var blogPosts = _data.BlogPosts();
			SaveRange(blogPosts);
            //search engine names
            blogPosts.Each(x =>
            {
                Save(new UrlRecord()
                {
                    EntityId = x.Id,
                    EntityName = "BlogPost",
                    LanguageId = x.LanguageId,
					Slug = ValidateSeName(x, x.Title),
                    IsActive = true
                });
            });
        }

		private void PopulateNews()
        {
            var newsItems = _data.NewsItems();
			SaveRange(newsItems);
            //search engine names
            newsItems.Each(x =>
            {
                Save(new UrlRecord()
                {
                    EntityId = x.Id,
                    EntityName = "NewsItem",
                    LanguageId = x.LanguageId,
                    IsActive = true,
					Slug = ValidateSeName(x, x.Title)
                });
            });
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

        private void AddProductTag(Product product, string tag)
        {
			var productTag = _ctx.Set<ProductTag>().FirstOrDefault(pt => pt.Name == tag);
            if (productTag == null)
            {
                productTag = new ProductTag()
                {
                    Name = tag
                };
            }
			product.ProductTags.Add(productTag);
			Save(product);
        }

		private void MovePictures()
		{
			if (!_config.StoreMediaInDB)
			{
				// All pictures have initially been stored in the DB.
				// Move the binaries to disk
				var pics = _ctx.Set<Picture>().ToList();
				foreach (var pic in pics)
				{
					this.PictureService.UpdatePicture(pic.Id, pic.PictureBinary, pic.MimeType, pic.SeoFilename, pic.IsNew, false);
				}
				_ctx.SaveChanges();
			}
		}

        #endregion

        #region Properties

        protected SeedDataConfiguration Configuration
        {
            get
            {
                return _config;
            }
        }

		protected SmartObjectContext DataContext
		{
			get
			{
				return _ctx;
			}
		}


		protected ISettingService SettingService
		{
			get
			{
				if (_settingService == null)
				{
					var rs = new EfRepository<Setting>(_ctx);
					rs.AutoCommitEnabled = false;

					_settingService = new SettingService(NullCache.Instance, NullEventPublisher.Instance, rs);
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

					_gaService = new GenericAttributeService(NullCache.Instance, rs, NullEventPublisher.Instance, rsOrder);
				}

				return _gaService;
			}
		}

		protected IPictureService PictureService
		{
			get
			{
				if (_pictureService == null)
				{
					var rs = new EfRepository<Picture>(_ctx);
					rs.AutoCommitEnabled = false;

					var rsMap = new EfRepository<ProductPicture>(_ctx);
					rs.AutoCommitEnabled = false;
					
					var mediaSettings = new MediaSettings();
					var webHelper = new WebHelper(null);

					_pictureService = new PictureService(
						rs, 
						rsMap,
						this.SettingService,
						webHelper,
						NullLogger.Instance,
						NullEventPublisher.Instance,
						mediaSettings,
						new ImageResizerService(),
						new ImageCache(mediaSettings, webHelper, null, null),
						new Notifier());
				}

				return _pictureService;
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

					var storeMappingService = new StoreMappingService(NullCache.Instance, null, null, null);
					var storeService = new StoreService(NullCache.Instance, new EfRepository<Store>(_ctx), NullEventPublisher.Instance);
					var storeContext = new WebStoreContext(storeService, new WebHelper(null), null);

					var locSettings = new LocalizationSettings();

					var languageService = new LanguageService(
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
						languageService,
						locSettings,
						NullEventPublisher.Instance);
				}

				return _locService;
			}
		}

        #endregion Properties

        #region Methods

        public virtual void Seed(SmartObjectContext context)
        {
			Guard.ArgumentNotNull(() => context);

			_ctx = context;
			_data.Initialize(_ctx);
			
			_ctx.Configuration.AutoDetectChangesEnabled = false;
			_ctx.Configuration.ValidateOnSaveEnabled = false;
			_ctx.HooksEnabled = false;

			_config.ProgressMessageCallback("Progress.CreatingRequiredData");

            // special mandatory (non-visible) settings
			_ctx.MigrateSettings(x =>
			{
				x.Add("Media.Images.StoreInDB", _config.StoreMediaInDB);
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
			Populate("PopulateCustomersAndUsers", () => PopulateCustomersAndUsers(_config.DefaultUserName, _config.DefaultUserPassword));
			Populate("PopulateEmailAccounts", _data.EmailAccounts());
			Populate("PopulateMessageTemplates", _data.MessageTemplates());
			Populate("PopulateTopics", _data.Topics());
			Populate("PopulateSettings", PopulateSettings);
			Populate("PopulateLocaleResources", PopulateLocaleResources);
			Populate("PopulateActivityLogTypes", _data.ActivityLogTypes());
			Populate("PopulateCustomersAndUsers", () => HashDefaultCustomerPassword(_config.DefaultUserName, _config.DefaultUserPassword));
			Populate("PopulateProductTemplates", _data.ProductTemplates());
			Populate("PopulateCategoryTemplates", _data.CategoryTemplates());
			Populate("PopulateManufacturerTemplates", PopulateManufacturerTemplates);
			Populate("PopulateScheduleTasks", _data.ScheduleTasks());

            if (_config.SeedSampleData)
            {
				_config.ProgressMessageCallback("Progress.CreatingSampleData");

				Populate("PopulateSpecificationAttributes", _data.SpecificationAttributes());
				Populate("PopulateProductAttributes", _data.ProductAttributes());
				Populate("PopulateCategories", PopulateCategories);
				Populate("PopulateManufacturers", PopulateManufacturers);
				Populate("PopulateProducts", PopulateProducts);
				Populate("PopulateProductBundleItems", _data.ProductBundleItems());
				Populate("PopulateProductVariantAttributes", _data.ProductVariantAttributes());
				Populate("ProductVariantAttributeCombinations", _data.ProductVariantAttributeCombinations());
				Populate("PopulateProductTags", _data.ProductTags());
				Populate("PopulateForumsGroups", _data.ForumGroups());
				Populate("PopulateForums", _data.Forums());
				Populate("PopulateDiscounts", _data.Discounts());
				Populate("PopulateBlogPosts", PopulateBlogPosts);
				Populate("PopulateNews", PopulateNews);
				Populate("PopulatePolls", _data.Polls());
            }

			Populate("MovePictures", MovePictures);
        }

		public bool RollbackOnFailure
		{
			get { return false; }
		}

        #endregion

		#region Utils

		private void SetModified<TEntity>(TEntity entity) 
			where TEntity : BaseEntity
		{
			_ctx.Set<TEntity>().Attach(entity);
			_ctx.Entry(entity).State = System.Data.Entity.EntityState.Modified;
		}

		private string ValidateSeName<TEntity>(TEntity entity, string name)
			where TEntity : BaseEntity, ISlugSupported
		{
			var seoSettings = new SeoSettings { LoadAllUrlAliasesOnStartup = false };
			
			if (_urlRecordService == null)
			{
				_urlRecordService = new UrlRecordService(NullCache.Instance, new EfRepository<UrlRecord>(_ctx) { AutoCommitEnabled = false }, seoSettings);
			}

			return entity.ValidateSeName<TEntity>("", name, true, _urlRecordService, new SeoSettings());
		}

		private void Populate<TEntity>(string stage, IEnumerable<TEntity> entities) 
			where TEntity : BaseEntity
		{
			try
			{
				SaveRange(entities);
			}
			catch (Exception ex)
			{
				throw new SeedDataException(stage, ex);
			}
		}

		private void Populate(string stage, Action populateAction)
		{
			try
			{
				populateAction();
			}
			catch (Exception ex)
			{
				throw new SeedDataException(stage, ex);
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
