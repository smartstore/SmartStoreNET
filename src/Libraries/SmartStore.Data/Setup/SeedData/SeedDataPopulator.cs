using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Threading;
using System.Globalization;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using SmartStore.Core;
using SmartStore.Core.Configuration;
using SmartStore.Core.Data;
using SmartStore.Core.Domain;
using SmartStore.Core.Domain.Blogs;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Discounts;
using SmartStore.Core.Domain.Forums;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Logging;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Domain.Messages;
using SmartStore.Core.Domain.News;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Payments;
using SmartStore.Core.Domain.Polls;
using SmartStore.Core.Domain.Security;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Domain.Tasks;
using SmartStore.Core.Domain.Tax;
using SmartStore.Core.Domain.Topics;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.IO;
using SmartStore.Services.Configuration;
using SmartStore.Core.Domain.Seo;
using SmartStore.Core.Domain.Stores;
using SmartStore.Data;
using SmartStore.Core.Caching;
using SmartStore.Utilities.Threading;
using SmartStore.Core.Domain.Themes;
using SmartStore.Utilities;
using SmartStore.Core.Domain.Configuration;
using System.ComponentModel;
using System.Security.Cryptography;
using Newtonsoft.Json;

namespace SmartStore.Data.Setup
{
    public partial class SeedDataPopulator
    {
        #region Fields & Constants

		private readonly SmartObjectContext _ctx;
        private SeedDataConfiguration _config;
        private InvariantSeedData _data;
		private int _defaultStoreId;

        #endregion Fields & Constants

        #region Ctor

		public SeedDataPopulator(SmartObjectContext dbContext, SeedDataConfiguration configuration)
        {
			Guard.ArgumentNotNull(() => dbContext);
			Guard.ArgumentNotNull(() => configuration);

			Guard.ArgumentNotNull(configuration.Language, "Language");
			Guard.ArgumentNotNull(configuration.Data, "SeedData");

			_ctx = dbContext;
			_config = configuration;
			_data = configuration.Data;

			_data.Initialize(_ctx);
        }

        #endregion Ctor

        #region Utilities

		private void PopulatePictures()
		{
			var pictures = _data.Pictures();
			SaveRange(pictures);

			if (_config.StoreMediaInDB)
				return;

			foreach (var picture in pictures)
			{
				var buffer = picture.PictureBinary;
				picture.PictureBinary = new byte[0];

				string lastPart = MimeTypes.MapMimeTypeToExtension(picture.MimeType);
				string fileName = string.Format("{0}-0.{1}", picture.Id.ToString("0000000"), lastPart);
				File.WriteAllBytes(GetPictureLocalPath(fileName), buffer);
			}

			_ctx.SaveChanges();
		}

		/// <summary>
		/// Copied over from <see cref="PictureService" /> to avoid dependency wiring
		/// </summary>
		private string GetPictureLocalPath(string fileName)
		{
			var imagesDirectoryPath = CommonHelper.MapPath("~/Media/");
			var filePath = Path.Combine(imagesDirectoryPath, fileName);
			return filePath;
		}

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
                if (_data.FixedTaxRates.HasItems() && _data.FixedTaxRates.Length > i)
                {
                    rate = _data.FixedTaxRates[i];
                }
                i++;
                SetSetting(string.Format("Tax.TaxProvider.FixedRate.TaxCategoryId{0}", id), rate, false);
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

			//// TODO: REF (Start)
			//var localizationService = EngineContext.Current.Resolve<ILocalizationService>();

			//// save resources
			//foreach (var filePath in System.IO.Directory.EnumerateFiles(locPath, "*.smres.xml", SearchOption.TopDirectoryOnly))
			//{
			//	var doc = new XmlDocument();
			//	doc.Load(filePath);

			//	doc = localizationService.FlattenResourceFile(doc);

			//	// now we have a parsed XML file (the same structure as exported language packs)
			//	// let's save resources
			//	localizationService.ImportResourcesFromXml(language, doc);
			//}
			//// TODO: REF (End)
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
            var adminUser = new Customer()
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

            adminUser.Addresses.Add(_data.AdminAddress());
            adminUser.BillingAddress = _data.AdminAddress();
            adminUser.ShippingAddress = _data.AdminAddress();
            adminUser.CustomerRoles.Add(customerRoles.SingleOrDefault(x => x.SystemName == SystemCustomerRoleNames.Administrators));
            adminUser.CustomerRoles.Add(customerRoles.SingleOrDefault(x => x.SystemName == SystemCustomerRoleNames.ForumModerators));
            adminUser.CustomerRoles.Add(customerRoles.SingleOrDefault(x => x.SystemName == SystemCustomerRoleNames.Registered));
            Save(adminUser);

			////set default customer name
			SaveGenericAttribute(adminUser, SystemCustomerAttributeNames.FirstName, adminUser.Addresses.FirstOrDefault().FirstName);
			SaveGenericAttribute(adminUser, SystemCustomerAttributeNames.LastName, adminUser.Addresses.FirstOrDefault().LastName);

            //search engine (crawler) built-in user
            var customer = _data.SearchEngineUser();
            customer.CustomerRoles.Add(customerRoles.SingleOrDefault(x => x.SystemName == SystemCustomerRoleNames.Guests));
            Save(customer);

            //built-in user for background tasks
            customer = _data.BackgroundTaskUser();
            customer.CustomerRoles.Add(customerRoles.SingleOrDefault(x => x.SystemName == SystemCustomerRoleNames.Guests));
            Save(customer);
        }

		private void HashDefaultCustomerPassword(string defaultUserEmail, string defaultUserPassword)
        {
			var adminUser = _ctx.Set<Customer>().Where(x => x.Email == _config.DefaultUserName).Single();

			// Generate a cryptographic random number
			var rng = new RNGCryptoServiceProvider();
			var buff = new byte[5];
			rng.GetBytes(buff);

			string saltKey = Convert.ToBase64String(buff);
			adminUser.PasswordSalt = saltKey;
			adminUser.PasswordFormat = PasswordFormat.Hashed;

			string saltAndPassword = String.Concat(_config.DefaultUserPassword, saltKey);
			var algorithm = HashAlgorithm.Create("SHA1");
			var hashByteArray = algorithm.ComputeHash(Encoding.UTF8.GetBytes(saltAndPassword));

			adminUser.Password = BitConverter.ToString(hashByteArray).Replace("-", "");			

			_ctx.SaveChanges();
        }

		private void PopulateSettings()
        {
            var settings = _data.Settings();
            foreach (var setting in settings)
            {
				SaveSettingClass(setting);
            }
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
                    Slug = SeoHelper.GetSeName(x.Name, true, false),
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
					Slug = SeoHelper.GetSeName(x.Name, true, false),
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
					Slug = SeoHelper.GetSeName(x.Name, true, false),
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
					Slug = SeoHelper.GetSeName(x.Name, true, false),
                    IsActive = true
                });
            });
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
					Slug = SeoHelper.GetSeName(x.Title, true, false),
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
					Slug = SeoHelper.GetSeName(x.Title, true, false)
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

		public virtual void SaveSettingClass(object settings)
		{
			Type t = settings.GetType();
			
			if (t.HasAttribute<JsonPersistAttribute>(true))
			{
				string key = t.Namespace + "." + t.Name;
				var rawSettings = JsonConvert.SerializeObject(settings);
				SetSetting(key, rawSettings);
				return;
			}

			foreach (var prop in t.GetProperties())
			{
				// get properties we can read and write to
				if (!prop.CanRead || !prop.CanWrite)
					continue;

				if (!TypeDescriptor.GetConverter(prop.PropertyType).CanConvertFrom(typeof(string)))
					continue;

				string key = t.Name + "." + prop.Name;
				// Duck typing is not supported in C#. That's why we're using dynamic type
				dynamic value = prop.GetValue(settings);

				SetSetting(key, value ?? "", false);
			}

			_ctx.SaveChanges();
		}

		private void SetSetting<T>(string key, T value, bool save = true)
		{
			key = key.Trim().ToLowerInvariant();
			string valueStr = TypeDescriptor.GetConverter(typeof(T)).ConvertToInvariantString(value);
			
			var setting = new Setting()
			{
				Name = key,
				Value = valueStr
			};

			if (save)
			{
				Save(setting);
			}
			else
			{
				_ctx.Set<Setting>().Add(setting);
			}
		}

		private void SaveGenericAttribute<TPropType>(BaseEntity entity, string key, TPropType value)
		{
			string valueStr = value.Convert<string>();
			string keyGroup = entity.GetUnproxiedEntityType().Name;
			var prop = new GenericAttribute()
			{
				EntityId = entity.Id,
				Key = key,
				KeyGroup = keyGroup,
				Value = valueStr
			};
			Save(prop);
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

        #endregion Properties

        #region Methods

        public virtual void Seed()
        {
            _ctx.Configuration.AutoDetectChangesEnabled = false;
			_ctx.Configuration.ValidateOnSaveEnabled = false;

            // special mandatory (non-visible) settings
			SetSetting("Media.Images.StoreInDB", _config.StoreMediaInDB);

			Populate("PopulatePictures", PopulatePictures);
			Populate("PopulateStores", PopulateStores);
			Populate("InstallLanguages", () => PopulateLanguage(_config.Language));
			Populate("PopulateMeasureDimensions", _data.MeasureDimensions());
			Populate("PopulateMeasureWeights", _data.MeasureWeights());
			Populate("PopulateTaxCategories", PopulateTaxCategories);
			Populate("PopulateCurrencies", PopulateCurrencies);
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
				Populate("PopulateSpecificationAttributes", _data.SpecificationAttributes());
				Populate("PopulateProductAttributes", _data.ProductAttributes());
				Populate("PopulateCategories", PopulateCategories);
				Populate("PopulateManufacturers", PopulateManufacturers);
				Populate("PopulateProducts", PopulateProducts);
				Populate("PopulateProductTags", _data.ProductTags());
				Populate("PopulateForumsGroups", _data.ForumGroups());
				Populate("PopulateForums", _data.Forums());
				Populate("PopulateDiscounts", _data.Discounts());
				Populate("PopulateBlogPosts", PopulateBlogPosts);
				Populate("PopulateNews", PopulateNews);
				Populate("PopulatePolls", _data.Polls());
            }
        }

		private void Populate<TEntity>(string stage, IEnumerable<TEntity> entities) where TEntity : BaseEntity
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

        #endregion methods
    }
        
} 
