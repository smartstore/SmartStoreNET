using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Threading;
using System.Globalization;
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
using SmartStore.Services.Common;
using SmartStore.Services.Customers;
using SmartStore.Services.Helpers;
using SmartStore.Services.Media;
using SmartStore.Services.Localization;
using SmartStore.Services.Configuration;
using SmartStore.Services.Seo;
using SmartStore.Core.Domain.Seo;
using SmartStore.Core.Domain.Stores;
using SmartStore.Data;
using SmartStore.Core.Caching;
using SmartStore.Utilities.Threading;
using SmartStore.Core.Domain.Themes;

namespace SmartStore.Services.Installation
{
    public partial class InstallationService : IInstallationService
    {
        #region Fields & Constants

        // codehint: sm-add
        private readonly ISettingService _settingService;
        private readonly IDbContext _dbContext;
        private InstallDataContext _installContext;
        private InvariantInstallationData _installData;
        private float _totalSteps;
        private float _currentStep;
		private int _defaultStoreId;

		private readonly IRepository<Store> _storeRepository;
        private readonly IRepository<MeasureDimension> _measureDimensionRepository;
        private readonly IRepository<MeasureWeight> _measureWeightRepository;
        private readonly IRepository<TaxCategory> _taxCategoryRepository;
        private readonly IRepository<Language> _languageRepository;
        private readonly IRepository<Currency> _currencyRepository;
        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<CustomerRole> _customerRoleRepository;
        private readonly IRepository<SpecificationAttribute> _specificationAttributeRepository;
        private readonly IRepository<ProductAttribute> _productAttributeRepository;
        private readonly IRepository<Category> _categoryRepository;
        private readonly IRepository<Manufacturer> _manufacturerRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<UrlRecord> _urlRecordRepository;
        private readonly IRepository<RelatedProduct> _relatedProductRepository;
        private readonly IRepository<EmailAccount> _emailAccountRepository;
        private readonly IRepository<MessageTemplate> _messageTemplateRepository;
        private readonly IRepository<ForumGroup> _forumGroupRepository;
        private readonly IRepository<Forum> _forumRepository;
        private readonly IRepository<Country> _countryRepository;
        private readonly IRepository<StateProvince> _stateProvinceRepository;
        private readonly IRepository<Discount> _discountRepository;
        private readonly IRepository<DeliveryTime> _deliveryTimeRepository;
        private readonly IRepository<BlogPost> _blogPostRepository;
        private readonly IRepository<Topic> _topicRepository;
        private readonly IRepository<NewsItem> _newsItemRepository;
        private readonly IRepository<Poll> _pollRepository;
        private readonly IRepository<PollAnswer> _pollAnswerRepository;
        private readonly IRepository<ShippingMethod> _shippingMethodRepository;
        private readonly IRepository<ActivityLogType> _activityLogTypeRepository;
        private readonly IRepository<ProductTag> _productTagRepository;
        private readonly IRepository<ProductTemplate> _productTemplateRepository;
        private readonly IRepository<CategoryTemplate> _categoryTemplateRepository;
        private readonly IRepository<ManufacturerTemplate> _manufacturerTemplateRepository;
        private readonly IRepository<ScheduleTask> _scheduleTaskRepository;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IWebHelper _webHelper;

        #endregion Fields & Constants

        #region Ctor

        public InstallationService(
            IDbContext context,
            ISettingService settingService,
			IRepository<Store> storeRepository,
            IRepository<MeasureDimension> measureDimensionRepository,
            IRepository<MeasureWeight> measureWeightRepository,
            IRepository<TaxCategory> taxCategoryRepository,
            IRepository<Language> languageRepository,
            IRepository<Currency> currencyRepository,
            IRepository<Customer> customerRepository,
            IRepository<CustomerRole> customerRoleRepository,
            IRepository<SpecificationAttribute> specificationAttributeRepository,
            IRepository<ProductAttribute> productAttributeRepository,
            IRepository<Category> categoryRepository,
            IRepository<Manufacturer> manufacturerRepository,
            IRepository<Product> productRepository,
            IRepository<UrlRecord> urlRecordRepository,
            IRepository<RelatedProduct> relatedProductRepository,
            IRepository<EmailAccount> emailAccountRepository,
            IRepository<MessageTemplate> messageTemplateRepository,
            IRepository<ForumGroup> forumGroupRepository,
            IRepository<Forum> forumRepository,
            IRepository<Country> countryRepository,
            IRepository<StateProvince> stateProvinceRepository,
            IRepository<Discount> discountRepository,
            IRepository<DeliveryTime> deliveryTimeRepository,
            IRepository<BlogPost> blogPostRepository,
            IRepository<Topic> topicRepository,
            IRepository<NewsItem> newsItemRepository,
            IRepository<Poll> pollRepository,
            IRepository<PollAnswer> pollAnswerRepository,
            IRepository<ShippingMethod> shippingMethodRepository,
            IRepository<ActivityLogType> activityLogTypeRepository,
            IRepository<ProductTag> productTagRepository,
            IRepository<ProductTemplate> productTemplateRepository,
            IRepository<CategoryTemplate> categoryTemplateRepository,
            IRepository<ManufacturerTemplate> manufacturerTemplateRepository,
            IRepository<ScheduleTask> scheduleTaskRepository,
            IGenericAttributeService genericAttributeService,
            IWebHelper webHelper)
        {
            this._dbContext = context;
            this._settingService = settingService;
			this._storeRepository = storeRepository;
            this._measureDimensionRepository = measureDimensionRepository;
            this._measureWeightRepository = measureWeightRepository;
            this._taxCategoryRepository = taxCategoryRepository;
            this._languageRepository = languageRepository;
            this._currencyRepository = currencyRepository;
            this._customerRepository = customerRepository;
            this._customerRoleRepository = customerRoleRepository;
            this._specificationAttributeRepository = specificationAttributeRepository;
            this._productAttributeRepository = productAttributeRepository;
            this._categoryRepository = categoryRepository;
            this._manufacturerRepository = manufacturerRepository;
            this._productRepository = productRepository;
            this._urlRecordRepository = urlRecordRepository;
            this._relatedProductRepository = relatedProductRepository;
            this._emailAccountRepository = emailAccountRepository;
            this._messageTemplateRepository = messageTemplateRepository;
            this._forumGroupRepository = forumGroupRepository;
            this._forumRepository = forumRepository;
            this._countryRepository = countryRepository;
            this._stateProvinceRepository = stateProvinceRepository;
            this._discountRepository = discountRepository;
            this._blogPostRepository = blogPostRepository;
            this._topicRepository = topicRepository;
            this._newsItemRepository = newsItemRepository;
            this._pollRepository = pollRepository;
            this._pollAnswerRepository = pollAnswerRepository;
            this._shippingMethodRepository = shippingMethodRepository;
            this._activityLogTypeRepository = activityLogTypeRepository;
            this._productTagRepository = productTagRepository;
            this._productTemplateRepository = productTemplateRepository;
            this._categoryTemplateRepository = categoryTemplateRepository;
            this._manufacturerTemplateRepository = manufacturerTemplateRepository;
            this._scheduleTaskRepository = scheduleTaskRepository;
            this._genericAttributeService = genericAttributeService;
            this._webHelper = webHelper;
            this._deliveryTimeRepository = deliveryTimeRepository;
        }

        #endregion Ctor

        #region Utilities

        // codehint: sm-add
        private void IncreaseProgress()
        {
            _currentStep++;
            int progress = (int)((_currentStep / _totalSteps) * 100);
            if (_installContext.ProgressCallback != null)
            {
                _installContext.ProgressCallback(progress);
            }
        }

		private void InstallStores()
		{
			try
			{
				var store = InvariantInstallationData.DefaultStore;
				_storeRepository.Insert(store);
				_defaultStoreId = store.Id;
			}
			catch (Exception ex)
			{
				throw new InstallationException("InstallStores", ex);
			}
		}

		private void UpdateStores()
		{
			var oldAutoDetect = _dbContext.AutoDetectChangesEnabled;

			try
			{
				_dbContext.AutoDetectChangesEnabled = true;

				var entities = _storeRepository.Table.OrderBy(x => x.DisplayOrder).ToList();

				_installData.DefaultStores(entities);

				foreach (var entity in entities)
					_storeRepository.Update(entity);
			}
			catch (Exception ex)
			{
				throw new InstallationException("UpdateStores", ex);
			}

			try
			{
				_dbContext.AutoDetectChangesEnabled = oldAutoDetect;
			}
			catch (Exception)
			{
			}
		}

        private void InstallMeasureDimensions()
        {
            try
            {
                _measureDimensionRepository.InsertRange(_installData.MeasureDimensions());
                IncreaseProgress();
            }
            catch(Exception ex) 
            {
                throw new InstallationException("InstallMeasureDimensions", ex);
            }
        }

        private void InstallMeasureWeights()
        {
            try
            {
                _measureWeightRepository.InsertRange(_installData.MeasureWeights());
                IncreaseProgress();
            }
            catch (Exception ex)
            {
                throw new InstallationException("InstallMeasureWeights", ex);
            }
        }

        private void WriteSetting(String Key, decimal Value)
        {
            _settingService.SetSetting(Key, Value);
        }

        private void InstallTaxCategories()
        {
            try
            {
                _taxCategoryRepository.InsertRange(_installData.TaxCategories());
                
                // add tax rates to fixed rate provider
                var taxCategories = _taxCategoryRepository.Table;
                int i = 0;
                var taxIds = taxCategories.OrderBy(x => x.Id).Select(x => x.Id).ToList();
                foreach (var id in taxIds)
                {
                    decimal rate = 0;
                    if (_installData.FixedTaxRates.HasItems() && _installData.FixedTaxRates.Length > i)
                    {
                        rate = _installData.FixedTaxRates[i];
                    }
                    i++;
                    //WriteSetting(string.Format("Tax.TaxProvider.FixedRate.TaxCategoryId{0}", id), rate);
                    _settingService.SetSetting(string.Format("Tax.TaxProvider.FixedRate.TaxCategoryId{0}", id), rate);
                }
               
                IncreaseProgress();
            }
            catch (Exception ex)
            {
                throw new InstallationException("InstallTaxCategories", ex);
            }

        }

        protected virtual void InstallLanguages(Language primaryLanguage)
        {
            try
            {
                primaryLanguage.Published = true;
                //primaryLanguage.DisplayOrder = 1; // MC: Must be 0
                _languageRepository.Insert(primaryLanguage);
                IncreaseProgress();
            }
            catch (Exception ex)
            {
                throw new InstallationException("InstallLanguages", ex);
            }
        }

        protected virtual void InstallLocaleResources() 
        {
            try
            {
                // Default primary language
                var language = _languageRepository.Table.Single();

                var locPath = _webHelper.MapPath("~/App_Data/Localization/App/" + language.LanguageCulture);
                if (!System.IO.Directory.Exists(locPath))
                {
                    // Fallback to neutral language folder (de, en etc.)
                    locPath = _webHelper.MapPath("~/App_Data/Localization/App/" + language.UniqueSeoCode);
                }

                var localizationService = EngineContext.Current.Resolve<ILocalizationService>();

                // save resources
                foreach (var filePath in System.IO.Directory.EnumerateFiles(locPath, "*.smres.xml", SearchOption.TopDirectoryOnly))
                {
                    var doc = new XmlDocument();
                    doc.Load(filePath);

                    doc = localizationService.FlattenResourceFile(doc);

                    // now we have a parsed XML file (the same structure as exported language packs)
                    // let's save resources
                    localizationService.ImportResourcesFromXml(language, doc);
                }
                IncreaseProgress();
            }
            catch (Exception ex)
            {
                throw new InstallationException("InstallLocaleResources", ex);
            }

        }

        // codehint: sm-add
        private Currency CreateCurrency(string locale, decimal rate = 1, string formatting = "", bool published = false, int order = 1)
        {
            RegionInfo info = null;
            Currency currency = null;
            try
            {
                info = new RegionInfo(locale);
                if (info != null)
                {
                    currency = new Currency();
                    currency.DisplayLocale = locale;
                    currency.Name = info.CurrencyNativeName;
                    currency.CurrencyCode = info.ISOCurrencySymbol;
                    currency.Rate = rate;
                    currency.CustomFormatting = formatting;
                    currency.Published = published;
                    currency.DisplayOrder = order;
                    currency.CreatedOnUtc = DateTime.UtcNow;
                    currency.UpdatedOnUtc = DateTime.UtcNow;
                }
                IncreaseProgress();
            }
            catch
            {
                return null;
            }

            return currency;
        }

        protected virtual void InstallCurrencies()
        {
            try
            {
                _currencyRepository.InsertRange(_installData.Currencies().Where(x => x != null));
                IncreaseProgress();
            }
            catch (Exception ex)
            {
                throw new InstallationException("InstallCurrencies", ex);
            }
        }

        protected virtual void InstallCountriesAndStates()
        {
            try
            {
                _countryRepository.InsertRange(_installData.Countries().Where(x => x != null));
                IncreaseProgress();
            }
            catch (Exception ex)
            {
                throw new InstallationException("InstallCountriesAndStates", ex);
            }
        }

        protected virtual void InstallShippingMethods()
        {
            try
            {
                _shippingMethodRepository.InsertRange(_installData.ShippingMethods().Where(x => x != null));
                IncreaseProgress();
            }
            catch (Exception ex)
            {
                throw new InstallationException("InstallShippingMethods", ex);
            }
        }

        protected virtual void InstallCustomersAndUsers(string defaultUserEmail, string defaultUserPassword)
        {
            try
            {
                //customerRoles.ForEach(cr => _customerRoleRepository.Insert(cr));
                var customerRoles = _installData.CustomerRoles();
                _customerRoleRepository.InsertRange(customerRoles.Where(x => x != null));

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

                adminUser.Addresses.Add(_installData.AdminAddress());
                adminUser.BillingAddress = _installData.AdminAddress();
                adminUser.ShippingAddress = _installData.AdminAddress();
                adminUser.CustomerRoles.Add(customerRoles.SingleOrDefault(x => x.SystemName == SystemCustomerRoleNames.Administrators));
                adminUser.CustomerRoles.Add(customerRoles.SingleOrDefault(x => x.SystemName == SystemCustomerRoleNames.ForumModerators));
                adminUser.CustomerRoles.Add(customerRoles.SingleOrDefault(x => x.SystemName == SystemCustomerRoleNames.Registered));
                _customerRepository.Insert(adminUser);
                //set default customer name
				_genericAttributeService.SaveAttribute(adminUser, SystemCustomerAttributeNames.FirstName, adminUser.Addresses.FirstOrDefault().FirstName);
				_genericAttributeService.SaveAttribute(adminUser, SystemCustomerAttributeNames.LastName, adminUser.Addresses.FirstOrDefault().LastName);

                //search engine (crawler) built-in user
                var customer = _installData.SearchEngineUser();
                customer.CustomerRoles.Add(customerRoles.SingleOrDefault(x => x.SystemName == SystemCustomerRoleNames.Guests));
                _customerRepository.Insert(customer);

                //built-in user for background tasks
                customer = _installData.BackgroundTaskUser();
                customer.CustomerRoles.Add(customerRoles.SingleOrDefault(x => x.SystemName == SystemCustomerRoleNames.Guests));
                _customerRepository.Insert(customer);

                IncreaseProgress();
            }
            catch (Exception ex)
            {
                throw new InstallationException("InstallCustomersAndUsers", ex);
            }
        }

        protected virtual void HashDefaultCustomerPassword(string defaultUserEmail, string defaultUserPassword)
        {
            try
            {
                var customerRegistrationService = EngineContext.Current.Resolve<ICustomerRegistrationService>();
                customerRegistrationService.ChangePassword(new ChangePasswordRequest(defaultUserEmail, false,
                 PasswordFormat.Hashed, defaultUserPassword));
                IncreaseProgress();
            }
            catch (Exception ex)
            {
                throw new InstallationException("HashDefaultCustomerPassword", ex);
            }    
        }

        protected virtual void InstallEmailAccounts()
        {
            try
            {
                _emailAccountRepository.InsertRange(_installData.EmailAccounts());
                IncreaseProgress();
            }
            catch (Exception ex)
            {
                throw new InstallationException("InstallEmailAccounts", ex);
            }
        }

        protected virtual void InstallMessageTemplates()
        {
            //var eaGeneral = _emailAccountRepository.Table.Where(ea => ea.DisplayName.Equals("General contact")).FirstOrDefault();
            //var eaSale = _emailAccountRepository.Table.Where(ea => ea.DisplayName.Equals("Sales representative")).FirstOrDefault();
            //var eaCustomer = _emailAccountRepository.Table.Where(ea => ea.DisplayName.Equals("Customer support")).FirstOrDefault();
            
            //messageTemplates.ForEach(mt => _messageTemplateRepository.Insert(mt));
            try
            {
                _messageTemplateRepository.InsertRange(_installData.MessageTemplates());
                IncreaseProgress();
            }
            catch (Exception ex)
            {
                throw new InstallationException("InstallMessageTemplates", ex);
            }

        }

        protected virtual void InstallTopics()
        {
            //topics.ForEach(t => _topicRepository.Insert(t));
            try
            {
                _topicRepository.InsertRange(_installData.Topics());
                IncreaseProgress();
            }
            catch (Exception ex)
            {
                throw new InstallationException("InstallTopics", ex);
            }
        }

        protected virtual void InstallSettings()
        {
            try
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

                var settings = _installData.Settings();
                foreach (var setting in settings)
                {
                    Type settingType = setting.GetType();
					Type settingServiceType = typeof(ISettingService);

					var settingService = EngineContext.Current.Resolve(settingServiceType);
					if (settingService != null)
					{
						var genericMethod = method.MakeGenericMethod(settingType);
						int storeId = (settingType.Equals(typeof(ThemeSettings)) ? _defaultStoreId : 0);

						genericMethod.Invoke(settingService, new object[] { setting, storeId });
					}
                }

                IncreaseProgress();
            }
            catch (Exception ex)
            {
                throw new InstallationException("InstallSettings", ex);
            }
        }

        protected virtual void InstallSpecificationAttributes()
        {
            #region oldcode
            //var sa1 = new SpecificationAttribute
            //{
            //    Name = "Screensize",
            //    DisplayOrder = 1,
            //};
            //sa1.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            //{
            //    Name = "10.0''",
            //    DisplayOrder = 3,
            //});
            //sa1.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            //{
            //    Name = "14.1''",
            //    DisplayOrder = 4,
            //});
            //sa1.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            //{
            //    Name = "15.4''",
            //    DisplayOrder = 5,
            //});
            //sa1.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            //{
            //    Name = "16.0''",
            //    DisplayOrder = 6,
            //});
            //var sa2 = new SpecificationAttribute
            //{
            //    Name = "CPU Type",
            //    DisplayOrder = 2,
            //};
            //sa2.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            //{
            //    Name = "AMD",
            //    DisplayOrder = 1,
            //});
            //sa2.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            //{
            //    Name = "Intel",
            //    DisplayOrder = 2,
            //});
            //var sa3 = new SpecificationAttribute
            //{
            //    Name = "Memory",
            //    DisplayOrder = 3,
            //};
            //sa3.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            //{
            //    Name = "1 GB",
            //    DisplayOrder = 1,
            //});
            //sa3.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            //{
            //    Name = "3 GB",
            //    DisplayOrder = 2,
            //});
            //var sa4 = new SpecificationAttribute
            //{
            //    Name = "Hardrive",
            //    DisplayOrder = 5,
            //};
            //sa4.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            //{
            //    Name = "320 GB",
            //    DisplayOrder = 7,
            //});
            //sa4.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            //{
            //    Name = "250 GB",
            //    DisplayOrder = 4,
            //});
            //sa4.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            //{
            //    Name = "160 GB",
            //    DisplayOrder = 3,
            //});
            //var specificationAttributes = new List<SpecificationAttribute>
            //                    {
            //                        sa1,
            //                        sa2,
            //                        sa3,
            //                        sa4
            //                    };


            //specificationAttributes.ForEach(sa => _specificationAttributeRepository.Insert(sa));
            //var specificationAttributes =  _installData.SpecificationAttributes();
            // specificationAttributes.ForEach(sa => _specificationAttributeRepository.Insert();
            #endregion oldcode
            try
            {
                _installData.SpecificationAttributes().Each(x => _specificationAttributeRepository.Insert(x));
                IncreaseProgress();
            }
            catch (Exception ex)
            {
                throw new InstallationException("InstallSpecificationAttributes", ex);
            }

        }

        protected virtual void InstallProductAttributes()
        {
            #region oldcode
            //var productAttributes = new List<ProductAttribute>
            //{
            //    new ProductAttribute
            //    {
            //        Name = "Color",
            //    },
            //    new ProductAttribute
            //    {
            //        Name = "Custom Text",
            //    },
            //    new ProductAttribute
            //    {
            //        Name = "HDD",
            //    },
            //    new ProductAttribute
            //    {
            //        Name = "OS",
            //    },
            //    new ProductAttribute
            //    {
            //        Name = "Processor",
            //    },
            //    new ProductAttribute
            //    {
            //        Name = "RAM",
            //    },
            //    new ProductAttribute
            //    {
            //        Name = "Size",
            //    },
            //    new ProductAttribute
            //    {
            //        Name = "Software",
            //    },
            //};
            //productAttributes.ForEach(pa => _productAttributeRepository.Insert(pa));
            #endregion oldcode

            try
            {
                _installData.ProductAttributes().Each(x => _productAttributeRepository.Insert(x));
                IncreaseProgress();
            }
            catch (Exception ex)
            {
                throw new InstallationException("InstallProductAttributes", ex);
            }
        }

        protected virtual void InstallCategories()
        {
            try
            {
                var categoriesFirstLevel = _installData.CategoriesFirstLevel();
                categoriesFirstLevel.Each(x => 
                    {
                        _categoryRepository.Insert(x);
                        IncreaseProgress();
                    });
                //search engine names
                categoriesFirstLevel.Each(x =>
                {
                    _urlRecordRepository.Insert(new UrlRecord()
                    {
                        EntityId = x.Id,
                        EntityName = "Category",
                        LanguageId = 0,
                        Slug = x.ValidateSeName("", x.Name, true),
                        IsActive = true
                    });
                });

                var categoriesSecondLevel = _installData.CategoriesSecondLevel();
                categoriesSecondLevel.Each(x => 
                    {
                        _categoryRepository.Insert(x);
                        //IncreaseProgress();
                        
                    });
                //search engine names
                categoriesSecondLevel.Each(x =>
                {
                    _urlRecordRepository.Insert(new UrlRecord()
                    {
                        EntityId = x.Id,
                        EntityName = "Category",
                        LanguageId = 0,
                        Slug = x.ValidateSeName("", x.Name, true),
                        IsActive = true
                    });
                });

                IncreaseProgress();

                //var categories =  _installData.Categories();
                //categories.Each(x => _categoryRepository.Insert(x));
                ////search engine names
                //categories.Each(x =>
                //    {
                //        _urlRecordRepository.Insert(new UrlRecord()
                //        {
                //            EntityId = x.Id,
                //            EntityName = "Category",
                //            LanguageId = 0,
                //            Slug = x.ValidateSeName("", x.Name, true),
                //            IsActive = true
                //        });
                //    });
            }
            catch (Exception ex)
            {
                throw new InstallationException("InstallCategories", ex);
            }

            #region oldcode
            //search engine names
            //foreach (var category in allCategories)
            //{
            //    _urlRecordRepository.Insert(new UrlRecord()
            //    {
            //        EntityId = category.Id,
            //        EntityName = "Category",
            //        LanguageId = 0,
            //        Slug = category.ValidateSeName("", category.Name, true)
            //    });
            //}
            #endregion oldcode

        }

        protected virtual void InstallManufacturers()
        {

        #region oldcode
            //var manufacturerTemplateInGridAndLines =
            //    _manufacturerTemplateRepository.Table.Where(pt => pt.Name == "Products in Grid or Lines").FirstOrDefault();

            //var allManufacturers = new List<Manufacturer>();
            //var manufacturerAsus = new Manufacturer
            //{
            //    Name = "ASUS",
            //    ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
            //    PageSize = 12,
            //    AllowCustomersToSelectPageSize = true,
            //    //PageSizeOptions = "4, 2, 8, 12",
            //    Published = true,
            //    DisplayOrder = 2,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow
            //};
            //_manufacturerRepository.Insert(manufacturerAsus);
            //allManufacturers.Add(manufacturerAsus);


            //var manufacturerHp = new Manufacturer
            //{
            //    Name = "HP",
            //    ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
            //    PageSize = 12,
            //    AllowCustomersToSelectPageSize = true,
            //    //PageSizeOptions = "4, 2, 8, 12",
            //    Published = true,
            //    DisplayOrder = 5,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow
            //};
            //_manufacturerRepository.Insert(manufacturerHp);
            //allManufacturers.Add(manufacturerHp);
                    

            ////search engine names
            //foreach (var manufacturer in allManufacturers)
            //{
            //    _urlRecordRepository.Insert(new UrlRecord()
            //    {
            //        EntityId = manufacturer.Id,
            //        EntityName = "Manufacturer",
            //        LanguageId = 0,
            //        Slug = manufacturer.ValidateSeName("", manufacturer.Name, true)
            //    });
            //}

        #endregion oldcode

            try
            {
                var manufacturers = _installData.Manufacturers();
                manufacturers.Each(x => 
                    {
                        _manufacturerRepository.Insert(x);
                        //IncreaseProgress();    
                    });
                //search engine names
                manufacturers.Each(x =>
                {
                    _urlRecordRepository.Insert(new UrlRecord()
                    {
                        EntityId = x.Id,
                        EntityName = "Manufacturer",
                        LanguageId = 0,
                        Slug = x.ValidateSeName("", x.Name, true),
                        IsActive = true
                    });
                });

                IncreaseProgress();
            }


            catch (Exception ex)
            {
                throw new InstallationException("InstallManufacturers", ex);
            }
        }

        protected virtual void InstallProducts()
        {


            #region search engine names
            //search engine names
            //foreach (var product in allProducts)
            //{
            //    _urlRecordRepository.Insert(new UrlRecord()
            //    {
            //        EntityId = product.Id,
            //        EntityName = "Product",
            //        LanguageId = 0,
            //        Slug = product.ValidateSeName("", product.Name, true)
            //    });
            //}
            #endregion search engine names


            #region oldcode relatedproducts
            ////related products
            //var relatedProducts = new List<RelatedProduct>()
            //{
            //    new RelatedProduct()
            //    {
            //         ProductId1 = productDiamondHeart.Id,
            //         ProductId2 = productDiamondBracelet.Id,
            //    },
            //    new RelatedProduct()
            //    {
            //         ProductId1 = productDiamondHeart.Id,
            //         ProductId2 = productDiamondEarrings.Id,
            //    },
            //    new RelatedProduct()
            //    {
            //         ProductId1 = productDiamondHeart.Id,
            //         ProductId2 = productEngagementRing.Id,
            //    },
            //    new RelatedProduct()
            //    {
            //         ProductId1 = productDiamondBracelet.Id,
            //         ProductId2 = productDiamondHeart.Id,
            //    },
            //    new RelatedProduct()
            //    {
            //         ProductId1 = productDiamondBracelet.Id,
            //         ProductId2 = productEngagementRing.Id,
            //    },
            //    new RelatedProduct()
            //    {
            //         ProductId1 = productDiamondBracelet.Id,
            //         ProductId2 = productDiamondEarrings.Id,
            //    },
            //    new RelatedProduct()
            //    {
            //         ProductId1 = productEngagementRing.Id,
            //         ProductId2 = productDiamondHeart.Id,
            //    },
            //    new RelatedProduct()
            //    {
            //         ProductId1 = productEngagementRing.Id,
            //         ProductId2 = productDiamondBracelet.Id,
            //    },
            //    new RelatedProduct()
            //    {
            //         ProductId1 = productEngagementRing.Id,
            //         ProductId2 = productDiamondEarrings.Id,
            //    },
            //    new RelatedProduct()
            //    {
            //         ProductId1 = productDiamondEarrings.Id,
            //         ProductId2 = productDiamondHeart.Id,
            //    },
            //    new RelatedProduct()
            //    {
            //         ProductId1 = productDiamondEarrings.Id,
            //         ProductId2 = productDiamondBracelet.Id,
            //    },
            //    new RelatedProduct()
            //    {
            //         ProductId1 = productDiamondEarrings.Id,
            //         ProductId2 = productEngagementRing.Id,
            //    },
            //    new RelatedProduct()
            //    {
            //         ProductId1 = productSingleLadies.Id,
            //         ProductId2 = productPokerFace.Id,
            //    },
            //    new RelatedProduct()
            //    {
            //         ProductId1 = productSingleLadies.Id,
            //         ProductId2 = productBattleOfLa.Id,
            //    },
            //    new RelatedProduct()
            //    {
            //         ProductId1 = productPokerFace.Id,
            //         ProductId2 = productSingleLadies.Id,
            //    },
            //    new RelatedProduct()
            //    {
            //         ProductId1 = productPokerFace.Id,
            //         ProductId2 = productBattleOfLa.Id,
            //    },
            //    new RelatedProduct()
            //    {
            //         ProductId1 = productBestSkilletRecipes.Id,
            //         ProductId2 = productCookingForTwo.Id,
            //    },
            //    new RelatedProduct()
            //    {
            //         ProductId1 = productBestSkilletRecipes.Id,
            //         ProductId2 = productEatingWell.Id,
            //    },
            //    new RelatedProduct()
            //    {
            //         ProductId1 = productBestSkilletRecipes.Id,
            //         ProductId2 = productBestGrillingRecipes.Id,
            //    },
            //    new RelatedProduct()
            //    {
            //         ProductId1 = productCookingForTwo.Id,
            //         ProductId2 = productBestSkilletRecipes.Id,
            //    },
            //    new RelatedProduct()
            //    {
            //         ProductId1 = productCookingForTwo.Id,
            //         ProductId2 = productEatingWell.Id,
            //    },
            //    new RelatedProduct()
            //    {
            //         ProductId1 = productCookingForTwo.Id,
            //         ProductId2 = productBestGrillingRecipes.Id,
            //    },
            //    new RelatedProduct()
            //    {
            //         ProductId1 = productEatingWell.Id,
            //         ProductId2 = productBestSkilletRecipes.Id,
            //    },
            //    new RelatedProduct()
            //    {
            //         ProductId1 = productEatingWell.Id,
            //         ProductId2 = productCookingForTwo.Id,
            //    },
            //    new RelatedProduct()
            //    {
            //         ProductId1 = productEatingWell.Id,
            //         ProductId2 = productBestGrillingRecipes.Id,
            //    },
            //    new RelatedProduct()
            //    {
            //         ProductId1 = productBestGrillingRecipes.Id,
            //         ProductId2 = productCookingForTwo.Id,
            //    },
            //    new RelatedProduct()
            //    {
            //         ProductId1 = productBestGrillingRecipes.Id,
            //         ProductId2 = productEatingWell.Id,
            //    },
            //    new RelatedProduct()
            //    {
            //         ProductId1 = productBestGrillingRecipes.Id,
            //         ProductId2 = productBestSkilletRecipes.Id,
            //    },
            //    new RelatedProduct()
            //    {
            //         ProductId1 = productAsusPc900.Id,
            //         ProductId2 = productSatellite.Id,
            //    },
            //    new RelatedProduct()
            //    {
            //         ProductId1 = productAsusPc900.Id,
            //         ProductId2 = productAsusPc1000.Id,
            //    },
            //    new RelatedProduct()
            //    {
            //         ProductId1 = productAsusPc900.Id,
            //         ProductId2 = productHpPavilion1.Id,
            //    },
            //    new RelatedProduct()
            //    {
            //         ProductId1 = productSatellite.Id,
            //         ProductId2 = productAsusPc900.Id,
            //    },
            //    new RelatedProduct()
            //    {
            //         ProductId1 = productSatellite.Id,
            //         ProductId2 = productAsusPc1000.Id,
            //    },
            //    new RelatedProduct()
            //    {
            //         ProductId1 = productSatellite.Id,
            //         ProductId2 = productAcerAspireOne.Id,
            //    },
            //    new RelatedProduct()
            //    {
            //         ProductId1 = productAsusPc1000.Id,
            //         ProductId2 = productSatellite.Id,
            //    },
            //    new RelatedProduct()
            //    {
            //         ProductId1 = productAsusPc1000.Id,
            //         ProductId2 = productHpPavilion1.Id,
            //    },
            //    new RelatedProduct()
            //    {
            //         ProductId1 = productAsusPc1000.Id,
            //         ProductId2 = productAcerAspireOne.Id,
            //    },
            //    new RelatedProduct()
            //    {
            //         ProductId1 = productHpPavilion3.Id,
            //         ProductId2 = productAsusPc900.Id,
            //    },
            //    new RelatedProduct()
            //    {
            //         ProductId1 = productHpPavilion3.Id,
            //         ProductId2 = productAsusPc1000.Id,
            //    },
            //    new RelatedProduct()
            //    {
            //         ProductId1 = productHpPavilion3.Id,
            //         ProductId2 = productAcerAspireOne.Id,
            //    },
            //    new RelatedProduct()
            //    {
            //         ProductId1 = productHpPavilion1.Id,
            //         ProductId2 = productAsusPc900.Id,
            //    },
            //    new RelatedProduct()
            //    {
            //         ProductId1 = productHpPavilion1.Id,
            //         ProductId2 = productAsusPc1000.Id,
            //    },
            //    new RelatedProduct()
            //    {
            //         ProductId1 = productHpPavilion1.Id,
            //         ProductId2 = productAcerAspireOne.Id,
            //    },
            //    new RelatedProduct()
            //    {
            //         ProductId1 = productCanonCamcoder.Id,
            //         ProductId2 = productSamsungPhone.Id,
            //    },
            //    new RelatedProduct()
            //    {
            //         ProductId1 = productCanonCamcoder.Id,
            //         ProductId2 = productSonyCamcoder.Id,
            //    },
            //    new RelatedProduct()
            //    {
            //         ProductId1 = productCanonCamcoder.Id,
            //         ProductId2 = productCanonCamera.Id,
            //    },
            //    new RelatedProduct()
            //    {
            //         ProductId1 = productSonyCamcoder.Id,
            //         ProductId2 = productSamsungPhone.Id,
            //    },
            //    new RelatedProduct()
            //    {
            //         ProductId1 = productSonyCamcoder.Id,
            //         ProductId2 = productCanonCamcoder.Id,
            //    },
            //    new RelatedProduct()
            //    {
            //         ProductId1 = productSonyCamcoder.Id,
            //         ProductId2 = productCanonCamera.Id,
            //    },
            //};


            //relatedProducts.ForEach(rp => _relatedProductRepository.Insert(rp));
            #endregion oldcode relatedproducts

            #region oldcode producttag
            ////product tags
            //AddProductTag(product25GiftCard, "nice");
            //AddProductTag(product25GiftCard, "gift");
            //AddProductTag(product5GiftCard, "nice");
            //AddProductTag(product5GiftCard, "gift");
            //AddProductTag(productRockabillyPolka, "cool");
            //AddProductTag(productRockabillyPolka, "apparel");
            //AddProductTag(productRockabillyPolka, "shirt");
            //AddProductTag(productAcerAspireOne, "computer");
            //AddProductTag(productAcerAspireOne, "cool");
            //AddProductTag(productAdidasShoe, "cool");
            //AddProductTag(productAdidasShoe, "shoes");
            //AddProductTag(productAdidasShoe, "apparel");
            //AddProductTag(productAdobePhotoshop, "computer");
            //AddProductTag(productAdobePhotoshop, "awesome");
            //AddProductTag(productApcUps, "computer");
            //AddProductTag(productApcUps, "cool");
            //AddProductTag(productArrow, "cool");
            //AddProductTag(productArrow, "apparel");
            //AddProductTag(productArrow, "shirt");
            //AddProductTag(productAsusPc1000, "compact");
            //AddProductTag(productAsusPc1000, "awesome");
            //AddProductTag(productAsusPc1000, "computer");
            //AddProductTag(productAsusPc900, "compact");
            //AddProductTag(productAsusPc900, "awesome");
            //AddProductTag(productAsusPc900, "computer");
            //AddProductTag(productBestGrillingRecipes, "awesome");
            //AddProductTag(productBestGrillingRecipes, "book");
            //AddProductTag(productBestGrillingRecipes, "nice");
            //AddProductTag(productDiamondHeart, "awesome");
            //AddProductTag(productDiamondHeart, "jewelry");
            //AddProductTag(productBlackBerry, "cell");
            //AddProductTag(productBlackBerry, "compact");
            //AddProductTag(productBlackBerry, "awesome");
            //AddProductTag(productBuildComputer, "awesome");
            //AddProductTag(productBuildComputer, "computer");
            //AddProductTag(productCanonCamera, "cool");
            //AddProductTag(productCanonCamera, "camera");
            //AddProductTag(productCanonCamcoder, "camera");
            //AddProductTag(productCanonCamcoder, "cool");
            //AddProductTag(productCompaq, "cool");
            //AddProductTag(productCompaq, "computer");
            //AddProductTag(productCookingForTwo, "awesome");
            //AddProductTag(productCookingForTwo, "book");
            //AddProductTag(productCorel, "awesome");
            //AddProductTag(productCorel, "computer");
            //AddProductTag(productCustomTShirt, "cool");
            //AddProductTag(productCustomTShirt, "shirt");
            //AddProductTag(productCustomTShirt, "apparel");
            //AddProductTag(productDiamondEarrings, "jewelry");
            //AddProductTag(productDiamondEarrings, "awesome");
            //AddProductTag(productDiamondBracelet, "awesome");
            //AddProductTag(productDiamondBracelet, "jewelry");
            //AddProductTag(productEatingWell, "book");
            //AddProductTag(productEtnies, "cool");
            //AddProductTag(productEtnies, "shoes");
            //AddProductTag(productEtnies, "apparel");
            //AddProductTag(productLeatherHandbag, "apparel");
            //AddProductTag(productLeatherHandbag, "cool");
            //AddProductTag(productLeatherHandbag, "awesome");
            //AddProductTag(productHp506, "awesome");
            //AddProductTag(productHp506, "computer");
            //AddProductTag(productHpPavilion1, "nice");
            //AddProductTag(productHpPavilion1, "computer");
            //AddProductTag(productHpPavilion1, "compact");
            //AddProductTag(productHpPavilion2, "nice");
            //AddProductTag(productHpPavilion2, "computer");
            //AddProductTag(productHpPavilion3, "computer");
            //AddProductTag(productHpPavilion3, "cool");
            //AddProductTag(productHpPavilion3, "compact");
            //AddProductTag(productHat, "apparel");
            //AddProductTag(productHat, "cool");
            //AddProductTag(productKensington, "computer");
            //AddProductTag(productKensington, "cool");
            //AddProductTag(productLeviJeans, "cool");
            //AddProductTag(productLeviJeans, "jeans");
            //AddProductTag(productLeviJeans, "apparel");
            //AddProductTag(productBaseball, "game");
            //AddProductTag(productBaseball, "computer");
            //AddProductTag(productBaseball, "cool");
            //AddProductTag(productPokerFace, "awesome");
            //AddProductTag(productPokerFace, "digital");
            //AddProductTag(productSunglasses, "apparel");
            //AddProductTag(productSunglasses, "cool");
            //AddProductTag(productSamsungPhone, "awesome");
            //AddProductTag(productSamsungPhone, "compact");
            //AddProductTag(productSamsungPhone, "cell");
            //AddProductTag(productSingleLadies, "digital");
            //AddProductTag(productSingleLadies, "awesome");
            //AddProductTag(productSonyCamcoder, "awesome");
            //AddProductTag(productSonyCamcoder, "cool");
            //AddProductTag(productSonyCamcoder, "camera");
            //AddProductTag(productBattleOfLa, "digital");
            //AddProductTag(productBattleOfLa, "awesome");
            //AddProductTag(productBestSkilletRecipes, "book");
            //AddProductTag(productSatellite, "awesome");
            //AddProductTag(productSatellite, "computer");
            //AddProductTag(productSatellite, "compact");
            //AddProductTag(productDenimShort, "jeans");
            //AddProductTag(productDenimShort, "cool");
            //AddProductTag(productDenimShort, "apparel");
            //AddProductTag(productEngagementRing, "jewelry");
            //AddProductTag(productEngagementRing, "awesome");
            //AddProductTag(productWoW, "computer");
            //AddProductTag(productWoW, "cool");
            //AddProductTag(productWoW, "game");
            //AddProductTag(productSoccer, "game");
            //AddProductTag(productSoccer, "cool");
            //AddProductTag(productSoccer, "computer");

            #endregion oldcode producttag

            #region new code
            try
            {
                var products = _installData.Products();
                products.Each(x => 
                    {
                        _productRepository.Insert(x);
                        //IncreaseProgress();   
                    });
                //search engine names
                products.Each(x =>
                {
                    _urlRecordRepository.Insert(new UrlRecord()
                    {
                        EntityId = x.Id,
                        EntityName = "Product",
                        LanguageId = 0,
                        Slug = x.ValidateSeName("", x.Name, true),
                        IsActive = true
                    });
                });
                IncreaseProgress();
            }
            catch (Exception ex)
            {
                throw new InstallationException("InstallProducts", ex);
            }
            #endregion new code

        }
        


        protected virtual void InstallForums()
        {
            #region oldcode

            //var forumGroup = new ForumGroup()
            //{
            //    Name = "General",
            //    Description = "",
            //    DisplayOrder = 5,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow,
            //};

            //_forumGroupRepository.Insert(forumGroup);

            //var newProductsForum = new Forum()
            //{
            //    ForumGroup = forumGroup,
            //    Name = "New Products",
            //    Description = "Discuss new products and industry trends",
            //    NumTopics = 0,
            //    NumPosts = 0,
            //    LastPostCustomerId = 0,
            //    LastPostTime = null,
            //    DisplayOrder = 1,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow,
            //};
            //_forumRepository.Insert(newProductsForum);

            //var mobileDevicesForum = new Forum()
            //{
            //    ForumGroup = forumGroup,
            //    Name = "Mobile Devices Forum",
            //    Description = "Discuss the mobile phone market",
            //    NumTopics = 0,
            //    NumPosts = 0,
            //    LastPostCustomerId = 0,
            //    LastPostTime = null,
            //    DisplayOrder = 10,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow,
            //};
            //_forumRepository.Insert(mobileDevicesForum);

            //var packagingShippingForum = new Forum()
            //{
            //    ForumGroup = forumGroup,
            //    Name = "Packaging & Shipping",
            //    Description = "Discuss packaging & shipping",
            //    NumTopics = 0,
            //    NumPosts = 0,
            //    LastPostTime = null,
            //    DisplayOrder = 20,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow,
            //};
            //_forumRepository.Insert(packagingShippingForum);

            #endregion oldcode

            try
            {
                _installData.ForumGroups().Each(x => _forumGroupRepository.Insert(x));
                _installData.Forums().Each(x => _forumRepository.Insert(x));
                IncreaseProgress();
            }
            catch (Exception ex)
            {
                throw new InstallationException("InstallForums", ex);
            }
        }

        protected virtual void InstallDiscounts()
        {
            #region oldcode
            //var discounts = new List<Discount>
            //                    {
            //                        new Discount
            //                            {
            //                                Name = "Sample discount with coupon code",
            //                                DiscountType = DiscountType.AssignedToSkus,
            //                                DiscountLimitation = DiscountLimitationType.Unlimited,
            //                                UsePercentage = false,
            //                                DiscountAmount = 10,
            //                                RequiresCouponCode = true,
            //                                CouponCode = "123",
            //                            },
            //                        new Discount
            //                            {
            //                                Name = "'20% order total' discount",
            //                                DiscountType = DiscountType.AssignedToOrderTotal,
            //                                DiscountLimitation = DiscountLimitationType.Unlimited,
            //                                UsePercentage = true,
            //                                DiscountPercentage = 20,
            //                                StartDateUtc = new DateTime(2010,1,1),
            //                                EndDateUtc = new DateTime(2020,1,1),
            //                                RequiresCouponCode = true,
            //                                CouponCode = "456",
            //                            },
            //                    };
            //discounts.ForEach(d => _discountRepository.Insert(d));
            #endregion oldcode

            try
            {
                _installData.Discounts().Each(x => _discountRepository.Insert(x));
                IncreaseProgress();
            }
            catch (Exception ex)
            {
                throw new InstallationException("InstallDiscounts", ex);
            }

        }

        protected virtual void InstallDeliveryTimes()
        {
            
            try
            {
                _installData.DeliveryTimes().Each(x => _deliveryTimeRepository.Insert(x));
                IncreaseProgress();
            }
            catch (Exception ex)
            {
                throw new InstallationException("InstallDeliveryTimes", ex);
            }

        }



        protected virtual void InstallBlogPosts()
        {
            #region oldcode

            //var defaultLanguage = _languageRepository.Table.FirstOrDefault();
            //var blogPosts = new List<BlogPost>
            //                    {
            //                        new BlogPost
            //                            {
            //                                 AllowComments = true,
            //                                 Language = defaultLanguage,
            //                                 Title = "Online Discount Coupons",
            //                                 Body = "<p>Online discount coupons enable access to great offers from some of the world&rsquo;s best sites for Internet shopping. The online coupons are designed to allow compulsive online shoppers to access massive discounts on a variety of products. The regular shopper accesses the coupons in bulk and avails of great festive offers and freebies thrown in from time to time.  The coupon code option is most commonly used when using a shopping cart. The coupon code is entered on the order page just before checking out. Every online shopping resource has a discount coupon submission option to confirm the coupon code. The dedicated web sites allow the shopper to check whether or not a discount is still applicable. If it is, the sites also enable the shopper to calculate the total cost after deducting the coupon amount like in the case of grocery coupons.  Online discount coupons are very convenient to use. They offer great deals and professionally negotiated rates if bought from special online coupon outlets. With a little research and at times, insider knowledge the online discount coupons are a real steal. They are designed to promote products by offering &lsquo;real value for money&rsquo; packages. The coupons are legitimate and help with budgeting, in the case of a compulsive shopper. They are available for special trade show promotions, nightlife, sporting events and dinner shows and just about anything that could be associated with the promotion of a product. The coupons enable the online shopper to optimize net access more effectively. Getting a &lsquo;big deal&rsquo; is not more utopian amidst rising prices. The online coupons offer internet access to the best and cheapest products displayed online. Big discounts are only a code away! By Gaynor Borade (buzzle.com)</p>",
            //                                 Tags = "e-commerce, money",
            //                                 CreatedOnUtc = DateTime.UtcNow,
            //                            },
            //                        new BlogPost
            //                            {
            //                                 AllowComments = true,
            //                                 Language = defaultLanguage,
            //                                 Title = "Customer Service - Client Service",
            //                                 Body = "<p>Managing online business requires different skills and abilities than managing a business in the &lsquo;real world.&rsquo; Customers can easily detect the size and determine the prestige of a business when they have the ability to walk in and take a look around. Not only do &lsquo;real-world&rsquo; furnishings and location tell the customer what level of professionalism to expect, but &quot;real world&quot; personal encounters allow first impressions to be determined by how the business approaches its customer service. When a customer walks into a retail business just about anywhere in the world, that customer expects prompt and personal service, especially with regards to questions that they may have about products they wish to purchase.<br /><br />Customer service or the client service is the service provided to the customer for his satisfaction during and after the purchase. It is necessary to every business organization to understand the customer needs for value added service. So customer data collection is essential. For this, a good customer service is important. The easiest way to lose a client is because of the poor customer service. The importance of customer service changes by product, industry and customer. Client service is an important part of every business organization. Each organization is different in its attitude towards customer service. Customer service requires a superior quality service through a careful design and execution of a series of activities which include people, technology and processes. Good customer service starts with the design and communication between the company and the staff.<br /><br />In some ways, the lack of a physical business location allows the online business some leeway that their &lsquo;real world&rsquo; counterparts do not enjoy. Location is not important, furnishings are not an issue, and most of the visual first impression is made through the professional design of the business website.<br /><br />However, one thing still remains true. Customers will make their first impressions on the customer service they encounter. Unfortunately, in online business there is no opportunity for front- line staff to make a good impression. Every interaction the customer has with the website will be their primary means of making their first impression towards the business and its client service. Good customer service in any online business is a direct result of good website design and planning.</p><p>By Jayashree Pakhare (buzzle.com)</p>",
            //                                 Tags = "e-commerce, SmartStore.NET, asp.net, sample tag, money",
            //                                 CreatedOnUtc = DateTime.UtcNow.AddSeconds(1),
            //                            },
            //                    };
            //blogPosts.ForEach(bp => _blogPostRepository.Insert(bp));

            ////search engine names
            //foreach (var blogPost in blogPosts)
            //{
            //    _urlRecordRepository.Insert(new UrlRecord()
            //    {
            //        EntityId = blogPost.Id,
            //        EntityName = "BlogPost",
            //        LanguageId = blogPost.LanguageId,
            //        Slug = blogPost.ValidateSeName("", blogPost.Title, true)
            //    });
            //}

            #endregion oldcode

            try
            {
                var blogPosts = _installData.BlogPosts();
                blogPosts.Each(x => _blogPostRepository.Insert(x));
                //search engine names
                blogPosts.Each(x =>
                {
                    _urlRecordRepository.Insert(new UrlRecord()
                    {
                        EntityId = x.Id,
                        EntityName = "BlogPost",
                        LanguageId = x.LanguageId,
                        Slug = x.ValidateSeName("", x.Title, true),
                        IsActive = true
                    });
                });
                IncreaseProgress();
            }
            catch (Exception ex)
            {
                throw new InstallationException("InstallBlogPosts", ex);
            }
        }

        protected virtual void InstallNews()
        {
            try
            {
                var newsItems = _installData.NewsItems();
                newsItems.Each(x => _newsItemRepository.Insert(x));
                //search engine names
                newsItems.Each(x =>
                {
                    _urlRecordRepository.Insert(new UrlRecord()
                    {
                        EntityId = x.Id,
                        EntityName = "NewsItem",
                        LanguageId = x.LanguageId,
                        IsActive = true,
                        Slug = x.ValidateSeName("", x.Title, true)
                    });
                });
                IncreaseProgress();
            }
            catch (Exception ex)
            {
                throw new InstallationException("InstallNews", ex);
            }
        }

        protected virtual void InstallPolls()
        {
            #region oldcode
            //var defaultLanguage = _languageRepository.Table.FirstOrDefault();
            //var poll1 = new Poll
            //{
            //    Language = defaultLanguage,
            //    Name = "Do you like SmartStore.NET?",
            //    SystemKeyword = "RightColumnPoll",
            //    Published = true,
            //    DisplayOrder = 1,
            //};
            //poll1.PollAnswers.Add(new PollAnswer()
            //{
            //    Name = "Excellent",
            //    DisplayOrder = 1,
            //});
            //poll1.PollAnswers.Add(new PollAnswer()
            //{
            //    Name = "Good",
            //    DisplayOrder = 2,
            //});
            //poll1.PollAnswers.Add(new PollAnswer()
            //{
            //    Name = "Poor",
            //    DisplayOrder = 3,
            //});
            //poll1.PollAnswers.Add(new PollAnswer()
            //{
            //    Name = "Very bad",
            //    DisplayOrder = 4,
            //});
            //_pollRepository.Insert(poll1);

            #endregion oldcode
            
            try
            {
                _installData.Polls().Each(x => _pollRepository.Insert(x));
                IncreaseProgress();
            }
            catch (Exception ex)
            {
                throw new InstallationException("InstallPolls", ex);
            }

        }

        protected virtual void InstallActivityLogTypes()
        { 
            try 
            {
            _activityLogTypeRepository.InsertRange( _installData.ActivityLogTypes());
            IncreaseProgress();

            }
            catch (Exception ex)
            {
                throw new InstallationException("InstallActivityLogTypes", ex);
            }
        }

        protected virtual void InstallProductTemplates()
        {
            try
            {
                _productTemplateRepository.InsertRange(_installData.ProductTemplates());
                IncreaseProgress();
            }
            catch (Exception ex)
            {
                throw new InstallationException("InstallProductTemplates", ex);
            }
        }

        protected virtual void InstallCategoryTemplates()
        {
            try
            {
            _categoryTemplateRepository.InsertRange(_installData.CategoryTemplates());
            IncreaseProgress();
            }
            catch (Exception ex)
            {
                throw new InstallationException("InstallCategoryTemplates", ex);
            }
        }

        protected virtual void InstallManufacturerTemplates()
        {
            try
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
                _manufacturerTemplateRepository.InsertRange(_installData.ManufacturerTemplates());
                IncreaseProgress();
            }
            catch (Exception ex)
            {
                throw new InstallationException("InstallManufacturerTemplates", ex);
            }
        }

        protected virtual void InstallScheduleTasks()
        {
           try
           {
                _scheduleTaskRepository.InsertRange(_installData.ScheduleTasks());
                IncreaseProgress();
           }
           catch (Exception ex)
           {
                throw new InstallationException("InstallScheduleTasks", ex);
           }
        }

        protected virtual void InstallProductTags()
        {
            try
            {
                _productTagRepository.InsertRange(_installData.ProductTags());
                IncreaseProgress();
            }
            catch (Exception ex)
            {
                throw new InstallationException("InstallProductTags", ex);
            }
        }

        private void AddProductTag(Product product, string tag)
        {
            try
            {
				var productTag = _productTagRepository.Table.FirstOrDefault(pt => pt.Name == tag);
                if (productTag == null)
                {
                    productTag = new ProductTag()
                    {
                        Name = tag
                    };
                }
				product.ProductTags.Add(productTag);
				_productRepository.Update(product);

                IncreaseProgress();
            }
            catch (Exception ex)
            {
                throw new InstallationException("AddProductTag", ex);
            }
        }

        #endregion

        #region Properties

        protected InstallDataContext Context
        {
            get
            {
                return _installContext;
            }
        }

        #endregion Properties

        #region Methods

		public virtual void InstallEarlyRequiredData()
		{
			InstallStores();
		}

        public virtual void InstallData(InstallDataContext context)
        {
            Guard.ArgumentNotNull(context.Language, "Language");
            Guard.ArgumentNotNull(context.InstallData, "InstallData");

            _dbContext.AutoDetectChangesEnabled = false;
            _dbContext.ValidateOnSaveEnabled = false;
            
            _installContext = context;
            _installData = context.InstallData;

            _totalSteps = context.InstallSampleData ? 28 : 18;

            // special mandatory (non-visible) settings
            _settingService.SetSetting<bool>("Media.Images.StoreInDB", _installContext.StoreMediaInDB);

			UpdateStores();
            InstallLanguages(context.Language);
            InstallMeasureDimensions();
            InstallMeasureWeights();
            InstallTaxCategories();
            InstallCurrencies();
            InstallCountriesAndStates();
            InstallShippingMethods();
            InstallDeliveryTimes();
            InstallCustomersAndUsers(context.DefaultUserName, context.DefaultUserPassword);
            InstallEmailAccounts();
            InstallMessageTemplates();
            InstallTopics();
            InstallSettings();
            InstallLocaleResources();
            InstallActivityLogTypes();
            HashDefaultCustomerPassword(context.DefaultUserName, context.DefaultUserPassword); // no progress
            InstallProductTemplates();
            InstallCategoryTemplates();
            InstallManufacturerTemplates();
            InstallScheduleTasks();
            

            if (context.InstallSampleData)
            {
                InstallSpecificationAttributes();
                InstallProductAttributes();
                InstallCategories();
                InstallManufacturers();
                InstallProducts();
                InstallProductTags();
                InstallForums();
                InstallDiscounts();
                InstallBlogPosts();
                InstallNews();
                InstallPolls();
            }

            _dbContext.AutoDetectChangesEnabled = true;
            _dbContext.ValidateOnSaveEnabled = true;

        }

        #endregion methods
    }
        
} 
