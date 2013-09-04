// codehint: sm-add (file)

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using SmartStore.Core;
using SmartStore.Core.Configuration;
using SmartStore.Core.Data;
using SmartStore.Core.Domain;
using SmartStore.Core.Domain.Blogs;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Cms;
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
using SmartStore.Core.Domain.Seo;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Domain.Tasks;
using SmartStore.Core.Domain.Tax;
using SmartStore.Core.Domain.Themes;
using SmartStore.Core.Domain.Topics;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.IO;
using SmartStore.Services.Helpers;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;

//using SmartStore.Services.Installation;

namespace SmartStore.Services.Installation
{
    public abstract class InvariantInstallationData
    {
        private readonly IPictureService _pictureService;
        private readonly string _sampleImagesPath;
        private readonly ILanguageService _languageService;

        private readonly IDownloadService _downloadService;
        private readonly string _sampleDownloadsPath;

        private readonly IRepository<Currency> _currencyRepository;
        private readonly IRepository<MeasureDimension> _measureDimensionRepository;
        private readonly IRepository<MeasureWeight> _measureWeightRepository;
        private readonly IRepository<EmailAccount> _emailAccountRepository;
        private readonly IRepository<Category> _categoryRepository;
        private readonly IRepository<SpecificationAttribute> _specificationAttributesRepository;
        private readonly IRepository<Manufacturer> _manufacturerRepository;
        private readonly IRepository<TaxCategory> _taxCategoryRepository;
        private readonly IRepository<ForumGroup> _forumGroupRepository;
        private readonly IRepository<Language> _languageRepository;
        private readonly IRepository<PollAnswer> _pollAnswerRepository;
        private readonly IRepository<DeliveryTime> _deliveryTimeRepository;
        private readonly IRepository<ProductTag> _productTagRepository;
        private readonly IRepository<Product> _productRepository;

        //private readonly IRepository<DeliveryTime> _deliveryTimeRepository;
        //private readonly IWebHelper _webHelper;

        //public InvariantInstallationData(IRepository<Currency> currencyRepository)
        public InvariantInstallationData()
        {
            //pictures
            this._pictureService = EngineContext.Current.Resolve<IPictureService>();
            this._sampleImagesPath = EngineContext.Current.Resolve<IWebHelper>().MapPath("~/content/samples/");

            //downloads
            this._downloadService = EngineContext.Current.Resolve<IDownloadService>();
            this._sampleDownloadsPath = EngineContext.Current.Resolve<IWebHelper>().MapPath("~/content/samples/");

            //languageservice
            this._languageService = EngineContext.Current.Resolve<ILanguageService>();

            //currency of settings
            this._currencyRepository = EngineContext.Current.Resolve<IRepository<Currency>>();

            //measureDimension of settings
            this._measureDimensionRepository = EngineContext.Current.Resolve<IRepository<MeasureDimension>>();
            this._measureWeightRepository = EngineContext.Current.Resolve<IRepository<MeasureWeight>>();
            this._emailAccountRepository = EngineContext.Current.Resolve<IRepository<EmailAccount>>();
            this._categoryRepository = EngineContext.Current.Resolve<IRepository<Category>>();
            this._specificationAttributesRepository = EngineContext.Current.Resolve<IRepository<SpecificationAttribute>>();
            this._manufacturerRepository = EngineContext.Current.Resolve<IRepository<Manufacturer>>();
            this._taxCategoryRepository = EngineContext.Current.Resolve<IRepository<TaxCategory>>();
            this._forumGroupRepository = EngineContext.Current.Resolve<IRepository<ForumGroup>>();
            this._languageRepository = EngineContext.Current.Resolve<IRepository<Language>>();
            this._pollAnswerRepository = EngineContext.Current.Resolve<IRepository<PollAnswer>>();
            this._deliveryTimeRepository = EngineContext.Current.Resolve<IRepository<DeliveryTime>>();
            this._productTagRepository = EngineContext.Current.Resolve<IRepository<ProductTag>>();
            this._productRepository = EngineContext.Current.Resolve<IRepository<Product>>();
        }

        #region Mandatory data creators

        public IList<MeasureDimension> MeasureDimensions()
        {
            var entities = new List<MeasureDimension>()
            {
                new MeasureDimension()
                {
                    Name = "millimetre",
                    SystemKeyword = "mm",
                    Ratio = 25.4M,
                    DisplayOrder = 1,
                },
                new MeasureDimension()
                {
                    Name = "centimetre",
                    SystemKeyword = "cm",
                    Ratio = 0.254M,
                    DisplayOrder = 2,
                },
                new MeasureDimension()
                {
                    Name = "meter",
                    SystemKeyword = "m",
                    Ratio = 0.0254M,
                    DisplayOrder = 3,
                },
                new MeasureDimension()
                {
                    Name = "in",
                    SystemKeyword = "inch",
                    Ratio = 1M,
                    DisplayOrder = 4,
                },
                new MeasureDimension()
                {
                    Name = "feet",
                    SystemKeyword = "ft",
                    Ratio = 0.08333333M,
                    DisplayOrder = 5,
                }
            };

            this.Alter(entities);
            return entities;
        }

        public IList<MeasureWeight> MeasureWeights()
        {
            var entities = new List<MeasureWeight>()
            {
                new MeasureWeight()
                {
                    Name = "ounce", // Ounce, Unze
                    SystemKeyword = "oz",
                    Ratio = 16M,
                    DisplayOrder = 5,
                },
                new MeasureWeight()
                {
                    Name = "lb", // Pound
                    SystemKeyword = "lb",
                    Ratio = 1M,
                    DisplayOrder = 6,
                },

                new MeasureWeight()
                {
                    Name = "kg",
                    SystemKeyword = "kg",
                    Ratio = 0.45359237M,
                    DisplayOrder = 1,
                },
                new MeasureWeight()
                {
                    Name = "gram",
                    SystemKeyword = "g",
                    Ratio = 453.59237M,
                    DisplayOrder = 2,
                },
                new MeasureWeight()
                {
                    Name = "liter",
                    SystemKeyword = "l",
                    Ratio = 0.45359237M,
                    DisplayOrder = 3,
                },
                new MeasureWeight()
                {
                    Name = "milliliter",
                    SystemKeyword = "ml",
                    Ratio = 0.45359237M,
                    DisplayOrder = 4,
                }
            };

            this.Alter(entities);
            return entities;
        }

        protected virtual string TaxNameBooks
        {
            get { return "Books"; }
        }

        protected virtual string TaxNameDigitalGoods
        {
            get { return "Downloadable Products"; }
        }

        protected virtual string TaxNameJewelry
        {
            get { return "Jewelry"; }
        }

        protected virtual string TaxNameApparel
        {
            get { return "Apparel & Shoes"; }
        }

        protected virtual string TaxNameFood
        {
            get { return "Food"; }
        }

        protected virtual string TaxNameElectronics
        {
            get { return "Electronics & Software"; }
        }

        protected virtual string TaxNameTaxFree
        {
            get { return "Tax free"; }
        }

        public virtual decimal[] FixedTaxRates
        {
            get { return new decimal[] { 0, 0, 0, 0, 0 }; }
        }

        public IList<TaxCategory> TaxCategories()
        {
            var entities = new List<TaxCategory>
            {
                new TaxCategory
                {
                    Name = this.TaxNameBooks,
                    DisplayOrder = 1,
                },
                new TaxCategory
                {
                    Name = this.TaxNameElectronics,
                    DisplayOrder = 5,
                },
                new TaxCategory
                {
                    Name = this.TaxNameDigitalGoods,
                    DisplayOrder = 10,
                },
                new TaxCategory
                {
                    Name = this.TaxNameJewelry,
                    DisplayOrder = 15,
                },
                new TaxCategory
                {
                    Name = this.TaxNameApparel,
                    DisplayOrder = 20,
                },
            };

            this.Alter(entities);
            return entities;
        }

        public IList<Currency> Currencies()
        {
            var entities = new List<Currency>()
            {
                // codehint: sm-edit
                CreateCurrency("en-US", published: true, rate: 1M, order: 0),
                CreateCurrency("en-GB", published: true, rate: 0.61M, order: 5),
                CreateCurrency("en-AU", published: true, rate: 0.94M, order: 10),
                CreateCurrency("en-CA", published: true, rate: 0.98M, order: 15),
                CreateCurrency("de-DE", rate: 0.79M, order: 20/*, formatting: string.Format("0.00 {0}", "\u20ac")*/),
                CreateCurrency("de-CH", rate: 0.93M, order: 25),
                CreateCurrency("zh-CN", rate: 6.48M, order: 30),
                CreateCurrency("zh-HK", rate: 7.75M, order: 35),
                CreateCurrency("ja-JP", rate: 80.07M, order: 40),
                CreateCurrency("ru-RU", rate: 27.7M, order: 45),
                CreateCurrency("tr-TR", rate: 1.78M, order: 50),
                CreateCurrency("sv-SE", rate: 6.19M, order: 55)
            };

            this.Alter(entities);
            return entities;
        }

        protected Currency CreateCurrency(string locale, decimal rate = 1M, string formatting = "", bool published = false, int order = 1)
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
            }
            catch
            {
                return null;
            }

            return currency;
        }

        public IList<Country> Countries()
        {
            var cUsa = new Country
            {
                Name = "United States",
                AllowsBilling = true,
                AllowsShipping = true,
                TwoLetterIsoCode = "US",
                ThreeLetterIsoCode = "USA",
                NumericIsoCode = 840,
                SubjectToVat = false,
                DisplayOrder = 1,
                Published = true,
            };
            #region US Regions
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "AA (Armed Forces Americas)",
                Abbreviation = "AA",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "AE (Armed Forces Europe)",
                Abbreviation = "AE",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Alabama",
                Abbreviation = "AL",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Alaska",
                Abbreviation = "AK",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "American Samoa",
                Abbreviation = "AS",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "AP (Armed Forces Pacific)",
                Abbreviation = "AP",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Arizona",
                Abbreviation = "AZ",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Arkansas",
                Abbreviation = "AR",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "California",
                Abbreviation = "CA",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Colorado",
                Abbreviation = "CO",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Connecticut",
                Abbreviation = "CT",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Delaware",
                Abbreviation = "DE",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "District of Columbia",
                Abbreviation = "DC",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Federated States of Micronesia",
                Abbreviation = "FM",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Florida",
                Abbreviation = "FL",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Georgia",
                Abbreviation = "GA",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Guam",
                Abbreviation = "GU",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Hawaii",
                Abbreviation = "HI",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Idaho",
                Abbreviation = "ID",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Illinois",
                Abbreviation = "IL",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Indiana",
                Abbreviation = "IN",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Iowa",
                Abbreviation = "IA",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Kansas",
                Abbreviation = "KS",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Kentucky",
                Abbreviation = "KY",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Louisiana",
                Abbreviation = "LA",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Maine",
                Abbreviation = "ME",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Marshall Islands",
                Abbreviation = "MH",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Maryland",
                Abbreviation = "MD",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Massachusetts",
                Abbreviation = "MA",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Michigan",
                Abbreviation = "MI",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Minnesota",
                Abbreviation = "MN",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Mississippi",
                Abbreviation = "MS",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Missouri",
                Abbreviation = "MO",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Montana",
                Abbreviation = "MT",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Nebraska",
                Abbreviation = "NE",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Nevada",
                Abbreviation = "NV",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "New Hampshire",
                Abbreviation = "NH",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "New Jersey",
                Abbreviation = "NJ",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "New Mexico",
                Abbreviation = "NM",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "New York",
                Abbreviation = "NY",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "North Carolina",
                Abbreviation = "NC",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "North Dakota",
                Abbreviation = "ND",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Northern Mariana Islands",
                Abbreviation = "MP",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Ohio",
                Abbreviation = "OH",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Oklahoma",
                Abbreviation = "OK",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Oregon",
                Abbreviation = "OR",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Palau",
                Abbreviation = "PW",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Pennsylvania",
                Abbreviation = "PA",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Puerto Rico",
                Abbreviation = "PR",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Rhode Island",
                Abbreviation = "RI",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "South Carolina",
                Abbreviation = "SC",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "South Dakota",
                Abbreviation = "SD",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Tennessee",
                Abbreviation = "TN",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Texas",
                Abbreviation = "TX",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Utah",
                Abbreviation = "UT",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Vermont",
                Abbreviation = "VT",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Virgin Islands",
                Abbreviation = "VI",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Virginia",
                Abbreviation = "VA",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Washington",
                Abbreviation = "WA",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "West Virginia",
                Abbreviation = "WV",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Wisconsin",
                Abbreviation = "WI",
                Published = true,
                DisplayOrder = 1,
            });
            cUsa.StateProvinces.Add(new StateProvince()
            {
                Name = "Wyoming",
                Abbreviation = "WY",
                Published = true,
                DisplayOrder = 1,
            });
            var cCanada = new Country
            {
                Name = "Canada",
                AllowsBilling = true,
                AllowsShipping = true,
                TwoLetterIsoCode = "CA",
                ThreeLetterIsoCode = "CAN",
                NumericIsoCode = 124,
                SubjectToVat = false,
                DisplayOrder = 2,
                Published = true,
            };
            cCanada.StateProvinces.Add(new StateProvince()
            {
                Name = "Alberta",
                Abbreviation = "AB",
                Published = true,
                DisplayOrder = 1,
            });
            cCanada.StateProvinces.Add(new StateProvince()
            {
                Name = "British Columbia",
                Abbreviation = "BC",
                Published = true,
                DisplayOrder = 1,
            });
            cCanada.StateProvinces.Add(new StateProvince()
            {
                Name = "Manitoba",
                Abbreviation = "MB",
                Published = true,
                DisplayOrder = 1,
            });
            cCanada.StateProvinces.Add(new StateProvince()
            {
                Name = "New Brunswick",
                Abbreviation = "NB",
                Published = true,
                DisplayOrder = 1,
            });
            cCanada.StateProvinces.Add(new StateProvince()
            {
                Name = "Newfoundland and Labrador",
                Abbreviation = "NL",
                Published = true,
                DisplayOrder = 1,
            });
            cCanada.StateProvinces.Add(new StateProvince()
            {
                Name = "Northwest Territories",
                Abbreviation = "NT",
                Published = true,
                DisplayOrder = 1,
            });
            cCanada.StateProvinces.Add(new StateProvince()
            {
                Name = "Nova Scotia",
                Abbreviation = "NS",
                Published = true,
                DisplayOrder = 1,
            });
            cCanada.StateProvinces.Add(new StateProvince()
            {
                Name = "Nunavut",
                Abbreviation = "NU",
                Published = true,
                DisplayOrder = 1,
            });
            cCanada.StateProvinces.Add(new StateProvince()
            {
                Name = "Ontario",
                Abbreviation = "ON",
                Published = true,
                DisplayOrder = 1,
            });
            cCanada.StateProvinces.Add(new StateProvince()
            {
                Name = "Prince Edward Island",
                Abbreviation = "PE",
                Published = true,
                DisplayOrder = 1,
            });
            cCanada.StateProvinces.Add(new StateProvince()
            {
                Name = "Quebec",
                Abbreviation = "QC",
                Published = true,
                DisplayOrder = 1,
            });
            cCanada.StateProvinces.Add(new StateProvince()
            {
                Name = "Saskatchewan",
                Abbreviation = "SK",
                Published = true,
                DisplayOrder = 1,
            });
            cCanada.StateProvinces.Add(new StateProvince()
            {
                Name = "Yukon Territory",
                Abbreviation = "YU",
                Published = true,
                DisplayOrder = 1,
            });
            #endregion

            var entities = new List<Country>()
            {
                new Country()
                {
                    Name = "Germany",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "DE",
	                ThreeLetterIsoCode = "DEU",
	                NumericIsoCode = 276,
	                SubjectToVat = true,
	                DisplayOrder = -10,
	                Published = true
                },

                new Country
                {
	                Name = "Austria",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "AT",
	                ThreeLetterIsoCode = "AUT",
	                NumericIsoCode = 40,
	                SubjectToVat = true,
	                DisplayOrder = -5,
	                Published = true
                },
                new Country
                {
	                Name = "Switzerland",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "CH",
	                ThreeLetterIsoCode = "CHE",
	                NumericIsoCode = 756,
	                SubjectToVat = false,
	                DisplayOrder = -1,
	                Published = true
                },
                cUsa,
                cCanada,

                //other countries
                new Country
                {
	                Name = "Argentina",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "AR",
	                ThreeLetterIsoCode = "ARG",
	                NumericIsoCode = 32,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Armenia",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "AM",
	                ThreeLetterIsoCode = "ARM",
	                NumericIsoCode = 51,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Aruba",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "AW",
	                ThreeLetterIsoCode = "ABW",
	                NumericIsoCode = 533,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Australia",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "AU",
	                ThreeLetterIsoCode = "AUS",
	                NumericIsoCode = 36,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Azerbaijan",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "AZ",
	                ThreeLetterIsoCode = "AZE",
	                NumericIsoCode = 31,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Bahamas",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "BS",
	                ThreeLetterIsoCode = "BHS",
	                NumericIsoCode = 44,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Bangladesh",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "BD",
	                ThreeLetterIsoCode = "BGD",
	                NumericIsoCode = 50,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Belarus",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "BY",
	                ThreeLetterIsoCode = "BLR",
	                NumericIsoCode = 112,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Belgium",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "BE",
	                ThreeLetterIsoCode = "BEL",
	                NumericIsoCode = 56,
	                SubjectToVat = true,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Belize",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "BZ",
	                ThreeLetterIsoCode = "BLZ",
	                NumericIsoCode = 84,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Bermuda",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "BM",
	                ThreeLetterIsoCode = "BMU",
	                NumericIsoCode = 60,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Bolivia",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "BO",
	                ThreeLetterIsoCode = "BOL",
	                NumericIsoCode = 68,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Bosnia and Herzegowina",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "BA",
	                ThreeLetterIsoCode = "BIH",
	                NumericIsoCode = 70,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Brazil",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "BR",
	                ThreeLetterIsoCode = "BRA",
	                NumericIsoCode = 76,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Bulgaria",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "BG",
	                ThreeLetterIsoCode = "BGR",
	                NumericIsoCode = 100,
	                SubjectToVat = true,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Cayman Islands",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "KY",
	                ThreeLetterIsoCode = "CYM",
	                NumericIsoCode = 136,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Chile",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "CL",
	                ThreeLetterIsoCode = "CHL",
	                NumericIsoCode = 152,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "China",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "CN",
	                ThreeLetterIsoCode = "CHN",
	                NumericIsoCode = 156,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Colombia",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "CO",
	                ThreeLetterIsoCode = "COL",
	                NumericIsoCode = 170,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Costa Rica",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "CR",
	                ThreeLetterIsoCode = "CRI",
	                NumericIsoCode = 188,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Croatia (local Name: Hrvatska)",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "HR",
	                ThreeLetterIsoCode = "HRV",
	                NumericIsoCode = 191,
	                SubjectToVat = true,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Cuba",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "CU",
	                ThreeLetterIsoCode = "CUB",
	                NumericIsoCode = 192,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Cyprus",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "CY",
	                ThreeLetterIsoCode = "CYP",
	                NumericIsoCode = 196,
	                SubjectToVat = true,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Czech Republic",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "CZ",
	                ThreeLetterIsoCode = "CZE",
	                NumericIsoCode = 203,
	                SubjectToVat = true,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Denmark",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "DK",
	                ThreeLetterIsoCode = "DNK",
	                NumericIsoCode = 208,
	                SubjectToVat = true,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Dominican Republic",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "DO",
	                ThreeLetterIsoCode = "DOM",
	                NumericIsoCode = 214,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Ecuador",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "EC",
	                ThreeLetterIsoCode = "ECU",
	                NumericIsoCode = 218,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Egypt",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "EG",
	                ThreeLetterIsoCode = "EGY",
	                NumericIsoCode = 818,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Finland",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "FI",
	                ThreeLetterIsoCode = "FIN",
	                NumericIsoCode = 246,
	                SubjectToVat = true,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "France",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "FR",
	                ThreeLetterIsoCode = "FRA",
	                NumericIsoCode = 250,
	                SubjectToVat = true,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Georgia",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "GE",
	                ThreeLetterIsoCode = "GEO",
	                NumericIsoCode = 268,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Gibraltar",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "GI",
	                ThreeLetterIsoCode = "GIB",
	                NumericIsoCode = 292,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Greece",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "GR",
	                ThreeLetterIsoCode = "GRC",
	                NumericIsoCode = 300,
	                SubjectToVat = true,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Guatemala",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "GT",
	                ThreeLetterIsoCode = "GTM",
	                NumericIsoCode = 320,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Hong Kong",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "HK",
	                ThreeLetterIsoCode = "HKG",
	                NumericIsoCode = 344,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Hungary",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "HU",
	                ThreeLetterIsoCode = "HUN",
	                NumericIsoCode = 348,
	                SubjectToVat = true,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "India",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "IN",
	                ThreeLetterIsoCode = "IND",
	                NumericIsoCode = 356,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Indonesia",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "ID",
	                ThreeLetterIsoCode = "IDN",
	                NumericIsoCode = 360,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Ireland",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "IE",
	                ThreeLetterIsoCode = "IRL",
	                NumericIsoCode = 372,
	                SubjectToVat = true,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Israel",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "IL",
	                ThreeLetterIsoCode = "ISR",
	                NumericIsoCode = 376,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Italy",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "IT",
	                ThreeLetterIsoCode = "ITA",
	                NumericIsoCode = 380,
	                SubjectToVat = true,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Jamaica",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "JM",
	                ThreeLetterIsoCode = "JAM",
	                NumericIsoCode = 388,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Japan",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "JP",
	                ThreeLetterIsoCode = "JPN",
	                NumericIsoCode = 392,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Jordan",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "JO",
	                ThreeLetterIsoCode = "JOR",
	                NumericIsoCode = 400,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Kazakhstan",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "KZ",
	                ThreeLetterIsoCode = "KAZ",
	                NumericIsoCode = 398,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Korea, Democratic People's Republic of",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "KP",
	                ThreeLetterIsoCode = "PRK",
	                NumericIsoCode = 408,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Kuwait",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "KW",
	                ThreeLetterIsoCode = "KWT",
	                NumericIsoCode = 414,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Malaysia",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "MY",
	                ThreeLetterIsoCode = "MYS",
	                NumericIsoCode = 458,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Mexico",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "MX",
	                ThreeLetterIsoCode = "MEX",
	                NumericIsoCode = 484,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Netherlands",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "NL",
	                ThreeLetterIsoCode = "NLD",
	                NumericIsoCode = 528,
	                SubjectToVat = true,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "New Zealand",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "NZ",
	                ThreeLetterIsoCode = "NZL",
	                NumericIsoCode = 554,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Norway",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "NO",
	                ThreeLetterIsoCode = "NOR",
	                NumericIsoCode = 578,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Pakistan",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "PK",
	                ThreeLetterIsoCode = "PAK",
	                NumericIsoCode = 586,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Paraguay",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "PY",
	                ThreeLetterIsoCode = "PRY",
	                NumericIsoCode = 600,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Peru",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "PE",
	                ThreeLetterIsoCode = "PER",
	                NumericIsoCode = 604,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Philippines",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "PH",
	                ThreeLetterIsoCode = "PHL",
	                NumericIsoCode = 608,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Poland",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "PL",
	                ThreeLetterIsoCode = "POL",
	                NumericIsoCode = 616,
	                SubjectToVat = true,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Portugal",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "PT",
	                ThreeLetterIsoCode = "PRT",
	                NumericIsoCode = 620,
	                SubjectToVat = true,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Puerto Rico",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "PR",
	                ThreeLetterIsoCode = "PRI",
	                NumericIsoCode = 630,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Qatar",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "QA",
	                ThreeLetterIsoCode = "QAT",
	                NumericIsoCode = 634,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Romania",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "RO",
	                ThreeLetterIsoCode = "ROM",
	                NumericIsoCode = 642,
	                SubjectToVat = true,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Russia",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "RU",
	                ThreeLetterIsoCode = "RUS",
	                NumericIsoCode = 643,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Saudi Arabia",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "SA",
	                ThreeLetterIsoCode = "SAU",
	                NumericIsoCode = 682,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Singapore",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "SG",
	                ThreeLetterIsoCode = "SGP",
	                NumericIsoCode = 702,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Slovakia (Slovak Republic)",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "SK",
	                ThreeLetterIsoCode = "SVK",
	                NumericIsoCode = 703,
	                SubjectToVat = true,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Slovenia",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "SI",
	                ThreeLetterIsoCode = "SVN",
	                NumericIsoCode = 705,
	                SubjectToVat = true,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "South Africa",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "ZA",
	                ThreeLetterIsoCode = "ZAF",
	                NumericIsoCode = 710,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Spain",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "ES",
	                ThreeLetterIsoCode = "ESP",
	                NumericIsoCode = 724,
	                SubjectToVat = true,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Sweden",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "SE",
	                ThreeLetterIsoCode = "SWE",
	                NumericIsoCode = 752,
	                SubjectToVat = true,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Taiwan",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "TW",
	                ThreeLetterIsoCode = "TWN",
	                NumericIsoCode = 158,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Thailand",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "TH",
	                ThreeLetterIsoCode = "THA",
	                NumericIsoCode = 764,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Turkey",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "TR",
	                ThreeLetterIsoCode = "TUR",
	                NumericIsoCode = 792,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Ukraine",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "UA",
	                ThreeLetterIsoCode = "UKR",
	                NumericIsoCode = 804,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "United Arab Emirates",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "AE",
	                ThreeLetterIsoCode = "ARE",
	                NumericIsoCode = 784,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "United Kingdom",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "GB",
	                ThreeLetterIsoCode = "GBR",
	                NumericIsoCode = 826,
	                SubjectToVat = true,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "United States minor outlying islands",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "UM",
	                ThreeLetterIsoCode = "UMI",
	                NumericIsoCode = 581,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Uruguay",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "UY",
	                ThreeLetterIsoCode = "URY",
	                NumericIsoCode = 858,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Uzbekistan",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "UZ",
	                ThreeLetterIsoCode = "UZB",
	                NumericIsoCode = 860,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Venezuela",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "VE",
	                ThreeLetterIsoCode = "VEN",
	                NumericIsoCode = 862,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Serbia",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "RS",
	                ThreeLetterIsoCode = "SRB",
	                NumericIsoCode = 688,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Afghanistan",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "AF",
	                ThreeLetterIsoCode = "AFG",
	                NumericIsoCode = 4,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Albania",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "AL",
	                ThreeLetterIsoCode = "ALB",
	                NumericIsoCode = 8,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Algeria",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "DZ",
	                ThreeLetterIsoCode = "DZA",
	                NumericIsoCode = 12,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "American Samoa",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "AS",
	                ThreeLetterIsoCode = "ASM",
	                NumericIsoCode = 16,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Andorra",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "AD",
	                ThreeLetterIsoCode = "AND",
	                NumericIsoCode = 20,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Angola",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "AO",
	                ThreeLetterIsoCode = "AGO",
	                NumericIsoCode = 24,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Anguilla",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "AI",
	                ThreeLetterIsoCode = "AIA",
	                NumericIsoCode = 660,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Antarctica",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "AQ",
	                ThreeLetterIsoCode = "ATA",
	                NumericIsoCode = 10,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Antigua and Barbuda",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "AG",
	                ThreeLetterIsoCode = "ATG",
	                NumericIsoCode = 28,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Bahrain",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "BH",
	                ThreeLetterIsoCode = "BHR",
	                NumericIsoCode = 48,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Barbados",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "BB",
	                ThreeLetterIsoCode = "BRB",
	                NumericIsoCode = 52,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Benin",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "BJ",
	                ThreeLetterIsoCode = "BEN",
	                NumericIsoCode = 204,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Bhutan",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "BT",
	                ThreeLetterIsoCode = "BTN",
	                NumericIsoCode = 64,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Botswana",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "BW",
	                ThreeLetterIsoCode = "BWA",
	                NumericIsoCode = 72,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Bouvet Island",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "BV",
	                ThreeLetterIsoCode = "BVT",
	                NumericIsoCode = 74,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "British Indian Ocean Territory",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "IO",
	                ThreeLetterIsoCode = "IOT",
	                NumericIsoCode = 86,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Brunei Darussalam",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "BN",
	                ThreeLetterIsoCode = "BRN",
	                NumericIsoCode = 96,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Burkina Faso",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "BF",
	                ThreeLetterIsoCode = "BFA",
	                NumericIsoCode = 854,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Burundi",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "BI",
	                ThreeLetterIsoCode = "BDI",
	                NumericIsoCode = 108,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Cambodia",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "KH",
	                ThreeLetterIsoCode = "KHM",
	                NumericIsoCode = 116,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Cameroon",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "CM",
	                ThreeLetterIsoCode = "CMR",
	                NumericIsoCode = 120,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Cape Verde",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "CV",
	                ThreeLetterIsoCode = "CPV",
	                NumericIsoCode = 132,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Central African Republic",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "CF",
	                ThreeLetterIsoCode = "CAF",
	                NumericIsoCode = 140,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Chad",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "TD",
	                ThreeLetterIsoCode = "TCD",
	                NumericIsoCode = 148,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Christmas Island",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "CX",
	                ThreeLetterIsoCode = "CXR",
	                NumericIsoCode = 162,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Cocos (Keeling) Islands",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "CC",
	                ThreeLetterIsoCode = "CCK",
	                NumericIsoCode = 166,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Comoros",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "KM",
	                ThreeLetterIsoCode = "COM",
	                NumericIsoCode = 174,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Congo",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "CG",
	                ThreeLetterIsoCode = "COG",
	                NumericIsoCode = 178,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Cook Islands",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "CK",
	                ThreeLetterIsoCode = "COK",
	                NumericIsoCode = 184,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Cote D'Ivoire",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "CI",
	                ThreeLetterIsoCode = "CIV",
	                NumericIsoCode = 384,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Djibouti",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "DJ",
	                ThreeLetterIsoCode = "DJI",
	                NumericIsoCode = 262,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Dominica",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "DM",
	                ThreeLetterIsoCode = "DMA",
	                NumericIsoCode = 212,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "El Salvador",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "SV",
	                ThreeLetterIsoCode = "SLV",
	                NumericIsoCode = 222,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Equatorial Guinea",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "GQ",
	                ThreeLetterIsoCode = "GNQ",
	                NumericIsoCode = 226,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Eritrea",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "ER",
	                ThreeLetterIsoCode = "ERI",
	                NumericIsoCode = 232,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Estonia",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "EE",
	                ThreeLetterIsoCode = "EST",
	                NumericIsoCode = 233,
	                SubjectToVat = true,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Ethiopia",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "ET",
	                ThreeLetterIsoCode = "ETH",
	                NumericIsoCode = 231,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Falkland Islands (Malvinas)",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "FK",
	                ThreeLetterIsoCode = "FLK",
	                NumericIsoCode = 238,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Faroe Islands",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "FO",
	                ThreeLetterIsoCode = "FRO",
	                NumericIsoCode = 234,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Fiji",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "FJ",
	                ThreeLetterIsoCode = "FJI",
	                NumericIsoCode = 242,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "French Guiana",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "GF",
	                ThreeLetterIsoCode = "GUF",
	                NumericIsoCode = 254,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "French Polynesia",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "PF",
	                ThreeLetterIsoCode = "PYF",
	                NumericIsoCode = 258,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "French Southern Territories",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "TF",
	                ThreeLetterIsoCode = "ATF",
	                NumericIsoCode = 260,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Gabon",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "GA",
	                ThreeLetterIsoCode = "GAB",
	                NumericIsoCode = 266,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Gambia",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "GM",
	                ThreeLetterIsoCode = "GMB",
	                NumericIsoCode = 270,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Ghana",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "GH",
	                ThreeLetterIsoCode = "GHA",
	                NumericIsoCode = 288,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Greenland",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "GL",
	                ThreeLetterIsoCode = "GRL",
	                NumericIsoCode = 304,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Grenada",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "GD",
	                ThreeLetterIsoCode = "GRD",
	                NumericIsoCode = 308,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Guadeloupe",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "GP",
	                ThreeLetterIsoCode = "GLP",
	                NumericIsoCode = 312,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Guam",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "GU",
	                ThreeLetterIsoCode = "GUM",
	                NumericIsoCode = 316,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Guinea",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "GN",
	                ThreeLetterIsoCode = "GIN",
	                NumericIsoCode = 324,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Guinea-bissau",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "GW",
	                ThreeLetterIsoCode = "GNB",
	                NumericIsoCode = 624,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Guyana",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "GY",
	                ThreeLetterIsoCode = "GUY",
	                NumericIsoCode = 328,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Haiti",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "HT",
	                ThreeLetterIsoCode = "HTI",
	                NumericIsoCode = 332,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Heard and Mc Donald Islands",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "HM",
	                ThreeLetterIsoCode = "HMD",
	                NumericIsoCode = 334,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Honduras",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "HN",
	                ThreeLetterIsoCode = "HND",
	                NumericIsoCode = 340,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Iceland",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "IS",
	                ThreeLetterIsoCode = "ISL",
	                NumericIsoCode = 352,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Iran (Islamic Republic of)",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "IR",
	                ThreeLetterIsoCode = "IRN",
	                NumericIsoCode = 364,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Iraq",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "IQ",
	                ThreeLetterIsoCode = "IRQ",
	                NumericIsoCode = 368,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Kenya",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "KE",
	                ThreeLetterIsoCode = "KEN",
	                NumericIsoCode = 404,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Kiribati",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "KI",
	                ThreeLetterIsoCode = "KIR",
	                NumericIsoCode = 296,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Korea",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "KR",
	                ThreeLetterIsoCode = "KOR",
	                NumericIsoCode = 410,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Kyrgyzstan",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "KG",
	                ThreeLetterIsoCode = "KGZ",
	                NumericIsoCode = 417,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Lao People's Democratic Republic",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "LA",
	                ThreeLetterIsoCode = "LAO",
	                NumericIsoCode = 418,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Latvia",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "LV",
	                ThreeLetterIsoCode = "LVA",
	                NumericIsoCode = 428,
	                SubjectToVat = true,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Lebanon",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "LB",
	                ThreeLetterIsoCode = "LBN",
	                NumericIsoCode = 422,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Lesotho",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "LS",
	                ThreeLetterIsoCode = "LSO",
	                NumericIsoCode = 426,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Liberia",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "LR",
	                ThreeLetterIsoCode = "LBR",
	                NumericIsoCode = 430,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Libyan Arab Jamahiriya",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "LY",
	                ThreeLetterIsoCode = "LBY",
	                NumericIsoCode = 434,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Liechtenstein",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "LI",
	                ThreeLetterIsoCode = "LIE",
	                NumericIsoCode = 438,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Lithuania",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "LT",
	                ThreeLetterIsoCode = "LTU",
	                NumericIsoCode = 440,
	                SubjectToVat = true,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Luxembourg",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "LU",
	                ThreeLetterIsoCode = "LUX",
	                NumericIsoCode = 442,
	                SubjectToVat = true,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Macau",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "MO",
	                ThreeLetterIsoCode = "MAC",
	                NumericIsoCode = 446,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Macedonia",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "MK",
	                ThreeLetterIsoCode = "MKD",
	                NumericIsoCode = 807,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Madagascar",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "MG",
	                ThreeLetterIsoCode = "MDG",
	                NumericIsoCode = 450,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Malawi",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "MW",
	                ThreeLetterIsoCode = "MWI",
	                NumericIsoCode = 454,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Maldives",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "MV",
	                ThreeLetterIsoCode = "MDV",
	                NumericIsoCode = 462,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Mali",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "ML",
	                ThreeLetterIsoCode = "MLI",
	                NumericIsoCode = 466,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Malta",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "MT",
	                ThreeLetterIsoCode = "MLT",
	                NumericIsoCode = 470,
	                SubjectToVat = true,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Marshall Islands",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "MH",
	                ThreeLetterIsoCode = "MHL",
	                NumericIsoCode = 584,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Martinique",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "MQ",
	                ThreeLetterIsoCode = "MTQ",
	                NumericIsoCode = 474,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Mauritania",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "MR",
	                ThreeLetterIsoCode = "MRT",
	                NumericIsoCode = 478,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Mauritius",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "MU",
	                ThreeLetterIsoCode = "MUS",
	                NumericIsoCode = 480,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Mayotte",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "YT",
	                ThreeLetterIsoCode = "MYT",
	                NumericIsoCode = 175,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Micronesia",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "FM",
	                ThreeLetterIsoCode = "FSM",
	                NumericIsoCode = 583,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Moldova",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "MD",
	                ThreeLetterIsoCode = "MDA",
	                NumericIsoCode = 498,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Monaco",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "MC",
	                ThreeLetterIsoCode = "MCO",
	                NumericIsoCode = 492,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Mongolia",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "MN",
	                ThreeLetterIsoCode = "MNG",
	                NumericIsoCode = 496,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Montserrat",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "MS",
	                ThreeLetterIsoCode = "MSR",
	                NumericIsoCode = 500,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Morocco",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "MA",
	                ThreeLetterIsoCode = "MAR",
	                NumericIsoCode = 504,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Mozambique",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "MZ",
	                ThreeLetterIsoCode = "MOZ",
	                NumericIsoCode = 508,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Myanmar",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "MM",
	                ThreeLetterIsoCode = "MMR",
	                NumericIsoCode = 104,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Namibia",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "NA",
	                ThreeLetterIsoCode = "NAM",
	                NumericIsoCode = 516,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Nauru",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "NR",
	                ThreeLetterIsoCode = "NRU",
	                NumericIsoCode = 520,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Nepal",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "NP",
	                ThreeLetterIsoCode = "NPL",
	                NumericIsoCode = 524,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Netherlands Antilles",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "AN",
	                ThreeLetterIsoCode = "ANT",
	                NumericIsoCode = 530,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "New Caledonia",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "NC",
	                ThreeLetterIsoCode = "NCL",
	                NumericIsoCode = 540,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Nicaragua",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "NI",
	                ThreeLetterIsoCode = "NIC",
	                NumericIsoCode = 558,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Niger",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "NE",
	                ThreeLetterIsoCode = "NER",
	                NumericIsoCode = 562,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Nigeria",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "NG",
	                ThreeLetterIsoCode = "NGA",
	                NumericIsoCode = 566,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Niue",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "NU",
	                ThreeLetterIsoCode = "NIU",
	                NumericIsoCode = 570,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Norfolk Island",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "NF",
	                ThreeLetterIsoCode = "NFK",
	                NumericIsoCode = 574,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Northern Mariana Islands",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "MP",
	                ThreeLetterIsoCode = "MNP",
	                NumericIsoCode = 580,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Oman",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "OM",
	                ThreeLetterIsoCode = "OMN",
	                NumericIsoCode = 512,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Palau",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "PW",
	                ThreeLetterIsoCode = "PLW",
	                NumericIsoCode = 585,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Panama",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "PA",
	                ThreeLetterIsoCode = "PAN",
	                NumericIsoCode = 591,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Papua New Guinea",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "PG",
	                ThreeLetterIsoCode = "PNG",
	                NumericIsoCode = 598,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Pitcairn",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "PN",
	                ThreeLetterIsoCode = "PCN",
	                NumericIsoCode = 612,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Reunion",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "RE",
	                ThreeLetterIsoCode = "REU",
	                NumericIsoCode = 638,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Rwanda",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "RW",
	                ThreeLetterIsoCode = "RWA",
	                NumericIsoCode = 646,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Saint Kitts and Nevis",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "KN",
	                ThreeLetterIsoCode = "KNA",
	                NumericIsoCode = 659,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Saint Lucia",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "LC",
	                ThreeLetterIsoCode = "LCA",
	                NumericIsoCode = 662,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Saint Vincent and the Grenadines",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "VC",
	                ThreeLetterIsoCode = "VCT",
	                NumericIsoCode = 670,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Samoa",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "WS",
	                ThreeLetterIsoCode = "WSM",
	                NumericIsoCode = 882,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "San Marino",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "SM",
	                ThreeLetterIsoCode = "SMR",
	                NumericIsoCode = 674,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Sao Tome and Principe",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "ST",
	                ThreeLetterIsoCode = "STP",
	                NumericIsoCode = 678,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Senegal",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "SN",
	                ThreeLetterIsoCode = "SEN",
	                NumericIsoCode = 686,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Seychelles",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "SC",
	                ThreeLetterIsoCode = "SYC",
	                NumericIsoCode = 690,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Sierra Leone",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "SL",
	                ThreeLetterIsoCode = "SLE",
	                NumericIsoCode = 694,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Solomon Islands",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "SB",
	                ThreeLetterIsoCode = "SLB",
	                NumericIsoCode = 90,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Somalia",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "SO",
	                ThreeLetterIsoCode = "SOM",
	                NumericIsoCode = 706,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "South Georgia & South Sandwich Islands",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "GS",
	                ThreeLetterIsoCode = "SGS",
	                NumericIsoCode = 239,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Sri Lanka",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "LK",
	                ThreeLetterIsoCode = "LKA",
	                NumericIsoCode = 144,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "St. Helena",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "SH",
	                ThreeLetterIsoCode = "SHN",
	                NumericIsoCode = 654,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "St. Pierre and Miquelon",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "PM",
	                ThreeLetterIsoCode = "SPM",
	                NumericIsoCode = 666,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Sudan",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "SD",
	                ThreeLetterIsoCode = "SDN",
	                NumericIsoCode = 736,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Suriname",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "SR",
	                ThreeLetterIsoCode = "SUR",
	                NumericIsoCode = 740,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Svalbard and Jan Mayen Islands",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "SJ",
	                ThreeLetterIsoCode = "SJM",
	                NumericIsoCode = 744,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Swaziland",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "SZ",
	                ThreeLetterIsoCode = "SWZ",
	                NumericIsoCode = 748,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Syrian Arab Republic",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "SY",
	                ThreeLetterIsoCode = "SYR",
	                NumericIsoCode = 760,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Tajikistan",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "TJ",
	                ThreeLetterIsoCode = "TJK",
	                NumericIsoCode = 762,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Tanzania",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "TZ",
	                ThreeLetterIsoCode = "TZA",
	                NumericIsoCode = 834,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Togo",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "TG",
	                ThreeLetterIsoCode = "TGO",
	                NumericIsoCode = 768,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Tokelau",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "TK",
	                ThreeLetterIsoCode = "TKL",
	                NumericIsoCode = 772,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Tonga",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "TO",
	                ThreeLetterIsoCode = "TON",
	                NumericIsoCode = 776,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Trinidad and Tobago",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "TT",
	                ThreeLetterIsoCode = "TTO",
	                NumericIsoCode = 780,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Tunisia",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "TN",
	                ThreeLetterIsoCode = "TUN",
	                NumericIsoCode = 788,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Turkmenistan",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "TM",
	                ThreeLetterIsoCode = "TKM",
	                NumericIsoCode = 795,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Turks and Caicos Islands",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "TC",
	                ThreeLetterIsoCode = "TCA",
	                NumericIsoCode = 796,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Tuvalu",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "TV",
	                ThreeLetterIsoCode = "TUV",
	                NumericIsoCode = 798,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Uganda",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "UG",
	                ThreeLetterIsoCode = "UGA",
	                NumericIsoCode = 800,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Vanuatu",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "VU",
	                ThreeLetterIsoCode = "VUT",
	                NumericIsoCode = 548,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Vatican City State (Holy See)",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "VA",
	                ThreeLetterIsoCode = "VAT",
	                NumericIsoCode = 336,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Viet Nam",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "VN",
	                ThreeLetterIsoCode = "VNM",
	                NumericIsoCode = 704,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Virgin Islands (British)",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "VG",
	                ThreeLetterIsoCode = "VGB",
	                NumericIsoCode = 92,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Virgin Islands (U.S.)",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "VI",
	                ThreeLetterIsoCode = "VIR",
	                NumericIsoCode = 850,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Wallis and Futuna Islands",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "WF",
	                ThreeLetterIsoCode = "WLF",
	                NumericIsoCode = 876,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Western Sahara",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "EH",
	                ThreeLetterIsoCode = "ESH",
	                NumericIsoCode = 732,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Yemen",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "YE",
	                ThreeLetterIsoCode = "YEM",
	                NumericIsoCode = 887,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Zambia",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "ZM",
	                ThreeLetterIsoCode = "ZMB",
	                NumericIsoCode = 894,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
                new Country
                {
	                Name = "Zimbabwe",
	                AllowsBilling = true,
	                AllowsShipping = true,
	                TwoLetterIsoCode = "ZW",
	                ThreeLetterIsoCode = "ZWE",
	                NumericIsoCode = 716,
	                SubjectToVat = false,
	                DisplayOrder = 100,
	                Published = true
                },
            };
            this.Alter(entities);
            return entities;
        }

        public IList<ShippingMethod> ShippingMethods()
        {
            var entities = new List<ShippingMethod>()
            {
                new ShippingMethod
                    {
                        Name = "In-Store Pickup",
                        Description ="Pick up your items at the store",
                        DisplayOrder = 0
                    },
                new ShippingMethod
                    {
                        Name = "By Ground",
                        Description ="Compared to other shipping methods, like by flight or over seas, ground shipping is carried out closer to the earth",
                        DisplayOrder = 1
                    },
            };
            this.Alter(entities);
            return entities;
        }

        public IList<CustomerRole> CustomerRoles()
        {
            var crAdministrators = new CustomerRole
            {
                Name = "Administrators",
                Active = true,
                IsSystemRole = true,
                SystemName = SystemCustomerRoleNames.Administrators,
            };
            var crForumModerators = new CustomerRole
            {
                Name = "Forum Moderators",
                Active = true,
                IsSystemRole = true,
                SystemName = SystemCustomerRoleNames.ForumModerators,
            };
            var crRegistered = new CustomerRole
            {
                Name = "Registered",
                Active = true,
                IsSystemRole = true,
                SystemName = SystemCustomerRoleNames.Registered,
            };
            var crGuests = new CustomerRole
            {
                Name = "Guests",
                Active = true,
                IsSystemRole = true,
                SystemName = SystemCustomerRoleNames.Guests,
            };
            var entities = new List<CustomerRole>
                                {
                                    crAdministrators,
                                    crForumModerators,
                                    crRegistered,
                                    crGuests
                                };
            this.Alter(entities);
            return entities;
        }

        public Address AdminAddress()
        {
            string addressThreeLetterIsoCode = "USA";
            var cCountry = this.Countries().Where(x => x.ThreeLetterIsoCode == addressThreeLetterIsoCode);

            var entity = new Address()
            {
                FirstName = "John",
                LastName = "Smith",
                PhoneNumber = "12345678",
                Email = "admin@myshop.com",
                FaxNumber = "",
                Company = "John Smith LLC",
                Address1 = "1234 Main Road",
                Address2 = "",
                City = "New York",
                StateProvince = cCountry.FirstOrDefault().StateProvinces.FirstOrDefault(),

                //StateProvince = _stateProvinceRepository.Table.Where(sp => sp.Name == "New York").FirstOrDefault(),
                Country = cCountry.FirstOrDefault(),
                ZipPostalCode = "12212",
                CreatedOnUtc = DateTime.UtcNow,
            };
            this.Alter(entity);
            return entity;
        }

        public Customer SearchEngineUser()
        {
            var entity = new Customer()
            {
                Email = "builtin@search-engine-record.com",
                CustomerGuid = Guid.NewGuid(),
                PasswordFormat = PasswordFormat.Clear,
                AdminComment = "Built-in system guest record used for requests from search engines.",
                Active = true,
                IsSystemAccount = true,
                SystemName = SystemCustomerNames.SearchEngine,
                CreatedOnUtc = DateTime.UtcNow,
                LastActivityDateUtc = DateTime.UtcNow,
            };

            this.Alter(entity);
            return entity;
        }

        public Customer BackgroundTaskUser()
        {
            var entity = new Customer()
            {
                Email = "builtin@background-task-record.com",
                CustomerGuid = Guid.NewGuid(),
                PasswordFormat = PasswordFormat.Clear,
                AdminComment = "Built-in system record used for background tasks.",
                Active = true,
                IsSystemAccount = true,
                SystemName = SystemCustomerNames.BackgroundTask,
                CreatedOnUtc = DateTime.UtcNow,
                LastActivityDateUtc = DateTime.UtcNow,
            };

            this.Alter(entity);
            return entity;
        }

        public IList<EmailAccount> EmailAccounts()
        {
            var entities = new List<EmailAccount>()
            {
                new EmailAccount
                {
                    Email = "test@mail.com",
                    DisplayName = "Store name",
                    Host = "smtp.mail.com",
                    Port = 25,
                    Username = "123",
                    Password = "123",
                    EnableSsl = false,
                    UseDefaultCredentials = false
                }
            };
            this.Alter(entities);
            return entities;
        }

        public IList<MessageTemplate> MessageTemplates()
        {
			var eaGeneral = this.EmailAccounts().FirstOrDefault(x => x.Email != null);

            string cssString = @"<style type=""text/css"">address, blockquote, center, del, dir, div, dl, fieldset, form, h1, h2, h3, h4, h5, h6, hr, ins, isindex, menu, noframes, noscript, ol, p, pre, table{ margin:0px; } body, td, p{ font-size: 13px;                        font-family: 'Segoe UI', Tahoma, Arial, Helvetica, sans-serif; line-height: 18px; color: #163764; } body{ background:#efefef; } p{ margin-top: 0px; margin-bottom: 10px; } img{ border:0px; } th{ font-weight:bold; color: #ffffff; padding: 5px 0 5px 0; } ul{ list-style-type: square; } li{ line-height: normal; margin-bottom: 5px; } .template-body { width:800px; padding: 10px; border: 1px solid #ccc; } .attr-caption { font-weight: bold; text-align:right; } .attr-value { text-align:right; min-width:158px; width:160px; }</style>";
            string templateHeader = cssString + "<center><table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" align=\"center\" bgcolor=\"#ffffff\" class=\"template-body\"><tbody><tr><td>";
            string templateFooter = "</td></tr></tbody></table></center>";

            var entities = new List<MessageTemplate>()
            {
                new MessageTemplate
                    {
                        Name = "Blog.BlogComment",
                        Subject = "%Store.Name%. New blog comment.",
                        Body = templateHeader + "<h2><a href=\"%Store.URL%\">%Store.Name%</a></h2><p><br /><br />A new blog comment has been created for blog post \"%BlogComment.BlogPostTitle%\".</p>" + templateFooter,
                        IsActive = true,
                        EmailAccountId = eaGeneral.Id,
                    },
                new MessageTemplate
                    {
                        Name = "Customer.BackInStock",
                        Subject = "%Store.Name%. Back in stock notification",
                        Body = templateHeader + "<h2><a href=\"%Store.URL%\">%Store.Name%</a></h2><p><br /><br />Hello %Customer.FullName%, <br />Product \"%BackInStockSubscription.ProductName%\" is in stock.</p>" + templateFooter,
                        IsActive = true,
                        EmailAccountId = eaGeneral.Id,
                    },
                new MessageTemplate
                    {
                        Name = "Customer.EmailValidationMessage",
                        Subject = "%Store.Name%. Email validation",
                        Body = templateHeader + "<h2><a href=\"%Store.URL%\">%Store.Name%</a></h2><br /><br />To activate your account <a href=\"%Customer.AccountActivationURL%\">click here</a>.     <br />  <br />  %Store.Name%" + templateFooter,
                        IsActive = true,
                        EmailAccountId = eaGeneral.Id,
                    },
                new MessageTemplate
                    {
                        Name = "Customer.NewPM",
                        Subject = "%Store.Name%. You have received a new private message",
                        Body = templateHeader + "<h2><a href=\"%Store.URL%\">%Store.Name%</a></h2><p><br /><br />You have received a new private message.</p>" + templateFooter,
                        IsActive = true,
                        EmailAccountId = eaGeneral.Id,
                    },
                new MessageTemplate
                    {
                        Name = "Customer.PasswordRecovery",
                        Subject = "%Store.Name%. Password recovery",
                        Body = templateHeader + "<h2><a href=\"%Store.URL%\">%Store.Name%</a></h2>  <br />  <br />  To change your password <a href=\"%Customer.PasswordRecoveryURL%\">click here</a>.     <br />  <br />  %Store.Name%" + templateFooter,
                        IsActive = true,
                        EmailAccountId = eaGeneral.Id,
                    },
                new MessageTemplate
                    {
                        Name = "Customer.WelcomeMessage",
                        Subject = "Welcome to %Store.Name%",
                        Body = templateHeader + "We welcome you to <a href=\"%Store.URL%\"> %Store.Name%</a>.<br /><br />You can now take part in the various services we have to offer you. Some of these services include:<br /><br /><b>Permanent Cart</b> Any products added to your online cart remain there until you remove them, or check them out.<br /><b>Address Book</b> - We can now deliver your products to another address other than yours! This is perfect to send birthday gifts direct to the birthday-person themselves.<br /><b>Order History</b> View your history of purchases that you have made with us.<br /><b>Products Reviews</b> Share your opinions on products with our other customers.<br /><br />For help with any of our online services, please email the store-owner: <a href=\"mailto:%Store.Email%\">%Store.Email%</a>.<br /><br /><b>Note:</b> This email address was given to us by one of our customers. If you did not signup to be a member, please send an email to <a href=\"mailto:%Store.Email%\">%Store.Email%</a>." + templateFooter,
                        IsActive = true,
                        EmailAccountId = eaGeneral.Id,
                    },
                new MessageTemplate
                    {
                        Name = "Forums.NewForumPost",
                        Subject = "%Store.Name%. New Post Notification.",
                        Body = templateHeader + "<p><a href=\"%Store.URL%\">%Store.Name%</a> <br /><br />A new post has been created in the topic <a href=\"%Forums.TopicURL%\">\"%Forums.TopicName%\"</a> at <a href=\"%Forums.ForumURL%\">\"%Forums.ForumName%\"</a> forum.<br /><br />Click <a href=\"%Forums.TopicURL%\">here</a> for more info.<br /><br /><b>Post author:</b> %Forums.PostAuthor%<br /><b>Post body:</b> %Forums.PostBody%</p>"  + templateFooter,
                        IsActive = true,
                        EmailAccountId = eaGeneral.Id,
                    },
                new MessageTemplate
                    {
                        Name = "Forums.NewForumTopic",
                        Subject = "%Store.Name%. New Topic Notification.",
                        Body = templateHeader + "<h2><a href=\"%Store.URL%\">%Store.Name%</a></h2><p><br /><br />A new topic <a href=\"%Forums.TopicURL%\">\"%Forums.TopicName%\"</a> has been created at <a href=\"%Forums.ForumURL%\">\"%Forums.ForumName%\"</a> forum.<br /><br />Click <a href=\"%Forums.TopicURL%\">here</a> for more info.</p>"  + templateFooter,
                        IsActive = true,
                        EmailAccountId = eaGeneral.Id,
                    },
                new MessageTemplate
                    {
                        Name = "GiftCard.Notification",
                        Subject = "%GiftCard.SenderName% has sent you a gift card for %Store.Name%",
                        Body = templateHeader + "<p>You have received a gift card for %Store.Name%</p><p>Dear %GiftCard.RecipientName%, <br /><br />%GiftCard.SenderName% (%GiftCard.SenderEmail%) has sent you a %GiftCard.Amount% gift cart for <a href=\"%Store.URL%\"> %Store.Name%</a></p><p>You gift card code is %GiftCard.CouponCode%</p><p>%GiftCard.Message%</p>"  + templateFooter,
                        IsActive = true,
                        EmailAccountId = eaGeneral.Id,
                    },
                new MessageTemplate
                    {
                        Name = "NewCustomer.Notification",
                        Subject = "%Store.Name%. New customer registration",
                        Body = templateHeader + "<h2><a href=\"%Store.URL%\">%Store.Name%</a></h2><p><br /><br />A new customer registered with your store. Below are the customer's details:<br /><b>Full name:</b> %Customer.FullName%<br /><b>Email:</b> %Customer.Email%</p>"  + templateFooter,
                        IsActive = true,
                        EmailAccountId = eaGeneral.Id,
                    },
                new MessageTemplate
                    {
                        Name = "NewReturnRequest.StoreOwnerNotification",
                        Subject = "%Store.Name%. New return request.",
                        Body = templateHeader + "<h2><a href=\"%Store.URL%\">%Store.Name%</a></h2><p><br /><br />%Customer.FullName% has just submitted a new return request. Details are below:<br /><b>Request ID:</b> %ReturnRequest.ID%<br /><b>Product:</b> %ReturnRequest.Product.Quantity% x <b>Product:</b> %ReturnRequest.Product.Name%<br /><b>Reason for return:</b> %ReturnRequest.Reason%<br /><b>Requested action:</b> %ReturnRequest.RequestedAction%<br /><b>Customer comments:</b><br />%ReturnRequest.CustomerComment%</p>"  + templateFooter,
                        IsActive = true,
                        EmailAccountId = eaGeneral.Id,
                    },
                new MessageTemplate
                    {
                        Name = "News.NewsComment",
                        Subject = "%Store.Name%. New news comment.",
                        Body = templateHeader + "<h2><a href=\"%Store.URL%\">%Store.Name%</a></h2><p><br /><br />A new news comment has been created for news \"%NewsComment.NewsTitle%\".</p>" + templateFooter,
                        IsActive = true,
                        EmailAccountId = eaGeneral.Id,
                    },
                new MessageTemplate
                    {
                        Name = "NewsLetterSubscription.ActivationMessage",
                        Subject = "%Store.Name%. Subscription activation message.",
                        Body = templateHeader + "<p><a href=\"%NewsLetterSubscription.ActivationUrl%\">Click here to confirm your subscription to our list.</a></p><p>If you received this email by mistake, simply delete it.</p>"  + templateFooter,
                        IsActive = true,
                        EmailAccountId = eaGeneral.Id,
                    },
                new MessageTemplate
                    {
                        Name = "NewsLetterSubscription.DeactivationMessage",
                        Subject = "%Store.Name%. Subscription deactivation message.",
                        Body = templateHeader + "<p><a href=\"%NewsLetterSubscription.DeactivationUrl%\">Click here to unsubscribe from news letters.</a></p><p>If you received this email by mistake, simply delete it.</p>"  + templateFooter,
                        IsActive = true,
                        EmailAccountId = eaGeneral.Id,
                    },
                new MessageTemplate
                    {
                        Name = "NewVATSubmitted.StoreOwnerNotification",
                        Subject = "%Store.Name%. New VAT number is submitted.",
                        Body = templateHeader + "<h2><a href=\"%Store.URL%\">%Store.Name%</a></h2><p><br /><br />%Customer.FullName% (%Customer.Email%) has just submitted a new VAT number. Details are below:<br /><b>VAT number:</b> %Customer.VatNumber%<br /><b>VAT number status:</b> %Customer.VatNumberStatus%<br /><b>Received name:</b> %VatValidationResult.Name%<br /><b>Received address:</b> %VatValidationResult.Address%</p>"  + templateFooter,
                        IsActive = true,
                        EmailAccountId = eaGeneral.Id,
                    },
                new MessageTemplate
                    {
                        Name = "OrderCancelled.CustomerNotification",
                        Subject = "%Store.Name%. Your order cancelled",
                        Body = templateHeader + "<h2><a href=\"%Store.URL%\">%Store.Name%</a></h2><p><br /><br />Hello %Order.CustomerFullName%, <br />Your order has been cancelled. Below is the summary of the order. <br /><br /><b>Order Number:</b> %Order.OrderNumber%<br /><b>Order Details:</b> <a target=\"_blank\" href=\"%Order.OrderURLForCustomer%\">%Order.OrderURLForCustomer%</a><br /><b>Date Ordered:</b> %Order.CreatedOn%<br /><br /><br /><br /><b>Billing Address</b><br />%Order.BillingFirstName% %Order.BillingLastName%<br />%Order.BillingAddress1%<br />%Order.BillingCity% %Order.BillingZipPostalCode%<br />%Order.BillingStateProvince% %Order.BillingCountry%<br /><br /><br /><br /><b>Shipping Address</b><br />%Order.ShippingFirstName% %Order.ShippingLastName%<br />%Order.ShippingAddress1%<br />%Order.ShippingCity% %Order.ShippingZipPostalCode%<br />%Order.ShippingStateProvince% %Order.ShippingCountry%<br /><br /><b>Shipping Method</b>: %Order.ShippingMethod%<br /><b>Zahlart:</b> %Order.PaymentMethod%<br /><br />%Order.Product(s)%</p>"  + templateFooter,
                        IsActive = true,
                        EmailAccountId = eaGeneral.Id,
                    },
                new MessageTemplate
                    {
                        Name = "OrderCompleted.CustomerNotification",
                        Subject = "%Store.Name%. Your order completed",
                        Body = templateHeader + "<h2><a href=\"%Store.URL%\">%Store.Name%</a></h2><p><br /><br />Hello %Order.CustomerFullName%, <br />Your order has been completed. Below is the summary of the order. <br /><br /><b>Order Number:</b> %Order.OrderNumber%<br /><b>Order Details:</b> <a target=\"_blank\" href=\"%Order.OrderURLForCustomer%\">%Order.OrderURLForCustomer%</a><br /><b>Date Ordered:</b> %Order.CreatedOn%<br /><br /><br /><br /><b>Billing Address</b><br />%Order.BillingFirstName% %Order.BillingLastName%<br />%Order.BillingAddress1%<br />%Order.BillingCity% %Order.BillingZipPostalCode%<br />%Order.BillingStateProvince% %Order.BillingCountry%<br /><br /><br /><br /><b>Shipping Address</b><br />%Order.ShippingFirstName% %Order.ShippingLastName%<br />%Order.ShippingAddress1%<br />%Order.ShippingCity% %Order.ShippingZipPostalCode%<br />%Order.ShippingStateProvince% %Order.ShippingCountry%<br /><br /><b>Shipping Method:</b> %Order.ShippingMethod%<br /><b>Zahlart:</b> %Order.PaymentMethod%<br /><br />%Order.Product(s)%</p>"  + templateFooter,
                        IsActive = true,
                        EmailAccountId = eaGeneral.Id,
                    },
                new MessageTemplate
                    {
                        Name = "ShipmentDelivered.CustomerNotification",
                        Subject = "Your order from %Store.Name% has been delivered.",
                        Body = templateHeader + "<h2><a href=\"%Store.URL%\">%Store.Name%</a></h2><p><br /><br />Hello %Order.CustomerFullName%, <br /> Good news! You order has been delivered. <br /> <b>Order Number:</b> %Order.OrderNumber%<br /> <b>Order Details:</b> <a href=\"%Order.OrderURLForCustomer%\" target=\"_blank\">%Order.OrderURLForCustomer%</a><br /> <b>Date Ordered:</b> %Order.CreatedOn%<br /> <br /> <br /> <br /> <b>Billing Address</b><br /> %Order.BillingFirstName% %Order.BillingLastName%<br /> %Order.BillingAddress1%<br /> %Order.BillingCity% %Order.BillingZipPostalCode%<br /> %Order.BillingStateProvince% %Order.BillingCountry%<br /> <br /> <br /> <br /> <b>Shipping Address</b><br /> %Order.ShippingFirstName% %Order.ShippingLastName%<br /> %Order.ShippingAddress1%<br /> %Order.ShippingCity% %Order.ShippingZipPostalCode%<br /> %Order.ShippingStateProvince% %Order.ShippingCountry%<br /> <br /> <b>Shipping Method:</b> %Order.ShippingMethod% <br /><b>Zahlart:</b> %Order.PaymentMethod%<br /><br /><b>Delivered Products:</b><br /><br />%Shipment.Product(s)%</p>"  + templateFooter,
                        IsActive = true,
                        EmailAccountId = eaGeneral.Id,
                    },

                new MessageTemplate
                    {
                        Name = "OrderPlaced.CustomerNotification",
                        Subject = "Order receipt from %Store.Name%.",
                        Body = templateHeader + "<h2><a href=\"%Store.URL%\">%Store.Name%</a></h2><p><br /><br />Hello %Order.CustomerFullName%, <br />Thanks for buying from <a href=\"%Store.URL%\">%Store.Name%</a>. Below is the summary of the order. <br /><br /><b>Order Number:</b> %Order.OrderNumber%<br /><b>Order Details:</b> <a target=\"_blank\" href=\"%Order.OrderURLForCustomer%\">%Order.OrderURLForCustomer%</a><br /><b>Date Ordered:</b> %Order.CreatedOn%<br /><br /><br /><br /><b>Billing Address</b><br />%Order.BillingFirstName% %Order.BillingLastName%<br />%Order.BillingAddress1%<br />%Order.BillingCity% %Order.BillingZipPostalCode%<br />%Order.BillingStateProvince% %Order.BillingCountry%<br /><br /><br /><br /><b>Shipping Address</b><br />%Order.ShippingFirstName% %Order.ShippingLastName%<br />%Order.ShippingAddress1%<br />%Order.ShippingCity% %Order.ShippingZipPostalCode%<br />%Order.ShippingStateProvince% %Order.ShippingCountry%<br /><br /><b>Shipping Method:</b>&nbsp;%Order.ShippingMethod%<br /><b>Zahlart:</b> %Order.PaymentMethod%<br /><br />%Order.Product(s)%</p>"  + templateFooter,
                        IsActive = true,
                        EmailAccountId = eaGeneral.Id,
                    },
                new MessageTemplate
                    {
                        Name = "OrderPlaced.StoreOwnerNotification",
                        Subject = "%Store.Name%. Purchase Receipt for Order #%Order.OrderNumber%",
                        Body = templateHeader + "<h2><a href=\"%Store.URL%\">%Store.Name%</a></h2><p><br /><br />%Order.CustomerFullName% (%Order.CustomerEmail%) has just placed an order from your store. Below is the summary of the order. <br /><br /><b>Order Number:</b> %Order.OrderNumber%<br /><b>Date Ordered:</b> %Order.CreatedOn%<br /><br /><br /><br /><b>Billing Address</b><br />%Order.BillingFirstName% %Order.BillingLastName%<br />%Order.BillingAddress1%<br />%Order.BillingCity% %Order.BillingZipPostalCode%<br />%Order.BillingStateProvince% %Order.BillingCountry%<br /><br /><br /><br /><b>Shipping Address</b><br />%Order.ShippingFirstName% %Order.ShippingLastName%<br />%Order.ShippingAddress1%<br />%Order.ShippingCity% %Order.ShippingZipPostalCode%<br />%Order.ShippingStateProvince% %Order.ShippingCountry%<br /><br /><b>Shipping Method:</b>&nbsp;%Order.ShippingMethod%<br /><b>Zahlart:</b> %Order.PaymentMethod%<br /><br />%Order.Product(s)%</p>"  + templateFooter,
                        IsActive = true,
                        EmailAccountId = eaGeneral.Id,
                    },
                new MessageTemplate
                    {
                        Name = "ShipmentSent.CustomerNotification",
                        Subject = "Your order from %Store.Name% has been shipped.",
                        Body = templateHeader + "<h2><a href=\"%Store.URL%\">%Store.Name%</a></h2><p><br /><br />Hello %Order.CustomerFullName%!, <br />Good news! You order has been shipped. <br /><b>Order Number:</b> %Order.OrderNumber%<br /><b>Order Details:</b> <a href=\"%Order.OrderURLForCustomer%\" target=\"_blank\">%Order.OrderURLForCustomer%</a><br /><b>Date Ordered:</b> %Order.CreatedOn%<br /><br /><br /><br /><b>Billing Address</b><br />%Order.BillingFirstName% %Order.BillingLastName%<br />%Order.BillingAddress1%<br />%Order.BillingCity% %Order.BillingZipPostalCode%<br />%Order.BillingStateProvince% %Order.BillingCountry%<br /><br /><br /><br /><b>Shipping Address</b><br />%Order.ShippingFirstName% %Order.ShippingLastName%<br />%Order.ShippingAddress1%<br />%Order.ShippingCity% %Order.ShippingZipPostalCode%<br />%Order.ShippingStateProvince% %Order.ShippingCountry%<br /><br /><b>Shipping Method:</b> %Order.ShippingMethod%<br /><b>Zahlart:</b> %Order.PaymentMethod% <br /> <br /> <b>Shipped Products:</b> <br /> <br /> %Shipment.Product(s)%</p>"  + templateFooter,
                        IsActive = true,
                        EmailAccountId = eaGeneral.Id,
                    },
                new MessageTemplate
                    {
                        Name = "Product.ProductReview",
                        Subject = "%Store.Name%. New product review.",
                        Body = templateHeader + "<h2><a href=\"%Store.URL%\">%Store.Name%</a></h2><p><br /><br />A new product review has been written for product \"%ProductReview.ProductName%\".</p>" + templateFooter,
                        IsActive = true,
                        EmailAccountId = eaGeneral.Id,
                    },
                new MessageTemplate
                    {
                        Name = "QuantityBelow.StoreOwnerNotification",
                        Subject = "%Store.Name%. Quantity below notification. %ProductVariant.FullProductName%",
                        Body = templateHeader + "<h2><a href=\"%Store.URL%\">%Store.Name%</a></h2><p><br /><br />%ProductVariant.FullProductName% (<b>ID:</b> %ProductVariant.ID%) low quantity. <br /><br /><b>Quantity:</b> %ProductVariant.StockQuantity%<br /></p>" + templateFooter,
                        IsActive = true,
                        EmailAccountId = eaGeneral.Id,
                    },
                new MessageTemplate
                    {
                        Name = "ReturnRequestStatusChanged.CustomerNotification",
                        Subject = "%Store.Name%. Return request status was changed.",
                        Body = templateHeader + "<h2><a href=\"%Store.URL%\">%Store.Name%</a></h2><p> <br /><br />Hello %Customer.FullName%,<br />Your return request #%ReturnRequest.ID% status has been changed.</p>"  + templateFooter,
                        IsActive = true,
                        EmailAccountId = eaGeneral.Id,
                    },
                new MessageTemplate
                    {
                        Name = "Service.EmailAFriend",
                        Subject = "%Store.Name%. Referred Item",
                        Body = templateHeader + "<h2><a href=\"%Store.URL%\">%Store.Name%</a></h2><p><br /><br />%EmailAFriend.Email% was shopping on %Store.Name% and wanted to share the following item with you. <br /><br /><b><a target=\"_blank\" href=\"%Product.ProductURLForCustomer%\">%Product.Name%</a></b> <br />%Product.ShortDescription% <br /><br />For more info click <a target=\"_blank\" href=\"%Product.ProductURLForCustomer%\">here</a> <br /><br /><br />%EmailAFriend.PersonalMessage%<br /><br />%Store.Name%</p>" + templateFooter,
                        IsActive = true,
                        EmailAccountId = eaGeneral.Id,
                    },
                new MessageTemplate
                    {
                        Name = "Wishlist.EmailAFriend",
                        Subject = "%Store.Name%. Wishlist",
                        Body = templateHeader + "<h2><a href=\"%Store.URL%\">%Store.Name%</a></h2><p><br /><br />%Wishlist.Email% was shopping on %Store.Name% and wanted to share a wishlist with you. <br /><br /><br />For more info click <a target=\"_blank\" href=\"%Wishlist.URLForCustomer%\">here</a> <br /><br /><br />%Wishlist.PersonalMessage%<br /><br />%Store.Name%</p>" + templateFooter,
                        IsActive = true,
                        EmailAccountId = eaGeneral.Id,
                    },
                new MessageTemplate
                    {
                        Name = "Customer.NewOrderNote",
                        Subject = "%Store.Name%. New order note has been added",
                        Body = templateHeader + "<h2><a href=\"%Store.URL%\">%Store.Name%</a></h2><p><br /><br />Hello %Customer.FullName%, <br />New order note has been added to your account:<br />\"%Order.NewNoteText%\".<br /><a target=\"_blank\" href=\"%Order.OrderURLForCustomer%\">%Order.OrderURLForCustomer%</a></p>" + templateFooter,
                        IsActive = true,
                        EmailAccountId = eaGeneral.Id,
                    },
                new MessageTemplate
                    {
                        Name = "RecurringPaymentCancelled.StoreOwnerNotification",
                        Subject = "%Store.Name%. Recurring payment cancelled",
                        Body = templateHeader + "<h2><a href=\"%Store.URL%\">%Store.Name%</a></h2><p><br /><br />%Customer.FullName% (%Customer.Email%) has just cancelled a recurring payment ID=%RecurringPayment.ID%.</p>" + templateFooter,
                        IsActive = true,
                        EmailAccountId = eaGeneral.Id,
                    },
                new MessageTemplate
                    {
                        Name = "Product.AskQuestion",
                        Subject = "%Store.Name% - Question concerning '%Product.Name%' from %ProductQuestion.SenderName%",
                        Body = templateHeader + "<p>%ProductQuestion.Message%</p><p>%ProductQuestion.Message%</p><p><strong>Email:</strong> %ProductQuestion.SenderEmail%<br /><strong>Name: </strong>%ProductQuestion.SenderName%<br /><strong>Phone: </strong>%ProductQuestion.SenderPhone%</p>" + templateFooter,
                        IsActive = true,
                        EmailAccountId = eaGeneral.Id,
                    },
            };
            this.Alter(entities);
            return entities;
        }

        public IList<Topic> Topics()
        {
            var entities = new List<Topic>()
            {
                new Topic
                    {
                        SystemName = "AboutUs",
                        IncludeInSitemap = false,
                        IsPasswordProtected = false,
                        Title = "About Us",
                        Body = "<p>Put your &quot;About Us&quot; information here. You can edit this in the admin site.</p>"
                    },
                new Topic
                    {
                        SystemName = "CheckoutAsGuestOrRegister",
                        IncludeInSitemap = false,
                        IsPasswordProtected = false,
                        Title = "",
                        Body = "<p><strong>Register and save time!</strong><br />Register with us for future convenience:</p><ul><li>Fast and easy check out</li><li>Easy access to your order history and status</li></ul>"
                    },
                new Topic
                    {
                        SystemName = "ConditionsOfUse",
                        IncludeInSitemap = false,
                        IsPasswordProtected = false,
                        Title = "Conditions of use",
                        Body = "<p>Put your conditions of use information here. You can edit this in the admin site.</p>"
                    },
                new Topic
                    {
                        SystemName = "ContactUs",
                        IncludeInSitemap = false,
                        IsPasswordProtected = false,
                        Title = "",
                        Body = "<p>Put your contact information here. You can edit this in the admin site.</p>"
                    },
                new Topic
                    {
                        SystemName = "ForumWelcomeMessage",
                        IncludeInSitemap = false,
                        IsPasswordProtected = false,
                        Title = "Forums",
                        Body = "<p>Put your welcome message here. You can edit this in the admin site.</p>"
                    },
                new Topic
                    {
                        SystemName = "HomePageText",
                        IncludeInSitemap = false,
                        IsPasswordProtected = false,
                        Title = "Welcome to our store",
                        Body = "<p>Online shopping is the process consumers go through to purchase products or services over the Internet. You can edit this in the admin site.</p></p>"
                    },
                new Topic
                    {
                        SystemName = "LoginRegistrationInfo",
                        IncludeInSitemap = false,
                        IsPasswordProtected = false,
                        Title = "About login / registration",
                        Body = "<p>Put your login / registration information here. You can edit this in the admin site.</p>"
                    },
                new Topic
                    {
                        SystemName = "PrivacyInfo",
                        IncludeInSitemap = false,
                        IsPasswordProtected = false,
                        Title = "Privacy policy",
                        Body = "<p>Put your privacy policy information here. You can edit this in the admin site.</p>"
                    },
                new Topic
                    {
                        SystemName = "ShippingInfo",
                        IncludeInSitemap = false,
                        IsPasswordProtected = false,
                        Title = "Shipping & Returns",
                        Body = "<p>Put your shipping &amp; returns information here. You can edit this in the admin site.</p>"
                    },
                new Topic
                    {
                        SystemName = "PageNotFound",
                        IncludeInSitemap = false,
                        IsPasswordProtected = false,
                        Title = "",
                        Body = "<p><strong>The page you requested was not found, and we have a fine guess why.</strong><ul><li>If you typed the URL directly, please make sure the spelling is correct.</li><li>The page no longer exists. In this case, we profusely apologize for the inconvenience and for any damage this may cause.</li></ul></p>"
                    },

                //codehint: sm-add begin
                new Topic
                    {
                        SystemName = "Imprint",
                        IncludeInSitemap = false,
                        IsPasswordProtected = false,
                        Title = "Imprint",
                        Body = @"<p>Put your imprint information here. YOu can edit this in the admin site.</p>"
                    },
                new Topic
                    {
                        SystemName = "Disclaimer",
                        IncludeInSitemap = false,
                        IsPasswordProtected = false,
                        Title = "Disclaimer",
                        Body = "<p>Put your disclaimer information here. You can edit this in the admin site.</p>"
                    },
                new Topic
                    {
                        SystemName = "PaymentInfo",
                        IncludeInSitemap = false,
                        IsPasswordProtected = false,
                        Title = "Payment info",
                        Body = "<p>Put your payment information here. You can edit this in the admin site.</p>"
                    },
            };
            this.Alter(entities);
            return entities;
        }

		public static Store DefaultStore
		{
			get
			{
				return new Store()
				{
					Name = "Your store name",
					Url = "http://www.yourStore.com/",
					Hosts = "yourstore.com,www.yourstore.com",
					SslEnabled = false,
					DisplayOrder = 1
				};
			}
		}

		public IList<Store> DefaultStores(IList<Store> entities)
		{
			var imgCompanyLogo = _pictureService.InsertPicture(File.ReadAllBytes(_sampleImagesPath + "company_logo.png"), "image/png", "", true, false);

			if (imgCompanyLogo != null)
				entities.First(x => x.DisplayOrder == 1).LogoPictureId = imgCompanyLogo.Id;

			this.Alter(entities);
			return entities;
		}

        public IList<ISettings> Settings()
        {
            var imgContentSliderBg = _pictureService.InsertPicture(File.ReadAllBytes(_sampleImagesPath + "clouds.png"), "image/png", "", true, false);

            var entities = new List<ISettings>
            {
                new PdfSettings
                {
                    Enabled = true,
                    LetterPageSizeEnabled = false,
                    RenderOrderNotes = true,
                    FontFileName = "OpenSans-Regular.ttf",
                },
                new CommonSettings
                {
                    UseSystemEmailForContactUsForm = true,
                    UseStoredProceduresIfSupported = true,
                    SitemapEnabled = true,
                    SitemapIncludeCategories = true,
                    SitemapIncludeManufacturers = true,
                    SitemapIncludeProducts = false,
                    SitemapIncludeTopics = true,
                    DisplayJavaScriptDisabledWarning = false,
                    UseFullTextSearch = false,
                    FullTextMode = FulltextSearchMode.ExactMatch,
                    
                },
                new SeoSettings()
                {
                    PageTitleSeparator = ". ",
                    PageTitleSeoAdjustment = PageTitleSeoAdjustment.PagenameAfterStorename,
                    DefaultTitle = "Your store",
                    DefaultMetaKeywords = "",
                    DefaultMetaDescription = "",
                    ConvertNonWesternChars = false,
                    AllowUnicodeCharsInUrls = true,
                    ReservedUrlRecordSlugs = new List<string>() { "admin", "install", "recentlyviewedproducts", "newproducts", "compareproducts", "clearcomparelist", "setproductreviewhelpfulness", "login", "register", "logout", "cart", "wishlist", "emailwishlist", "checkout", "onepagecheckout", "contactus", "passwordrecovery", "subscribenewsletter", "blog", "boards", "inboxupdate", "sentupdate", "news", "sitemap", "sitemapseo", "search", "config" },
                },
                new SocialSettings()
                {
                    ShowSocialLinksInFooter = true,
                    FacebookLink = "#",
                    GooglePlusLink = "#",
                    TwitterLink = "#",
                    PinterestLink = "#",
                },
                new AdminAreaSettings()
                {
                    GridPageSize = 15,
                    DisplayProductPictures = true,
                },
                new CatalogSettings()
                {
                    ShowProductSku = false,
                    ShowManufacturerPartNumber = false,
                    AllowProductSorting = true,
                    AllowProductViewModeChanging = true,
                    DefaultViewMode = "grid",
                    ShowProductsFromSubcategories = false,
                    ShowCategoryProductNumber = false,
                    ShowCategoryProductNumberIncludingSubcategories = false,
                    CategoryBreadcrumbEnabled = true,
                    ShowShareButton = true,
                    PageShareCode = "<!-- AddThis Button BEGIN --><div class=\"addthis_toolbox addthis_default_style \"><a class=\"addthis_button_preferred_1\"></a><a class=\"addthis_button_preferred_2\"></a><a class=\"addthis_button_preferred_3\"></a><a class=\"addthis_button_preferred_4\"></a><a class=\"addthis_button_compact\"></a><a class=\"addthis_counter addthis_bubble_style\"></a></div><script type=\"text/javascript\">var addthis_config = {\"data_track_addressbar\":true};</script><script type=\"text/javascript\" src=\"//s7.addthis.com/js/300/addthis_widget.js#pubid=ra-50f6c18f03ecbb2f\"></script><!-- AddThis Button END -->",
                    ProductReviewsMustBeApproved = false,
                    DefaultProductRatingValue = 5,
                    AllowAnonymousUsersToReviewProduct = false,
                    NotifyStoreOwnerAboutNewProductReviews = true,
                    EmailAFriendEnabled = true,
                    AskQuestionEnabled = true,
                    AllowAnonymousUsersToEmailAFriend = false,
                    RecentlyViewedProductsNumber = 6,
                    RecentlyViewedProductsEnabled = true,
                    RecentlyAddedProductsNumber = 10,
                    RecentlyAddedProductsEnabled = true,
                    CompareProductsEnabled = true,
                    ProductSearchAutoCompleteEnabled = true,
                    ProductSearchAutoCompleteNumberOfProducts = 10,
                    ProductSearchTermMinimumLength = 3,
                    ShowProductImagesInSearchAutoComplete = false,
                    ShowBestsellersOnHomepage = false,
                    NumberOfBestsellersOnHomepage = 6,
                    SearchPageProductsPerPage = 6,
                    ProductsAlsoPurchasedEnabled = true,
                    ProductsAlsoPurchasedNumber = 6,
                    EnableDynamicPriceUpdate = false,
                    NumberOfProductTags = 15,
                    ProductsByTagPageSize = 12,
                    IncludeShortDescriptionInCompareProducts = false,
                    IncludeFullDescriptionInCompareProducts = false,
                    UseSmallProductBoxOnHomePage = true,
                    IncludeFeaturedProductsInNormalLists = false,
                    DisplayTierPricesWithDiscounts = true,
                    IgnoreDiscounts = false,
                    IgnoreFeaturedProducts = false,
                    DefaultPageSizeOptions = "12, 18, 36, 72, 150",

                    //DefaultCategoryPageSizeOptions = "12, 18, 36, 72, 150", // codehint: sm-delete
                    //DefaultManufacturerPageSizeOptions = "12, 18, 36, 72, 150", // codehint: sm-delete
                    ProductsByTagAllowCustomersToSelectPageSize = true,
                    ProductsByTagPageSizeOptions = "12, 18, 36, 72, 150",
                    MaximumBackInStockSubscriptions = 200,
                    FileUploadMaximumSizeBytes = 1024 * 200, //200KB
                    ManufacturersBlockItemsToDisplay = 5,
					DisplayAllImagesNumber = 6,	// codehint: sm-add
                    ShowColorSquaresInLists = true
                },
                new LocalizationSettings()
                {
                    //DefaultAdminLanguageId = _languageRepository.Table.First().Id, // TODO: ZA
                    DefaultAdminLanguageId = _languageService.GetAllLanguages().First().Id,
                    UseImagesForLanguageSelection = true,
                },
                new CustomerSettings()
                {
                    UsernamesEnabled = true,
                    CheckUsernameAvailabilityEnabled = false,
                    AllowUsersToChangeUsernames = false,
                    DefaultPasswordFormat = PasswordFormat.Hashed,
                    HashedPasswordFormat = "SHA1",
                    PasswordMinLength = 6,
                    UserRegistrationType = UserRegistrationType.Standard,
                    AllowCustomersToUploadAvatars = false,
                    AvatarMaximumSizeBytes = 20000,
                    DefaultAvatarEnabled = true,
                    ShowCustomersLocation = false,
                    ShowCustomersJoinDate = false,
                    AllowViewingProfiles = false,
                    NotifyNewCustomerRegistration = false,
                    HideDownloadableProductsTab = false,
                    HideBackInStockSubscriptionsTab = false,
                    DownloadableProductsValidateUser = false,
                    CustomerNameFormat = CustomerNameFormat.ShowUsernames,
                    GenderEnabled = true,
                    DateOfBirthEnabled = true,
                    CompanyEnabled = true,
                    StreetAddressEnabled = false,
                    StreetAddress2Enabled = false,
                    ZipPostalCodeEnabled = false,
                    CityEnabled = false,
                    CountryEnabled = false,
                    StateProvinceEnabled = false,
                    PhoneEnabled = false,
                    FaxEnabled = false,
                    NewsletterEnabled = true,
                    HideNewsletterBlock = false,
                    OnlineCustomerMinutes = 20,
                    StoreLastVisitedPage = true,
                },
                new AddressSettings()
                {
                    CompanyEnabled = true,
                    StreetAddressEnabled = true,
                    StreetAddressRequired = true,
                    StreetAddress2Enabled = true,
                    ZipPostalCodeEnabled = true,
                    ZipPostalCodeRequired = true,
                    CityEnabled = true,
                    CityRequired = true,
                    CountryEnabled = true,
                    StateProvinceEnabled = true,
                    PhoneEnabled = true,
                    PhoneRequired = true,
                    FaxEnabled = true,
                },
                new MediaSettings()
                {
                    AvatarPictureSize = 85,
                    ProductThumbPictureSize = 100,  //codehint: sm-edit
                    ProductDetailsPictureSize = 300,
                    ProductThumbPictureSizeOnProductDetailsPage = 70,
                    ProductVariantPictureSize = 125,
                    CategoryThumbPictureSize = 125,
                    ManufacturerThumbPictureSize = 125,
                    CartThumbPictureSize = 80,
                    MiniCartThumbPictureSize = 32,
                    AutoCompleteSearchThumbPictureSize = 20,
                    MaximumImageSize = 1280,
                    DefaultPictureZoomEnabled = true,
                    PictureZoomType = "window",
                    DefaultImageQuality = 90,
                    MultipleThumbDirectories = true
                },
                new StoreInformationSettings()
                {
                    StoreClosed = false,
                    StoreClosedAllowForAdmins = true,
                    
                    //DefaultStoreThemeForDesktops = "Alpha", // codehint: sm-delete
                    //AllowCustomerToSelectTheme = true, // codehint: sm-delete
                    //MobileDevicesSupported = true, // codehint: sm-delete
                    //DefaultStoreThemeForMobileDevices = "Mobile", // codehint: sm-delete
                    //EmulateMobileDevice = false, // codehint: sm-delete
                    DisplayMiniProfilerInPublicStore = false
                },
                new RewardPointsSettings()
                {
                    Enabled = false,
                    ExchangeRate = 1,
                    PointsForRegistration = 0,
                    PointsForPurchases_Amount = 10,
                    PointsForPurchases_Points = 1,
                    PointsForPurchases_Awarded = OrderStatus.Complete,
                    PointsForPurchases_Canceled = OrderStatus.Cancelled,
                },
                new CurrencySettings()
                {
                    PrimaryStoreCurrencyId = _currencyRepository.Table.First().Id,
                    PrimaryExchangeRateCurrencyId = _currencyRepository.Table.First().Id,

                    //PrimaryStoreCurrencyId = _currencyRepository.Table.Where(c => c.CurrencyCode == "EUR").Single().Id, // TODO: ZA
                    //PrimaryExchangeRateCurrencyId = _currencyRepository.Table.Where(c => c.CurrencyCode == "EUR").Single().Id, // TODO: ZA

                    //ActiveExchangeRateProviderSystemName = "CurrencyExchange.MoneyConverter",
                    AutoUpdateEnabled = false,
                    LastUpdateTime = 0
                },
                new MeasureSettings()
                {
                    BaseDimensionId = _measureDimensionRepository.Table.Where(m => m.SystemKeyword == "inch").Single().Id, // TODO: ZA
                    BaseWeightId = _measureWeightRepository.Table.Where(m => m.SystemKeyword == "lb").Single().Id, // TODO: ZA
                },
                new MessageTemplatesSettings()
                {
                    CaseInvariantReplacement = false,
                    Color1 = "#3A87AD",
                    Color2 = "#F7F7F7",
                    Color3 = "#F5F5F5",
                },
                new ShoppingCartSettings()
                {
                    DisplayCartAfterAddingProduct = false,
                    DisplayWishlistAfterAddingProduct = false,
                    MaximumShoppingCartItems = 1000,
                    MaximumWishlistItems = 1000,
                    AllowOutOfStockItemsToBeAddedToWishlist = false,
                    MoveItemsFromWishlistToCart = true,
                    ShowProductImagesOnShoppingCart = true,
                    ShowProductImagesOnWishList = true,
                    ShowDiscountBox = true,
                    ShowGiftCardBox = true,
                    CrossSellsNumber = 2,
                    EmailWishlistEnabled = true,
                    AllowAnonymousUsersToEmailWishlist = false,
                    MiniShoppingCartEnabled = true,
                    ShowProductImagesInMiniShoppingCart = true,
                    MiniShoppingCartProductNumber = 5,
                    RoundPricesDuringCalculation = true,
                    ShowConfirmOrderLegalHint = false
                },
                new OrderSettings()
                {
                    IsReOrderAllowed = true,
                    MinOrderSubtotalAmount = 0,
                    MinOrderTotalAmount = 0,
                    AnonymousCheckoutAllowed = true,
                    TermsOfServiceEnabled = true, // codehint: sm-edit > false,
                    OnePageCheckoutEnabled = false, // codehint: sm-edit > true,
                    ReturnRequestsEnabled = true,
                    ReturnRequestActions = new List<string>() { "Repair", "Replacement", "Store Credit" },
                    ReturnRequestReasons = new List<string>() { "Received Wrong Product", "Wrong Product Ordered", "There Was A Problem With The Product" },
                    NumberOfDaysReturnRequestAvailable = 365,
                    MinimumOrderPlacementInterval = 30,
                },
                new SecuritySettings()
                {
                    ForceSslForAllPages = false,
                    EncryptionKey = "273ece6f97dd844d",
                    AdminAreaAllowedIpAddresses = null
                },
                new ShippingSettings()
                {
                    ActiveShippingRateComputationMethodSystemNames = new List<string>() { "Shipping.FixedRate" },
                    FreeShippingOverXEnabled = false,
                    FreeShippingOverXValue = decimal.Zero,
                    FreeShippingOverXIncludingTax = false,
                    EstimateShippingEnabled = true,
                    DisplayShipmentEventsToCustomers = false,
                    ReturnValidOptionsIfThereAreAny = true,
                },
                new PaymentSettings()
                {
                    ActivePaymentMethodSystemNames = new List<string>() // TODO: ZA (welche?)
                    {
                        "Payments.CashOnDelivery",
                        "Payments.CheckMoneyOrder",
                        "Payments.Manual",
                        "Payments.PayInStore",
                        "Payments.PurchaseOrder",
                    },
                    AllowRePostingPayments = true,
                    BypassPaymentMethodSelectionIfOnlyOne = true,
                },
                new TaxSettings()
                {
                    TaxBasedOn = TaxBasedOn.BillingAddress,
                    TaxDisplayType = TaxDisplayType.ExcludingTax,
                    ActiveTaxProviderSystemName = "Tax.FixedRate",
                    DefaultTaxAddressId = 0,
                    DisplayTaxSuffix = true, // codehint: sm-edit
                    DisplayTaxRates = false,
                    PricesIncludeTax = true, // codehint: sm-edit
                    AllowCustomersToSelectTaxDisplayType = false,
                    HideZeroTax = false,
                    HideTaxInOrderSummary = false,
                    ShippingIsTaxable = false,
                    ShippingPriceIncludesTax = false,
                    ShippingTaxClassId = 0,
                    PaymentMethodAdditionalFeeIsTaxable = false,
                    PaymentMethodAdditionalFeeIncludesTax = false,
                    PaymentMethodAdditionalFeeTaxClassId = 0,
                    EuVatEnabled = false,
                    EuVatShopCountryId = 0,
                    EuVatAllowVatExemption = true,
                    EuVatUseWebService = false,
                    EuVatEmailAdminWhenNewVatSubmitted = false,
                    ShowLegalHintsInFooter = true,                  //codehint: sm-add
                    ShowLegalHintsInProductDetails = true,          //codehint: sm-add
                    ShowLegalHintsInProductList = true              //codehint: sm-add
                },
                new FileSystemSettings()
                {
                },
                new DateTimeSettings()
                {
                    DefaultStoreTimeZoneId = "",
                    AllowCustomersToSetTimeZone = false
                },
                new BlogSettings()
                {
                    Enabled = true,
                    PostsPageSize = 10,
                    AllowNotRegisteredUsersToLeaveComments = true,
                    NotifyAboutNewBlogComments = false,
                    NumberOfTags = 15,
                    ShowHeaderRssUrl = false,
                },
                new NewsSettings()
                {
                    Enabled = true,
                    AllowNotRegisteredUsersToLeaveComments = true,
                    NotifyAboutNewNewsComments = false,
                    ShowNewsOnMainPage = true,
                    MainPageNewsCount = 3,
                    NewsArchivePageSize = 10,
                    ShowHeaderRssUrl = false,
                },
                new ForumSettings()
                {
                    ForumsEnabled = true,
                    RelativeDateTimeFormattingEnabled = true,
                    AllowCustomersToDeletePosts = false,
                    AllowCustomersToEditPosts = false,
                    AllowCustomersToManageSubscriptions = false,
                    AllowGuestsToCreatePosts = false,
                    AllowGuestsToCreateTopics = false,
                    TopicSubjectMaxLength = 450,
                    PostMaxLength = 4000,
                    StrippedTopicMaxLength = 45,
                    TopicsPageSize = 10,
                    PostsPageSize = 10,
                    SearchResultsPageSize = 10,
                    LatestCustomerPostsPageSize = 10,
                    ShowCustomersPostCount = true,
                    ForumEditor = EditorType.BBCodeEditor,
                    SignaturesEnabled = true,
                    AllowPrivateMessages = false,
                    ShowAlertForPM = false,
                    PrivateMessagesPageSize = 10,
                    ForumSubscriptionsPageSize = 10,
                    NotifyAboutPrivateMessages = false,
                    PMSubjectMaxLength = 450,
                    PMTextMaxLength = 4000,
                    HomePageActiveDiscussionsTopicCount = 5,
                    ActiveDiscussionsPageTopicCount = 50,
                    ActiveDiscussionsFeedEnabled = false,
                    ActiveDiscussionsFeedCount = 25,
                    ForumFeedsEnabled = false,
                    ForumFeedCount = 10,
                    ForumSearchTermMinimumLength = 3,
                },
                new EmailAccountSettings()
                {
                    //DefaultEmailAccountId = _emailAccountRepository.Table.FirstOrDefault().Id // TODO: ZA
                    DefaultEmailAccountId = _emailAccountRepository.Table.FirstOrDefault().Id
                },
                new ContentSliderSettings()
                {
                    IsActive = true,
                    ContentSliderHeight = "300px",
                    BackgroundPictureId = imgContentSliderBg.Id,

                    //BackgroundPictureUrl = _pictureService.GetPictureUrl(contentSliderBgId),
                    Slides = new List<ContentSliderSlideSettings>(),
                },
                new ThemeSettings()
                {
                    BundleOptimizationEnabled = 0,
                    CssCacheEnabled = 0,
                    CssMinifyEnabled = 0,
                    DefaultDesktopTheme = "Alpha",
                    DefaultMobileTheme = "Mobile",
                    AllowCustomerToSelectTheme = true,
                    MobileDevicesSupported = true,
                    EmulateMobileDevice = false,
                }
            };

            this.Alter(entities);
            return entities;
        }

        public IList<ActivityLogType> ActivityLogTypes()
        {
            var entities = new List<ActivityLogType>()
                                      {
                                          //admin area activities
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "AddNewCategory",
                                                  Enabled = true,
                                                  Name = "Add a new category"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "AddNewCheckoutAttribute",
                                                  Enabled = true,
                                                  Name = "Add a new checkout attribute"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "AddNewCustomer",
                                                  Enabled = true,
                                                  Name = "Add a new customer"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "AddNewCustomerRole",
                                                  Enabled = true,
                                                  Name = "Add a new customer role"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "AddNewDiscount",
                                                  Enabled = true,
                                                  Name = "Add a new discount"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "AddNewGiftCard",
                                                  Enabled = true,
                                                  Name = "Add a new gift card"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "AddNewManufacturer",
                                                  Enabled = true,
                                                  Name = "Add a new manufacturer"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "AddNewProduct",
                                                  Enabled = true,
                                                  Name = "Add a new product"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "AddNewProductAttribute",
                                                  Enabled = true,
                                                  Name = "Add a new product attribute"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "AddNewProductVariant",
                                                  Enabled = true,
                                                  Name = "Add a new product variant"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "AddNewSetting",
                                                  Enabled = true,
                                                  Name = "Add a new setting"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "AddNewSpecAttribute",
                                                  Enabled = true,
                                                  Name = "Add a new specification attribute"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "AddNewWidget",
                                                  Enabled = true,
                                                  Name = "Add a new widget"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "DeleteCategory",
                                                  Enabled = true,
                                                  Name = "Delete category"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "DeleteCheckoutAttribute",
                                                  Enabled = true,
                                                  Name = "Delete a checkout attribute"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "DeleteCustomer",
                                                  Enabled = true,
                                                  Name = "Delete a customer"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "DeleteCustomerRole",
                                                  Enabled = true,
                                                  Name = "Delete a customer role"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "DeleteDiscount",
                                                  Enabled = true,
                                                  Name = "Delete a discount"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "DeleteGiftCard",
                                                  Enabled = true,
                                                  Name = "Delete a gift card"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "DeleteManufacturer",
                                                  Enabled = true,
                                                  Name = "Delete a manufacturer"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "DeleteProduct",
                                                  Enabled = true,
                                                  Name = "Delete a product"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "DeleteProductAttribute",
                                                  Enabled = true,
                                                  Name = "Delete a product attribute"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "DeleteProductVariant",
                                                  Enabled = true,
                                                  Name = "Delete a product variant"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "DeleteReturnRequest",
                                                  Enabled = true,
                                                  Name = "Delete a return request"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "DeleteSetting",
                                                  Enabled = true,
                                                  Name = "Delete a setting"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "DeleteSpecAttribute",
                                                  Enabled = true,
                                                  Name = "Delete a specification attribute"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "DeleteWidget",
                                                  Enabled = true,
                                                  Name = "Delete a widget"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "EditCategory",
                                                  Enabled = true,
                                                  Name = "Edit category"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "EditCheckoutAttribute",
                                                  Enabled = true,
                                                  Name = "Edit a checkout attribute"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "EditCustomer",
                                                  Enabled = true,
                                                  Name = "Edit a customer"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "EditCustomerRole",
                                                  Enabled = true,
                                                  Name = "Edit a customer role"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "EditDiscount",
                                                  Enabled = true,
                                                  Name = "Edit a discount"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "EditGiftCard",
                                                  Enabled = true,
                                                  Name = "Edit a gift card"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "EditManufacturer",
                                                  Enabled = true,
                                                  Name = "Edit a manufacturer"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "EditProduct",
                                                  Enabled = true,
                                                  Name = "Edit a product"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "EditProductAttribute",
                                                  Enabled = true,
                                                  Name = "Edit a product attribute"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "EditProductVariant",
                                                  Enabled = true,
                                                  Name = "Edit a product variant"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "EditPromotionProviders",
                                                  Enabled = true,
                                                  Name = "Edit promotion providers"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "EditReturnRequest",
                                                  Enabled = true,
                                                  Name = "Edit a return request"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "EditSettings",
                                                  Enabled = true,
                                                  Name = "Edit setting(s)"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "EditSpecAttribute",
                                                  Enabled = true,
                                                  Name = "Edit a specification attribute"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "EditWidget",
                                                  Enabled = true,
                                                  Name = "Edit a widget"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "EditThemeVars",
                                                  Enabled = true,
                                                  Name = "Edit theme variables"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "ResetThemeVars",
                                                  Enabled = true,
                                                  Name = "Reset theme variables to defaults"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "ImportThemeVars",
                                                  Enabled = true,
                                                  Name = "Import theme variables"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "ExportThemeVars",
                                                  Enabled = true,
                                                  Name = "Export theme variables"
                                              },

                                          //public store activities
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "PublicStore.ViewCategory",
                                                  Enabled = false,
                                                  Name = "Public store. View a category"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "PublicStore.ViewManufacturer",
                                                  Enabled = false,
                                                  Name = "Public store. View a manufacturer"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "PublicStore.ViewProduct",
                                                  Enabled = false,
                                                  Name = "Public store. View a product"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "PublicStore.PlaceOrder",
                                                  Enabled = false,
                                                  Name = "Public store. Place an order"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "PublicStore.SendPM",
                                                  Enabled = false,
                                                  Name = "Public store. Send PM"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "PublicStore.ContactUs",
                                                  Enabled = false,
                                                  Name = "Public store. Use contact us form"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "PublicStore.AddToCompareList",
                                                  Enabled = false,
                                                  Name = "Public store. Add to compare list"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "PublicStore.AddToShoppingCart",
                                                  Enabled = false,
                                                  Name = "Public store. Add to shopping cart"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "PublicStore.AddToWishlist",
                                                  Enabled = false,
                                                  Name = "Public store. Add to wishlist"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "PublicStore.Login",
                                                  Enabled = false,
                                                  Name = "Public store. Login"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "PublicStore.Logout",
                                                  Enabled = false,
                                                  Name = "Public store. Logout"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "PublicStore.AddProductReview",
                                                  Enabled = false,
                                                  Name = "Public store. Add product review"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "PublicStore.AddNewsComment",
                                                  Enabled = false,
                                                  Name = "Public store. Add news comment"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "PublicStore.AddBlogComment",
                                                  Enabled = false,
                                                  Name = "Public store. Add blog comment"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "PublicStore.AddForumTopic",
                                                  Enabled = false,
                                                  Name = "Public store. Add forum topic"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "PublicStore.EditForumTopic",
                                                  Enabled = false,
                                                  Name = "Public store. Edit forum topic"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "PublicStore.DeleteForumTopic",
                                                  Enabled = false,
                                                  Name = "Public store. Delete forum topic"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "PublicStore.AddForumPost",
                                                  Enabled = false,
                                                  Name = "Public store. Add forum post"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "PublicStore.EditForumPost",
                                                  Enabled = false,
                                                  Name = "Public store. Edit forum post"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "PublicStore.DeleteForumPost",
                                                  Enabled = false,
                                                  Name = "Public store. Delete forum post"
                                              },
                                        //new ActivityLogType
                                        //      {
                                        //          SystemKeyword = "EditThemeVars",
                                        //          Enabled = false,
                                        //          Name = "Edit theme variable"
                                        //      },
                                        //new ActivityLogType
                                        //      {
                                        //          SystemKeyword = "ResetThemeVars",
                                        //          Enabled = false,
                                        //          Name = "Reset theme variable"
                                        //      },
                                      };

            this.Alter(entities);
            return entities;
        }

        public IList<ProductTemplate> ProductTemplates()
        {
            var entities = new List<ProductTemplate>
                               {
                                    new ProductTemplate
                                    {
                                        Name = "Single Product Variant",
                                        ViewPath = "ProductTemplate.SingleVariant",
                                        DisplayOrder = 1
                                    },
                                    new ProductTemplate
                                    {
                                        Name = "Variants in Grid",
                                        ViewPath = "ProductTemplate.VariantsInGrid",
                                        DisplayOrder = 2
                                    }
                               };
            this.Alter(entities);
            return entities;
        }

        public IList<CategoryTemplate> CategoryTemplates()
        {
            var entities = new List<CategoryTemplate>
                               {
                                   new CategoryTemplate
                                       {
                                           Name = "Products in Grid or Lines",
                                           ViewPath = "CategoryTemplate.ProductsInGridOrLines",
                                           DisplayOrder = 1
                                       },
                               };
            this.Alter(entities);
            return entities;
        }

        public IList<ManufacturerTemplate> ManufacturerTemplates()
        {
            var entities = new List<ManufacturerTemplate>
                               {
                                   new ManufacturerTemplate
                                       {
                                           Name = "Products in Grid or Lines",
                                           ViewPath = "ManufacturerTemplate.ProductsInGridOrLines",
                                           DisplayOrder = 1
                                       },
                               };
            this.Alter(entities);
            return entities;
        }

        #endregion Mandatory data creators

        public IList<ScheduleTask> ScheduleTasks()
        {
            var entities = new List<ScheduleTask>
            {
                new ScheduleTask()
                {
                    Name = "Send emails",
                    Seconds = 60,
                    Type = "SmartStore.Services.Messages.QueuedMessagesSendTask, SmartStore.Services",
                    Enabled = true,
                    StopOnError = false,
                },
                new ScheduleTask()
                {
                    Name = "Keep alive",
                    Seconds = 300,
                    Type = "SmartStore.Services.Common.KeepAliveTask, SmartStore.Services",
                    Enabled = true,
                    StopOnError = false,
                },
                new ScheduleTask()
                {
                    Name = "Delete guests",
                    Seconds = 600,
                    Type = "SmartStore.Services.Customers.DeleteGuestsTask, SmartStore.Services",
                    Enabled = true,
                    StopOnError = false,
                },
                new ScheduleTask()
                {
                    Name = "Clear cache",
                    Seconds = 600,
                    Type = "SmartStore.Services.Caching.ClearCacheTask, SmartStore.Services",
                    Enabled = false,
                    StopOnError = false,
                },
                new ScheduleTask()
                {
                    Name = "Update currency exchange rates",
                    Seconds = 900,
                    Type = "SmartStore.Services.Directory.UpdateExchangeRateTask, SmartStore.Services",
                    Enabled = true,
                    StopOnError = false,
                },
            };
            this.Alter(entities);
            return entities;
        }

        #region Sample data creators

        // [...]
        public IList<SpecificationAttribute> SpecificationAttributes()
        {
            // var entities = new List<SpecificationAttribute>

            #region predefined older attributes

            //    var sa1 = new SpecificationAttribute
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

            #endregion predefined older attributes

            #region new attributes

            #region sa1 CPU-Manufacturer

            var sa1 = new SpecificationAttribute
            {
                Name = "CPU-Manufacturer",
                DisplayOrder = 1,
            };
            sa1.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "AMD",
                DisplayOrder = 1,
            });
            sa1.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "Intel",
                DisplayOrder = 2,
            });
            sa1.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "ARM",
                DisplayOrder = 3,
            });
            sa1.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "Samsung",
                DisplayOrder = 4,
            });
            sa1.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "Apple",
                DisplayOrder = 5,
            });

            #endregion sa1 CPU-Manufacturer

            #region sa2 color

            var sa2 = new SpecificationAttribute
            {
                Name = "color",
                DisplayOrder = 2,
            };
            sa2.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "white",
                DisplayOrder = 1,
            });
            sa2.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "black",
                DisplayOrder = 2,
            });
            sa2.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "beige",
                DisplayOrder = 3,
            });
            sa2.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "red",
                DisplayOrder = 4,
            });
            sa2.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "blue",
                DisplayOrder = 5,
            });
            sa2.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "green",
                DisplayOrder = 6,
            });
            sa2.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "yellow",
                DisplayOrder = 7,
            });

            #endregion sa2 color

            #region sa3 harddisk capacity

            var sa3 = new SpecificationAttribute
            {
                Name = "harddisk capacity",
                DisplayOrder = 3,
            };
            sa3.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "250 GB",
                DisplayOrder = 1,
            });
            sa3.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "500 GB",
                DisplayOrder = 2,
            });
            sa3.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "750 GB",
                DisplayOrder = 3,
            });
            sa3.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "1000 GB",
                DisplayOrder = 4,
            });
            sa3.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "1500 GB",
                DisplayOrder = 5,
            });

            #endregion sa3 harddisk capacity

            #region sa4 ram

            var sa4 = new SpecificationAttribute
            {
                Name = "RAM",
                DisplayOrder = 4,
            };
            sa4.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "4 GB",
                DisplayOrder = 1,
            });
            sa4.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "8 GB",
                DisplayOrder = 2,
            });
            sa4.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "13 GB",
                DisplayOrder = 3,
            });
            sa4.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "2 GB",
                DisplayOrder = 4,
            });
            sa4.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "1500 GB",
                DisplayOrder = 5,
            });

            #endregion sa4 ram

            #region sa5 Operating System

            var sa5 = new SpecificationAttribute
            {
                Name = "Operating System",
                DisplayOrder = 5,
            };
            sa5.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "Windows 7 32 Bit",
                DisplayOrder = 1,
            });
            sa5.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "Windows 7 64 Bit",
                DisplayOrder = 2,
            });
            sa5.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "Windows 8 32 Bit",
                DisplayOrder = 3,
            });
            sa5.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "Windows 8 64 Bit",
                DisplayOrder = 4,
            });
            sa5.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "Linux",
                DisplayOrder = 5,
            });
            sa5.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "Mac OS",
                DisplayOrder = 6,
            });
            sa5.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "Android 2",
                DisplayOrder = 7,
            });
            sa5.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "Android 4",
                DisplayOrder = 8,
            });
            sa5.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "iOS",
                DisplayOrder = 9,
            });

            #endregion sa5 Operating System

            #region sa6 ports

            var sa6 = new SpecificationAttribute
            {
                Name = "ports",
                DisplayOrder = 6,
            };
            sa6.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "USB 2.0",
                DisplayOrder = 1,
            });
            sa6.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "USB 3.0",
                DisplayOrder = 2,
            });
            sa6.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "Firewire",
                DisplayOrder = 3,
            });
            sa6.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "HDMI",
                DisplayOrder = 4,
            });
            sa6.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "DVI",
                DisplayOrder = 5,
            });
            sa6.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "VGA",
                DisplayOrder = 6,
            });
            sa6.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "Ethernet/RJ45",
                DisplayOrder = 7,
            });

            #endregion sa6 ports

            #region sa7 Gender

            var sa7 = new SpecificationAttribute
            {
                Name = "Gender",
                DisplayOrder = 7,
            };
            sa7.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "gentlemen",
                DisplayOrder = 1,
            });
            sa7.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "ladies",
                DisplayOrder = 2,
            });
            sa7.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "unisex",
                DisplayOrder = 3,
            });

            #endregion sa7 Gender

            #region sa8 material

            var sa8 = new SpecificationAttribute
            {
                Name = "material",
                DisplayOrder = 8,
            };
            sa8.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "stainless steel",
                DisplayOrder = 1,
            });
            sa8.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "titanium",
                DisplayOrder = 2,
            });
            sa8.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "plastic",
                DisplayOrder = 3,
            });
            sa8.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "aluminum",
                DisplayOrder = 4,
            });

            #endregion sa8 material

            #region sa9 movement

            var sa9 = new SpecificationAttribute
            {
                Name = "movement",
                DisplayOrder = 9,
            };
            sa9.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "mechanical, self winding",
                DisplayOrder = 1,
            });
            sa9.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "mechanical",
                DisplayOrder = 2,
            });
            sa9.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "quarz, battery operated",
                DisplayOrder = 3,
            });

            #endregion sa9 movement

            #region sa10 clasp

            var sa10 = new SpecificationAttribute
            {
                Name = "clasp",
                DisplayOrder = 10,
            };
            sa10.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "glidelock",
                DisplayOrder = 1,
            });
            sa10.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "folding clasp",
                DisplayOrder = 2,
            });
            sa10.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "thorn close",
                DisplayOrder = 3,
            });

            #endregion sa10 clasp

            #region sa11 window material

            var sa11 = new SpecificationAttribute
            {
                Name = "window material",
                DisplayOrder = 11,
            };
            sa11.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "mineral",
                DisplayOrder = 1,
            });
            sa11.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "sapphire",
                DisplayOrder = 2,
            });

            #endregion sa11 window material

            #region sa12 language

            var sa12 = new SpecificationAttribute
            {
                Name = "language",
                DisplayOrder = 12,
            };
            sa12.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "german",
                DisplayOrder = 1,
            });
            sa12.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "english",
                DisplayOrder = 2,
            });
            sa12.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "french",
                DisplayOrder = 3,
            });
            sa12.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "italian",
                DisplayOrder = 4,
            });

            #endregion sa12 language

            #region sa13 edition

            var sa13 = new SpecificationAttribute
            {
                Name = "edition",
                DisplayOrder = 13,
            };
            sa13.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "bound",
                DisplayOrder = 1,
            });
            sa13.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "paperback",
                DisplayOrder = 2,
            });

            #endregion sa13 edition

            #region sa14 category

            var sa14 = new SpecificationAttribute
            {
                Name = "category",
                DisplayOrder = 14,
            };
            sa14.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "adventure",
                DisplayOrder = 1,
            });
            sa14.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "fantasy & science fiction",
                DisplayOrder = 2,
            });
            sa14.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "history",
                DisplayOrder = 3,
            });
            sa14.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "internet & computer",
                DisplayOrder = 4,
            });
            sa14.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "thriller",
                DisplayOrder = 5,
            });
            sa14.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "cars",
                DisplayOrder = 6,
            });
            sa14.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "novel",
                DisplayOrder = 7,
            });
            sa14.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "cook and bake",
                DisplayOrder = 8,
            });
            sa14.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "non-fiction",
                DisplayOrder = 9,
            });

            #endregion sa14 category

            #region sa15 Computer-type

            var sa15 = new SpecificationAttribute
            {
                Name = "Computer-type",
                DisplayOrder = 15,
            };
            sa15.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "desktop",
                DisplayOrder = 1,
            });
            sa15.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "all-in-one",
                DisplayOrder = 2,
            });
            sa15.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "laptop",
                DisplayOrder = 3,
            });
            sa15.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "tablet",
                DisplayOrder = 4,
            });

            #endregion sa15 Computer-type

            #region sa16 type of mass-storage

            var sa16 = new SpecificationAttribute
            {
                Name = "type of mass-storage",
                DisplayOrder = 16,
            };
            sa16.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "HDD",
                DisplayOrder = 1,
            });
            sa16.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "SSD",
                DisplayOrder = 2,
            });
            sa16.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "Flash",
                DisplayOrder = 3,
            });

            #endregion sa16 type of mass-storage

            #region sa17 Size (ext. HDD)

            var sa17 = new SpecificationAttribute
            {
                Name = "Size (ext. HDD)",
                DisplayOrder = 17,
            };
            sa17.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "3,5",
                DisplayOrder = 1,
            });
            sa17.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "2,5",
                DisplayOrder = 2,
            });

            #endregion sa17 Size (ext. HDD)

            #region sa18 MP3 quality

            var sa18 = new SpecificationAttribute
            {
                Name = "MP3 quality",
                DisplayOrder = 18,
            };
            sa18.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "128 kbit/s",
                DisplayOrder = 1,
            });
            sa18.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "256 kbit/s",
                DisplayOrder = 2,
            });
            sa18.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "320 kbit/s",
                DisplayOrder = 3,
            });

            #endregion sa18 MP3 quality

            #region sa19 music genre

            var sa19 = new SpecificationAttribute
            {
                Name = "music genre",
                DisplayOrder = 19,
            };
            sa19.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "blues",
                DisplayOrder = 1,
            });
            sa19.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "jazz",
                DisplayOrder = 2,
            });
            sa19.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "disco",
                DisplayOrder = 3,
            });
            sa19.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "pop",
                DisplayOrder = 4,
            });
            sa19.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "funk",
                DisplayOrder = 5,
            });
            sa19.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "classical",
                DisplayOrder = 6,
            });
            sa19.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "R&B",
                DisplayOrder = 7,
            });

            #endregion sa19 music genre

            #region sa20 manufacturer

            var sa20 = new SpecificationAttribute
            {
                Name = "manufacturer",
                DisplayOrder = 20,
            };
            sa20.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "Apple",
                DisplayOrder = 1,
            });
            sa20.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "Samsung",
                DisplayOrder = 2,
            });
            sa20.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "HTC",
                DisplayOrder = 3,
            });
            sa20.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "LG",
                DisplayOrder = 4,
            });
            sa20.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "Motorola",
                DisplayOrder = 5,
            });
            sa20.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "Nokia",
                DisplayOrder = 6,
            });
            sa20.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "Sony",
                DisplayOrder = 7,
            });
            sa20.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "Blackberry",
                DisplayOrder = 8,
            });
            sa20.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "Microsoft",
                DisplayOrder = 9,
            });
            sa20.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "HP",
                DisplayOrder = 10,
            });
            sa20.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "Canon",
                DisplayOrder = 11,
            });
            sa20.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "Casio",
                DisplayOrder = 12,
            });
            sa20.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "Panasonic",
                DisplayOrder = 13,
            });
            sa20.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "Certina",
                DisplayOrder = 14,
            });
            sa20.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "Festina",
                DisplayOrder = 15,
            });

            #endregion sa20 manufacturer

            #endregion new attributes

            var entities = new List<SpecificationAttribute>
            {
                sa1,sa2,sa3,sa4,sa5,sa6,sa7,sa8,sa9,sa10,sa11,sa12,sa13,sa14,sa15,sa16,sa17,sa18,sa19,sa20
            };

            this.Alter(entities);
            return entities;
        }

        public IList<ProductAttribute> ProductAttributes()
        {
            var entities = new List<ProductAttribute>
            {
                new ProductAttribute
                {
                    Name = "Color",
                    Description = "Color",
                },
                new ProductAttribute
                {
                    Name = "Custom Text",
                    Description = "Custom Text",
                },
                new ProductAttribute
                {
                    Name = "HDD",
                    Description = "HDD",
                },
                new ProductAttribute
                {
                    Name = "OS",
                    Description = "OS",
                },
                new ProductAttribute
                {
                    Name = "Processor",
                    Description = "Processor",
                },
                new ProductAttribute
                {
                    Name = "RAM",
                    Description = "RAM",
                },
                new ProductAttribute
                {
                    Name = "Size",
                    Description = "Size"
                },
                new ProductAttribute
                {
                    Name = "Software",
                    Description = "Software",
                },
            };

            this.Alter(entities);
            return entities;
        }

        public IList<ProductTag> ProductTags()
        {
            #region tag gift
            var productTagGift = new ProductTag
			{
				Name = "gift"
			};

			_productRepository.Table.Where(pt => pt.MetaTitle == "$5 Virtual Gift Card").FirstOrDefault().ProductTags.Add(productTagGift);
			_productRepository.Table.Where(pt => pt.MetaTitle == "$25 Virtual Gift Card").FirstOrDefault().ProductTags.Add(productTagGift);
			_productRepository.Table.Where(pt => pt.MetaTitle == "$50 Virtual Gift Card").FirstOrDefault().ProductTags.Add(productTagGift);

            #endregion tag gift

            #region tag computer
            var productTagComputer = new ProductTag
            {
                Name = "computer"
            };

			_productRepository.Table.Where(pt => pt.MetaTitle == "Dell Optiplex 3010 DT Base").FirstOrDefault().ProductTags.Add(productTagComputer);
			_productRepository.Table.Where(pt => pt.MetaTitle == "Acer Aspire One 8.9").FirstOrDefault().ProductTags.Add(productTagComputer);
			_productRepository.Table.Where(pt => pt.MetaTitle == "Dell Inspiron One 23").FirstOrDefault().ProductTags.Add(productTagComputer);

            #endregion tag computer

            #region tag notebook
            var productTagNotebook = new ProductTag
            {
                Name = "notebook"
            };

			_productRepository.Table.Where(pt => pt.MetaTitle == "Acer Aspire One 8.9").FirstOrDefault().ProductTags.Add(productTagNotebook);

            #endregion productTagNotebook 

            #region tag compact
            var productTagCompact = new ProductTag
            {
                Name = "compact"
            };

			_productRepository.Table.Where(pt => pt.MetaTitle == "Acer Aspire One 8.9").FirstOrDefault().ProductTags.Add(productTagCompact);

            #endregion productTagCompact 

            #region tag book
            var productTagBook = new ProductTag
            {
                Name = "book"
            };

            _productRepository.Table.Where(pt => pt.MetaTitle == "Überman: The novel").FirstOrDefault().ProductTags.Add(productTagBook);
            _productRepository.Table.Where(pt => pt.MetaTitle == "Best Grilling Recipes").FirstOrDefault().ProductTags.Add(productTagBook);
            _productRepository.Table.Where(pt => pt.MetaTitle == "Car of superlatives").FirstOrDefault().ProductTags.Add(productTagBook);
            _productRepository.Table.Where(pt => pt.MetaTitle == "Picture Atlas Motorcycles").FirstOrDefault().ProductTags.Add(productTagBook);
            _productRepository.Table.Where(pt => pt.MetaTitle == "The Car Book").FirstOrDefault().ProductTags.Add(productTagBook);
            _productRepository.Table.Where(pt => pt.MetaTitle == "Fast Cars").FirstOrDefault().ProductTags.Add(productTagBook);
			_productRepository.Table.Where(pt => pt.MetaTitle == "Motorcycle Adventures").FirstOrDefault().ProductTags.Add(productTagBook);

            #endregion tag book

            #region tag cooking
            var productTagCooking = new ProductTag
            {
                Name = "cooking"
            };

            _productRepository.Table.Where(pt => pt.MetaTitle == "Überman: The novel").FirstOrDefault().ProductTags.Add(productTagCooking);
			_productRepository.Table.Where(pt => pt.MetaTitle == "Best Grilling Recipes").FirstOrDefault().ProductTags.Add(productTagCooking);

            #endregion tag cooking

            #region tag cars
            var productTagCars = new ProductTag
            {
                Name = "cars"
            };

            _productRepository.Table.Where(pt => pt.MetaTitle == "The Car Book").FirstOrDefault().ProductTags.Add(productTagCars);
			_productRepository.Table.Where(pt => pt.MetaTitle == "Fast Cars").FirstOrDefault().ProductTags.Add(productTagCars);

            #endregion tag cars

            #region tag motorbikes
            var productTagMotorbikes = new ProductTag
            {
                Name = "motorbikes"
            };

            _productRepository.Table.Where(pt => pt.MetaTitle == "Fast Cars").FirstOrDefault().ProductTags.Add(productTagMotorbikes);
			_productRepository.Table.Where(pt => pt.MetaTitle == "Motorcycle Adventures").FirstOrDefault().ProductTags.Add(productTagMotorbikes);

            #endregion tag motorbikes

            #region tag Intel
            var productTagIntel = new ProductTag
            {
                Name = "Intel"
            };

            _productRepository.Table.Where(pt => pt.MetaTitle == "Dell Inspiron One 23").FirstOrDefault().ProductTags.Add(productTagIntel);
            _productRepository.Table.Where(pt => pt.MetaTitle == "Dell Optiplex 3010 DT Base").FirstOrDefault().ProductTags.Add(productTagIntel);
			_productRepository.Table.Where(pt => pt.MetaTitle == "Acer Aspire One 8.9").FirstOrDefault().ProductTags.Add(productTagIntel);

            #endregion tag Intel

            #region tag Dell
            var productTagDell = new ProductTag
            {
                Name = "Dell"
            };

            _productRepository.Table.Where(pt => pt.MetaTitle == "Dell Inspiron One 23").FirstOrDefault().ProductTags.Add(productTagDell);
			_productRepository.Table.Where(pt => pt.MetaTitle == "Dell Optiplex 3010 DT Base").FirstOrDefault().ProductTags.Add(productTagDell);

            #endregion tag Dell

            #region tag iPhone
            var productTagIphone = new ProductTag
            {
                Name = "iPhone"
            };

			_productRepository.Table.Where(pt => pt.MetaTitle == "Apple iPhone 5 32 GB").FirstOrDefault().ProductTags.Add(productTagIphone);

            #endregion tag iPhone

            #region tag mp3
            var productTagMP3 = new ProductTag
            {
                Name = "mp3"
            };

            _productRepository.Table.Where(pt => pt.MetaTitle == "Antonio Vivaldi: spring").FirstOrDefault().ProductTags.Add(productTagMP3);
			_productRepository.Table.Where(pt => pt.MetaTitle == "Ludwig van Beethoven: Für Elise").FirstOrDefault().ProductTags.Add(productTagMP3);

            #endregion tag mp3

            #region tag download
            var productTagDownload = new ProductTag
            {
                Name = "download"
            };

            _productRepository.Table.Where(pt => pt.MetaTitle == "Antonio Vivaldi: spring").FirstOrDefault().ProductTags.Add(productTagDownload);
			_productRepository.Table.Where(pt => pt.MetaTitle == "Ludwig van Beethoven: Für Elise").FirstOrDefault().ProductTags.Add(productTagDownload);

            #endregion tag download

            #region tag watches
            var productTagWatches = new ProductTag
            {
                Name = "watches"
            };

			_productRepository.Table.Where(pt => pt.MetaTitle == "Certina DS Podium Big Size").FirstOrDefault().ProductTags.Add(productTagWatches);
            
            #endregion tag download

            var entities = new List<ProductTag>
            {
               productTagGift, productTagComputer, productTagNotebook, productTagCompact, productTagBook, productTagCooking, productTagCars, productTagMotorbikes, productTagIntel, productTagDell, productTagIphone,
               productTagMP3, productTagDownload
            };

            this.Alter(entities);
            return entities;
        }


        public IList<Category> CategoriesFirstLevel()
        {
            //pictures
            var pictureService = EngineContext.Current.Resolve<IPictureService>();
            var sampleImagesPath = this._sampleImagesPath;

            var categoryTemplateInGridAndLines =
                this.CategoryTemplates().Where(pt => pt.ViewPath == "CategoryTemplate.ProductsInGridOrLines").FirstOrDefault();

            //categories

            #region category definitions

            var categoryBooks = new Category
            {
                Name = "Books",
                CategoryTemplateId = categoryTemplateInGridAndLines.Id,
                PageSize = 12,
                AllowCustomersToSelectPageSize = true,
                PageSizeOptions = "12,18,36,72,150",
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "emblem_library.png"), "image/jpeg", pictureService.GetPictureSeName("Books"), true,false).Id,
                Published = true,
                DisplayOrder = 1,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                MetaTitle = "Books"
            };

            var categoryComputers = new Category
            {
                Name = "Computers",
                CategoryTemplateId = categoryTemplateInGridAndLines.Id,
                PageSize = 12,
                AllowCustomersToSelectPageSize = true,
                PageSizeOptions = "12,18,36,72,150",
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "category_computers.jpeg"), "image/jpeg", pictureService.GetPictureSeName("Computers"), true, false).Id,
                Published = true,
                DisplayOrder = 2,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                MetaTitle = "Computers"
            };

            

            var categoryCellPhones = new Category
            {
                Name = "Cell phones",
                CategoryTemplateId = categoryTemplateInGridAndLines.Id,
                PageSize = 12,
                AllowCustomersToSelectPageSize = true,
                PageSizeOptions = "12,18,36,72,150",

                //ParentCategoryId = categoryElectronics.Id,
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "category_cell_phones.jpeg"), "image/jpeg", pictureService.GetPictureSeName("Cell phones"), true, false).Id,
                Published = true,
                DisplayOrder = 3,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                MetaTitle = "Cell phones"
            };

            var categoryDigitalDownloads = new Category
            {
                Name = "Instant music",
                CategoryTemplateId = categoryTemplateInGridAndLines.Id,
                PageSize = 12,
                AllowCustomersToSelectPageSize = true,
                PageSizeOptions = "12,18,36,72,150",
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "category_digital_downloads.jpeg"), "image/jpeg", pictureService.GetPictureSeName("Digital downloads"), true, false).Id,
                Published = true,
                DisplayOrder = 6,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                MetaTitle = "Instant music"
            };

            var categoryGiftCards = new Category
            {
                Name = "Gift Cards",
                CategoryTemplateId = categoryTemplateInGridAndLines.Id,
                PageSize = 12,
                AllowCustomersToSelectPageSize = true,
                PageSizeOptions = "12,18,36,72,150",
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "category_gift_cards.jpeg"), "image/jpeg", pictureService.GetPictureSeName("Gift Cards"), true).Id,
                Published = true,
                DisplayOrder = 11,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                MetaTitle = "Gift cards"
            };

            var categoryWatches = new Category
            {
                Name = "Watches",
                CategoryTemplateId = categoryTemplateInGridAndLines.Id,
                PageSize = 12,
                AllowCustomersToSelectPageSize = true,
                PageSizeOptions = "12,18,36,72,150",
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "0001514_uhren.jpeg"), "image/jpeg", pictureService.GetPictureSeName("Watches"), true, false).Id,
                Published = true,
                DisplayOrder = 10,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                MetaTitle = "Watches"
            };

            #endregion category definitions

            var entities = new List<Category>
            {
               categoryBooks, categoryComputers, categoryCellPhones, categoryDigitalDownloads, categoryGiftCards, categoryWatches
            };

            this.Alter(entities);
            return entities;
        }


        public IList<Category> CategoriesSecondLevel()
        {
            //pictures
            var pictureService = EngineContext.Current.Resolve<IPictureService>();
            var sampleImagesPath = this._sampleImagesPath;

            var categoryTemplateInGridAndLines =
                this.CategoryTemplates().Where(pt => pt.ViewPath == "CategoryTemplate.ProductsInGridOrLines").FirstOrDefault();

            //categories

            #region category definitions

            var categoryBooksSpiegel = new Category
            {
                Name = "SPIEGEL-Bestseller",
                CategoryTemplateId = categoryTemplateInGridAndLines.Id,
                PageSize = 12,
                AllowCustomersToSelectPageSize = true,
                PageSizeOptions = "12,18,36,72,150",
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "0000930_spiegel-bestseller.png"), "image/png", pictureService.GetPictureSeName("SPIEGEL-Bestseller"), true,false).Id,
                Published = true,
                ParentCategoryId = _categoryRepository.Table.Where(x => x.MetaTitle =="Books" ).FirstOrDefault().Id, 
                DisplayOrder = 1,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                MetaTitle = "SPIEGEL-Bestseller"
            };

            var categoryBooksCookAndEnjoy = new Category
            {
                Name = "Cook and enjoy",
                CategoryTemplateId = categoryTemplateInGridAndLines.Id,
                PageSize = 12,
                AllowCustomersToSelectPageSize = true,
                PageSizeOptions = "12,18,36,72,150",
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "0000936_kochen-geniesen.jpeg"), "image/jpeg", pictureService.GetPictureSeName("Cook and enjoy"), true, false).Id,
                Published = true,
                ParentCategoryId = _categoryRepository.Table.Where(x => x.MetaTitle =="Books" ).FirstOrDefault().Id,
                DisplayOrder = 2,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                MetaTitle = "Cook and enjoy"
            };

            var categoryDesktops = new Category
            {
                Name = "Desktops",
                CategoryTemplateId = categoryTemplateInGridAndLines.Id,
                PageSize = 12,
                AllowCustomersToSelectPageSize = true,
                PageSizeOptions = "12,18,36,72,150",
                ParentCategoryId = _categoryRepository.Table.Where(x => x.MetaTitle =="Computers" ).FirstOrDefault().Id,
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "category_desktops.jpg"), "image/jpeg", pictureService.GetPictureSeName("Desktops"), true,false).Id,
                PriceRanges = "-1000;1000-1200;1200-;",
                Published = true,
                DisplayOrder = 1,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                MetaTitle = "Desktops"
            };

            var categoryNotebooks = new Category
            {
                Name = "Notebooks",
                CategoryTemplateId = categoryTemplateInGridAndLines.Id,
                PageSize = 12,
                AllowCustomersToSelectPageSize = true,
                PageSizeOptions = "12,18,36,72,150",
                ParentCategoryId = _categoryRepository.Table.Where(x => x.MetaTitle =="Computers" ).FirstOrDefault().Id,
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "category_notebooks.jpg"), "image/pjpeg", pictureService.GetPictureSeName("Notebooks"), true, false).Id,
                Published = true,
                DisplayOrder = 2,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                MetaTitle = "Notebooks"
            };

            //var categorySoftware = new Category
            //{
            //    Name = "Software",
            //    CategoryTemplateId = categoryTemplateInGridAndLines.Id,
            //    PageSize = 12,
            //    AllowCustomersToSelectPageSize = true,
            //    PageSizeOptions = "12,18,36,72,150",
            //    ParentCategoryId = _categoryRepository.Table.Where(x => x.MetaTitle =="Computers" ).FirstOrDefault().Id,
            //    PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "category_software.jpg"), "image/pjpeg", pictureService.GetPictureSeName("Software"), true, false).Id,
            //    Published = true,
            //    DisplayOrder = 2,
            //    CreatedOnUtc = DateTime.UtcNow,
            //    UpdatedOnUtc = DateTime.UtcNow,
            //    MetaTitle = "Software"
            //};

            #endregion category definitions

            var entities = new List<Category>
            {
                categoryBooksSpiegel, categoryBooksCookAndEnjoy, categoryDesktops, categoryNotebooks
            };

            this.Alter(entities);
            return entities;
        }

        public IList<Manufacturer> Manufacturers()
        {
            //pictures
            var pictureService = EngineContext.Current.Resolve<IPictureService>();
            var sampleImagesPath = this._sampleImagesPath;

            var manufacturerTemplateInGridAndLines =
                this.ManufacturerTemplates().Where(pt => pt.ViewPath == "ManufacturerTemplate.ProductsInGridOrLines").FirstOrDefault();

            //var categoryTemplateInGridAndLines =
            //    this.CategoryTemplates().Where(pt => pt.Name == "Products in Grid or Lines").FirstOrDefault();

            //categories

            #region Apple

            var manufacturerApple = new Manufacturer
            {
                Name = "Apple",
                ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                PageSize = 12,
                AllowCustomersToSelectPageSize = true,
                PageSizeOptions = "12,18,36,72,150",
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "0000746_apple_125.jpeg"), "image/pjpeg", pictureService.GetPictureSeName("Apple"), true, false).Id,
                Published = true,
                DisplayOrder = 2,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };

            #endregion Apple

            #region Samsung

            var manufacturerSamsung = new Manufacturer
            {
                Name = "Samsung",
                ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                PageSize = 12,
                AllowCustomersToSelectPageSize = true,
                PageSizeOptions = "12,18,36,72,150",
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "0000904_samsung_125.jpeg"), "image/pjpeg", pictureService.GetPictureSeName("Samsung"), true, false).Id,
                Published = true,
                DisplayOrder = 2,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };

            #endregion Samsung

            #region LG

            var manufacturerLG = new Manufacturer
            {
                Name = "LG",
                ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                PageSize = 12,
                AllowCustomersToSelectPageSize = true,
                PageSizeOptions = "12,18,36,72,150",
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "0000900_lg.jpeg"), "image/pjpeg", pictureService.GetPictureSeName("LG"), true, false).Id,
                Published = true,
                DisplayOrder = 2,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };

            #endregion LG

            #region HP

            var manufacturerHP = new Manufacturer
            {
                Name = "HP",
                ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                PageSize = 12,
                AllowCustomersToSelectPageSize = true,
                PageSizeOptions = "12,18,36,72,150",
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "HP_Logo_2012.svg.png"), "image/png", pictureService.GetPictureSeName("HP"), true, false).Id,
                Published = true,
                DisplayOrder = 2,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };

            #endregion HP

            #region Acer

            var manufacturerAcer = new Manufacturer
            {
                Name = "Acer",
                ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                PageSize = 12,
                AllowCustomersToSelectPageSize = true,
                PageSizeOptions = "12,18,36,72,150",
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "acer-logo.jpg"), "image/pjpeg", pictureService.GetPictureSeName("Acer"), true, false).Id,
                Published = true,
                DisplayOrder = 2,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };

            #endregion Acer


            #region TrekStor

            var manufacturerTrekStor = new Manufacturer
            {
                Name = "TrekStor",
                ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                PageSize = 12,
                AllowCustomersToSelectPageSize = true,
                PageSizeOptions = "12,18,36,72,150",
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "0000993_trekstor.jpeg"), "image/pjpeg", pictureService.GetPictureSeName("TrekStor"), true, false).Id,
                Published = true,
                DisplayOrder = 2,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };

            #endregion TrekStor

            #region Western Digital

            var manufacturerWesternDigital = new Manufacturer
            {
                Name = "Western Digital",
                ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                PageSize = 12,
                AllowCustomersToSelectPageSize = true,
                PageSizeOptions = "12,18,36,72,150",
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "0000994_western-digital.jpeg"), "image/pjpeg", pictureService.GetPictureSeName("Western Digital"), true, false).Id,
                Published = true,
                DisplayOrder = 2,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };

            #endregion Western Digital

            #region Dell

            var manufacturerDell = new Manufacturer
            {
                Name = "Dell",
                ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                PageSize = 12,
                AllowCustomersToSelectPageSize = true,
                PageSizeOptions = "12,18,36,72,150",
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "Dell_Logo.png"), "image/pjpeg", pictureService.GetPictureSeName("Dell"), true, false).Id,
                Published = true,
                DisplayOrder = 2,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };

            #endregion Dell

            #region MSI

            var manufacturerMSI = new Manufacturer
            {
                Name = "MSI",
                ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                PageSize = 12,
                AllowCustomersToSelectPageSize = true,
                PageSizeOptions = "12,18,36,72,150",
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "msi_logo1.jpg"), "image/pjpeg", pictureService.GetPictureSeName("MSI"), true, false).Id,
                Published = true,
                DisplayOrder = 2,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };

            #endregion MSI

            #region Canon

            var manufacturerCanon = new Manufacturer
            {
                Name = "Canon",
                ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                PageSize = 12,
                AllowCustomersToSelectPageSize = true,
                PageSizeOptions = "12,18,36,72,150",
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "0001085_canon.jpeg"), "image/pjpeg", pictureService.GetPictureSeName("Canon"), true, false).Id,
                Published = true,
                DisplayOrder = 2,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };

            #endregion Canon

            #region Casio

            var manufacturerCasio = new Manufacturer
            {
                Name = "Casio",
                ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                PageSize = 12,
                AllowCustomersToSelectPageSize = true,
                PageSizeOptions = "12,18,36,72,150",
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "0001086_casio.jpeg"), "image/pjpeg", pictureService.GetPictureSeName("Casio"), true, false).Id,
                Published = true,
                DisplayOrder = 2,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };

            #endregion Casio

            #region Panasonic

            var manufacturerPanasonic = new Manufacturer
            {
                Name = "Panasonic",
                ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                PageSize = 12,
                AllowCustomersToSelectPageSize = true,
                PageSizeOptions = "12,18,36,72,150",
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "0001087_panasonic.jpeg"), "image/pjpeg", pictureService.GetPictureSeName("Panasonic"), true, false).Id,
                Published = true,
                DisplayOrder = 2,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };

            #endregion Panasonic

            #region BlackBerry

            var manufacturerBlackBerry = new Manufacturer
            {
                Name = "BlackBerry",
                ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                PageSize = 12,
                AllowCustomersToSelectPageSize = true,
                PageSizeOptions = "12,18,36,72,150",
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "0000898_blackberry.jpeg"), "image/pjpeg", pictureService.GetPictureSeName("BlackBerry"), true, false).Id,
                Published = true,
                DisplayOrder = 2,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };

            #endregion BlackBerry

            #region HTC

            var manufacturerHTC = new Manufacturer
            {
                Name = "HTC",
                ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                PageSize = 12,
                AllowCustomersToSelectPageSize = true,
                PageSizeOptions = "12,18,36,72,150",
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "0000899_htc.png"), "image/pjpeg", pictureService.GetPictureSeName("HTC"), true, false).Id,
                Published = true,
                DisplayOrder = 2,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };

            #endregion HTC

            #region Festina

            var manufacturerFestina = new Manufacturer
            {
                Name = "Festina",
                ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                PageSize = 12,
                AllowCustomersToSelectPageSize = true,
                PageSizeOptions = "12,18,36,72,150",
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "0001591_festina.jpeg"), "image/pjpeg", pictureService.GetPictureSeName("Festina"), true, false).Id,
                Published = true,
                DisplayOrder = 2,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };

            #endregion Festina

            #region Certina

            var manufacturerCertina = new Manufacturer
            {
                Name = "Certina",
                ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                PageSize = 12,
                AllowCustomersToSelectPageSize = true,
                PageSizeOptions = "12,18,36,72,150",
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "0001328_certina_125.jpeg"), "image/pjpeg", pictureService.GetPictureSeName("Certina"), true, false).Id,
                Published = true,
                DisplayOrder = 2,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };

            #endregion Certina

            var entities = new List<Manufacturer>
            {
              manufacturerApple,manufacturerSamsung,manufacturerLG,manufacturerTrekStor, manufacturerWesternDigital,manufacturerDell, manufacturerMSI,
              manufacturerCanon, manufacturerCasio, manufacturerPanasonic, manufacturerBlackBerry, manufacturerHTC, manufacturerFestina, manufacturerCertina, manufacturerHP, manufacturerAcer };

            this.Alter(entities);
            return entities;
        }

        public IList<Product> Products()
        {
            #region definitions

            //pictures
            var pictureService = EngineContext.Current.Resolve<IPictureService>();
            var sampleImagesPath = this._sampleImagesPath;

            //downloads
            var downloadService = EngineContext.Current.Resolve<IDownloadService>();
            var sampleDownloadsPath = this._sampleDownloadsPath;

            //templates
            var productTemplateInGrid =
                this.ProductTemplates().Where(pt => pt.ViewPath == "ProductTemplate.VariantsInGrid").FirstOrDefault();
            var productTemplateSingleVariant =
                this.ProductTemplates().Where(pt => pt.ViewPath == "ProductTemplate.SingleVariant").FirstOrDefault();

            #endregion definitions

            //categoryRepository
            //_categoryRepository

            //products

            #region category Gift Cards

            #region product5GiftCard

            var product5GiftCard = new Product()
            {
                Name = "$5 Virtual Gift Card",
                ShortDescription = "$5 Gift Card. Gift Cards must be redeemed through our site Web site toward the purchase of eligible products.",
                FullDescription = "<p>Gift Cards must be redeemed through our site Web site toward the purchase of eligible products. Purchases are deducted from the GiftCard balance. Any unused balance will be placed in the recipient's GiftCard account when redeemed. If an order exceeds the amount of the GiftCard, the balance must be paid with a credit card or other available payment method.</p>",
                ProductTemplateId = productTemplateSingleVariant.Id,

                //SeName = "5-virtual-gift-card",
                AllowCustomerReviews = true,
                Published = true,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                MetaTitle = "$5 Virtual Gift Card"
            };

            product5GiftCard.ProductVariants.Add(new ProductVariant()
            {
                Price = 5M,
                IsGiftCard = true,
                GiftCardType = GiftCardType.Virtual,
                ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                Published = true,
                DisplayOrder = 1,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
            });


            //var productTag = _productTagRepository.Table.Where(pt => pt.Name == "gift").FirstOrDefault();
            //productTag.ProductCount++;
            //productTag.Products.Add(product5GiftCard);
            //_productTagRepository.Update(productTag);

            //product5GiftCard.ProductCategories.Add(new ProductCategory()
            //{
            //    Category = this._categoryRepository.Table.Where(c => c.Name == "Gift Cards").Single(),
            //    DisplayOrder = 1,
            //});

            product5GiftCard.ProductPictures.Add(new ProductPicture()
            {
                Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_5giftcart.jpeg"), "image/jpeg", pictureService.GetPictureSeName(product5GiftCard.Name), true, false),
                DisplayOrder = 1,  
            });

            #endregion product5GiftCard

            #region product25GiftCard

            var product25GiftCard = new Product()
            {
                Name = "$25 Virtual Gift Card",
                ShortDescription = "$25 Gift Card. Gift Cards must be redeemed through our site Web site toward the purchase of eligible products.",
                FullDescription = "<p>Gift Cards must be redeemed through our site Web site toward the purchase of eligible products. Purchases are deducted from the GiftCard balance. Any unused balance will be placed in the recipient's GiftCard account when redeemed. If an order exceeds the amount of the GiftCard, the balance must be paid with a credit card or other available payment method.</p>",
                ProductTemplateId = productTemplateSingleVariant.Id,

                //SeName = "5-virtual-gift-card",
                AllowCustomerReviews = true,
                Published = true,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                MetaTitle = "$25 Virtual Gift Card"
            };

            product25GiftCard.ProductVariants.Add(new ProductVariant()
            {
                Price = 25M,
                IsGiftCard = true,
                GiftCardType = GiftCardType.Virtual,
                ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                Published = true,
                DisplayOrder = 1,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
            });

            //product25GiftCard.ProductCategories.Add(new ProductCategory()
            //{
            //    Category = this._categoryRepository.Table.Where(c => c.Name == "Gift Cards").Single(),
            //    DisplayOrder = 1,
            //});

            product25GiftCard.ProductPictures.Add(new ProductPicture()
            {
                Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_25giftcart.jpeg"), "image/jpeg", pictureService.GetPictureSeName(product25GiftCard.Name), true, false),
                DisplayOrder = 1,
            });

            #endregion product25GiftCard

            #region product50GiftCard

            var product50GiftCard = new Product()
            {
                Name = "$50 Virtual Gift Card",
                ShortDescription = "$50 Gift Card. Gift Cards must be redeemed through our site Web site toward the purchase of eligible products.",
                FullDescription = "<p>Gift Cards must be redeemed through our site Web site toward the purchase of eligible products. Purchases are deducted from the GiftCard balance. Any unused balance will be placed in the recipient's GiftCard account when redeemed. If an order exceeds the amount of the GiftCard, the balance must be paid with a credit card or other available payment method.</p>",
                ProductTemplateId = productTemplateSingleVariant.Id,

                //SeName = "5-virtual-gift-card",
                AllowCustomerReviews = true,
                Published = true,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                MetaTitle = "$50 Virtual Gift Card"
            };

            product50GiftCard.ProductVariants.Add(new ProductVariant()
            {
                Price = 50M,
                IsGiftCard = true,
                GiftCardType = GiftCardType.Virtual,
                ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                Published = true,
                DisplayOrder = 1,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
            });

            //product50GiftCard.ProductCategories.Add(new ProductCategory()
            //{
            //    Category = this._categoryRepository.Table.Where(c => c.Name == "Gift Cards").Single(),
            //    DisplayOrder = 1,
            //});

            product50GiftCard.ProductPictures.Add(new ProductPicture()
            {
                Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "product_50giftcart.jpeg"), "image/jpeg", pictureService.GetPictureSeName(product50GiftCard.Name), true, false),
                DisplayOrder = 1,
            });

            #endregion product50GiftCard

            #endregion category Gift Cards

            #region category books

            #region productBooksUberMan

            var productBooksUberMan = new Product()
            {
                Name = "Überman: The novel",
                ShortDescription = "(Hardcover)",
                FullDescription = "<p>From idiots to riches - and back ... Ever since it with my Greek financial advisors were no more delicious cookies to meetings, I should have known something. Was the last cookie it when I bought a Romanian forest funds and leveraged discount certificates on lean hogs - which is sort of a more stringent bet that the price of lean hogs will remain stable, and that's nothing special because it is also available for cattle and cotton and fat pig. Again and again and I joked Kosmas Nikiforos Sarantakos. About all the part-time seer who tremblingly put for fear the euro crisis gold coins under the salami slices of their frozen pizzas And then came the day that revealed to me in almost Sarantakos fraudulent casualness that my plan had not worked out really. 'Why all of a sudden> my plan', 'I heard myself asking yet, but it was in the garage I realized what that really meant minus 211.2 percent in my portfolio report: personal bankruptcy, gutter and Drug Addiction with subsequent loss of the incisors . Not even the study of my friend, I would still be able to finance. The only way out was to me as quickly as secretly again to draw from this unspeakable Greek shit - I had to be Überman! By far the bekloppteste story about 'idiot' Simon Peter! »Tommy Jaud – Deutschlands witzigste Seite.« Alex Dengler, Bild am Sonntag</p>",
                ProductTemplateId = productTemplateSingleVariant.Id,

                //SeName = "5-virtual-gift-card",
                AllowCustomerReviews = true,
                Published = true,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                MetaTitle = "Überman: The novel"
            };

            productBooksUberMan.ProductVariants.Add(new ProductVariant()
             {
                 Price = 16.99M,

                 //DeliveryTime = _deliveryTimeRepository.Table.Where(dt => dt.DisplayOrder == 0).Single(),
                 //TaxCategoryId = _taxCategoryRepository.Table.Where(tc => tc.Name == "Books").Single().Id,
                 ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
                 OrderMinimumQuantity = 1,
                 OrderMaximumQuantity = 10000,
                 StockQuantity = 10000,
                 NotifyAdminForQuantityBelow = 1,
                 AllowBackInStockSubscriptions = false,
                 Published = true,
                 DisplayOrder = 1,
                 CreatedOnUtc = DateTime.UtcNow,
                 UpdatedOnUtc = DateTime.UtcNow,
                 IsShipEnabled = true,
                 //DeliveryTime = _deliveryTimeRepository.Table.Where(sa => sa.DisplayOrder == 0).Single()
             });

            //categories
           // productBooksUberMan.ProductCategories.Add(new ProductCategory()
           //{
           //    Category = this._categoryRepository.Table.Where(c => c.Name == "SPIEGEL-Bestseller").Single(),
           //    DisplayOrder = 1,
           //});

            //pictures

            //var productImageUberMan = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "0000899_htc.png"), "image/png", pictureService.GetPictureSeName(productBooksUberMan.Name), true,false);

            productBooksUberMan.ProductPictures.Add(new ProductPicture()
           {
               Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "0000932_uberman-der-roman.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productBooksUberMan.Name), true, false),
               DisplayOrder = 1,
           });

            //attributes
            productBooksUberMan.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute ()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 3,
                // Edition -> bound
                SpecificationAttributeOption = _specificationAttributesRepository.Table.Where(sa => sa.DisplayOrder == 13).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 1).Single()
            });
            productBooksUberMan.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 3,
                // Category -> bound
                SpecificationAttributeOption = _specificationAttributesRepository.Table.Where(sa => sa.DisplayOrder == 14).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 7).Single()
            });
            productBooksUberMan.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 3,
                // Language -> German
                SpecificationAttributeOption = _specificationAttributesRepository.Table.Where(sa => sa.DisplayOrder == 12).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 1).Single()
            });
            #endregion productBooksUberMan

            #region productBooksGefangeneDesHimmels

            var productBooksGefangeneDesHimmels = new Product()
            {
                Name = "The Prisoner of Heaven: A Novel",
                ShortDescription = "(Hardcover)",
                FullDescription = "<p>By Shadow of the Wind and The Angel's Game, the new large-Barcelona novel by Carlos Ruiz Zafón. - Barcelona, Christmas 1957th The bookseller Daniel Sempere and his friend Fermín be drawn again into a great adventure. In the continuation of his international success with Carlos Ruiz Zafón takes the reader on a fascinating journey into his Barcelona. Creepy and fascinating, with incredible suction power and humor, the novel, the story of Fermin, who 'rose from the dead, and the key to the future is.' Fermin's life story linking the threads of The Shadow of the Wind with those from The Angel's Game. A masterful puzzle that keeps the reader around the world in thrall. </p> <p> Product Hardcover: 416 pages Publisher: S. Fischer Verlag; 1 edition (October 25, 2012) Language: German ISBN-10: 3,100,954,025 ISBN-13: 978-3100954022 Original title: El prisionero del cielo Size and / or weight: 21.4 x 13.6 cm x 4.4 </p>",
                ProductTemplateId = productTemplateSingleVariant.Id,

                //SeName = "5-virtual-gift-card",
                AllowCustomerReviews = true,
                Published = true,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                MetaTitle = "The Prisoner of Heaven: A Novel"
            };

            productBooksGefangeneDesHimmels.ProductVariants.Add(new ProductVariant()
            {
                Price = 22.99M,
                //TaxCategoryId = _taxCategoryRepository.Table.Where(tc => tc.Name == "Books").Single().Id,
                //DeliveryTime = _deliveryTimeRepository.Table
                ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                Published = true,
                DisplayOrder = 1,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                IsShipEnabled = true,
                DeliveryTime = _deliveryTimeRepository.Table.Where(sa => sa.DisplayOrder == 0).Single()
            });

            //categories
            //productBooksGefangeneDesHimmels.ProductCategories.Add(new ProductCategory()
            //{
            //    Category = this._categoryRepository.Table.Where(c => c.Name == "SPIEGEL-Bestseller").Single(),
            //    DisplayOrder = 1,
            //});

            //pictures
            productBooksGefangeneDesHimmels.ProductPictures.Add(new ProductPicture()
            {
                Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "0000935_der-gefangene-des-himmels-roman_300.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productBooksGefangeneDesHimmels.Name), true, false),
                DisplayOrder = 1,
            });

            //attributes
            productBooksGefangeneDesHimmels.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 3,
                // Edition -> bound
                SpecificationAttributeOption = _specificationAttributesRepository.Table.Where(sa => sa.DisplayOrder == 13).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 1).Single()
            });
            productBooksGefangeneDesHimmels.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 3,
                // Category -> bound
                SpecificationAttributeOption = _specificationAttributesRepository.Table.Where(sa => sa.DisplayOrder == 14).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 7).Single()
            });
            productBooksGefangeneDesHimmels.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 3,
                // Language -> German
                SpecificationAttributeOption = _specificationAttributesRepository.Table.Where(sa => sa.DisplayOrder == 12).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 1).Single()
            });

            #endregion productBooksGefangeneDesHimmels

            #region productBooksBestGrillingRecipes

            var productBooksBestGrillingRecipes = new Product()
            {
                Name = "Best Grilling Recipes",
                ShortDescription = "More Than 100 Regional Favorites Tested and Perfected for the Outdoor Cook (Hardcover)",
                FullDescription = "<p> Take a winding cross-country trip and you'll discover barbecue shacks with offerings like tender-smoky Baltimore pit beef and saucy St. Louis pork steaks. To bring you the best of these hidden gems, along with all the classics, the editors of Cook's Country magazine scoured the country, then tested and perfected their favorites. HEre traditions large and small are brought into the backyard, from Hawaii's rotisserie favorite, the golden-hued Huli Huli Chicken, to fall-off-the-bone Chicago Barbecued Ribs. In Kansas City, they're all about the sauce, and for our saucy Kansas City Sticky Ribs, we found a surprise ingredient-root beer. We also tackle all the best sides. </p> <p> Not sure where or how to start? This cookbook kicks off with an easy-to-follow primer that will get newcomers all fired up. Whether you want to entertain a crowd or just want to learn to make perfect burgers, Best Grilling Recipes shows you the way. </p>",
                ProductTemplateId = productTemplateSingleVariant.Id,

                //SeName = "5-virtual-gift-card",
                AllowCustomerReviews = true,
                Published = true,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                MetaTitle = "Best Grilling Recipes"
            };

            productBooksBestGrillingRecipes.ProductVariants.Add(new ProductVariant()
            {
                Price = 27.00M,
                //TaxCategoryId = _taxCategoryRepository.Table.Where(tc => tc.Name == "Books").Single().Id,
                //DeliveryTime = _deliveryTimeRepository.Table
                ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                Published = true,
                DisplayOrder = 1,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                IsShipEnabled = true,
                DeliveryTime = _deliveryTimeRepository.Table.Where(sa => sa.DisplayOrder == 0).Single()
            });

            //categories
            //productBooksBestGrillingRecipes.ProductCategories.Add(new ProductCategory()
            //{
            //    Category = this._categoryRepository.Table.Where(c => c.Name == "Cook and enjoy").Single(),
            //    DisplayOrder = 1,
            //});

            //pictures
            productBooksBestGrillingRecipes.ProductPictures.Add(new ProductPicture()
            {
                Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "0000937_best-grilling-recipes.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productBooksBestGrillingRecipes.Name), true, false),
                DisplayOrder = 1,
            });

            //attributes
            productBooksBestGrillingRecipes.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 3,
                // Edition -> bound
                SpecificationAttributeOption = _specificationAttributesRepository.Table.Where(sa => sa.DisplayOrder == 13).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 1).Single()
            });
            productBooksBestGrillingRecipes.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 3,
                // Category -> cook & bake
                SpecificationAttributeOption = _specificationAttributesRepository.Table.Where(sa => sa.DisplayOrder == 14).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 8).Single()
            });
            productBooksBestGrillingRecipes.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 3,
                // Language -> German
                SpecificationAttributeOption = _specificationAttributesRepository.Table.Where(sa => sa.DisplayOrder == 12).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 2).Single()
            });

            #endregion productBooksBestGrillingRecipes

            #region productBooksCookingForTwo

            var productBooksCookingForTwo = new Product()
            {
                Name = "Cooking for Two",
                ShortDescription = "More Than 200 Foolproof Recipes for Weeknights and Special Occasions (Hardcover)",
                FullDescription = "<p>In Cooking for Two, the test kitchen's goal was to take traditional recipes and cut them down to size to serve just twowith tailored cooking techniques and smart shopping tips that will cut down on wasted food and wasted money. Great lasagna starts to lose its luster when you're eating the leftovers for the fourth day in a row. While it may seem obvious that a recipe for four can simply be halved to work, our testing has proved that this is not always the case; cooking with smaller amounts of ingredients often requires different preparation techniques, cooking time, temperature, and the proportion of ingredients. This was especially true as we worked on scaled-down desserts; baking is an unforgiving science in which any changes in recipe amounts often called for changes in baking times and temperatures. </p> <p> Hardcover: 352 pages<br> Publisher: America's Test Kitchen (May 2009)<br> Language: English<br> ISBN-10: 1933615435<br> ISBN-13: 978-1933615431<br> </p>",
                ProductTemplateId = productTemplateSingleVariant.Id,

                //SeName = "5-virtual-gift-card",
                AllowCustomerReviews = true,
                Published = true,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                MetaTitle = "Cooking for Two"
            };

            productBooksCookingForTwo.ProductVariants.Add(new ProductVariant()
            {
                Price = 27.00M,
                //TaxCategoryId = _taxCategoryRepository.Table.Where(tc => tc.Name == "Books").Single().Id,
                //DeliveryTime = _deliveryTimeRepository.Table
                ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                Published = true,
                DisplayOrder = 1,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                IsShipEnabled = true,
                DeliveryTime = _deliveryTimeRepository.Table.Where(sa => sa.DisplayOrder == 1).Single()
            });

            //categories
            //productBooksCookingForTwo.ProductCategories.Add(new ProductCategory()
            //{
            //    Category = this._categoryRepository.Table.Where(c => c.Name == "Cook and enjoy").Single(),
            //    DisplayOrder = 1,
            //});

            //pictures
            productBooksCookingForTwo.ProductPictures.Add(new ProductPicture()
            {
                Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "0000938_cooking-for-two.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productBooksCookingForTwo.Name), true, false),
                DisplayOrder = 1,
            });

            //attributes
            productBooksCookingForTwo.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 3,
                // Edition -> bound
                SpecificationAttributeOption = _specificationAttributesRepository.Table.Where(sa => sa.DisplayOrder == 13).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 1).Single()
            });
            productBooksCookingForTwo.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 3,
                // Category -> cook & bake
                SpecificationAttributeOption = _specificationAttributesRepository.Table.Where(sa => sa.DisplayOrder == 14).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 8).Single()
            });
            productBooksCookingForTwo.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 3,
                // Language -> German
                SpecificationAttributeOption = _specificationAttributesRepository.Table.Where(sa => sa.DisplayOrder == 12).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 2).Single()
            });

            #endregion productBooksCookingForTwo

            #region productBooksAutosDerSuperlative

            var productBooksAutosDerSuperlative = new Product()
            {
                Name = "Car of superlatives: the strongest, the first, the most beautiful, the fastest",
                ShortDescription = "Hardcover",
                FullDescription = "<p> For some, the car is only a useful means of transportation. For everyone else, there are 'cars - The Ultimate Guide' of art-connoisseur Michael Doerflinger. With authentic images, all important data and a lot of information can be presented to the fastest, most innovative, the strongest, the most unusual and the most successful examples of automotive history. A comprehensive manual for the specific reference and extensive browsing. </p>",
                ProductTemplateId = productTemplateSingleVariant.Id,
                AllowCustomerReviews = true,
                Published = true,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                MetaTitle = "Car of superlatives"
            };

            productBooksAutosDerSuperlative.ProductVariants.Add(new ProductVariant()
            {
                Price = 14.95M,
                //TaxCategoryId = _taxCategoryRepository.Table.Where(tc => tc.Name == "Books").Single().Id,
                //DeliveryTime = _deliveryTimeRepository.Table
                ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                Published = true,
                DisplayOrder = 1,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                IsShipEnabled = true,
                DeliveryTime = _deliveryTimeRepository.Table.Where(sa => sa.DisplayOrder == 2).Single()
            });

            //categories
            //productBooksAutosDerSuperlative.ProductCategories.Add(new ProductCategory()
            //{
            //    Category = this._categoryRepository.Table.Where(c => c.Name == "Books").Single(),
            //    DisplayOrder = 1,
            //});

            //pictures
            productBooksAutosDerSuperlative.ProductPictures.Add(new ProductPicture()
            {
                Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "0000944_autos-der-superlative-die-starksten-die-ersten-die-schonsten-die-schnellsten.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productBooksAutosDerSuperlative.Name), true, false),
                DisplayOrder = 1,
            });

            //attributes
            productBooksAutosDerSuperlative.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 3,
                // Edition -> bound
                SpecificationAttributeOption = _specificationAttributesRepository.Table.Where(sa => sa.DisplayOrder == 13).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 1).Single()
            });
            productBooksAutosDerSuperlative.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 3,
                // Category -> cars
                SpecificationAttributeOption = _specificationAttributesRepository.Table.Where(sa => sa.DisplayOrder == 14).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 6).Single()
            });
            productBooksAutosDerSuperlative.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 3,
                // Language -> German
                SpecificationAttributeOption = _specificationAttributesRepository.Table.Where(sa => sa.DisplayOrder == 12).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 1).Single()
            });


            #endregion productBooksAutosDerSuperlative

            #region productBooksBildatlasMotorraeder

            var productBooksBildatlasMotorraeder = new Product()
            {
                Name = "Picture Atlas Motorcycles: With more than 350 brilliant images",
                ShortDescription = "Hardcover",
                FullDescription = "<p> Motorcycles are like no other means of transportation for the great dream of freedom and adventure. This richly illustrated atlas image portrayed in brilliant color photographs and informative text, the most famous bikes of the world's motorcycle history. From the primitive steam engine under the saddle of the late 19th Century up to the hugely powerful, equipped with the latest electronics and computer technology superbikes of today he is an impressive picture of the development and fabrication of noble and fast-paced motorcycles. The myth of the motorcycle is just as much investigated as a motorcycle as a modern lifestyle product of our time. Country-specific, company-historical background information and interesting stories and History about the people who preceded drove one of the seminal inventions of recent centuries and evolved, make this comprehensive illustrated book an incomparable reference for any motorcycle enthusiast and technology enthusiasts. </p> <p> • Extensive history of the legendary models of all major motorcycle manufacturers worldwide<br> • With more than 350 brilliant color photographs and fascinating background information relating<br> • With informative drawings, stunning detail shots and explanatory info-boxes<br> </p> <p> content • 1817 1913: The beginning of a success story<br> • 1914 1945: mass mobility<br> • 1946 1990: Battle for the World Market<br> • In 1991: The modern motorcycle<br> • motorcycle cult object: From Transportation to Lifestyle<br> </p>",
                ProductTemplateId = productTemplateSingleVariant.Id,
                AllowCustomerReviews = true,
                Published = true,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                MetaTitle = "Picture Atlas Motorcycles"
            };

            productBooksBildatlasMotorraeder.ProductVariants.Add(new ProductVariant()
            {
                Price = 14.99M,
                //TaxCategoryId = _taxCategoryRepository.Table.Where(tc => tc.Name == "Books").Single().Id,
                //DeliveryTime = _deliveryTimeRepository.Table
                ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                Published = true,
                DisplayOrder = 1,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                IsShipEnabled = true,
                DeliveryTime = _deliveryTimeRepository.Table.Where(sa => sa.DisplayOrder == 0).Single()
            });

            //categories
            //productBooksBildatlasMotorraeder.ProductCategories.Add(new ProductCategory()
            //{
            //    Category = this._categoryRepository.Table.Where(c => c.Name == "Books").Single(),
            //    DisplayOrder = 1,
            //});

            //pictures
            productBooksBildatlasMotorraeder.ProductPictures.Add(new ProductPicture()
            {
                Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "0000942_bildatlas-motorrader-mit-mehr-als-350-brillanten-abbildungen.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productBooksBildatlasMotorraeder.Name), true, false),
                DisplayOrder = 1,
            });

            //attributes
            productBooksBildatlasMotorraeder.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 3,
                // Edition -> bound
                SpecificationAttributeOption = _specificationAttributesRepository.Table.Where(sa => sa.DisplayOrder == 13).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 1).Single()
            });
            productBooksBildatlasMotorraeder.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 3,
                // Category -> non-fiction
                SpecificationAttributeOption = _specificationAttributesRepository.Table.Where(sa => sa.DisplayOrder == 14).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 9).Single()
            });
            productBooksBildatlasMotorraeder.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 3,
                // Language -> German
                SpecificationAttributeOption = _specificationAttributesRepository.Table.Where(sa => sa.DisplayOrder == 12).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 1).Single()
            });

            #endregion productBooksBildatlasMotorraeder

            #region productBooksAutoBuch

            var productBooksAutoBuch = new Product()
            {
                Name = "The Car Book. The great history with over 1200 models",
                ShortDescription = "Hardcover",
                FullDescription = "<p> Makes, models, milestones<br> The car - for some, a utensil for other expression of lifestyle, cult object and passion. Few inventions have changed their lives as well as the good of the automobile 125 years ago - one more reason for this extensive chronicle. The car-book brings the history of the automobile to life. It presents more than 1200 important models - Karl Benz 'Motorwagen about legendary cult car to advanced hybrid vehicles. It explains the milestones in engine technology and portrays the big brands and their designers. Characteristics from small cars to limousines and send racing each era invite you to browse and discover. The most comprehensive and bestbebildert illustrated book on the market - it would be any car lover! </p> <p> Hardcover: 360 pages<br> Publisher: Dorling Kindersley Publishing (September 27, 2012)<br> Language: German<br> ISBN-10: 3,831,022,062<br> ISBN-13: 978-3831022069<br> Size and / or weight: 30.6 x 25.8 x 2.8 cm<br> </p>",
                ProductTemplateId = productTemplateSingleVariant.Id,
                AllowCustomerReviews = true,
                Published = true,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                MetaTitle = "The Car Book"
            };

            productBooksAutoBuch.ProductVariants.Add(new ProductVariant()
            {
                Price = 29.95M,
                //TaxCategoryId = _taxCategoryRepository.Table.Where(tc => tc.Name == "Books").Single().Id,
                //DeliveryTime = _deliveryTimeRepository.Table
                ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                Published = true,
                DisplayOrder = 1,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                IsShipEnabled = true,
                DeliveryTime = _deliveryTimeRepository.Table.Where(sa => sa.DisplayOrder == 0).Single()
            });

            //categories
            //productBooksAutoBuch.ProductCategories.Add(new ProductCategory()
            //{
            //    Category = this._categoryRepository.Table.Where(c => c.Name == "Books").Single(),
            //    DisplayOrder = 1,
            //});

            //pictures
            productBooksAutoBuch.ProductPictures.Add(new ProductPicture()
            {
                Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "0000947_das-auto-buch-die-grose-chronik-mit-uber-1200-modellen_300.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productBooksAutoBuch.Name), true, false),
                DisplayOrder = 1,
            });

            //attributes
            productBooksAutoBuch.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 3,
                // Edition -> bound
                SpecificationAttributeOption = _specificationAttributesRepository.Table.Where(sa => sa.DisplayOrder == 13).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 1).Single()
            });
            productBooksAutoBuch.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 3,
                // Category -> non-fiction
                SpecificationAttributeOption = _specificationAttributesRepository.Table.Where(sa => sa.DisplayOrder == 14).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 9).Single()
            });
            productBooksAutoBuch.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 3,
                // Language -> German
                SpecificationAttributeOption = _specificationAttributesRepository.Table.Where(sa => sa.DisplayOrder == 12).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 1).Single()
            });

            #endregion productBooksAutoBuch

            #region productBooksFastCars

            var productBooksFastCars = new Product()
            {
                Name = "Fast Cars, Image Calendar 2013",
                ShortDescription = "spiral bound",
                FullDescription = "<p> Large Size: 48.5 x 34 cm.<br> This impressive picture calendar with silver ring binding thrilled with impressive photographs of exclusive sports cars. Who understands cars not only as a pure commercial vehicles, will find the most sought-after status symbols at all: fast cars are effectively set to the razor sharp and vivid photos in scene and convey freedom, speed, strength and the highest technical perfection. Starting with the 450-horsepower Maserati GranTurismo MC Stradale on the stylish, luxurious Aston Martin Virage Volante accompany up to the produced only in small numbers Mosler Photon MT900S the fast racer with style and elegance through the months. </p> <p> Besides the calendar draws another picture to look at interesting details. There are the essential information on any sports car in the English language. After this year, the high-quality photos are framed an eye-catcher on the wall of every lover of fast cars. Even as a gift this beautiful years companion is wonderfully suited. 12 calendar pages, neutral and discreet held calendar. Printed on paper from sustainable forests. For lovers of luxury vintage cars also available in ALPHA EDITION: the large format image Classic Cars Calendar 2013: ISBN 9,783,840,733,376th </p> <p> Spiral-bound: 14 pages<br> Publisher: Alpha Edition (June 1, 2012)<br> Language: German<br> ISBN-10: 3,840,733,383<br> ISBN-13: 978-3840733383<br> Size and / or weight: 48.8 x 34.2 x 0.6 cm<br> </p>",
                ProductTemplateId = productTemplateSingleVariant.Id,
                AllowCustomerReviews = true,
                Published = true,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                MetaTitle = "Fast Cars",
                ShowOnHomePage = true
            };

            productBooksFastCars.ProductVariants.Add(new ProductVariant()
            {
                Price = 16.95M,
                //TaxCategoryId = _taxCategoryRepository.Table.Where(tc => tc.Name == "Books").Single().Id,
                //DeliveryTime = _deliveryTimeRepository.Table
                ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                Published = true,
                DisplayOrder = 1,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                IsShipEnabled = true,
                DeliveryTime = _deliveryTimeRepository.Table.Where(sa => sa.DisplayOrder == 0).Single()
            });

            //categories
            //productBooksFastCars.ProductCategories.Add(new ProductCategory()
            //{
            //    Category = this._categoryRepository.Table.Where(c => c.Name == "Books").Single(),
            //    DisplayOrder = 1,
            //});

            //pictures
            productBooksFastCars.ProductPictures.Add(new ProductPicture()
            {
                Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "0000946_fast-cars-bildkalender-2013_300.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productBooksFastCars.Name), true, false),
                DisplayOrder = 1,
            });

            //attributes
            productBooksFastCars.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 3,
                // Edition -> bound
                SpecificationAttributeOption = _specificationAttributesRepository.Table.Where(sa => sa.DisplayOrder == 13).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 1).Single()
            });
            productBooksFastCars.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 3,
                // Category -> cars
                SpecificationAttributeOption = _specificationAttributesRepository.Table.Where(sa => sa.DisplayOrder == 14).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 6).Single()
            });
            productBooksFastCars.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 3,
                // Language -> German
                SpecificationAttributeOption = _specificationAttributesRepository.Table.Where(sa => sa.DisplayOrder == 12).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 1).Single()
            });

            #endregion productBooksFastCars

            #region productBooksMotorradAbenteuer

            var productBooksMotorradAbenteuer = new Product()
            {
                Name = "Motorcycle Adventures: Riding for travel enduros",
                ShortDescription = "Hardcover",
                FullDescription = "<p> Modern travel enduro bikes are ideal for adventure travel. Their technique is complex, their weight considerably. The driving behavior changes depending on the load and distance. </p> <p> Before the tour starts, you should definitely attend a training course. This superbly illustrated book presents practical means of many informative series photos the right off-road driving in mud and sand, gravel and rock with and without luggage. In addition to the driving course full of information and tips on choosing the right motorcycle for travel planning and practical issues may be on the way. </p>",
                ProductTemplateId = productTemplateSingleVariant.Id,
                AllowCustomerReviews = true,
                Published = true,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                MetaTitle = "Motorcycle Adventures"
            };

            productBooksMotorradAbenteuer.ProductVariants.Add(new ProductVariant()
            {
                Price = 24.90M,
                //TaxCategoryId = _taxCategoryRepository.Table.Where(tc => tc.Name == "Books").Single().Id,
                //DeliveryTime = _deliveryTimeRepository.Table
                ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                Published = true,
                DisplayOrder = 1,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                IsShipEnabled = true,
                DeliveryTime = _deliveryTimeRepository.Table.Where(sa => sa.DisplayOrder == 1).Single()
            });

            //categories
            //productBooksMotorradAbenteuer.ProductCategories.Add(new ProductCategory()
            //{
            //    Category = this._categoryRepository.Table.Where(c => c.Name == "Books").Single(),
            //    DisplayOrder = 1,
            //});

            //pictures
            productBooksMotorradAbenteuer.ProductPictures.Add(new ProductPicture()
            {
                Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "0000943_motorrad-abenteuer-fahrtechnik-fur-reise-enduros.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productBooksMotorradAbenteuer.Name), true, false),
                DisplayOrder = 1,
            });

            //attributes
            productBooksMotorradAbenteuer.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 3,
                // Edition -> bound
                SpecificationAttributeOption = _specificationAttributesRepository.Table.Where(sa => sa.DisplayOrder == 13).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 1).Single()
            });
            productBooksMotorradAbenteuer.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 3,
                // Category -> cars
                SpecificationAttributeOption = _specificationAttributesRepository.Table.Where(sa => sa.DisplayOrder == 14).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 9).Single()
            });
            productBooksMotorradAbenteuer.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 3,
                // Language -> German
                SpecificationAttributeOption = _specificationAttributesRepository.Table.Where(sa => sa.DisplayOrder == 12).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 1).Single()
            });

            #endregion productBooksMotorradAbenteuer

            #endregion category books

            #region computer

            #region productComputerDellInspiron23

            var productComputerDellInspiron23 = new Product()
            {
                Name = "Dell Inspiron One 23",
                ShortDescription = "This 58 cm (23'')-All-in-One PC with Full HD, Windows 8 and powerful Intel ® Core ™ processor third generation allows practical interaction with a touch screen.",
                FullDescription = "<p> Ultra high performance all-in-one i7 PC with Windows 8, Intel ® Core ™ processor, huge 2TB hard drive and Blu-Ray drive. </p> <p> Intel® Core™ i7-3770S Processor ( 3,1 GHz, 6 MB Cache)<br> Windows 8 64bit , english<br> 8 GB1 DDR3 SDRAM at 1600 MHz<br> 2 TB-Serial ATA-Harddisk (7.200 rot/min)<br> 1GB AMD Radeon HD 7650<br> </p>",
                ProductTemplateId = productTemplateSingleVariant.Id,
                AllowCustomerReviews = true,
                Published = true,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                MetaTitle = "Dell Inspiron One 23"
            };

            #region variants
            productComputerDellInspiron23.ProductVariants.Add(new ProductVariant()
            {
                Price = 589.00M,
                //TaxCategoryId = _taxCategoryRepository.Table.Where(tc => tc.Name == "Electronics & Software").Single().Id,
                //DeliveryTime = _deliveryTimeRepository.Table
                ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                Published = true,
                DisplayOrder = 1,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                IsShipEnabled = true,
                DeliveryTime = _deliveryTimeRepository.Table.Where(sa => sa.DisplayOrder == 0).Single()
            });
            #endregion variants

            #region categories
            //categories
            //productComputerDellInspiron23.ProductCategories.Add(new ProductCategory()
            //{
            //    Category = this._categoryRepository.Table.Where(c => c.Name == "Desktops").Single(),
            //    DisplayOrder = 1,
            //});
            #endregion categories

            #region pictures

            //pictures
            productComputerDellInspiron23.ProductPictures.Add(new ProductPicture()
            {
                Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "0000953_dell-inspiron-one-23.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productComputerDellInspiron23.Name), true, false),
                DisplayOrder = 1,
            });
            productComputerDellInspiron23.ProductPictures.Add(new ProductPicture()
            {
                Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "0000954_dell-inspiron-one-23.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productComputerDellInspiron23.Name), true, false),
                DisplayOrder = 2,
            });
            productComputerDellInspiron23.ProductPictures.Add(new ProductPicture()
            {
                Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "0000956_dell-inspiron-one-23.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productComputerDellInspiron23.Name), true, false),
                DisplayOrder = 3,
            });

            #endregion pictures

            #region manufacturer

            //manufacturer
            productComputerDellInspiron23.ProductManufacturers.Add(new ProductManufacturer()
            {
                Manufacturer = this._manufacturerRepository.Table.Where(c => c.Name == "Dell").Single(),
                DisplayOrder = 1,
            });

            #endregion manufacturer

            #region SpecificationAttributes
            //attributes
            productComputerDellInspiron23.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 1,
                // CPU -> Intel
                SpecificationAttributeOption = _specificationAttributesRepository.Table.Where(sa => sa.DisplayOrder == 1).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 2).Single()
            });
            productComputerDellInspiron23.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 2,
                // RAM -> 4 GB 
                SpecificationAttributeOption = _specificationAttributesRepository.Table.Where(sa => sa.DisplayOrder == 4).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 1).Single()
            });
            productComputerDellInspiron23.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 3,
                // Harddisk-Typ / HDD
                SpecificationAttributeOption = _specificationAttributesRepository.Table.Where(sa => sa.DisplayOrder == 16).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 1).Single()
            });
            productComputerDellInspiron23.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 4,
                // Harddisk-Capacity / 750 GB
                SpecificationAttributeOption = _specificationAttributesRepository.Table.Where(sa => sa.DisplayOrder == 3).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 3).Single()
            });
            productComputerDellInspiron23.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 5,
                // OS / Windows 7 32 Bit
                SpecificationAttributeOption = _specificationAttributesRepository.Table.Where(sa => sa.DisplayOrder == 5).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 1).Single()
            });
            #endregion SpecificationAttributes

            #endregion productComputerDellInspiron23

            #region productComputerDellOptiplex3010

            var productComputerDellOptiplex3010 = new Product()
            {
                Name = "Dell Optiplex 3010 DT Base",
                ShortDescription = "SPECIAL OFFER: Extra 50 € discount on all Dell OptiPlex desktops from a value of € 549. Online Coupon:? W8DWQ0ZRKTM1, valid until 04/12/2013.",
                FullDescription = "<p>Also included in this system include To change these selections, the</p> <p> 1 Year Basic Service - On-Site NBD - No Upgrade Selected<br> No asset tag required </p> <p> The following options are default selections included with your order. <br> German (QWERTY) Dell KB212-B Multimedia USB Keyboard Black<br> X11301001<br> WINDOWS LIVE <br> OptiPlex ™ order - Germany  <br> OptiPlex ™ Intel ® Core ™ i3 sticker <br> Optical software is not required, operating system software sufficiently   <br> </p>",
                ProductTemplateId = productTemplateSingleVariant.Id,
                AllowCustomerReviews = true,
                Published = true,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                MetaTitle = "Dell Optiplex 3010 DT Base"
            };

            #region variants
            productComputerDellOptiplex3010.ProductVariants.Add(new ProductVariant()
            {
                Price = 419.00M,
                //TaxCategoryId = _taxCategoryRepository.Table.Where(tc => tc.Name == "Electronics & Software").Single().Id,
                //DeliveryTime = _deliveryTimeRepository.Table
                ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                Published = true,
                DisplayOrder = 1,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                IsShipEnabled = true,
                DeliveryTime = _deliveryTimeRepository.Table.Where(sa => sa.DisplayOrder == 0).Single()
            });
            #endregion variants

            #region categories
            //categories
            //productComputerDellInspiron23.ProductCategories.Add(new ProductCategory()
            //{
            //    Category = this._categoryRepository.Table.Where(c => c.Name == "Desktops").Single(),
            //    DisplayOrder = 1,
            //});
            #endregion categories

            #region pictures

            //pictures
            productComputerDellOptiplex3010.ProductPictures.Add(new ProductPicture()
            {
                Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "0000953_dell-inspiron-one-23.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productComputerDellOptiplex3010.Name), true, false),
                DisplayOrder = 1,
            });
            productComputerDellOptiplex3010.ProductPictures.Add(new ProductPicture()
            {
                Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "0000954_dell-inspiron-one-23.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productComputerDellOptiplex3010.Name), true, false),
                DisplayOrder = 2,
            });
            productComputerDellOptiplex3010.ProductPictures.Add(new ProductPicture()
            {
                Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "0000956_dell-inspiron-one-23.jpeg"), "image/jpeg", pictureService.GetPictureSeName(productComputerDellOptiplex3010.Name), true, false),
                DisplayOrder = 3,
            });

            #endregion pictures

            #region manufacturer

            //manufacturer
            productComputerDellOptiplex3010.ProductManufacturers.Add(new ProductManufacturer()
            {
                Manufacturer = this._manufacturerRepository.Table.Where(c => c.Name == "Dell").Single(),
                DisplayOrder = 1,
            });

            #endregion manufacturer

            #region SpecificationAttributes
            //attributes
            productComputerDellOptiplex3010.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 1,
                // CPU -> Intel
                SpecificationAttributeOption = _specificationAttributesRepository.Table.Where(sa => sa.DisplayOrder == 1).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 2).Single()
            });
            productComputerDellOptiplex3010.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 2,
                // RAM -> 4 GB 
                SpecificationAttributeOption = _specificationAttributesRepository.Table.Where(sa => sa.DisplayOrder == 4).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 2).Single()
            });
            productComputerDellOptiplex3010.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 3,
                // Harddisk-Typ / HDD
                SpecificationAttributeOption = _specificationAttributesRepository.Table.Where(sa => sa.DisplayOrder == 16).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 2).Single()
            });
            productComputerDellOptiplex3010.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 4,
                // Harddisk-Capacity / 750 GB
                SpecificationAttributeOption = _specificationAttributesRepository.Table.Where(sa => sa.DisplayOrder == 3).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 2).Single()
            });
            productComputerDellOptiplex3010.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 5,
                // OS / Windows 7 32 Bit
                SpecificationAttributeOption = _specificationAttributesRepository.Table.Where(sa => sa.DisplayOrder == 5).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 4).Single()
            });
            #endregion SpecificationAttributes

            #endregion productComputerDellOptiplex3010

            #region productComputerAcerAspireOne
            var productComputerAcerAspireOne = new Product()
            {
                Name = "Acer Aspire One 8.9\" Mini-Notebook Case - (Black)",
                ShortDescription = "Acer Aspire One 8.9\" Mini-Notebook and 6 Cell Battery model (AOA150-1447)",
                FullDescription = "<p>Acer Aspire One 8.9&quot; Memory Foam Pouch is the perfect fit for Acer Aspire One 8.9&quot;. This pouch is made out of premium quality shock absorbing memory form and it provides extra protection even though case is very light and slim. This pouch is water resistant and has internal supporting bands for Acer Aspire One 8.9&quot;. Made In Korea.</p>",
                ProductTemplateId = productTemplateSingleVariant.Id,
                //SeName = "acer-aspire-one-89-mini-notebook-case-black",
                AllowCustomerReviews = true,
                Published = true,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                MetaTitle = "Acer Aspire One 8.9",
                ShowOnHomePage = true,
            };

            #region productTags
            
            #endregion productTags

            #region variants
            productComputerAcerAspireOne.ProductVariants.Add(new ProductVariant()
            {
                Price = 210.6M,
                IsShipEnabled = true,
                Weight = 2,
                Length = 2,
                Width = 2,
                Height = 3,
                //TaxCategoryId = _taxCategoryRepository.Table.Where(tc => tc.Name == "Electronics & Software").Single().Id,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                DisplayStockAvailability = true,
                LowStockActivity = LowStockActivity.DisableBuyButton,
                BackorderMode = BackorderMode.NoBackorders,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                Published = true,
                DisplayOrder = 1,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                DeliveryTime = _deliveryTimeRepository.Table.Where(sa => sa.DisplayOrder == 0).Single()
            });
            #endregion variants

            #region manufacturer

            //manufacturer
            productComputerAcerAspireOne.ProductManufacturers.Add(new ProductManufacturer()
            {
                Manufacturer = this._manufacturerRepository.Table.Where(c => c.Name == "Acer").Single(),
                DisplayOrder = 1,
            });

            #endregion manufacturer

            #region tierPrieces
            productComputerAcerAspireOne.ProductVariants.FirstOrDefault().TierPrices.Add(new TierPrice()
            {
                Quantity = 2,
                Price = 205
            });
            productComputerAcerAspireOne.ProductVariants.FirstOrDefault().TierPrices.Add(new TierPrice()
            {
                Quantity = 5,
                Price = 189
            });
            productComputerAcerAspireOne.ProductVariants.FirstOrDefault().TierPrices.Add(new TierPrice()
            {
                Quantity = 10,
                Price = 155
            });
            productComputerAcerAspireOne.ProductVariants.FirstOrDefault().HasTierPrices = true;
            #endregion tierPrieces

            //productComputerAcerAspireOne.ProductCategories.Add(new ProductCategory()
            //{
            //    Category = _categoryRepository.Table.Where(c => c.Name == "Notebooks").Single(),
            //    DisplayOrder = 1,
            //});

            #region pictures
            productComputerAcerAspireOne.ProductPictures.Add(new ProductPicture()
            {
                Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "acer-aspire-one.jpg"), "image/jpeg", pictureService.GetPictureSeName(productComputerAcerAspireOne.Name), true, false),
                DisplayOrder = 1,
            });
            productComputerAcerAspireOne.ProductPictures.Add(new ProductPicture()
            {
                Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "01-12Hand_Aspire1.jpg"), "image/jpeg", pictureService.GetPictureSeName(productComputerAcerAspireOne.Name), true, false),
                DisplayOrder = 2,
            });
            productComputerAcerAspireOne.ProductPictures.Add(new ProductPicture()
            {
                Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "58_00007561.jpg"), "image/jpeg", pictureService.GetPictureSeName(productComputerAcerAspireOne.Name), true, false),
                DisplayOrder = 3,
            });
            
            #endregion tierPrieces

            #endregion productComputerAcerAspireOne

            #endregion computer

            #region Smartphones

            #region productSmartPhonesAppleIphone5

            var productSmartPhonesAppleIphone5 = new Product()
            {
                Name = "Apple iPhone 5 32 GB",
                ShortDescription = "The biggest thing to happen to iPhone since iPhone.",
                FullDescription = "<p> The iPhone 5 at a glance. New design. </p> <p> With 7.6 mm and 112 g3 the iPhone 5 has a remarkably thin and light design. It is made of anodized aluminum. The beveled edges are precision cut with a diamond. <br> Brilliant 4 \"Retina Display.<br> Now you can see everything more vivid and detailed. And although the screen is bigger, it has the same width as the iPhone 4S and is therefore just as easily operated with one hand. Powerful A6 chip. Compared with the A5 chip, he has up to twice the CPU and graphics performance. And despite its speed, the iPhone 5 has a fantastic battery life. Ultra-fast mobile data. The iPhone 5 supports the latest wireless technologies and can be even more so with the world verbinden.4Und networks and Wi-Fi is faster. </p> <p> Delivery 6 Apple iPhone 5 with iOS EarPods with Remote and Mic to USB Cable Lightning USB Power Adapter (Power Supply) Choose the iPhone that suits you. 16 GB, 32 GB or 64 GB capacity? The iPhone is available in three capacities: 16, 32 and 64 GB. \"GB\" stands for gigabytes. The more gigabytes your iPhone, the more space you have. For content such as apps, games, photos, HD videos, music, movies and more In a large music or photo library, or if you frequently invite HD movies to an iPhone recommends using a larger capacity. If your music or photo library is small or you do or buy rare films, ranging from an iPhone with a smaller memory size for you. With time comes along Some of content on your iPhone. Therefore, you should consider your growing storage needs of the iPhone purchase. The unlocked iPhone The unlocked iPhone is not dependent on any cellular network, which means that you can decide for any supported GSM network worldwide. Buy an iPhone without a contract at the Apple Online Store, then look for a tariff for the iPhone, which covers only the SIM card. Or contact your current provider if you want to keep your current rate. </p> <p> PrePaid<br> If you want a contract, you can choose one of the \"prepaid\" starter packs of available network operators in the Apple Online Store. With \"prepaid\" you get your iPhone and ready. Compatible with a SIM card A credit check is not required. You pay online just for what you use, and can switch the network provider at any time. \"Prepaid\" is a good option if you want to use when traveling abroad prefer a local supplier. Which SIM card do I need? It does not matter if you are a new contract, further Lead your existing contract or \"prepaid\" choose - you will need a compatible SIM card. For the iPhone 5, it takes a nano-SIM card for the iPhone 4S and the iPhone 4 is a micro-SIM card. For more details you get with your wireless carrier. Cellular networks and Wi-Fi </p> <p> A1428 GSM model: UMTS / HSPA + / DC-HSDPA (850, 900, 1900, 2100 MHz) GSM / EDGE (850, 900, 1800, 1900 MHz), LTE (Band 4 and 17) <br> Model A1429 * CDMA: CDMA EV-DO Rev. A and Rev. B (800, 1900, 2100 MHz), UMTS / HSPA + / DC-HSDPA (850, 900, 1900, 2100 MHz) GSM / EDGE (850, 900 , 1800, 1900 MHz), LTE band (1, 3, 5, 13, 25) <br> * A1429 GSM model: UMTS / HSPA + / DC-HSDPA (850, 900, 1900, 2100 MHz) GSM / EDGE (850, 900, 1800, 1900 MHz), LTE (Volume 1, 3, 5) <br> 802.11a/b/g/n Wi-Fi (802.11n 2.4 GHz and 5 GHz) <br> Bluetooth 4.0 <br> Display Retina Display 4 \"widescreen Multi-Touch display (10.16 cm diagonal) 1136 x 640 pixels at 326 ppi Typical contrast ratio: 500 cd/m2 simultaneously resistant oleophobic coating on the front support for display of multiple languages ​​and characters: 800:1 Maximum brightness typical Camera, photos and videos 8-megapixel iSight camera Panorama Video recording, HD (1080p) up to 30 frames per second with audio FaceTime HD camera with 1.2-megapixel still images and HD video (720p) for up to 30 frames per second Autofocus Tap to focus video photos and facial recognition in video and photos LED flash Improved video stabilization for geotagging photos and videos Audio Support for AirPlay Mirroring to Apple TV with 720p AirPlay video streaming to Apple TV (3rd generation) up to 1080p and Apple TV (2nd generation) up to 720p Video formats supported: H.264 video up to 1080p, 30 frames per second, High Profile level 4.1 with AAC-LC audio up to 160 kbit / s, 48kHz, stereo audio in m4v, mp4 and mov... , MPEG-4 video, up to 2.5 Mbit / s, 640 x 480 pixels, 30 frames per second, Simple Profile with AAC-LC audio up to 160 Kbps m4v / s, 48 ​​kHz, stereo audio in. , mp4 and mov file formats Motion JPEG (M-JPEG)... up to 35 Mbit / s, 1280 x 720 pixels, 30 frames per second, audio in ulaw, PCM stereo audio in avi format </p>",
                ProductTemplateId = productTemplateSingleVariant.Id,
                AllowCustomerReviews = true,
                Published = true,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                MetaTitle = "Apple iPhone 5 32 GB",
                ShowOnHomePage = true
                
            };

            #region variants
            productSmartPhonesAppleIphone5.ProductVariants.Add(new ProductVariant()
            {
                Price = 579.00M,
                //TaxCategoryId = _taxCategoryRepository.Table.Where(tc => tc.Name == "Electronics & Software").Single().Id,
                //DeliveryTime = _deliveryTimeRepository.Table
                ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                Published = true,
                DisplayOrder = 1,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                IsShipEnabled = true,
                DeliveryTime = _deliveryTimeRepository.Table.Where(sa => sa.DisplayOrder == 2).Single()
            });
            #endregion variants

            #region categories
            //categories
            //productComputerDellInspiron23.ProductCategories.Add(new ProductCategory()
            //{
            //    Category = this._categoryRepository.Table.Where(c => c.Name == "Desktops").Single(),
            //    DisplayOrder = 1,
            //});
            #endregion categories

            #region pictures

            //pictures
            productSmartPhonesAppleIphone5.ProductPictures.Add(new ProductPicture()
            {
                Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "0000789-apple-iphone-5-32-gb.jpg"), "image/jpeg", pictureService.GetPictureSeName(productSmartPhonesAppleIphone5.Name), true, false),
                DisplayOrder = 1,
            });
            productSmartPhonesAppleIphone5.ProductPictures.Add(new ProductPicture()
            {
                Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "0000785-apple-iphone-5-32-gb.jpg"), "image/jpeg", pictureService.GetPictureSeName(productSmartPhonesAppleIphone5.Name), true, false),
                DisplayOrder = 2,
            });
            productSmartPhonesAppleIphone5.ProductPictures.Add(new ProductPicture()
            {
                Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "0000786-apple-iphone-5-32-gb.jpg"), "image/jpeg", pictureService.GetPictureSeName(productSmartPhonesAppleIphone5.Name), true, false),
                DisplayOrder = 3,
            });
            productSmartPhonesAppleIphone5.ProductPictures.Add(new ProductPicture()
            {
                Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "0000787-apple-iphone-5-32-gb.jpg"), "image/jpeg", pictureService.GetPictureSeName(productSmartPhonesAppleIphone5.Name), true, false),
                DisplayOrder = 4,
            });
            productSmartPhonesAppleIphone5.ProductPictures.Add(new ProductPicture()
            {
                Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "0000788-apple-iphone-5-32-gb.jpg"), "image/jpeg", pictureService.GetPictureSeName(productSmartPhonesAppleIphone5.Name), true, false),
                DisplayOrder = 4,
            });
            
            

            #endregion pictures

            #region manufacturer

            //manufacturer
            productSmartPhonesAppleIphone5.ProductManufacturers.Add(new ProductManufacturer()
            {
                Manufacturer = this._manufacturerRepository.Table.Where(c => c.Name == "Apple").Single(),
                DisplayOrder = 1,
            });

            #endregion manufacturer

            #region SpecificationAttributes
            //attributes
            productSmartPhonesAppleIphone5.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 1,
                // housing > alu
                SpecificationAttributeOption = _specificationAttributesRepository.Table.Where(sa => sa.DisplayOrder == 8).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 4).Single()
            });
            productSmartPhonesAppleIphone5.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 2,
                // manufacturer > apple
                SpecificationAttributeOption = _specificationAttributesRepository.Table.Where(sa => sa.DisplayOrder == 20).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 1).Single()
            });
            productSmartPhonesAppleIphone5.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 5,
                // OS / Windows 7 32 Bit
                SpecificationAttributeOption = _specificationAttributesRepository.Table.Where(sa => sa.DisplayOrder == 5).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 9).Single()
            });
            #endregion SpecificationAttributes

            #region ProductReviews
            //productSmartPhonesAppleIphone5.ProductReviews.Add(new ProductReview()
            //{
            //    Rating = 5,
            //    Title = "Best smartphone in the world!!!",
            //    IsApproved = true,
            //    ReviewText = "<p>this is really the best phone on the planet. i love it!</p>"
            //});

            #endregion ProductReviews
            #endregion productSmartPhonesAppleIphone5


            #endregion Smartphones

            #region Instant Download Music

            #region Antonio Vivaldi: then spring
            var productInstantDownloadVivaldi = new Product()
            {
                Name = "Antonio Vivaldi: spring",
                ShortDescription = "MP3, 320 kbit/s",
                FullDescription = "<p>Antonio Vivaldi: Spring</p> <p>Antonio Lucio Vivaldi (March 4, 1678 in Venice, &dagger; 28 July 1741 in Vienna) was a Venetian composer and violinist in the Baroque.</p> <p>The Four Seasons (Le quattro stagioni Italian) is perhaps the most famous works of Antonio Vivaldi. It's four violin concertos with extra-musical programs, each portraying a concert season. This is the individual concerts one - probably written by Vivaldi himself - Sonnet preceded by consecutive letters in front of the lines and in the appropriate places in the score arrange the verbal description of the music.</p> <p>Vivaldi had previously always been experimenting with non-musical programs, which often reflected in his tracks, the exact interpretation of the individual points score is unusual for him. His experience as a virtuoso violinist allowed him access to particularly effective playing techniques, as an opera composer, he had developed a strong sense of effects, both of which benefitted from him.</p> <p>As the title suggests, especially to imitate natural phenomena - gentle winds, severe storms and thunderstorms are elements that are common to all four concerts. There are also various birds and even a dog, further human activities such as hunting, a barn dance, ice skating, including stumbling and falling to the heavy sleep of a drunkard.</p> <p>The work dates from 1725 and is available in two print editions, which appeared more or less simultaneously published in Amsterdam and Paris.</p>",
                ProductTemplateId = productTemplateSingleVariant.Id,
                AllowCustomerReviews = true,
                Published = true,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                MetaTitle = "Antonio Vivaldi: spring",
                ShowOnHomePage = true
            };

            #region download
            var downloadVivaldi = new Download()
            {
                DownloadGuid = Guid.NewGuid(),
                ContentType = "audio/mp3",
                DownloadBinary = File.ReadAllBytes(sampleDownloadsPath + "vivaldi-four-seasons-spring.mp3"),
                Extension = ".mp3",
                Filename = "vivaldi-four-seasons-spring",
                IsNew = true,
            };
            _downloadService.InsertDownload(downloadVivaldi);
            #endregion download

            #region variants
            productInstantDownloadVivaldi.ProductVariants.Add(new ProductVariant()
            {
                Price = 1.99M,
                //TaxCategoryId = _taxCategoryRepository.Table.Where(tc => tc.Name == "Electronics & Software").Single().Id,
                //DeliveryTime = _deliveryTimeRepository.Table
                ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                Published = true,
                DisplayOrder = 1,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                IsDownload = true,
                HasSampleDownload = true,
                SampleDownloadId = downloadVivaldi.Id,
            });
            #endregion variants

            #region categories
            //categories
            //productComputerDellInspiron23.ProductCategories.Add(new ProductCategory()
            //{
            //    Category = this._categoryRepository.Table.Where(c => c.Name == "Desktops").Single(),
            //    DisplayOrder = 1,
            //});
            #endregion categories

            #region pictures

            //pictures
            productInstantDownloadVivaldi.ProductPictures.Add(new ProductPicture()
            {
                Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "0000740-antonio-vivaldi-der-fruhling-100.jpg"), "image/jpeg", pictureService.GetPictureSeName(productInstantDownloadVivaldi.Name), true, false),
                DisplayOrder = 1,
            });
            
            #endregion pictures

            #region manufacturer

            //manufacturer
            //productInstantDownloadVivaldi.ProductManufacturers.Add(new ProductManufacturer()
            //{
            //    Manufacturer = this._manufacturerRepository.Table.Where(c => c.Name == "Apple").Single(),
            //    DisplayOrder = 1,
            //});

            #endregion manufacturer

            #region SpecificationAttributes
            //attributes
            productInstantDownloadVivaldi.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 1,
                // mp3 quality > 320 kbit/S
                SpecificationAttributeOption = _specificationAttributesRepository.Table.Where(sa => sa.DisplayOrder == 18).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 3).Single()
            });
            productInstantDownloadVivaldi.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 2,
                // genre > classic
                SpecificationAttributeOption = _specificationAttributesRepository.Table.Where(sa => sa.DisplayOrder == 19).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 6).Single()
            });
            
            #endregion SpecificationAttributes

            #endregion Antonio Vivildi: then spring


            #region Beethoven für Elise
            var productInstantDownloadBeethoven = new Product()
            {
                Name = "Ludwig van Beethoven: For Elise",
                ShortDescription = "Ludwig van Beethoven's most popular compositions",
                FullDescription = "<p> The score was not published until 1867, 40 years after the composer's death in 1827. The discoverer of the piece, Ludwig Nohl, affirmed that the original autographed manuscript, now lost, was dated 27 April 1810.[4] The version of \"Für Elise\" we hear today is an earlier version that was transcribed by Ludwig Nohl. There is a later version, with drastic changes to the accompaniment which was transcribed from a later manuscript by Barry Cooper. The most notable difference is in the first theme, the left-hand arpeggios are delayed by a 16th note beat. There are a few extra bars in the transitional section into the B section; and finally, the rising A minor arpeggio figure is moved later into the piece. The tempo marking Poco Moto is believed to have been on the manuscript that Ludwig Nohl transcribed (now lost). The later version includes the marking Molto Grazioso. It is believed that Beethoven intended to add the piece to a cycle of bagatelles.[citation needed] </p> <p> Therese Malfatti, widely believed to be the dedicatee of \"Für Elise\" The pianist and musicologist Luca Chiantore (es) argued in his thesis and his 2010 book Beethoven al piano that Beethoven might not have been the person who gave the piece the form that we know today. Chiantore suggested that the original signed manuscript, upon which Ludwig Nohl claimed to base his transcription, may never have existed.[5] On the other hand, the musicologist Barry Cooper stated, in a 1984 essay in The Musical Times, that one of two surviving sketches closely resembles the published version.[6] </p>",
                ProductTemplateId = productTemplateSingleVariant.Id,
                AllowCustomerReviews = true,
                Published = true,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                MetaTitle = "Ludwig van Beethoven: Für Elise",
                ShowOnHomePage = true
            };

            #region download
            var downloadBeethoven = new Download()
            {
                DownloadGuid = Guid.NewGuid(),
                ContentType = "audio/mp3",
                DownloadBinary = File.ReadAllBytes(sampleDownloadsPath + "beethoven-fur-elise.mp3"),
                Extension = ".mp3",
                Filename = "beethoven-fur-elise.mp3",
                IsNew = true,
            };
            _downloadService.InsertDownload(downloadBeethoven);
            #endregion download

            #region variants
            productInstantDownloadBeethoven.ProductVariants.Add(new ProductVariant()
            {
                Price = 1.89M,
                //TaxCategoryId = _taxCategoryRepository.Table.Where(tc => tc.Name == "Electronics & Software").Single().Id,
                //DeliveryTime = _deliveryTimeRepository.Table
                ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                Published = true,
                DisplayOrder = 1,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                IsDownload = true,
                HasSampleDownload = true,
                SampleDownloadId = downloadVivaldi.Id,
            });
            #endregion variants

            #region categories
            //categories
            //productComputerDellInspiron23.ProductCategories.Add(new ProductCategory()
            //{
            //    Category = this._categoryRepository.Table.Where(c => c.Name == "Desktops").Single(),
            //    DisplayOrder = 1,
            //});
            #endregion categories

            #region pictures

            //pictures
            productInstantDownloadBeethoven.ProductPictures.Add(new ProductPicture()
            {
                Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "Beethoven.jpg"), "image/jpeg", pictureService.GetPictureSeName(productInstantDownloadBeethoven.Name), true, false),
                DisplayOrder = 1,
            });

            #endregion pictures

            #region SpecificationAttributes
            //attributes
            productInstantDownloadBeethoven.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 1,
                // mp3 quality > 320 kbit/S
                SpecificationAttributeOption = _specificationAttributesRepository.Table.Where(sa => sa.DisplayOrder == 18).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 3).Single()
            });
            productInstantDownloadBeethoven.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 2,
                // genre > classic
                SpecificationAttributeOption = _specificationAttributesRepository.Table.Where(sa => sa.DisplayOrder == 19).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 6).Single()
            });

            #endregion SpecificationAttributes

            #endregion Beethoven für Elise
            #endregion Instant Download Music

            #region watches

            #region productWatchesCertinaDSPodiumBigSize

            var productWatchesCertinaDSPodiumBigSize  = new Product()
            {
                Name = "Certina DS Podium Big Size ",
                ShortDescription = "C001.617.26.037.00",
                FullDescription = "<p> The iPhone 5 at a glance. New design. </p> <p> With 7.6 mm and 112 g3 the iPhone 5 has a remarkably thin and light design. It is made of anodized aluminum. The beveled edges are precision cut with a diamond. <br> Brilliant 4 \"Retina Display.<br> Now you can see everything more vivid and detailed. And although the screen is bigger, it has the same width as the iPhone 4S and is therefore just as easily operated with one hand. Powerful A6 chip. Compared with the A5 chip, he has up to twice the CPU and graphics performance. And despite its speed, the iPhone 5 has a fantastic battery life. Ultra-fast mobile data. The iPhone 5 supports the latest wireless technologies and can be even more so with the world verbinden.4Und networks and Wi-Fi is faster. </p> <p> Delivery 6 Apple iPhone 5 with iOS EarPods with Remote and Mic to USB Cable Lightning USB Power Adapter (Power Supply) Choose the iPhone that suits you. 16 GB, 32 GB or 64 GB capacity? The iPhone is available in three capacities: 16, 32 and 64 GB. \"GB\" stands for gigabytes. The more gigabytes your iPhone, the more space you have. For content such as apps, games, photos, HD videos, music, movies and more In a large music or photo library, or if you frequently invite HD movies to an iPhone recommends using a larger capacity. If your music or photo library is small or you do or buy rare films, ranging from an iPhone with a smaller memory size for you. With time comes along Some of content on your iPhone. Therefore, you should consider your growing storage needs of the iPhone purchase. The unlocked iPhone The unlocked iPhone is not dependent on any cellular network, which means that you can decide for any supported GSM network worldwide. Buy an iPhone without a contract at the Apple Online Store, then look for a tariff for the iPhone, which covers only the SIM card. Or contact your current provider if you want to keep your current rate. </p> <p> PrePaid<br> If you want a contract, you can choose one of the \"prepaid\" starter packs of available network operators in the Apple Online Store. With \"prepaid\" you get your iPhone and ready. Compatible with a SIM card A credit check is not required. You pay online just for what you use, and can switch the network provider at any time. \"Prepaid\" is a good option if you want to use when traveling abroad prefer a local supplier. Which SIM card do I need? It does not matter if you are a new contract, further Lead your existing contract or \"prepaid\" choose - you will need a compatible SIM card. For the iPhone 5, it takes a nano-SIM card for the iPhone 4S and the iPhone 4 is a micro-SIM card. For more details you get with your wireless carrier. Cellular networks and Wi-Fi </p> <p> A1428 GSM model: UMTS / HSPA + / DC-HSDPA (850, 900, 1900, 2100 MHz) GSM / EDGE (850, 900, 1800, 1900 MHz), LTE (Band 4 and 17) <br> Model A1429 * CDMA: CDMA EV-DO Rev. A and Rev. B (800, 1900, 2100 MHz), UMTS / HSPA + / DC-HSDPA (850, 900, 1900, 2100 MHz) GSM / EDGE (850, 900 , 1800, 1900 MHz), LTE band (1, 3, 5, 13, 25) <br> * A1429 GSM model: UMTS / HSPA + / DC-HSDPA (850, 900, 1900, 2100 MHz) GSM / EDGE (850, 900, 1800, 1900 MHz), LTE (Volume 1, 3, 5) <br> 802.11a/b/g/n Wi-Fi (802.11n 2.4 GHz and 5 GHz) <br> Bluetooth 4.0 <br> Display Retina Display 4 \"widescreen Multi-Touch display (10.16 cm diagonal) 1136 x 640 pixels at 326 ppi Typical contrast ratio: 500 cd/m2 simultaneously resistant oleophobic coating on the front support for display of multiple languages ​​and characters: 800:1 Maximum brightness typical Camera, photos and videos 8-megapixel iSight camera Panorama Video recording, HD (1080p) up to 30 frames per second with audio FaceTime HD camera with 1.2-megapixel still images and HD video (720p) for up to 30 frames per second Autofocus Tap to focus video photos and facial recognition in video and photos LED flash Improved video stabilization for geotagging photos and videos Audio Support for AirPlay Mirroring to Apple TV with 720p AirPlay video streaming to Apple TV (3rd generation) up to 1080p and Apple TV (2nd generation) up to 720p Video formats supported: H.264 video up to 1080p, 30 frames per second, High Profile level 4.1 with AAC-LC audio up to 160 kbit / s, 48kHz, stereo audio in m4v, mp4 and mov... , MPEG-4 video, up to 2.5 Mbit / s, 640 x 480 pixels, 30 frames per second, Simple Profile with AAC-LC audio up to 160 Kbps m4v / s, 48 ​​kHz, stereo audio in. , mp4 and mov file formats Motion JPEG (M-JPEG)... up to 35 Mbit / s, 1280 x 720 pixels, 30 frames per second, audio in ulaw, PCM stereo audio in avi format </p>",
                ProductTemplateId = productTemplateSingleVariant.Id,
                AllowCustomerReviews = true,
                Published = true,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                MetaTitle = "Certina DS Podium Big Size",
                ShowOnHomePage = true

            };

            #region variants
            productWatchesCertinaDSPodiumBigSize.ProductVariants.Add(new ProductVariant()
            {
                Price = 479.00M,
                //TaxCategoryId = _taxCategoryRepository.Table.Where(tc => tc.Name == "Electronics & Software").Single().Id,
                //DeliveryTime = _deliveryTimeRepository.Table
                ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                Published = true,
                DisplayOrder = 1,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                IsShipEnabled = true,
                DeliveryTime = _deliveryTimeRepository.Table.Where(sa => sa.DisplayOrder == 2).Single()
            });
            #endregion variants

            #region categories
            //categories
            //productComputerDellInspiron23.ProductCategories.Add(new ProductCategory()
            //{
            //    Category = this._categoryRepository.Table.Where(c => c.Name == "Desktops").Single(),
            //    DisplayOrder = 1,
            //});
            #endregion categories

            #region pictures

            //pictures
            productWatchesCertinaDSPodiumBigSize.ProductPictures.Add(new ProductPicture()
            {
                Picture = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "0001424-certina-ds-podium-big-size-herrenchronograph.jpg"), "image/jpeg", pictureService.GetPictureSeName(productWatchesCertinaDSPodiumBigSize.Name), true, false),
                DisplayOrder = 1,
            });
            
            #endregion pictures

            #region manufacturer

            //manufacturer
            productWatchesCertinaDSPodiumBigSize.ProductManufacturers.Add(new ProductManufacturer()
            {
                Manufacturer = this._manufacturerRepository.Table.Where(c => c.Name == "Certina").Single(),
                DisplayOrder = 1,
            });

            #endregion manufacturer

            #region SpecificationAttributes
            //attributes
            productWatchesCertinaDSPodiumBigSize.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 1,
                // housing > steel
                SpecificationAttributeOption = _specificationAttributesRepository.Table.Where(sa => sa.DisplayOrder == 8).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 1).Single()
            });
            productWatchesCertinaDSPodiumBigSize.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 2,
                // manufacturer > Certina
                SpecificationAttributeOption = _specificationAttributesRepository.Table.Where(sa => sa.DisplayOrder == 20).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 14).Single()
            });
            productWatchesCertinaDSPodiumBigSize.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 5,
                // Gender -> gentlemen
                SpecificationAttributeOption = _specificationAttributesRepository.Table.Where(sa => sa.DisplayOrder == 7).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 1).Single()
            });
            productWatchesCertinaDSPodiumBigSize.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 5,
                // movement -> quarz
                SpecificationAttributeOption = _specificationAttributesRepository.Table.Where(sa => sa.DisplayOrder == 9).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 3).Single()
            });
            #endregion SpecificationAttributes

            #region ProductReviews
            //productSmartPhonesAppleIphone5.ProductReviews.Add(new ProductReview()
            //{
            //    Rating = 5,
            //    Title = "Best smartphone in the world!!!",
            //    IsApproved = true,
            //    ReviewText = "<p>this is really the best phone on the planet. i love it!</p>"
            //});

            #endregion ProductReviews
            #endregion productWatchesCertinaDSPodiumBigSize

            #endregion watches


            

            var entities = new List<Product>
            {
                product5GiftCard, product25GiftCard, product50GiftCard, productBooksUberMan, productBooksGefangeneDesHimmels,
                productBooksBestGrillingRecipes, productBooksCookingForTwo, productBooksAutosDerSuperlative,  productBooksBildatlasMotorraeder,productBooksAutoBuch ,productBooksFastCars,
                productBooksMotorradAbenteuer,  productComputerDellInspiron23, productComputerDellOptiplex3010,productSmartPhonesAppleIphone5, 
                productInstantDownloadVivaldi, productComputerAcerAspireOne,productInstantDownloadBeethoven, productWatchesCertinaDSPodiumBigSize
            };

            this.Alter(entities);
            return entities;
        }


        #region ForumGroups
        public IList<ForumGroup> ForumGroups()
        {
            var forumGroupGeneral = new ForumGroup()
            {
                Name = "General",
                Description = "",
                DisplayOrder = 1,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
            };

            var entities = new List<ForumGroup>
            {
                forumGroupGeneral
            };

            this.Alter(entities);
            return entities;
        }
        #endregion ForumGroups

        #region Forums
        public IList<Forum> Forums()
        {
            var newProductsForum = new Forum()
            {
                ForumGroup = _forumGroupRepository.Table.Where(c => c.DisplayOrder == 1).Single(),
                Name = "New Products",
                Description = "Discuss new products and industry trends",
                NumTopics = 0,
                NumPosts = 0,
                LastPostCustomerId = 0,
                LastPostTime = null,
                DisplayOrder = 1,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
            };

            var packagingShippingForum = new Forum()
            {
                ForumGroup = _forumGroupRepository.Table.Where(c => c.DisplayOrder == 1).Single(),
                Name = "Packaging & Shipping",
                Description = "Discuss packaging & shipping",
                NumTopics = 0,
                NumPosts = 0,
                LastPostTime = null,
                DisplayOrder = 20,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
            };


            var entities = new List<Forum>
            {
                newProductsForum, packagingShippingForum
            };

            this.Alter(entities);
            return entities;
        }
        #endregion Forums

        #region Discounts
        public IList<Discount> Discounts()
        {
            var sampleDiscountWithCouponCode = new Discount()
                {
                    Name = "Sample discount with coupon code",
                    DiscountType = DiscountType.AssignedToSkus,
                    DiscountLimitation = DiscountLimitationType.Unlimited,
                    UsePercentage = false,
                    DiscountAmount = 10,
                    RequiresCouponCode = true,
                    CouponCode = "123",
                };
            var sampleDiscounTwentyPercentTotal = new Discount()
                {
                    Name = "'20% order total' discount",
                    DiscountType = DiscountType.AssignedToOrderTotal,
                    DiscountLimitation = DiscountLimitationType.Unlimited,
                    UsePercentage = true,
                    DiscountPercentage = 20,
                    StartDateUtc = new DateTime(2013, 1, 1),
                    EndDateUtc = new DateTime(2020, 1, 1),
                    RequiresCouponCode = true,
                    CouponCode = "456",
                };

            var entities = new List<Discount>
            {
                sampleDiscountWithCouponCode, sampleDiscounTwentyPercentTotal
            };

            this.Alter(entities);
            return entities;
        }    
        #endregion Discounts

        #region Deliverytimes
        public IList<DeliveryTime> DeliveryTimes()
        {
            var entities = new List<DeliveryTime>()
            {
                new DeliveryTime
                    {
                        Name = "available and ready to ship",
                        DisplayOrder = 0,
                        ColorHexValue = "#008000"
                    },
                new DeliveryTime
                    {
                        Name = "2-5 woking days",
                        DisplayOrder = 1,
                        ColorHexValue = "#FFFF00"
                    },
                new DeliveryTime
                    {
                        Name = "7 working days",
                        DisplayOrder = 2,
                        ColorHexValue = "#FF9900"
                    },
            };
            this.Alter(entities);
            return entities;
        }

        #endregion Deliverytimes


        #region BlogPost
        public IList<BlogPost> BlogPosts()
        {
            var defaultLanguage = _languageRepository.Table.FirstOrDefault();
            var blogPostDiscountCoupon = new BlogPost()
            {
                AllowComments = true,
                Language = defaultLanguage,
                Title = "Online Discount Coupons",
                Body = "<p>Online discount coupons enable access to great offers from some of the world&rsquo;s best sites for Internet shopping. The online coupons are designed to allow compulsive online shoppers to access massive discounts on a variety of products. The regular shopper accesses the coupons in bulk and avails of great festive offers and freebies thrown in from time to time.  The coupon code option is most commonly used when using a shopping cart. The coupon code is entered on the order page just before checking out. Every online shopping resource has a discount coupon submission option to confirm the coupon code. The dedicated web sites allow the shopper to check whether or not a discount is still applicable. If it is, the sites also enable the shopper to calculate the total cost after deducting the coupon amount like in the case of grocery coupons.  Online discount coupons are very convenient to use. They offer great deals and professionally negotiated rates if bought from special online coupon outlets. With a little research and at times, insider knowledge the online discount coupons are a real steal. They are designed to promote products by offering &lsquo;real value for money&rsquo; packages. The coupons are legitimate and help with budgeting, in the case of a compulsive shopper. They are available for special trade show promotions, nightlife, sporting events and dinner shows and just about anything that could be associated with the promotion of a product. The coupons enable the online shopper to optimize net access more effectively. Getting a &lsquo;big deal&rsquo; is not more utopian amidst rising prices. The online coupons offer internet access to the best and cheapest products displayed online. Big discounts are only a code away! By Gaynor Borade (buzzle.com)</p>",
                Tags = "e-commerce, money",
                CreatedOnUtc = DateTime.UtcNow,
            };
            var blogPostCustomerService = new BlogPost()
            {
                AllowComments = true,
                Language = defaultLanguage,
                Title = "Customer Service - Client Service",
                Body = "<p>Managing online business requires different skills and abilities than managing a business in the &lsquo;real world.&rsquo; Customers can easily detect the size and determine the prestige of a business when they have the ability to walk in and take a look around. Not only do &lsquo;real-world&rsquo; furnishings and location tell the customer what level of professionalism to expect, but &quot;real world&quot; personal encounters allow first impressions to be determined by how the business approaches its customer service. When a customer walks into a retail business just about anywhere in the world, that customer expects prompt and personal service, especially with regards to questions that they may have about products they wish to purchase.<br /><br />Customer service or the client service is the service provided to the customer for his satisfaction during and after the purchase. It is necessary to every business organization to understand the customer needs for value added service. So customer data collection is essential. For this, a good customer service is important. The easiest way to lose a client is because of the poor customer service. The importance of customer service changes by product, industry and customer. Client service is an important part of every business organization. Each organization is different in its attitude towards customer service. Customer service requires a superior quality service through a careful design and execution of a series of activities which include people, technology and processes. Good customer service starts with the design and communication between the company and the staff.<br /><br />In some ways, the lack of a physical business location allows the online business some leeway that their &lsquo;real world&rsquo; counterparts do not enjoy. Location is not important, furnishings are not an issue, and most of the visual first impression is made through the professional design of the business website.<br /><br />However, one thing still remains true. Customers will make their first impressions on the customer service they encounter. Unfortunately, in online business there is no opportunity for front- line staff to make a good impression. Every interaction the customer has with the website will be their primary means of making their first impression towards the business and its client service. Good customer service in any online business is a direct result of good website design and planning.</p><p>By Jayashree Pakhare (buzzle.com)</p>",
                Tags = "e-commerce, SmartStore.NET, asp.net, sample tag, money",
                CreatedOnUtc = DateTime.UtcNow.AddSeconds(1),
            };

            var entities = new List<BlogPost>
            {
                blogPostDiscountCoupon, blogPostCustomerService
            };

            this.Alter(entities);
            return entities;
        }
        #endregion BlogPost

        #region NewsItems
        public IList<NewsItem> NewsItems()
        {
            var defaultLanguage = _languageRepository.Table.FirstOrDefault();
            var news1 = new NewsItem()
            {
                AllowComments = true,
                Language = defaultLanguage,
                Title = "smartstore.net new release!",
                Short = "smartstore.net includes everything you need to begin your e-commerce online store.",
                Full = "<p>SmartStore.net includes everything you need to begin your e-commerce online store.<br/>  We have thought of everything and it's all included!<br/><br/>smartstore.net is a fully customizable shop-system. It's stable and highly usable.<br>  From downloads to documentation, www.smartstore.com offers a comprehensive base of information, resources, and support to the smartstore.net community.</p>",
                Published = true,
                MetaTitle = "smartstore.net new release!",
                CreatedOnUtc = DateTime.Now
            };
            var news2 = new NewsItem()
            {
                AllowComments = true,
                Language = defaultLanguage,
                Title = "New online store is open!",
                Short = "The new smartstore.net store is open now!  We are very excited to offer our new range of products. We will be constantly adding to our range so please register on our site, this will enable you to keep up to date with any new products.",
                Full = "<p>Our online store is officially up and running. Stock up for the holiday season!  We have a great selection of items. We will be constantly adding to our range so please register on our site,  this will enable you to keep up to date with any new products.</p><p>  All shipping is worldwide and will leave the same day an order is placed! Happy Shopping and spread the word!!</p>",
                Published = true,
                MetaTitle = "New online store is open!",
                //CreatedOnUtc = DateTime.UtcNow.AddSeconds(1),
                CreatedOnUtc = DateTime.Now
            };

            var entities = new List<NewsItem>
            {
                news1, news2
            };

            this.Alter(entities);
            return entities;
        }
        #endregion NewsItems

        #region PollAnswer
        public IList<PollAnswer> PollAnswers()
        {
            var pollAnswer1 = new PollAnswer()
            {
                Name = "Excellent",
                DisplayOrder = 1,
            };
            var pollAnswer2 = new PollAnswer()
            {
                Name = "Good",
                DisplayOrder = 2,
            };
            var pollAnswer3 = new PollAnswer()
            {
                Name = "Poor",
                DisplayOrder = 3,
            };
            var pollAnswer4 = new PollAnswer()
            {
                Name = "Very bad",
                DisplayOrder = 4,
            };
            var pollAnswer5 = new PollAnswer()
            {
                Name = "Daily",
                DisplayOrder = 5,
            };
            var pollAnswer6 = new PollAnswer()
            {
                Name = "Once a week",
                DisplayOrder = 6,
            };
            var pollAnswer7 = new PollAnswer()
            {
                Name = "Every two weeks",
                DisplayOrder = 7,
            };
            var pollAnswer8 = new PollAnswer()
            {
                Name = "Once a month",
                DisplayOrder = 8,
            };

            var entities = new List<PollAnswer>
            {
                pollAnswer1, pollAnswer2, pollAnswer3, pollAnswer4, pollAnswer5,  pollAnswer6,  pollAnswer7,  pollAnswer8
            };

            this.Alter(entities);
            return entities;
        }
        #endregion PollAnswer

        #region Polls
        public IList<Poll> Polls()
        {
            var defaultLanguage = _languageRepository.Table.FirstOrDefault();
            var poll1 = new Poll()
            {
                Language = defaultLanguage,
                Name = "How do you like the shop?",
                SystemKeyword = "RightColumnPoll",
                Published = true,
                DisplayOrder = 1,
            };

            poll1.PollAnswers.Add(new PollAnswer()
            {
                Name = "Excellent",
                DisplayOrder = 1,
            });

            poll1.PollAnswers.Add(new PollAnswer()
            {
                Name = "Good",
                DisplayOrder = 2,
            });

            poll1.PollAnswers.Add(new PollAnswer()
            {
                Name = "Poor",
                DisplayOrder = 3,
            });

            poll1.PollAnswers.Add(new PollAnswer()
            {
                Name = "Very bad",
                DisplayOrder = 4,
            });


            //_pollAnswerRepository.Table.Where(x => x.DisplayOrder < 5).Each(y =>
            //    {
            //        poll1.PollAnswers.Add(y);
            //    });

            var poll2 = new Poll()
            {
                Language = defaultLanguage,
                Name = "How often do you buy online?",
                SystemKeyword = "RightColumnPoll",
                Published = true,
                DisplayOrder = 2,
            };

            poll2.PollAnswers.Add(new PollAnswer()
            {
                Name = "Daily",
                DisplayOrder = 1,
            });

            poll2.PollAnswers.Add(new PollAnswer()
            {
                Name = "Once a week",
                DisplayOrder = 2,
            });

            poll2.PollAnswers.Add(new PollAnswer()
            {
                Name = "Every two weeks",
                DisplayOrder = 3,
            });

            poll2.PollAnswers.Add(new PollAnswer()
            {
                Name = "Once a month",
                DisplayOrder = 4,
            });

            //_pollAnswerRepository.Table.Where(x => x.DisplayOrder > 4).Each(y =>
            //{
            //    poll2.PollAnswers.Add(y);
            //});


            var entities = new List<Poll>
            {
                poll1, poll2
            };

            this.Alter(entities);
            return entities;
        }
        #endregion Polls


        #region Alterations

        protected virtual void Alter(IList<MeasureDimension> entities)
        {
        }

        protected virtual void Alter(IList<MeasureWeight> entities)
        {
        }

        protected virtual void Alter(IList<TaxCategory> entities)
        {
        }

        protected virtual void Alter(IList<Currency> entities)
        {
        }

        protected virtual void Alter(IList<Country> entities)
        {
        }

        protected virtual void Alter(IList<ShippingMethod> entities)
        {
        }

        protected virtual void Alter(IList<CustomerRole> entities)
        {
        }

        protected virtual void Alter(Address entity)
        {
        }

        protected virtual void Alter(Customer entity)
        {
        }

        protected virtual void Alter(IList<DeliveryTime> entities)
        {
        }
        
        protected virtual void Alter(IList<EmailAccount> entities)
        {
        }

        protected virtual void Alter(IList<MessageTemplate> entities)
        {
        }

        protected virtual void Alter(IList<Topic> entities)
        {
        }

		protected virtual void Alter(IList<Store> entities)
		{
		}

        protected virtual void Alter(IList<ISettings> settings)
        {
        }

        protected virtual void Alter(IList<StoreInformationSettings> settings)
        {
        }

        protected virtual void Alter(IList<OrderSettings> settings)
        {
        }

        protected virtual void Alter(IList<MeasureSettings> settings)
        {
        }

        protected virtual void Alter(IList<ShippingSettings> settings)
        {
        }

        protected virtual void Alter(IList<PaymentSettings> settings)
        {
        }

        protected virtual void Alter(IList<TaxSettings> settings)
        {
        }

        protected virtual void Alter(IList<EmailAccountSettings> settings)
        {
        }

        protected virtual void Alter(IList<ActivityLogType> entities)
        {
        }

        protected virtual void Alter(IList<ProductTemplate> entities)
        {
        }

        protected virtual void Alter(IList<CategoryTemplate> entities)
        {
        }

        protected virtual void Alter(IList<ManufacturerTemplate> entities)
        {
        }

        protected virtual void Alter(IList<ScheduleTask> entities)
        {
        }

        protected virtual void Alter(IList<SpecificationAttribute> entities)
        {
        }

        protected virtual void Alter(IList<ProductAttribute> entities)
        {
        }

        protected virtual void Alter(IList<Category> entities)
        {
        }

        protected virtual void Alter(IList<Manufacturer> entities)
        {
        }

        protected virtual void Alter(IList<Product> entities)
        {
        }

        protected virtual void Alter(IList<ForumGroup> entities)
        { 
        }

        protected virtual void Alter(IList<Forum> entities)
        {
        }

        protected virtual void Alter(IList<Discount> entities)
        { 
        }

        protected virtual void Alter(IList<BlogPost> entities)
        {
        }

        protected virtual void Alter(IList<NewsItem> entities)
        {
        }

        protected virtual void Alter(IList<Poll> entities)
        { 
        }

        protected virtual void Alter(IList<PollAnswer> entities)
        {
        }

        protected virtual void Alter(IList<ProductTag> entities)
        { 
        }
        //protected virtual void Alter(IList<Country> entities)
        //{
        //}

        #endregion Alterations

        #endregion Sample data creators
    }
}