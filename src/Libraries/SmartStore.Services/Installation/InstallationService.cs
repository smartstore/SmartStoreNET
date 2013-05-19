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
using SmartStore.Data;
using SmartStore.Core.Caching;
using SmartStore.Utilities.Threading;

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
            IDbContext context, // codehint: sm-add
            ISettingService settingService, // codehint: sm-add
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
            this._dbContext = context; // codehint: sm-add
            this._settingService = settingService; // codehint: sm-add
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

        #region Classes

        private class LocaleStringResourceParent : LocaleStringResource
        {
            public LocaleStringResourceParent(XmlNode localStringResource, string nameSpace = "")
            {
                Namespace = nameSpace;
                var resNameAttribute = localStringResource.Attributes["Name"];
                var resValueNode = localStringResource.SelectSingleNode("Value");

                if (resNameAttribute == null)
                {
                    throw new SmartException("All language resources must have an attribute Name=\"Value\".");
                }
                var resName = resNameAttribute.Value.Trim();
                if (string.IsNullOrEmpty(resName))
                {
                    throw new SmartException("All languages resource attributes 'Name' must have a value.'");
                }
                ResourceName = resName;

                if (resValueNode == null || string.IsNullOrEmpty(resValueNode.InnerText.Trim()))
                {
                    IsPersistable = false;
                }
                else
                {
                    IsPersistable = true;
                    ResourceValue = resValueNode.InnerText.Trim();
                }

                foreach (XmlNode childResource in localStringResource.SelectNodes("Children/LocaleResource"))
                {
                    ChildLocaleStringResources.Add(new LocaleStringResourceParent(childResource, NameWithNamespace));
                }
            }
            public string Namespace { get; set; }
            public IList<LocaleStringResourceParent> ChildLocaleStringResources = new List<LocaleStringResourceParent>();

            public bool IsPersistable { get; set; }

            public string NameWithNamespace
            {
                get
                {
                    var newNamespace = Namespace;
                    if (!string.IsNullOrEmpty(newNamespace))
                    {
                        newNamespace += ".";
                    }
                    return newNamespace + ResourceName;
                }
            }
        }

        private class ComparisonComparer<T> : IComparer<T>, IComparer
        {
            private readonly Comparison<T> _comparison;

            public ComparisonComparer(Comparison<T> comparison)
            {
                _comparison = comparison;
            }

            public int Compare(T x, T y)
            {
                return _comparison(x, y);
            }

            public int Compare(object o1, object o2)
            {
                return _comparison((T)o1, (T)o2);
            }
        }

        #endregion Classes

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

        private void RecursivelyWriteResource(LocaleStringResourceParent resource, XmlWriter writer)
        {
            //The value isn't actually used, but the name is used to create a namespace.
            if (resource.IsPersistable)
            {
                writer.WriteStartElement("LocaleResource", "");

                writer.WriteStartAttribute("Name", "");
                writer.WriteString(resource.NameWithNamespace);
                writer.WriteEndAttribute();

                writer.WriteStartElement("Value", "");
                writer.WriteString(resource.ResourceValue);
                writer.WriteEndElement();

                writer.WriteEndElement();
            }

            foreach (var child in resource.ChildLocaleStringResources)
            {
                RecursivelyWriteResource(child, writer);
            }

        }

        private void RecursivelySortChildrenResource(LocaleStringResourceParent resource)
        {
            ArrayList.Adapter((IList)resource.ChildLocaleStringResources).Sort(new InstallationService.ComparisonComparer<LocaleStringResourceParent>((x1, x2) => x1.ResourceName.CompareTo(x2.ResourceName)));

            foreach (var child in resource.ChildLocaleStringResources)
            {
                RecursivelySortChildrenResource(child);
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
                // codehint: sm-add
                if (!System.IO.Directory.Exists(locPath))
                {
                    // Fallback to neutral language folder (de, en etc.)
                    locPath = _webHelper.MapPath("~/App_Data/Localization/App/" + language.UniqueSeoCode);
                }

                // save resources
                foreach (var filePath in System.IO.Directory.EnumerateFiles(locPath, "*.smres.xml", SearchOption.TopDirectoryOnly))
                {
                    #region Parse resource files (with <Children> elements)
                    //read and parse original file with resources (with <Children> elements)

                    var originalXmlDocument = new XmlDocument();
                    originalXmlDocument.Load(filePath);

                    var resources = new List<LocaleStringResourceParent>();

                    foreach (XmlNode resNode in originalXmlDocument.SelectNodes(@"//Language/LocaleResource"))
                        resources.Add(new LocaleStringResourceParent(resNode));

                    resources.Sort((x1, x2) => x1.ResourceName.CompareTo(x2.ResourceName));

                    foreach (var resource in resources)
                        RecursivelySortChildrenResource(resource);

                    var sb = new StringBuilder();
                    var writer = XmlWriter.Create(sb);
                    writer.WriteStartDocument();
                    writer.WriteStartElement("Language", "");

                    writer.WriteStartAttribute("Name", "");
                    writer.WriteString(originalXmlDocument.SelectSingleNode(@"//Language").Attributes["Name"].InnerText.Trim());
                    writer.WriteEndAttribute();

                    foreach (var resource in resources)
                        RecursivelyWriteResource(resource, writer);

                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                    writer.Flush();

                    var parsedXml = sb.ToString();
                    
                    #endregion Parse resource files (with <Children> elements)

                    // now we have a parsed XML file (the same structure as exported language packs)
                    // let's save resources
                    var localizationService = EngineContext.Current.Resolve<ILocalizationService>();
                    localizationService.ImportResourcesFromXml(language, parsedXml);
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
                var settings = _installData.Settings();
                foreach (var setting in settings)
                {
                    Type settingType = setting.GetType();
                    Type configProviderGenericType = typeof(IConfigurationProvider<>).MakeGenericType(settingType);

                    var configProvider = EngineContext.Current.Resolve(configProviderGenericType);
                    if (configProvider != null)
                    {
                        // call "SaveSettings" with reflection, as we have no strong typing
                        // and thus no intellisense.
                        configProvider.GetType().GetMethod("SaveSettings").Invoke(configProvider, new object[] { setting });
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
            #region oldcode
            ////pictures
            //var pictureService = EngineContext.Current.Resolve<IPictureService>();
            //var sampleImagesPath = _webHelper.MapPath("~/content/samples/");

            //var categoryTemplateInGridAndLines =
            //    _categoryTemplateRepository.Table.Where(pt => pt.Name == "Products in Grid or Lines").FirstOrDefault();

           
            ////categories
            //var allCategories = new List<Category>();
            //var categoryBooks = new Category
            //{
            //    Name = "Books",
            //    CategoryTemplateId = categoryTemplateInGridAndLines.Id,
            //    MetaKeywords = "Books, Dictionary, Textbooks",
            //    MetaDescription = "Books category description",
            //    PageSize = 12,
            //    AllowCustomersToSelectPageSize = true,
            //    //PageSizeOptions = "4, 2, 8, 12",
            //    PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "category_book.jpeg"), "image/jpeg", pictureService.GetPictureSeName("Book"), true).Id,
            //    PriceRanges = "-25;25-50;50-;",
            //    Published = true,
            //    DisplayOrder = 1,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow
            //};
            //allCategories.Add(categoryBooks);
            //_categoryRepository.Insert(categoryBooks);

            //var categoryComputers = new Category
            //{
            //    Name = "Computers",
            //    CategoryTemplateId = categoryTemplateInGridAndLines.Id,
            //    PageSize = 12,
            //    AllowCustomersToSelectPageSize = true,
            //    //PageSizeOptions = "4, 2, 8, 12",
            //    PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "category_computers.jpeg"), "image/jpeg", pictureService.GetPictureSeName("Computers"), true).Id,
            //    Published = true,
            //    DisplayOrder = 2,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow
            //};
            //allCategories.Add(categoryComputers);
            //_categoryRepository.Insert(categoryComputers);


            //var categoryDesktops = new Category
            //{
            //    Name = "Desktops",
            //    CategoryTemplateId = categoryTemplateInGridAndLines.Id,
            //    PageSize = 12,
            //    AllowCustomersToSelectPageSize = true,
            //    //PageSizeOptions = "4, 2, 8, 12",
            //    ParentCategoryId = categoryComputers.Id,
            //    PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "category_desktops.jpg"), "image/pjpeg", pictureService.GetPictureSeName("Desktops"), true).Id,
            //    PriceRanges = "-1000;1000-1200;1200-;",
            //    Published = true,
            //    DisplayOrder = 1,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow
            //};
            //allCategories.Add(categoryDesktops);
            //_categoryRepository.Insert(categoryDesktops);


            //var categoryNotebooks = new Category
            //{
            //    Name = "Notebooks",
            //    CategoryTemplateId = categoryTemplateInGridAndLines.Id,
            //    PageSize = 12,
            //    AllowCustomersToSelectPageSize = true,
            //    //PageSizeOptions = "4, 2, 8, 12",
            //    ParentCategoryId = categoryComputers.Id,
            //    PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "category_notebooks.jpg"), "image/pjpeg", pictureService.GetPictureSeName("Notebooks"), true).Id,
            //    Published = true,
            //    DisplayOrder = 2,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow
            //};
            //allCategories.Add(categoryNotebooks);
            //_categoryRepository.Insert(categoryNotebooks);


            //var categoryAccessories = new Category
            //{
            //    Name = "Accessories",
            //    CategoryTemplateId = categoryTemplateInGridAndLines.Id,
            //    PageSize = 12,
            //    AllowCustomersToSelectPageSize = true,
            //    //PageSizeOptions = "4, 2, 8, 12",
            //    ParentCategoryId = categoryComputers.Id,
            //    PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "category_accessories.jpg"), "image/pjpeg", pictureService.GetPictureSeName("Accessories"), true).Id,
            //    PriceRanges = "-100;100-;",
            //    Published = true,
            //    DisplayOrder = 3,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow
            //};
            //allCategories.Add(categoryAccessories);
            //_categoryRepository.Insert(categoryAccessories);


            //var categorySoftware = new Category
            //{
            //    Name = "Software",
            //    CategoryTemplateId = categoryTemplateInGridAndLines.Id,
            //    PageSize = 12,
            //    AllowCustomersToSelectPageSize = true,
            //    //PageSizeOptions = "4, 2, 8, 12",
            //    ParentCategoryId = categoryComputers.Id,
            //    PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "category_software.jpg"), "image/pjpeg", pictureService.GetPictureSeName("Software"), true).Id,
            //    Published = true,
            //    DisplayOrder = 5,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow
            //};
            //allCategories.Add(categorySoftware);
            //_categoryRepository.Insert(categorySoftware);


            //var categoryGames = new Category
            //{
            //    Name = "Games",
            //    CategoryTemplateId = categoryTemplateInGridAndLines.Id,
            //    PageSize = 12,
            //    AllowCustomersToSelectPageSize = true,
            //    //PageSizeOptions = "4, 2, 8, 12",
            //    ParentCategoryId = categoryComputers.Id,
            //    PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "category_games.jpg"), "image/pjpeg", pictureService.GetPictureSeName("Games"), true).Id,
            //    Published = true,
            //    DisplayOrder = 4,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow
            //};
            //allCategories.Add(categoryGames);
            //_categoryRepository.Insert(categoryGames);



            //var categoryElectronics = new Category
            //{
            //    Name = "Electronics",
            //    CategoryTemplateId = categoryTemplateInGridAndLines.Id,
            //    PageSize = 12,
            //    AllowCustomersToSelectPageSize = true,
            //    //PageSizeOptions = "4, 2, 8, 12",
            //    PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "category_electronics.jpeg"), "image/jpeg", pictureService.GetPictureSeName("Electronics"), true).Id,
            //    Published = true,
            //    DisplayOrder = 3,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow
            //};
            //allCategories.Add(categoryElectronics);
            //_categoryRepository.Insert(categoryElectronics);


            //var categoryCameraPhoto = new Category
            //{
            //    Name = "Camera, photo",
            //    CategoryTemplateId = categoryTemplateInGridAndLines.Id,
            //    PageSize = 12,
            //    AllowCustomersToSelectPageSize = true,
            //    //PageSizeOptions = "4, 2, 8, 12",
            //    ParentCategoryId = categoryElectronics.Id,
            //    PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "category_camera_photo.jpeg"), "image/jpeg", pictureService.GetPictureSeName("Camera, photo"), true).Id,
            //    PriceRanges = "-500;500-;",
            //    Published = true,
            //    DisplayOrder = 2,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow
            //};
            //allCategories.Add(categoryCameraPhoto);
            //_categoryRepository.Insert(categoryCameraPhoto);


            //var categoryCellPhones = new Category
            //{
            //    Name = "Cell phones",
            //    CategoryTemplateId = categoryTemplateInGridAndLines.Id,
            //    PageSize = 12,
            //    AllowCustomersToSelectPageSize = true,
            //    //PageSizeOptions = "4, 2, 8, 12",
            //    ParentCategoryId = categoryElectronics.Id,
            //    PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "category_cell_phones.jpeg"), "image/jpeg", pictureService.GetPictureSeName("Cell phones"), true).Id,
            //    Published = true,
            //    DisplayOrder = 4,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow
            //};
            //allCategories.Add(categoryCellPhones);
            //_categoryRepository.Insert(categoryCellPhones);


            //var categoryApparelShoes = new Category
            //{
            //    Name = "Apparel & Shoes",
            //    CategoryTemplateId = categoryTemplateInGridAndLines.Id,
            //    PageSize = 12,
            //    AllowCustomersToSelectPageSize = true,
            //    //PageSizeOptions = "4, 2, 8, 12",
            //    PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "category_apparel_shoes.jpeg"), "image/jpeg", pictureService.GetPictureSeName("Apparel & Shoes"), true).Id,
            //    Published = true,
            //    DisplayOrder = 5,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow
            //};
            //allCategories.Add(categoryApparelShoes);
            //_categoryRepository.Insert(categoryApparelShoes);


            //var categoryShirts = new Category
            //{
            //    Name = "Shirts",
            //    CategoryTemplateId = categoryTemplateInGridAndLines.Id,
            //    PageSize = 12,
            //    AllowCustomersToSelectPageSize = true,
            //    //PageSizeOptions = "4, 2, 8, 12",
            //    ParentCategoryId = categoryApparelShoes.Id,
            //    PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "category_shirts.jpg"), "image/pjpeg", pictureService.GetPictureSeName("Shirts"), true).Id,
            //    PriceRanges = "-20;20-;",
            //    Published = true,
            //    DisplayOrder = 1,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow
            //};
            //allCategories.Add(categoryShirts);
            //_categoryRepository.Insert(categoryShirts);


            //var categoryJeans = new Category
            //{
            //    Name = "Jeans",
            //    CategoryTemplateId = categoryTemplateInGridAndLines.Id,
            //    PageSize = 12,
            //    AllowCustomersToSelectPageSize = true,
            //    //PageSizeOptions = "4, 2, 8, 12",
            //    ParentCategoryId = categoryApparelShoes.Id,
            //    PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "category_jeans.jpg"), "image/pjpeg", pictureService.GetPictureSeName("Jeans"), true).Id,
            //    PriceRanges = "-20;20-;",
            //    Published = true,
            //    DisplayOrder = 2,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow
            //};
            //allCategories.Add(categoryJeans);
            //_categoryRepository.Insert(categoryJeans);


            //var categoryShoes = new Category
            //{
            //    Name = "Shoes",
            //    CategoryTemplateId = categoryTemplateInGridAndLines.Id,
            //    PageSize = 12,
            //    AllowCustomersToSelectPageSize = true,
            //    //PageSizeOptions = "4, 2, 8, 12",
            //    ParentCategoryId = categoryApparelShoes.Id,
            //    PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "category_shoes.jpg"), "image/pjpeg", pictureService.GetPictureSeName("Shoes"), true).Id,
            //    PriceRanges = "-20;20-;",
            //    Published = true,
            //    DisplayOrder = 3,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow
            //};
            //allCategories.Add(categoryShoes);
            //_categoryRepository.Insert(categoryShoes);


            //var categoryAccessoriesShoes = new Category
            //{
            //    Name = "Apparel accessories",
            //    CategoryTemplateId = categoryTemplateInGridAndLines.Id,
            //    PageSize = 12,
            //    AllowCustomersToSelectPageSize = true,
            //    //PageSizeOptions = "4, 2, 8, 12",
            //    ParentCategoryId = categoryApparelShoes.Id,
            //    PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "category_accessories_apparel.jpg"), "image/pjpeg", pictureService.GetPictureSeName("Apparel accessories"), true).Id,
            //    PriceRanges = "-30;30-;",
            //    Published = true,
            //    DisplayOrder = 4,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow
            //};
            //allCategories.Add(categoryAccessoriesShoes);
            //_categoryRepository.Insert(categoryAccessoriesShoes);


            //var categoryDigitalDownloads = new Category
            //{
            //    Name = "Digital downloads",
            //    CategoryTemplateId = categoryTemplateInGridAndLines.Id,
            //    PageSize = 12,
            //    AllowCustomersToSelectPageSize = true,
            //    //PageSizeOptions = "4, 2, 8, 12",
            //    PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "category_digital_downloads.jpeg"), "image/jpeg", pictureService.GetPictureSeName("Digital downloads"), true).Id,
            //    Published = true,
            //    DisplayOrder = 6,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow
            //};
            //allCategories.Add(categoryDigitalDownloads);
            //_categoryRepository.Insert(categoryDigitalDownloads);


            //var categoryJewelry = new Category
            //{
            //    Name = "Jewelry",
            //    CategoryTemplateId = categoryTemplateInGridAndLines.Id,
            //    PageSize = 12,
            //    AllowCustomersToSelectPageSize = true,
            //    //PageSizeOptions = "4, 2, 8, 12",
            //    PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "category_jewelry.jpeg"), "image/jpeg", pictureService.GetPictureSeName("Jewelry"), true).Id,
            //    PriceRanges = "0-500;500-700;700-3000;",
            //    Published = true,
            //    DisplayOrder = 7,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow
            //};
            //allCategories.Add(categoryJewelry);
            //_categoryRepository.Insert(categoryJewelry);

            //var categoryGiftCards = new Category
            //{
            //    Name = "Gift Cards",
            //    CategoryTemplateId = categoryTemplateInGridAndLines.Id,
            //    PageSize = 12,
            //    AllowCustomersToSelectPageSize = true,
            //    //PageSizeOptions = "4, 2, 8, 12",
            //    PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "category_gift_cards.jpeg"), "image/jpeg", pictureService.GetPictureSeName("Gift Cards"), true).Id,
            //    Published = true,
            //    DisplayOrder = 10,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow
            //};
            //allCategories.Add(categoryGiftCards);
            //_categoryRepository.Insert(categoryGiftCards);
            #endregion oldcode

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

            
            #region oldcode products
            //var productTemplateInGrid =
            //    _productTemplateRepository.Table.Where(pt => pt.Name == "Variants in Grid").FirstOrDefault();
            //var productTemplateSingleVariant =
            //    _productTemplateRepository.Table.Where(pt => pt.Name == "Single Product Variant").FirstOrDefault();

            ////pictures
            //var pictureService = EngineContext.Current.Resolve<IPictureService>();
            //var sampleImagesPath = _webHelper.MapPath("~/content/samples/");

            ////downloads
            //var downloadService = EngineContext.Current.Resolve<IDownloadService>();
            //var sampleDownloadsPath = _webHelper.MapPath("~/content/samples/");


            ////products
            //var allProducts = new List<Product>();
            //var product5GiftCard = new Product()
            //{
            //    Name = "$5 Virtual Gift Card",            
            //    ShortDescription = "$5 Gift Card. Gift Cards must be redeemed through our site Web site toward the purchase of eligible products.",
            //    FullDescription = "<p>Gift Cards must be redeemed through our site Web site toward the purchase of eligible products. Purchases are deducted from the GiftCard balance. Any unused balance will be placed in the recipient's GiftCard account when redeemed. If an order exceeds the amount of the GiftCard, the balance must be paid with a credit card or other available payment method.</p>",
            //    ProductTemplateId = productTemplateSingleVariant.Id,
            //    //SeName = "5-virtual-gift-card",
            //    AllowCustomerReviews = true,
            //    Published = true,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow
            //};
            //allProducts.Add(product5GiftCard);
            //product5GiftCard.ProductVariants.Add(new ProductVariant()
            //{
            //    Price = 5M,
            //    IsGiftCard = true,
            //    GiftCardType = GiftCardType.Virtual,
            //    ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
            //    OrderMinimumQuantity = 1,
            //    OrderMaximumQuantity = 10000,
            //    StockQuantity = 10000,
            //    NotifyAdminForQuantityBelow = 1,
            //    AllowBackInStockSubscriptions = false,
            //    Published = true,
            //    DisplayOrder = 1,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow,
            //});
            //product5GiftCard.ProductCategories.Add(new ProductCategory()
            //{
            //    Category = _categoryRepository.Table.Where(c => c.Name == "Gift Cards").Single(),
            //    DisplayOrder = 1,
            //});
            //product5GiftCard.ProductPictures.Add(new ProductPicture()
            //{
            //    Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_5giftcart.jpeg"), "image/jpeg", pictureService.GetPictureSeName(product5GiftCard.Name), true),
            //    DisplayOrder = 1,
            //});
            //_productRepository.Insert(product5GiftCard);



            //var product25GiftCard = new Product()
            //{
            //    Name = "$25 Virtual Gift Card",
            //    ShortDescription = "$25 Gift Card. Gift Cards must be redeemed through our site Web site toward the purchase of eligible products.",
            //    FullDescription = "<p>Gift Cards must be redeemed through our site Web site toward the purchase of eligible products. Purchases are deducted from the GiftCard balance. Any unused balance will be placed in the recipient's GiftCard account when redeemed. If an order exceeds the amount of the GiftCard, the balance must be paid with a credit card or other available payment method.</p>",
            //    ProductTemplateId = productTemplateSingleVariant.Id,
            //    //SeName = "25-virtual-gift-card",
            //    AllowCustomerReviews = true,
            //    Published = true,
            //    ShowOnHomePage = true,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow
            //};
            //allProducts.Add(product25GiftCard);
            //product25GiftCard.ProductVariants.Add(new ProductVariant()
            //{
            //    Price = 25M,
            //    IsGiftCard = true,
            //    GiftCardType = GiftCardType.Virtual,
            //    ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
            //    OrderMinimumQuantity = 1,
            //    OrderMaximumQuantity = 10000,
            //    StockQuantity = 10000,
            //    NotifyAdminForQuantityBelow = 1,
            //    AllowBackInStockSubscriptions = false,
            //    Published = true,
            //    DisplayOrder = 1,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow,
            //});
            //product25GiftCard.ProductCategories.Add(new ProductCategory()
            //{
            //    Category = _categoryRepository.Table.Where(c => c.Name == "Gift Cards").Single(),
            //    DisplayOrder = 2,
            //});
            //product25GiftCard.ProductPictures.Add(new ProductPicture()
            //{
            //    Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_25giftcart.jpeg"), "image/jpeg", pictureService.GetPictureSeName(product25GiftCard.Name), true),
            //    DisplayOrder = 1,
            //});
            //_productRepository.Insert(product25GiftCard);



            //var product50GiftCard = new Product()
            //{
            //    Name = "$50 Physical Gift Card",
            //    ShortDescription = "$50 Gift Card. Gift Cards must be redeemed through our site Web site toward the purchase of eligible products.",
            //    FullDescription = "<p>Gift Cards must be redeemed through our site Web site toward the purchase of eligible products. Purchases are deducted from the GiftCard balance. Any unused balance will be placed in the recipient's GiftCard account when redeemed. If an order exceeds the amount of the GiftCard, the balance must be paid with a credit card or other available payment method.</p>",
            //    ProductTemplateId = productTemplateSingleVariant.Id,
            //    //SeName = "50-physical-gift-card",
            //    AllowCustomerReviews = true,
            //    Published = true,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow
            //};
            //allProducts.Add(product50GiftCard);
            //product50GiftCard.ProductVariants.Add(new ProductVariant()
            //{
            //    Price = 50M,
            //    IsGiftCard = true,
            //    GiftCardType = GiftCardType.Physical,
            //    IsShipEnabled = true,
            //    Weight = 1,
            //    Length = 1,
            //    Width = 1,
            //    Height = 1,
            //    ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
            //    OrderMinimumQuantity = 1,
            //    OrderMaximumQuantity = 10000,
            //    StockQuantity = 10000,
            //    NotifyAdminForQuantityBelow = 1,
            //    AllowBackInStockSubscriptions = false,
            //    Published = true,
            //    DisplayOrder = 1,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow,
            //});
            //product50GiftCard.ProductCategories.Add(new ProductCategory()
            //{
            //    Category = _categoryRepository.Table.Where(c => c.Name == "Gift Cards").Single(),
            //    DisplayOrder = 3,
            //});
            //product50GiftCard.ProductPictures.Add(new ProductPicture()
            //{
            //    Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_50giftcart.jpeg"), "image/jpeg", pictureService.GetPictureSeName(product50GiftCard.Name), true),
            //    DisplayOrder = 1,
            //});
            //_productRepository.Insert(product50GiftCard);





            //var product100GiftCard = new Product()
            //{
            //    Name = "$100 Physical Gift Card",
            //    ShortDescription = "$100 Gift Card. Gift Cards must be redeemed through our site Web site toward the purchase of eligible products.",
            //    FullDescription = "<p>Gift Cards must be redeemed through our site Web site toward the purchase of eligible products. Purchases are deducted from the GiftCard balance. Any unused balance will be placed in the recipient's GiftCard account when redeemed. If an order exceeds the amount of the GiftCard, the balance must be paid with a credit card or other available payment method.</p>",
            //    ProductTemplateId = productTemplateSingleVariant.Id,
            //    //SeName = "100-physical-gift-card",
            //    AllowCustomerReviews = true,
            //    Published = true,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow
            //};
            //allProducts.Add(product100GiftCard);
            //product100GiftCard.ProductVariants.Add(new ProductVariant()
            //{
            //    Price = 100M,
            //    IsGiftCard = true,
            //    GiftCardType = GiftCardType.Physical,
            //    IsShipEnabled = true,
            //    Weight = 1,
            //    Length = 1,
            //    Width = 1,
            //    Height = 1,
            //    ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
            //    OrderMinimumQuantity = 1,
            //    OrderMaximumQuantity = 10000,
            //    StockQuantity = 10000,
            //    NotifyAdminForQuantityBelow = 1,
            //    AllowBackInStockSubscriptions = false,
            //    Published = true,
            //    DisplayOrder = 1,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow,
            //});
            //product100GiftCard.ProductCategories.Add(new ProductCategory()
            //{
            //    Category = _categoryRepository.Table.Where(c => c.Name == "Gift Cards").Single(),
            //    DisplayOrder = 4,
            //});
            //product100GiftCard.ProductPictures.Add(new ProductPicture()
            //{
            //    Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_100giftcart.jpeg"), "image/jpeg", pictureService.GetPictureSeName(product100GiftCard.Name), true),
            //    DisplayOrder = 1,
            //});
            //_productRepository.Insert(product100GiftCard);





            //var productRockabillyPolka = new Product()
            //{
            //    Name = "50's Rockabilly Polka Dot Top JR Plus Size",
            //    ShortDescription = "",
            //    FullDescription = "<p>Fitted polkadot print cotton top with tie cap sleeves.</p>",
            //    ProductTemplateId = productTemplateInGrid.Id,
            //    //SeName = "50s-rockabilly-polka-dot-top-jr-plus-size",
            //    AllowCustomerReviews = true,
            //    Published = true,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow
            //};
            //allProducts.Add(productRockabillyPolka);
            //productRockabillyPolka.ProductVariants.Add(new ProductVariant()
            //{
            //    Price = 15M,
            //    IsShipEnabled = true,
            //    Weight = 1,
            //    Length = 2,
            //    Width = 3,
            //    Height = 3,
            //    TaxCategoryId = _taxCategoryRepository.Table.Where(tc => tc.Name == "Apparel & Shoes").Single().Id,
            //    ManageInventoryMethod = ManageInventoryMethod.ManageStock,
            //    StockQuantity = 10000,
            //    NotifyAdminForQuantityBelow = 1,
            //    AllowBackInStockSubscriptions = false,
            //    DisplayStockAvailability = true,
            //    LowStockActivity = LowStockActivity.DisableBuyButton,
            //    BackorderMode = BackorderMode.NoBackorders,
            //    OrderMinimumQuantity = 1,
            //    OrderMaximumQuantity = 10000,
            //    Published = true,
            //    DisplayOrder = 1,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow,
            //});
            //var pvaRockabillyPolka1 = new ProductVariantAttribute()
            //{
            //    ProductAttribute = _productAttributeRepository.Table.Where(x => x.Name == "Size").Single(),
            //    AttributeControlType = AttributeControlType.DropdownList,
            //    IsRequired = true,
            //};
            //pvaRockabillyPolka1.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
            //{
            //    Name = "Small",
            //    DisplayOrder = 1,
            //});
            //pvaRockabillyPolka1.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
            //{
            //    Name = "1X",
            //    DisplayOrder = 2,
            //});
            //pvaRockabillyPolka1.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
            //{
            //    Name = "2X",
            //    DisplayOrder = 3,
            //});
            //pvaRockabillyPolka1.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
            //{
            //    Name = "3X",
            //    DisplayOrder = 4,
            //});
            //pvaRockabillyPolka1.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
            //{
            //    Name = "4X",
            //    DisplayOrder = 5,
            //});
            //pvaRockabillyPolka1.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
            //{
            //    Name = "5X",
            //    DisplayOrder = 6,
            //});
            //productRockabillyPolka.ProductVariants.FirstOrDefault().ProductVariantAttributes.Add(pvaRockabillyPolka1);
            //productRockabillyPolka.ProductCategories.Add(new ProductCategory()
            //{
            //    Category = _categoryRepository.Table.Where(c => c.Name == "Shirts").Single(),
            //    DisplayOrder = 1,
            //});
            //productRockabillyPolka.ProductPictures.Add(new ProductPicture()
            //{
            //    Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_RockabillyPolka.jpg"), "image/pjpeg", pictureService.GetPictureSeName(productRockabillyPolka.Name), true),
            //    DisplayOrder = 1,
            //});
            //_productRepository.Insert(productRockabillyPolka);





            //var productAcerAspireOne = new Product()
            //{
            //    Name = "Acer Aspire One 8.9\" Mini-Notebook Case - (Black)",
            //    ShortDescription = "Acer Aspire One 8.9\" Mini-Notebook and 6 Cell Battery model (AOA150-1447)",
            //    FullDescription = "<p>Acer Aspire One 8.9&quot; Memory Foam Pouch is the perfect fit for Acer Aspire One 8.9&quot;. This pouch is made out of premium quality shock absorbing memory form and it provides extra protection even though case is very light and slim. This pouch is water resistant and has internal supporting bands for Acer Aspire One 8.9&quot;. Made In Korea.</p>",
            //    ProductTemplateId = productTemplateSingleVariant.Id,
            //    //SeName = "acer-aspire-one-89-mini-notebook-case-black",
            //    AllowCustomerReviews = true,
            //    Published = true,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow
            //};
            //allProducts.Add(productAcerAspireOne);
            //productAcerAspireOne.ProductVariants.Add(new ProductVariant()
            //{
            //    Price = 21.6M,
            //    IsShipEnabled = true,
            //    Weight = 2,
            //    Length = 2,
            //    Width = 2,
            //    Height = 3,
            //    TaxCategoryId = _taxCategoryRepository.Table.Where(tc => tc.Name == "Electronics & Software").Single().Id,
            //    ManageInventoryMethod = ManageInventoryMethod.ManageStock,
            //    StockQuantity = 10000,
            //    NotifyAdminForQuantityBelow = 1,
            //    AllowBackInStockSubscriptions = false,
            //    DisplayStockAvailability = true,
            //    LowStockActivity = LowStockActivity.DisableBuyButton,
            //    BackorderMode = BackorderMode.NoBackorders,
            //    OrderMinimumQuantity = 1,
            //    OrderMaximumQuantity = 10000,
            //    Published = true,
            //    DisplayOrder = 1,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow,
            //});
            //productAcerAspireOne.ProductVariants.FirstOrDefault().TierPrices.Add(new TierPrice()
            //{
            //    Quantity = 2,
            //    Price = 19
            //});
            //productAcerAspireOne.ProductVariants.FirstOrDefault().TierPrices.Add(new TierPrice()
            //{
            //    Quantity = 5,
            //    Price = 17
            //});
            //productAcerAspireOne.ProductVariants.FirstOrDefault().TierPrices.Add(new TierPrice()
            //{
            //    Quantity = 10,
            //    Price = 15
            //});
            //productAcerAspireOne.ProductVariants.FirstOrDefault().HasTierPrices = true;

            //productAcerAspireOne.ProductCategories.Add(new ProductCategory()
            //{
            //    Category = _categoryRepository.Table.Where(c => c.Name == "Accessories").Single(),
            //    DisplayOrder = 1,
            //});
            //productAcerAspireOne.ProductPictures.Add(new ProductPicture()
            //{
            //    Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_AcerAspireOne_1.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productAcerAspireOne.Name), true),
            //    DisplayOrder = 1,
            //});
            //productAcerAspireOne.ProductPictures.Add(new ProductPicture()
            //{
            //    Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_AcerAspireOne_2.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productAcerAspireOne.Name), true),
            //    DisplayOrder = 2,
            //});
            //_productRepository.Insert(productAcerAspireOne);





            //var productAdidasShoe = new Product()
            //{
            //    Name = "adidas Women's Supernova CSH 7 Running Shoe",
            //    ShortDescription = "Now there are even more reasons to love this training favorite. An improved last, new step-in sockliner and the smooth control of 3-D ForMotion™ deliver a natural, balanced touchdown that feels better than ever.",
            //    FullDescription = "<p>Built to take you far and fast, Adidas Supernova Cushion 7 road-running shoes offer incredible cushioning and comfort with low weight. * Abrasion-resistant nylon mesh uppers are lightweight and highly breathable; synthetic leather overlays create structure and support * GeoFit construction at ankles provides an anatomically correct fit and extra comfort * Nylon linings and molded, antimicrobial dual-layer EVA footbeds dry quickly and fight odor * adiPRENE&reg; midsoles absorb shock in the heels and help maximize heel protection and stability * adiPRENE&reg;+ under forefeet retains natural propulsive forces for improved efficiency * Torsion&reg; system at the midfoot allows natural rotation between the rearfoot and the forefoot, helping improve surface adaptability * ForMotion&reg; freely moving, decoupled heel system allows your feet to adapt to the ground strike and adjust for forward momentum * adiWEAR&reg; rubber outsoles give ample durability in high-wear areas and offer lightweight grip and cushion Mens shoes , men's shoes , running shoes , adidas shoes , adidas running shoes , mens running shoes , snova running shoes , snova mens adidas , snova adidas running , snova shoes , sport shoes mens , sport shoes adidas , mens shoes , men's shoes , running , adidas</p>",
            //    ProductTemplateId = productTemplateInGrid.Id,
            //    //SeName = "adidas-womens-supernova-csh-7-running-shoe",
            //    AllowCustomerReviews = true,
            //    Published = true,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow
            //};
            //allProducts.Add(productAdidasShoe);
            //productAdidasShoe.ProductVariants.Add(new ProductVariant()
            //{
            //    Price = 40M,
            //    IsShipEnabled = true,
            //    Weight = 2,
            //    Length = 2,
            //    Width = 2,
            //    Height = 2,
            //    TaxCategoryId = _taxCategoryRepository.Table.Where(tc => tc.Name == "Apparel & Shoes").Single().Id,
            //    ManageInventoryMethod = ManageInventoryMethod.ManageStock,
            //    StockQuantity = 10000,
            //    NotifyAdminForQuantityBelow = 1,
            //    AllowBackInStockSubscriptions = false,
            //    DisplayStockAvailability = true,
            //    LowStockActivity = LowStockActivity.DisableBuyButton,
            //    BackorderMode = BackorderMode.NoBackorders,
            //    OrderMinimumQuantity = 1,
            //    OrderMaximumQuantity = 10000,
            //    Published = true,
            //    DisplayOrder = 1,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow,
            //});
            //var pvaAdidasShoe1 = new ProductVariantAttribute()
            //{
            //    ProductAttribute = _productAttributeRepository.Table.Where(x => x.Name == "Size").Single(),
            //    AttributeControlType = AttributeControlType.DropdownList,
            //    IsRequired = true,
            //};
            //pvaAdidasShoe1.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
            //{
            //    Name = "8",
            //    DisplayOrder = 1,
            //});
            //pvaAdidasShoe1.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
            //{
            //    Name = "9",
            //    DisplayOrder = 2,
            //});
            //pvaAdidasShoe1.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
            //{
            //    Name = "10",
            //    DisplayOrder = 3,
            //});
            //pvaAdidasShoe1.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
            //{
            //    Name = "11",
            //    DisplayOrder = 4,
            //});
            //productAdidasShoe.ProductVariants.FirstOrDefault().ProductVariantAttributes.Add(pvaAdidasShoe1);
            //var pvaAdidasShoe2 = new ProductVariantAttribute()
            //{
            //    ProductAttribute = _productAttributeRepository.Table.Where(x => x.Name == "Color").Single(),
            //    AttributeControlType = AttributeControlType.DropdownList,
            //    IsRequired = true,
            //};
            //pvaAdidasShoe2.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
            //{
            //    Name = "White/Blue",
            //    DisplayOrder = 1,
            //});
            //pvaAdidasShoe2.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
            //{
            //    Name = "White/Black",
            //    DisplayOrder = 2,
            //});
            //productAdidasShoe.ProductVariants.FirstOrDefault().ProductVariantAttributes.Add(pvaAdidasShoe2);
            //productAdidasShoe.ProductCategories.Add(new ProductCategory()
            //{
            //    Category = _categoryRepository.Table.Where(c => c.Name == "Shoes").Single(),
            //    DisplayOrder = 1,
            //});
            //productAdidasShoe.ProductPictures.Add(new ProductPicture()
            //{
            //    Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_AdidasShoe_1.jpg"), "image/pjpeg", pictureService.GetPictureSeName(productAdidasShoe.Name), true),
            //    DisplayOrder = 1,
            //});
            //productAdidasShoe.ProductPictures.Add(new ProductPicture()
            //{
            //    Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_AdidasShoe_2.jpg"), "image/pjpeg", pictureService.GetPictureSeName(productAdidasShoe.Name), true),
            //    DisplayOrder = 2,
            //});
            //_productRepository.Insert(productAdidasShoe);





            //var productAdobePhotoshop = new Product()
            //{
            //    Name = "Adobe Photoshop Elements 7",
            //    ShortDescription = "Easily find and view all your photos",
            //    FullDescription = "<p>Adobe Photoshop Elements 7 software combines power and simplicity so you can make ordinary photos extraordinary; tell engaging stories in beautiful, personalized creations for print and web; and easily find and view all your photos. New Photoshop.com membership* works with Photoshop Elements so you can protect your photos with automatic online backup and 2 GB of storage; view your photos anywhere you are; and share your photos in fun, interactive ways with invitation-only Online Albums.</p>",
            //    ProductTemplateId = productTemplateSingleVariant.Id,
            //    //SeName = "adobe-photoshop-elements-7",
            //    AllowCustomerReviews = true,
            //    Published = true,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow
            //};
            //allProducts.Add(productAdobePhotoshop);
            //productAdobePhotoshop.ProductVariants.Add(new ProductVariant()
            //{
            //    Price = 75M,
            //    IsShipEnabled = true,
            //    Weight = 2,
            //    Length = 2,
            //    Width = 2,
            //    Height = 3,
            //    TaxCategoryId = _taxCategoryRepository.Table.Where(tc => tc.Name == "Electronics & Software").Single().Id,
            //    ManageInventoryMethod = ManageInventoryMethod.ManageStock,
            //    StockQuantity = 10000,
            //    NotifyAdminForQuantityBelow = 1,
            //    AllowBackInStockSubscriptions = false,
            //    DisplayStockAvailability = true,
            //    LowStockActivity = LowStockActivity.DisableBuyButton,
            //    BackorderMode = BackorderMode.NoBackorders,
            //    OrderMinimumQuantity = 1,
            //    OrderMaximumQuantity = 10000,
            //    Published = true,
            //    DisplayOrder = 1,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow,
            //});
            //productAdobePhotoshop.ProductCategories.Add(new ProductCategory()
            //{
            //    Category = _categoryRepository.Table.Where(c => c.Name == "Software").Single(),
            //    DisplayOrder = 1,
            //});
            //productAdobePhotoshop.ProductPictures.Add(new ProductPicture()
            //{
            //    Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_AdobePhotoshop.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productAdobePhotoshop.Name), true),
            //    DisplayOrder = 1,
            //});
            //_productRepository.Insert(productAdobePhotoshop);





            //var productApcUps = new Product()
            //{
            //    Name = "APC Back-UPS RS 800VA - UPS - 800 VA - UPS battery - lead acid ( BR800BLK )",
            //    ShortDescription = "APC Back-UPS RS, 800VA/540W, Input 120V/Output 120V, Interface Port USB. ",
            //    FullDescription = "<p>The Back-UPS RS offers high performance protection for your business and office computer systems. It provides abundant battery backup power, allowing you to work through medium and extended length power outages. It also safeguards your equipment from damaging surges and spikes that travel along utility, phone and network lines. A distinguishing feature of the Back-UPS RS is automatic voltage regulation (AVR). AVR instantly adjusts both low and high voltages to safe levels, so you can work indefinitely during brownouts and overvoltage situations, saving the battery for power outages when you need it most. Award-winning shutdown software automatically powers down your computer system in the event of an extended power outage.</p>",
            //    ProductTemplateId = productTemplateSingleVariant.Id,
            //    //SeName = "apc-back-ups-rs-800va-ups-800-va-ups-battery-lead-acid-br800blk",
            //    AllowCustomerReviews = true,
            //    Published = true,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow
            //};
            //allProducts.Add(productApcUps);
            //productApcUps.ProductVariants.Add(new ProductVariant()
            //{
            //    Price = 75M,
            //    IsShipEnabled = true,
            //    Weight = 2,
            //    Length = 2,
            //    Width = 2,
            //    Height = 3,
            //    TaxCategoryId = _taxCategoryRepository.Table.Where(tc => tc.Name == "Electronics & Software").Single().Id,
            //    ManageInventoryMethod = ManageInventoryMethod.ManageStock,
            //    StockQuantity = 10000,
            //    NotifyAdminForQuantityBelow = 1,
            //    AllowBackInStockSubscriptions = false,
            //    DisplayStockAvailability = true,
            //    LowStockActivity = LowStockActivity.DisableBuyButton,
            //    BackorderMode = BackorderMode.NoBackorders,
            //    OrderMinimumQuantity = 1,
            //    OrderMaximumQuantity = 10000,
            //    Published = true,
            //    DisplayOrder = 1,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow,
            //});
            //productApcUps.ProductCategories.Add(new ProductCategory()
            //{
            //    Category = _categoryRepository.Table.Where(c => c.Name == "Accessories").Single(),
            //    DisplayOrder = 1,
            //});
            //productApcUps.ProductPictures.Add(new ProductPicture()
            //{
            //    Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_ApcUps.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productApcUps.Name), true),
            //    DisplayOrder = 1,
            //});
            //_productRepository.Insert(productApcUps);





            //var productArrow = new Product()
            //{
            //    Name = "Arrow Men's Wrinkle Free Pinpoint Solid Long Sleeve",
            //    ShortDescription = "",
            //    FullDescription = "<p>This Wrinkle Free Pinpoint Long Sleeve Dress Shirt needs minimum ironing. It is a great product at a great value!</p>",
            //    ProductTemplateId = productTemplateSingleVariant.Id,
            //    //SeName = "arrow-mens-wrinkle-free-pinpoint-solid-long-sleeve",
            //    AllowCustomerReviews = true,
            //    Published = true,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow
            //};
            //allProducts.Add(productArrow);
            //productArrow.ProductVariants.Add(new ProductVariant()
            //{
            //    Price = 24M,
            //    IsShipEnabled = true,
            //    Weight = 4,
            //    Length = 3,
            //    Width = 3,
            //    Height = 3,
            //    TaxCategoryId = _taxCategoryRepository.Table.Where(tc => tc.Name == "Apparel & Shoes").Single().Id,
            //    ManageInventoryMethod = ManageInventoryMethod.ManageStock,
            //    StockQuantity = 10000,
            //    NotifyAdminForQuantityBelow = 1,
            //    AllowBackInStockSubscriptions = false,
            //    DisplayStockAvailability = true,
            //    LowStockActivity = LowStockActivity.DisableBuyButton,
            //    BackorderMode = BackorderMode.NoBackorders,
            //    OrderMinimumQuantity = 1,
            //    OrderMaximumQuantity = 10000,
            //    Published = true,
            //    DisplayOrder = 1,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow,
            //});
            //productArrow.ProductVariants.FirstOrDefault().TierPrices.Add(new TierPrice()
            //{
            //    Quantity = 3,
            //    Price = 21
            //});
            //productArrow.ProductVariants.FirstOrDefault().TierPrices.Add(new TierPrice()
            //{
            //    Quantity = 7,
            //    Price = 19
            //});
            //productArrow.ProductVariants.FirstOrDefault().TierPrices.Add(new TierPrice()
            //{
            //    Quantity = 10,
            //    Price = 16
            //});
            //productArrow.ProductVariants.FirstOrDefault().HasTierPrices = true;

            //productArrow.ProductCategories.Add(new ProductCategory()
            //{
            //    Category = _categoryRepository.Table.Where(c => c.Name == "Shirts").Single(),
            //    DisplayOrder = 1,
            //});
            //productArrow.ProductPictures.Add(new ProductPicture()
            //{
            //    Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_arrow.jpg"), "image/pjpeg", pictureService.GetPictureSeName(productArrow.Name), true),
            //    DisplayOrder = 1,
            //});
            //_productRepository.Insert(productArrow);





            //var productAsusPc1000 = new Product()
            //{
            //    Name = "ASUS Eee PC 1000HA 10-Inch Netbook",
            //    ShortDescription = "Super Hybrid Engine offers a choice of performance and power consumption modes for easy adjustments according to various needs",
            //    FullDescription = "<p>Much more compact than a standard-sized notebook and weighing just over 3 pounds, the Eee PC 1000HA is perfect for students toting to school or road warriors packing away to Wi-Fi hotspots. The Eee PC 1000HA also features a 160 GB hard disk drive (HDD), 1 GB of RAM, 1.3-megapixel webcam integrated into the bezel above the LCD, 54g Wi-Fi networking (802.11b/g), Secure Digital memory card slot, multiple USB ports, a VGA output for connecting to a monitor.</p><p>It comes preinstalled with the Microsoft Windows XP Home operating system, which offers more experienced users an enhanced and innovative experience that incorporates Windows Live features like Windows Live Messenger for instant messaging and Windows Live Mail for consolidated email accounts on your desktop. Complementing this is Microsoft Works, which equips the user with numerous office applications to work efficiently.</p><p>The new Eee PC 1000HA has a customized, cutting-edge Infusion casing technology in Fine Ebony. Inlaid within the chassis itself, the motifs are an integral part of the entire cover and will not fade with time. The Infusion surface also provides a new level of resilience, providing scratch resistance and a beautiful style while out and about.</p>",
            //    ProductTemplateId = productTemplateSingleVariant.Id,
            //    //SeName = "asus-eee-pc-1000ha-10-inch-netbook",
            //    AllowCustomerReviews = true,
            //    Published = true,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow
            //};
            //allProducts.Add(productAsusPc1000);
            //productAsusPc1000.ProductVariants.Add(new ProductVariant()
            //{
            //    Price = 2600M,
            //    IsShipEnabled = true,
            //    Weight = 3,
            //    Length = 3,
            //    Width = 2,
            //    Height = 2,
            //    TaxCategoryId = _taxCategoryRepository.Table.Where(tc => tc.Name == "Electronics & Software").Single().Id,
            //    ManageInventoryMethod = ManageInventoryMethod.ManageStock,
            //    StockQuantity = 10000,
            //    NotifyAdminForQuantityBelow = 1,
            //    AllowBackInStockSubscriptions = false,
            //    DisplayStockAvailability = true,
            //    LowStockActivity = LowStockActivity.DisableBuyButton,
            //    BackorderMode = BackorderMode.NoBackorders,
            //    OrderMinimumQuantity = 1,
            //    OrderMaximumQuantity = 10000,
            //    Published = true,
            //    DisplayOrder = 1,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow,
            //});
            //productAsusPc1000.ProductCategories.Add(new ProductCategory()
            //{
            //    Category = _categoryRepository.Table.Where(c => c.Name == "Notebooks").Single(),
            //    DisplayOrder = 1,
            //});
            //productAsusPc1000.ProductManufacturers.Add(new ProductManufacturer()
            //{
            //    Manufacturer = _manufacturerRepository.Table.Where(c => c.Name == "ASUS").Single(),
            //    DisplayOrder = 2,
            //});
            //productAsusPc1000.ProductPictures.Add(new ProductPicture()
            //{
            //    Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_asuspc1000.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productAsusPc1000.Name), true),
            //    DisplayOrder = 1,
            //});
            //productAsusPc1000.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            //{
            //    AllowFiltering = false,
            //    ShowOnProductPage = true,
            //    DisplayOrder = 1,
            //    SpecificationAttributeOption = _specificationAttributeRepository.Table.Where(sa => sa.Name == "Screensize").Single().SpecificationAttributeOptions.Where(sao => sao.Name == "10.0''").Single()
            //});
            //productAsusPc1000.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            //{
            //    AllowFiltering = true,
            //    ShowOnProductPage = true,
            //    DisplayOrder = 2,
            //    SpecificationAttributeOption = _specificationAttributeRepository.Table.Where(sa => sa.Name == "CPU Type").Single().SpecificationAttributeOptions.Where(sao => sao.Name == "AMD").Single()
            //});
            //productAsusPc1000.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            //{
            //    AllowFiltering = true,
            //    ShowOnProductPage = true,
            //    DisplayOrder = 3,
            //    SpecificationAttributeOption = _specificationAttributeRepository.Table.Where(sa => sa.Name == "Memory").Single().SpecificationAttributeOptions.Where(sao => sao.Name == "1 GB").Single()
            //});
            //productAsusPc1000.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            //{
            //    AllowFiltering = false,
            //    ShowOnProductPage = true,
            //    DisplayOrder = 4,
            //    SpecificationAttributeOption = _specificationAttributeRepository.Table.Where(sa => sa.Name == "Hardrive").Single().SpecificationAttributeOptions.Where(sao => sao.Name == "160 GB").Single()
            //});
            //_productRepository.Insert(productAsusPc1000);





            //var productAsusPc900 = new Product()
            //{
            //    Name = "ASUS Eee PC 900HA 8.9-Inch Netbook Black",
            //    ShortDescription = "High Speed Connectivity Anywhere with Wi-Fi 802.11b/g.",
            //    FullDescription = "<p>Much more compact than a standard-sized notebook and weighing just 2.5 pounds, the Eee PC 900HA is perfect for students toting to school or road warriors packing away to Wi-Fi hotspots. In addition to the 160 GB hard disk drive (HDD), the Eee PC 900HA also features 1 GB of RAM, VGA-resolution webcam integrated into the bezel above the LCD, 54g Wi-Fi networking (802.11b/g), multiple USB ports, SD memory card slot, a VGA output for connecting to a monitor, and up to 10 GB of online storage (complimentary for 18 months).</p><p>It comes preinstalled with the Microsoft Windows XP Home operating system, which offers more experienced users an enhanced and innovative experience that incorporates Windows Live features like Windows Live Messenger for instant messaging and Windows Live Mail for consolidated email accounts on your desktop. Complementing this is Microsoft Works, which equips the user with numerous office applications to work efficiently.</p>",
            //    ProductTemplateId = productTemplateSingleVariant.Id,
            //    //SeName = "asus-eee-pc-900ha-89-inch-netbook-black",
            //    AllowCustomerReviews = true,
            //    Published = true,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow
            //};
            //allProducts.Add(productAsusPc900);
            //productAsusPc900.ProductVariants.Add(new ProductVariant()
            //{
            //    Price = 1500M,
            //    IsShipEnabled = true,
            //    Weight = 7,
            //    Length = 7,
            //    Width = 7,
            //    Height = 7,
            //    TaxCategoryId = _taxCategoryRepository.Table.Where(tc => tc.Name == "Electronics & Software").Single().Id,
            //    ManageInventoryMethod = ManageInventoryMethod.ManageStock,
            //    StockQuantity = 10000,
            //    NotifyAdminForQuantityBelow = 1,
            //    AllowBackInStockSubscriptions = false,
            //    DisplayStockAvailability = true,
            //    LowStockActivity = LowStockActivity.DisableBuyButton,
            //    BackorderMode = BackorderMode.NoBackorders,
            //    OrderMinimumQuantity = 1,
            //    OrderMaximumQuantity = 10000,
            //    Published = true,
            //    DisplayOrder = 1,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow,
            //});
            //productAsusPc900.ProductCategories.Add(new ProductCategory()
            //{
            //    Category = _categoryRepository.Table.Where(c => c.Name == "Notebooks").Single(),
            //    DisplayOrder = 1,
            //});
            //productAsusPc900.ProductManufacturers.Add(new ProductManufacturer()
            //{
            //    Manufacturer = _manufacturerRepository.Table.Where(c => c.Name == "ASUS").Single(),
            //    DisplayOrder = 1,
            //});
            //productAsusPc900.ProductPictures.Add(new ProductPicture()
            //{
            //    Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_asuspc900.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productAsusPc900.Name), true),
            //    DisplayOrder = 1,
            //});
            //productAsusPc900.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            //{
            //    AllowFiltering = true,
            //    ShowOnProductPage = true,
            //    DisplayOrder = 2,
            //    SpecificationAttributeOption = _specificationAttributeRepository.Table.Where(sa => sa.Name == "CPU Type").Single().SpecificationAttributeOptions.Where(sao => sao.Name == "AMD").Single()
            //});
            //productAsusPc900.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            //{
            //    AllowFiltering = true,
            //    ShowOnProductPage = true,
            //    DisplayOrder = 3,
            //    SpecificationAttributeOption = _specificationAttributeRepository.Table.Where(sa => sa.Name == "Memory").Single().SpecificationAttributeOptions.Where(sao => sao.Name == "1 GB").Single()
            //});
            //productAsusPc900.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            //{
            //    AllowFiltering = false,
            //    ShowOnProductPage = true,
            //    DisplayOrder = 4,
            //    SpecificationAttributeOption = _specificationAttributeRepository.Table.Where(sa => sa.Name == "Hardrive").Single().SpecificationAttributeOptions.Where(sao => sao.Name == "160 GB").Single()
            //});
            //_productRepository.Insert(productAsusPc900);





            //var productBestGrillingRecipes = new Product()
            //{
            //    Name = "Best Grilling Recipes",
            //    ShortDescription = "More Than 100 Regional Favorites Tested and Perfected for the Outdoor Cook (Hardcover)",
            //    FullDescription = "<p>Take a winding cross-country trip and you'll discover barbecue shacks with offerings like tender-smoky Baltimore pit beef and saucy St. Louis pork steaks. To bring you the best of these hidden gems, along with all the classics, the editors of Cook's Country magazine scoured the country, then tested and perfected their favorites. HEre traditions large and small are brought into the backyard, from Hawaii's rotisserie favorite, the golden-hued Huli Huli Chicken, to fall-off-the-bone Chicago Barbecued Ribs. In Kansas City, they're all about the sauce, and for our saucy Kansas City Sticky Ribs, we found a surprise ingredient-root beer. We also tackle all the best sides. <br /><br />Not sure where or how to start? This cookbook kicks off with an easy-to-follow primer that will get newcomers all fired up. Whether you want to entertain a crowd or just want to learn to make perfect burgers, Best Grilling Recipes shows you the way.</p>",
            //    ProductTemplateId = productTemplateSingleVariant.Id,
            //    //SeName = "best-grilling-recipes",
            //    AllowCustomerReviews = true,
            //    Published = true,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow
            //};
            //allProducts.Add(productBestGrillingRecipes);
            //productBestGrillingRecipes.ProductVariants.Add(new ProductVariant()
            //{
            //    Price = 27M,
            //    OldPrice = 30M,
            //    IsShipEnabled = true,
            //    Weight = 2,
            //    Length = 2,
            //    Width = 2,
            //    Height = 2,
            //    TaxCategoryId = _taxCategoryRepository.Table.Where(tc => tc.Name == "Books").Single().Id,
            //    ManageInventoryMethod = ManageInventoryMethod.ManageStock,
            //    StockQuantity = 10000,
            //    NotifyAdminForQuantityBelow = 1,
            //    AllowBackInStockSubscriptions = false,
            //    DisplayStockAvailability = true,
            //    LowStockActivity = LowStockActivity.DisableBuyButton,
            //    BackorderMode = BackorderMode.NoBackorders,
            //    OrderMinimumQuantity = 1,
            //    OrderMaximumQuantity = 10000,
            //    Published = true,
            //    DisplayOrder = 1,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow,
            //});
            //productBestGrillingRecipes.ProductCategories.Add(new ProductCategory()
            //{
            //    Category = _categoryRepository.Table.Where(c => c.Name == "Books").Single(),
            //    DisplayOrder = 1,
            //});
            //productBestGrillingRecipes.ProductPictures.Add(new ProductPicture()
            //{
            //    Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_BestGrillingRecipes.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productBestGrillingRecipes.Name), true),
            //    DisplayOrder = 1,
            //});
            //_productRepository.Insert(productBestGrillingRecipes);





            //var productDiamondHeart = new Product()
            //{
            //    Name = "Black & White Diamond Heart",
            //    ShortDescription = "Heart Pendant 1/4 Carat (ctw) in Sterling Silver",
            //    FullDescription = "<p>Bold black diamonds alternate with sparkling white diamonds along a crisp sterling silver heart to create a look that is simple and beautiful. This sleek and stunning 1/4 carat (ctw) diamond heart pendant which includes an 18 inch silver chain, and a free box of godiva chocolates makes the perfect Valentine's Day gift.</p>",
            //    ProductTemplateId = productTemplateSingleVariant.Id,
            //    //SeName = "black-white-diamond-heart",
            //    AllowCustomerReviews = true,
            //    Published = true,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow
            //};
            //allProducts.Add(productDiamondHeart);
            //productDiamondHeart.ProductVariants.Add(new ProductVariant()
            //{
            //    Price = 130M,
            //    IsShipEnabled = true,
            //    Weight = 2,
            //    Length = 2,
            //    Width = 2,
            //    Height = 2,
            //    TaxCategoryId = _taxCategoryRepository.Table.Where(tc => tc.Name == "Jewelry").Single().Id,
            //    ManageInventoryMethod = ManageInventoryMethod.ManageStock,
            //    StockQuantity = 10000,
            //    NotifyAdminForQuantityBelow = 1,
            //    AllowBackInStockSubscriptions = false,
            //    DisplayStockAvailability = true,
            //    LowStockActivity = LowStockActivity.DisableBuyButton,
            //    BackorderMode = BackorderMode.NoBackorders,
            //    OrderMinimumQuantity = 1,
            //    OrderMaximumQuantity = 10000,
            //    Published = true,
            //    DisplayOrder = 1,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow,
            //});
            //productDiamondHeart.ProductCategories.Add(new ProductCategory()
            //{
            //    Category = _categoryRepository.Table.Where(c => c.Name == "Jewelry").Single(),
            //    DisplayOrder = 1,
            //});
            //productDiamondHeart.ProductPictures.Add(new ProductPicture()
            //{
            //    Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_DiamondHeart.jpg"), "image/pjpeg", pictureService.GetPictureSeName(productDiamondHeart.Name), true),
            //    DisplayOrder = 1,
            //});
            //_productRepository.Insert(productDiamondHeart);





            //var productBlackBerry = new Product()
            //{
            //    Name = "BlackBerry Bold 9000 Phone, Black (AT&T)",
            //    ShortDescription = "Global Blackberry messaging smartphone with quad-band GSM",
            //    FullDescription = "<p>Keep yourself on track for your next meeting with turn-by-turn directions via the AT&amp;T Navigator service, which is powered by TeleNav and provides spoken or text-based turn-by-turn directions with automatic missed turn rerouting and a local business finder service in 20 countries. It also supports AT&amp;T mobile music services and access to thousands of video clips via Cellular Video. Other features include a 2-megapixel camera/camcorder, Bluetooth for handsfree communication, 1 GB of internal memory with MicroSD expansion (up to 32 GB), multi-format audio/video playback, and up to 4.5 hours of talk time.</p><p>The Blackberry Bold also comes with free access to AT&amp;T Wi-Fi Hotspots, available at more than 17,000 locations nationwide including Starbucks. The best part is that you do'nt need to sign up for anything new to use this service--Wi-Fi access for is included in all Blackberry Personal and Enterprise Rate Plans. (You must subscribe to a Blackberry Data Rate Plan to access AT&amp;T Wi-Fi Hotspots.) Additionally, the Blackberry Bold is the first RIM device that supports AT&amp;T Cellular Video (CV).</p>",
            //    ProductTemplateId = productTemplateSingleVariant.Id,
            //    //SeName = "blackberry-bold-9000-phone-black-att",
            //    AllowCustomerReviews = true,
            //    Published = true,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow
            //};
            //allProducts.Add(productBlackBerry);
            //productBlackBerry.ProductVariants.Add(new ProductVariant()
            //{
            //    Price = 245M,
            //    IsShipEnabled = true,
            //    Weight = 2,
            //    Length = 2,
            //    Width = 2,
            //    Height = 2,
            //    TaxCategoryId = _taxCategoryRepository.Table.Where(tc => tc.Name == "Electronics & Software").Single().Id,
            //    ManageInventoryMethod = ManageInventoryMethod.ManageStock,
            //    StockQuantity = 10000,
            //    NotifyAdminForQuantityBelow = 1,
            //    AllowBackInStockSubscriptions = false,
            //    DisplayStockAvailability = true,
            //    LowStockActivity = LowStockActivity.DisableBuyButton,
            //    BackorderMode = BackorderMode.NoBackorders,
            //    OrderMinimumQuantity = 1,
            //    OrderMaximumQuantity = 10000,
            //    Published = true,
            //    DisplayOrder = 1,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow,
            //});
            //productBlackBerry.ProductCategories.Add(new ProductCategory()
            //{
            //    Category = _categoryRepository.Table.Where(c => c.Name == "Cell phones").Single(),
            //    DisplayOrder = 1,
            //});
            //productBlackBerry.ProductPictures.Add(new ProductPicture()
            //{
            //    Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_BlackBerry.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productBlackBerry.Name), true),
            //    DisplayOrder = 1,
            //});
            //_productRepository.Insert(productBlackBerry);




            //var productBuildComputer = new Product()
            //{
            //    Name = "Build your own computer",
            //    ShortDescription = "Build it",
            //    FullDescription = "<p>Fight back against cluttered workspaces with the stylish Sony VAIO JS All-in-One desktop PC, featuring powerful computing resources and a stunning 20.1-inch widescreen display with stunning XBRITE-HiColor LCD technology. The silver Sony VAIO VGC-JS110J/S has a built-in microphone and MOTION EYE camera with face-tracking technology that allows for easy communication with friends and family. And it has a built-in DVD burner and Sony's Movie Store software so you can create a digital entertainment library for personal viewing at your convenience. Easy to setup and even easier to use, this JS-series All-in-One includes an elegantly designed keyboard and a USB mouse.</p>",
            //    ProductTemplateId = productTemplateInGrid.Id,
            //    //SeName = "build-your-own-computer",
            //    AllowCustomerReviews = true,
            //    Published = true,
            //    ShowOnHomePage = true,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow
            //};
            //allProducts.Add(productBuildComputer);
            //productBuildComputer.ProductVariants.Add(new ProductVariant()
            //{
            //    Price = 1200M,
            //    IsShipEnabled = true,
            //    Weight = 2,
            //    Length = 2,
            //    Width = 2,
            //    Height = 2,
            //    TaxCategoryId = _taxCategoryRepository.Table.Where(tc => tc.Name == "Electronics & Software").Single().Id,
            //    ManageInventoryMethod = ManageInventoryMethod.ManageStock,
            //    StockQuantity = 10000,
            //    NotifyAdminForQuantityBelow = 1,
            //    AllowBackInStockSubscriptions = false,
            //    DisplayStockAvailability = true,
            //    LowStockActivity = LowStockActivity.DisableBuyButton,
            //    BackorderMode = BackorderMode.NoBackorders,
            //    OrderMinimumQuantity = 1,
            //    OrderMaximumQuantity = 10000,
            //    Published = true,
            //    DisplayOrder = 1,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow,
            //});
            //var pvaBuildComputer1 = new ProductVariantAttribute()
            //{
            //    ProductAttribute = _productAttributeRepository.Table.Where(x => x.Name == "Processor").Single(),
            //    AttributeControlType = AttributeControlType.DropdownList,
            //    IsRequired = true,
            //};
            //pvaBuildComputer1.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
            //{
            //    Name = "2.2 GHz Intel Pentium Dual-Core E2200",
            //    DisplayOrder = 1,
            //});
            //pvaBuildComputer1.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
            //{
            //    Name = "2.5 GHz Intel Pentium Dual-Core E2200",
            //    IsPreSelected = true,
            //    PriceAdjustment = 15,
            //    DisplayOrder = 2,
            //});
            //productBuildComputer.ProductVariants.FirstOrDefault().ProductVariantAttributes.Add(pvaBuildComputer1);
            //var pvaBuildComputer2 = new ProductVariantAttribute()
            //{
            //    ProductAttribute = _productAttributeRepository.Table.Where(x => x.Name == "RAM").Single(),
            //    AttributeControlType = AttributeControlType.DropdownList,
            //    IsRequired = true,
            //};
            //pvaBuildComputer2.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
            //{
            //    Name = "2 GB",
            //    DisplayOrder = 1,
            //});
            //pvaBuildComputer2.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
            //{
            //    Name = "4GB",
            //    PriceAdjustment = 20,
            //    DisplayOrder = 2,
            //});
            //pvaBuildComputer2.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
            //{
            //    Name = "8GB",
            //    PriceAdjustment = 60,
            //    DisplayOrder = 3,
            //});
            //productBuildComputer.ProductVariants.FirstOrDefault().ProductVariantAttributes.Add(pvaBuildComputer2);
            //var pvaBuildComputer3 = new ProductVariantAttribute()
            //{
            //    ProductAttribute = _productAttributeRepository.Table.Where(x => x.Name == "HDD").Single(),
            //    AttributeControlType = AttributeControlType.RadioList,
            //    IsRequired = true,
            //};
            //pvaBuildComputer3.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
            //{
            //    Name = "320 GB",
            //    DisplayOrder = 1,
            //});
            //pvaBuildComputer3.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
            //{
            //    Name = "400 GB",
            //    PriceAdjustment = 100,
            //    DisplayOrder = 2,
            //});
            //productBuildComputer.ProductVariants.FirstOrDefault().ProductVariantAttributes.Add(pvaBuildComputer3);
            //var pvaBuildComputer4 = new ProductVariantAttribute()
            //{
            //    ProductAttribute = _productAttributeRepository.Table.Where(x => x.Name == "OS").Single(),
            //    AttributeControlType = AttributeControlType.RadioList,
            //    IsRequired = true,
            //};
            //pvaBuildComputer4.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
            //{
            //    Name = "Vista Home",
            //    PriceAdjustment = 50,
            //    IsPreSelected = true,
            //    DisplayOrder = 1,
            //});
            //pvaBuildComputer4.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
            //{
            //    Name = "Vista Premium",
            //    PriceAdjustment = 60,
            //    DisplayOrder = 2,
            //});
            //productBuildComputer.ProductVariants.FirstOrDefault().ProductVariantAttributes.Add(pvaBuildComputer4);
            //var pvaBuildComputer5 = new ProductVariantAttribute()
            //{
            //    ProductAttribute = _productAttributeRepository.Table.Where(x => x.Name == "Software").Single(),
            //    AttributeControlType = AttributeControlType.Checkboxes,
            //};
            //pvaBuildComputer5.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
            //{
            //    Name = "Microsoft Office",
            //    PriceAdjustment = 50,
            //    IsPreSelected = true,
            //    DisplayOrder = 1,
            //});
            //pvaBuildComputer5.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
            //{
            //    Name = "Acrobat Reader",
            //    PriceAdjustment = 10,
            //    DisplayOrder = 2,
            //});
            //pvaBuildComputer5.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
            //{
            //    Name = "Total Commander",
            //    PriceAdjustment = 5,
            //    DisplayOrder = 2,
            //});
            //productBuildComputer.ProductVariants.FirstOrDefault().ProductVariantAttributes.Add(pvaBuildComputer5);
            //productBuildComputer.ProductCategories.Add(new ProductCategory()
            //{
            //    Category = _categoryRepository.Table.Where(c => c.Name == "Desktops").Single(),
            //    DisplayOrder = 1,
            //});
            //productBuildComputer.ProductPictures.Add(new ProductPicture()
            //{
            //    Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_Desktops_1.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productBuildComputer.Name), true),
            //    DisplayOrder = 1,
            //});
            //productBuildComputer.ProductPictures.Add(new ProductPicture()
            //{
            //    Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_Desktops_2.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productBuildComputer.Name), true),
            //    DisplayOrder = 2,
            //});
            //productBuildComputer.ProductPictures.Add(new ProductPicture()
            //{
            //    Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_Desktops_3.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productBuildComputer.Name), true),
            //    DisplayOrder = 3,
            //});
            //_productRepository.Insert(productBuildComputer);





            //var productCanonCamera = new Product()
            //{
            //    Name = "Canon Digital Rebel XSi 12.2 MP Digital SLR Camera",
            //    ShortDescription = "12.2-megapixel CMOS sensor captures enough detail for poster-size, photo-quality prints",
            //    FullDescription = "<p>For stunning photography with point and shoot ease, look no further than Canon&rsquo;s EOS Rebel XSi. The EOS Rebel XSi brings staggering technological innovation to the masses. It features Canon&rsquo;s EOS Integrated Cleaning System, Live View Function, a powerful DIGIC III Image Processor, plus a new 12.2-megapixel CMOS sensor and is available in a kit with the new EF-S 18-55mm f/3.5-5.6 IS lens with Optical Image Stabilizer. The EOS Rebel XSi&rsquo;s refined, ergonomic design includes a new 3.0-inch LCD monitor, compatibility with SD and SDHC memory cards and new accessories that enhance every aspect of the photographic experience.</p>",
            //    ProductTemplateId = productTemplateInGrid.Id,
            //    //SeName = "canon-digital-rebel-xsi-122-mp-digital-slr-camera",
            //    AllowCustomerReviews = true,
            //    Published = true,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow
            //};
            //allProducts.Add(productCanonCamera);
            //productCanonCamera.ProductVariants.Add(new ProductVariant()
            //{
            //    Name = "Black",
            //    PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_CanonCamera_black.jpeg"), "image/jpeg", pictureService.GetPictureSeName("Canon Digital Rebel XSi 12.2 MP Digital SLR Camera (Black)"), true).Id,
            //    Price = 670M,
            //    IsShipEnabled = true,
            //    Weight = 2,
            //    Length = 2,
            //    Width = 2,
            //    Height = 2,
            //    TaxCategoryId = _taxCategoryRepository.Table.Where(tc => tc.Name == "Electronics & Software").Single().Id,
            //    ManageInventoryMethod = ManageInventoryMethod.ManageStock,
            //    StockQuantity = 10000,
            //    NotifyAdminForQuantityBelow = 1,
            //    AllowBackInStockSubscriptions = false,
            //    DisplayStockAvailability = true,
            //    LowStockActivity = LowStockActivity.DisableBuyButton,
            //    BackorderMode = BackorderMode.NoBackorders,
            //    OrderMinimumQuantity = 1,
            //    OrderMaximumQuantity = 10000,
            //    Published = true,
            //    DisplayOrder = 1,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow,
            //});
            //productCanonCamera.ProductVariants.Add(new ProductVariant()
            //{
            //    Name = "Silver",
            //    PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_CanonCamera_silver.jpeg"), "image/jpeg", pictureService.GetPictureSeName("Canon Digital Rebel XSi 12.2 MP Digital SLR Camera (Silver)"), true).Id,
            //    Price = 630M,
            //    IsShipEnabled = true,
            //    Weight = 2,
            //    Length = 2,
            //    Width = 2,
            //    Height = 2,
            //    TaxCategoryId = _taxCategoryRepository.Table.Where(tc => tc.Name == "Electronics & Software").Single().Id,
            //    ManageInventoryMethod = ManageInventoryMethod.ManageStock,
            //    StockQuantity = 10000,
            //    NotifyAdminForQuantityBelow = 1,
            //    AllowBackInStockSubscriptions = false,
            //    DisplayStockAvailability = true,
            //    LowStockActivity = LowStockActivity.DisableBuyButton,
            //    BackorderMode = BackorderMode.NoBackorders,
            //    OrderMinimumQuantity = 1,
            //    OrderMaximumQuantity = 10000,
            //    Published = true,
            //    DisplayOrder = 1,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow,
            //});
            //productCanonCamera.ProductCategories.Add(new ProductCategory()
            //{
            //    Category = _categoryRepository.Table.Where(c => c.Name == "Camera, photo").Single(),
            //    DisplayOrder = 1,
            //});
            //productCanonCamera.ProductPictures.Add(new ProductPicture()
            //{
            //    Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_CanonCamera_1.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productCanonCamera.Name), true),
            //    DisplayOrder = 1,
            //});
            //productCanonCamera.ProductPictures.Add(new ProductPicture()
            //{
            //    Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_CanonCamera_2.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productCanonCamera.Name), true),
            //    DisplayOrder = 2,
            //});
            //_productRepository.Insert(productCanonCamera);





            //var productCanonCamcoder = new Product()
            //{
            //    Name = "Canon VIXIA HF100 Camcorder",
            //    ShortDescription = "12x optical zoom; SuperRange Optical Image Stabilizer",
            //    FullDescription = "<p>From Canon's long history of optical excellence, advanced image processing, superb performance and technological innovation in photographic and broadcast television cameras comes the latest in high definition camcorders. <br /><br />Now, with the light, compact Canon VIXIA HF100, you can have stunning AVCHD (Advanced Video Codec High Definition) format recording with the ease and numerous benefits of Flash Memory. It's used in some of the world's most innovative electronic products such as laptop computers, MP3 players, PDAs and cell phones. <br /><br />Add to that the VIXIA HF100's Canon Exclusive features such as our own 3.3 Megapixel Full HD CMOS sensor and advanced DIGIC DV II Image Processor, SuperRange Optical Image Stabilization, Instant Auto Focus, our 2.7-inch Widescreen Multi-Angle Vivid LCD and the Genuine Canon 12x HD video zoom lens and you have a Flash Memory camcorder that's hard to beat.</p>",
            //    ProductTemplateId = productTemplateSingleVariant.Id,
            //    //SeName = "canon-vixia-hf100-camcorder",
            //    AllowCustomerReviews = true,
            //    Published = true,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow
            //};
            //allProducts.Add(productCanonCamcoder);
            //productCanonCamcoder.ProductVariants.Add(new ProductVariant()
            //{
            //    Price = 530M,
            //    IsShipEnabled = true,
            //    Weight = 7,
            //    Length = 7,
            //    Width = 7,
            //    Height = 7,
            //    TaxCategoryId = _taxCategoryRepository.Table.Where(tc => tc.Name == "Electronics & Software").Single().Id,
            //    ManageInventoryMethod = ManageInventoryMethod.ManageStock,
            //    StockQuantity = 10000,
            //    NotifyAdminForQuantityBelow = 1,
            //    AllowBackInStockSubscriptions = false,
            //    DisplayStockAvailability = true,
            //    LowStockActivity = LowStockActivity.DisableBuyButton,
            //    BackorderMode = BackorderMode.NoBackorders,
            //    OrderMinimumQuantity = 1,
            //    OrderMaximumQuantity = 10000,
            //    Published = true,
            //    DisplayOrder = 1,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow,
            //});
            //productCanonCamcoder.ProductCategories.Add(new ProductCategory()
            //{
            //    Category = _categoryRepository.Table.Where(c => c.Name == "Camera, photo").Single(),
            //    DisplayOrder = 1,
            //});
            //productCanonCamcoder.ProductPictures.Add(new ProductPicture()
            //{
            //    Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_CanonCamcoder.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productCanonCamcoder.Name), true),
            //    DisplayOrder = 1,
            //});
            //_productRepository.Insert(productCanonCamcoder);





            //var productCompaq = new Product()
            //{
            //    Name = "Compaq Presario SR1519X Pentium 4 Desktop PC with CDRW",
            //    ShortDescription = "Compaq Presario Desktop PC",
            //    FullDescription = "<p>Compaq Presario PCs give you solid performance, ease of use, and deliver just what you need so you can do more with less effort. Whether you are e-mailing family, balancing your online checkbook or creating school projects, the Presario is the right PC for you.</p>",
            //    ProductTemplateId = productTemplateSingleVariant.Id,
            //    //SeName = "compaq-presario-sr1519x-pentium-4-desktop-pc-with-cdrw",
            //    AllowCustomerReviews = true,
            //    Published = true,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow
            //};
            //allProducts.Add(productCompaq);
            //productCompaq.ProductVariants.Add(new ProductVariant()
            //{
            //    Price = 500M,
            //    IsShipEnabled = true,
            //    Weight = 7,
            //    Length = 7,
            //    Width = 7,
            //    Height = 7,
            //    TaxCategoryId = _taxCategoryRepository.Table.Where(tc => tc.Name == "Electronics & Software").Single().Id,
            //    ManageInventoryMethod = ManageInventoryMethod.ManageStock,
            //    StockQuantity = 10000,
            //    NotifyAdminForQuantityBelow = 1,
            //    AllowBackInStockSubscriptions = false,
            //    DisplayStockAvailability = true,
            //    LowStockActivity = LowStockActivity.DisableBuyButton,
            //    BackorderMode = BackorderMode.NoBackorders,
            //    OrderMinimumQuantity = 1,
            //    OrderMaximumQuantity = 10000,
            //    Published = true,
            //    DisplayOrder = 1,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow,
            //});
            //productCompaq.ProductCategories.Add(new ProductCategory()
            //{
            //    Category = _categoryRepository.Table.Where(c => c.Name == "Desktops").Single(),
            //    DisplayOrder = 1,
            //});
            //productCompaq.ProductPictures.Add(new ProductPicture()
            //{
            //    Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_Compaq.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productCompaq.Name), true),
            //    DisplayOrder = 1,
            //});
            //_productRepository.Insert(productCompaq);





            //var productCookingForTwo = new Product()
            //{
            //    Name = "Cooking for Two",
            //    ShortDescription = "More Than 200 Foolproof Recipes for Weeknights and Special Occasions (Hardcover)",
            //    FullDescription = "<p>Hardcover: 352 pages<br />Publisher: America's Test Kitchen (May 2009)<br />Language: English<br />ISBN-10: 1933615435<br />ISBN-13: 978-1933615431</p>",
            //    ProductTemplateId = productTemplateSingleVariant.Id,
            //    //SeName = "cooking-for-two",
            //    AllowCustomerReviews = true,
            //    Published = true,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow
            //};
            //allProducts.Add(productCookingForTwo);
            //productCookingForTwo.ProductVariants.Add(new ProductVariant()
            //{
            //    Price = 19M,
            //    OldPrice = 27M,
            //    IsShipEnabled = true,
            //    Weight = 2,
            //    Length = 2,
            //    Width = 2,
            //    Height = 2,
            //    TaxCategoryId = _taxCategoryRepository.Table.Where(tc => tc.Name == "Books").Single().Id,
            //    ManageInventoryMethod = ManageInventoryMethod.ManageStock,
            //    StockQuantity = 10000,
            //    NotifyAdminForQuantityBelow = 1,
            //    AllowBackInStockSubscriptions = false,
            //    DisplayStockAvailability = true,
            //    LowStockActivity = LowStockActivity.DisableBuyButton,
            //    BackorderMode = BackorderMode.NoBackorders,
            //    OrderMinimumQuantity = 1,
            //    OrderMaximumQuantity = 10000,
            //    Published = true,
            //    DisplayOrder = 1,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow,
            //});
            //productCookingForTwo.ProductCategories.Add(new ProductCategory()
            //{
            //    Category = _categoryRepository.Table.Where(c => c.Name == "Books").Single(),
            //    DisplayOrder = 1,
            //});
            //productCookingForTwo.ProductPictures.Add(new ProductPicture()
            //{
            //    Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_CookingForTwo.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productCookingForTwo.Name), true),
            //    DisplayOrder = 1,
            //});
            //_productRepository.Insert(productCookingForTwo);





            //var productCorel = new Product()
            //{
            //    Name = "Corel Paint Shop Pro Photo X2",
            //    ShortDescription = "The ideal choice for any aspiring photographer's digital darkroom",
            //    FullDescription = "<p>Corel Paint Shop Pro Photo X2 is the ideal choice for any aspiring photographer's digital darkroom. Fix brightness, color, and photo flaws in a few clicks; use precision editing tools to create the picture you want; give photos a unique, exciting look using hundreds of special effects, and much more! Plus, the NEW one-of-a-kind Express Lab helps you quickly view and fix dozens of photos in the time it used to take to edit a few. Paint Shop Pro Photo X2 even includes a built-in Learning Center to help you get started, it's the easiest way to get professional-looking photos - fast!</p>",
            //    ProductTemplateId = productTemplateSingleVariant.Id,
            //    //SeName = "corel-paint-shop-pro-photo-x2",
            //    AllowCustomerReviews = true,
            //    Published = true,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow
            //};
            //allProducts.Add(productCorel);
            //productCorel.ProductVariants.Add(new ProductVariant()
            //{
            //    Price = 65M,
            //    IsShipEnabled = true,
            //    Weight = 2,
            //    Length = 2,
            //    Width = 2,
            //    Height = 3,
            //    TaxCategoryId = _taxCategoryRepository.Table.Where(tc => tc.Name == "Electronics & Software").Single().Id,
            //    ManageInventoryMethod = ManageInventoryMethod.ManageStock,
            //    StockQuantity = 10000,
            //    NotifyAdminForQuantityBelow = 1,
            //    AllowBackInStockSubscriptions = false,
            //    DisplayStockAvailability = true,
            //    LowStockActivity = LowStockActivity.DisableBuyButton,
            //    BackorderMode = BackorderMode.NoBackorders,
            //    OrderMinimumQuantity = 1,
            //    OrderMaximumQuantity = 10000,
            //    Published = true,
            //    DisplayOrder = 1,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow,
            //});
            //productCorel.ProductCategories.Add(new ProductCategory()
            //{
            //    Category = _categoryRepository.Table.Where(c => c.Name == "Software").Single(),
            //    DisplayOrder = 1,
            //});
            //productCorel.ProductPictures.Add(new ProductPicture()
            //{
            //    Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_Corel.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productCorel.Name), true),
            //    DisplayOrder = 1,
            //});
            //_productRepository.Insert(productCorel);





            //var productCustomTShirt = new Product()
            //{
            //    Name = "Custom T-Shirt",
            //    ShortDescription = "T-Shirt - Add Your Content",
            //    FullDescription = "<p>Comfort comes in all shapes and forms, yet this tee out does it all. Rising above the rest, our classic cotton crew provides the simple practicality you need to make it through the day. Tag-free, relaxed fit wears well under dress shirts or stands alone in laid-back style. Reinforced collar and lightweight feel give way to long-lasting shape and breathability. One less thing to worry about, rely on this tee to provide comfort and ease with every wear.</p>",
            //    ProductTemplateId = productTemplateInGrid.Id,
            //    //SeName = "custom-t-shirt",
            //    AllowCustomerReviews = true,
            //    Published = true,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow
            //};
            //allProducts.Add(productCustomTShirt);
            //productCustomTShirt.ProductVariants.Add(new ProductVariant()
            //{
            //    Price = 15M,
            //    IsShipEnabled = true,
            //    Weight = 4,
            //    Length = 3,
            //    Width = 3,
            //    Height = 3,
            //    TaxCategoryId = _taxCategoryRepository.Table.Where(tc => tc.Name == "Apparel & Shoes").Single().Id,
            //    ManageInventoryMethod = ManageInventoryMethod.ManageStock,
            //    StockQuantity = 10000,
            //    NotifyAdminForQuantityBelow = 1,
            //    AllowBackInStockSubscriptions = false,
            //    DisplayStockAvailability = true,
            //    LowStockActivity = LowStockActivity.DisableBuyButton,
            //    BackorderMode = BackorderMode.NoBackorders,
            //    OrderMinimumQuantity = 1,
            //    OrderMaximumQuantity = 10000,
            //    Published = true,
            //    DisplayOrder = 1,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow,
            //});
            //productCustomTShirt.ProductVariants.FirstOrDefault().ProductVariantAttributes.Add(new ProductVariantAttribute()
            //{
            //    ProductAttribute = _productAttributeRepository.Table.Where(x => x.Name == "Custom Text").Single(),
            //    TextPrompt = "Enter your text:",
            //    AttributeControlType = AttributeControlType.TextBox,
            //    IsRequired = true,
            //});

            //productCustomTShirt.ProductCategories.Add(new ProductCategory()
            //{
            //    Category = _categoryRepository.Table.Where(c => c.Name == "Shirts").Single(),
            //    DisplayOrder = 1,
            //});
            //productCustomTShirt.ProductPictures.Add(new ProductPicture()
            //{
            //    Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_CustomTShirt.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productCustomTShirt.Name), true),
            //    DisplayOrder = 1,
            //});
            //_productRepository.Insert(productCustomTShirt);





            //var productDiamondEarrings = new Product()
            //{
            //    Name = "Diamond Pave Earrings",
            //    ShortDescription = "1/2 Carat (ctw) in White Gold",
            //    FullDescription = "<p>Perfect for both a professional look as well as perhaps something more sensual, these 10 karat white gold huggie earrings boast 86 sparkling round diamonds set in a pave arrangement that total 1/2 carat (ctw).</p>",
            //    ProductTemplateId = productTemplateSingleVariant.Id,
            //    //SeName = "diamond-pave-earrings",
            //    AllowCustomerReviews = true,
            //    Published = true,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow
            //};
            //allProducts.Add(productDiamondEarrings);
            //productDiamondEarrings.ProductVariants.Add(new ProductVariant()
            //{
            //    Price = 569M,
            //    IsShipEnabled = true,
            //    Weight = 2,
            //    Length = 2,
            //    Width = 2,
            //    Height = 2,
            //    TaxCategoryId = _taxCategoryRepository.Table.Where(tc => tc.Name == "Jewelry").Single().Id,
            //    ManageInventoryMethod = ManageInventoryMethod.ManageStock,
            //    StockQuantity = 10000,
            //    NotifyAdminForQuantityBelow = 1,
            //    AllowBackInStockSubscriptions = false,
            //    DisplayStockAvailability = true,
            //    LowStockActivity = LowStockActivity.DisableBuyButton,
            //    BackorderMode = BackorderMode.NoBackorders,
            //    OrderMinimumQuantity = 1,
            //    OrderMaximumQuantity = 10000,
            //    Published = true,
            //    DisplayOrder = 1,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow,
            //});
            //productDiamondEarrings.ProductCategories.Add(new ProductCategory()
            //{
            //    Category = _categoryRepository.Table.Where(c => c.Name == "Jewelry").Single(),
            //    DisplayOrder = 1,
            //});
            //productDiamondEarrings.ProductPictures.Add(new ProductPicture()
            //{
            //    Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_DiamondEarrings.jpg"), "image/pjpeg", pictureService.GetPictureSeName(productDiamondEarrings.Name), true),
            //    DisplayOrder = 1,
            //});
            //_productRepository.Insert(productDiamondEarrings);





            //var productDiamondBracelet = new Product()
            //{
            //    Name = "Diamond Tennis Bracelet",
            //    ShortDescription = "1.0 Carat (ctw) in White Gold",
            //    FullDescription = "<p>Jazz up any outfit with this classic diamond tennis bracelet. This piece has one full carat of diamonds uniquely set in brilliant 10 karat white gold.</p>",
            //    ProductTemplateId = productTemplateSingleVariant.Id,
            //    //SeName = "diamond-tennis-bracelet",
            //    AllowCustomerReviews = true,
            //    Published = true,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow
            //};
            //allProducts.Add(productDiamondBracelet);
            //productDiamondBracelet.ProductVariants.Add(new ProductVariant()
            //{
            //    Price = 360M,
            //    IsShipEnabled = true,
            //    Weight = 2,
            //    Length = 2,
            //    Width = 2,
            //    Height = 2,
            //    TaxCategoryId = _taxCategoryRepository.Table.Where(tc => tc.Name == "Jewelry").Single().Id,
            //    ManageInventoryMethod = ManageInventoryMethod.ManageStock,
            //    StockQuantity = 10000,
            //    NotifyAdminForQuantityBelow = 1,
            //    AllowBackInStockSubscriptions = false,
            //    DisplayStockAvailability = true,
            //    LowStockActivity = LowStockActivity.DisableBuyButton,
            //    BackorderMode = BackorderMode.NoBackorders,
            //    OrderMinimumQuantity = 1,
            //    OrderMaximumQuantity = 10000,
            //    Published = true,
            //    DisplayOrder = 1,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow,
            //});
            //productDiamondBracelet.ProductCategories.Add(new ProductCategory()
            //{
            //    Category = _categoryRepository.Table.Where(c => c.Name == "Jewelry").Single(),
            //    DisplayOrder = 1,
            //});
            //productDiamondBracelet.ProductPictures.Add(new ProductPicture()
            //{
            //    Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_DiamondBracelet_1.jpg"), "image/pjpeg", pictureService.GetPictureSeName(productDiamondBracelet.Name), true),
            //    DisplayOrder = 1,
            //});
            //productDiamondBracelet.ProductPictures.Add(new ProductPicture()
            //{
            //    Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_DiamondBracelet_2.jpg"), "image/pjpeg", pictureService.GetPictureSeName(productDiamondBracelet.Name), true),
            //    DisplayOrder = 2,
            //});
            //_productRepository.Insert(productDiamondBracelet);





            //var productEatingWell = new Product()
            //{
            //    Name = "EatingWell in Season",
            //    ShortDescription = "A Farmers' Market Cookbook (Hardcover)",
            //    FullDescription = "<p>Trying to get big chocolate flavor into a crisp holiday cookie is no easy feat. Any decent baker can get a soft, chewy cookie to scream &ldquo;chocolate,&rdquo; but a dough that can withstand a rolling pin and cookie cutters simply can&rsquo;t be too soft. Most chocolate butter cookies skimp on the gooey chocolate and their chocolate flavor is quite modest.</p>",
            //    ProductTemplateId = productTemplateSingleVariant.Id,
            //    //SeName = "eatingwell-in-season",
            //    AllowCustomerReviews = true,
            //    Published = true,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow
            //};
            //allProducts.Add(productEatingWell);
            //productEatingWell.ProductVariants.Add(new ProductVariant()
            //{
            //    Price = 51M,
            //    OldPrice = 67M,
            //    IsShipEnabled = true,
            //    Weight = 2,
            //    Length = 2,
            //    Width = 2,
            //    Height = 2,
            //    TaxCategoryId = _taxCategoryRepository.Table.Where(tc => tc.Name == "Books").Single().Id,
            //    ManageInventoryMethod = ManageInventoryMethod.ManageStock,
            //    StockQuantity = 10000,
            //    NotifyAdminForQuantityBelow = 1,
            //    AllowBackInStockSubscriptions = false,
            //    DisplayStockAvailability = true,
            //    LowStockActivity = LowStockActivity.DisableBuyButton,
            //    BackorderMode = BackorderMode.NoBackorders,
            //    OrderMinimumQuantity = 1,
            //    OrderMaximumQuantity = 10000,
            //    Published = true,
            //    DisplayOrder = 1,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow,
            //});
            //productEatingWell.ProductCategories.Add(new ProductCategory()
            //{
            //    Category = _categoryRepository.Table.Where(c => c.Name == "Books").Single(),
            //    DisplayOrder = 1,
            //});
            //productEatingWell.ProductPictures.Add(new ProductPicture()
            //{
            //    Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_EatingWell.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productEatingWell.Name), true),
            //    DisplayOrder = 1,
            //});
            //_productRepository.Insert(productEatingWell);




            //var productEtnies = new Product()
            //{
            //    Name = "etnies Men's Digit Sneaker",
            //    ShortDescription = "This sleek shoe has all you need--from the padded tongue and collar and internal EVA midsole, to the STI Level 2 cushioning for impact absorption and stability.",
            //    FullDescription = "<p>Established in 1986, etnies is the first skateboarder-owned and skateboarder-operated global action sports footwear and apparel company. etnies not only pushed the envelope by creating the first pro model skate shoe, but it pioneered technological advances and changed the face of skateboard footwear forever. Today, etnies' vision is to remain the leading action sports company committed to creating functional products that provide the most style, comfort, durability and protection possible. etnies stays true to its roots by sponsoring a world-class team of skateboarding, surfing, snowboarding, moto-x, and BMX athletes and continues its dedication by giving back to each of these communities.</p>",
            //    ProductTemplateId = productTemplateInGrid.Id,
            //    //SeName = "etnies-mens-digit-sneaker",
            //    AllowCustomerReviews = true,
            //    Published = true,
            //    ShowOnHomePage = true,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow
            //};
            //allProducts.Add(productEtnies);
            //productEtnies.ProductVariants.Add(new ProductVariant()
            //{
            //    Price = 17.56M,
            //    IsShipEnabled = true,
            //    Weight = 2,
            //    Length = 2,
            //    Width = 2,
            //    Height = 2,
            //    TaxCategoryId = _taxCategoryRepository.Table.Where(tc => tc.Name == "Apparel & Shoes").Single().Id,
            //    ManageInventoryMethod = ManageInventoryMethod.ManageStock,
            //    StockQuantity = 10000,
            //    NotifyAdminForQuantityBelow = 1,
            //    AllowBackInStockSubscriptions = false,
            //    DisplayStockAvailability = true,
            //    LowStockActivity = LowStockActivity.DisableBuyButton,
            //    BackorderMode = BackorderMode.NoBackorders,
            //    OrderMinimumQuantity = 1,
            //    OrderMaximumQuantity = 10000,
            //    Published = true,
            //    DisplayOrder = 1,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow,
            //});
            //var pvaEtnies1 = new ProductVariantAttribute()
            //{
            //    ProductAttribute = _productAttributeRepository.Table.Where(x => x.Name == "Size").Single(),
            //    AttributeControlType = AttributeControlType.DropdownList,
            //    IsRequired = true,
            //};
            //pvaEtnies1.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
            //{
            //    Name = "8",
            //    DisplayOrder = 1,
            //});
            //pvaEtnies1.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
            //{
            //    Name = "9",
            //    DisplayOrder = 2,
            //});
            //pvaEtnies1.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
            //{
            //    Name = "10",
            //    DisplayOrder = 3,
            //});
            //pvaEtnies1.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
            //{
            //    Name = "11",
            //    DisplayOrder = 4,
            //});
            //productEtnies.ProductVariants.FirstOrDefault().ProductVariantAttributes.Add(pvaEtnies1);
            //var pvaEtnies2 = new ProductVariantAttribute()
            //{
            //    ProductAttribute = _productAttributeRepository.Table.Where(x => x.Name == "Color").Single(),
            //    AttributeControlType = AttributeControlType.DropdownList,
            //    IsRequired = true,
            //};
            //pvaEtnies2.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
            //{
            //    Name = "White/Blue",
            //    DisplayOrder = 1,
            //});
            //pvaEtnies2.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
            //{
            //    Name = "White/Black",
            //    DisplayOrder = 2,
            //});
            //productEtnies.ProductVariants.FirstOrDefault().ProductVariantAttributes.Add(pvaEtnies2);
            //productEtnies.ProductCategories.Add(new ProductCategory()
            //{
            //    Category = _categoryRepository.Table.Where(c => c.Name == "Shoes").Single(),
            //    DisplayOrder = 1,
            //});
            //productEtnies.ProductPictures.Add(new ProductPicture()
            //{
            //    Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_Etnies.jpg"), "image/pjpeg", pictureService.GetPictureSeName(productEtnies.Name), true),
            //    DisplayOrder = 1,
            //});
            //_productRepository.Insert(productEtnies);





            //var productLeatherHandbag = new Product()
            //{
            //    Name = "Genuine Leather Handbag with Cell Phone Holder & Many Pockets",
            //    ShortDescription = "Classic Leather Handbag",
            //    FullDescription = "<p>This fine leather handbag will quickly become your favorite bag. It has a zipper organizer on the front that includes a notepad pocket, pen holder, credit card slots and zipper pocket divider. On top of this is a zipper pocket and another flap closure pocket. The main compartment is fully lined and includes a side zipper pocket. On the back is another zipper pocket. And don't forget the convenient built in cell phone holder on the side! The long strap is fully adjustable so you can wear it crossbody or over the shoulder. This is a very well-made, quality leather bag that is not too big, but not too small.</p>",
            //    ProductTemplateId = productTemplateSingleVariant.Id,
            //    //SeName = "genuine-leather-handbag-with-cell-phone-holder-many-pockets",
            //    AllowCustomerReviews = true,
            //    Published = true,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow
            //};
            //allProducts.Add(productLeatherHandbag);
            //productLeatherHandbag.ProductVariants.Add(new ProductVariant()
            //{
            //    Price = 35M,
            //    IsShipEnabled = true,
            //    Weight = 2,
            //    Length = 2,
            //    Width = 2,
            //    Height = 2,
            //    TaxCategoryId = _taxCategoryRepository.Table.Where(tc => tc.Name == "Apparel & Shoes").Single().Id,
            //    ManageInventoryMethod = ManageInventoryMethod.ManageStock,
            //    StockQuantity = 10000,
            //    NotifyAdminForQuantityBelow = 1,
            //    AllowBackInStockSubscriptions = false,
            //    DisplayStockAvailability = true,
            //    LowStockActivity = LowStockActivity.DisableBuyButton,
            //    BackorderMode = BackorderMode.NoBackorders,
            //    OrderMinimumQuantity = 1,
            //    OrderMaximumQuantity = 10000,
            //    Published = true,
            //    DisplayOrder = 1,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow,
            //});
            //productLeatherHandbag.ProductCategories.Add(new ProductCategory()
            //{
            //    Category = _categoryRepository.Table.Where(c => c.Name == "Apparel accessories").Single(),
            //    DisplayOrder = 1,
            //});
            //productLeatherHandbag.ProductPictures.Add(new ProductPicture()
            //{
            //    Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_LeatherHandbag_1.jpg"), "image/pjpeg", pictureService.GetPictureSeName(productLeatherHandbag.Name), true),
            //    DisplayOrder = 1,
            //});
            //productLeatherHandbag.ProductPictures.Add(new ProductPicture()
            //{
            //    Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_LeatherHandbag_2.jpg"), "image/pjpeg", pictureService.GetPictureSeName(productLeatherHandbag.Name), true),
            //    DisplayOrder = 2,
            //});
            //_productRepository.Insert(productLeatherHandbag);





            //var productHp506 = new Product()
            //{
            //    Name = "HP IQ506 TouchSmart Desktop PC",
            //    ShortDescription = "",
            //    FullDescription = "<p>Redesigned with a next-generation, touch-enabled 22-inch high-definition LCD screen, the HP TouchSmart IQ506 all-in-one desktop PC is designed to fit wherever life happens: in the kitchen, family room, or living room. With one touch you can check the weather, download your e-mail, or watch your favorite TV show. It's also designed to maximize energy, with a power-saving Intel Core 2 Duo processor and advanced power management technology, as well as material efficiency--right down to the packaging. It has a sleek piano black design with elegant espresso side-panel highlights, and the HP Ambient Light lets you set a mood--or see your keyboard in the dark.</p>",
            //    ProductTemplateId = productTemplateSingleVariant.Id,
            //    //SeName = "hp-iq506-touchsmart-desktop-pc",
            //    AllowCustomerReviews = true,
            //    Published = true,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow
            //};
            //allProducts.Add(productHp506);
            //productHp506.ProductVariants.Add(new ProductVariant()
            //{
            //    Price = 1199M,
            //    IsShipEnabled = true,
            //    Weight = 7,
            //    Length = 7,
            //    Width = 7,
            //    Height = 7,
            //    TaxCategoryId = _taxCategoryRepository.Table.Where(tc => tc.Name == "Electronics & Software").Single().Id,
            //    ManageInventoryMethod = ManageInventoryMethod.ManageStock,
            //    StockQuantity = 10000,
            //    NotifyAdminForQuantityBelow = 1,
            //    AllowBackInStockSubscriptions = false,
            //    DisplayStockAvailability = true,
            //    LowStockActivity = LowStockActivity.DisableBuyButton,
            //    BackorderMode = BackorderMode.NoBackorders,
            //    OrderMinimumQuantity = 1,
            //    OrderMaximumQuantity = 10000,
            //    Published = true,
            //    DisplayOrder = 1,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow,
            //});
            //productHp506.ProductCategories.Add(new ProductCategory()
            //{
            //    Category = _categoryRepository.Table.Where(c => c.Name == "Desktops").Single(),
            //    DisplayOrder = 1,
            //});
            //productHp506.ProductManufacturers.Add(new ProductManufacturer()
            //{
            //    Manufacturer = _manufacturerRepository.Table.Where(c => c.Name == "HP").Single(),
            //    DisplayOrder = 1,
            //});
            //productHp506.ProductPictures.Add(new ProductPicture()
            //{
            //    Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_Hp506.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productHp506.Name), true),
            //    DisplayOrder = 1,
            //});
            //_productRepository.Insert(productHp506);





            //var productHpPavilion1 = new Product()
            //{
            //    Name = "HP Pavilion Artist Edition DV2890NR 14.1-inch Laptop",
            //    ShortDescription = "Unique Asian-influenced HP imprint wraps the laptop both inside and out",
            //    FullDescription = "<p>Optimize your mobility with a BrightView 14.1-inch display that has the same viewable area as a 15.4-inch screen--in a notebook that weighs a pound less. Encouraging more direct interaction, the backlit media control panel responds to the touch or sweep of a finger. Control settings for audio and video playback from up to 10 feet away with the included HP remote, then store it conveniently in the PC card slot. Enjoy movies or music in seconds with the external DVD or music buttons to launch HP QuickPlay (which bypasses the boot process).</p><p>It's powered by the 1.83 GHz Intel Core 2 Duo T5550 processor, which provides an optimized, multithreaded architecture for improved gaming and multitasking performance, as well as excellent battery management. It also includes Intel's 4965 AGN wireless LAN, which will connect to draft 802.11n routers and offers compatibility with 802.11a/b/g networks as well. It also features a 250 GB hard drive, 3 GB of installed RAM (4 GB maximum), LighScribe dual-layer DVD&plusmn;R burner, HDMI port for connecting to an HDTV, and Nvidia GeForce Go 8400M GS video/graphics card with up to 1407 MB of total allocated video memory (128 MB dedicated). It also includes an integrated Webcam in the LCD's bezel and an omnidirectional microphone for easy video chats.</p>",
            //    ProductTemplateId = productTemplateSingleVariant.Id,
            //    //SeName = "hp-pavilion-artist-edition-dv2890nr-141-inch-laptop",
            //    AllowCustomerReviews = true,
            //    Published = true,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow
            //};
            //allProducts.Add(productHpPavilion1);
            //productHpPavilion1.ProductVariants.Add(new ProductVariant()
            //{
            //    Price = 1590M,
            //    IsShipEnabled = true,
            //    Weight = 7,
            //    Length = 7,
            //    Width = 7,
            //    Height = 7,
            //    TaxCategoryId = _taxCategoryRepository.Table.Where(tc => tc.Name == "Electronics & Software").Single().Id,
            //    ManageInventoryMethod = ManageInventoryMethod.ManageStock,
            //    StockQuantity = 10000,
            //    NotifyAdminForQuantityBelow = 1,
            //    AllowBackInStockSubscriptions = false,
            //    DisplayStockAvailability = true,
            //    LowStockActivity = LowStockActivity.DisableBuyButton,
            //    BackorderMode = BackorderMode.NoBackorders,
            //    OrderMinimumQuantity = 1,
            //    OrderMaximumQuantity = 10000,
            //    Published = true,
            //    DisplayOrder = 1,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow,
            //});
            //productHpPavilion1.ProductCategories.Add(new ProductCategory()
            //{
            //    Category = _categoryRepository.Table.Where(c => c.Name == "Notebooks").Single(),
            //    DisplayOrder = 1,
            //});
            //productHpPavilion1.ProductManufacturers.Add(new ProductManufacturer()
            //{
            //    Manufacturer = _manufacturerRepository.Table.Where(c => c.Name == "HP").Single(),
            //    DisplayOrder = 2,
            //});
            //productHpPavilion1.ProductPictures.Add(new ProductPicture()
            //{
            //    Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_HpPavilion1.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productHpPavilion1.Name), true),
            //    DisplayOrder = 1,
            //});
            //productHpPavilion1.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            //{
            //    AllowFiltering = false,
            //    ShowOnProductPage = true,
            //    DisplayOrder = 1,
            //    SpecificationAttributeOption = _specificationAttributeRepository.Table.Where(sa => sa.Name == "Screensize").Single().SpecificationAttributeOptions.Where(sao => sao.Name == "14.1''").Single()
            //});
            //productHpPavilion1.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            //{
            //    AllowFiltering = true,
            //    ShowOnProductPage = true,
            //    DisplayOrder = 2,
            //    SpecificationAttributeOption = _specificationAttributeRepository.Table.Where(sa => sa.Name == "CPU Type").Single().SpecificationAttributeOptions.Where(sao => sao.Name == "Intel").Single()
            //});
            //productHpPavilion1.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            //{
            //    AllowFiltering = true,
            //    ShowOnProductPage = true,
            //    DisplayOrder = 3,
            //    SpecificationAttributeOption = _specificationAttributeRepository.Table.Where(sa => sa.Name == "Memory").Single().SpecificationAttributeOptions.Where(sao => sao.Name == "3 GB").Single()
            //});
            //productHpPavilion1.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            //{
            //    AllowFiltering = false,
            //    ShowOnProductPage = true,
            //    DisplayOrder = 4,
            //    SpecificationAttributeOption = _specificationAttributeRepository.Table.Where(sa => sa.Name == "Hardrive").Single().SpecificationAttributeOptions.Where(sao => sao.Name == "250 GB").Single()
            //});
            //_productRepository.Insert(productHpPavilion1);





            //var productHpPavilion2 = new Product()
            //{
            //    Name = "HP Pavilion Elite M9150F Desktop PC",
            //    ShortDescription = "Top-of-the-line multimedia desktop featuring 2.4 GHz Intel Core 2 Quad Processor Q6600 with four lightning fast execution cores",
            //    FullDescription = "<p>The updated chassis with sleek piano black paneling and components is far from the most significant improvements in the multimedia powerhouse HP Pavilion Elite m9150f desktop PC. It's powered by Intel's newest processor--the 2.4 GHz Intel Core 2 Quad Q6600--which delivers four complete execution cores within a single processor for unprecedented performance and responsiveness in multi-threaded and multi-tasking business/home environments. You can also go wireless and clutter-free with wireless keyboard, mouse, and remote control, and it includes the next step in Wi-Fi networking with a 54g wireless LAN (802.11b/g).</p>",
            //    ProductTemplateId = productTemplateSingleVariant.Id,
            //    //SeName = "hp-pavilion-elite-m9150f-desktop-pc",
            //    AllowCustomerReviews = true,
            //    Published = true,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow
            //};
            //allProducts.Add(productHpPavilion2);
            //productHpPavilion2.ProductVariants.Add(new ProductVariant()
            //{
            //    Price = 1350M,
            //    IsShipEnabled = true,
            //    Weight = 7,
            //    Length = 7,
            //    Width = 7,
            //    Height = 7,
            //    TaxCategoryId = _taxCategoryRepository.Table.Where(tc => tc.Name == "Electronics & Software").Single().Id,
            //    ManageInventoryMethod = ManageInventoryMethod.ManageStock,
            //    StockQuantity = 10000,
            //    NotifyAdminForQuantityBelow = 1,
            //    AllowBackInStockSubscriptions = false,
            //    DisplayStockAvailability = true,
            //    LowStockActivity = LowStockActivity.DisableBuyButton,
            //    BackorderMode = BackorderMode.NoBackorders,
            //    OrderMinimumQuantity = 1,
            //    OrderMaximumQuantity = 10000,
            //    Published = true,
            //    DisplayOrder = 1,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow,
            //});
            //productHpPavilion2.ProductCategories.Add(new ProductCategory()
            //{
            //    Category = _categoryRepository.Table.Where(c => c.Name == "Desktops").Single(),
            //    DisplayOrder = 1,
            //});
            //productHpPavilion2.ProductManufacturers.Add(new ProductManufacturer()
            //{
            //    Manufacturer = _manufacturerRepository.Table.Where(c => c.Name == "HP").Single(),
            //    DisplayOrder = 3,
            //});
            //productHpPavilion2.ProductPictures.Add(new ProductPicture()
            //{
            //    Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_HpPavilion2_1.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productHpPavilion2.Name), true),
            //    DisplayOrder = 1,
            //});
            //productHpPavilion2.ProductPictures.Add(new ProductPicture()
            //{
            //    Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_HpPavilion2_2.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productHpPavilion2.Name), true),
            //    DisplayOrder = 2,
            //});
            //_productRepository.Insert(productHpPavilion2);





            //var productHpPavilion3 = new Product()
            //{
            //    Name = "HP Pavilion G60-230US 16.0-Inch Laptop",
            //    ShortDescription = "Streamlined multimedia laptop with 16-inch screen for basic computing, entertainment and online communication",
            //    FullDescription = "<p>Chat face to face, or take pictures and video clips with the webcam and integrated digital microphone. Play games and enhance multimedia with the Intel GMA 4500M with up to 1309 MB of total available graphics memory. And enjoy movies or music in seconds with the external DVD or music buttons to launch HP QuickPlay (which bypasses the boot process).  It offers dual-core productivity from its 2.0 GHz Intel Pentium T4200 processor for excellent multitasking. Other features include a 320 GB hard drive, 3 GB of installed RAM (4 GB maximum capacity), dual-layer DVD&plusmn;RW drive (which also burns CDs), quad-mode Wi-Fi (802.11a/b/g/n), 5-in-1 memory card reader, and pre-installed Windows Vista Home Premium (SP1).</p>",
            //    ProductTemplateId = productTemplateSingleVariant.Id,
            //    //SeName = "hp-pavilion-g60-230us-160-inch-laptop",
            //    AllowCustomerReviews = true,
            //    Published = true,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow
            //};
            //allProducts.Add(productHpPavilion3);
            //productHpPavilion3.ProductVariants.Add(new ProductVariant()
            //{
            //    Price = 1460M,
            //    IsShipEnabled = true,
            //    Weight = 7,
            //    Length = 7,
            //    Width = 7,
            //    Height = 7,
            //    TaxCategoryId = _taxCategoryRepository.Table.Where(tc => tc.Name == "Electronics & Software").Single().Id,
            //    ManageInventoryMethod = ManageInventoryMethod.ManageStock,
            //    StockQuantity = 10000,
            //    NotifyAdminForQuantityBelow = 1,
            //    AllowBackInStockSubscriptions = false,
            //    DisplayStockAvailability = true,
            //    LowStockActivity = LowStockActivity.DisableBuyButton,
            //    BackorderMode = BackorderMode.NoBackorders,
            //    OrderMinimumQuantity = 1,
            //    OrderMaximumQuantity = 10000,
            //    Published = true,
            //    DisplayOrder = 1,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow,
            //});
            //productHpPavilion3.ProductCategories.Add(new ProductCategory()
            //{
            //    Category = _categoryRepository.Table.Where(c => c.Name == "Notebooks").Single(),
            //    DisplayOrder = 1,
            //});
            //productHpPavilion3.ProductManufacturers.Add(new ProductManufacturer()
            //{
            //    Manufacturer = _manufacturerRepository.Table.Where(c => c.Name == "HP").Single(),
            //    DisplayOrder = 4,
            //});
            //productHpPavilion3.ProductPictures.Add(new ProductPicture()
            //{
            //    Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_HpPavilion3.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productHpPavilion3.Name), true),
            //    DisplayOrder = 1,
            //});
            //productHpPavilion3.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            //{
            //    AllowFiltering = false,
            //    ShowOnProductPage = true,
            //    DisplayOrder = 1,
            //    SpecificationAttributeOption = _specificationAttributeRepository.Table.Where(sa => sa.Name == "Screensize").Single().SpecificationAttributeOptions.Where(sao => sao.Name == "16.0''").Single()
            //});
            //productHpPavilion3.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            //{
            //    AllowFiltering = true,
            //    ShowOnProductPage = true,
            //    DisplayOrder = 2,
            //    SpecificationAttributeOption = _specificationAttributeRepository.Table.Where(sa => sa.Name == "CPU Type").Single().SpecificationAttributeOptions.Where(sao => sao.Name == "Intel").Single()
            //});
            //productHpPavilion3.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            //{
            //    AllowFiltering = true,
            //    ShowOnProductPage = true,
            //    DisplayOrder = 3,
            //    SpecificationAttributeOption = _specificationAttributeRepository.Table.Where(sa => sa.Name == "Memory").Single().SpecificationAttributeOptions.Where(sao => sao.Name == "3 GB").Single()
            //});
            //productHpPavilion3.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            //{
            //    AllowFiltering = false,
            //    ShowOnProductPage = true,
            //    DisplayOrder = 4,
            //    SpecificationAttributeOption = _specificationAttributeRepository.Table.Where(sa => sa.Name == "Hardrive").Single().SpecificationAttributeOptions.Where(sao => sao.Name == "320 GB").Single()
            //});
            //_productRepository.Insert(productHpPavilion3);





            //var productHat = new Product()
            //{
            //    Name = "Indiana Jones® Shapeable Wool Hat",
            //    ShortDescription = "Wear some adventure with the same hat Indiana Jones&reg; wears in his movies.",
            //    FullDescription = "<p>Wear some adventure with the same hat Indiana Jones&reg; wears in his movies. Easy to shape to fit your personal style. Wool. Import. Please Note - Due to new UPS shipping rules and the size of the box, if you choose to expedite your hat order (UPS 3-day, 2-day or Overnight), an additional non-refundable $20 shipping charge per hat will be added at the time your order is processed.</p>",
            //    ProductTemplateId = productTemplateInGrid.Id,
            //    //SeName = "indiana-jones-shapeable-wool-hat",
            //    AllowCustomerReviews = true,
            //    Published = true,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow
            //};
            //allProducts.Add(productHat);
            //productHat.ProductVariants.Add(new ProductVariant()
            //{
            //    Price = 30M,
            //    IsShipEnabled = true,
            //    Weight = 2,
            //    Length = 2,
            //    Width = 2,
            //    Height = 2,
            //    TaxCategoryId = _taxCategoryRepository.Table.Where(tc => tc.Name == "Apparel & Shoes").Single().Id,
            //    ManageInventoryMethod = ManageInventoryMethod.ManageStock,
            //    StockQuantity = 10000,
            //    NotifyAdminForQuantityBelow = 1,
            //    AllowBackInStockSubscriptions = false,
            //    DisplayStockAvailability = true,
            //    LowStockActivity = LowStockActivity.DisableBuyButton,
            //    BackorderMode = BackorderMode.NoBackorders,
            //    OrderMinimumQuantity = 1,
            //    OrderMaximumQuantity = 10000,
            //    Published = true,
            //    DisplayOrder = 1,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow,
            //});
            //var pvaHat1 = new ProductVariantAttribute()
            //{
            //    ProductAttribute = _productAttributeRepository.Table.Where(x => x.Name == "Size").Single(),
            //    AttributeControlType = AttributeControlType.DropdownList,
            //    IsRequired = true,
            //};
            //pvaHat1.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
            //{
            //    Name = "Small",
            //    DisplayOrder = 1,
            //});
            //pvaHat1.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
            //{
            //    Name = "Medium",
            //    DisplayOrder = 2,
            //});
            //pvaHat1.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
            //{
            //    Name = "Large",
            //    DisplayOrder = 3,
            //});
            //pvaHat1.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
            //{
            //    Name = "X-Large",
            //    DisplayOrder = 4,
            //});
            //productHat.ProductVariants.FirstOrDefault().ProductVariantAttributes.Add(pvaHat1);
            //productHat.ProductCategories.Add(new ProductCategory()
            //{
            //    Category = _categoryRepository.Table.Where(c => c.Name == "Apparel accessories").Single(),
            //    DisplayOrder = 1,
            //});
            //productHat.ProductPictures.Add(new ProductPicture()
            //{
            //    Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_hat.jpg"), "image/pjpeg", pictureService.GetPictureSeName(productHat.Name), true),
            //    DisplayOrder = 1,
            //});
            //_productRepository.Insert(productHat);





            //var productKensington = new Product()
            //{
            //    Name = "Kensington 33117 International All-in-One Travel Plug Adapter",
            //    ShortDescription = "Includes plug adapters for use in more than 150 countries",
            //    FullDescription = "<p>The Kensington 33117 Travel Plug Adapter is a pocket-sized power adapter for go-anywhere convenience. This all-in-one unit provides plug adapters for use in more than 150 countries, so you never need to be at a loss for power again. The Kensington 33117 is easy to use, with slide-out power plugs that ensure you won't lose any vital pieces, in a compact, self-contained unit that eliminates any hassles. This all-in-one plug adapts power outlets for laptops, chargers, and similar devices, and features a safety release button and built-in fuse to ensure safe operation. The Kensington 33117 does not reduce or convert electrical voltage, is suitable for most consumer electronics ranging from 110-volts to Mac 275-watts, to 220-volts to Mac 550-watts. Backed by Kensington's one-year warranty, this unit weighs 0.5, and measures 1.875 x 2 x 2.25 inches (WxDxH). Please note that this adapter is not designed for use with high-watt devices such as hairdryers and irons, so users should check electronic device specifications before using.</p>",
            //    ProductTemplateId = productTemplateSingleVariant.Id,
            //    //SeName = "kensington-33117-international-all-in-one-travel-plug-adapter",
            //    AllowCustomerReviews = true,
            //    Published = true,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow
            //};
            //allProducts.Add(productKensington);
            //productKensington.ProductVariants.Add(new ProductVariant()
            //{
            //    Price = 35M,
            //    IsShipEnabled = true,
            //    Weight = 7,
            //    Length = 7,
            //    Width = 7,
            //    Height = 7,
            //    TaxCategoryId = _taxCategoryRepository.Table.Where(tc => tc.Name == "Electronics & Software").Single().Id,
            //    ManageInventoryMethod = ManageInventoryMethod.ManageStock,
            //    StockQuantity = 10000,
            //    NotifyAdminForQuantityBelow = 1,
            //    AllowBackInStockSubscriptions = false,
            //    DisplayStockAvailability = true,
            //    LowStockActivity = LowStockActivity.DisableBuyButton,
            //    BackorderMode = BackorderMode.NoBackorders,
            //    OrderMinimumQuantity = 1,
            //    OrderMaximumQuantity = 10000,
            //    Published = true,
            //    DisplayOrder = 1,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow,
            //});
            //productKensington.ProductCategories.Add(new ProductCategory()
            //{
            //    Category = _categoryRepository.Table.Where(c => c.Name == "Accessories").Single(),
            //    DisplayOrder = 1,
            //});
            //productKensington.ProductPictures.Add(new ProductPicture()
            //{
            //    Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_Kensington.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productKensington.Name), true),
            //    DisplayOrder = 1,
            //});
            //_productRepository.Insert(productKensington);





            //var productLeviJeans = new Product()
            //{
            //    Name = "Levi's Skinny 511 Jeans",
            //    ShortDescription = "Levi's Faded Black Skinny 511 Jeans ",
            //    FullDescription = "",
            //    ProductTemplateId = productTemplateSingleVariant.Id,
            //    //SeName = "levis-skinny-511-jeans",
            //    AllowCustomerReviews = true,
            //    Published = true,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow
            //};
            //allProducts.Add(productLeviJeans);
            //productLeviJeans.ProductVariants.Add(new ProductVariant()
            //{
            //    Price = 43.5M,
            //    OldPrice = 55M,
            //    IsShipEnabled = true,
            //    Weight = 2,
            //    Length = 2,
            //    Width = 2,
            //    Height = 2,
            //    TaxCategoryId = _taxCategoryRepository.Table.Where(tc => tc.Name == "Apparel & Shoes").Single().Id,
            //    ManageInventoryMethod = ManageInventoryMethod.ManageStock,
            //    StockQuantity = 10000,
            //    NotifyAdminForQuantityBelow = 1,
            //    AllowBackInStockSubscriptions = false,
            //    DisplayStockAvailability = true,
            //    LowStockActivity = LowStockActivity.DisableBuyButton,
            //    BackorderMode = BackorderMode.NoBackorders,
            //    OrderMinimumQuantity = 1,
            //    OrderMaximumQuantity = 10000,
            //    Published = true,
            //    DisplayOrder = 1,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow,
            //});
            //productLeviJeans.ProductVariants.FirstOrDefault().TierPrices.Add(new TierPrice()
            //{
            //    Quantity = 3,
            //    Price = 40
            //});
            //productLeviJeans.ProductVariants.FirstOrDefault().TierPrices.Add(new TierPrice()
            //{
            //    Quantity = 6,
            //    Price = 38
            //});
            //productLeviJeans.ProductVariants.FirstOrDefault().TierPrices.Add(new TierPrice()
            //{
            //    Quantity = 10,
            //    Price = 35
            //});
            //productLeviJeans.ProductVariants.FirstOrDefault().HasTierPrices = true;

            //productLeviJeans.ProductCategories.Add(new ProductCategory()
            //{
            //    Category = _categoryRepository.Table.Where(c => c.Name == "Jeans").Single(),
            //    DisplayOrder = 1,
            //});
            //productLeviJeans.ProductPictures.Add(new ProductPicture()
            //{
            //    Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_LeviJeans_1.jpg"), "image/pjpeg", pictureService.GetPictureSeName(productLeviJeans.Name), true),
            //    DisplayOrder = 1,
            //});
            //productLeviJeans.ProductPictures.Add(new ProductPicture()
            //{
            //    Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_LeviJeans_2.jpg"), "image/pjpeg", pictureService.GetPictureSeName(productLeviJeans.Name), true),
            //    DisplayOrder = 2,
            //});
            //_productRepository.Insert(productLeviJeans);





            //var productBaseball = new Product()
            //{
            //    Name = "Major League Baseball 2K9",
            //    ShortDescription = "Take charge of your franchise and enjoy the all-new MLB.com presentation style",
            //    FullDescription = "<p>Major League Baseball 2K9 captures the essence of baseball down to some of the most minute, player- specific details including batting stances, pitching windups and signature swings. 2K Sports has gone above and beyond the call of duty to deliver this in true major league fashion. Additionally, gameplay enhancements in pitching, batting, fielding and base running promise this year's installment to be user-friendly and enjoyable for rookies or veterans. New commentary and presentation provide the icing to this ultimate baseball experience. If you really want to Play Ball this is the game for you.</p>",
            //    ProductTemplateId = productTemplateSingleVariant.Id,
            //    //SeName = "major-league-baseball-2k9",
            //    AllowCustomerReviews = true,
            //    Published = true,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow
            //};
            //allProducts.Add(productBaseball);
            //productBaseball.ProductVariants.Add(new ProductVariant()
            //{
            //    Price = 14.99M,
            //    IsShipEnabled = true,
            //    Weight = 7,
            //    Length = 7,
            //    Width = 7,
            //    Height = 7,
            //    TaxCategoryId = _taxCategoryRepository.Table.Where(tc => tc.Name == "Electronics & Software").Single().Id,
            //    ManageInventoryMethod = ManageInventoryMethod.ManageStock,
            //    StockQuantity = 10000,
            //    NotifyAdminForQuantityBelow = 1,
            //    AllowBackInStockSubscriptions = false,
            //    DisplayStockAvailability = true,
            //    LowStockActivity = LowStockActivity.DisableBuyButton,
            //    BackorderMode = BackorderMode.NoBackorders,
            //    OrderMinimumQuantity = 1,
            //    OrderMaximumQuantity = 10000,
            //    Published = true,
            //    DisplayOrder = 1,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow,
            //});
            //productBaseball.ProductCategories.Add(new ProductCategory()
            //{
            //    Category = _categoryRepository.Table.Where(c => c.Name == "Games").Single(),
            //    DisplayOrder = 1,
            //});
            //productBaseball.ProductPictures.Add(new ProductPicture()
            //{
            //    Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_Baseball.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productBaseball.Name), true),
            //    DisplayOrder = 1,
            //});
            //_productRepository.Insert(productBaseball);





            //var productMedalOfHonor = new Product()
            //{
            //    Name = "Medal of Honor - Limited Edition (Xbox 360)",
            //    ShortDescription = "One of the great pioneers in military simulations returns to gaming as the Medal of Honor series depicts modern warfare for the first time, with a harrowing tour of duty in current day Afghanistan.",
            //    FullDescription = "You'll take control of both ordinary U.S. Army Rangers and Tier 1 Elite Ops Special Forces as you fight enemy insurgents in the most dangerous theatre of war of the modern age. The intense first person combat has been created with input from U.S. military consultants and based on real-life descriptions from veteran soldiers. This allows you to use genuine military tactics and advanced technology including combat drones and targeted air strikes.",
            //    ProductTemplateId = productTemplateSingleVariant.Id,
            //    //SeName = "medal-of-honor-limited-edition-xbox-360",
            //    AllowCustomerReviews = true,
            //    Published = true,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow
            //};
            //allProducts.Add(productMedalOfHonor);
            //productMedalOfHonor.ProductVariants.Add(new ProductVariant()
            //{
            //    Price = 37M,
            //    IsShipEnabled = true,
            //    Weight = 7,
            //    Length = 7,
            //    Width = 7,
            //    Height = 7,
            //    TaxCategoryId = _taxCategoryRepository.Table.Where(tc => tc.Name == "Electronics & Software").Single().Id,
            //    ManageInventoryMethod = ManageInventoryMethod.ManageStock,
            //    StockQuantity = 10000,
            //    NotifyAdminForQuantityBelow = 1,
            //    AllowBackInStockSubscriptions = false,
            //    DisplayStockAvailability = true,
            //    LowStockActivity = LowStockActivity.DisableBuyButton,
            //    BackorderMode = BackorderMode.NoBackorders,
            //    OrderMinimumQuantity = 1,
            //    OrderMaximumQuantity = 10000,
            //    Published = true,
            //    DisplayOrder = 1,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow,
            //});
            //productMedalOfHonor.ProductCategories.Add(new ProductCategory()
            //{
            //    Category = _categoryRepository.Table.Where(c => c.Name == "Games").Single(),
            //    DisplayOrder = 1,
            //});
            //productMedalOfHonor.ProductPictures.Add(new ProductPicture()
            //{
            //    Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_MedalOfHonor.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productMedalOfHonor.Name), true),
            //    DisplayOrder = 1,
            //});
            //_productRepository.Insert(productMedalOfHonor);





            //var productMouse = new Product()
            //{
            //    Name = "Microsoft Bluetooth Notebook Mouse 5000 Mac/Windows",
            //    ShortDescription = "Enjoy reliable, transceiver-free wireless connection to your PC with Bluetooth Technology",
            //    FullDescription = "<p>Enjoy wireless freedom with the Microsoft&reg; Bluetooth&reg; Notebook Mouse 5000 &mdash; no transceiver to connect or lose! Keep USB ports free for other devices. And, take it with you in a convenient carrying case (included)</p>",
            //    ProductTemplateId = productTemplateInGrid.Id,
            //    //SeName = "microsoft-bluetooth-notebook-mouse-5000-macwindows",
            //    AllowCustomerReviews = true,
            //    Published = true,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow
            //};
            //allProducts.Add(productMouse);
            //productMouse.ProductVariants.Add(new ProductVariant()
            //{
            //    Price = 37M,
            //    IsShipEnabled = true,
            //    Weight = 7,
            //    Length = 7,
            //    Width = 7,
            //    Height = 7,
            //    TaxCategoryId = _taxCategoryRepository.Table.Where(tc => tc.Name == "Electronics & Software").Single().Id,
            //    ManageInventoryMethod = ManageInventoryMethod.ManageStock,
            //    StockQuantity = 10000,
            //    NotifyAdminForQuantityBelow = 1,
            //    AllowBackInStockSubscriptions = false,
            //    DisplayStockAvailability = true,
            //    LowStockActivity = LowStockActivity.DisableBuyButton,
            //    BackorderMode = BackorderMode.NoBackorders,
            //    OrderMinimumQuantity = 1,
            //    OrderMaximumQuantity = 10000,
            //    Published = true,
            //    DisplayOrder = 1,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow,
            //});
            //productMouse.ProductCategories.Add(new ProductCategory()
            //{
            //    Category = _categoryRepository.Table.Where(c => c.Name == "Accessories").Single(),
            //    DisplayOrder = 1,
            //});
            //productMouse.ProductPictures.Add(new ProductPicture()
            //{
            //    Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_Mouse.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productMouse.Name), true),
            //    DisplayOrder = 1,
            //});
            //_productRepository.Insert(productMouse);





            //var productGolfBelt = new Product()
            //{
            //    Name = "NIKE Golf Casual Belt",
            //    ShortDescription = "NIKE Golf Casual Belt is a great look for in the clubhouse after a round of golf.",
            //    FullDescription = "<p>NIKE Golf Casual Belt is a great look for in the clubhouse after a round of golf. The belt strap is made of full grain oil tanned leather. The buckle is made of antique brushed metal with an embossed Swoosh design on it. This belt features an English beveled edge with rivets on the tab and tip of the 38mm wide strap. Size: 32; Color: Black.</p>",
            //    ProductTemplateId = productTemplateSingleVariant.Id,
            //    //SeName = "nike-golf-casual-belt",
            //    AllowCustomerReviews = true,
            //    Published = true,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow
            //};
            //allProducts.Add(productGolfBelt);
            //productGolfBelt.ProductVariants.Add(new ProductVariant()
            //{
            //    Price = 45M,
            //    IsShipEnabled = true,
            //    Weight = 7,
            //    Length = 7,
            //    Width = 7,
            //    Height = 7,
            //    TaxCategoryId = _taxCategoryRepository.Table.Where(tc => tc.Name == "Apparel & Shoes").Single().Id,
            //    ManageInventoryMethod = ManageInventoryMethod.ManageStock,
            //    StockQuantity = 10000,
            //    NotifyAdminForQuantityBelow = 1,
            //    AllowBackInStockSubscriptions = false,
            //    DisplayStockAvailability = true,
            //    LowStockActivity = LowStockActivity.DisableBuyButton,
            //    BackorderMode = BackorderMode.NoBackorders,
            //    OrderMinimumQuantity = 1,
            //    OrderMaximumQuantity = 10000,
            //    Published = true,
            //    DisplayOrder = 1,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow,
            //});
            //productGolfBelt.ProductCategories.Add(new ProductCategory()
            //{
            //    Category = _categoryRepository.Table.Where(c => c.Name == "Apparel accessories").Single(),
            //    DisplayOrder = 1,
            //});
            //productGolfBelt.ProductPictures.Add(new ProductPicture()
            //{
            //    Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_GolfBelt.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productGolfBelt.Name), true),
            //    DisplayOrder = 1,
            //});
            //_productRepository.Insert(productGolfBelt);





            //var productPanasonic = new Product()
            //{
            //    Name = "Panasonic HDC-SDT750K, High Definition 3D Camcorder",
            //    ShortDescription = "World's first 3D Shooting Camcorder",
            //    FullDescription = "<p>Unlike previous 3D images that required complex, professional equipment to create, now you can shoot your own. Simply attach the 3D Conversion Lens to the SDT750 for quick and easy 3D shooting. And because the SDT750 features the Advanced 3MOS System, which has gained worldwide popularity, colors are vivid and 3D images are extremely realistic. Let the SDT750 save precious moments for you in true-to-life images.</p>",
            //    ProductTemplateId = productTemplateSingleVariant.Id,
            //    //SeName = "panasonic-hdc-sdt750k-high-definition-3d-camcorder",
            //    AllowCustomerReviews = true,
            //    Published = true,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow
            //};
            //allProducts.Add(productPanasonic);
            //productPanasonic.ProductVariants.Add(new ProductVariant()
            //{
            //    Price = 1300M,
            //    IsShipEnabled = true,
            //    Weight = 7,
            //    Length = 7,
            //    Width = 7,
            //    Height = 7,
            //    TaxCategoryId = _taxCategoryRepository.Table.Where(tc => tc.Name == "Electronics & Software").Single().Id,
            //    ManageInventoryMethod = ManageInventoryMethod.ManageStock,
            //    StockQuantity = 10000,
            //    NotifyAdminForQuantityBelow = 1,
            //    AllowBackInStockSubscriptions = false,
            //    DisplayStockAvailability = true,
            //    LowStockActivity = LowStockActivity.DisableBuyButton,
            //    BackorderMode = BackorderMode.NoBackorders,
            //    OrderMinimumQuantity = 1,
            //    OrderMaximumQuantity = 10000,
            //    Published = true,
            //    DisplayOrder = 1,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow,
            //});
            //productPanasonic.ProductCategories.Add(new ProductCategory()
            //{
            //    Category = _categoryRepository.Table.Where(c => c.Name == "Camera, photo").Single(),
            //    DisplayOrder = 1,
            //});
            //productPanasonic.ProductPictures.Add(new ProductPicture()
            //{
            //    Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_productPanasonic.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productPanasonic.Name), true),
            //    DisplayOrder = 1,
            //});
            //_productRepository.Insert(productPanasonic);





            //var productSunglasses = new Product()
            //{
            //    Name = "Ray Ban Aviator Sunglasses RB 3025",
            //    ShortDescription = "Aviator sunglasses are one of the first widely popularized styles of modern day sunwear.",
            //    FullDescription = "<p>Since 1937, Ray-Ban can genuinely claim the title as the world's leading sunglasses and optical eyewear brand. Combining the best of fashion and sports performance, the Ray-Ban line of Sunglasses delivers a truly classic style that will have you looking great today and for years to come.</p>",
            //    ProductTemplateId = productTemplateSingleVariant.Id,
            //    //SeName = "ray-ban-aviator-sunglasses-rb-3025",
            //    AllowCustomerReviews = true,
            //    Published = true,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow
            //};
            //allProducts.Add(productSunglasses);
            //productSunglasses.ProductVariants.Add(new ProductVariant()
            //{
            //    Price = 25M,
            //    IsShipEnabled = true,
            //    Weight = 7,
            //    Length = 7,
            //    Width = 7,
            //    Height = 7,
            //    TaxCategoryId = _taxCategoryRepository.Table.Where(tc => tc.Name == "Apparel & Shoes").Single().Id,
            //    ManageInventoryMethod = ManageInventoryMethod.ManageStock,
            //    StockQuantity = 10000,
            //    NotifyAdminForQuantityBelow = 1,
            //    AllowBackInStockSubscriptions = false,
            //    DisplayStockAvailability = true,
            //    LowStockActivity = LowStockActivity.DisableBuyButton,
            //    BackorderMode = BackorderMode.NoBackorders,
            //    OrderMinimumQuantity = 1,
            //    OrderMaximumQuantity = 10000,
            //    Published = true,
            //    DisplayOrder = 1,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow,
            //});
            //productSunglasses.ProductCategories.Add(new ProductCategory()
            //{
            //    Category = _categoryRepository.Table.Where(c => c.Name == "Apparel accessories").Single(),
            //    DisplayOrder = 1,
            //});
            //productSunglasses.ProductPictures.Add(new ProductPicture()
            //{
            //    Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_Sunglasses.jpg"), "image/pjpeg", pictureService.GetPictureSeName(productSunglasses.Name), true),
            //    DisplayOrder = 1,
            //});
            //_productRepository.Insert(productSunglasses);





            //var productSamsungPhone = new Product()
            //{
            //    Name = "Samsung Rugby A837 Phone, Black (AT&T)",
            //    ShortDescription = "Ruggedized 3G handset in black great for outdoor workforces",
            //    FullDescription = "<p>Ideal for on-site field services, the ruggedized Samsung Rugby for AT&amp;T can take just about anything you can throw at it. This highly durable handset is certified to Military Standard MIL-STD 810F standards that's perfect for users like construction foremen and landscape designers. In addition to access to AT&amp;T Navigation turn-by-turn direction service, the Rugby also features compatibility with Push to Talk communication, Enterprise Paging, and AT&amp;T's breakthrough Video Share calling services. This quad-band GSM phone runs on AT&amp;T's dual-band 3G (HSDPA/UMTS) network, for fast downloads and seamless video calls. It also offers a 1.3-megapixel camera, microSD memory expansion to 8 GB, Bluetooth for handsfree communication and stereo music streaming, access to personal email and instant messaging, and up to 5 hours of talk time.</p>",
            //    ProductTemplateId = productTemplateSingleVariant.Id,
            //    //SeName = "samsung-rugby-a837-phone-black-att",
            //    AllowCustomerReviews = true,
            //    Published = true,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow
            //};
            //allProducts.Add(productSamsungPhone);
            //productSamsungPhone.ProductVariants.Add(new ProductVariant()
            //{
            //    Price = 100M,
            //    IsShipEnabled = true,
            //    Weight = 7,
            //    Length = 7,
            //    Width = 7,
            //    Height = 7,
            //    TaxCategoryId = _taxCategoryRepository.Table.Where(tc => tc.Name == "Electronics & Software").Single().Id,
            //    ManageInventoryMethod = ManageInventoryMethod.ManageStock,
            //    StockQuantity = 10000,
            //    NotifyAdminForQuantityBelow = 1,
            //    AllowBackInStockSubscriptions = false,
            //    DisplayStockAvailability = true,
            //    LowStockActivity = LowStockActivity.DisableBuyButton,
            //    BackorderMode = BackorderMode.NoBackorders,
            //    OrderMinimumQuantity = 1,
            //    OrderMaximumQuantity = 10000,
            //    Published = true,
            //    DisplayOrder = 1,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow,
            //});
            //productSamsungPhone.ProductCategories.Add(new ProductCategory()
            //{
            //    Category = _categoryRepository.Table.Where(c => c.Name == "Cell phones").Single(),
            //    DisplayOrder = 1,
            //});
            //productSamsungPhone.ProductPictures.Add(new ProductPicture()
            //{
            //    Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_SamsungPhone_1.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productSamsungPhone.Name), true),
            //    DisplayOrder = 1,
            //});
            //productSamsungPhone.ProductPictures.Add(new ProductPicture()
            //{
            //    Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_SamsungPhone_2.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productSamsungPhone.Name), true),
            //    DisplayOrder = 2,
            //});
            //_productRepository.Insert(productSamsungPhone);





            //var productSonyCamcoder = new Product()
            //{
            //    Name = "Sony DCR-SR85 1MP 60GB Hard Drive Handycam Camcorder",
            //    ShortDescription = "Capture video to hard disk drive; 60 GB storage",
            //    FullDescription = "<p>You&rsquo;ll never miss a moment because of switching tapes or discs with the DCR-SR85. Its built-in 60GB hard disk drive offers plenty of storage as you zero in on your subjects with the professional-quality Carl Zeiss Vario-Tessar lens and a powerful 25x optical/2000x digital zoom. Compose shots using the 2.7-inch wide (16:9) touch-panel LCD display, and maintain total control and clarity with the Super SteadyShot image stabilization system. Hybrid recording technology even gives you the choice to record video to either the internal hard disk drive or removable Memory Stick Pro Duo media.</p>",
            //    ProductTemplateId = productTemplateSingleVariant.Id,
            //    //SeName = "sony-dcr-sr85-1mp-60gb-hard-drive-handycam-camcorder",
            //    AllowCustomerReviews = true,
            //    Published = true,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow
            //};
            //allProducts.Add(productSonyCamcoder);
            //productSonyCamcoder.ProductVariants.Add(new ProductVariant()
            //{
            //    Price = 349M,
            //    IsShipEnabled = true,
            //    Weight = 7,
            //    Length = 7,
            //    Width = 7,
            //    Height = 7,
            //    TaxCategoryId = _taxCategoryRepository.Table.Where(tc => tc.Name == "Electronics & Software").Single().Id,
            //    ManageInventoryMethod = ManageInventoryMethod.ManageStock,
            //    StockQuantity = 10000,
            //    NotifyAdminForQuantityBelow = 1,
            //    AllowBackInStockSubscriptions = false,
            //    DisplayStockAvailability = true,
            //    LowStockActivity = LowStockActivity.DisableBuyButton,
            //    BackorderMode = BackorderMode.NoBackorders,
            //    OrderMinimumQuantity = 1,
            //    OrderMaximumQuantity = 10000,
            //    Published = true,
            //    DisplayOrder = 1,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow,
            //});
            //productSonyCamcoder.ProductCategories.Add(new ProductCategory()
            //{
            //    Category = _categoryRepository.Table.Where(c => c.Name == "Camera, photo").Single(),
            //    DisplayOrder = 1,
            //});
            //productSonyCamcoder.ProductPictures.Add(new ProductPicture()
            //{
            //    Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_SonyCamcoder.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productSonyCamcoder.Name), true),
            //    DisplayOrder = 1,
            //});
            //_productRepository.Insert(productSonyCamcoder);





            //var productBestSkilletRecipes = new Product()
            //{
            //    Name = "The Best Skillet Recipes",
            //    ShortDescription = "What's the Best Way to Make Lasagna With Rich, Meaty Flavor, Chunks of Tomato, and Gooey Cheese, Without Ever Turning on the Oven or Boiling a Pot of (Hardcover)",
            //    FullDescription = "<p>In this latest addition of the Best Recipe Classic series, <i>Cooks Illustrated</i> editor Christopher Kimball and his team of kitchen scientists celebrate the untold versatility of that ordinary workhorse, the 12-inch skillet. An indispensable tool for eggs, pan-seared meats and saut&eacute;ed vegetables, the skillet can also be used for stovetop-to-oven dishes such as All-American Mini Meatloaves; layered dishes such as tamale pie and Tuscan bean casserole; and even desserts such as hot fudge pudding cake. In the trademark style of other America's Test Kitchen publications, the cookbook contains plenty of variations on basic themes (you can make chicken and rice with peas and scallions, broccoli and cheddar, or coconut milk and pistachios); ingredient and equipment roundups; and helpful illustrations for preparing mango and stringing snowpeas. Yet the true strength of the series lies in the sheer thoughtfulness and detail of the recipes. Whether or not you properly appreciate your skillet, this book will at least teach you to wield it gracefully. <i>(Mar.)</i>   <br />Copyright &copy; Reed Business Information, a division of Reed Elsevier Inc. All rights reserved.</p>",
            //    ProductTemplateId = productTemplateSingleVariant.Id,
            //    //SeName = "the-best-skillet-recipes",
            //    AllowCustomerReviews = true,
            //    Published = true,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow
            //};
            //allProducts.Add(productBestSkilletRecipes);
            //productBestSkilletRecipes.ProductVariants.Add(new ProductVariant()
            //{
            //    Price = 24M,
            //    OldPrice = 35M,
            //    IsShipEnabled = true,
            //    Weight = 2,
            //    Length = 2,
            //    Width = 2,
            //    Height = 2,
            //    TaxCategoryId = _taxCategoryRepository.Table.Where(tc => tc.Name == "Books").Single().Id,
            //    ManageInventoryMethod = ManageInventoryMethod.ManageStock,
            //    StockQuantity = 10000,
            //    NotifyAdminForQuantityBelow = 1,
            //    AllowBackInStockSubscriptions = false,
            //    DisplayStockAvailability = true,
            //    LowStockActivity = LowStockActivity.DisableBuyButton,
            //    BackorderMode = BackorderMode.NoBackorders,
            //    OrderMinimumQuantity = 1,
            //    OrderMaximumQuantity = 10000,
            //    Published = true,
            //    DisplayOrder = 1,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow,
            //});
            //productBestSkilletRecipes.ProductCategories.Add(new ProductCategory()
            //{
            //    Category = _categoryRepository.Table.Where(c => c.Name == "Books").Single(),
            //    DisplayOrder = 1,
            //});
            //productBestSkilletRecipes.ProductPictures.Add(new ProductPicture()
            //{
            //    Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_BestSkilletRecipes.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productBestSkilletRecipes.Name), true),
            //    DisplayOrder = 1,
            //});
            //_productRepository.Insert(productBestSkilletRecipes);





            //var productSatellite = new Product()
            //{
            //    Name = "Toshiba Satellite A305-S6908 15.4-Inch Laptop",
            //    ShortDescription = "Stylish, highly versatile laptop with 15.4-inch LCD, webcam integrated into bezel, and high-gloss finish",
            //    FullDescription = "<p>It's powered by the 2.0 GHz Intel Core 2 Duo T6400 processor, which boosts speed, reduces power requirements, and saves on battery life. It also offers a fast 800 MHz front-side bus speed and 2 MB L2 cache. It also includes Intel's 5100AGN wireless LAN, which will connect to draft 802.11n routers and offers compatibility with 802.11a/b/g networks as well. Other features include an enormous 250 GB hard drive,&nbsp;1 GB of installed RAM (max capacity), dual-layer DVD&plusmn;RW burner (with Labelflash disc printing), ExpressCard 54/34 slot, a combo USB/eSATA port, SPDIF digital audio output for surround sound, and a 5-in-1 memory card adapter.</p><p>This PC comes preinstalled with the 64-bit version of Microsoft Windows Vista Home Premium (SP1), which includes all of the Windows Media Center capabilities for turning your PC into an all-in-one home entertainment center. In addition to easily playing your DVD movies and managing your digital audio library, you'll be able to record and watch your favorite TV shows (even HDTV). Vista also integrates new search tools throughout the operating system, includes new parental control features, and offers new tools that can warn you of impending hardware failures</p>",
            //    ProductTemplateId = productTemplateSingleVariant.Id,
            //    //SeName = "toshiba-satellite-a305-s6908-154-inch-laptop",
            //    AllowCustomerReviews = true,
            //    Published = true,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow
            //};
            //allProducts.Add(productSatellite);
            //productSatellite.ProductVariants.Add(new ProductVariant()
            //{
            //    Price = 1360M,
            //    IsShipEnabled = true,
            //    Weight = 7,
            //    Length = 7,
            //    Width = 7,
            //    Height = 7,
            //    TaxCategoryId = _taxCategoryRepository.Table.Where(tc => tc.Name == "Electronics & Software").Single().Id,
            //    ManageInventoryMethod = ManageInventoryMethod.ManageStock,
            //    StockQuantity = 10000,
            //    NotifyAdminForQuantityBelow = 1,
            //    AllowBackInStockSubscriptions = false,
            //    DisplayStockAvailability = true,
            //    LowStockActivity = LowStockActivity.DisableBuyButton,
            //    BackorderMode = BackorderMode.NoBackorders,
            //    OrderMinimumQuantity = 1,
            //    OrderMaximumQuantity = 10000,
            //    Published = true,
            //    DisplayOrder = 1,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow,
            //});
            //productSatellite.ProductCategories.Add(new ProductCategory()
            //{
            //    Category = _categoryRepository.Table.Where(c => c.Name == "Notebooks").Single(),
            //    DisplayOrder = 1,
            //});
            //productSatellite.ProductPictures.Add(new ProductPicture()
            //{
            //    Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_Notebooks.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productSatellite.Name), true),
            //    DisplayOrder = 1,
            //});
            //productSatellite.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            //{
            //    AllowFiltering = false,
            //    ShowOnProductPage = true,
            //    DisplayOrder = 1,
            //    SpecificationAttributeOption = _specificationAttributeRepository.Table.Where(sa => sa.Name == "Screensize").Single().SpecificationAttributeOptions.Where(sao => sao.Name == "15.4''").Single()
            //});
            //productSatellite.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            //{
            //    AllowFiltering = true,
            //    ShowOnProductPage = true,
            //    DisplayOrder = 2,
            //    SpecificationAttributeOption = _specificationAttributeRepository.Table.Where(sa => sa.Name == "CPU Type").Single().SpecificationAttributeOptions.Where(sao => sao.Name == "Intel").Single()
            //});
            //productSatellite.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            //{
            //    AllowFiltering = true,
            //    ShowOnProductPage = true,
            //    DisplayOrder = 3,
            //    SpecificationAttributeOption = _specificationAttributeRepository.Table.Where(sa => sa.Name == "Memory").Single().SpecificationAttributeOptions.Where(sao => sao.Name == "1 GB").Single()
            //});
            //productSatellite.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            //{
            //    AllowFiltering = false,
            //    ShowOnProductPage = true,
            //    DisplayOrder = 4,
            //    SpecificationAttributeOption = _specificationAttributeRepository.Table.Where(sa => sa.Name == "Hardrive").Single().SpecificationAttributeOptions.Where(sao => sao.Name == "250 GB").Single()
            //});
            //_productRepository.Insert(productSatellite);





            //var productDenimShort = new Product()
            //{
            //    Name = "V-Blue Juniors' Cuffed Denim Short with Rhinestones",
            //    ShortDescription = "Superior construction and reinforced seams",
            //    FullDescription = "",
            //    ProductTemplateId = productTemplateSingleVariant.Id,
            //    //SeName = "v-blue-juniors-cuffed-denim-short-with-rhinestones",
            //    AllowCustomerReviews = true,
            //    Published = true,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow
            //};
            //allProducts.Add(productDenimShort);
            //productDenimShort.ProductVariants.Add(new ProductVariant()
            //{
            //    Price = 10M,
            //    IsShipEnabled = true,
            //    Weight = 2,
            //    Length = 2,
            //    Width = 2,
            //    Height = 2,
            //    TaxCategoryId = _taxCategoryRepository.Table.Where(tc => tc.Name == "Apparel & Shoes").Single().Id,
            //    ManageInventoryMethod = ManageInventoryMethod.ManageStock,
            //    StockQuantity = 10000,
            //    NotifyAdminForQuantityBelow = 1,
            //    AllowBackInStockSubscriptions = false,
            //    DisplayStockAvailability = true,
            //    LowStockActivity = LowStockActivity.DisableBuyButton,
            //    BackorderMode = BackorderMode.NoBackorders,
            //    OrderMinimumQuantity = 1,
            //    OrderMaximumQuantity = 10000,
            //    Published = true,
            //    DisplayOrder = 1,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow,
            //});
            //productDenimShort.ProductCategories.Add(new ProductCategory()
            //{
            //    Category = _categoryRepository.Table.Where(c => c.Name == "Jeans").Single(),
            //    DisplayOrder = 1,
            //});
            //productDenimShort.ProductPictures.Add(new ProductPicture()
            //{
            //    Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_DenimShort.jpg"), "image/pjpeg", pictureService.GetPictureSeName(productDenimShort.Name), true),
            //    DisplayOrder = 1,
            //});
            //_productRepository.Insert(productDenimShort);





            //var productEngagementRing = new Product()
            //{
            //    Name = "Vintage Style Three Stone Diamond Engagement Ring",
            //    ShortDescription = "1.24 Carat (ctw) in 14K White Gold (Certified)",
            //    FullDescription = "<p>Dazzle her with this gleaming 14 karat white gold vintage proposal. A ravishing collection of 11 decadent diamonds come together to invigorate a superbly ornate gold shank. Total diamond weight on this antique style engagement ring equals 1 1/4 carat (ctw). Item includes diamond certificate.</p>",
            //    ProductTemplateId = productTemplateSingleVariant.Id,
            //    //SeName = "vintage-style-three-stone-diamond-engagement-ring",
            //    AllowCustomerReviews = true,
            //    Published = true,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow
            //};
            //allProducts.Add(productEngagementRing);
            //productEngagementRing.ProductVariants.Add(new ProductVariant()
            //{
            //    Price = 2100M,
            //    IsShipEnabled = true,
            //    Weight = 2,
            //    Length = 2,
            //    Width = 2,
            //    Height = 2,
            //    TaxCategoryId = _taxCategoryRepository.Table.Where(tc => tc.Name == "Jewelry").Single().Id,
            //    ManageInventoryMethod = ManageInventoryMethod.ManageStock,
            //    StockQuantity = 10000,
            //    NotifyAdminForQuantityBelow = 1,
            //    AllowBackInStockSubscriptions = false,
            //    DisplayStockAvailability = true,
            //    LowStockActivity = LowStockActivity.DisableBuyButton,
            //    BackorderMode = BackorderMode.NoBackorders,
            //    OrderMinimumQuantity = 1,
            //    OrderMaximumQuantity = 10000,
            //    Published = true,
            //    DisplayOrder = 1,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow,
            //});
            //productEngagementRing.ProductCategories.Add(new ProductCategory()
            //{
            //    Category = _categoryRepository.Table.Where(c => c.Name == "Jewelry").Single(),
            //    DisplayOrder = 1,
            //});
            //productEngagementRing.ProductPictures.Add(new ProductPicture()
            //{
            //    Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_EngagementRing_1.jpg"), "image/pjpeg", pictureService.GetPictureSeName(productEngagementRing.Name), true),
            //    DisplayOrder = 1,
            //});
            //productEngagementRing.ProductPictures.Add(new ProductPicture()
            //{
            //    Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_EngagementRing_2.jpg"), "image/pjpeg", pictureService.GetPictureSeName(productEngagementRing.Name), true),
            //    DisplayOrder = 2,
            //});
            //_productRepository.Insert(productEngagementRing);





            //var productWoW = new Product()
            //{
            //    Name = "World of Warcraft: Wrath of the Lich King Expansion Pack",
            //    ShortDescription = "This expansion pack REQUIRES the original World of Warcraft game in order to run",
            //    FullDescription = "<p>Fans of World of Warcraft, prepare for Blizzard Entertainment's next installment -- World of Warcraft: Wrath of King Lich. In this latest expansion, something is afoot in the cold, harsh northlands. The Lich King Arthas has set in motion events that could lead to the extinction of all life on Azeroth. The necromantic power of the plague and legions of undead armies threaten to sweep across the land. Only the mightiest heroes can oppose the Lich King and end his reign of terror.</p><p>This expansion adds a host of content to the already massive existing game world. Players will achieve soaring levels of power, explore Northrend (the vast icy continent of the Lich King), and battle high-level heroes to determine the ultimate fate of Azeroth. As you face the dangers of the frigid, harsh north, prepare to master the dark necromantic powers of the Death Night -- World of Warcraft's first Hero class. No longer servants of the Lich King, the Death Knights begin their new calling as experienced, formidable adversaries. Each is heavily armed, armored, and in possession of a deadly arsenal of forbidden magic.</p><p>If you have a World of Warcraft account with a character of at least level 55, you will be able to create a new level-55 Death Knight of any race (if on a PvP realm, the Death Knight must be the same faction as your existing character). And upon entering the new world, your Death Knight will begin to quest to level 80, gaining potent new abilities and talents along the way. This expansion allows for only one Death Knight per realm, per account.</p>",
            //    ProductTemplateId = productTemplateSingleVariant.Id,
            //    //SeName = "world-of-warcraft-wrath-of-the-lich-king-expansion-pack",
            //    AllowCustomerReviews = true,
            //    Published = true,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow
            //};
            //allProducts.Add(productWoW);
            //productWoW.ProductVariants.Add(new ProductVariant()
            //{
            //    Price = 29.5M,
            //    IsShipEnabled = true,
            //    Weight = 7,
            //    Length = 7,
            //    Width = 7,
            //    Height = 7,
            //    TaxCategoryId = _taxCategoryRepository.Table.Where(tc => tc.Name == "Electronics & Software").Single().Id,
            //    ManageInventoryMethod = ManageInventoryMethod.ManageStock,
            //    StockQuantity = 10000,
            //    NotifyAdminForQuantityBelow = 1,
            //    AllowBackInStockSubscriptions = false,
            //    DisplayStockAvailability = true,
            //    LowStockActivity = LowStockActivity.DisableBuyButton,
            //    BackorderMode = BackorderMode.NoBackorders,
            //    OrderMinimumQuantity = 1,
            //    OrderMaximumQuantity = 10000,
            //    Published = true,
            //    DisplayOrder = 1,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow,
            //});
            //productWoW.ProductCategories.Add(new ProductCategory()
            //{
            //    Category = _categoryRepository.Table.Where(c => c.Name == "Games").Single(),
            //    DisplayOrder = 1,
            //});
            //productWoW.ProductPictures.Add(new ProductPicture()
            //{
            //    Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_wow.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productWoW.Name), true),
            //    DisplayOrder = 1,
            //});
            //_productRepository.Insert(productWoW);





            //var productSoccer = new Product()
            //{
            //    Name = "World Wide Soccer Manager 2009",
            //    ShortDescription = "Worldwide Soccer Manager 2009 from Sega for the PC or Mac is an in-depth soccer management game",
            //    FullDescription = "<p>Worldwide Soccer Manager 2009 from Sega for the PC or Mac is an in-depth soccer management game. At the helm, you'll enter the new season with a wide array of all-new features. The most impressive update is the first-time-ever, real-time 3D match engine with motion captured animations. With over 5,000 playable teams and every management decision in the palm of your hand, you'll love watching your matches and decisions unfold from multiple camera angles as you compete in leagues around the world and major international tournaments.</p><p>Watch your match in real-time, or use the Match Time Bar to fast-forward through sluggish minutes or rewind key moments in the game. With this customization at your fingertips you can also choose the information you'd like to see during the match, such as latest scores or player performance stats for the match.</p>",
            //    ProductTemplateId = productTemplateSingleVariant.Id,
            //    //SeName = "world-wide-soccer-manager-2009",
            //    AllowCustomerReviews = true,
            //    Published = true,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow
            //};
            //allProducts.Add(productSoccer);
            //productSoccer.ProductVariants.Add(new ProductVariant()
            //{
            //    Price = 25.99M,
            //    IsShipEnabled = true,
            //    Weight = 7,
            //    Length = 7,
            //    Width = 7,
            //    Height = 7,
            //    TaxCategoryId = _taxCategoryRepository.Table.Where(tc => tc.Name == "Electronics & Software").Single().Id,
            //    ManageInventoryMethod = ManageInventoryMethod.ManageStock,
            //    StockQuantity = 10000,
            //    NotifyAdminForQuantityBelow = 1,
            //    AllowBackInStockSubscriptions = false,
            //    DisplayStockAvailability = true,
            //    LowStockActivity = LowStockActivity.DisableBuyButton,
            //    BackorderMode = BackorderMode.NoBackorders,
            //    OrderMinimumQuantity = 1,
            //    OrderMaximumQuantity = 10000,
            //    Published = true,
            //    DisplayOrder = 1,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow,
            //});
            //productSoccer.ProductCategories.Add(new ProductCategory()
            //{
            //    Category = _categoryRepository.Table.Where(c => c.Name == "Games").Single(),
            //    DisplayOrder = 1,
            //});
            //productSoccer.ProductPictures.Add(new ProductPicture()
            //{
            //    Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_Soccer.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productSoccer.Name), true),
            //    DisplayOrder = 1,
            //});
            //_productRepository.Insert(productSoccer);





            //var productPokerFace = new Product()
            //{
            //    Name = "Poker Face",
            //    ShortDescription = "Poker Face by Lady GaGa",
            //    FullDescription = "<p>Original Release Date: October 28, 2008</p><p>Release Date: October 28, 2008</p><p>Label: Streamline/Interscoope/KonLive/Cherrytree</p><p>Copyright: (C) 2008 Interscope Records</p>",
            //    ProductTemplateId = productTemplateSingleVariant.Id,
            //    //SeName = "poker-face",
            //    AllowCustomerReviews = true,
            //    Published = true,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow
            //};
            //allProducts.Add(productPokerFace);
            //var downloadPokerFace1 = new Download()
            //{
            //    DownloadGuid = Guid.NewGuid(),
            //    ContentType = "application/x-zip-co",
            //    DownloadBinary = File.ReadAllBytes(sampleDownloadsPath + "product_PokerFace_1.zip"),
            //    Extension = ".zip",
            //    Filename = "Poker_Face_1",
            //    IsNew = true,
            //};
            //downloadService.InsertDownload(downloadPokerFace1);
            //var downloadPokerFace2 = new Download()
            //{
            //    DownloadGuid = Guid.NewGuid(),
            //    ContentType = "text/plain",
            //    DownloadBinary = File.ReadAllBytes(sampleDownloadsPath + "product_PokerFace_2.txt"),
            //    Extension = ".txt",
            //    Filename = "Poker_Face_1",
            //    IsNew = true,
            //};
            //downloadService.InsertDownload(downloadPokerFace2);
            //productPokerFace.ProductVariants.Add(new ProductVariant()
            //{
            //    Price = 2.8M,
            //    TaxCategoryId = _taxCategoryRepository.Table.Where(tc => tc.Name == "Downloadable Products").Single().Id,
            //    ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
            //    StockQuantity = 10000,
            //    NotifyAdminForQuantityBelow = 1,
            //    AllowBackInStockSubscriptions = false,
            //    DisplayStockAvailability = true,
            //    LowStockActivity = LowStockActivity.DisableBuyButton,
            //    BackorderMode = BackorderMode.NoBackorders,
            //    OrderMinimumQuantity = 1,
            //    OrderMaximumQuantity = 10000,
            //    Published = true,
            //    DisplayOrder = 1,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow,
            //    IsDownload = true,
            //    DownloadId = downloadPokerFace1.Id,
            //    DownloadActivationType = DownloadActivationType.WhenOrderIsPaid,
            //    UnlimitedDownloads = true,
            //    HasUserAgreement = false,
            //    HasSampleDownload = true,
            //    SampleDownloadId = downloadPokerFace2.Id,

            //});
            //productPokerFace.ProductCategories.Add(new ProductCategory()
            //{
            //    Category = _categoryRepository.Table.Where(c => c.Name == "Digital downloads").Single(),
            //    DisplayOrder = 1,
            //});
            //productPokerFace.ProductPictures.Add(new ProductPicture()
            //{
            //    Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_PokerFace.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productPokerFace.Name), true),
            //    DisplayOrder = 1,
            //});
            //_productRepository.Insert(productPokerFace);





            //var productSingleLadies = new Product()
            //{
            //    Name = "Single Ladies (Put A Ring On It)",
            //    ShortDescription = "Single Ladies (Put A Ring On It) by Beyonce",
            //    FullDescription = "<p>Original Release Date: November 18, 2008</p><p>Label: Music World Music/Columbia</p><p>Copyright: (P) 2008 SONY BMG MUSIC ENTERTAINMENT</p><p>Song Length: 3:13 minutes</p>",
            //    ProductTemplateId = productTemplateSingleVariant.Id,
            //    //SeName = "single-ladies-put-a-ring-on-it",
            //    AllowCustomerReviews = true,
            //    Published = true,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow
            //};
            //allProducts.Add(productSingleLadies);
            //var downloadSingleLadies1 = new Download()
            //{
            //    DownloadGuid = Guid.NewGuid(),
            //    ContentType = "application/x-zip-co",
            //    DownloadBinary = File.ReadAllBytes(sampleDownloadsPath + "product_SingleLadies_1.zip"),
            //    Extension = ".zip",
            //    Filename = "Single_Ladies_1",
            //    IsNew = true,
            //};
            //downloadService.InsertDownload(downloadSingleLadies1);
            //var downloadSingleLadies2 = new Download()
            //{
            //    DownloadGuid = Guid.NewGuid(),
            //    ContentType = "text/plain",
            //    DownloadBinary = File.ReadAllBytes(sampleDownloadsPath + "product_SingleLadies_2.txt"),
            //    Extension = ".txt",
            //    Filename = "Single_Ladies_1",
            //    IsNew = true,
            //};
            //downloadService.InsertDownload(downloadSingleLadies2);
            //productSingleLadies.ProductVariants.Add(new ProductVariant()
            //{
            //    Price = 3M,
            //    TaxCategoryId = _taxCategoryRepository.Table.Where(tc => tc.Name == "Downloadable Products").Single().Id,
            //    ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
            //    StockQuantity = 10000,
            //    NotifyAdminForQuantityBelow = 1,
            //    AllowBackInStockSubscriptions = false,
            //    DisplayStockAvailability = true,
            //    LowStockActivity = LowStockActivity.DisableBuyButton,
            //    BackorderMode = BackorderMode.NoBackorders,
            //    OrderMinimumQuantity = 1,
            //    OrderMaximumQuantity = 10000,
            //    Published = true,
            //    DisplayOrder = 1,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow,
            //    IsDownload = true,
            //    DownloadId = downloadSingleLadies1.Id,
            //    DownloadActivationType = DownloadActivationType.WhenOrderIsPaid,
            //    UnlimitedDownloads = true,
            //    HasUserAgreement = false,
            //    HasSampleDownload = true,
            //    SampleDownloadId = downloadSingleLadies2.Id,

            //});
            //productSingleLadies.ProductCategories.Add(new ProductCategory()
            //{
            //    Category = _categoryRepository.Table.Where(c => c.Name == "Digital downloads").Single(),
            //    DisplayOrder = 1,
            //});
            //productSingleLadies.ProductPictures.Add(new ProductPicture()
            //{
            //    Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_SingleLadies.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productSingleLadies.Name), true),
            //    DisplayOrder = 1,
            //});
            //_productRepository.Insert(productSingleLadies);





            //var productBattleOfLa = new Product()
            //{
            //    Name = "The Battle Of Los Angeles",
            //    ShortDescription = "The Battle Of Los Angeles by Rage Against The Machine",
            //    FullDescription = "<p># Original Release Date: November 2, 1999<br /># Label: Epic<br /># Copyright: 1999 Sony Music Entertainment Inc. (c) 1999 Sony Music Entertainment Inc.</p>",
            //    ProductTemplateId = productTemplateSingleVariant.Id,
            //    //SeName = "the-battle-of-los-angeles",
            //    AllowCustomerReviews = true,
            //    Published = true,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow
            //};
            //allProducts.Add(productBattleOfLa);
            //var downloadBattleOfLa = new Download()
            //{
            //    DownloadGuid = Guid.NewGuid(),
            //    ContentType = "application/x-zip-co",
            //    DownloadBinary = File.ReadAllBytes(sampleDownloadsPath + "product_BattleOfLa_1.zip"),
            //    Extension = ".zip",
            //    Filename = "The_Battle_Of_Los_Angeles",
            //    IsNew = true,
            //};
            //downloadService.InsertDownload(downloadBattleOfLa);
            //productBattleOfLa.ProductVariants.Add(new ProductVariant()
            //{
            //    Price = 3M,
            //    TaxCategoryId = _taxCategoryRepository.Table.Where(tc => tc.Name == "Downloadable Products").Single().Id,
            //    ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
            //    StockQuantity = 10000,
            //    NotifyAdminForQuantityBelow = 1,
            //    AllowBackInStockSubscriptions = false,
            //    DisplayStockAvailability = true,
            //    LowStockActivity = LowStockActivity.DisableBuyButton,
            //    BackorderMode = BackorderMode.NoBackorders,
            //    OrderMinimumQuantity = 1,
            //    OrderMaximumQuantity = 10000,
            //    Published = true,
            //    DisplayOrder = 1,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow,
            //    IsDownload = true,
            //    DownloadId = downloadBattleOfLa.Id,
            //    DownloadActivationType = DownloadActivationType.WhenOrderIsPaid,
            //    UnlimitedDownloads = true,
            //    HasUserAgreement = false,

            //});
            //productBattleOfLa.ProductCategories.Add(new ProductCategory()
            //{
            //    Category = _categoryRepository.Table.Where(c => c.Name == "Digital downloads").Single(),
            //    DisplayOrder = 1,
            //});
            //productBattleOfLa.ProductPictures.Add(new ProductPicture()
            //{
            //    Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_BattleOfLA.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productBattleOfLa.Name), true),
            //    DisplayOrder = 1,
            //});
            //_productRepository.Insert(productBattleOfLa);
            #endregion oldcode products

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

            //try
            //{
            //    _installData.PollAnswers().Each(x => _pollAnswerRepository.Insert(x));
            //    IncreaseProgress();
            //}
            //catch (Exception ex)
            //{
            //    throw new InstallationException("InstallPollAnswers", ex);
            //}
            
            
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
                var productTag = _productTagRepository.Table.Where(pt => pt.Name == tag).FirstOrDefault();
                if (productTag == null)
                {
                    productTag = new ProductTag()
                    {
                        Name = tag,
                        ProductCount = 1,
                    };
                    productTag.Products.Add(product);
                    _productTagRepository.Insert(productTag);
                }
                else
                {
                    productTag.ProductCount++;
                    productTag.Products.Add(product);
                    _productTagRepository.Update(productTag);
                }
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

        public virtual void InstallData(InstallDataContext context /* codehint: sm-edit */)
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

            //_dbContext.AutoDetectChangesEnabled = true;
            //_dbContext.ValidateOnSaveEnabled = true;

        }

        #endregion methods
    }
        
} 
