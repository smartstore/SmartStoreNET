using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using SmartStore.Core;
using SmartStore.Core.Configuration;
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
using SmartStore.Data;
using SmartStore.Utilities;

namespace SmartStore.Data.Setup
{
	public abstract class InvariantSeedData
	{
		private SmartObjectContext _ctx;
		private string _sampleImagesPath;
		private string _sampleDownloadsPath;

		protected InvariantSeedData()
		{
		}

		public void Initialize(SmartObjectContext context)
		{
			_ctx = context;

			_sampleImagesPath = CommonHelper.MapPath("~/App_Data/Samples/");
			_sampleDownloadsPath = CommonHelper.MapPath("~/App_Data/Samples/");
		}

		#region Mandatory data creators

		public IList<Picture> Pictures()
		{
			var entities = new List<Picture>
			{
				CreatePicture(File.ReadAllBytes(_sampleImagesPath + "company_logo.png"), "image/png", GetSeName("company-logo")),

				CreatePicture(File.ReadAllBytes(_sampleImagesPath + "product_allstar_charcoal.jpg"), "image/jpeg", "all-star-charcoal"),
				CreatePicture(File.ReadAllBytes(_sampleImagesPath + "product_allstar_maroon.jpg"), "image/jpeg", "all-star-maroon"),
				CreatePicture(File.ReadAllBytes(_sampleImagesPath + "product_allstar_navy.jpg"), "image/jpeg", "all-star-navy"),
				CreatePicture(File.ReadAllBytes(_sampleImagesPath + "product_allstar_purple.jpg"), "image/jpeg", "all-star-purple"),
				CreatePicture(File.ReadAllBytes(_sampleImagesPath + "product_allstar_white.jpg"), "image/jpeg", "all-star-white"),

				CreatePicture(File.ReadAllBytes(_sampleImagesPath + "wayfarer_havana.png"), "image/png", "wayfarer_havana"),
				CreatePicture(File.ReadAllBytes(_sampleImagesPath + "wayfarer_havana_black.png"), "image/png", "wayfarer_havana_black"),
				CreatePicture(File.ReadAllBytes(_sampleImagesPath + "wayfarer_rayban-black.png"), "image/png", "wayfarer_rayban_black")
			};

			this.Alter(entities);
			return entities;
		}

		public IList<Store> Stores()
		{
			var seName = GetSeName("company-logo");
			var imgCompanyLogo = _ctx.Set<Picture>().Where(x => x.SeoFilename == seName).FirstOrDefault();
			
			var currency = _ctx.Set<Currency>().FirstOrDefault(x => x.CurrencyCode == "EUR");
			if (currency == null)
				currency = _ctx.Set<Currency>().First();
			
			var entities = new List<Store>()
			{
				new Store()
				{
					Name = "Your store name",
					Url = "http://www.yourStore.com/",
					Hosts = "yourstore.com,www.yourstore.com",
					SslEnabled = false,
					DisplayOrder = 1,
					LogoPictureId = imgCompanyLogo.Id,
					PrimaryStoreCurrencyId = currency.Id,
					PrimaryExchangeRateCurrencyId = currency.Id
				}
			};
			this.Alter(entities);
			return entities;
		}

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
				CreateCurrency("en-US", published: true, rate: 1M, order: 0),
				CreateCurrency("en-GB", published: true, rate: 0.61M, order: 5),
				CreateCurrency("en-AU", published: true, rate: 0.94M, order: 10),
				CreateCurrency("en-CA", published: true, rate: 0.98M, order: 15),
				CreateCurrency("de-DE", rate: 0.79M, order: 20/*, formatting: string.Format("0.00 {0}", "\u20ac")*/),
				CreateCurrency("de-CH", rate: 0.93M, order: 25, formatting: "CHF #,##0.00"),
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

            #endregion

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

            #region CA Regions

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
					Name = "Croatia",
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
					Name = "Montenegro",
					AllowsBilling = true,
					AllowsShipping = true,
					TwoLetterIsoCode = "ME",
					ThreeLetterIsoCode = "MNE",
					NumericIsoCode = 499,
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
            var cCountry = _ctx.Set<Country>()
                .Where(x => x.ThreeLetterIsoCode == "USA")
                .FirstOrDefault();

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
				StateProvince = cCountry.StateProvinces.FirstOrDefault(),
				Country = cCountry,
				ZipPostalCode = "12212",
				CreatedOnUtc = DateTime.UtcNow,
			};
			this.Alter(entity);
			return entity;
		}

		public Customer SearchEngineUser()
		{
			var entity = new Customer
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
			var entity = new Customer
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

		public Customer PdfConverterUser()
		{
			var entity = new Customer
			{
				Email = "builtin@pdf-converter-record.com",
				CustomerGuid = Guid.NewGuid(),
				PasswordFormat = PasswordFormat.Clear,
				AdminComment = "Built-in system record used for the PDF converter.",
				Active = true,
				IsSystemAccount = true,
				SystemName = SystemCustomerNames.PdfConverter,
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
						Body = "<p><strong>Not registered yet?</strong></p><p>Create your own account now and experience our diversity. With an account you can place orders faster and will always have a&nbsp;perfect overview of your current and previous orders.</p>"
					},
				new Topic
					{
						SystemName = "PrivacyInfo",
						IncludeInSitemap = false,
						IsPasswordProtected = false,
						Title = "Privacy policy",
						Body = "<p><strong></strong></p>"
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

		public IList<ISettings> Settings()
		{
			var entities = new List<ISettings>
			{
				new PdfSettings
				{
				},
				new CommonSettings
				{
				},
				new SeoSettings()
				{
				},
				new SocialSettings()
				{
				},
				new AdminAreaSettings()
				{
				},
				new CatalogSettings()
				{
				},
				new LocalizationSettings()
				{
					DefaultAdminLanguageId = _ctx.Set<Language>().First().Id
				},
				new CustomerSettings()
				{
				},
				new AddressSettings()
				{
				},
				new MediaSettings()
				{
				},
				new StoreInformationSettings()
				{
				},
				new RewardPointsSettings()
				{
				},
				new CurrencySettings()
				{
				},
				new MeasureSettings()
				{
					BaseDimensionId = _ctx.Set<MeasureDimension>().Where(m => m.SystemKeyword == "inch").Single().Id,
					BaseWeightId = _ctx.Set<MeasureWeight>().Where(m => m.SystemKeyword == "lb").Single().Id,
				},
				new ShoppingCartSettings()
				{
				},
				new OrderSettings()
				{
				},
				new SecuritySettings()
				{
				},
				new ShippingSettings()
				{
				},
				new PaymentSettings()
				{
					ActivePaymentMethodSystemNames = new List<string>()
					{
						"Payments.CashOnDelivery",
						"Payments.Manual",
						"Payments.PayInStore",
						"Payments.Prepayment"
					}
				},
				new TaxSettings()
				{
				},
				new BlogSettings()
				{
				},
				new NewsSettings()
				{
				},
				new ForumSettings()
				{
				},
				new EmailAccountSettings()
				{
					DefaultEmailAccountId = _ctx.Set<EmailAccount>().First().Id
				},
				new ThemeSettings()
				{
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
										Name = "Default Product Template",
										ViewPath = "Product",
										DisplayOrder = 10
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

		public IList<ScheduleTask> ScheduleTasks()
		{
			var entities = new List<ScheduleTask>
			{
				new ScheduleTask
				{
					Name = "Send emails",
					CronExpression = "* * * * *", // every Minute
					Type = "SmartStore.Services.Messages.QueuedMessagesSendTask, SmartStore.Services",
					Enabled = true,
					StopOnError = false,
				},
				new ScheduleTask
				{
					Name = "Delete guests",
					CronExpression = "*/10 * * * *", // Every 10 minutes
					Type = "SmartStore.Services.Customers.DeleteGuestsTask, SmartStore.Services",
					Enabled = true,
					StopOnError = false,
				},
				new ScheduleTask
				{
					Name = "Delete logs",
					CronExpression = "0 1 * * *", // At 01:00
					Type = "SmartStore.Services.Logging.DeleteLogsTask, SmartStore.Services",
					Enabled = true,
					StopOnError = false,
				},
				new ScheduleTask
				{
					Name = "Clear cache",
					CronExpression = "0 */12 * * *", // Every 12 hours
					Type = "SmartStore.Services.Caching.ClearCacheTask, SmartStore.Services",
					Enabled = false,
					StopOnError = false,
				},
				new ScheduleTask
				{
					Name = "Update currency exchange rates",
					CronExpression = "0 */6 * * *", // Every 6 hours
					Type = "SmartStore.Services.Directory.UpdateExchangeRateTask, SmartStore.Services",
					Enabled = false,
					StopOnError = false,
				},
				new ScheduleTask
				{
					Name = "Clear transient uploads",
					CronExpression = "30 1,13 * * *", // At 01:30 and 13:30
					Type = "SmartStore.Services.Media.TransientMediaClearTask, SmartStore.Services",
					Enabled = true,
					StopOnError = false,
				},
				new ScheduleTask
				{
					Name = "Clear email queue",
					CronExpression = "0 2 * * *", // At 02:00
					Type = "SmartStore.Services.Messages.QueuedMessagesClearTask, SmartStore.Services",
					Enabled = true,
					StopOnError = false,
				},
				new ScheduleTask
				{
					Name = "Cleanup temporary files",
					CronExpression = "30 3 * * *", // At 03:30
					Type = "SmartStore.Services.Common.TempFileCleanupTask, SmartStore.Services",
					Enabled = true,
					StopOnError = false
				}
			};
			this.Alter(entities);
			return entities;
		}

		#endregion

		#region Sample data creators

		public IList<SpecificationAttribute> SpecificationAttributes()
		{
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
				Name = "Color",
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
				Name = "Harddisk capacity",
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
				Name = "Ports",
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
				Name = "Material",
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
				Name = "aluminium",
				DisplayOrder = 4,
			});

            sa8.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "leather",
                DisplayOrder = 5,
            });

            sa8.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "nylon",
                DisplayOrder = 6,
            });

            sa8.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "silicone",
                DisplayOrder = 7,
            });

            sa8.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "ceramic",
                DisplayOrder = 8,
            });

			sa8.SpecificationAttributeOptions.Add(new SpecificationAttributeOption
			{
				Name = "cotton",
				DisplayOrder = 9,
			});
			sa8.SpecificationAttributeOptions.Add(new SpecificationAttributeOption
			{
				Name = "100% organic cotton",
				DisplayOrder = 10,
			});
			sa8.SpecificationAttributeOptions.Add(new SpecificationAttributeOption
			{
				Name = "polyamide",
				DisplayOrder = 11,
			});
            sa8.SpecificationAttributeOptions.Add(new SpecificationAttributeOption
            {
                Name = "rubber",
                DisplayOrder = 12,
            });
			sa8.SpecificationAttributeOptions.Add(new SpecificationAttributeOption
			{
				Name = "wood",
				DisplayOrder = 13,
			});
			sa8.SpecificationAttributeOptions.Add(new SpecificationAttributeOption
			{
				Name = "glass",
				DisplayOrder = 14,
			});
			sa8.SpecificationAttributeOptions.Add(new SpecificationAttributeOption
			{
				Name = "spandex",
				DisplayOrder = 15,
			});
			sa8.SpecificationAttributeOptions.Add(new SpecificationAttributeOption
			{
				Name = "polyester",
				DisplayOrder = 16,
			});

			#endregion sa8 material

			#region sa9 movement

			var sa9 = new SpecificationAttribute
			{
				Name = "Movement",
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
				Name = "Clasp",
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
				Name = "Window material",
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
				Name = "Language",
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
				Name = "Edition",
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
				Name = "Category",
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
				Name = "Type of mass-storage",
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
				Name = "3.5",
				DisplayOrder = 1,
			});
			sa17.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
			{
				Name = "2.5",
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
				Name = "Music genre",
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
				Name = "Manufacturer",
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
            sa20.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "Seiko",
                DisplayOrder = 16,
            });
            sa20.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "Tissot",
                DisplayOrder = 17,
            });
            sa20.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "Breitling",
                DisplayOrder = 18,
            });
            sa20.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "Adidas",
                DisplayOrder = 19,
            });
            sa20.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "Nike",
                DisplayOrder = 20,
            });
            sa20.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "Puma",
                DisplayOrder = 21,
            });

            #endregion sa20 manufacturer

            #region sa21 Watches for whom

            var sa21 = new SpecificationAttribute
            {
                Name = "For whom",
                DisplayOrder = 21,
            };
            sa21.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "For him",
                DisplayOrder = 1,
            });
            sa21.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "For her",
                DisplayOrder = 2,
            });

            #endregion sa11 Watches for whom

            #region sa22 Offer

            var sa22 = new SpecificationAttribute
            {
                Name = "Offer",
                DisplayOrder = 22,
            };

            sa22.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "Clearance",
                DisplayOrder = 1,
            });

            sa22.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "Permanent low price",
                DisplayOrder = 2,
            });

            sa22.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "Promotion",
                DisplayOrder = 3,
            });

            sa22.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "Reduced price",
                DisplayOrder = 4,
            });

            sa22.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "Special Buy",
                DisplayOrder = 5,
            });

            sa22.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "Offer of the day",
                DisplayOrder = 6,
            });

            sa22.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "Weekly offer",
                DisplayOrder = 7,
            });

            sa22.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "Best Price",
                DisplayOrder = 8,
            });

            #endregion sa22 Offer

            #region sa23 Size

            var sa23 = new SpecificationAttribute
            {
                Name = "Size",
                DisplayOrder = 23,
            };

            sa23.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "XS",
                DisplayOrder = 1,
            });

            sa23.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "S",
                DisplayOrder = 2,
            });

            sa23.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "M",
                DisplayOrder = 3,
            });

            sa23.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "L",
                DisplayOrder = 4,
            });

            sa23.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "XL",
                DisplayOrder = 5,
            });

            sa23.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "XXL",
                DisplayOrder = 6,
            });


            #endregion sa23 Size

            #region sa24 diameter

            var sa24 = new SpecificationAttribute
            {
                Name = "Diameter",
                DisplayOrder = 24,
            };

            sa24.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "38mm",
                DisplayOrder = 1,
            });

            sa24.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "40mm",
                DisplayOrder = 2,
            });

            sa24.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "44mm",
                DisplayOrder = 3,
            });

            #endregion sa24 diameter

            #region sa25 closure

            var sa25 = new SpecificationAttribute
            {
                Name = "Closure",
                DisplayOrder = 25,
            };

            sa25.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "snap closure",
                DisplayOrder = 1,
            });

            sa25.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "folding clasp",
                DisplayOrder = 2,
            });

            sa25.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "thorn close",
                DisplayOrder = 3,
            });

            #endregion sa25 closure

            #region sa26 facial shape

            var sa26 = new SpecificationAttribute
            {
                Name = "Facial shape",
                DisplayOrder = 26,
            };

            sa26.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "oval",
                DisplayOrder = 1,
            });

            sa26.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "round",
                DisplayOrder = 2,
            });

            sa26.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "heart shaped",
                DisplayOrder = 3,
            });

            sa26.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "angular",
                DisplayOrder = 4,
            });

            #endregion sa26 facial shape

            #region sa27 storage capacity

            var sa27 = new SpecificationAttribute
            {
                Name = "Storage capacity",
                DisplayOrder = 27,
            };

            sa27.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "32 GB",
                DisplayOrder = 1,
            });

            sa27.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "64 GB",
                DisplayOrder = 2,
            });

            sa27.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "128 GB",
                DisplayOrder = 3,
            });

            #endregion sa27 facial shape

            #region sa28 Dial window material type

            var sa28 = new SpecificationAttribute
            {
                Name = "Dial window material type",
                DisplayOrder = 28,
            };

            sa28.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "Mineral",
                DisplayOrder = 1,
            });

            sa28.SpecificationAttributeOptions.Add(new SpecificationAttributeOption()
            {
                Name = "Sapphire",
                DisplayOrder = 2,
            });

            #endregion sa28 Dial window material type

            var entities = new List<SpecificationAttribute>
			{
				sa1,sa2,sa3,sa4,sa5,sa6,sa7,sa8,sa9,sa10,sa11,sa12,sa13,sa14,sa15,sa16,sa17,sa18,sa19,sa20,sa21,sa22,sa23,sa24,sa25,sa26,sa27,sa28
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
					Alias = "color"
				},
				new ProductAttribute
				{
					Name = "Custom Text",
					Alias = "custom-text"
				},
				new ProductAttribute
				{
					Name = "HDD",
					Alias = "hdd"
				},
				new ProductAttribute
				{
					Name = "OS",
					Alias = "os"
				},
				new ProductAttribute
				{
					Name = "Processor",
					Alias = "processor"
				},
				new ProductAttribute
				{
					Name = "RAM",
					Alias = "ram",
				},
				new ProductAttribute
				{
					Name = "Size",
					Alias = "size"
				},
				new ProductAttribute
				{
					Name = "Software",
					Alias = "software"
				},
				new ProductAttribute
				{
					Name = "Game",
					Alias = "game"
				},
				new ProductAttribute
				{
					Name = "Color",
					Alias = "iphone-color"
				},
                new ProductAttribute
                {
                    Name = "Color",
                    Alias = "ipad-color"
                },
                new ProductAttribute
				{
					Name = "Memory capacity",
					Alias = "memory-capacity"
				},
				new ProductAttribute
				{
					Name = "Width",
					Alias = "width"
				},
				new ProductAttribute
				{
					Name = "Length",
					Alias = "length"
				},
				new ProductAttribute
				{
					Name = "Plate",
					Alias = "plate"
				},
				new ProductAttribute
				{
					Name = "Plate Thickness",
					Alias = "plate-thickness"
				},		
                new ProductAttribute
                {
                    Name = "Ballsize",
                    Alias = "ballsize"
				},
				new ProductAttribute
				{
					Name = "Leather color",
					Alias = "leather-color"
				},
				new ProductAttribute
				{
					Name = "Seat Shell",
					Alias = "seat-shell"
				},
				new ProductAttribute
				{
					Name = "Base",
					Alias = "base"
				},
				new ProductAttribute
				{
					Name = "Material",
					Alias = "material"
				},
				new ProductAttribute
				{
					Name = "Style",
					Alias = "style"
				},
                new ProductAttribute
                {
                    Name = "Controller",
                    Alias = "controller"
                },
                new ProductAttribute
                {
                    Name = "Framecolor",
                    Alias = "framecolor"
                },
                new ProductAttribute
                {
                    Name = "Lenscolor",
                    Alias = "lenscolor"
                },
                new ProductAttribute
                {
                    Name = "Lenstype",
                    Alias = "lenstype"
                },
                new ProductAttribute
                {
                    Name = "Lenscolor",
                    Alias = "wayfarerlenscolor"
                },
                new ProductAttribute
                {
                    Name = "Framecolor",
                    Alias = "wayfarerframecolor"
                }
            };

			this.Alter(entities);
			return entities;
		}

        public IList<ProductAttributeOptionsSet> ProductAttributeOptionsSets()
        {
            var entities = new List<ProductAttributeOptionsSet>();
            var colorAttribute = _ctx.Set<ProductAttribute>().First(x => x.Alias == "color");

            entities.Add(new ProductAttributeOptionsSet
            {
                Name = "General colors",
                ProductAttributeId = colorAttribute.Id
            });

            this.Alter(entities);
            return entities;
        }

        public IList<ProductAttributeOption> ProductAttributeOptions()
        {
            var entities = new List<ProductAttributeOption>();
            var colorAttribute = _ctx.Set<ProductAttribute>().First(x => x.Alias == "color");
			var sets = _ctx.Set<ProductAttributeOptionsSet>().ToList();

			var generalColors = new[]
			{
				new { Color = "Red", Code = "#ff0000" },
				new { Color = "Green", Code = "#008000" },
				new { Color = "Blue", Code = "#0000ff" },
				new { Color = "Yellow", Code = "#ffff00" },
				new { Color = "Black", Code = "#000000" },
				new { Color = "White", Code = "#ffffff" },
				new { Color = "Gray", Code = "#808080" },
				new { Color = "Silver", Code = "#dddfde" },
				new { Color = "Brown", Code = "#a52a2a" },
			};

			for (var i = 0; i < generalColors.Length; ++i)
            {
                entities.Add(new ProductAttributeOption
                {
                    ProductAttributeOptionsSetId = sets[0].Id,
                    Alias = generalColors[i].Color.ToLower(),
                    Name = generalColors[i].Color,
                    Quantity = 1,
                    DisplayOrder = i + 1,
                    Color = generalColors[i].Code                    
                });
            }

            this.Alter(entities);
            return entities;
        }

        public IList<ProductVariantAttribute> ProductVariantAttributes()
		{
			var entities = new List<ProductVariantAttribute>();
			var attrColor = _ctx.Set<ProductAttribute>().First(x => x.Alias == "color");
            var attrController = _ctx.Set<ProductAttribute>().First(x => x.Alias == "controller");
            var attrSize = _ctx.Set<ProductAttribute>().First(x => x.Alias == "size");
			var attrGames = _ctx.Set<ProductAttribute>().First(x => x.Alias == "game");
            var attrBallsize = _ctx.Set<ProductAttribute>().First(x => x.Alias == "ballsize");
            var attrMemoryCapacity = _ctx.Set<ProductAttribute>().First(x => x.Alias == "memory-capacity");
            var attrLensType = _ctx.Set<ProductAttribute>().First(x => x.Alias == "lenstype");
            var attrFramecolor = _ctx.Set<ProductAttribute>().First(x => x.Alias =="framecolor");
            var attrLenscolor = _ctx.Set<ProductAttribute>().First(x => x.Alias == "lenscolor");
            var attrIphoneColor = _ctx.Set<ProductAttribute>().First(x => x.Alias == "iphone-color");
            var attr97iPadColor = _ctx.Set<ProductAttribute>().First(x => x.Alias == "ipad-color");
			var attrWidth = _ctx.Set<ProductAttribute>().First(x => x.Alias == "width");
			var attrLength = _ctx.Set<ProductAttribute>().First(x => x.Alias == "length");
			var attrPlate = _ctx.Set<ProductAttribute>().First(x => x.Alias == "plate");
			var attrPlateThickness = _ctx.Set<ProductAttribute>().First(x => x.Alias == "plate-thickness");
			var attrLeatherColor = _ctx.Set<ProductAttribute>().First(x => x.Alias == "leather-color");
			var attrSeatShell = _ctx.Set<ProductAttribute>().First(x => x.Alias == "seat-shell");
			var attrBase = _ctx.Set<ProductAttribute>().First(x => x.Alias == "base");
			var attrMaterial = _ctx.Set<ProductAttribute>().First(x => x.Alias == "material");
            var attrWayfarerLenscolor = _ctx.Set<ProductAttribute>().First(x => x.Alias == "wayfarerlenscolor");
            var attrWayfarerFramecolor = _ctx.Set<ProductAttribute>().First(x => x.Alias == "wayfarerframecolor");

            var generalColors = new[]
			{
				new { Name = "Black", Color = "#000000" },
				new { Name = "White", Color = "#ffffff" },
				new { Name = "Anthracite", Color = "#32312f" },
				new { Name = "Fuliginous", Color = "#5F5B5C" },
				new { Name = "Light grey", Color = "#e3e3e5" },
				new { Name = "Natural", Color = "#BBB98B" },
				new { Name = "Biscuit", Color = "#e0ccab" },
				new { Name = "Beige", Color = "#d1bc8a" },
				new { Name = "Hazelnut", Color = "#94703e" },
				new { Name = "Brown", Color = "#755232" },
				new { Name = "Dark brown", Color = "#27160F" },
				new { Name = "Dark green", Color = "#0a3210" },
				new { Name = "Blue", Color = "#0000ff" },
				new { Name = "Cognac", Color = "#e9aa1b" },
				new { Name = "Yellow", Color = "#e6e60c" },
				new { Name = "Orange", Color = "#ff6501" },
				new { Name = "Tomato red", Color = "#b10101" },
				new { Name = "Red", Color = "#fe0000" },
				new { Name = "Dark red", Color = "#5e0000" }
			};


            #region Oakley custom flak

            var productCustomFlak = _ctx.Set<Product>().First(x => x.Sku == "P-3002");

            var attributeLensType = new ProductVariantAttribute()
            {
                Product = productCustomFlak,
                ProductAttribute = attrLensType,
                IsRequired = true,
                DisplayOrder = 1,
                AttributeControlType = AttributeControlType.RadioList
            };

            attributeLensType.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
            {
                Name = "Standard",
                Alias = "standard",
                IsPreSelected = true,
                DisplayOrder = 1,
                Quantity = 1,
                ValueType = ProductVariantAttributeValueType.Simple,
                PriceAdjustment = 100.0M
            });

            attributeLensType.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
            {
                Name = "Polarized",
                Alias = "polarized",
                DisplayOrder = 2,
                Quantity = 1,
                ValueType = ProductVariantAttributeValueType.Simple,
                PriceAdjustment = 200.0M
            });
            attributeLensType.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
            {
                Name = "Prizm",
                Alias = "prizm",
                DisplayOrder = 3,
                Quantity = 1,
                ValueType = ProductVariantAttributeValueType.Simple,
                PriceAdjustment = 200.0M
            });

            entities.Add(attributeLensType);


            var attributeFramecolor = new ProductVariantAttribute()
            {
                Product = productCustomFlak,
                ProductAttribute = attrFramecolor,
                IsRequired = true,
                DisplayOrder = 2,
                AttributeControlType = AttributeControlType.Boxes
            };

            attributeFramecolor.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
            {
                Name = "Matte Black",
                Alias = "matteblack",
                IsPreSelected = true,
                DisplayOrder = 1,
                Quantity = 1,
                ValueType = ProductVariantAttributeValueType.Simple,
                Color = "#2d2d2d"
            });

            attributeFramecolor.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
            {
                Name = "Polishedwhite",
                Alias = "polishedwhite",
                DisplayOrder = 2,
                Quantity = 1,
                ValueType = ProductVariantAttributeValueType.Simple,
                Color = "#f5f5f5"
            });

            attributeFramecolor.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
            {
                Name = "Sky Blue",
                Alias = "skyblue",
                DisplayOrder = 3,
                Quantity = 1,
                ValueType = ProductVariantAttributeValueType.Simple,
                Color = "#4187f6"
            });

            attributeFramecolor.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
            {
                Name = "Orange Flare",
                Alias = "orangeflare",
                DisplayOrder = 4,
                Quantity = 1,
                ValueType = ProductVariantAttributeValueType.Simple,
                Color = "#f55700"
            });

            attributeFramecolor.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
            {
                Name = "Redline",
                Alias = "redline",
                DisplayOrder = 5,
                Quantity = 1,
                ValueType = ProductVariantAttributeValueType.Simple,
                Color = "#cf0a02"
            });

            entities.Add(attributeFramecolor);

            var attributeLenscolor = new ProductVariantAttribute()
            {
                Product = productCustomFlak,
                ProductAttribute = attrLenscolor,
                IsRequired = true,
                DisplayOrder = 3,
                AttributeControlType = AttributeControlType.Boxes
            };

            attributeLenscolor.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
            {
                Name = "Gray",
                Alias = "gray",
                IsPreSelected = true,
                DisplayOrder = 1,
                Quantity = 1,
                ValueType = ProductVariantAttributeValueType.Simple,
                Color = "#7A798B"
            });

            attributeLenscolor.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
            {
                Name = "Sapphire Iridium",
                Alias = "sapphireiridium",
                DisplayOrder = 2,
                Quantity = 1,
                ValueType = ProductVariantAttributeValueType.Simple,
                Color = "#4460BB"
            });

            attributeLenscolor.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
            {
                Name = "Violet Iridium",
                Alias = "violetiridium",
                DisplayOrder = 3,
                Quantity = 1,
                ValueType = ProductVariantAttributeValueType.Simple,
                Color = "#5C5A89"
            });

            attributeLenscolor.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
            {
                Name = "Jade Iridium",
                Alias = "jadeiridium",
                DisplayOrder = 4,
                Quantity = 1,
                ValueType = ProductVariantAttributeValueType.Simple,
                Color = "#376559"
            });

            attributeLenscolor.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
            {
                Name = "Ruby Iridium",
                Alias = "rubyiridium",
                DisplayOrder = 5,
                Quantity = 1,
                ValueType = ProductVariantAttributeValueType.Simple,
                Color = "#CCAD12"
            });

            attributeLenscolor.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
            {
                Name = "24K Iridium",
                Alias = "24kiridium",
                DisplayOrder = 6,
                Quantity = 1,
                ValueType = ProductVariantAttributeValueType.Simple,
                Color = "#CE9D12"
            });

            attributeLenscolor.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
            {
                Name = "Positive Red Iridium",
                Alias = "positiverediridium",
                DisplayOrder = 7,
                Quantity = 1,
                ValueType = ProductVariantAttributeValueType.Simple,
                Color = "#764CDC"
            });

            attributeLenscolor.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
            {
                Name = "Clear",
                Alias = "clear",
                DisplayOrder = 7,
                Quantity = 1,
                ValueType = ProductVariantAttributeValueType.Simple,
                Color = "#e2e2e3"
            });
            attributeLenscolor.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
            {
                Name = "Fire Iridium",
                Alias = "fireiridium",
                DisplayOrder = 7,
                Quantity = 1,
                ValueType = ProductVariantAttributeValueType.Simple,
                Color = "#E2C724"
            });

            entities.Add(attributeLenscolor);

            #endregion Oakley custom flak


            #region wayfarer

            var productWayfarer = _ctx.Set<Product>().First(x => x.Sku == "P-3003");
            var wayfarerFramePictures = _ctx.Set<Picture>().Where(x => x.SeoFilename.StartsWith("wayfarer_")).ToList();

            var attributeWayfarerLenscolor = new ProductVariantAttribute()
            {
                Product = productWayfarer,
                ProductAttribute = attrWayfarerLenscolor,
                IsRequired = true,
                DisplayOrder = 3,
                AttributeControlType = AttributeControlType.Boxes
            };

            attributeWayfarerLenscolor.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
            {
                Name = "Blue-Gray classic",
                Alias = "blue-gray-classic",
                IsPreSelected = true,
                DisplayOrder = 1,
                Quantity = 1,
                ValueType = ProductVariantAttributeValueType.Simple,
                Color = "#3e4659"
            });

            attributeWayfarerLenscolor.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
            {
                Name = "Brown course",
                Alias = "brown-course",
                DisplayOrder = 2,
                Quantity = 1,
                ValueType = ProductVariantAttributeValueType.Simple,
                Color = "#3e4659"
            });

            attributeWayfarerLenscolor.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
            {
                Name = "Gray course",
                Alias = "gray-course",
                DisplayOrder = 3,
                Quantity = 1,
                ValueType = ProductVariantAttributeValueType.Simple,
                Color = "#727377"
            });

            attributeWayfarerLenscolor.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
            {
                Name = "Green classic",
                Alias = "green-classic",
                DisplayOrder = 4,
                Quantity = 1,
                ValueType = ProductVariantAttributeValueType.Simple,
                Color = "#3c432e"
            });

            entities.Add(attributeWayfarerLenscolor);

            var attributeWayfarerFramecolor = new ProductVariantAttribute()
            {
                Product = productWayfarer,
                ProductAttribute = attrWayfarerFramecolor,
                IsRequired = true,
                DisplayOrder = 3,
                AttributeControlType = AttributeControlType.Boxes
            };

            var wayfarerFramePicture = wayfarerFramePictures.First(x => x.SeoFilename.EndsWith("_rayban_black"));

            attributeWayfarerFramecolor.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
            {
                Name = "Black",
                Alias = "rayban-black",
                IsPreSelected = true,
                DisplayOrder = 1,
                Quantity = 1,
                ValueType = ProductVariantAttributeValueType.Simple,
                //Color = "#3e4659"
                PictureId = wayfarerFramePicture.Id
            });

            wayfarerFramePicture = wayfarerFramePictures.First(x => x.SeoFilename.EndsWith("_havana_black"));
            attributeWayfarerFramecolor.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
            {
                Name = "Havana; Black",
                Alias = "havana-black",
                DisplayOrder = 2,
                Quantity = 1,
                ValueType = ProductVariantAttributeValueType.Simple,
                //Color = "#3e4659"
                PictureId = wayfarerFramePicture.Id
            });

            wayfarerFramePicture = wayfarerFramePictures.First(x => x.SeoFilename.EndsWith("_havana"));
            attributeWayfarerFramecolor.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
            {
                Name = "Havana",
                Alias = "havana",
                DisplayOrder = 3,
                Quantity = 1,
                ValueType = ProductVariantAttributeValueType.Simple,
                //Color = "#727377",
                PictureId = wayfarerFramePicture.Id
            });


            entities.Add(attributeWayfarerFramecolor);

            #endregion wayfarer

            #region 9,7 iPad

            var product97iPad = _ctx.Set<Product>().First(x => x.Sku == "P-2004");

            var attribute97iPadMemoryCapacity = new ProductVariantAttribute()
            {
                Product = product97iPad,
                ProductAttribute = attrMemoryCapacity,
                IsRequired = true,
                DisplayOrder = 1,
                AttributeControlType = AttributeControlType.RadioList
            };

            attribute97iPadMemoryCapacity.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
            {
                Name = "64 GB",
                Alias = "64gb",
                IsPreSelected = true,
                DisplayOrder = 2,
                Quantity = 1,
                ValueType = ProductVariantAttributeValueType.Simple,
                PriceAdjustment = 100.0M
            });

            attribute97iPadMemoryCapacity.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
            {
                Name = "128 GB",
                Alias = "128gb",
                DisplayOrder = 3,
                Quantity = 1,
                ValueType = ProductVariantAttributeValueType.Simple,
                PriceAdjustment = 200.0M
            });

            entities.Add(attribute97iPadMemoryCapacity);


            var attribute97iPadColor = new ProductVariantAttribute()
            {
                Product = product97iPad,
                ProductAttribute = attr97iPadColor,
                IsRequired = true,
                DisplayOrder = 2,
                AttributeControlType = AttributeControlType.Boxes
            };

            attribute97iPadColor.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
            {
                Name = "Silver",
                Alias = "silver",
                IsPreSelected = true,
                DisplayOrder = 1,
                Quantity = 1,
                ValueType = ProductVariantAttributeValueType.Simple,
                Color = "#dddfde"
            });

            attribute97iPadColor.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
            {
                Name = "Gold",
                Alias = "gold",
                DisplayOrder = 2,
                Quantity = 1,
                ValueType = ProductVariantAttributeValueType.Simple,
                Color = "#e3d0ba"
            });

            attribute97iPadColor.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
            {
                Name = "Spacegray",
                Alias = "spacegray",
                DisplayOrder = 3,
                Quantity = 1,
                ValueType = ProductVariantAttributeValueType.Simple,
                Color = "#abaeb1"
            });

            attribute97iPadColor.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
            {
                Name = "Rose",
                Alias = "rose",
                DisplayOrder = 4,
                Quantity = 1,
                ValueType = ProductVariantAttributeValueType.Simple,
                Color = "#d9a6ad"
            });

            attribute97iPadColor.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
            {
                Name = "Mint",
                Alias = "mint",
                DisplayOrder = 5,
                Quantity = 1,
                ValueType = ProductVariantAttributeValueType.Simple,
                Color = "#a6dbb1"
            });

            attribute97iPadColor.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
            {
                Name = "Purple",
                Alias = "purple",
                DisplayOrder = 6,
                Quantity = 1,
                ValueType = ProductVariantAttributeValueType.Simple,
                Color = "#dba5d7"
            });

            attribute97iPadColor.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
            {
                Name = "Lightblue",
                Alias = "lightblue",
                DisplayOrder = 7,
                Quantity = 1,
                ValueType = ProductVariantAttributeValueType.Simple,
                Color = "#a6b9df"
            });

            attribute97iPadColor.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
            {
                Name = "Turquoise",
                Alias = "turquoise",
                DisplayOrder = 8,
                Quantity = 1,
                ValueType = ProductVariantAttributeValueType.Simple,
                Color = "#a4dbde"
            });

            attribute97iPadColor.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
            {
                Name = "Yellow",
                Alias = "yellow",
                DisplayOrder = 7,
                Quantity = 1,
                ValueType = ProductVariantAttributeValueType.Simple,
                Color = "#dfddb6"
            });


            entities.Add(attribute97iPadColor);

            #endregion 9,7 iPad

            #region iPhone 7 plus

            var productIphone7Plus = _ctx.Set<Product>().First(x => x.Sku == "P-2001");

            var attributeIphone7PlusMemoryCapacity = new ProductVariantAttribute()
            {
                Product = productIphone7Plus,
                ProductAttribute = attrMemoryCapacity,
                IsRequired = true,
                DisplayOrder = 1,
                AttributeControlType = AttributeControlType.RadioList
            };

            attributeIphone7PlusMemoryCapacity.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
            {
                Name = "64 GB",
                Alias = "64gb",
                IsPreSelected = true,
                DisplayOrder = 2,
                Quantity = 1,
                ValueType = ProductVariantAttributeValueType.Simple,
                PriceAdjustment = 100.0M
            });

            attributeIphone7PlusMemoryCapacity.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
            {
                Name = "128 GB",
                Alias = "128gb",
                DisplayOrder = 3,
                Quantity = 1,
                ValueType = ProductVariantAttributeValueType.Simple,
                PriceAdjustment = 200.0M
            });

            entities.Add(attributeIphone7PlusMemoryCapacity);


            var attributeIphone7PlusColor = new ProductVariantAttribute()
            {
                Product = productIphone7Plus,
                ProductAttribute = attrIphoneColor,
                IsRequired = true,
                DisplayOrder = 2,
                AttributeControlType = AttributeControlType.Boxes
            };

            attributeIphone7PlusColor.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
            {
                Name = "Silver",
                Alias = "silver",
                IsPreSelected = true,
                DisplayOrder = 1,
                Quantity = 1,
                ValueType = ProductVariantAttributeValueType.Simple,
                Color = "#dddfde"
            });

            attributeIphone7PlusColor.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
            {
                Name = "Gold",
                Alias = "gold",
                DisplayOrder = 2,
                Quantity = 1,
                ValueType = ProductVariantAttributeValueType.Simple,
                Color = "#e3d0ba"
            });

            attributeIphone7PlusColor.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
            {
                Name = "Red",
                Alias = "red",
                DisplayOrder = 3,
                Quantity = 1,
                ValueType = ProductVariantAttributeValueType.Simple,
                Color = "#af1e2d"
            });

            attributeIphone7PlusColor.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
            {
                Name = "Rose",
                Alias = "rose",
                DisplayOrder = 4,
                Quantity = 1,
                ValueType = ProductVariantAttributeValueType.Simple,
                Color = "#d9a6ad"
            });

            attributeIphone7PlusColor.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
            {
                Name = "Black",
                Alias = "black",
                DisplayOrder = 5,
                Quantity = 1,
                ValueType = ProductVariantAttributeValueType.Simple,
                Color = "#000000"
            });

            entities.Add(attributeIphone7PlusColor);

            #endregion iPhone 7 plus

            #region iPhone

   //         var productIphone = _ctx.Set<Product>().First(x => x.Sku == "Apple-1001");

			//var attributeIphoneMemoryCapacity = new ProductVariantAttribute()
			//{
			//	Product = productIphone,
			//	ProductAttribute = attrMemoryCapacity,
			//	IsRequired = true,
			//	DisplayOrder = 1,
			//	AttributeControlType = AttributeControlType.DropdownList
			//};

			//attributeIphoneMemoryCapacity.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
			//{
			//	Name = "16 GB",
			//	Alias = "16gb",
			//	IsPreSelected = true,
			//	DisplayOrder = 1,
			//	Quantity = 1,
			//	ValueType = ProductVariantAttributeValueType.Simple
			//});

			//attributeIphoneMemoryCapacity.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
			//{
			//	Name = "64 GB",
			//	Alias = "64gb",
			//	DisplayOrder = 2,
			//	Quantity = 1,
			//	ValueType = ProductVariantAttributeValueType.Simple,
			//	PriceAdjustment = 100.0M
			//});

			//attributeIphoneMemoryCapacity.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
			//{
			//	Name = "128 GB",
			//	Alias = "128gb",
			//	DisplayOrder = 3,
			//	Quantity = 1,
			//	ValueType = ProductVariantAttributeValueType.Simple,
			//	PriceAdjustment = 200.0M
			//});

			//entities.Add(attributeIphoneMemoryCapacity);


			//var attributeIphoneColor = new ProductVariantAttribute()
			//{
			//	Product = productIphone,
			//	ProductAttribute = attrIphoneColor,
			//	IsRequired = true,
			//	DisplayOrder = 2,
			//	AttributeControlType = AttributeControlType.DropdownList
			//};

			//attributeIphoneColor.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
			//{
			//	Name = "Silver",
			//	Alias = "silver",
			//	IsPreSelected = true,
			//	DisplayOrder = 1,
			//	Quantity = 1,
			//	ValueType = ProductVariantAttributeValueType.Simple
			//});

			//attributeIphoneColor.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
			//{
			//	Name = "Gold",
			//	Alias = "gold",
			//	DisplayOrder = 2,
			//	Quantity = 1,
			//	ValueType = ProductVariantAttributeValueType.Simple
			//});

			//attributeIphoneColor.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
			//{
			//	Name = "Space gray",
			//	Alias = "spacegray",
			//	DisplayOrder = 3,
			//	Quantity = 1,
			//	ValueType = ProductVariantAttributeValueType.Simple
			//});

			//entities.Add(attributeIphoneColor);

			#endregion iPhone

			#region attributeDualshock3ControllerColor

			var productPs3 = _ctx.Set<Product>().First(x => x.Sku == "Sony-PS399000");

			var attributeDualshock3ControllerColor = new ProductVariantAttribute()
			{
				Product = productPs3,
				ProductAttribute = attrController,
				IsRequired = true,
				DisplayOrder = 1,
				AttributeControlType = AttributeControlType.RadioList
			};

			attributeDualshock3ControllerColor.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
			{
				Name = "without controller",
				Alias = "without_controller",
				IsPreSelected = true,
				DisplayOrder = 1,
				Quantity = 1,
				ValueType = ProductVariantAttributeValueType.Simple
			});

			attributeDualshock3ControllerColor.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
			{
				Name = "with controller",
				Alias = "with_controller",
				PriceAdjustment = 60.0M,
				DisplayOrder = 2,
				Quantity = 1,
				ValueType = ProductVariantAttributeValueType.Simple
			});

			entities.Add(attributeDualshock3ControllerColor);

            #endregion attributeDualshock3ControllerColor

            #region attribute  Apple Airpod

            var productAirpod = _ctx.Set<Product>().First(x => x.Sku == "P-2003");

            var attributeAirpod = new ProductVariantAttribute()
            {
                Product = productAirpod,
                ProductAttribute = attrColor,
                IsRequired = true,
                DisplayOrder = 1,
                AttributeControlType = AttributeControlType.Boxes
            };

            attributeAirpod.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
            {
                Name = "Gold",
                Alias = "gold",
                DisplayOrder = 1,
                Quantity = 1,
                Color = "#e3d0ba",
                ValueType = ProductVariantAttributeValueType.Simple,
                PriceAdjustment = 5.00M
                //LinkedProductId = _ctx.Set<Product>().First(x => x.Sku == "Ubi-acreed3").Id
            });

            attributeAirpod.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
            {
                Name = "Rose",
                Alias = "rose",
                DisplayOrder = 2,
                Quantity = 1,
                Color = "#d9a6ad",
                ValueType = ProductVariantAttributeValueType.Simple,
                PriceAdjustment = 10.00M,
                //LinkedProductId = _ctx.Set<Product>().First(x => x.Sku == "Ubi-watchdogs").Id
            });

            attributeAirpod.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
            {
                Name = "Mint",
                Alias = "mint",
                DisplayOrder = 3,
                Quantity = 1,
                Color= "#a6dbb1",
                ValueType = ProductVariantAttributeValueType.Simple,
                PriceAdjustment = 15.00M
                //LinkedProductId = _ctx.Set<Product>().First(x => x.Sku == "Ubi-princepersia").Id
            });

            attributeAirpod.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
            {
                Name = "Lightblue",
                Alias = "lightblue",
                DisplayOrder = 3,
                Quantity = 1,
                Color = "#a6b9df",
                ValueType = ProductVariantAttributeValueType.Simple,
                PriceAdjustment = 15.00M
                //LinkedProductId = _ctx.Set<Product>().First(x => x.Sku == "Ubi-princepersia").Id
            });

            attributeAirpod.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
            {
                Name = "Turquoise",
                Alias = "turquoise",
                DisplayOrder = 3,
                Quantity = 1,
                Color = "#a4dbde",
                ValueType = ProductVariantAttributeValueType.Simple,
                PriceAdjustment = 15.00M
                //LinkedProductId = _ctx.Set<Product>().First(x => x.Sku == "Ubi-princepersia").Id
            });

            attributeAirpod.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
            {
                Name = "White",
                Alias = "white",
                DisplayOrder = 3,
                Quantity = 1,
                Color = "#ffffff",
                IsPreSelected = true,
                ValueType = ProductVariantAttributeValueType.Simple,
                PriceAdjustment = 15.00M
                //LinkedProductId = _ctx.Set<Product>().First(x => x.Sku == "Ubi-princepersia").Id
            });

            entities.Add(attributeAirpod);

            #endregion attribute Apple Airpod

            #region attribute Evopower 5.3 Trainer HS Ball

            var productEvopower = _ctx.Set<Product>().First(x => x.Sku == "P-5003");

            var attributeEvopower = new ProductVariantAttribute()
            {
                Product = productEvopower,
                ProductAttribute = attrBallsize,
                IsRequired = true,
                DisplayOrder = 1,
                AttributeControlType = AttributeControlType.RadioList
            };

            attributeEvopower.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
            {
                Name = "3",
                Alias = "ballsize-3",
                DisplayOrder = 1,
                Quantity = 1,
                ValueType = ProductVariantAttributeValueType.Simple,
                PriceAdjustment = 5.00M
                //LinkedProductId = _ctx.Set<Product>().First(x => x.Sku == "Ubi-acreed3").Id
            });

            attributeEvopower.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
            {
                Name = "4",
                Alias = "ballsize-4",
                DisplayOrder = 2,
                Quantity = 1,
                ValueType = ProductVariantAttributeValueType.Simple,
                PriceAdjustment = 10.00M,
                IsPreSelected = true
                //LinkedProductId = _ctx.Set<Product>().First(x => x.Sku == "Ubi-watchdogs").Id
            });

            attributeEvopower.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
            {
                Name = "5",
                Alias = "ballsize-5",
                DisplayOrder = 3,
                Quantity = 1,
                ValueType = ProductVariantAttributeValueType.Simple,
                PriceAdjustment = 15.00M
                //LinkedProductId = _ctx.Set<Product>().First(x => x.Sku == "Ubi-princepersia").Id
            });

            
            entities.Add(attributeEvopower);

            #endregion attribute Evopower 5.3 Trainer HS Ball

            #region attributePs3OneGameFree

   //         var productPs3OneGameFree = _ctx.Set<Product>().First(x => x.Sku == "Sony-PS310111");

			//var attributePs3OneGameFree = new ProductVariantAttribute()
			//{
			//	Product = productPs3OneGameFree,
			//	ProductAttribute = attrGames,
			//	IsRequired = true,
			//	DisplayOrder = 1,
			//	AttributeControlType = AttributeControlType.DropdownList
			//};

			//attributePs3OneGameFree.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
			//{
			//	Name = "Minecraft - Playstation 4 Edition",
			//	Alias = "minecraft-playstation4edition",
			//	DisplayOrder = 1,
			//	Quantity = 1,
			//	ValueType = ProductVariantAttributeValueType.ProductLinkage,
			//	LinkedProductId = _ctx.Set<Product>().First(x => x.Sku == "PD-Minecraft4ps4").Id
			//});

			//attributePs3OneGameFree.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
			//{
			//	Name = "Watch Dogs",
			//	Alias = "watch-dogs",
			//	DisplayOrder = 2,
			//	Quantity = 1,
			//	ValueType = ProductVariantAttributeValueType.ProductLinkage,
			//	LinkedProductId = _ctx.Set<Product>().First(x => x.Sku == "Ubi-watchdogs").Id
			//});

			//attributePs3OneGameFree.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
			//{
			//	Name = "Horizon Zero Dawn - PlayStation 4",
			//	Alias = "horizon-zero-dawn-playStation-4",
			//	DisplayOrder = 3,
			//	Quantity = 1,
			//	ValueType = ProductVariantAttributeValueType.ProductLinkage,
			//	LinkedProductId = _ctx.Set<Product>().First(x => x.Sku == "PD-ZeroDown4PS4").Id
			//});

			//attributePs3OneGameFree.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue()
			//{
			//	Name = "LEGO Worlds - PlayStation 4",
   //             Alias = "lego-worlds-playstation_4",
			//	DisplayOrder = 4,
			//	Quantity = 1,
			//	ValueType = ProductVariantAttributeValueType.ProductLinkage,
			//	LinkedProductId = _ctx.Set<Product>().First(x => x.Sku == "Gaming-Lego-001").Id
			//});

			//entities.Add(attributePs3OneGameFree);

			#endregion attributePs3OneGameFree

			#region Fashion - Converse All Star

			var productAllStar = _ctx.Set<Product>().First(x => x.Sku == "Fashion-112355");
			var allStarColors = new string[] { "Charcoal", "Maroon", "Navy", "Purple", "White" };
			var allStarPictures = _ctx.Set<Picture>().Where(x => x.SeoFilename.StartsWith("all-star-")).ToList();

			var attrAllStarColor = new ProductVariantAttribute
			{
				Product = productAllStar,
				ProductAttribute = attrColor,
				IsRequired = true,
				DisplayOrder = 1,
				AttributeControlType = AttributeControlType.Boxes
			};

			for (var i = 0; i < allStarColors.Length; ++i)
			{
				var allStarPicture = allStarPictures.First(x => x.SeoFilename.EndsWith(allStarColors[i].ToLower()));
				attrAllStarColor.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue
				{
					Name = allStarColors[i],
					Alias = allStarColors[i].ToLower(),
					DisplayOrder = i + 1,
					Quantity = 1,
					PictureId = allStarPicture.Id
				});
			}
			entities.Add(attrAllStarColor);

			var attrAllStarSize = new ProductVariantAttribute
			{
				Product = productAllStar,
				ProductAttribute = attrSize,
				IsRequired = true,
				DisplayOrder = 2,
				AttributeControlType = AttributeControlType.Boxes
			};
			attrAllStarSize.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue
			{
				Name = "42",
				Alias = "42",
				DisplayOrder = 1,
				Quantity = 1,
				IsPreSelected = true
			});
			attrAllStarSize.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue
			{
				Name = "43",
				Alias = "43",
				DisplayOrder = 2,
				Quantity = 1
			});
			attrAllStarSize.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue
			{
				Name = "44",
				Alias = "44",
				DisplayOrder = 3,
				Quantity = 1
			});
			entities.Add(attrAllStarSize);

			#endregion

			#region Fashion - Shirt Meccanica

			var productShirtMeccanica = _ctx.Set<Product>().First(x => x.Sku == "Fashion-987693502");
			var shirtMeccanicaSizes = new string[] { "XS", "S", "M", "L", "XL" };
			var shirtMeccanicaColors = new[]
			{
				new { Color = "Red", Code = "#fe0000" },
				new { Color = "Black", Code = "#000000" }
			};

			var attrShirtMeccanicaColor = new ProductVariantAttribute
			{
				Product = productShirtMeccanica,
				ProductAttribute = attrColor,
				IsRequired = true,
				DisplayOrder = 1,
				AttributeControlType = AttributeControlType.Boxes
			};

			for (var i = 0; i < shirtMeccanicaColors.Length; ++i)
			{
				attrShirtMeccanicaColor.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue
				{
					Name = shirtMeccanicaColors[i].Color,
					Alias = shirtMeccanicaColors[i].Color.ToLower(),
					DisplayOrder = i + 1,
					Quantity = 1,
					Color = shirtMeccanicaColors[i].Code,
					IsPreSelected = shirtMeccanicaColors[i].Color == "Red"
				});
			}
			entities.Add(attrShirtMeccanicaColor);

			var attrShirtMeccanicaSize = new ProductVariantAttribute
			{
				Product = productShirtMeccanica,
				ProductAttribute = attrSize,
				IsRequired = true,
				DisplayOrder = 2,
				AttributeControlType = AttributeControlType.Boxes
			};

			for (var i = 0; i < shirtMeccanicaSizes.Length; ++i)
			{
				attrShirtMeccanicaSize.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue
				{
					Name = shirtMeccanicaSizes[i],
					Alias = shirtMeccanicaSizes[i].ToLower(),
					DisplayOrder = i + 1,
					Quantity = 1,
					IsPreSelected = shirtMeccanicaSizes[i] == "XS"
				});
			}
			entities.Add(attrShirtMeccanicaSize);

			#endregion

			#region Fashion - Ladies Jacket

			var productLadiesJacket = _ctx.Set<Product>().First(x => x.Sku == "Fashion-JN1107");
			var ladiesJacketSizes = new string[] { "XS", "S", "M", "L", "XL" };
			var ladiesJacketColors = new[]
			{
				new { Color = "Red", Code = "#CE1F1C" },
				new { Color = "Orange", Code = "#EB7F01" },
				new { Color = "Green", Code = "#24B87E" },
				new { Color = "Blue", Code = "#0F8CCE" },
				new { Color = "Navy", Code = "#525671" },
				new { Color = "Silver", Code = "#ABB0B3" },
				new { Color = "Black", Code = "#404040" }
			};

			var attrLadiesJacketColor = new ProductVariantAttribute
			{
				Product = productLadiesJacket,
				ProductAttribute = attrColor,
				IsRequired = true,
				DisplayOrder = 1,
				AttributeControlType = AttributeControlType.Boxes
			};

			for (var i = 0; i < ladiesJacketColors.Length; ++i)
			{
				attrLadiesJacketColor.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue
				{
					Name = ladiesJacketColors[i].Color,
					Alias = ladiesJacketColors[i].Color.ToLower(),
					DisplayOrder = i + 1,
					Quantity = 1,
					Color = ladiesJacketColors[i].Code,
					IsPreSelected = ladiesJacketColors[i].Color == "Red"
				});
			}
			entities.Add(attrLadiesJacketColor);

			var attrLadiesJacketSize = new ProductVariantAttribute
			{
				Product = productLadiesJacket,
				ProductAttribute = attrSize,
				IsRequired = true,
				DisplayOrder = 2,
				AttributeControlType = AttributeControlType.RadioList
			};

			for (var i = 0; i < ladiesJacketSizes.Length; ++i)
			{
				attrLadiesJacketSize.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue
				{
					Name = ladiesJacketSizes[i],
					Alias = ladiesJacketSizes[i].ToLower(),
					DisplayOrder = i + 1,
					Quantity = 1,
					IsPreSelected = ladiesJacketSizes[i] == "XS"
				});
			}
			entities.Add(attrLadiesJacketSize);

			#endregion

			#region Fashion - Clark Jeans

			var productClarkJeans = _ctx.Set<Product>().First(x => x.Sku == "Fashion-65986524");
			var clarkJeansWidth = new string[] { "31", "32", "33", "34", "35", "36", "38", "40", "42", "44", "46" };
			var clarkJeansLength = new string[] { "30", "32", "34" };

			var attrClarkJeansWidth = new ProductVariantAttribute
			{
				Product = productClarkJeans,
				ProductAttribute = attrWidth,
				IsRequired = true,
				DisplayOrder = 1,
				AttributeControlType = AttributeControlType.Boxes
			};

			for (var i = 0; i < clarkJeansWidth.Length; ++i)
			{
				attrClarkJeansWidth.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue
				{
					Name = clarkJeansWidth[i],
					Alias = clarkJeansWidth[i],
					DisplayOrder = i + 1,
					Quantity = 1,
					IsPreSelected = clarkJeansWidth[i] == "31"
				});
			}
			entities.Add(attrClarkJeansWidth);

			var attrClarkJeansLength = new ProductVariantAttribute
			{
				Product = productClarkJeans,
				ProductAttribute = attrLength,
				IsRequired = true,
				DisplayOrder = 2,
				AttributeControlType = AttributeControlType.Boxes
			};

			for (var i = 0; i < clarkJeansLength.Length; ++i)
			{
				attrClarkJeansLength.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue
				{
					Name = clarkJeansLength[i],
					Alias = clarkJeansLength[i],
					DisplayOrder = i + 1,
					Quantity = 1,
					IsPreSelected = clarkJeansLength[i] == "30"
				});
			}
			entities.Add(attrClarkJeansLength);
            
            #endregion Fashion - Clark Jeans

            #region Furniture - Le Corbusier LC 6 table

            var productCorbusierTable = _ctx.Set<Product>().First(x => x.Sku == "Furniture-lc6");

			var attrCorbusierTablePlate = new ProductVariantAttribute
			{
				Product = productCorbusierTable,
				ProductAttribute = attrPlate,
				IsRequired = true,
				DisplayOrder = 1,
				AttributeControlType = AttributeControlType.Boxes
			};
			attrCorbusierTablePlate.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue
			{
				Name = "Clear glass",
				Alias = "clear-glass",
				DisplayOrder = 1,
				Quantity = 1,
				IsPreSelected = true
			});
			attrCorbusierTablePlate.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue
			{
				Name = "Sandblasted glass",
				Alias = "sandblasted-glass",
				DisplayOrder = 2,
				Quantity = 1
			});
			entities.Add(attrCorbusierTablePlate);

			var attrCorbusierTableThickness = new ProductVariantAttribute
			{
				Product = productCorbusierTable,
				ProductAttribute = attrPlateThickness,
				IsRequired = true,
				DisplayOrder = 2,
				AttributeControlType = AttributeControlType.Boxes
			};
			attrCorbusierTableThickness.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue
			{
				Name = "15 mm",
				Alias = "15mm",
				DisplayOrder = 1,
				Quantity = 1,
				IsPreSelected = true
			});
			attrCorbusierTableThickness.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue
			{
				Name = "19 mm",
				Alias = "19mm",
				DisplayOrder = 2,
				Quantity = 1
			});
			entities.Add(attrCorbusierTableThickness);

			#endregion

            #region Soccer Adidas TANGO SALA BALL

            var productAdidasTANGOSALABALL = _ctx.Set<Product>().First(x => x.Sku == "P-5001");
            var productAdidasTANGOSALABALLSizes = new string[] { "3", "4", "5" };
            var productAdidasTANGOSALABALLColors = new[]
            {
                new { Color = "Red", Code = "#ff0000" },
                new { Color = "Yellow", Code = " #ffff00" },
                new { Color = "Green", Code = "#008000" },
                new { Color = "Blue", Code = "#0000ff" },
                new { Color = "Gray", Code = "#808080" },
                new { Color = "White", Code = "#ffffff" },
                new { Color = "Brown", Code = "#a52a2a" }
            };

            var attrAdidasTANGOSALABALLColor = new ProductVariantAttribute
            {
                Product = productAdidasTANGOSALABALL,
                ProductAttribute = attrColor,
                IsRequired = true,
                DisplayOrder = 1,
                AttributeControlType = AttributeControlType.Boxes
            };

            for (var i = 0; i < productAdidasTANGOSALABALLColors.Length; ++i)
            {
                attrAdidasTANGOSALABALLColor.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue
                {
                    Name = productAdidasTANGOSALABALLColors[i].Color,
                    Alias = productAdidasTANGOSALABALLColors[i].Color.ToLower(),
                    DisplayOrder = i + 1,
                    Quantity = 1,
                    Color = productAdidasTANGOSALABALLColors[i].Code,
                    IsPreSelected = productAdidasTANGOSALABALLColors[i].Color == "White"
                });
            }
            entities.Add(attrAdidasTANGOSALABALLColor);

            var attrAdidasTANGOSALABALLSize = new ProductVariantAttribute
            {
                Product = productAdidasTANGOSALABALL,
                ProductAttribute = attrSize,
                IsRequired = true,
                DisplayOrder = 2,
                AttributeControlType = AttributeControlType.RadioList
            };

            for (var i = 0; i < productAdidasTANGOSALABALLSizes.Length; ++i)
            {
                attrAdidasTANGOSALABALLSize.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue
                {
                    Name = productAdidasTANGOSALABALLSizes[i],
                    Alias = productAdidasTANGOSALABALLSizes[i].ToLower(),
                    DisplayOrder = i + 1,
                    Quantity = 1,
                    IsPreSelected = productAdidasTANGOSALABALLSizes[i] == "5"
                });
            }
            entities.Add(attrAdidasTANGOSALABALLSize);

            #endregion Soccer Adidas TANGO SALA BALL

            #region Torfabrik official game ball

            var productTorfabrikBall = _ctx.Set<Product>().First(x => x.Sku == "P-5002");
            var productTorfabrikBallSizes = new string[] { "3", "4", "5" };
            var productTorfabrikBallColors = new[]
            {
                new { Color = "Red", Code = "#ff0000" },
                new { Color = "Yellow", Code = " #ffff00" },
                new { Color = "Green", Code = "#008000" },
                new { Color = "Blue", Code = "#0000ff" },
                new { Color = "White", Code = "#ffffff" },
            };

            var attrTorfabrikBallColor = new ProductVariantAttribute
            {
                Product = productTorfabrikBall,
                ProductAttribute = attrColor,
                IsRequired = true,
                DisplayOrder = 1,
                AttributeControlType = AttributeControlType.Boxes
            };

            for (var i = 0; i < productTorfabrikBallColors.Length; ++i)
            {
                attrTorfabrikBallColor.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue
                {
                    Name = productTorfabrikBallColors[i].Color,
                    Alias = productTorfabrikBallColors[i].Color.ToLower(),
                    DisplayOrder = i + 1,
                    Quantity = 1,
                    Color = productTorfabrikBallColors[i].Code,
                    IsPreSelected = productTorfabrikBallColors[i].Color == "White"
                });
            }
            entities.Add(attrTorfabrikBallColor);

            var attrTorfabrikSize = new ProductVariantAttribute
            {
                Product = productTorfabrikBall,
                ProductAttribute = attrSize,
                IsRequired = true,
                DisplayOrder = 2,
                AttributeControlType = AttributeControlType.RadioList
            };

            for (var i = 0; i < productTorfabrikBallSizes.Length; ++i)
            {
                attrTorfabrikSize.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue
                {
                    Name = productTorfabrikBallSizes[i],
                    Alias = productTorfabrikBallSizes[i].ToLower(),
                    DisplayOrder = i + 1,
                    Quantity = 1,
                    IsPreSelected = productTorfabrikBallSizes[i] == "5"
                });
            }
            entities.Add(attrTorfabrikSize);

            #endregion Soccer Torfabrik official game ball

			#region Furniture - Ball chair

			var productBallChair = _ctx.Set<Product>().First(x => x.Sku == "Furniture-ball-chair");
            
			var attrBallChairMaterial = new ProductVariantAttribute
			{
				Product = productBallChair,
				ProductAttribute = attrMaterial,
				IsRequired = true,
				DisplayOrder = 1,
				AttributeControlType = AttributeControlType.DropdownList
			};
			attrBallChairMaterial.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue
			{
				Name = "Leather Special",
				Alias = "leather-special",
				DisplayOrder = 1,
				Quantity = 1,
				IsPreSelected = true
			});
			attrBallChairMaterial.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue
			{
				Name = "Leather Aniline",
				Alias = "leather-aniline",
				DisplayOrder = 2,
				Quantity = 1
			});
			attrBallChairMaterial.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue
			{
				Name = "Mixed Linen",
				Alias = "mixed-linen",
				DisplayOrder = 3,
				Quantity = 1
			});
			entities.Add(attrBallChairMaterial);

			var attrBallChairColor = new ProductVariantAttribute
			{
				Product = productBallChair,
				ProductAttribute = attrColor,
				IsRequired = true,
				DisplayOrder = 2,
				AttributeControlType = AttributeControlType.Boxes
			};
			attrBallChairColor.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue
			{
				Name = "White",
				Alias = "white",
                Color = "#ffffff",
				DisplayOrder = 1,
				Quantity = 1,
				IsPreSelected = true
			});
			attrBallChairColor.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue
			{
				Name = "Black",
				Alias = "black",
                Color = "#000000",
                DisplayOrder = 2,
				Quantity = 1
			});
			entities.Add(attrBallChairColor);

			var attrBallChairLeatherColor = new ProductVariantAttribute
			{
				Product = productBallChair,
				ProductAttribute = attrLeatherColor,
				IsRequired = true,
				DisplayOrder = 3,
				AttributeControlType = AttributeControlType.Boxes
			};

			for (var i = 0; i < generalColors.Length; ++i)
			{
				attrBallChairLeatherColor.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue
				{
					Name = generalColors[i].Name,
					Alias = generalColors[i].Name.Replace(" ", "-").ToLower(),
					DisplayOrder = i + 1,
					Quantity = 1,
					Color = generalColors[i].Color,
					IsPreSelected = (generalColors[i].Name == "Tomato red")
				});
			}
			entities.Add(attrBallChairLeatherColor);

			#endregion

			#region Furniture - Lounge chair

			var productLoungeChair = _ctx.Set<Product>().First(x => x.Sku == "Furniture-lounge-chair");

			var attrLoungeChairMaterial = new ProductVariantAttribute
			{
				Product = productLoungeChair,
				ProductAttribute = attrMaterial,
				IsRequired = true,
				DisplayOrder = 1,
				AttributeControlType = AttributeControlType.DropdownList
			};
			attrLoungeChairMaterial.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue
			{
				Name = "Leather Special",
				Alias = "leather-special",
				DisplayOrder = 1,
				Quantity = 1,
				IsPreSelected = true
			});
			attrLoungeChairMaterial.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue
			{
				Name = "Leather Aniline",
				Alias = "leather-aniline",
				DisplayOrder = 2,
				Quantity = 1
			});
			entities.Add(attrLoungeChairMaterial);

			var loungeChairSeatShells = new string[] { "Palisander", "Cherry", "Walnut", "Wooden black lacquered" };
			var attrLoungeChairSeatShell = new ProductVariantAttribute
			{
				Product = productLoungeChair,
				ProductAttribute = attrSeatShell,
				IsRequired = true,
				DisplayOrder = 2,
				AttributeControlType = AttributeControlType.DropdownList
			};

			for (var i = 0; i < loungeChairSeatShells.Length; ++i)
			{
				attrLoungeChairSeatShell.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue
				{
					Name = loungeChairSeatShells[i],
					Alias = loungeChairSeatShells[i].Replace(" ", "-").ToLower(),
					DisplayOrder = i + 1,
					Quantity = 1,
					IsPreSelected = (i == 0),
					PriceAdjustment = (loungeChairSeatShells[i] == "Wooden black lacquered" ? 100.00M : decimal.Zero)
				});
			}
			entities.Add(attrLoungeChairSeatShell);

			var attrLoungeChairBase = new ProductVariantAttribute
			{
				Product = productLoungeChair,
				ProductAttribute = attrBase,
				IsRequired = true,
				DisplayOrder = 3,
				AttributeControlType = AttributeControlType.DropdownList
			};
			attrLoungeChairBase.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue
			{
				Name = "Top edge polished",
				Alias = "top-edge-polished",
				DisplayOrder = 1,
				Quantity = 1,
				IsPreSelected = true
			});
			attrLoungeChairBase.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue
			{
				Name = "Completely polished",
				Alias = "completely-polished",
				DisplayOrder = 2,
				Quantity = 1,
				PriceAdjustment = 150.00M
			});
			entities.Add(attrLoungeChairBase);

			var attrLoungeChairLeatherColor = new ProductVariantAttribute
			{
				Product = productLoungeChair,
				ProductAttribute = attrLeatherColor,
				IsRequired = true,
				DisplayOrder = 4,
				AttributeControlType = AttributeControlType.Boxes
			};

			for (var i = 0; i < generalColors.Length; ++i)
			{
				attrLoungeChairLeatherColor.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue
				{
					Name = generalColors[i].Name,
					Alias = generalColors[i].Name.Replace(" ", "-").ToLower(),
					DisplayOrder = i + 1,
					Quantity = 1,
					Color = generalColors[i].Color,
					IsPreSelected = (generalColors[i].Name == "White")
				});
			}
			entities.Add(attrLoungeChairLeatherColor);

			#endregion

			#region Furniture - Cube chair

			var productCubeChair = _ctx.Set<Product>().First(x => x.Sku == "Furniture-cube-chair");

			var attrCubeChairMaterial = new ProductVariantAttribute
			{
				Product = productCubeChair,
				ProductAttribute = attrMaterial,
				IsRequired = true,
				DisplayOrder = 1,
				AttributeControlType = AttributeControlType.DropdownList
			};
			attrCubeChairMaterial.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue
			{
				Name = "Leather Special",
				Alias = "leather-special",
				DisplayOrder = 1,
				Quantity = 1,
				IsPreSelected = true
			});
			attrCubeChairMaterial.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue
			{
				Name = "Leather Aniline",
				Alias = "leather-aniline",
				DisplayOrder = 2,
				Quantity = 1,
				PriceAdjustment = 400.00M
			});
			entities.Add(attrCubeChairMaterial);

			var attrCubeChairLeatherColor = new ProductVariantAttribute
			{
				Product = productCubeChair,
				ProductAttribute = attrLeatherColor,
				IsRequired = true,
				DisplayOrder = 2,
				AttributeControlType = AttributeControlType.Boxes
			};

			for (var i = 0; i < generalColors.Length; ++i)
			{
				attrCubeChairLeatherColor.ProductVariantAttributeValues.Add(new ProductVariantAttributeValue
				{
					Name = generalColors[i].Name,
					Alias = generalColors[i].Name.Replace(" ", "-").ToLower(),
					DisplayOrder = i + 1,
					Quantity = 1,
					Color = generalColors[i].Color,
					IsPreSelected = (generalColors[i].Name == "Black")
				});
			}
			entities.Add(attrCubeChairLeatherColor);

			#endregion

			this.Alter(entities);
			return entities;
		}

		public IList<ProductVariantAttributeCombination> ProductVariantAttributeCombinations()
		{
			var sb = new StringBuilder();
			var entities = new List<ProductVariantAttributeCombination>();
			var attrColor = _ctx.Set<ProductAttribute>().First(x => x.Alias == "color");
            var attrController = _ctx.Set<ProductAttribute>().First(x => x.Alias == "controller");
            var attrSize = _ctx.Set<ProductAttribute>().First(x => x.Alias == "size");
            var attrMemoryCapacity = _ctx.Set<ProductAttribute>().First(x => x.Alias == "memory-capacity");
            var attrColorIphoneColors = _ctx.Set<ProductAttribute>().First(x => x.Alias == "iphone-color");
            var attr97iPadColors = _ctx.Set<ProductAttribute>().First(x => x.Alias == "ipad-color");
			var attrPlate = _ctx.Set<ProductAttribute>().First(x => x.Alias == "plate");
			var attrPlateThickness = _ctx.Set<ProductAttribute>().First(x => x.Alias == "plate-thickness");
			var attrMaterial = _ctx.Set<ProductAttribute>().First(x => x.Alias == "material");
			var attrLeatherColor = _ctx.Set<ProductAttribute>().First(x => x.Alias == "leather-color");
			var attrSeatShell = _ctx.Set<ProductAttribute>().First(x => x.Alias == "seat-shell");
			var attrBase = _ctx.Set<ProductAttribute>().First(x => x.Alias == "base");
            var attrFlakLenstype = _ctx.Set<ProductAttribute>().First(x => x.Alias == "lenstype");
            var attrFlakFramecolor = _ctx.Set<ProductAttribute>().First(x => x.Alias == "framecolor");
            var attrFlakLenscolor = _ctx.Set<ProductAttribute>().First(x => x.Alias == "lenscolor");
            var attrWayfarerLenscolor = _ctx.Set<ProductAttribute>().First(x => x.Alias == "wayfarerlenscolor");
            var attrWayfarerFramecolor = _ctx.Set<ProductAttribute>().First(x => x.Alias == "wayfarerframecolor");


            #region ORIGINAL WAYFARER AT COLLECTION

            var productWayfarer = _ctx.Set<Product>().First(x => x.Sku == "P-3003");
            var wayfarerPictureIds = productWayfarer.ProductPictures.Select(pp => pp.PictureId).ToList();
            var picturesWayfarer = _ctx.Set<Picture>().Where(x => wayfarerPictureIds.Contains(x.Id)).ToList();

            //var attributeColorIphone7Plus = _ctx.Set<ProductVariantAttribute>().First(x => x.ProductId == productIphone7Plus.Id && x.ProductAttributeId == attrColor.Id);

            var wayfarerLenscolor = _ctx.Set<ProductVariantAttribute>().First(x => x.ProductId == productWayfarer.Id && x.ProductAttributeId == attrWayfarerLenscolor.Id);
            var wayfarerLenscolorValues = _ctx.Set<ProductVariantAttributeValue>().Where(x => x.ProductVariantAttributeId == wayfarerLenscolor.Id).ToList();

            var wayfarerFramecolor = _ctx.Set<ProductVariantAttribute>().First(x => x.ProductId == productWayfarer.Id && x.ProductAttributeId == attrWayfarerFramecolor.Id);
            var wayfarerFramecolorValues = _ctx.Set<ProductVariantAttributeValue>().Where(x => x.ProductVariantAttributeId == wayfarerFramecolor.Id).ToList();

            #region blue-gray-classic-black
            entities.Add(new ProductVariantAttributeCombination()
            {
                Product = productWayfarer,
                Sku = productWayfarer.Sku + "_blue-gray-classic-black",

                AttributesXml = FormatAttributeXml(
                    wayfarerLenscolor.Id, wayfarerLenscolorValues.First(x => x.Alias == "blue-gray-classic").Id,
                    wayfarerFramecolor.Id, wayfarerFramecolorValues.First(x => x.Alias == "rayban-black").Id),

                StockQuantity = 10000,
                AllowOutOfStockOrders = true,
                IsActive = true,
                //Price = 299M,
                AssignedPictureIds = picturesWayfarer.First(x => x.SeoFilename.EndsWith("_blue-gray-classic-black")).Id.ToString()
            });

            #endregion blue-gray-classic-black

            #region gray-course-black
            entities.Add(new ProductVariantAttributeCombination()
            {
                Product = productWayfarer,
                Sku = productWayfarer.Sku + "_gray-course-black",

                AttributesXml = FormatAttributeXml(
                    wayfarerLenscolor.Id, wayfarerLenscolorValues.First(x => x.Alias == "gray-course").Id,
                    wayfarerFramecolor.Id, wayfarerFramecolorValues.First(x => x.Alias == "rayban-black").Id),

                StockQuantity = 10000,
                AllowOutOfStockOrders = true,
                IsActive = true,
                //Price = 299M,
                AssignedPictureIds = picturesWayfarer.First(x => x.SeoFilename.EndsWith("_gray-course-black")).Id.ToString()
            });

            #endregion gray-course-black

            #region brown-course-havana
            entities.Add(new ProductVariantAttributeCombination()
            {
                Product = productWayfarer,
                Sku = productWayfarer.Sku + "_brown-course-havana",

                AttributesXml = FormatAttributeXml(
                    wayfarerLenscolor.Id, wayfarerLenscolorValues.First(x => x.Alias == "brown-course").Id,
                    wayfarerFramecolor.Id, wayfarerFramecolorValues.First(x => x.Alias == "havana").Id),

                StockQuantity = 10000,
                AllowOutOfStockOrders = true,
                IsActive = true,
                //Price = 299M,
                AssignedPictureIds = picturesWayfarer.First(x => x.SeoFilename.EndsWith("_gray-course-black")).Id.ToString()
            });

            #endregion brown-course-havana

            #region green-classic-havana-black
            entities.Add(new ProductVariantAttributeCombination()
            {
                Product = productWayfarer,
                Sku = productWayfarer.Sku + "_green-classic-havana-black",

                AttributesXml = FormatAttributeXml(
                    wayfarerLenscolor.Id, wayfarerLenscolorValues.First(x => x.Alias == "green-classic").Id,
                    wayfarerFramecolor.Id, wayfarerFramecolorValues.First(x => x.Alias == "havana-black").Id),

                StockQuantity = 10000,
                AllowOutOfStockOrders = true,
                IsActive = true,
                //Price = 299M,
                AssignedPictureIds = picturesWayfarer.First(x => x.SeoFilename.EndsWith("_green-classic-havana-black")).Id.ToString()
            });

            #endregion green-classic-havana-black

            // not available products not available products not available products not available products not available products

            #region blue-gray-classic-havana-black
            entities.Add(new ProductVariantAttributeCombination()
            {
                Product = productWayfarer,
                Sku = productWayfarer.Sku + "_blue-gray-classic-havana-black",

                AttributesXml = FormatAttributeXml(
                    wayfarerLenscolor.Id, wayfarerLenscolorValues.First(x => x.Alias == "blue-gray-classic").Id,
                    wayfarerFramecolor.Id, wayfarerFramecolorValues.First(x => x.Alias == "havana-black").Id),

                StockQuantity = 0,
                AllowOutOfStockOrders = true,
                IsActive = false,
                //Price = 299M,
                AssignedPictureIds = picturesWayfarer.First(x => x.SeoFilename.EndsWith("_green-classic-havana-black")).Id.ToString()
            });

            #endregion green-classic-havana-black

            #region blue-gray-classic-havana
            entities.Add(new ProductVariantAttributeCombination()
            {
                Product = productWayfarer,
                Sku = productWayfarer.Sku + "_blue-gray-classic-havana",

                AttributesXml = FormatAttributeXml(
                    wayfarerLenscolor.Id, wayfarerLenscolorValues.First(x => x.Alias == "blue-gray-classic").Id,
                    wayfarerFramecolor.Id, wayfarerFramecolorValues.First(x => x.Alias == "havana").Id),

                StockQuantity = 0,
                AllowOutOfStockOrders = true,
                IsActive = false,
                //Price = 299M,
                AssignedPictureIds = picturesWayfarer.First(x => x.SeoFilename.EndsWith("_green-classic-havana-black")).Id.ToString()
            });

            #endregion green-classic-rayban-black

            // gray-course
            #region gray-course-havana-black
            entities.Add(new ProductVariantAttributeCombination()
            {
                Product = productWayfarer,
                Sku = productWayfarer.Sku + "_gray-course-havana-black",

                AttributesXml = FormatAttributeXml(
                    wayfarerLenscolor.Id, wayfarerLenscolorValues.First(x => x.Alias == "gray-course").Id,
                    wayfarerFramecolor.Id, wayfarerFramecolorValues.First(x => x.Alias == "havana-black").Id),

                StockQuantity = 0,
                AllowOutOfStockOrders = true,
                IsActive = true,
                //Price = 299M,
                AssignedPictureIds = picturesWayfarer.First(x => x.SeoFilename.EndsWith("_green-classic-havana-black")).Id.ToString()
            });

            #endregion gray-course-havana-black
            
            #region gray-course-havana
            entities.Add(new ProductVariantAttributeCombination()
            {
                Product = productWayfarer,
                Sku = productWayfarer.Sku + "_gray-course-havana",

                AttributesXml = FormatAttributeXml(
                    wayfarerLenscolor.Id, wayfarerLenscolorValues.First(x => x.Alias == "gray-course").Id,
                    wayfarerFramecolor.Id, wayfarerFramecolorValues.First(x => x.Alias == "havana").Id),

                StockQuantity = 0,
                AllowOutOfStockOrders = true,
                IsActive = false,
                //Price = 299M,
                AssignedPictureIds = picturesWayfarer.First(x => x.SeoFilename.EndsWith("_green-classic-havana-black")).Id.ToString()
            });

            #endregion gray-course-rayban-black

            #region green-classic-rayban-black
            entities.Add(new ProductVariantAttributeCombination()
            {
                Product = productWayfarer,
                Sku = productWayfarer.Sku + "_green-classic-rayban-black",

                AttributesXml = FormatAttributeXml(
                    wayfarerLenscolor.Id, wayfarerLenscolorValues.First(x => x.Alias == "green-classic").Id,
                    wayfarerFramecolor.Id, wayfarerFramecolorValues.First(x => x.Alias == "rayban-black").Id),

                StockQuantity = 0,
                AllowOutOfStockOrders = true,
                IsActive = false,
                //Price = 299M,
                AssignedPictureIds = picturesWayfarer.First(x => x.SeoFilename.EndsWith("_green-classic-havana-black")).Id.ToString()
            });

            #endregion green-classic-rayban-black

            #region green-classic-havana
            entities.Add(new ProductVariantAttributeCombination()
            {
                Product = productWayfarer,
                Sku = productWayfarer.Sku + "_green-classic-havana",

                AttributesXml = FormatAttributeXml(
                    wayfarerLenscolor.Id, wayfarerLenscolorValues.First(x => x.Alias == "green-classic").Id,
                    wayfarerFramecolor.Id, wayfarerFramecolorValues.First(x => x.Alias == "havana").Id),

                StockQuantity = 0,
                AllowOutOfStockOrders = true,
                IsActive = false,
                //Price = 299M,
                AssignedPictureIds = picturesWayfarer.First(x => x.SeoFilename.EndsWith("_green-classic-havana-black")).Id.ToString()
            });

            #endregion gray-course-rayban-black

            // brown-course
            #region brown-course-havana-black
            entities.Add(new ProductVariantAttributeCombination()
            {
                Product = productWayfarer,
                Sku = productWayfarer.Sku + "_brown-course-havana-black",

                AttributesXml = FormatAttributeXml(
                    wayfarerLenscolor.Id, wayfarerLenscolorValues.First(x => x.Alias == "brown-course").Id,
                    wayfarerFramecolor.Id, wayfarerFramecolorValues.First(x => x.Alias == "havana-black").Id),

                StockQuantity = 0,
                AllowOutOfStockOrders = true,
                IsActive = false,
                //Price = 299M,
                AssignedPictureIds = picturesWayfarer.First(x => x.SeoFilename.EndsWith("_green-classic-havana-black")).Id.ToString()
            });

            #endregion brown-course-havana-black

            #region brown-course-rayban-black
            entities.Add(new ProductVariantAttributeCombination()
            {
                Product = productWayfarer,
                Sku = productWayfarer.Sku + "_brown-course-rayban-black",

                AttributesXml = FormatAttributeXml(
                    wayfarerLenscolor.Id, wayfarerLenscolorValues.First(x => x.Alias == "brown-course").Id,
                    wayfarerFramecolor.Id, wayfarerFramecolorValues.First(x => x.Alias == "rayban-black").Id),

                StockQuantity = 0,
                AllowOutOfStockOrders = true,
                IsActive = false,
                //Price = 299M,
                AssignedPictureIds = picturesWayfarer.First(x => x.SeoFilename.EndsWith("_green-classic-havana-black")).Id.ToString()
            });

            #endregion brown-course-rayban-black

            #endregion ORIGINAL WAYFARER AT COLLECTION

            #region Custom Flak

            var productFlak = _ctx.Set<Product>().First(x => x.Sku == "P-3002");
            var flakPictureIds = productFlak.ProductPictures.Select(pp => pp.PictureId).ToList();
            var picturesFlak = _ctx.Set<Picture>().Where(x => flakPictureIds.Contains(x.Id)).ToList();

            //var attributeColorIphone7Plus = _ctx.Set<ProductVariantAttribute>().First(x => x.ProductId == productIphone7Plus.Id && x.ProductAttributeId == attrColor.Id);

            var flakLenscolor = _ctx.Set<ProductVariantAttribute>().First(x => x.ProductId == productFlak.Id && x.ProductAttributeId == attrFlakLenscolor.Id);
            var flakLenscolorValues = _ctx.Set<ProductVariantAttributeValue>().Where(x => x.ProductVariantAttributeId == flakLenscolor.Id).ToList();

            var flakLenstype = _ctx.Set<ProductVariantAttribute>().First(x => x.ProductId == productFlak.Id && x.ProductAttributeId == attrFlakLenstype.Id);
            var flakLenstypeValues = _ctx.Set<ProductVariantAttributeValue>().Where(x => x.ProductVariantAttributeId == flakLenstype.Id).ToList();

            var flakFramecolor = _ctx.Set<ProductVariantAttribute>().First(x => x.ProductId == productFlak.Id && x.ProductAttributeId == attrFlakFramecolor.Id);
            var flakFramecolorValues = _ctx.Set<ProductVariantAttributeValue>().Where(x => x.ProductVariantAttributeId == flakFramecolor.Id).ToList();

            //#region matteblack-gray-standard

            foreach (var lenscolorValue in flakLenscolorValues)
            {
                foreach (var framecolorValue in flakFramecolorValues)
                {

                    foreach (var lenstypeValue in flakLenstypeValues)
                    {
                        try { 
                            entities.Add(new ProductVariantAttributeCombination
                                {
                                    Product = productFlak,
                                    Sku = productFlak.Sku + string.Concat("-", framecolorValue.Alias, "-", lenscolorValue.Alias, "-",lenstypeValue.Alias),
                                    AttributesXml = FormatAttributeXml(flakLenscolor.Id, lenscolorValue.Id, flakLenstype.Id, lenstypeValue.Id, flakFramecolor.Id, framecolorValue.Id),
                                    StockQuantity = 10000,
                                    AllowOutOfStockOrders = true,
                                    IsActive = true,
                            
                                    AssignedPictureIds = picturesFlak.First(x => x.SeoFilename.Contains(framecolorValue.Alias + "_" + lenscolorValue.Alias)).Id.ToString(),
                            
                                    //Price = ballChairPrice
                            });
                        } 
                        catch
                        {
                            Console.WriteLine("An error occurred: '{0}'", framecolorValue.Alias + "_" + lenscolorValue.Alias);
                        }
                    }
                }
            }


            

            #endregion Custom Flak

            #region ps3

            var productPs3 = _ctx.Set<Product>().First(x => x.Sku == "Sony-PS399000");
			var ps3PictureIds = productPs3.ProductPictures.Select(pp => pp.PictureId).ToList();
			var picturesPs3 = _ctx.Set<Picture>().Where(x => ps3PictureIds.Contains(x.Id)).ToList();

			var productAttributeColor = _ctx.Set<ProductVariantAttribute>().First(x => x.ProductId == productPs3.Id && x.ProductAttributeId == attrController.Id);
			var attributeColorValues = _ctx.Set<ProductVariantAttributeValue>().Where(x => x.ProductVariantAttributeId == productAttributeColor.Id).ToList();

			entities.Add(new ProductVariantAttributeCombination()
			{
				Product = productPs3,
				Sku = productPs3.Sku + "-B",
				AttributesXml = FormatAttributeXml(productAttributeColor.Id, attributeColorValues.First(x => x.Alias == "with_controller").Id),
				StockQuantity = 10000,
				AllowOutOfStockOrders = true,
				IsActive = true,
				AssignedPictureIds = picturesPs3.First(x => x.SeoFilename.EndsWith("-controller")).Id.ToString()
			});

			entities.Add(new ProductVariantAttributeCombination()
			{
				Product = productPs3,
				Sku = productPs3.Sku + "-W",
				AttributesXml = FormatAttributeXml(productAttributeColor.Id, attributeColorValues.First(x => x.Alias == "without_controller").Id),
				StockQuantity = 10000,
				AllowOutOfStockOrders = true,
				IsActive = true,
				AssignedPictureIds = picturesPs3.First(x => x.SeoFilename.EndsWith("-single")).Id.ToString()
			});

            #endregion ps3

            #region Apple Airpod

            var productAirpod = _ctx.Set<Product>().First(x => x.Sku == "P-2003");
            var airpodPictureIds = productAirpod.ProductPictures.Select(pp => pp.PictureId).ToList();
            var picturesAirpod = _ctx.Set<Picture>().Where(x => airpodPictureIds.Contains(x.Id)).ToList();

            var airpodAttributeColor = _ctx.Set<ProductVariantAttribute>().First(x => x.ProductId == productAirpod.Id && x.ProductAttributeId == attrColor.Id);
            var airpodAttributeColorValues = _ctx.Set<ProductVariantAttributeValue>().Where(x => x.ProductVariantAttributeId == airpodAttributeColor.Id).ToList();

            entities.Add(new ProductVariantAttributeCombination()
            {
                Product = productAirpod,
                Sku = productAirpod.Sku + "-gold",
                AttributesXml = FormatAttributeXml(airpodAttributeColor.Id, airpodAttributeColorValues.First(x => x.Alias == "gold").Id),
                StockQuantity = 10000,
                AllowOutOfStockOrders = true,
                IsActive = true,
                AssignedPictureIds = picturesAirpod.First(x => x.SeoFilename.EndsWith("-gold")).Id.ToString()
            });

            entities.Add(new ProductVariantAttributeCombination()
            {
                Product = productAirpod,
                Sku = productAirpod.Sku + "-rose",
                AttributesXml = FormatAttributeXml(airpodAttributeColor.Id, airpodAttributeColorValues.First(x => x.Alias == "rose").Id),
                StockQuantity = 10000,
                AllowOutOfStockOrders = true,
                IsActive = true,
                AssignedPictureIds = picturesAirpod.First(x => x.SeoFilename.EndsWith("-rose")).Id.ToString()
            });
            entities.Add(new ProductVariantAttributeCombination()
            {
                Product = productAirpod,
                Sku = productAirpod.Sku + "-mint",
                AttributesXml = FormatAttributeXml(airpodAttributeColor.Id, airpodAttributeColorValues.First(x => x.Alias == "mint").Id),
                StockQuantity = 10000,
                AllowOutOfStockOrders = true,
                IsActive = true,
                AssignedPictureIds = picturesAirpod.First(x => x.SeoFilename.EndsWith("-mint")).Id.ToString()
            });
            entities.Add(new ProductVariantAttributeCombination()
            {
                Product = productAirpod,
                Sku = productAirpod.Sku + "-lightblue",
                AttributesXml = FormatAttributeXml(airpodAttributeColor.Id, airpodAttributeColorValues.First(x => x.Alias == "lightblue").Id),
                StockQuantity = 10000,
                AllowOutOfStockOrders = true,
                IsActive = true,
                AssignedPictureIds = picturesAirpod.First(x => x.SeoFilename.EndsWith("-lightblue")).Id.ToString()
            });
            entities.Add(new ProductVariantAttributeCombination()
            {
                Product = productAirpod,
                Sku = productAirpod.Sku + "-turquoise",
                AttributesXml = FormatAttributeXml(airpodAttributeColor.Id, airpodAttributeColorValues.First(x => x.Alias == "turquoise").Id),
                StockQuantity = 10000,
                AllowOutOfStockOrders = true,
                IsActive = true,
                AssignedPictureIds = picturesAirpod.First(x => x.SeoFilename.EndsWith("-turquoise")).Id.ToString()
            });
            entities.Add(new ProductVariantAttributeCombination()
            {
                Product = productAirpod,
                Sku = productAirpod.Sku + "-white",
                AttributesXml = FormatAttributeXml(airpodAttributeColor.Id, airpodAttributeColorValues.First(x => x.Alias == "white").Id),
                StockQuantity = 10000,
                AllowOutOfStockOrders = true,
                IsActive = true,
                AssignedPictureIds = picturesAirpod.First(x => x.SeoFilename.EndsWith("-white")).Id.ToString()
            });

            #endregion Apple Airpod

            #region 9,7 Ipad

            var productiPad97 = _ctx.Set<Product>().First(x => x.Sku == "P-2004");
            var iPad97PictureIds = productiPad97.ProductPictures.Select(pp => pp.PictureId).ToList();
            var picturesiPad97 = _ctx.Set<Picture>().Where(x => iPad97PictureIds.Contains(x.Id)).ToList();

            //var attributeColorIphone7Plus = _ctx.Set<ProductVariantAttribute>().First(x => x.ProductId == productIphone7Plus.Id && x.ProductAttributeId == attrColor.Id);

            var iPad97Color = _ctx.Set<ProductVariantAttribute>().First(x => x.ProductId == productiPad97.Id && x.ProductAttributeId == attr97iPadColors.Id);
            var iPad97ColorValues = _ctx.Set<ProductVariantAttributeValue>().Where(x => x.ProductVariantAttributeId == iPad97Color.Id).ToList();

            var ipad97Capacity = _ctx.Set<ProductVariantAttribute>().First(x => x.ProductId == productiPad97.Id && x.ProductAttributeId == attrMemoryCapacity.Id);
            var iPad97CapacityValues = _ctx.Set<ProductVariantAttributeValue>().Where(x => x.ProductVariantAttributeId == ipad97Capacity.Id).ToList();

            #region silver
            entities.Add(new ProductVariantAttributeCombination()
            {
                Product = productiPad97,
                Sku = productiPad97.Sku + "-silver-64gb",

                AttributesXml = FormatAttributeXml(
                    iPad97Color.Id, iPad97ColorValues.First(x => x.Alias == "silver").Id,
                    ipad97Capacity.Id, iPad97CapacityValues.First(x => x.Alias == "64gb").Id),

                StockQuantity = 10000,
                AllowOutOfStockOrders = true,
                IsActive = true,
                Price = 299M,
                AssignedPictureIds = picturesiPad97.First(x => x.SeoFilename.EndsWith("-silver")).Id.ToString()
            });

            entities.Add(new ProductVariantAttributeCombination()
            {
                Product = productiPad97,
                Sku = productiPad97.Sku + "silver-128gb",

                AttributesXml = FormatAttributeXml(
                    iPad97Color.Id, iPad97ColorValues.First(x => x.Alias == "silver").Id,
                    ipad97Capacity.Id, iPad97CapacityValues.First(x => x.Alias == "128gb").Id),

                StockQuantity = 10000,
                AllowOutOfStockOrders = true,
                IsActive = true,
                AssignedPictureIds = picturesiPad97.First(x => x.SeoFilename.EndsWith("-silver")).Id.ToString()
            });

            #endregion silver

            #region gold
            entities.Add(new ProductVariantAttributeCombination()
            {
                Product = productiPad97,
                Sku = productiPad97.Sku + "-gold-64gb",

                AttributesXml = FormatAttributeXml(
                    iPad97Color.Id, iPad97ColorValues.First(x => x.Alias == "gold").Id,
                    ipad97Capacity.Id, iPad97CapacityValues.First(x => x.Alias == "64gb").Id),
                Price = 279M,
                StockQuantity = 10000,
                AllowOutOfStockOrders = true,
                IsActive = true,
                AssignedPictureIds = picturesiPad97.First(x => x.SeoFilename.EndsWith("-gold")).Id.ToString()
            });

            entities.Add(new ProductVariantAttributeCombination()
            {
                Product = productiPad97,
                Sku = productiPad97.Sku + "gold-128gb",

                AttributesXml = FormatAttributeXml(
                    iPad97Color.Id, iPad97ColorValues.First(x => x.Alias == "gold").Id,
                    ipad97Capacity.Id, iPad97CapacityValues.First(x => x.Alias == "128gb").Id),

                StockQuantity = 10000,
                AllowOutOfStockOrders = true,
                IsActive = true,
                AssignedPictureIds = picturesiPad97.First(x => x.SeoFilename.EndsWith("-gold")).Id.ToString()
            });
            #endregion gold

            #region spacegray
            entities.Add(new ProductVariantAttributeCombination()
            {
                Product = productiPad97,
                Sku = productiPad97.Sku + "-spacegray-64gb",

                AttributesXml = FormatAttributeXml(
                    iPad97Color.Id, iPad97ColorValues.First(x => x.Alias == "spacegray").Id,
                    ipad97Capacity.Id, iPad97CapacityValues.First(x => x.Alias == "64gb").Id),

                StockQuantity = 10000,
                AllowOutOfStockOrders = true,
                IsActive = true,
                AssignedPictureIds = picturesiPad97.First(x => x.SeoFilename.EndsWith("-spacegray")).Id.ToString()
            });

            entities.Add(new ProductVariantAttributeCombination()
            {
                Product = productiPad97,
                Sku = productiPad97.Sku + "spacegray-128gb",

                AttributesXml = FormatAttributeXml(
                    iPad97Color.Id, iPad97ColorValues.First(x => x.Alias == "spacegray").Id,
                    ipad97Capacity.Id, iPad97CapacityValues.First(x => x.Alias == "128gb").Id),

                StockQuantity = 10000,
                AllowOutOfStockOrders = true,
                IsActive = true,
                AssignedPictureIds = picturesiPad97.First(x => x.SeoFilename.EndsWith("-spacegray")).Id.ToString()
            });
            #endregion spacegray

            #region rose
            entities.Add(new ProductVariantAttributeCombination()
            {
                Product = productiPad97,
                Sku = productiPad97.Sku + "-rose-64gb",

                AttributesXml = FormatAttributeXml(
                    iPad97Color.Id, iPad97ColorValues.First(x => x.Alias == "rose").Id,
                    ipad97Capacity.Id, iPad97CapacityValues.First(x => x.Alias == "64gb").Id),

                StockQuantity = 10000,
                AllowOutOfStockOrders = true,
                IsActive = true,
                AssignedPictureIds = picturesiPad97.First(x => x.SeoFilename.EndsWith("-rose")).Id.ToString()
            });

            entities.Add(new ProductVariantAttributeCombination()
            {
                Product = productiPad97,
                Sku = productiPad97.Sku + "rose-128gb",

                AttributesXml = FormatAttributeXml(
                    iPad97Color.Id, iPad97ColorValues.First(x => x.Alias == "rose").Id,
                    ipad97Capacity.Id, iPad97CapacityValues.First(x => x.Alias == "128gb").Id),

                StockQuantity = 10000,
                AllowOutOfStockOrders = true,
                IsActive = true,
                AssignedPictureIds = picturesiPad97.First(x => x.SeoFilename.EndsWith("-rose")).Id.ToString()
            });
            #endregion rose

            #region mint
            entities.Add(new ProductVariantAttributeCombination()
            {
                Product = productiPad97,
                Sku = productiPad97.Sku + "-mint-64gb",

                AttributesXml = FormatAttributeXml(
                    iPad97Color.Id, iPad97ColorValues.First(x => x.Alias == "mint").Id,
                    ipad97Capacity.Id, iPad97CapacityValues.First(x => x.Alias == "64gb").Id),

                StockQuantity = 10000,
                AllowOutOfStockOrders = true,
                IsActive = true,
                AssignedPictureIds = picturesiPad97.First(x => x.SeoFilename.EndsWith("-mint")).Id.ToString()
            });

            entities.Add(new ProductVariantAttributeCombination()
            {
                Product = productiPad97,
                Sku = productiPad97.Sku + "mint-128gb",

                AttributesXml = FormatAttributeXml(
                    iPad97Color.Id, iPad97ColorValues.First(x => x.Alias == "mint").Id,
                    ipad97Capacity.Id, iPad97CapacityValues.First(x => x.Alias == "128gb").Id),

                StockQuantity = 10000,
                AllowOutOfStockOrders = true,
                IsActive = true,
                AssignedPictureIds = picturesiPad97.First(x => x.SeoFilename.EndsWith("-mint")).Id.ToString()
            });
            #endregion mint

            #region purple
            entities.Add(new ProductVariantAttributeCombination()
            {
                Product = productiPad97,
                Sku = productiPad97.Sku + "-purple-64gb",

                AttributesXml = FormatAttributeXml(
                    iPad97Color.Id, iPad97ColorValues.First(x => x.Alias == "purple").Id,
                    ipad97Capacity.Id, iPad97CapacityValues.First(x => x.Alias == "64gb").Id),

                StockQuantity = 10000,
                AllowOutOfStockOrders = true,
                IsActive = true,
                AssignedPictureIds = picturesiPad97.First(x => x.SeoFilename.EndsWith("-purple")).Id.ToString()
            });

            entities.Add(new ProductVariantAttributeCombination()
            {
                Product = productiPad97,
                Sku = productiPad97.Sku + "purple-128gb",

                AttributesXml = FormatAttributeXml(
                    iPad97Color.Id, iPad97ColorValues.First(x => x.Alias == "purple").Id,
                    ipad97Capacity.Id, iPad97CapacityValues.First(x => x.Alias == "128gb").Id),

                StockQuantity = 10000,
                AllowOutOfStockOrders = true,
                IsActive = true,
                AssignedPictureIds = picturesiPad97.First(x => x.SeoFilename.EndsWith("-purple")).Id.ToString()
            });
            #endregion purple

            #region lightblue
            entities.Add(new ProductVariantAttributeCombination()
            {
                Product = productiPad97,
                Sku = productiPad97.Sku + "-lightblue-64gb",

                AttributesXml = FormatAttributeXml(
                    iPad97Color.Id, iPad97ColorValues.First(x => x.Alias == "lightblue").Id,
                    ipad97Capacity.Id, iPad97CapacityValues.First(x => x.Alias == "64gb").Id),

                StockQuantity = 10000,
                AllowOutOfStockOrders = true,
                IsActive = true,
                AssignedPictureIds = picturesiPad97.First(x => x.SeoFilename.EndsWith("-lightblue")).Id.ToString()
            });

            entities.Add(new ProductVariantAttributeCombination()
            {
                Product = productiPad97,
                Sku = productiPad97.Sku + "lightblue-128gb",

                AttributesXml = FormatAttributeXml(
                    iPad97Color.Id, iPad97ColorValues.First(x => x.Alias == "lightblue").Id,
                    ipad97Capacity.Id, iPad97CapacityValues.First(x => x.Alias == "128gb").Id),

                StockQuantity = 10000,
                AllowOutOfStockOrders = true,
                IsActive = true,
                AssignedPictureIds = picturesiPad97.First(x => x.SeoFilename.EndsWith("-lightblue")).Id.ToString()
            });
            #endregion lightblue

            #region yellow
            entities.Add(new ProductVariantAttributeCombination()
            {
                Product = productiPad97,
                Sku = productiPad97.Sku + "-yellow-64gb",

                AttributesXml = FormatAttributeXml(
                    iPad97Color.Id, iPad97ColorValues.First(x => x.Alias == "yellow").Id,
                    ipad97Capacity.Id, iPad97CapacityValues.First(x => x.Alias == "64gb").Id),

                StockQuantity = 10000,
                AllowOutOfStockOrders = true,
                IsActive = true,
                AssignedPictureIds = picturesiPad97.First(x => x.SeoFilename.EndsWith("-yellow")).Id.ToString()
            });

            entities.Add(new ProductVariantAttributeCombination()
            {
                Product = productiPad97,
                Sku = productiPad97.Sku + "yellow-128gb",

                AttributesXml = FormatAttributeXml(
                    iPad97Color.Id, iPad97ColorValues.First(x => x.Alias == "yellow").Id,
                    ipad97Capacity.Id, iPad97CapacityValues.First(x => x.Alias == "128gb").Id),

                StockQuantity = 10000,
                AllowOutOfStockOrders = true,
                IsActive = true,
                AssignedPictureIds = picturesiPad97.First(x => x.SeoFilename.EndsWith("-yellow")).Id.ToString()
            });
            #endregion yellow

            #region turquoise
            entities.Add(new ProductVariantAttributeCombination()
            {
                Product = productiPad97,
                Sku = productiPad97.Sku + "-turquoise-64gb",

                AttributesXml = FormatAttributeXml(
                    iPad97Color.Id, iPad97ColorValues.First(x => x.Alias == "turquoise").Id,
                    ipad97Capacity.Id, iPad97CapacityValues.First(x => x.Alias == "64gb").Id),

                StockQuantity = 10000,
                AllowOutOfStockOrders = true,
                IsActive = true,
                AssignedPictureIds = picturesiPad97.First(x => x.SeoFilename.EndsWith("-turquoise")).Id.ToString()
            });

            entities.Add(new ProductVariantAttributeCombination()
            {
                Product = productiPad97,
                Sku = productiPad97.Sku + "turquoise-128gb",

                AttributesXml = FormatAttributeXml(
                    iPad97Color.Id, iPad97ColorValues.First(x => x.Alias == "turquoise").Id,
                    ipad97Capacity.Id, iPad97CapacityValues.First(x => x.Alias == "128gb").Id),

                StockQuantity = 10000,
                AllowOutOfStockOrders = true,
                IsActive = true,
                AssignedPictureIds = picturesiPad97.First(x => x.SeoFilename.EndsWith("-turquoise")).Id.ToString()
            });
            #endregion turquoise

            #endregion 9,7 Ipad

            #region Iphone 7 plus

            var productIphone7Plus = _ctx.Set<Product>().First(x => x.Sku == "P-2001");
            var Iphone7PlusPictureIds = productIphone7Plus.ProductPictures.Select(pp => pp.PictureId).ToList();
            var picturesIphone7Plus = _ctx.Set<Picture>().Where(x => Iphone7PlusPictureIds.Contains(x.Id)).ToList();

            //var attributeColorIphone7Plus = _ctx.Set<ProductVariantAttribute>().First(x => x.ProductId == productIphone7Plus.Id && x.ProductAttributeId == attrColor.Id);

            var Iphone7PlusColor = _ctx.Set<ProductVariantAttribute>().First(x => x.ProductId == productIphone7Plus.Id && x.ProductAttributeId == attrColorIphoneColors.Id);
            var Iphone7PlusColorValues = _ctx.Set<ProductVariantAttributeValue>().Where(x => x.ProductVariantAttributeId == Iphone7PlusColor.Id).ToList();

            var Iphone7PlusCapacity = _ctx.Set<ProductVariantAttribute>().First(x => x.ProductId == productIphone7Plus.Id && x.ProductAttributeId == attrMemoryCapacity.Id);
            var Iphone7PlusCapacityValues = _ctx.Set<ProductVariantAttributeValue>().Where(x => x.ProductVariantAttributeId == Iphone7PlusCapacity.Id).ToList();


            entities.Add(new ProductVariantAttributeCombination()
            {
                Product = productIphone7Plus,
                Sku = productIphone7Plus.Sku + "-black-64gb",

                AttributesXml = FormatAttributeXml(
                    Iphone7PlusColor.Id, Iphone7PlusColorValues.First(x => x.Alias == "black").Id,
                    Iphone7PlusCapacity.Id, Iphone7PlusCapacityValues.First(x => x.Alias == "64gb").Id),

                StockQuantity = 10000,
                AllowOutOfStockOrders = true,
                IsActive = true,
                AssignedPictureIds = picturesIphone7Plus.First(x => x.SeoFilename.EndsWith("-black")).Id.ToString()
            });

            entities.Add(new ProductVariantAttributeCombination()
            {
                Product = productIphone7Plus,
                Sku = productIphone7Plus.Sku + "-black-128gb",

                AttributesXml = FormatAttributeXml(
                    Iphone7PlusColor.Id, Iphone7PlusColorValues.First(x => x.Alias == "black").Id,
                    Iphone7PlusCapacity.Id, Iphone7PlusCapacityValues.First(x => x.Alias == "128gb").Id),

                StockQuantity = 10000,
                AllowOutOfStockOrders = true,
                IsActive = true,
                AssignedPictureIds = picturesIphone7Plus.First(x => x.SeoFilename.EndsWith("-black")).Id.ToString()
            });

            entities.Add(new ProductVariantAttributeCombination()
            {
                Product = productIphone7Plus,
                Sku = productIphone7Plus.Sku + "-red-64",
                AttributesXml = FormatAttributeXml(
                    Iphone7PlusColor.Id, Iphone7PlusColorValues.First(x => x.Alias == "red").Id,
                    Iphone7PlusCapacity.Id, Iphone7PlusCapacityValues.First(x => x.Alias == "64gb").Id),
                StockQuantity = 10000,
                AllowOutOfStockOrders = true,
                IsActive = true,
                AssignedPictureIds = picturesIphone7Plus.First(x => x.SeoFilename.EndsWith("-red")).Id.ToString()
            });

            entities.Add(new ProductVariantAttributeCombination()
            {
                Product = productIphone7Plus,
                Sku = productIphone7Plus.Sku + "-red-128",
                AttributesXml = FormatAttributeXml(
                    Iphone7PlusColor.Id, Iphone7PlusColorValues.First(x => x.Alias == "red").Id,
                    Iphone7PlusCapacity.Id, Iphone7PlusCapacityValues.First(x => x.Alias == "128gb").Id),
                StockQuantity = 10000,
                AllowOutOfStockOrders = true,
                IsActive = true,
                AssignedPictureIds = picturesIphone7Plus.First(x => x.SeoFilename.EndsWith("-red")).Id.ToString()
            });

            entities.Add(new ProductVariantAttributeCombination()
            {
                Product = productIphone7Plus,
                Sku = productIphone7Plus.Sku + "-silver-64",
                AttributesXml = FormatAttributeXml(
                    Iphone7PlusColor.Id, Iphone7PlusColorValues.First(x => x.Alias == "silver").Id,
                    Iphone7PlusCapacity.Id, Iphone7PlusCapacityValues.First(x => x.Alias == "64gb").Id),
                StockQuantity = 10000,
                AllowOutOfStockOrders = true,
                IsActive = true,
                AssignedPictureIds = picturesIphone7Plus.First(x => x.SeoFilename.EndsWith("-silver")).Id.ToString()
            });


            entities.Add(new ProductVariantAttributeCombination()
            {
                Product = productIphone7Plus,
                Sku = productIphone7Plus.Sku + "-silver-128",
                AttributesXml = FormatAttributeXml(
                    Iphone7PlusColor.Id, Iphone7PlusColorValues.First(x => x.Alias == "silver").Id,
                    Iphone7PlusCapacity.Id, Iphone7PlusCapacityValues.First(x => x.Alias == "128gb").Id),
                StockQuantity = 10000,
                AllowOutOfStockOrders = true,
                IsActive = true,
                AssignedPictureIds = picturesIphone7Plus.First(x => x.SeoFilename.EndsWith("-silver")).Id.ToString()
            });

            entities.Add(new ProductVariantAttributeCombination()
            {
                Product = productIphone7Plus,
                Sku = productIphone7Plus.Sku + "-rose-64",
                AttributesXml = FormatAttributeXml(
                    Iphone7PlusColor.Id, Iphone7PlusColorValues.First(x => x.Alias == "rose").Id,
                    Iphone7PlusCapacity.Id, Iphone7PlusCapacityValues.First(x => x.Alias == "64gb").Id),
                StockQuantity = 10000,
                AllowOutOfStockOrders = true,
                IsActive = true,
                AssignedPictureIds = picturesIphone7Plus.First(x => x.SeoFilename.EndsWith("-rose")).Id.ToString()
            });

            entities.Add(new ProductVariantAttributeCombination()
            {
                Product = productIphone7Plus,
                Sku = productIphone7Plus.Sku + "-rose-128",
                AttributesXml = FormatAttributeXml(
                    Iphone7PlusColor.Id, Iphone7PlusColorValues.First(x => x.Alias == "rose").Id,
                    Iphone7PlusCapacity.Id, Iphone7PlusCapacityValues.First(x => x.Alias == "128gb").Id),
                StockQuantity = 10000,
                AllowOutOfStockOrders = true,
                IsActive = true,
                AssignedPictureIds = picturesIphone7Plus.First(x => x.SeoFilename.EndsWith("-rose")).Id.ToString()
            });

            entities.Add(new ProductVariantAttributeCombination()
            {
                Product = productIphone7Plus,
                Sku = productIphone7Plus.Sku + "-gold-64",
                AttributesXml = FormatAttributeXml(
                    Iphone7PlusColor.Id, Iphone7PlusColorValues.First(x => x.Alias == "gold").Id,
                    Iphone7PlusCapacity.Id, Iphone7PlusCapacityValues.First(x => x.Alias == "64gb").Id),
                StockQuantity = 10000,
                AllowOutOfStockOrders = true,
                IsActive = true,
                AssignedPictureIds = picturesIphone7Plus.First(x => x.SeoFilename.EndsWith("-gold")).Id.ToString()
            });

            entities.Add(new ProductVariantAttributeCombination()
            {
                Product = productIphone7Plus,
                Sku = productIphone7Plus.Sku + "-gold-128",
                AttributesXml = FormatAttributeXml(
                    Iphone7PlusColor.Id, Iphone7PlusColorValues.First(x => x.Alias == "gold").Id,
                    Iphone7PlusCapacity.Id, Iphone7PlusCapacityValues.First(x => x.Alias == "128gb").Id),
                StockQuantity = 10000,
                AllowOutOfStockOrders = true,
                IsActive = true,
                AssignedPictureIds = picturesIphone7Plus.First(x => x.SeoFilename.EndsWith("-gold")).Id.ToString()
            });

            #endregion Iphone 7 plus

			#region Fashion - Converse All Star

			var productAllStar = _ctx.Set<Product>().First(x => x.Sku == "Fashion-112355");
			var allStarPictureIds = productAllStar.ProductPictures.Select(x => x.PictureId).ToList();
			var allStarPictures = _ctx.Set<Picture>().Where(x => allStarPictureIds.Contains(x.Id)).ToList();

			var allStarColor = _ctx.Set<ProductVariantAttribute>().First(x => x.ProductId == productAllStar.Id && x.ProductAttributeId == attrColor.Id);
			var allStarColorValues = _ctx.Set<ProductVariantAttributeValue>().Where(x => x.ProductVariantAttributeId == allStarColor.Id).ToList();

			var allStarSize = _ctx.Set<ProductVariantAttribute>().First(x => x.ProductId == productAllStar.Id && x.ProductAttributeId == attrSize.Id);
			var allStarSizeValues = _ctx.Set<ProductVariantAttributeValue>().Where(x => x.ProductVariantAttributeId == allStarSize.Id).ToList();

			var allStarCombinations = new[]
			{
				new { Color = "Charcoal", Size = "42" },
				new { Color = "Charcoal", Size = "43" },
				new { Color = "Charcoal", Size = "44" },
				new { Color = "Maroon", Size = "42" },
				new { Color = "Maroon", Size = "43" },
				new { Color = "Maroon", Size = "44" },
				new { Color = "Navy", Size = "42" },
				new { Color = "Navy", Size = "43" },
				new { Color = "Navy", Size = "44" },
				new { Color = "Purple", Size = "42" },
				new { Color = "Purple", Size = "43" },
				new { Color = "Purple", Size = "44" },
				new { Color = "White", Size = "42" },
				new { Color = "White", Size = "43" },
				new { Color = "White", Size = "44" },
			};

			foreach (var comb in allStarCombinations)
			{
				var lowerColor = comb.Color.ToLower();
				entities.Add(new ProductVariantAttributeCombination
				{
					Product = productAllStar,
					Sku = productAllStar.Sku + string.Concat("-", lowerColor, "-", comb.Size),
					AttributesXml = FormatAttributeXml(
						allStarColor.Id, allStarColorValues.First(x => x.Alias == lowerColor).Id,
						allStarSize.Id, allStarSizeValues.First(x => x.Alias == comb.Size).Id),
					StockQuantity = 10000,
					AllowOutOfStockOrders = true,
					IsActive = true,
					AssignedPictureIds = allStarPictures.First(x => x.SeoFilename.EndsWith(lowerColor)).Id.ToString()
				});
			}

			#endregion

			#region Fashion - Shirt Meccanica

			var productShirtMeccanica = _ctx.Set<Product>().First(x => x.Sku == "Fashion-987693502");
			var shirtMeccanicaPictureIds = productShirtMeccanica.ProductPictures.Select(x => x.PictureId).ToList();
			var shirtMeccanicaPictures = _ctx.Set<Picture>().Where(x => shirtMeccanicaPictureIds.Contains(x.Id)).ToList();

			var shirtMeccanicaColor = _ctx.Set<ProductVariantAttribute>().First(x => x.ProductId == productShirtMeccanica.Id && x.ProductAttributeId == attrColor.Id);
			var shirtMeccanicaColorValues = _ctx.Set<ProductVariantAttributeValue>().Where(x => x.ProductVariantAttributeId == shirtMeccanicaColor.Id).ToList();

			var shirtMeccanicaSize = _ctx.Set<ProductVariantAttribute>().First(x => x.ProductId == productShirtMeccanica.Id && x.ProductAttributeId == attrSize.Id);
			var shirtMeccanicaSizeValues = _ctx.Set<ProductVariantAttributeValue>().Where(x => x.ProductVariantAttributeId == shirtMeccanicaSize.Id).ToList();

			var shirtMeccanicaCombinations = new[]
			{
				new { Color = "Red", Size = "XS" },
				new { Color = "Red", Size = "S" },
				new { Color = "Red", Size = "M" },
				new { Color = "Red", Size = "L" },
				new { Color = "Red", Size = "XL" },
				new { Color = "Black", Size = "XS" },
				new { Color = "Black", Size = "S" },
				new { Color = "Black", Size = "M" },
				new { Color = "Black", Size = "L" },
				new { Color = "Black", Size = "XL" }
			};

			foreach (var comb in shirtMeccanicaCombinations)
			{
				var lowerColor = comb.Color.ToLower();
				var lowerSize = comb.Size.ToLower();
				var pictureIds = shirtMeccanicaPictures.Where(x => x.SeoFilename.Contains($"_{lowerColor}_")).Select(x => x.Id);

				entities.Add(new ProductVariantAttributeCombination
				{
					Product = productShirtMeccanica,
					Sku = productShirtMeccanica.Sku + string.Concat("-", lowerColor, "-", lowerSize),
					AttributesXml = FormatAttributeXml(
						shirtMeccanicaColor.Id, shirtMeccanicaColorValues.First(x => x.Alias == lowerColor).Id,
						shirtMeccanicaSize.Id, shirtMeccanicaSizeValues.First(x => x.Alias == lowerSize).Id),
					StockQuantity = 10000,
					AllowOutOfStockOrders = true,
					IsActive = true,
					AssignedPictureIds = string.Join(",", pictureIds)
				});
			}

			#endregion

			#region Fashion - Ladies Jacket

			var productLadiesJacket = _ctx.Set<Product>().First(x => x.Sku == "Fashion-JN1107");
			var ladiesJacketPictureIds = productLadiesJacket.ProductPictures.Select(x => x.PictureId).ToList();
			var ladiesJacketPictures = _ctx.Set<Picture>().Where(x => ladiesJacketPictureIds.Contains(x.Id)).ToList();

			var ladiesJacketColor = _ctx.Set<ProductVariantAttribute>().First(x => x.ProductId == productLadiesJacket.Id && x.ProductAttributeId == attrColor.Id);
			var ladiesJacketColorValues = _ctx.Set<ProductVariantAttributeValue>().Where(x => x.ProductVariantAttributeId == ladiesJacketColor.Id).ToList();

			var ladiesJacketSize = _ctx.Set<ProductVariantAttribute>().First(x => x.ProductId == productLadiesJacket.Id && x.ProductAttributeId == attrSize.Id);
			var ladiesJacketSizeValues = _ctx.Set<ProductVariantAttributeValue>().Where(x => x.ProductVariantAttributeId == ladiesJacketSize.Id).ToList();

			var ladiesJacketCombinations = new[]
			{
				new { Color = "Red", Size = "XS" },
				new { Color = "Red", Size = "S" },
				new { Color = "Red", Size = "M" },
				new { Color = "Red", Size = "L" },
				new { Color = "Red", Size = "XL" },
				new { Color = "Orange", Size = "XS" },
				new { Color = "Orange", Size = "S" },
				new { Color = "Orange", Size = "M" },
				new { Color = "Orange", Size = "L" },
				new { Color = "Orange", Size = "XL" },
				new { Color = "Green", Size = "XS" },
				new { Color = "Green", Size = "S" },
				new { Color = "Green", Size = "M" },
				new { Color = "Green", Size = "L" },
				new { Color = "Green", Size = "XL" },
				new { Color = "Blue", Size = "XS" },
				new { Color = "Blue", Size = "S" },
				new { Color = "Blue", Size = "M" },
				new { Color = "Blue", Size = "L" },
				new { Color = "Blue", Size = "XL" },
				new { Color = "Navy", Size = "XS" },
				new { Color = "Navy", Size = "S" },
				new { Color = "Navy", Size = "M" },
				new { Color = "Navy", Size = "L" },
				new { Color = "Navy", Size = "XL" },
				new { Color = "Silver", Size = "XS" },
				new { Color = "Silver", Size = "S" },
				new { Color = "Silver", Size = "M" },
				new { Color = "Silver", Size = "L" },
				new { Color = "Silver", Size = "XL" },
				new { Color = "Black", Size = "XS" },
				new { Color = "Black", Size = "S" },
				new { Color = "Black", Size = "M" },
				new { Color = "Black", Size = "L" },
				new { Color = "Black", Size = "XL" }
			};

			foreach (var comb in ladiesJacketCombinations)
			{
				var lowerColor = comb.Color.ToLower();
				var lowerSize = comb.Size.ToLower();

				entities.Add(new ProductVariantAttributeCombination
				{
					Product = productLadiesJacket,
					Sku = productLadiesJacket.Sku + string.Concat("-", lowerColor, "-", lowerSize),
					AttributesXml = FormatAttributeXml(
						ladiesJacketColor.Id, ladiesJacketColorValues.First(x => x.Alias == lowerColor).Id,
						ladiesJacketSize.Id, ladiesJacketSizeValues.First(x => x.Alias == lowerSize).Id),
					StockQuantity = 10000,
					AllowOutOfStockOrders = true,
					IsActive = true,
					AssignedPictureIds = ladiesJacketPictures.First(x => x.SeoFilename.EndsWith(lowerColor)).Id.ToString()
				});
			}

            #endregion

			#region Furniture - Le Corbusier LC 6 table

			var productCorbusierTable = _ctx.Set<Product>().First(x => x.Sku == "Furniture-lc6");

			var corbusierTablePlate = _ctx.Set<ProductVariantAttribute>().First(x => x.ProductId == productCorbusierTable.Id && x.ProductAttributeId == attrPlate.Id);
			var corbusierTablePlateValues = _ctx.Set<ProductVariantAttributeValue>().Where(x => x.ProductVariantAttributeId == corbusierTablePlate.Id).ToList();

			var corbusierTablePlateThickness = _ctx.Set<ProductVariantAttribute>().First(x => x.ProductId == productCorbusierTable.Id && x.ProductAttributeId == attrPlateThickness.Id);
			var corbusierTablePlateThicknessValues = _ctx.Set<ProductVariantAttributeValue>().Where(x => x.ProductVariantAttributeId == corbusierTablePlateThickness.Id).ToList();

			entities.Add(new ProductVariantAttributeCombination
			{
				Product = productCorbusierTable,
				Sku = productCorbusierTable.Sku + "-clear-15",
				AttributesXml = FormatAttributeXml(
					corbusierTablePlate.Id, corbusierTablePlateValues.First(x => x.Alias == "clear-glass").Id,
					corbusierTablePlateThickness.Id, corbusierTablePlateThicknessValues.First(x => x.Alias == "15mm").Id),
				StockQuantity = 10000,
				AllowOutOfStockOrders = true,
				IsActive = true,
				Price = 749.00M
			});
			entities.Add(new ProductVariantAttributeCombination
			{
				Product = productCorbusierTable,
				Sku = productCorbusierTable.Sku + "-clear-19",
				AttributesXml = FormatAttributeXml(
					corbusierTablePlate.Id, corbusierTablePlateValues.First(x => x.Alias == "clear-glass").Id,
					corbusierTablePlateThickness.Id, corbusierTablePlateThicknessValues.First(x => x.Alias == "19mm").Id),
				StockQuantity = 10000,
				AllowOutOfStockOrders = true,
				IsActive = true,
				Price = 899.00M
			});
			entities.Add(new ProductVariantAttributeCombination
			{
				Product = productCorbusierTable,
				Sku = productCorbusierTable.Sku + "-sandblasted-15",
				AttributesXml = FormatAttributeXml(
					corbusierTablePlate.Id, corbusierTablePlateValues.First(x => x.Alias == "sandblasted-glass").Id,
					corbusierTablePlateThickness.Id, corbusierTablePlateThicknessValues.First(x => x.Alias == "15mm").Id),
				StockQuantity = 10000,
				AllowOutOfStockOrders = true,
				IsActive = true,
				Price = 849.00M
			});
			entities.Add(new ProductVariantAttributeCombination
			{
				Product = productCorbusierTable,
				Sku = productCorbusierTable.Sku + "-sandblasted-19",
				AttributesXml = FormatAttributeXml(
					corbusierTablePlate.Id, corbusierTablePlateValues.First(x => x.Alias == "sandblasted-glass").Id,
					corbusierTablePlateThickness.Id, corbusierTablePlateThicknessValues.First(x => x.Alias == "19mm").Id),
				StockQuantity = 10000,
				AllowOutOfStockOrders = true,
				IsActive = true,
				Price = 999.00M
			});

			#endregion

            #region Soccer Adidas TANGO SALA BALL

            var productAdidasTANGOSALABALL = _ctx.Set<Product>().First(x => x.Sku == "P-5001");
            var adidasTANGOSALABALLPictureIds = productAdidasTANGOSALABALL.ProductPictures.Select(x => x.PictureId).ToList();
            var adidasTANGOSALABALLJacketPictures = _ctx.Set<Picture>().Where(x => adidasTANGOSALABALLPictureIds.Contains(x.Id)).ToList();

            var adidasTANGOSALABALLColor = _ctx.Set<ProductVariantAttribute>().First(x => x.ProductId == productAdidasTANGOSALABALL.Id && x.ProductAttributeId == attrColor.Id);
            var adidasTANGOSALABALLColorValues = _ctx.Set<ProductVariantAttributeValue>().Where(x => x.ProductVariantAttributeId == adidasTANGOSALABALLColor.Id).ToList();

            var adidasTANGOSALABALLSize = _ctx.Set<ProductVariantAttribute>().First(x => x.ProductId == productAdidasTANGOSALABALL.Id && x.ProductAttributeId == attrSize.Id);
            var adidasTANGOSALABALLSizeValues = _ctx.Set<ProductVariantAttributeValue>().Where(x => x.ProductVariantAttributeId == adidasTANGOSALABALLSize.Id).ToList();

            var adidasTANGOSALABALLCombinations = new[]
            {
                new { Color = "Red", Size = "3" },
                new { Color = "Red", Size = "4" },
                new { Color = "Red", Size = "5" },
                
                new { Color = "Yellow", Size = "3" },
                new { Color = "Yellow", Size = "4" },
                new { Color = "Yellow", Size = "5" },
               
                new { Color = "Green", Size = "3" },
                new { Color = "Green", Size = "4" },
                new { Color = "Green", Size = "5" },
                
                new { Color = "Blue", Size = "3" },
                new { Color = "Blue", Size = "4" },
                new { Color = "Blue", Size = "5" },
                
                new { Color = "Gray", Size = "3" },
                new { Color = "Gray", Size = "4" },
                new { Color = "Gray", Size = "5" },

                new { Color = "White", Size = "3" },
                new { Color = "White", Size = "4" },
                new { Color = "White", Size = "5" },

                new { Color = "Brown", Size = "3" },
                new { Color = "Brown", Size = "4" },
                new { Color = "Brown", Size = "5" },

            };

            foreach (var comb in adidasTANGOSALABALLCombinations)
            {
                var lowerColor = comb.Color.ToLower();
                var lowerSize = comb.Size.ToLower();

                entities.Add(new ProductVariantAttributeCombination
                {
                    Product = productAdidasTANGOSALABALL,
                    Sku = productAdidasTANGOSALABALL.Sku + string.Concat("-", lowerColor, "-", lowerSize),
                    AttributesXml = FormatAttributeXml(
                        adidasTANGOSALABALLColor.Id, adidasTANGOSALABALLColorValues.First(x => x.Alias == lowerColor).Id,
                        adidasTANGOSALABALLSize.Id, adidasTANGOSALABALLSizeValues.First(x => x.Alias == lowerSize).Id),
                    StockQuantity = 10000,
                    AllowOutOfStockOrders = true,
                    IsActive = true,
                    AssignedPictureIds = adidasTANGOSALABALLJacketPictures.First(x => x.SeoFilename.EndsWith(lowerColor)).Id.ToString()
                });
            }

            #endregion Soccer Adidas TANGO SALA BALL

            #region Soccer Torfabrik official game ball

            var productTorfabrikBall = _ctx.Set<Product>().First(x => x.Sku == "P-5002");
            var torfabrikBallPictureIds = productTorfabrikBall.ProductPictures.Select(x => x.PictureId).ToList();
            var torfabrikBallPictures = _ctx.Set<Picture>().Where(x => torfabrikBallPictureIds.Contains(x.Id)).ToList();

            var torfabrikBallColor = _ctx.Set<ProductVariantAttribute>().First(x => x.ProductId == productTorfabrikBall.Id && x.ProductAttributeId == attrColor.Id);
            var torfabrikBallColorValues = _ctx.Set<ProductVariantAttributeValue>().Where(x => x.ProductVariantAttributeId == torfabrikBallColor.Id).ToList();

            var torfabrikBallSize = _ctx.Set<ProductVariantAttribute>().First(x => x.ProductId == productTorfabrikBall.Id && x.ProductAttributeId == attrSize.Id);
            var torfabrikBallSizeValues = _ctx.Set<ProductVariantAttributeValue>().Where(x => x.ProductVariantAttributeId == torfabrikBallSize.Id).ToList();

            var torfabrikBallSizeCombinations = new[]
            {
                new { Color = "Red", Size = "3" },
                new { Color = "Red", Size = "4" },
                new { Color = "Red", Size = "5" },

                new { Color = "Yellow", Size = "3" },
                new { Color = "Yellow", Size = "4" },
                new { Color = "Yellow", Size = "5" },

                new { Color = "Green", Size = "3" },
                new { Color = "Green", Size = "4" },
                new { Color = "Green", Size = "5" },

                new { Color = "Blue", Size = "3" },
                new { Color = "Blue", Size = "4" },
                new { Color = "Blue", Size = "5" },

                new { Color = "White", Size = "3" },
                new { Color = "White", Size = "4" },
                new { Color = "White", Size = "5" },

            };

            foreach (var comb in torfabrikBallSizeCombinations)
            {
                var lowerColor = comb.Color.ToLower();
                var lowerSize = comb.Size.ToLower();

                entities.Add(new ProductVariantAttributeCombination
                {
                    Product = productTorfabrikBall,
                    Sku = productTorfabrikBall.Sku + string.Concat("-", lowerColor, "-", lowerSize),
                    AttributesXml = FormatAttributeXml(
                        torfabrikBallColor.Id, torfabrikBallColorValues.First(x => x.Alias == lowerColor).Id,
                        torfabrikBallSize.Id, torfabrikBallSizeValues.First(x => x.Alias == lowerSize).Id),
                    StockQuantity = 10000,
                    AllowOutOfStockOrders = true,
                    IsActive = true,
                    AssignedPictureIds = torfabrikBallPictures.First(x => x.SeoFilename.EndsWith(lowerColor)).Id.ToString()
                });
            }

            #endregion Soccer Torfabrik official game ball

			#region Furniture - Ball chair

			var productBallChair = _ctx.Set<Product>().First(x => x.Sku == "Furniture-ball-chair");
			var ballChairPictureIds = productBallChair.ProductPictures.Select(x => x.PictureId).ToList();
			var ballChairPictures = _ctx.Set<Picture>().Where(x => ballChairPictureIds.Contains(x.Id)).ToList();

			var ballChairMaterial = _ctx.Set<ProductVariantAttribute>().First(x => x.ProductId == productBallChair.Id && x.ProductAttributeId == attrMaterial.Id);
			var ballChairMaterialValues = _ctx.Set<ProductVariantAttributeValue>().Where(x => x.ProductVariantAttributeId == ballChairMaterial.Id).ToList();

			var ballChairColor = _ctx.Set<ProductVariantAttribute>().First(x => x.ProductId == productBallChair.Id && x.ProductAttributeId == attrColor.Id);
			var ballChairColorValues = _ctx.Set<ProductVariantAttributeValue>().Where(x => x.ProductVariantAttributeId == ballChairColor.Id).ToList();

			var ballChairLeatherColor = _ctx.Set<ProductVariantAttribute>().First(x => x.ProductId == productBallChair.Id && x.ProductAttributeId == attrLeatherColor.Id);
			var ballChairLeatherColorValues = _ctx.Set<ProductVariantAttributeValue>().Where(x => x.ProductVariantAttributeId == ballChairLeatherColor.Id).ToList();

			foreach (var materialValue in ballChairMaterialValues)
			{
				foreach (var colorValue in ballChairColorValues)
				{
					decimal ballChairPrice = 2199.00M;

					if (materialValue.Alias.StartsWith("leather-special"))
					{
						ballChairPrice = 2599.00M;
					}
					else if (materialValue.Alias.StartsWith("leather-aniline"))
					{
						ballChairPrice = 2999.00M;
					}

					foreach (var leatherColorValue in ballChairLeatherColorValues)
					{
						entities.Add(new ProductVariantAttributeCombination
						{
							Product = productBallChair,
							Sku = productBallChair.Sku + string.Concat("-", colorValue.Alias, "-", materialValue.Alias),
							AttributesXml = FormatAttributeXml(ballChairMaterial.Id, materialValue.Id, ballChairColor.Id, colorValue.Id, ballChairLeatherColor.Id, leatherColorValue.Id),
							StockQuantity = 10000,
							AllowOutOfStockOrders = true,
							IsActive = true,
							AssignedPictureIds = ballChairPictures.First(x => x.SeoFilename.EndsWith(colorValue.Alias)).Id.ToString(),
							Price = ballChairPrice
						});
					}
				}
			}

			#endregion

			return entities;
		}

		public IList<ProductTag> ProductTags()
		{
            #region tag apple
            var productTagApple = new ProductTag
            {
                Name = "apple"
            };

            _ctx.Set<Product>().Where(pt => pt.MetaTitle == "iPhone Plus").First().ProductTags.Add(productTagApple);

            #endregion tag apple

            #region tag gift
            var productTagGift = new ProductTag
			{
				Name = "gift"
			};

			_ctx.Set<Product>().Where(pt => pt.MetaTitle == "$10 Virtual Gift Card").First().ProductTags.Add(productTagGift);
			_ctx.Set<Product>().Where(pt => pt.MetaTitle == "$25 Virtual Gift Card").First().ProductTags.Add(productTagGift);
			_ctx.Set<Product>().Where(pt => pt.MetaTitle == "$50 Virtual Gift Card").First().ProductTags.Add(productTagGift);
            _ctx.Set<Product>().Where(pt => pt.MetaTitle == "$100 Virtual Gift Card").First().ProductTags.Add(productTagGift);

            #endregion tag gift

			#region tag book
			var productTagBook = new ProductTag
			{
				Name = "book"
			};

			_ctx.Set<Product>().Where(pt => pt.MetaTitle == "Überman: The novel").First().ProductTags.Add(productTagBook);
			_ctx.Set<Product>().Where(pt => pt.MetaTitle == "Best Grilling Recipes").First().ProductTags.Add(productTagBook);
			_ctx.Set<Product>().Where(pt => pt.MetaTitle == "Car of superlatives").First().ProductTags.Add(productTagBook);
			_ctx.Set<Product>().Where(pt => pt.MetaTitle == "Picture Atlas Motorcycles").First().ProductTags.Add(productTagBook);
			_ctx.Set<Product>().Where(pt => pt.MetaTitle == "The Car Book").First().ProductTags.Add(productTagBook);
			_ctx.Set<Product>().Where(pt => pt.MetaTitle == "Fast Cars").First().ProductTags.Add(productTagBook);
			_ctx.Set<Product>().Where(pt => pt.MetaTitle == "Motorcycle Adventures").First().ProductTags.Add(productTagBook);

			#endregion tag book

			#region tag cooking
			var productTagCooking = new ProductTag
			{
				Name = "cooking"
			};

			_ctx.Set<Product>().Where(pt => pt.MetaTitle == "Überman: The novel").FirstOrDefault().ProductTags.Add(productTagCooking);
			_ctx.Set<Product>().Where(pt => pt.MetaTitle == "Best Grilling Recipes").FirstOrDefault().ProductTags.Add(productTagCooking);

			#endregion tag cooking

			#region tag cars
			var productTagCars = new ProductTag
			{
				Name = "cars"
			};

			_ctx.Set<Product>().Where(pt => pt.MetaTitle == "The Car Book").FirstOrDefault().ProductTags.Add(productTagCars);
			_ctx.Set<Product>().Where(pt => pt.MetaTitle == "Fast Cars").FirstOrDefault().ProductTags.Add(productTagCars);

			#endregion tag cars

			#region tag motorbikes
			var productTagMotorbikes = new ProductTag
			{
				Name = "motorbikes"
			};

			_ctx.Set<Product>().Where(pt => pt.MetaTitle == "Fast Cars").FirstOrDefault().ProductTags.Add(productTagMotorbikes);
			_ctx.Set<Product>().Where(pt => pt.MetaTitle == "Motorcycle Adventures").FirstOrDefault().ProductTags.Add(productTagMotorbikes);

			#endregion tag motorbikes

			#region tag mp3
			var productTagMP3 = new ProductTag
			{
				Name = "mp3"
			};

			_ctx.Set<Product>().Where(pt => pt.MetaTitle == "Antonio Vivaldi: spring").FirstOrDefault().ProductTags.Add(productTagMP3);
			_ctx.Set<Product>().Where(pt => pt.MetaTitle == "Ludwig van Beethoven: Für Elise").FirstOrDefault().ProductTags.Add(productTagMP3);

			#endregion tag mp3

			#region tag download
			var productTagDownload = new ProductTag
			{
				Name = "download"
			};

			_ctx.Set<Product>().Where(pt => pt.MetaTitle == "Antonio Vivaldi: spring").FirstOrDefault().ProductTags.Add(productTagDownload);
			_ctx.Set<Product>().Where(pt => pt.MetaTitle == "Ludwig van Beethoven: Für Elise").FirstOrDefault().ProductTags.Add(productTagDownload);

			#endregion tag download

			#region tag watches
			var productTagWatches = new ProductTag
			{
				Name = "watches"
			};

			_ctx.Set<Product>().Where(pt => pt.MetaTitle == "Certina DS Podium Big Size").FirstOrDefault().ProductTags.Add(productTagWatches);

			#endregion tag download

			var entities = new List<ProductTag>
			{
			   productTagGift, productTagBook, productTagCooking, productTagCars, productTagMotorbikes,
			   productTagMP3, productTagDownload
			};

			this.Alter(entities);
			return entities;
		}

		public IList<Category> CategoriesFirstLevel()
		{
			var sampleImagesPath = this._sampleImagesPath;
			var categoryTemplateInGridAndLines = this.CategoryTemplates().Where(pt => pt.ViewPath == "CategoryTemplate.ProductsInGridOrLines").FirstOrDefault();

            #region category definitions

            var categoryFurniture = new Category
            {
                Name = "Furniture",
                Alias = "Furniture",
                CategoryTemplateId = categoryTemplateInGridAndLines.Id,
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "category_furniture.jpg"), "image/jpeg", GetSeName("Furniture")),
                Published = true,
                DisplayOrder = 1,
                MetaTitle = "Furniture",
                ShowOnHomePage = true
            };

            var categoryApple = new Category
            {
                Name = "Apple",
                Alias = "Apple",
                CategoryTemplateId = categoryTemplateInGridAndLines.Id,
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "category_apple.png"), "image/jpeg", GetSeName("Apple")),
                Published = true,
                DisplayOrder = 1,
                MetaTitle = "Apple",
                ShowOnHomePage = true
            };

            var categorySports = new Category
            {
                Name = "Sports",
                Alias = "Sports",
                CategoryTemplateId = categoryTemplateInGridAndLines.Id,
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "category_sports.jpg"), "image/jpeg", GetSeName("Sports")),
                Published = true,
                DisplayOrder = 1,
                MetaTitle = "Sports",
                ShowOnHomePage = true
            };

            var categoryBooks = new Category
			{
				Name = "Books",
                Alias = "Books",
				CategoryTemplateId = categoryTemplateInGridAndLines.Id,
				Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "emblem_library.png"), "image/jpeg", GetSeName("Books")),
				Published = true,
				DisplayOrder = 1,
				MetaTitle = "Books"
			};

			//var categoryComputers = new Category
			//{
			//	Name = "Computers",
   //             Alias = "Computers",
			//	CategoryTemplateId = categoryTemplateInGridAndLines.Id,
   //             Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "category_computers.png"), "image/png", GetSeName("Computers")),
			//	Published = true,
			//	DisplayOrder = 2,
			//	MetaTitle = "Computers"
			//};

            var categoryFashion = new Category
            {
                Name = "Fashion",
                Alias = "Fashion",
                CategoryTemplateId = categoryTemplateInGridAndLines.Id,
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "category_fashion.jpg"), "image/png", GetSeName("Fashion")),
                Published = true,
                DisplayOrder = 2,
                MetaTitle = "Fashion",
                ShowOnHomePage = true,
                BadgeText = "SALE",
                BadgeStyle = 4
            };

            var categoryGaming = new Category
			{
				Name = "Gaming",
				Alias = "Gaming",
				CategoryTemplateId = categoryTemplateInGridAndLines.Id,
				Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "ps4_bundle_minecraft.jpg"), "image/png", GetSeName("Gaming")),
				Published = true,
				DisplayOrder = 3,
                ShowOnHomePage = true,
				MetaTitle = "Gaming"
			};

			//var categoryCellPhones = new Category
			//{
			//	Name = "Cell phones",
   //             Alias = "Cell phones",
			//	CategoryTemplateId = categoryTemplateInGridAndLines.Id,
			//	//ParentCategoryId = categoryElectronics.Id,
   //             Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "category_cellphone.png"), "image/png", GetSeName("Cell phones")),
			//	Published = true,
			//	DisplayOrder = 4,
			//	MetaTitle = "Cell phones"
			//};

			var categoryDigitalDownloads = new Category
			{
				Name = "Digital Products",
                Alias = "Digital Products",
				CategoryTemplateId = categoryTemplateInGridAndLines.Id,
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "category_digitalproducts.jpg"), "image/jpeg", GetSeName("Digital Products")),
				Published = true,
				DisplayOrder = 6,
				MetaTitle = "Digital Products",
                ShowOnHomePage = true
            };

			var categoryGiftCards = new Category
			{
				Name = "Gift Cards",
                Alias = "Gift Cards",
				CategoryTemplateId = categoryTemplateInGridAndLines.Id,
				Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "category_gift-cards.png"), "image/png", GetSeName("Gift Cards")),
				Published = true,
				DisplayOrder = 12,
				MetaTitle = "Gift cards",
                ShowOnHomePage = true,
            };

			var categoryWatches = new Category
			{
				Name = "Watches",
                Alias = "Watches",
				CategoryTemplateId = categoryTemplateInGridAndLines.Id,
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "category_watches.png"), "image/png", GetSeName("Watches")),
				Published = true,
				DisplayOrder = 10,
				MetaTitle = "Watches",
                ShowOnHomePage = true,
                BadgeText = "%",
                BadgeStyle = 5
            };

			#endregion

			var entities = new List<Category>
			{
				categoryApple, categorySports, categoryBooks, categoryFurniture, categoryDigitalDownloads, categoryGaming,
				categoryGiftCards, categoryFashion, categoryWatches
            };

			this.Alter(entities);
			return entities;
		}

		public IList<Category> CategoriesSecondLevel()
		{
			var sampleImagesPath = this._sampleImagesPath;
			var categoryTemplateInGridAndLines = this.CategoryTemplates().Where(pt => pt.ViewPath == "CategoryTemplate.ProductsInGridOrLines").FirstOrDefault();

            #region category definitions

            var categorySportsGolf = new Category
            {
                Name = "Golf",
                Alias = "Golf",
                CategoryTemplateId = categoryTemplateInGridAndLines.Id,
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "category_golf.jpg"), "image/png", GetSeName("Golf")),
                Published = true,
                ParentCategoryId = _ctx.Set<Category>().Where(x => x.MetaTitle == "Sports").First().Id,
                DisplayOrder = 1,
                MetaTitle = "Golf",
                ShowOnHomePage = true
            };

            var categorySportsSunglasses = new Category
            {
                Name = "Sunglasses",
                Alias = "Sunglasses",
                CategoryTemplateId = categoryTemplateInGridAndLines.Id,
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "category_glasses.png"), "image/png", GetSeName("Sunglasses")),
                Published = true,
                ParentCategoryId = _ctx.Set<Category>().Where(x => x.MetaTitle == "Fashion").First().Id,
                DisplayOrder = 1,
                MetaTitle = "Sunglasses",
                ShowOnHomePage = true
            };

            var categorySportsSoccer = new Category
            {
                Name = "Soccer",
                Alias = "Soccer",
                CategoryTemplateId = categoryTemplateInGridAndLines.Id,
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "category_soccer.png"), "image/png", GetSeName("Soccer")),
                Published = true,
                ParentCategoryId = _ctx.Set<Category>().Where(x => x.MetaTitle == "Sports").First().Id,
                DisplayOrder = 1,
                MetaTitle = "Soccer",
                ShowOnHomePage = true
            };

            var categorySportsBasketball = new Category
            {
                Name = "Basketball",
                Alias = "Basketball",
                CategoryTemplateId = categoryTemplateInGridAndLines.Id,
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "category_basketball.png"), "image/png", GetSeName("Basketball")),
                Published = true,
                ParentCategoryId = _ctx.Set<Category>().Where(x => x.MetaTitle == "Sports").First().Id,
                DisplayOrder = 1,
                MetaTitle = "Basketball",
                ShowOnHomePage = true
            };

            var categoryBooksSpiegel = new Category
			{
				Name = "SPIEGEL-Bestseller",
                Alias = "SPIEGEL-Bestseller",
				CategoryTemplateId = categoryTemplateInGridAndLines.Id,
				Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "0000930_spiegel-bestseller.png"), "image/png", GetSeName("SPIEGEL-Bestseller")),
				Published = true,
				ParentCategoryId = _ctx.Set<Category>().Where(x => x.MetaTitle == "Books").First().Id,
				DisplayOrder = 1,
				MetaTitle = "SPIEGEL-Bestseller"
			};

			var categoryBooksCookAndEnjoy = new Category
			{
				Name = "Cook and enjoy",
                Alias = "Cook and enjoy",
				CategoryTemplateId = categoryTemplateInGridAndLines.Id,
				Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "0000936_kochen-geniesen.jpeg"), "image/jpeg", GetSeName("Cook and enjoy")),
				Published = true,
				ParentCategoryId = _ctx.Set<Category>().Where(x => x.MetaTitle == "Books").First().Id,
				DisplayOrder = 2,
				MetaTitle = "Cook and enjoy"
			};

			//var categoryDesktops = new Category
			//{
			//	Name = "Desktops",
   //             Alias = "Desktops",
			//	CategoryTemplateId = categoryTemplateInGridAndLines.Id,
			//	ParentCategoryId = _ctx.Set<Category>().Where(x => x.MetaTitle == "Computers").First().Id,
			//	Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "category_desktops.png"), "image/png", GetSeName("Desktops")),
			//	Published = true,
			//	DisplayOrder = 1,
			//	MetaTitle = "Desktops"
			//};

			//var categoryNotebooks = new Category
			//{
			//	Name = "Notebooks",
   //             Alias = "Notebooks",
			//	CategoryTemplateId = categoryTemplateInGridAndLines.Id,
			//	ParentCategoryId = _ctx.Set<Category>().Where(x => x.MetaTitle == "Computers").First().Id,
   //             Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "category_notebooks.png"), "image/png", GetSeName("Notebooks")),
			//	Published = true,
			//	DisplayOrder = 2,
			//	MetaTitle = "Notebooks"
			//};

			var categoryGamingAccessories = new Category
			{
				Name = "Gaming Accessories",
				Alias = "Gaming Accessories",
				CategoryTemplateId = categoryTemplateInGridAndLines.Id,
				ParentCategoryId = _ctx.Set<Category>().Where(x => x.MetaTitle == "Gaming").First().Id,
				Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "category_gaming_accessories.png"), "image/png", GetSeName("Gaming Accessories")),
				Published = true,
				DisplayOrder = 2,
				MetaTitle = "Gaming Accessories"
			};

			var categoryGamingGames = new Category
			{
				Name = "Games",
				Alias = "Games",
				CategoryTemplateId = categoryTemplateInGridAndLines.Id,
				ParentCategoryId = _ctx.Set<Category>().Where(x => x.MetaTitle == "Gaming").First().Id,
				Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "category_games.jpg"), "image/png", GetSeName("Games")),
				Published = true,
				DisplayOrder = 3,
				MetaTitle = "Games"
			};

			#endregion

			var entities = new List<Category>
			{
                categorySportsSunglasses,categorySportsSoccer, categorySportsBasketball,categorySportsGolf, categoryBooksSpiegel, categoryBooksCookAndEnjoy,
				categoryGamingAccessories, categoryGamingGames
			};

			this.Alter(entities);
			return entities;
		}

		public IList<Manufacturer> Manufacturers()
		{
			//pictures
			var sampleImagesPath = this._sampleImagesPath;

			var manufacturerTemplateInGridAndLines =
				this.ManufacturerTemplates().Where(pt => pt.ViewPath == "ManufacturerTemplate.ProductsInGridOrLines").FirstOrDefault();

            //var categoryTemplateInGridAndLines =
            //    this.CategoryTemplates().Where(pt => pt.Name == "Products in Grid or Lines").FirstOrDefault();

            //categories

            #region EA Sports

            var manufacturerWarnerHome = new Manufacturer
            {
                Name = "EA Sports",
                ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "manufacturer_EA_Sports.png"), "image/png", GetSeName("EA Sports")),
                Published = true,
                DisplayOrder = 1
            };

            #endregion EA Sports

            #region Warner Home Video Games

            var manufacturerEASports = new Manufacturer
            {
                Name = "Warner Home Video Games",
                ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "manufacturer_wb.png"), "image/png", GetSeName("Warner Home Video Games")),
                Published = true,
                DisplayOrder = 1
            };

            #endregion Warner Home Video Games

            #region Breitling

            var manufacturerBreitling = new Manufacturer
            {
                Name = "Breitling",
                ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "manufacturer_breitling.png"), "image/png", GetSeName("Breitling")),
                Published = true,
                DisplayOrder = 1
            };

            #endregion Breitling

            #region Tissot

            var manufacturerTissot = new Manufacturer
            {
                Name = "Tissot",
                ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "manufacturer_Tissot.png"), "image/png", GetSeName("Tissot")),
                Published = true,
                DisplayOrder = 1
            };

            #endregion Tissot

            #region Seiko

            var manufacturerSeiko = new Manufacturer
            {
                Name = "Seiko",
                ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "manufacturer_seiko.png"), "image/png", GetSeName("Seiko")),
                Published = true,
                DisplayOrder = 1
            };

            #endregion Seiko

            #region Titleist

            var manufacturerTitleist = new Manufacturer
            {
                Name = "Titleist",
                ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "manufacturer_titleist.png"), "image/png", GetSeName("Titleist")),
                Published = true,
                DisplayOrder = 1
            };

            #endregion Titleist

            #region Puma

            var manufacturerPuma = new Manufacturer
            {
                Name = "Puma",
                ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "manufacturer_puma.jpg"), "image/png", GetSeName("Puma")),
                Published = true,
                DisplayOrder = 1
            };

            #endregion Puma

            #region Nike

            var manufacturerNike = new Manufacturer
            {
                Name = "Nike",
                ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "manufacturer_nike.png"), "image/png", GetSeName("Nike")),
                Published = true,
                DisplayOrder = 1
            };

            #endregion Nike

            #region Wilson

            var manufacturerWilson = new Manufacturer
            {
                Name = "Wilson",
                ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "manufacturer_wilson.png"), "image/png", GetSeName("Wilson")),
                Published = true,
                DisplayOrder = 1
            };

            #endregion Wilson

            #region Adidas

            var manufacturerAdidas = new Manufacturer
            {
                Name = "Adidas",
                ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "manufacturer_adidas.png"), "image/png", GetSeName("Adidas")),
                Published = true,
                DisplayOrder = 1
            };

            #endregion Adidas

            #region Ray-ban

            var manufacturerRayban = new Manufacturer
            {
                Name = "Ray-Ban",
                ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "manufacturer_ray-ban.jpg"), "image/png", GetSeName("Ray-Ban")),
                Published = true,
                DisplayOrder = 1
            };

            #endregion Ray-ban

            #region Oakley

            var manufacturerOakley = new Manufacturer
            {
                Name = "Oakley",
                ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "manufacturer_oakley.png"), "image/png", GetSeName("Oakley")),
                Published = true,
                DisplayOrder = 1
            };

            #endregion Oakley

            #region Apple

            var manufacturerApple = new Manufacturer
			{
				Name = "Apple",
				ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "manufacturer_apple.png"), "image/png", GetSeName("Apple")),
				Published = true,
				DisplayOrder = 1
			};

			#endregion Apple

            #region Android

            var manufacturerAndroid = new Manufacturer
            {
                Name = "Android",
                ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "manufacturer-android.png"), "image/png", GetSeName("Android")),
                Published = true,
                DisplayOrder = 2
            };

            #endregion Android

            #region LG

            var manufacturerLG = new Manufacturer
            {
                Name = "LG",
                ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "manufacturer-lg.png"), "image/png", GetSeName("LG")),
                Published = true,
                DisplayOrder = 3
            };

            #endregion LG

            #region Dell

            var manufacturerDell = new Manufacturer
            {
                Name = "Dell",
                ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "manufacturer-dell.png"), "image/png", GetSeName("Dell")),
                Published = true,
                DisplayOrder = 4
            };

            #endregion Dell

            #region HP

            var manufacturerHP = new Manufacturer
            {
                Name = "HP",
                ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "manufacturer-hp.png"), "image/png", GetSeName("HP")),
                Published = true,
                DisplayOrder = 5
            };

            #endregion HP

            #region Microsoft

            var manufacturerMicrosoft = new Manufacturer
            {
                Name = "Microsoft",
                ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "manufacturer-microsoft.png"), "image/png", GetSeName("Microsoft")),
                Published = true,
                DisplayOrder = 6
            };

            #endregion Microsoft

            #region Samsung

            var manufacturerSamsung = new Manufacturer
			{
				Name = "Samsung",
				ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "manufacturer-samsung.png"), "image/png", GetSeName("Samsung")),
				Published = true,
				DisplayOrder = 7
			};

			#endregion Samsung

			#region Acer

			var manufacturerAcer = new Manufacturer
			{
				Name = "Acer",
				ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
				Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "acer-logo.jpg"), "image/pjpeg", GetSeName("Acer")),
				Published = true,
				DisplayOrder = 8
			};

			#endregion Acer

			#region TrekStor

			var manufacturerTrekStor = new Manufacturer
			{
				Name = "TrekStor",
				ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "manufacturer-trekstor.png"), "image/png", GetSeName("TrekStor")),
				Published = true,
				DisplayOrder = 9
			};

			#endregion TrekStor

			#region Western Digital

			var manufacturerWesternDigital = new Manufacturer
			{
				Name = "Western Digital",
				ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "manufacturer-westerndigital.png"), "image/png", GetSeName("Western Digital")),
				Published = true,
				DisplayOrder = 10
			};

			#endregion Western Digital

			#region MSI

			var manufacturerMSI = new Manufacturer
			{
				Name = "MSI",
				ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "manufacturer-msi.png"), "image/png", GetSeName("MSI")),
				Published = true,
				DisplayOrder = 11
			};

			#endregion MSI

			#region Canon

			var manufacturerCanon = new Manufacturer
			{
				Name = "Canon",
				ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "manufacturer-canon.png"), "image/png", GetSeName("Canon")),
				Published = true,
				DisplayOrder = 12
			};

			#endregion Canon

			#region Casio

			var manufacturerCasio = new Manufacturer
			{
				Name = "Casio",
				ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "manufacturer-casio.png"), "image/png", GetSeName("Casio")),
				Published = true,
				DisplayOrder = 13
			};

			#endregion Casio

			#region Panasonic

			var manufacturerPanasonic = new Manufacturer
			{
				Name = "Panasonic",
				ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "manufacturer-panasonic.png"), "image/png", GetSeName("Panasonic")),
				Published = true,
				DisplayOrder = 14
			};

			#endregion Panasonic

			#region BlackBerry

			var manufacturerBlackBerry = new Manufacturer
			{
				Name = "BlackBerry",
				ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "manufacturer-blackberry.png"), "image/png", GetSeName("BlackBerry")),
				Published = true,
				DisplayOrder = 15
			};

			#endregion BlackBerry

			#region HTC

			var manufacturerHTC = new Manufacturer
			{
				Name = "HTC",
				ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "manufacturer-htc.png"), "image/png", GetSeName("HTC")),
				Published = true,
				DisplayOrder = 16
			};

			#endregion HTC

			#region Festina

			var manufacturerFestina = new Manufacturer
			{
				Name = "Festina",
				ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "manufacturer_festina.png"), "image/png", GetSeName("Festina")),
				Published = true,
				DisplayOrder = 17
			};

			#endregion Festina

			#region Certina

			var manufacturerCertina = new Manufacturer
			{
				Name = "Certina",
				ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "manufacturer-certina.png"), "image/png", GetSeName("Certina")),
				Published = true,
				DisplayOrder = 18
			};

			#endregion Certina

			#region Sony

			var manufacturerSony = new Manufacturer
			{
				Name = "Sony",
				ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "manufacturer_sony.png"), "image/png", GetSeName("Sony")),
				Published = true,
				DisplayOrder = 19
			};

			#endregion Sony

			#region Ubisoft

			var manufacturerUbisoft = new Manufacturer
			{
				Name = "Ubisoft",
				ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id,
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "manufacturer_ubisoft.png"), "image/png", GetSeName("Ubisoft")),
				Published = true,
				DisplayOrder = 20
			};

			#endregion Ubisoft

			var entities = new List<Manufacturer>
			{
              manufacturerEASports,manufacturerWarnerHome,manufacturerBreitling,manufacturerTissot,manufacturerSeiko, manufacturerTitleist,manufacturerApple,manufacturerSamsung,manufacturerLG,manufacturerTrekStor, manufacturerWesternDigital,manufacturerDell, manufacturerMSI,
			  manufacturerCanon, manufacturerCasio, manufacturerPanasonic, manufacturerBlackBerry, manufacturerHTC, manufacturerFestina, manufacturerCertina, 
			  manufacturerHP, manufacturerAcer, manufacturerSony, manufacturerUbisoft,manufacturerOakley,manufacturerRayban,manufacturerAdidas, manufacturerWilson,manufacturerPuma,manufacturerNike
            };

			this.Alter(entities);
			return entities;
		}

		private List<Product> GetFashionProducts()
		{
			var result = new List<Product>();
			var productTemplateSimple = _ctx.Set<ProductTemplate>().First(x => x.ViewPath == "Product");
			var firstDeliveryTime = _ctx.Set<DeliveryTime>().First(x => x.DisplayOrder == 0);
			var fashionCategory = _ctx.Set<Category>().First(x => x.Alias == "Fashion");
			var specialPriceEndDate = DateTime.UtcNow.AddMonths(1);
			var specOptionCotton = _ctx.Set<SpecificationAttribute>().First(x => x.DisplayOrder == 8).SpecificationAttributeOptions.First(x => x.DisplayOrder == 9);

			// Converse All Star
			var converseAllStar = new Product
			{
				ProductType = ProductType.SimpleProduct,
				VisibleIndividually = true,
				Name = "Converse All Star",
				MetaTitle = "Converse All Star",
				ShortDescription = "The classical sneaker!",
				FullDescription = "<p>Since 1912 and to this day unrivalled: the converse All Star sneaker. A shoe for every occasion.</p>",
				Sku = "Fashion-112355",
				ProductTemplateId = productTemplateSimple.Id,
				AllowCustomerReviews = true,
				Published = true,
                ShowOnHomePage = true,
				Price = 79.90M,
				ManageInventoryMethod = ManageInventoryMethod.ManageStock,
				OrderMinimumQuantity = 1,
				OrderMaximumQuantity = 10000,
				StockQuantity = 10000,
				NotifyAdminForQuantityBelow = 1,
				IsShipEnabled = true,
				DeliveryTime = firstDeliveryTime,
				DisplayOrder = 1
			};

			converseAllStar.ProductCategories.Add(new ProductCategory
			{
				Category = fashionCategory,
				DisplayOrder = 1
			});

			var allStarImages = new string[] { "product_allstar_converse.jpg", "product_allstar_hi_charcoal.jpg", "product_allstar_hi_maroon.jpg", "product_allstar_hi_navy.jpg",
				"product_allstar_hi_purple.jpg", "product_allstar_hi_white.jpg" };

			for (var i = 0; i < allStarImages.Length; ++i)
			{
				converseAllStar.ProductPictures.Add(new ProductPicture
				{
					Picture = CreatePicture(File.ReadAllBytes(_sampleImagesPath + allStarImages[i]), "image/jpeg", allStarImages[i].Replace("product_", "").Replace(".jpg", "")),
					DisplayOrder = i + 1
				});
			}

			converseAllStar.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
			{
				AllowFiltering = true,
				ShowOnProductPage = true,
				DisplayOrder = 1,
				SpecificationAttributeOption = specOptionCotton
			});

			result.Add(converseAllStar);

			// Shirt Meccanica
			var shirtMeccanica = new Product
			{
				ProductType = ProductType.SimpleProduct,
				VisibleIndividually = true,
				Name = "Sleeveless shirt Meccanica",
				MetaTitle = "Sleeveless shirt Meccanica",
				ShortDescription = "Woman shirt with trendy imprint",
				FullDescription = "<p>Also in summer, the Ducati goes with fashion style! With the sleeveless shirt Meccanica, every woman can express her passion for Ducati with a comfortable and versatile piece of clothing. The shirt is available in black and vintage red. It carries on the front the traditional lettering in plastisol print, which makes it even clearer and more radiant, while on the back in the neck area is the famous logo with the typical \"wings\" of the fifties.</p>",
				Sku = "Fashion-987693502",
				ManufacturerPartNumber = "987693502",
				Gtin = "987693502",
				ProductTemplateId = productTemplateSimple.Id,
				AllowCustomerReviews = true,
				Published = true,
				Price = 38.00M,
				ManageInventoryMethod = ManageInventoryMethod.ManageStock,
				OrderMinimumQuantity = 1,
				OrderMaximumQuantity = 10000,
				StockQuantity = 10000,
				NotifyAdminForQuantityBelow = 1,
				IsShipEnabled = true,
				DeliveryTime = firstDeliveryTime,
				DisplayOrder = 4
			};

			shirtMeccanica.ProductCategories.Add(new ProductCategory
			{
				Category = fashionCategory,
				DisplayOrder = 1
			});

			shirtMeccanica.TierPrices.Add(new TierPrice
			{
				Quantity = 10,
				Price = 36.00M
			});
			shirtMeccanica.TierPrices.Add(new TierPrice
			{
				Quantity = 50,
				Price = 29.00M
			});

			var shirtMeccanicaImages = new string[] { "product_shirt_meccanica_red_1.jpg", "product_shirt_meccanica_red_2.jpg", "product_shirt_meccanica_red_3.jpg",
				"product_shirt_meccanica_red_4.jpg", "product_shirt_meccanica_black_1.jpg", "product_shirt_meccanica_black_2.jpg", "product_shirt_meccanica_black_3.jpg" };

			for (var i = 0; i < shirtMeccanicaImages.Length; ++i)
			{
				shirtMeccanica.ProductPictures.Add(new ProductPicture
				{
					Picture = CreatePicture(File.ReadAllBytes(_sampleImagesPath + shirtMeccanicaImages[i]), "image/jpeg", shirtMeccanicaImages[i].Replace("product_", "").Replace(".jpg", "")),
					DisplayOrder = i + 1
				});
			}

			shirtMeccanica.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
			{
				AllowFiltering = true,
				ShowOnProductPage = true,
				DisplayOrder = 1,
				SpecificationAttributeOption = specOptionCotton
			});

			result.Add(shirtMeccanica);

			// Ladies jacket
			var ladiesJacket = new Product
			{
				ProductType = ProductType.SimpleProduct,
				VisibleIndividually = true,
				Name = "Ladies Sports Jacket",
				MetaTitle = "Ladies Sports Jacket",
				FullDescription = "<p>Lightweight wind and water repellent fabric, lining of soft single jersey knit cuffs on arm and waistband. 2 side pockets with zipper, hood in slightly waisted cut.</p><ul><il>Material: 100% polyamide</il><il>Lining: 65% polyester, 35% cotton</il><il>Lining 2: 100% polyester.</il></ul>",
				Sku = "Fashion-JN1107",
				ManufacturerPartNumber = "JN1107",
				ProductTemplateId = productTemplateSimple.Id,
				AllowCustomerReviews = true,
				Published = true,
				Price = 55.00M,
				OldPrice = 60.00M,
				ProductCost = 20.00M,
				SpecialPrice = 52.99M,
				SpecialPriceStartDateTimeUtc = new DateTime(2017, 5, 1, 0, 0, 0),
				SpecialPriceEndDateTimeUtc = specialPriceEndDate,
				ManageInventoryMethod = ManageInventoryMethod.ManageStock,
				OrderMinimumQuantity = 1,
				OrderMaximumQuantity = 10000,
				StockQuantity = 10000,
				NotifyAdminForQuantityBelow = 1,
				IsShipEnabled = true,
				DeliveryTime = firstDeliveryTime,
				DisplayOrder = 2
			};

			ladiesJacket.ProductCategories.Add(new ProductCategory
			{
				Category = fashionCategory,
				DisplayOrder = 1
			});

			var ladiesJacketImages = new string[] { "product_ladies_jacket_red.jpg", "product_ladies_jacket_orange.jpg", "product_ladies_jacket_green.jpg",
				"product_ladies_jacket_blue.jpg", "product_ladies_jacket_navy.jpg", "product_ladies_jacket_silver.jpg", "product_ladies_jacket_black.jpg" };

			for (var i = 0; i < ladiesJacketImages.Length; ++i)
			{
				ladiesJacket.ProductPictures.Add(new ProductPicture
				{
					Picture = CreatePicture(File.ReadAllBytes(_sampleImagesPath + ladiesJacketImages[i]), "image/jpeg", ladiesJacketImages[i].Replace("product_", "").Replace(".jpg", "")),
					DisplayOrder = i + 1
				});
			}

			ladiesJacket.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
			{
				AllowFiltering = true,
				ShowOnProductPage = true,
				DisplayOrder = 1,
				SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().First(x => x.DisplayOrder == 8).SpecificationAttributeOptions.First(x => x.DisplayOrder == 11)
			});

			result.Add(ladiesJacket);

			// Clark Premium Blue Jeans
			var clarkJeans = new Product
			{
				ProductType = ProductType.SimpleProduct,
				VisibleIndividually = true,
				Name = "Clark Premium Blue Jeans",
				MetaTitle = "Clark Premium Blue Jeans",
				ShortDescription = "Modern Jeans in Easy Comfort Fit",
				FullDescription = "<p>Real five-pocket jeans by Joker with additional, set-up pocket. Thanks to easy comfort fit with normal rise and comfortable leg width suitable for any character.</p><ul><li>Material: softer, lighter premium denim made of 100% cotton.</li><li>Waist (inch): 29-46</li><li>leg (inch): 30 to 38</li></ul>",
				Sku = "Fashion-65986524",
				ProductTemplateId = productTemplateSimple.Id,
				AllowCustomerReviews = true,
				Published = true,
				Price = 109.90M,
				ManageInventoryMethod = ManageInventoryMethod.ManageStock,
				OrderMinimumQuantity = 1,
				OrderMaximumQuantity = 10000,
				StockQuantity = 10000,
				NotifyAdminForQuantityBelow = 1,
				IsShipEnabled = true,
				DeliveryTime = firstDeliveryTime,
				DisplayOrder = 5
			};

			clarkJeans.ProductCategories.Add(new ProductCategory
			{
				Category = fashionCategory,
				DisplayOrder = 1
			});

			clarkJeans.ProductPictures.Add(new ProductPicture
			{
				Picture = CreatePicture(File.ReadAllBytes(_sampleImagesPath + "product_clark_premium_jeans.jpg"), "image/jpeg", "clark_premium_jeans"),
				DisplayOrder = 1
			});

			clarkJeans.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
			{
				AllowFiltering = true,
				ShowOnProductPage = true,
				DisplayOrder = 1,
				SpecificationAttributeOption = specOptionCotton
			});

			result.Add(clarkJeans);


			return result;
		}

		private List<Product> GetFurnitureProducts()
		{
			var result = new List<Product>();
			var productTemplateSimple = _ctx.Set<ProductTemplate>().First(x => x.ViewPath == "Product");
			var thirdDeliveryTime = _ctx.Set<DeliveryTime>().First(x => x.DisplayOrder == 2);
			var furnitureCategory = _ctx.Set<Category>().First(x => x.MetaTitle == "Furniture");
			var specOptionLeather = _ctx.Set<SpecificationAttribute>().First(x => x.DisplayOrder == 8).SpecificationAttributeOptions.First(x => x.DisplayOrder == 5);
			var specOptionWood = _ctx.Set<SpecificationAttribute>().First(x => x.DisplayOrder == 8).SpecificationAttributeOptions.First(x => x.DisplayOrder == 13);
			var specOptionPlastic = _ctx.Set<SpecificationAttribute>().First(x => x.DisplayOrder == 8).SpecificationAttributeOptions.First(x => x.DisplayOrder == 3);
			var specOptionGlass = _ctx.Set<SpecificationAttribute>().First(x => x.DisplayOrder == 8).SpecificationAttributeOptions.First(x => x.DisplayOrder == 14);
			var specOptionSteel = _ctx.Set<SpecificationAttribute>().First(x => x.DisplayOrder == 8).SpecificationAttributeOptions.First(x => x.DisplayOrder == 1);
			var specOptionAluminium = _ctx.Set<SpecificationAttribute>().First(x => x.DisplayOrder == 8).SpecificationAttributeOptions.First(x => x.DisplayOrder == 4);

			// Le Corbusier LC 6 table
			var corbusierTable = new Product
			{
				ProductType = ProductType.SimpleProduct,
				VisibleIndividually = true,
				Name = "Le Corbusier LC 6 dining table (1929)",
				MetaTitle = "Le Corbusier LC 6 dining table (1929)",
				ShortDescription = "Dining table LC 6, designer: Le Corbusier, W x H x D: 225 x 69/74 (adjustable) x 85 cm, substructure: steel pipe, glass plate: Clear or sandblasted, 15 or 19 mm, height-adjustable.",
				FullDescription = "<p>Four small plates carry a glass plate. The structure of the steel pipe is covered in clear structures. The LC6 is a true classic of Bauhaus art and is used in combination with the swivel chairs LC7 as a form-beautiful Le Corbusier dining area. In addition, the table is also increasingly found in offices or in halls. It is height-adjustable and can thus be perfectly adapted to the respective purpose.</p><p>Le Corbusier's beautifully shaped table is available with a clear or sandblasted glass plate. The substructure consists of oval steel tubes.</p>",
				Sku = "Furniture-lc6",
				ProductTemplateId = productTemplateSimple.Id,
				AllowCustomerReviews = true,
				Published = true,
				Price = 749.00M,
				HasTierPrices = true,
				ManageInventoryMethod = ManageInventoryMethod.ManageStock,
				OrderMinimumQuantity = 1,
				OrderMaximumQuantity = 10000,
				StockQuantity = 10000,
				NotifyAdminForQuantityBelow = 1,
				IsShipEnabled = true,
				DeliveryTime = thirdDeliveryTime
			};

			corbusierTable.ProductCategories.Add(new ProductCategory
			{
				Category = furnitureCategory,
				DisplayOrder = 1
			});

			var corbusierTableImages = new string[] { "product_corbusier_lc6_table_1.jpg", "product_corbusier_lc6_table_2.jpg", "product_corbusier_lc6_table_3.jpg",
				"product_corbusier_lc6_table_4.jpg" };

			for (var i = 0; i < corbusierTableImages.Length; ++i)
			{
				corbusierTable.ProductPictures.Add(new ProductPicture
				{
					Picture = CreatePicture(File.ReadAllBytes(_sampleImagesPath + corbusierTableImages[i]), "image/jpeg", corbusierTableImages[i].Replace("product_", "").Replace(".jpg", "")),
					DisplayOrder = i + 1
				});
			}

			corbusierTable.TierPrices.Add(new TierPrice
			{
				Quantity = 2,
				Price = 647.10M
			});
			corbusierTable.TierPrices.Add(new TierPrice
			{
				Quantity = 4,
				Price = 636.65M
			});

			corbusierTable.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
			{
				AllowFiltering = true,
				ShowOnProductPage = true,
				DisplayOrder = 1,
				SpecificationAttributeOption = specOptionSteel
			});
			corbusierTable.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
			{
				AllowFiltering = true,
				ShowOnProductPage = true,
				DisplayOrder = 2,
				SpecificationAttributeOption = specOptionGlass
			});

			result.Add(corbusierTable);

			// Ball Chair
			var ballChair = new Product
			{
				ProductType = ProductType.SimpleProduct,
				VisibleIndividually = true,
				Name = "Eero Aarnio Ball Chair (1966)",
				MetaTitle = "Eero Aarnio Ball Chair (1966)",
				FullDescription = "<p>The ball chair, or also called the globe chair, is a real masterpiece of the legendary designer Eero Aarnio. The ball chair from the Sixties has written designer history. The egg designed armchair rests on a trumpet foot and is not lastly appreciated due to its shape and the quiet atmosphere inside this furniture. The design of the furniture body allows noise and disturbing outer world elements in the Hintergurnd us. A place as created for resting and relaxing. With its wide range of colours, the eyeball chair fits in every living and working environment. A chair that stands out for its timeless design and always has the modern look. The ball chair is 360° to rotate to change the view of the surroundings. The outer shell in fiberglass white or black. The upholstery is mixed in leather or linen.</p><p>Dimension: Width 102 cm, depth 87 cm, height 124 cm, seat height: 44 cm.</p>",
				Sku = "Furniture-ball-chair",
				ProductTemplateId = productTemplateSimple.Id,
				AllowCustomerReviews = true,
				Published = true,
				Price = 2199.00M,
				HasTierPrices = true,
				ManageInventoryMethod = ManageInventoryMethod.ManageStock,
				OrderMinimumQuantity = 1,
				OrderMaximumQuantity = 10000,
				StockQuantity = 10000,
				NotifyAdminForQuantityBelow = 1,
				IsShipEnabled = true,
				DeliveryTime = thirdDeliveryTime
			};

			ballChair.ProductCategories.Add(new ProductCategory
			{
				Category = furnitureCategory,
				DisplayOrder = 1
			});

			ballChair.ProductPictures.Add(new ProductPicture
			{
				Picture = CreatePicture(File.ReadAllBytes(_sampleImagesPath + "product_ball_chair_white.jpg"), "image/jpeg", "ball_chair_white"),
				DisplayOrder = 1
			});
			ballChair.ProductPictures.Add(new ProductPicture
			{
				Picture = CreatePicture(File.ReadAllBytes(_sampleImagesPath + "product_ball_chair_black.jpg"), "image/jpeg", "ball_chair_black"),
				DisplayOrder = 2
			});

			ballChair.TierPrices.Add(new TierPrice
			{
				Quantity = 2,
				Price = 1979.10M
			});
			ballChair.TierPrices.Add(new TierPrice
			{
				Quantity = 4,
				Price = 1869.15M
			});

			ballChair.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
			{
				AllowFiltering = true,
				ShowOnProductPage = true,
				DisplayOrder = 1,
				SpecificationAttributeOption = specOptionPlastic
			});
			ballChair.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
			{
				AllowFiltering = true,
				ShowOnProductPage = true,
				DisplayOrder = 2,
				SpecificationAttributeOption = specOptionLeather
			});

			result.Add(ballChair);

			// Lounge chair
			var loungeChair = new Product
			{
				ProductType = ProductType.SimpleProduct,
				VisibleIndividually = true,
				Name = "Charles Eames Lounge Chair (1956)",
				MetaTitle = "Charles Eames Lounge Chair (1956)",
				ShortDescription = "Club lounge chair, designer: Charles Eames, width 80 cm, depth 80 cm, height 60 cm, seat shell: plywood, foot (rotatable): Aluminium casting, cushion (upholstered) with leather cover.",
				FullDescription = "<p>That's how you sit in a baseball glove. In any case, this was one of the ideas Charles Eames had in mind when designing this club chair. The lounge chair should be a comfort armchair, in which one can sink luxuriously. Through the construction of three interconnected, movable seat shells and a comfortable upholstery Charles Eames succeeded in the implementation. In fact, the club armchair with a swiveling foot is a contrast to the Bauhaus characteristics that emphasized minimalism and functionality. Nevertheless, he became a classic of Bauhaus history and still provides in many living rooms and clubs for absolute comfort with style.</p><p>Dimensions: Width 80 cm, depth 60 cm, height total 80 cm (height backrest: 60 cm). CBM: 0.70.</p><p>Lounge chair with seat shell of laminated curved plywood with rosewood veneer, walnut nature or in black. Rotatable base made of aluminium cast black with polished edges or optionally fully chromed. Elaborate upholstery of pillows in leather.</p><p>All upholstery units are removable at the Eames Lounge chair (seat, armrest, backrest, headrest).</p>",
				Sku = "Furniture-lounge-chair",
				ProductTemplateId = productTemplateSimple.Id,
				AllowCustomerReviews = true,
				Published = true,
                ShowOnHomePage = true,
				Price = 1799.00M,
				OldPrice = 1999.00M,
				HasTierPrices = true,
				ManageInventoryMethod = ManageInventoryMethod.ManageStock,
				OrderMinimumQuantity = 1,
				OrderMaximumQuantity = 10000,
				StockQuantity = 10000,
				NotifyAdminForQuantityBelow = 1,
				IsShipEnabled = true,
				DeliveryTime = thirdDeliveryTime
			};

			loungeChair.ProductCategories.Add(new ProductCategory
			{
				Category = furnitureCategory,
				DisplayOrder = 1
			});

			loungeChair.ProductPictures.Add(new ProductPicture
			{
				Picture = CreatePicture(File.ReadAllBytes(_sampleImagesPath + "product_charles_eames_lounge_chair_white.jpg"), "image/jpeg", "charles_eames_lounge_chair_white"),
				DisplayOrder = 1
			});
			loungeChair.ProductPictures.Add(new ProductPicture
			{
				Picture = CreatePicture(File.ReadAllBytes(_sampleImagesPath + "product_charles_eames_lounge_chair_black.jpg"), "image/jpeg", "charles_eames_lounge_chair_black"),
				DisplayOrder = 2
			});

			loungeChair.TierPrices.Add(new TierPrice
			{
				Quantity = 2,
				Price = 1709.05M
			});
			loungeChair.TierPrices.Add(new TierPrice
			{
				Quantity = 4,
				Price = 1664.08M
			});
			loungeChair.TierPrices.Add(new TierPrice
			{
				Quantity = 6,
				Price = 1619.10M
			});

			loungeChair.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
			{
				AllowFiltering = true,
				ShowOnProductPage = true,
				DisplayOrder = 1,
				SpecificationAttributeOption = specOptionWood
			});
			loungeChair.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
			{
				AllowFiltering = true,
				ShowOnProductPage = true,
				DisplayOrder = 2,
				SpecificationAttributeOption = specOptionLeather
			});
			loungeChair.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
			{
				AllowFiltering = true,
				ShowOnProductPage = true,
				DisplayOrder = 3,
				SpecificationAttributeOption = specOptionAluminium
			});

			result.Add(loungeChair);

			// Cube chair
			var cubeChair = new Product
			{
				ProductType = ProductType.SimpleProduct,
				VisibleIndividually = true,
				Name = "Josef Hoffmann cube chair (1910)",
				MetaTitle = "Josef Hoffmann cube chair (1910)",
				ShortDescription = "Armchair Cube, Designer: Josef Hoffmann, width 93 cm, depth 72 cm, height 77 cm, basic frame: solid beech wood, upholstery: solid polyurethane foam (shape resistant), Upholstery: leather",
				FullDescription = "<p>The cube chair by Josef Hoffmann holds what the name promises and that is the same in two respects. It consists of many squares, both in terms of construction and in relation to the design of the surface. In addition, the cube, with its purely geometric form, was a kind of harbinger of cubism. The chair by Josef Hoffmann was designed in 1910 and still stands today as a replica in numerous business and residential areas.</p><p>Originally, the cube was a club chair. Together with the two-and three-seater sofa of the series, a cosy sitting area with a sophisticated charisma is created. The basic frame of the armchair is made of wood. The form-resistant upholstery is covered with leather and has been shaped visually to squares with a special sewing.</p><p>Dimensions: Width 93 cm, depth 72 cm, height 77 cm. CBM: 0.70.</p>",
				Sku = "Furniture-cube-chair",
				ProductTemplateId = productTemplateSimple.Id,
				AllowCustomerReviews = true,
				Published = true,
                ShowOnHomePage = true,
				Price = 2299.00M,
				HasTierPrices = true,
				ManageInventoryMethod = ManageInventoryMethod.ManageStock,
				OrderMinimumQuantity = 1,
				OrderMaximumQuantity = 10000,
				StockQuantity = 10000,
				NotifyAdminForQuantityBelow = 1,
				IsShipEnabled = true,
				DeliveryTime = thirdDeliveryTime
			};

			cubeChair.ProductCategories.Add(new ProductCategory
			{
				Category = furnitureCategory,
				DisplayOrder = 1
			});

			cubeChair.ProductPictures.Add(new ProductPicture
			{
				Picture = CreatePicture(File.ReadAllBytes(_sampleImagesPath + "product_hoffmann_cube_chair_black.jpg"), "image/jpeg", "hoffmann_cube_chair_black"),
				DisplayOrder = 1
			});

			cubeChair.TierPrices.Add(new TierPrice
			{
				Quantity = 4,
				Price = 1899.05M
			});
			cubeChair.TierPrices.Add(new TierPrice
			{
				Quantity = 6,
				Price = 1799.10M
			});

			cubeChair.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
			{
				AllowFiltering = true,
				ShowOnProductPage = true,
				DisplayOrder = 1,
				SpecificationAttributeOption = specOptionLeather
			});

			result.Add(cubeChair);

			return result;
		}

		public IList<Product> Products()
		{
			#region definitions

			// Pictures
			var sampleImagesPath = this._sampleImagesPath;

			// Downloads
			var sampleDownloadsPath = this._sampleDownloadsPath;

			// Templates
			var productTemplate = _ctx.Set<ProductTemplate>().First(x => x.ViewPath == "Product");

			var firstDeliveryTime = _ctx.Set<DeliveryTime>().First(sa => sa.DisplayOrder == 0);

            var specialPriceEndDate = DateTime.UtcNow.AddMonths(1);

            #endregion definitions

            #region category golf

            var categoryGolf = this._ctx.Set<Category>().First(c => c.Alias == "Golf");

            #region product Titleist SM6 Tour Chrome

            var productTitleistSM6TourChrome = new Product()
            {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Titleist SM6 Tour Chrome",
                IsEsd = false,
                ShortDescription = "For golfers who want maximum impact control and feedback.",
                FullDescription = "​​<p><strong>Inspired by the best iron players in the world</strong> </p> <p>The new 'Spin Milled 6' wages establish a new performance class in three key areas of the Wedge game: precise length steps, bounce and maximum spin. </p> <p>   <br />   For each loft the center of gravity of the wedge is determined individually. Therefore, the SM6 offers a particularly precise length and flight curve control combined with great impact.   <br />   Bob Vokey's tourer-puffed sole cleat allows all golfers more bounce, adapted to their personal swing profile and the respective ground conditions. </p> <p>   <br />   A new, parallel face texture was developed for the absolutely exact and with 100% quality control machined grooves. The result is a consistently higher edge sharpness for more spin. </p> <p> </p> <ul>   <li>Precise lengths and flight curve control thanks to progressively placed center of gravity.</li>   <li>Improved bounce due to Bob Vokey's proven soles.</li>   <li>TX4 grooves produce more spin through a new surface and edge sharpness.</li>   <li>Multiple personalization options.</li> </ul> ",
                Sku = "P-7004",
                ProductTemplateId = productTemplate.Id,
                AllowCustomerReviews = true,
                Published = true,
                MetaTitle = "Titleist SM6 Tour Chrome",
                Price = 164.95M,
                OldPrice= 199.95M,
                IsGiftCard = false,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                IsShipEnabled = true,
                DeliveryTime = _ctx.Set<DeliveryTime>().Where(sa => sa.DisplayOrder == 2).Single()
            };

            productTitleistSM6TourChrome.ProductCategories.Add(new ProductCategory() { Category = categoryGolf, DisplayOrder = 1 });

            productTitleistSM6TourChrome.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_titleist_sm6_tour_chrome.jpg"), "image/png", GetSeName(productTitleistSM6TourChrome.Name)),
                DisplayOrder = 1,
            });

            productTitleistSM6TourChrome.ProductManufacturers.Add(new ProductManufacturer()
            {
                Manufacturer = _ctx.Set<Manufacturer>().Where(c => c.Name == "Titleist").Single(),
                DisplayOrder = 1,
            });

            #endregion product Titleist SM6 Tour Chrome

            #region product Titleist Pro V1x

            var productTitleistProV1x = new Product()
            {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Titleist Pro V1x",
                IsEsd = false,
                ShortDescription = "Golf ball with high ball flight",
                FullDescription = "​​The top players rely on the new Titleist Pro V1x. High ball flight, soft feel and more spin in the short game are the advantages of the V1x version. Perfect performance from the leading manufacturer. The new Titleist Pro V1 golf ball is exactly defined and promises penetrating ball flight with very soft hit feeling.",
                Sku = "P-7001",
                ProductTemplateId = productTemplate.Id,
                AllowCustomerReviews = true,
                Published = true,
                MetaTitle = "Titleist Pro V1x",
                Price = 2.1M,
                IsGiftCard = false,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                IsShipEnabled = true,
                DeliveryTime = _ctx.Set<DeliveryTime>().Where(sa => sa.DisplayOrder == 2).Single()
            };

            productTitleistProV1x.ProductCategories.Add(new ProductCategory() { Category = categoryGolf, DisplayOrder = 1 });

            productTitleistProV1x.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_titleist-pro-v1x.jpg"), "image/png", GetSeName(productTitleistProV1x.Name)),
                DisplayOrder = 1,
            });

            productTitleistProV1x.ProductManufacturers.Add(new ProductManufacturer()
            {
                Manufacturer = _ctx.Set<Manufacturer>().Where(c => c.Name == "Titleist").Single(),
                DisplayOrder = 1,
            });

            #endregion product Titleist Pro V1x

            #region product Supreme Golfball

            var productSupremeGolfball = new Product()
            {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Supreme Golfball",
                IsEsd = false,
                ShortDescription = "Training balls with perfect flying characteristics",
                FullDescription = "​Perfect golf exercise ball with the characteristics like the 'original', but in a glass-fracture-proof execution. Massive core, an ideal training ball for yard and garden. Colors: white, yellow, orange.",
                Sku = "P-7002",
                ProductTemplateId = productTemplate.Id,
                AllowCustomerReviews = true,
                Published = true,
                MetaTitle = "Supreme Golfball",
                Price = 1.9M,
                IsGiftCard = false,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                IsShipEnabled = true,
                DeliveryTime = _ctx.Set<DeliveryTime>().Where(sa => sa.DisplayOrder == 2).Single()
            };

            productSupremeGolfball.ProductCategories.Add(new ProductCategory() { Category = categoryGolf, DisplayOrder = 1 });

            productSupremeGolfball.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_supremeGolfball_1.jpg"), "image/png", GetSeName(productSupremeGolfball.Name)),
                DisplayOrder = 1,
            });

            productSupremeGolfball.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_supremeGolfball_2.jpg"), "image/png", GetSeName(productSupremeGolfball.Name)),
                DisplayOrder = 1,
            });

            productSupremeGolfball.ProductManufacturers.Add(new ProductManufacturer()
            {
                Manufacturer = _ctx.Set<Manufacturer>().Where(c => c.Name == "Titleist").Single(),
                DisplayOrder = 1,
            });

            #endregion product Supreme Golfball

            #region product GBB Epic Sub Zero Driver

            var productGBBEpicSubZeroDriver = new Product()
            {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "GBB Epic Sub Zero Driver",
                IsEsd = false,
                ShortDescription = "Low spin for good golfing!",
                FullDescription = "Your game wins with the GBB Epic Sub Zero Driver. A golf club with an extremely low spin and the phenomenal high-speed characteristic.",
                Sku = "P-7003",
                ProductTemplateId = productTemplate.Id,
                AllowCustomerReviews = true,
                Published = true,
                MetaTitle = "GBB Epic Sub Zero Driver",
                Price = 489M,
                IsGiftCard = false,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                IsShipEnabled = true,
                DeliveryTime = _ctx.Set<DeliveryTime>().Where(sa => sa.DisplayOrder == 2).Single()
            };

            productGBBEpicSubZeroDriver.ProductCategories.Add(new ProductCategory() { Category = categoryGolf, DisplayOrder = 1 });

            productGBBEpicSubZeroDriver.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_gbb-epic-sub-zero-driver.jpg"), "image/png", GetSeName(productGBBEpicSubZeroDriver.Name)),
                DisplayOrder = 1,
            });

            productGBBEpicSubZeroDriver.ProductManufacturers.Add(new ProductManufacturer()
            {
                Manufacturer = _ctx.Set<Manufacturer>().Where(c => c.Name == "Titleist").Single(),
                DisplayOrder = 1,
            });

            #endregion product GBB Epic Sub Zero Driver

            #endregion category golf

            #region category Soccer

            var categorySoccer = this._ctx.Set<Category>().First(c => c.Alias == "Soccer");

            #region product Nike Strike Football

            var productNikeStrikeFootball = new Product()
            {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Nike Strike Football",
                IsEsd = false,
                ShortDescription = "GREAT TOUCH. HIGH VISIBILITY.",
                FullDescription = "<p><strong>Enhance play everyday, with the Nike Strike Football. </strong> </p> <p>Reinforced rubber retains its shape for confident and consistent control. A stand out Visual Power graphic in black, green and orange is best for ball tracking, despite dark or inclement conditions. </p> <ul>   <li>Visual Power graphic helps give a true read on flight trajectory.</li>   <li>Textured casing offers superior touch.</li>   <li>Reinforced rubber bladder supports air and shape retention.</li>   <li>66% rubber/ 15% polyurethane/ 13% polyester/ 7% EVA.</li> </ul> ",  
                Sku = "P-5004",
                ProductTemplateId = productTemplate.Id,
                AllowCustomerReviews = true,
                Published = true,
                MetaTitle = "Nike Strike Football",
                Price = 59.90M,
                OldPrice = 69.90M,
                IsGiftCard = false,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                IsShipEnabled = true,
                DeliveryTime = _ctx.Set<DeliveryTime>().Where(sa => sa.DisplayOrder == 2).Single()
            };

            productNikeStrikeFootball.ProductCategories.Add(new ProductCategory() { Category = categorySoccer, DisplayOrder = 1 });

            productNikeStrikeFootball.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "products_nike-strike-football.jpg"), "image/png", GetSeName(productNikeStrikeFootball.Name)),
                DisplayOrder = 1,
            });

            productNikeStrikeFootball.ProductManufacturers.Add(new ProductManufacturer()
            {
                Manufacturer = _ctx.Set<Manufacturer>().Where(c => c.Name == "Nike").Single(),
                DisplayOrder = 1,
            });

            //attributes
            productNikeStrikeFootball.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 3,
                // Manufacturer -> Nike
                SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder ==20).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 20).Single()
            });
            productNikeStrikeFootball.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 3,
                // Material -> rubber
                SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 8).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 12).Single()
            });

            #region tierPrieces
            productNikeStrikeFootball.TierPrices.Add(new TierPrice()
            {
                Quantity = 6,
                Price = 26.90M
            });
            productNikeStrikeFootball.TierPrices.Add(new TierPrice()
            {
                Quantity = 12,
                Price = 24.90M
            });
            productNikeStrikeFootball.TierPrices.Add(new TierPrice()
            {
                Quantity = 24,
                Price = 22.90M
            });
            productNikeStrikeFootball.HasTierPrices = true;
            #endregion tierPrieces

            #endregion product Nike Strike Football

            #region product Evopower 5.3 Trainer HS Ball

            var productNikeEvoPowerBall = new Product()
            {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Evopower 5.3 Trainer HS Ball",
                IsEsd = false,
                ShortDescription = "Entry level training ball.",
                FullDescription = "<p><strong>Entry level training ball.</strong></ p >< p > Constructed from 32 panels with equal surface areas for reduced seam-stress and a perfectly round shape.Handstitched panels with multilayered woven backing for enhanced stability and aerodynamics.</ p >",
                Sku = "P-5003",
                ProductTemplateId = productTemplate.Id,
                AllowCustomerReviews = true,
                Published = true,
                MetaTitle = "Evopower 5.3 Trainer HS Ball",
                Price = 59.90M,
                IsGiftCard = false,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                IsShipEnabled = true,
                DeliveryTime = _ctx.Set<DeliveryTime>().Where(sa => sa.DisplayOrder == 2).Single()
            };

            productNikeEvoPowerBall.ProductCategories.Add(new ProductCategory() { Category = categorySoccer, DisplayOrder = 1 });

            productNikeEvoPowerBall.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_nike-vopower-53-trainer-hs-ball.jpg"), "image/png", GetSeName(productNikeEvoPowerBall.Name)),
                DisplayOrder = 1,
            });

            productNikeEvoPowerBall.ProductManufacturers.Add(new ProductManufacturer()
            {
                Manufacturer = _ctx.Set<Manufacturer>().Where(c => c.Name == "Nike").Single(),
                DisplayOrder = 1,
            });

            //attributes
            productNikeEvoPowerBall.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 3,
                // Manufacturer -> Nike
                SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 20).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 20).Single()
            });
            productNikeEvoPowerBall.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 3,
                // Material -> leather
                SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 8).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 5).Single()
            });

            #endregion Evopower 5.3 Trainer HS Ball

            #region product Torfabrik official game ball

            var productTorfabrikOfficialGameBall = new Product()
            {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Torfabrik official game ball",
                IsEsd = false,
                ShortDescription = "Available in different colors",
                FullDescription = "",
                Sku = "P-5002",
                ProductTemplateId = productTemplate.Id,
                AllowCustomerReviews = true,
                Published = true,
                MetaTitle = "Torfabrik official game ball",
                Price = 59.90M,
                IsGiftCard = false,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                IsShipEnabled = true,
                DeliveryTime = _ctx.Set<DeliveryTime>().Where(sa => sa.DisplayOrder == 2).Single()
            };

            productTorfabrikOfficialGameBall.ProductCategories.Add(new ProductCategory() { Category = categorySoccer, DisplayOrder = 1 });

            productTorfabrikOfficialGameBall.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_torfabrik-offizieller-spielball_white.png"), "image/png", GetSeName(productTorfabrikOfficialGameBall.Name) + "white"),
                DisplayOrder = 1,
            });

            productTorfabrikOfficialGameBall.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_torfabrik-offizieller-spielball_red.png"), "image/png", GetSeName(productTorfabrikOfficialGameBall.Name) + "red"),
                DisplayOrder = 1,
            });

            productTorfabrikOfficialGameBall.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_torfabrik-offizieller-spielball_yellow.png"), "image/png", GetSeName(productTorfabrikOfficialGameBall.Name) + "yellow"),
                DisplayOrder = 1,
            });

            productTorfabrikOfficialGameBall.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_torfabrik-offizieller-spielball_blue.png"), "image/png", GetSeName(productTorfabrikOfficialGameBall.Name) + "blue"),
                DisplayOrder = 1,
            });

            productTorfabrikOfficialGameBall.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_torfabrik-offizieller-spielball_green.png"), "image/png", GetSeName(productTorfabrikOfficialGameBall.Name) + "green"),
                DisplayOrder = 1,
            });

            productTorfabrikOfficialGameBall.ProductManufacturers.Add(new ProductManufacturer()
            {
                Manufacturer = _ctx.Set<Manufacturer>().Where(c => c.Name == "Adidas").Single(),
                DisplayOrder = 1,
            });

            //attributes
            productTorfabrikOfficialGameBall.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 3,
                // Manufacturer -> Adidas
                SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 20).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 19).Single()
            });
            productTorfabrikOfficialGameBall.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 3,
                // Material -> leather
                SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 8).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 5).Single()
            });


            #endregion Torfabrik official game ball

            #region product Adidas TANGO SALA BALL

            var productAdidasTangoSalaBall = new Product()
            {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Adidas TANGO SALA BALL",
                IsEsd = false,
                ShortDescription = "In different colors",
                FullDescription = "<p><strong>TANGO SALA BALL</strong>   <br />   A SALA BALL TO MATCH YOUR INDOOR PLAYMAKING. </p> <p>Take the game indoors. With a design nod to the original Tango ball that set the performance standard, this indoor soccer is designed for low rebound and enhanced control for futsal. Machine-stitched for a soft touch and high durability. </p> <ul>   <li>Machine-stitched for soft touch and high durability</li>   <li>Low rebound for enhanced ball control</li>   <li>Butyl bladder for best air retention</li>   <li>Requires inflation</li>   <li>100% natural rubber</li>   <li>Imported</li> </ul> <p> </p> ",
                Sku = "P-5001",
                ProductTemplateId = productTemplate.Id,
                AllowCustomerReviews = true,
                Published = true,
                MetaTitle = "Adidas TANGO SALA BALL",
                Price = 59.90M,
                IsGiftCard = false,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                IsShipEnabled = true,
                DeliveryTime = _ctx.Set<DeliveryTime>().Where(sa => sa.DisplayOrder == 2).Single()
            };

            productAdidasTangoSalaBall.ProductCategories.Add(new ProductCategory() { Category = categorySoccer, DisplayOrder = 1 });

            productAdidasTangoSalaBall.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_adidas-tango-pasadena-ball-white.png"), "image/png", GetSeName(productAdidasTangoSalaBall.Name) + "-white"),
                DisplayOrder = 1,
            });

            productAdidasTangoSalaBall.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_adidas-tango-pasadena-ball-yellow.jpg"), "image/png", GetSeName(productAdidasTangoSalaBall.Name) + "-yellow"),
                DisplayOrder = 1,
            });

            productAdidasTangoSalaBall.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_adidas-tango-pasadena-ball-red.jpg"), "image/png", GetSeName(productAdidasTangoSalaBall.Name) + "-red"),
                DisplayOrder = 1,
            });

            productAdidasTangoSalaBall.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_adidas-tango-pasadena-ball-green.jpg"), "image/png", GetSeName(productAdidasTangoSalaBall.Name) + "-green"),
                DisplayOrder = 1,
            });

            productAdidasTangoSalaBall.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_adidas-tango-pasadena-ball-gray.jpg"), "image/png", GetSeName(productAdidasTangoSalaBall.Name) + "-gray"),
                DisplayOrder = 1,
            });

            productAdidasTangoSalaBall.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_adidas-tango-pasadena-ball-brown.jpg"), "image/png", GetSeName(productAdidasTangoSalaBall.Name) + "-brown"),
                DisplayOrder = 1,
            });

            productAdidasTangoSalaBall.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_adidas-tango-pasadena-ball-blue.jpg"), "image/png", GetSeName(productAdidasTangoSalaBall.Name) + "-blue"),
                DisplayOrder = 1,
            });

            productAdidasTangoSalaBall.ProductManufacturers.Add(new ProductManufacturer()
            {
                Manufacturer = _ctx.Set<Manufacturer>().Where(c => c.Name == "Adidas").Single(),
                DisplayOrder = 1,
            });

            //attributes
            productAdidasTangoSalaBall.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 3,
                // Manufacturer -> Adidas
                SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 20).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 19).Single()
            });
            productAdidasTangoSalaBall.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 3,
                // Material -> leather
                SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 8).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 5).Single()
            });

            #endregion Adidas TANGO SALA BALL

            #endregion category Soccer

            #region category Basketball

            var categoryBasketball = this._ctx.Set<Category>().First(c => c.Alias == "Basketball");

            #region Wilson Evolution High School Game Basketball

            var productEvolutionHighSchoolGameBasketball = new Product()
            {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Evolution High School Game Basketball",
                IsEsd = false,
                ShortDescription = "For all positions on all levels, match day and every day",
                FullDescription = "<p>The Wilson Evolution High School Game Basketball has exclusive microfiber composite leather construction with deep embossed pebbles to give you the ultimate in feel and control.</p><p>Its patented Cushion Core Technology enhances durability for longer play. This microfiber composite Evolution high school basketball is pebbled with composite channels for better grip, helping players raise their game to the next level.</p><p>For all positions at all levels of play, game day and every day, Wilson delivers the skill-building performance that players demand.</p><p>This regulation-size 29.5' Wilson basketball is an ideal basketball for high school players, and is designed for either recreational use or for league games. It is NCAA and NFHS approved, so you know it's a high-quality basketball that will help you hone your shooting, passing and ball-handling skills.</p><p>Take your team all the way to the championship with the Wilson Evolution High School Game Basketball.</p>",
                Sku = "P-4001",
                ProductTemplateId = productTemplate.Id,
                AllowCustomerReviews = true,
                Published = true,
                MetaTitle = "Evolution High School Game Basketball",
                Price = 25.90M,
                OldPrice = 29.90M,
                IsGiftCard = false,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                IsShipEnabled = true,
                DeliveryTime = _ctx.Set<DeliveryTime>().Where(sa => sa.DisplayOrder == 2).Single()
            };

            productEvolutionHighSchoolGameBasketball.ProductCategories.Add(new ProductCategory() { Category = categoryBasketball, DisplayOrder = 1 });

            productEvolutionHighSchoolGameBasketball.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_evolution-high-school-game-basketball.jpg"), "image/png", GetSeName(productEvolutionHighSchoolGameBasketball.Name)),
                DisplayOrder = 1,
            });

            productEvolutionHighSchoolGameBasketball.ProductManufacturers.Add(new ProductManufacturer()
            {
                Manufacturer = _ctx.Set<Manufacturer>().Where(c => c.Name == "Adidas").Single(),
                DisplayOrder = 1,
            });

            #region tierPrieces
            productEvolutionHighSchoolGameBasketball.TierPrices.Add(new TierPrice()
            {
                Quantity = 6,
                Price = 24.90M
            });
            productEvolutionHighSchoolGameBasketball.TierPrices.Add(new TierPrice()
            {
                Quantity = 12,
                Price = 22.90M
            });
            productEvolutionHighSchoolGameBasketball.TierPrices.Add(new TierPrice()
            {
                Quantity = 24,
                Price = 20.90M
            });
            productEvolutionHighSchoolGameBasketball.HasTierPrices = true;
            #endregion tierPrieces


            #endregion Wilson Evolution High School Game Basketball


            #region All Court Basketball

            var productAllCourtBasketball = new Product()
            {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "All-Court Basketball",
                IsEsd = false,
                ShortDescription = "A durable Basketball for all surfaces",
                FullDescription = "<p><strong>All-Court Prep Ball</strong> </p> <p>A durable basketball for all surfaces. </p> <p>Whether on parquet or on asphalt - the adidas All-Court Prep Ball hat has only one goal: the basket. This basketball is made of durable artificial leather, was also predestined for indoor games also for outdoor games. </p> <ul>   <li>Composite cover made of artificial leather</li>   <li>suitable for indoors and outdoors</li>   <li>Delivered unpumped</li> </ul> ",
                Sku = "P-4002",
                ProductTemplateId = productTemplate.Id,
                AllowCustomerReviews = true,
                Published = true,
                MetaTitle = "All-Court Basketball",
                Price = 25.90M,
                IsGiftCard = false,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                IsShipEnabled = true,
                DeliveryTime = _ctx.Set<DeliveryTime>().Where(sa => sa.DisplayOrder == 2).Single()
            };

            productAllCourtBasketball.ProductCategories.Add(new ProductCategory() { Category = categoryBasketball, DisplayOrder = 1 });

            productAllCourtBasketball.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_all-court-basketball.png"), "image/png", GetSeName(productAllCourtBasketball.Name)),
                DisplayOrder = 1,
            });

            productAllCourtBasketball.ProductManufacturers.Add(new ProductManufacturer()
            {
                Manufacturer = _ctx.Set<Manufacturer>().Where(c => c.Name == "Adidas").Single(),
                DisplayOrder = 1,
            });

            #endregion All Court Basketball

            #endregion category Basketball

            #region category sunglasses

            var categorySunglasses = this._ctx.Set<Category>().First(c => c.Alias == "Sunglasses");

            #region product Top bar

            var productRayBanTopBar = new Product()
            {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Ray-Ban Top Bar RB 3183",
                IsEsd = false,
                ShortDescription = "The Ray-Ban Original Wayfarer is the most famous style in the history of sunglasses. With the original design from 1952 the Wayfarer is popular with celebrities, musicians, artists and fashion experts.",
                FullDescription = "<p>The Ray-Ban ® RB3183 sunglasses give me their aerodynamic shape a reminiscence of speed.</p><p>A rectangular shape and the classic Ray-Ban logo imprinted on the straps characterize this light Halbrand model.</p>",
                Sku = "P-3004",
                ProductTemplateId = productTemplate.Id,
                AllowCustomerReviews = true,
                Published = true,
                MetaTitle = "Ray-Ban Top Bar RB 3183",
                Price = 139M,
                IsGiftCard = false,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                IsShipEnabled = true,
                DeliveryTime = _ctx.Set<DeliveryTime>().Where(sa => sa.DisplayOrder == 2).Single()
            };

            productRayBanTopBar.ProductCategories.Add(new ProductCategory() { Category = categorySunglasses, DisplayOrder = 1 });

            productRayBanTopBar.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_RayBanTopBar_1.jpg"), "image/png", GetSeName(productRayBanTopBar.Name)),
                DisplayOrder = 1,
            });

            productRayBanTopBar.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_RayBanTopBar_2.jpg"), "image/png", GetSeName(productRayBanTopBar.Name)),
                DisplayOrder = 1,
            });

            productRayBanTopBar.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_RayBanTopBar_3.jpg"), "image/png", GetSeName(productRayBanTopBar.Name)),
                DisplayOrder = 1,
            });

            productRayBanTopBar.ProductManufacturers.Add(new ProductManufacturer()
            {
                Manufacturer = _ctx.Set<Manufacturer>().Where(c => c.Name == "Ray-Ban").Single(),
                DisplayOrder = 1,
            });

            #endregion product Top bar

            #region product ORIGINAL WAYFARER AT COLLECTION

            var productOriginalWayfarer = new Product()
            {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "ORIGINAL WAYFARER AT COLLECTION",
                IsEsd = false,
                ShortDescription = "The Ray-Ban Original Wayfarer is the most famous style in the history of sunglasses. With the original design from 1952 the Wayfarer is popular with celebrities, musicians, artists and fashion experts.",
                FullDescription = "<p><strong>Radar® EV Path™ PRIZM™ Road</strong> </p> <p>A new milestone in the heritage of performance, Radar® EV takes breakthroughs of a revolutionary design even further with a taller lens that extends the upper field of view. From the comfort and protection of the O Matter® frame to the grip of its Unobtanium® components, this premium design builds on the legacy of Radar innovation and style. </p> <p><strong>Features</strong> </p> <ul>   <li>PRIZM™ is a revolutionary lens technology that fine-tunes vision for specific sports and environments. See what you’ve been missing. Click here to learn more about Prizm Lens Technology.</li>   <li>Path lenses enhance performance if traditional lenses touch your cheeks and help extend the upper field of view</li>   <li>Engineered for maximized airflow for optimal ventilation to keep you cool</li>   <li>Unobtanium® earsocks and nosepads keep glasses in place, increasing grip despite perspiration</li>   <li>Interchangeable Lenses let you change lenses in seconds to optimize vision in any sport environment</li> </ul>",
                Sku = "P-3003",
                ProductTemplateId = productTemplate.Id,
                AllowCustomerReviews = true,
                Published = true,
                MetaTitle = "ORIGINAL WAYFARER AT COLLECTION",
                Price = 149M,
                IsGiftCard = false,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                IsShipEnabled = true,
                DeliveryTime = _ctx.Set<DeliveryTime>().Where(sa => sa.DisplayOrder == 2).Single()
            };

            productOriginalWayfarer.ProductCategories.Add(new ProductCategory() { Category = categorySunglasses, DisplayOrder = 1 });

            productOriginalWayfarer.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_productOriginalWayfarer_1.jpg"), "image/png", GetSeName(productOriginalWayfarer.Name) + "_blue-gray-classic-black"),
                DisplayOrder = 1,
            });

            productOriginalWayfarer.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_productOriginalWayfarer_2.jpg"), "image/png", GetSeName(productOriginalWayfarer.Name) + "_blue-gray-classic-black"),
                DisplayOrder = 1,
            });

            productOriginalWayfarer.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_productOriginalWayfarer_3.jpg"), "image/png", GetSeName(productOriginalWayfarer.Name) + "_gray-course-black"),
                DisplayOrder = 1,
            });

            productOriginalWayfarer.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_productOriginalWayfarer_4.jpg"), "image/png", GetSeName(productOriginalWayfarer.Name) + "_brown-course-havana"),
                DisplayOrder = 1,
            });

            productOriginalWayfarer.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_productOriginalWayfarer_5.jpg"), "image/png", GetSeName(productOriginalWayfarer.Name) + "_green-classic-havana-black"),
                DisplayOrder = 1,
            });

            productOriginalWayfarer.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_productOriginalWayfarer_6.jpg"), "image/png", GetSeName(productOriginalWayfarer.Name) + "_blue-gray-classic-black"),
                DisplayOrder = 1,
            });

            productOriginalWayfarer.ProductManufacturers.Add(new ProductManufacturer()
            {
                Manufacturer = _ctx.Set<Manufacturer>().Where(c => c.Name == "Ray-Ban").Single(),
                DisplayOrder = 1,
            });

            #endregion product ORIGINAL WAYFARER AT COLLECTION

            #region product Radar EV Prizm Sports Sunglasses

            var productRadarEVPrizmSportsSunglasses = new Product()
            {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Radar EV Prizm Sports Sunglasses",
                IsEsd = false,
                ShortDescription = "",
                FullDescription = "<p><strong>Radar® EV Path™ PRIZM™ Road</strong> </p> <p>A new milestone in the heritage of performance, Radar® EV takes breakthroughs of a revolutionary design even further with a taller lens that extends the upper field of view. From the comfort and protection of the O Matter® frame to the grip of its Unobtanium® components, this premium design builds on the legacy of Radar innovation and style. </p> <p><strong>Features</strong> </p> <ul>   <li>PRIZM™ is a revolutionary lens technology that fine-tunes vision for specific sports and environments. See what you’ve been missing. Click here to learn more about Prizm Lens Technology.</li>   <li>Path lenses enhance performance if traditional lenses touch your cheeks and help extend the upper field of view</li>   <li>Engineered for maximized airflow for optimal ventilation to keep you cool</li>   <li>Unobtanium® earsocks and nosepads keep glasses in place, increasing grip despite perspiration</li>   <li>Interchangeable Lenses let you change lenses in seconds to optimize vision in any sport environment</li> </ul>",
                Sku = "P-3001",
                ProductTemplateId = productTemplate.Id,
                AllowCustomerReviews = true,
                Published = true,
                MetaTitle = "Radar EV Prizm Sports Sunglasses",
                Price = 149M,
                IsGiftCard = false,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                IsShipEnabled = true,
                DeliveryTime = _ctx.Set<DeliveryTime>().Where(sa => sa.DisplayOrder == 2).Single()
            };

            productRadarEVPrizmSportsSunglasses.ProductCategories.Add(new ProductCategory() { Category = categorySunglasses, DisplayOrder = 1 });

            productRadarEVPrizmSportsSunglasses.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_radar_ev_prizm.jpg"), "image/png", GetSeName(productRadarEVPrizmSportsSunglasses.Name)),
                DisplayOrder = 1,
            });

            productRadarEVPrizmSportsSunglasses.ProductManufacturers.Add(new ProductManufacturer()
            {
                Manufacturer = _ctx.Set<Manufacturer>().Where(c => c.Name == "Oakley").Single(),
                DisplayOrder = 1,
            });

            #endregion product Radar EV Prizm Sports Sunglasses

            #region product Custom Flak Sunglasses

            var productCustomFlakSunglasses = new Product()
            {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Custom Flak Sunglasses",
                IsEsd = false,
                ShortDescription = "",
                FullDescription = "<p><strong>Radar® EV Path™ PRIZM™ Road</strong> </p> <p>A new milestone in the heritage of performance, Radar® EV takes breakthroughs of a revolutionary design even further with a taller lens that extends the upper field of view. From the comfort and protection of the O Matter® frame to the grip of its Unobtanium® components, this premium design builds on the legacy of Radar innovation and style. </p> <p><strong>Features</strong> </p> <ul>   <li>PRIZM™ is a revolutionary lens technology that fine-tunes vision for specific sports and environments. See what you’ve been missing. Click here to learn more about Prizm Lens Technology.</li>   <li>Path lenses enhance performance if traditional lenses touch your cheeks and help extend the upper field of view</li>   <li>Engineered for maximized airflow for optimal ventilation to keep you cool</li>   <li>Unobtanium® earsocks and nosepads keep glasses in place, increasing grip despite perspiration</li>   <li>Interchangeable Lenses let you change lenses in seconds to optimize vision in any sport environment</li> </ul>",
                Sku = "P-3002",
                ProductTemplateId = productTemplate.Id,
                AllowCustomerReviews = true,
                Published = true,
                MetaTitle = "Custom Flak Sunglasses",
                Price = 179M,
                IsGiftCard = false,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                IsShipEnabled = true,
                DeliveryTime = _ctx.Set<DeliveryTime>().Where(sa => sa.DisplayOrder == 2).Single()
            };

            productCustomFlakSunglasses.ProductCategories.Add(new ProductCategory() { Category = categorySunglasses, DisplayOrder = 1 });

            productCustomFlakSunglasses.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_CustomFlakSunglasses.jpg"), "image/png", GetSeName(productCustomFlakSunglasses.Name) + "product_CustomFlakSunglasses.jpg"),
                DisplayOrder = 1,
            });

            productCustomFlakSunglasses.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "productCustomFlakSunglasses_black_white.jpg"), "image/png", GetSeName(productCustomFlakSunglasses.Name) + "productCustomFlakSunglasses_black_white.jpg"),
                DisplayOrder = 1,
            });

            productCustomFlakSunglasses.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_CustomFlak_matteblack_gray.jpg"), "image/png", GetSeName(productCustomFlakSunglasses.Name) + "product_CustomFlak_matteblack_gray.jpg"),
                DisplayOrder = 1,
            });
            productCustomFlakSunglasses.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_CustomFlak_matteblack_clear.jpg"), "image/png", GetSeName(productCustomFlakSunglasses.Name) + "product_CustomFlak_matteblack_clear.jpg"),
                DisplayOrder = 1,
            });
            productCustomFlakSunglasses.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_CustomFlak_matteblack_jadeiridium.jpg"), "image/png", GetSeName(productCustomFlakSunglasses.Name) + "product_CustomFlak_matteblack_jadeiridium.jpg"),
                DisplayOrder = 1,
            });

            productCustomFlakSunglasses.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_CustomFlak_matteblack_positiverediridium.jpg"), "image/png", GetSeName(productCustomFlakSunglasses.Name) + "product_CustomFlak_matteblack_positiverediridium.jpg"),
                DisplayOrder = 1,
            });
            productCustomFlakSunglasses.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_CustomFlak_matteblack_rubyiridium.jpg"), "image/png", GetSeName(productCustomFlakSunglasses.Name) + "product_CustomFlak_matteblack_rubyiridium.jpg"),
                DisplayOrder = 1,
            });
            productCustomFlakSunglasses.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_CustomFlak_matteblack_sapphireiridium.jpg"), "image/png", GetSeName(productCustomFlakSunglasses.Name) + "product_CustomFlak_matteblack_sapphireiridium.jpg"),
                DisplayOrder = 1,
            });

            productCustomFlakSunglasses.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_CustomFlak_matteblack_violetiridium.jpg"), "image/png", GetSeName(productCustomFlakSunglasses.Name) + "product_CustomFlak_matteblack_violetiridium.jpg"),
                DisplayOrder = 1,
            });
            productCustomFlakSunglasses.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_CustomFlak_matteblack_24kiridium.jpg"), "image/png", GetSeName(productCustomFlakSunglasses.Name) + "product_CustomFlak_matteblack_24kiridium.jpg"),
                DisplayOrder = 1,
            });
            productCustomFlakSunglasses.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_CustomFlak_matteblack_fireiridium.jpg"), "image/png", GetSeName(productCustomFlakSunglasses.Name) + "product_CustomFlak_matteblack_fireiridium.jpg"),
                DisplayOrder = 1,
            });

            productCustomFlakSunglasses.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_CustomFlak_orangeflare_24kiridium.jpg"), "image/png", GetSeName(productCustomFlakSunglasses.Name) + "product_CustomFlak_orangeflare_24kiridium.jpg"),
                DisplayOrder = 1,
            });
            productCustomFlakSunglasses.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_CustomFlak_orangeflare_clear.jpg"), "image/png", GetSeName(productCustomFlakSunglasses.Name) + "product_CustomFlak_orangeflare_clear.jpg"),
                DisplayOrder = 1,
            });
            productCustomFlakSunglasses.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_CustomFlak_orangeflare_fireiridium.jpg"), "image/png", GetSeName(productCustomFlakSunglasses.Name) + "product_CustomFlak_orangeflare_fireiridium.jpg"),
                DisplayOrder = 1,
            });


            productCustomFlakSunglasses.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_CustomFlak_orangeflare_gray.jpg"), "image/png", GetSeName(productCustomFlakSunglasses.Name) + "product_CustomFlak_orangeflare_gray.jpg"),
                DisplayOrder = 1,
            });
            productCustomFlakSunglasses.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_CustomFlak_orangeflare_jadeiridium.jpg"), "image/png", GetSeName(productCustomFlakSunglasses.Name) + "product_CustomFlak_orangeflare_jadeiridium.jpg"),
                DisplayOrder = 1,
            });
            productCustomFlakSunglasses.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_CustomFlak_orangeflare_positiverediridium.jpg"), "image/png", GetSeName(productCustomFlakSunglasses.Name) + "product_CustomFlak_orangeflare_positiverediridium.jpg"),
                DisplayOrder = 1,
            });


            productCustomFlakSunglasses.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_CustomFlak_orangeflare_rubyiridium.jpg"), "image/png", GetSeName(productCustomFlakSunglasses.Name) + "product_CustomFlak_orangeflare_rubyiridium.jpg"),
                DisplayOrder = 1,
            });
            productCustomFlakSunglasses.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_CustomFlak_orangeflare_sapphireiridium.jpg"), "image/png", GetSeName(productCustomFlakSunglasses.Name) + "product_CustomFlak_orangeflare_sapphireiridium.jpg"),
                DisplayOrder = 1,
            });
            productCustomFlakSunglasses.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_CustomFlak_orangeflare_violetiridium.jpg"), "image/png", GetSeName(productCustomFlakSunglasses.Name) + "product_CustomFlak_orangeflare_violetiridium.jpg"),
                DisplayOrder = 1,
            });


            productCustomFlakSunglasses.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_CustomFlak_polishedwhite_24kiridium.jpg"), "image/png", GetSeName(productCustomFlakSunglasses.Name) + "product_CustomFlak_polishedwhite_24kiridium.jpg"),
                DisplayOrder = 1,
            });
            productCustomFlakSunglasses.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_CustomFlak_polishedwhite_clear.jpg"), "image/png", GetSeName(productCustomFlakSunglasses.Name) + "product_CustomFlak_polishedwhite_clear.jpg"),
                DisplayOrder = 1,
            });
            productCustomFlakSunglasses.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_CustomFlak_polishedwhite_fireiridium.jpg"), "image/png", GetSeName(productCustomFlakSunglasses.Name) + "product_CustomFlak_polishedwhite_fireiridium.jpg"),
                DisplayOrder = 1,
            });


            productCustomFlakSunglasses.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_CustomFlak_polishedwhite_gray.jpg"), "image/png", GetSeName(productCustomFlakSunglasses.Name) + "product_CustomFlak_polishedwhite_gray.jpg"),
                DisplayOrder = 1,
            });
            productCustomFlakSunglasses.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_CustomFlak_polishedwhite_jadeiridium.jpg"), "image/png", GetSeName(productCustomFlakSunglasses.Name) + "product_CustomFlak_polishedwhite_jadeiridium.jpg"),
                DisplayOrder = 1,
            });
            productCustomFlakSunglasses.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_CustomFlak_polishedwhite_rubyiridium.jpg"), "image/png", GetSeName(productCustomFlakSunglasses.Name) + "product_CustomFlak_polishedwhite_rubyiridium.jpg"),
                DisplayOrder = 1,
            });

            productCustomFlakSunglasses.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_CustomFlak_polishedwhite_sapphireiridium.jpg"), "image/png", GetSeName(productCustomFlakSunglasses.Name) + "product_CustomFlak_polishedwhite_sapphireiridium.jpg"),
                DisplayOrder = 1,
            });
            productCustomFlakSunglasses.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_CustomFlak_polishedwhite_violetiridium.jpg"), "image/png", GetSeName(productCustomFlakSunglasses.Name) + "product_CustomFlak_polishedwhite_violetiridium.jpg"),
                DisplayOrder = 1,
            });
            productCustomFlakSunglasses.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_CustomFlak_polishedwhite_positiverediridium.jpg"), "image/png", GetSeName(productCustomFlakSunglasses.Name) + "product_CustomFlak_polishedwhite_positiverediridium.jpg"),
                DisplayOrder = 1,
            });

            productCustomFlakSunglasses.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_CustomFlak_redline_24kiridium.jpg"), "image/png", GetSeName(productCustomFlakSunglasses.Name) + "product_CustomFlak_redline_24kiridium.jpg"),
                DisplayOrder = 1,
            });
            productCustomFlakSunglasses.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_CustomFlak_redline_clear.jpg"), "image/png", GetSeName(productCustomFlakSunglasses.Name) + "product_CustomFlak_redline_clear.jpg"),
                DisplayOrder = 1,
            });
            productCustomFlakSunglasses.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_CustomFlak_redline_fireiridium.jpg"), "image/png", GetSeName(productCustomFlakSunglasses.Name) + "product_CustomFlak_redline_fireiridium.jpg"),
                DisplayOrder = 1,
            });
            productCustomFlakSunglasses.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_CustomFlak_redline_gray.jpg"), "image/png", GetSeName(productCustomFlakSunglasses.Name) + "product_CustomFlak_redline_gray.jpg"),
                DisplayOrder = 1,
            });
            productCustomFlakSunglasses.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_CustomFlak_redline_jadeiridium.jpg"), "image/png", GetSeName(productCustomFlakSunglasses.Name) + "product_CustomFlak_redline_jadeiridium.jpg"),
                DisplayOrder = 1,
            });
            productCustomFlakSunglasses.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_CustomFlak_redline_positiverediridium.jpg"), "image/png", GetSeName(productCustomFlakSunglasses.Name) + "product_CustomFlak_redline_positiverediridium.jpg"),
                DisplayOrder = 1,
            });
            productCustomFlakSunglasses.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_CustomFlak_redline_rubyiridium.jpg"), "image/png", GetSeName(productCustomFlakSunglasses.Name) + "product_CustomFlak_redline_rubyiridium.jpg"),
                DisplayOrder = 1,
            });
            productCustomFlakSunglasses.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_CustomFlak_redline_sapphireiridium.jpg"), "image/png", GetSeName(productCustomFlakSunglasses.Name) + "product_CustomFlak_redline_sapphireiridium.jpg"),
                DisplayOrder = 1,
            });
            productCustomFlakSunglasses.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_CustomFlak_redline_violetiridium.jpg"), "image/png", GetSeName(productCustomFlakSunglasses.Name) + "product_CustomFlak_redline_violetiridium.jpg"),
                DisplayOrder = 1,
            });
            productCustomFlakSunglasses.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_CustomFlak_skyblue_24kiridium.jpg"), "image/png", GetSeName(productCustomFlakSunglasses.Name) + "product_CustomFlak_skyblue_24kiridium.jpg"),
                DisplayOrder = 1,
            });
            productCustomFlakSunglasses.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_CustomFlak_skyblue_clear.jpg"), "image/png", GetSeName(productCustomFlakSunglasses.Name) + "product_CustomFlak_skyblue_clear.jpg"),
                DisplayOrder = 1,
            });
            productCustomFlakSunglasses.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_CustomFlak_skyblue_fireiridium.jpg"), "image/png", GetSeName(productCustomFlakSunglasses.Name) + "product_CustomFlak_skyblue_fireiridium.jpg"),
                DisplayOrder = 1,
            });

            productCustomFlakSunglasses.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_CustomFlak_skyblue_gray.jpg"), "image/png", GetSeName(productCustomFlakSunglasses.Name) + "product_CustomFlak_skyblue_gray.jpg"),
                DisplayOrder = 1,
            });
            productCustomFlakSunglasses.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_CustomFlak_skyblue_jadeiridium.jpg"), "image/png", GetSeName(productCustomFlakSunglasses.Name) + "product_CustomFlak_skyblue_jadeiridium.jpg"),
                DisplayOrder = 1,
            });
            productCustomFlakSunglasses.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_CustomFlak_skyblue_positiverediridium.jpg"), "image/png", GetSeName(productCustomFlakSunglasses.Name) + "product_CustomFlak_skyblue_positiverediridium.jpg"),
                DisplayOrder = 1,
            });
            productCustomFlakSunglasses.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_CustomFlak_skyblue_rubyiridium.jpg"), "image/png", GetSeName(productCustomFlakSunglasses.Name) + "product_CustomFlak_skyblue_rubyiridium.jpg"),
                DisplayOrder = 1,
            });
            productCustomFlakSunglasses.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_CustomFlak_skyblue_sapphireiridium.jpg"), "image/png", GetSeName(productCustomFlakSunglasses.Name) + "product_CustomFlak_skyblue_sapphireiridium.jpg"),
                DisplayOrder = 1,
            });
            productCustomFlakSunglasses.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_CustomFlak_skyblue_violetiridium.jpg"), "image/png", GetSeName(productCustomFlakSunglasses.Name) + "product_CustomFlak_skyblue_violetiridium.jpg"),
                DisplayOrder = 1,
            });



            productCustomFlakSunglasses.ProductManufacturers.Add(new ProductManufacturer()
            {
                Manufacturer = _ctx.Set<Manufacturer>().Where(c => c.Name == "Oakley").Single(),
                DisplayOrder = 1,
            });

            #endregion product Custom Flak Sunglasses



            #endregion category sunglasses

            #region category apple

            var categoryApple = this._ctx.Set<Category>().First(c => c.Alias == "Apple");

            #region product iphone plus

            var productIphoneplus = new Product()
            {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "iPhone Plus",
                IsEsd = false,
                ShortDescription = "iPhone 7 dramatically improves the most important aspects of the iPhone experience. It introduces advanced new camera systems. The best performance and battery life ever in an iPhone. Immersive stereo speakers. The brightest, most colorful iPhone display. Splash and water resistance.1 And it looks every bit as powerful as it is. This is iPhone 7.",
                FullDescription = "",
                Sku = "P-2001",
                ProductTemplateId = productTemplate.Id,
                AllowCustomerReviews = true,
                Published = true,
                MetaTitle = "iPhone Plus",
                Price = 878M,
                IsGiftCard = false,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 9,
                StockQuantity = 10000,
                DisplayStockAvailability = true,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                IsShipEnabled = true,
                IsFreeShipping = true,
                DeliveryTime = _ctx.Set<DeliveryTime>().Where(sa => sa.DisplayOrder == 2).Single()
            };
            
            productIphoneplus.ProductCategories.Add(new ProductCategory() { Category = categoryApple, DisplayOrder = 1 });

            productIphoneplus.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_iphone-plus_all_colors.jpg"), "image/png", GetSeName(productIphoneplus.Name)),
                DisplayOrder = 1,
            });

            productIphoneplus.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_iphoneplus_1.jpg"), "image/png", GetSeName(productIphoneplus.Name)),
                DisplayOrder = 2,
            });

            productIphoneplus.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_iphone-plus_red.jpg"), "image/png", GetSeName(productIphoneplus.Name) + "-red"),
                DisplayOrder = 2,
            });
            
            productIphoneplus.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_iphone-plus_silver.jpg"), "image/png", GetSeName(productIphoneplus.Name) + "-silver"),
                DisplayOrder = 2,
            });

            productIphoneplus.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_iphone-plus_black.jpg"), "image/png", GetSeName(productIphoneplus.Name) + "-black"),
                DisplayOrder = 2,
            });

            productIphoneplus.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_iphone-plus_rose.jpg"), "image/png", GetSeName(productIphoneplus.Name) + "-rose"),
                DisplayOrder = 2,
            });

            productIphoneplus.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_iphone-plus_gold.jpg"), "image/png", GetSeName(productIphoneplus.Name) + "-gold"),
                DisplayOrder = 2,
            });

            //attributes
            productIphoneplus.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 1,
                // offer type -> Permanent low price
                SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 22).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 2).Single()
            });

            productIphoneplus.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 1,
                // storage capacity -> 64gb
                SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 27).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 2).Single()
            });
            productIphoneplus.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 1,
                // storage capacity -> 128gb
                SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 27).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 3).Single()
            });
            productIphoneplus.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 1,
                // operating system -> ios
                SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 5).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 9).Single()
            });

            #endregion product iphone plus

            #region product Watch Series 2

            var productWatchSeries2 = new Product()
            {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = false,
                Name = "Watch Series 2",
                IsEsd = false,
                ShortDescription = "Live a better day. Built-in GPS. Water resistance to 50 meters.1 A lightning-fast dual‑core processor. And a display that’s two times brighter than before. Full of features that help you stay active, motivated, and connected, Apple Watch Series 2 is the perfect partner for a healthy life.",
                FullDescription = "",
                Sku = "P-2002",
                ProductTemplateId = productTemplate.Id,
                AllowCustomerReviews = true,
                Published = true,
                MetaTitle = "Watch Series 2",
                Price = 299M,
                OldPrice = 399M,
                IsGiftCard = false,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                IsShipEnabled = true,
                DeliveryTime = _ctx.Set<DeliveryTime>().Where(sa => sa.DisplayOrder == 2).Single()
            };

            productWatchSeries2.ProductCategories.Add(new ProductCategory() { Category = categoryApple, DisplayOrder = 1 });

            productWatchSeries2.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_watchseries2_1.jpg"), "image/png", GetSeName(productWatchSeries2.Name)),
                DisplayOrder = 1,
            });

            productWatchSeries2.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_watchseries2_2.jpg"), "image/png", GetSeName(productWatchSeries2.Name)),
                DisplayOrder = 2,
            });

            productWatchSeries2.ProductManufacturers.Add(new ProductManufacturer()
            {
                Manufacturer = _ctx.Set<Manufacturer>().Where(c => c.Name == "Apple").Single(),
                DisplayOrder = 1,
            });

            //attributes
            productWatchSeries2.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 1,
                // offer type -> offer of the day
                SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 22).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 6).Single()
            });

            productWatchSeries2.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 1,
                // storage capacity -> 32gb
                SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 27).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 1).Single()
            });

            productWatchSeries2.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 1,
                // operating system -> ios
                SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 5).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 9).Single()
            });


            #endregion product Watch Series 2

            #region product Airpods

            var productAirpods = new Product()
            {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "AirPods",
                IsEsd = false,
                ShortDescription = "Wireless. Effortless. Magical. Just take them out and they’re ready to use with all your devices. Put them in your ears and they connect instantly. Speak into them and your voice sounds clear. Introducing AirPods. Simplicity and technology, together like never before. The result is completely magical.",
                FullDescription = "",
                Sku = "P-2003",
                ProductTemplateId = productTemplate.Id,
                AllowCustomerReviews = true,
                Published = true,
                MetaTitle = "AirPods",
                Price = 999M,
                IsGiftCard = false,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                IsShipEnabled = true,
                DeliveryTime = _ctx.Set<DeliveryTime>().Where(sa => sa.DisplayOrder == 2).Single()
            };

            productAirpods.ProductCategories.Add(new ProductCategory() { Category = categoryApple, DisplayOrder = 1 });

            productAirpods.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "products_airpods_white.jpg"), "image/png", GetSeName(productAirpods.Name) + "-white"),
                DisplayOrder = 1,
            });

            productAirpods.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "products_airpods_turquoise.jpg"), "image/png", GetSeName(productAirpods.Name) + "-turquoise"),
                DisplayOrder = 2,
            });

            productAirpods.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "products_airpods_lightblue.jpg"), "image/png", GetSeName(productAirpods.Name) + "-lightblue"),
                DisplayOrder = 3,
            });

            productAirpods.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "products_airpods_rose.jpg"), "image/png", GetSeName(productAirpods.Name) + "-rose"),
                DisplayOrder = 4,
            });

            productAirpods.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "products_airpods_gold.jpg"), "image/png", GetSeName(productAirpods.Name) + "-gold"),
                DisplayOrder = 5,
            });

            productAirpods.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "products_airpods_mint.jpg"), "image/png", GetSeName(productAirpods.Name) + "-mint"),
                DisplayOrder = 6,
            });

            productAirpods.ProductManufacturers.Add(new ProductManufacturer()
            {
                Manufacturer = _ctx.Set<Manufacturer>().Where(c => c.Name == "Apple").Single(),
                DisplayOrder = 7,
            });

            #endregion product Airpods

            #region product Ultimate Apple Pro Hipster Bundle

            var productAppleProHipsterBundle = new Product()
            {
                ProductType = ProductType.BundledProduct,
                VisibleIndividually = true,
                Name = "Ultimate Apple Pro Hipster Bundle",
                IsEsd = false,
                ShortDescription = "Save with this set 5%!",
                FullDescription = "As an Apple fan and hipster, it is your basic need to always have the latest Apple products. So you do not have to spend four times a year in front of the Apple Store, simply subscribe to the Ultimate Apple Pro Hipster Set in the year subscription!",
                Sku = "P-2005-Bundle",
                ProductTemplateId = productTemplate.Id,
                AllowCustomerReviews = true,
                Published = true,
                ShowOnHomePage = true,
                MetaTitle = "Ultimate Apple Pro Hipster Bundle",
                Price = 2371M,
                IsGiftCard = false,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                IsShipEnabled = true,
                DeliveryTime = _ctx.Set<DeliveryTime>().Where(sa => sa.DisplayOrder == 2).Single(),
                BundleTitleText = "Bundle includes",
                BundlePerItemPricing = true,
                BundlePerItemShoppingCart = true
            };
            
            productAppleProHipsterBundle.ProductCategories.Add(new ProductCategory() { Category = categoryApple, DisplayOrder = 1 });

            productAppleProHipsterBundle.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_ultimate-apple-pro-hipster-bundle.jpg"), "image/png", GetSeName(productAppleProHipsterBundle.Name)),
                DisplayOrder = 1,
            });

            productAppleProHipsterBundle.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "products_airpods_white.jpg"), "image/png", GetSeName(productAppleProHipsterBundle.Name)),
                DisplayOrder = 2,
            });

            productAppleProHipsterBundle.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_watchseries2_2.jpg"), "image/png", GetSeName(productAppleProHipsterBundle.Name)),
                DisplayOrder = 2,
            });

            productAppleProHipsterBundle.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_iphoneplus_2.jpg"), "image/png", GetSeName(productAppleProHipsterBundle.Name)),
                DisplayOrder = 2,
            });

            productAppleProHipsterBundle.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "category_apple.png"), "image/png", GetSeName(productAppleProHipsterBundle.Name)),
                DisplayOrder = 2,
            });

            productAppleProHipsterBundle.ProductManufacturers.Add(new ProductManufacturer()
            {
                Manufacturer = _ctx.Set<Manufacturer>().Where(c => c.Name == "Apple").Single(),
                DisplayOrder = 1,
            });

            #endregion product Ultimate Apple Pro Hipster Bundle
            
            #region product 9,7 iPad

            var product97ipad = new Product()
            {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "9,7' iPad",
                IsEsd = false,
                ShortDescription = "Flat-out fun. Learn, play, surf, create. iPad gives you the incredible display, performance, and apps to do what you love to do. Anywhere. Easily. Magically.",
                FullDescription = "<ul>  <li>9,7' Retina Display mit True Tone und</li>  <li>A9X Chip der dritten Generation mit 64-Bit Desktoparchitektur</li>  <li>Touch ID Fingerabdrucksensor</li>  <li>12 Megapixel iSight Kamera mit 4K Video</li>  <li>5 Megapixel FaceTime HD Kamera</li>  <li>802.11ac WLAN mit MIMO</li>  <li>Bis zu 10 Stunden Batterielaufzeit***</li>  <li>4-Lautsprecher-Audio</li></ul>",
                Sku = "P-2004",
                ProductTemplateId = productTemplate.Id,
                AllowCustomerReviews = true,
                Published = true,
                ShowOnHomePage = true,
                MetaTitle = "9,7' iPad",
                Price = 319.00M,
                OldPrice = 349.00M,
                SpecialPrice = 299.00M,
                SpecialPriceStartDateTimeUtc = new DateTime(2017, 5, 1, 0, 0, 0),
                SpecialPriceEndDateTimeUtc = specialPriceEndDate,
                IsGiftCard = false,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                IsShipEnabled = true,
                DeliveryTime = _ctx.Set<DeliveryTime>().Where(sa => sa.DisplayOrder == 2).Single()
            };

            product97ipad.ProductCategories.Add(new ProductCategory() { Category = categoryApple, DisplayOrder = 1 });

            #region pictures
            product97ipad.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_ipad_1.jpg"), "image/png", GetSeName(product97ipad.Name)),
                DisplayOrder = 1,
            });

            product97ipad.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_ipad_2.jpg"), "image/png", GetSeName(product97ipad.Name)),
                DisplayOrder = 2,
            });

            product97ipad.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_97-ipad-yellow.jpg"), "image/png", GetSeName(product97ipad.Name) + "-yellow"),
                DisplayOrder = 2,
            });
            product97ipad.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_97-ipad-turquoise.jpg"), "image/png", GetSeName(product97ipad.Name) + "-turquoise"),
                DisplayOrder = 2,
            });

            product97ipad.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_97-ipad-lightblue.jpg"), "image/png", GetSeName(product97ipad.Name) + "-lightblue"),
                DisplayOrder = 2,
            });

            product97ipad.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_97-ipad-purple.jpg"), "image/png", GetSeName(product97ipad.Name) + "-purple"),
                DisplayOrder = 2,
            });

            product97ipad.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_97-ipad-mint.jpg"), "image/png", GetSeName(product97ipad.Name) + "-mint"),
                DisplayOrder = 2,
            });

            product97ipad.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_97-ipad-rose.jpg"), "image/png", GetSeName(product97ipad.Name) + "-rose"),
                DisplayOrder = 2,
            });

            product97ipad.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_97-ipad-spacegray.jpg"), "image/png", GetSeName(product97ipad.Name) + "-spacegray"),
                DisplayOrder = 2,
            });

            product97ipad.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_97-ipad-gold.jpg"), "image/png", GetSeName(product97ipad.Name) + "-gold"),
                DisplayOrder = 2,
            });

            product97ipad.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_97-ipad-silver.jpg"), "image/png", GetSeName(product97ipad.Name) + "-silver"),
                DisplayOrder = 2,
            });
            #endregion pictures

            product97ipad.ProductManufacturers.Add(new ProductManufacturer()
            {
                Manufacturer = _ctx.Set<Manufacturer>().Where(c => c.Name == "Apple").Single(),
                DisplayOrder = 1,
            });

            #region attributes
            //attributes
            product97ipad.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 1,
                // offer type -> promotion
                SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 22).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 3).Single()
            });
            
            product97ipad.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 1,
                // storage capacity -> 64gb
                SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 27).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 2).Single()
            });
            product97ipad.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 1,
                // storage capacity -> 128gb
                SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 27).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 3).Single()
            });
            product97ipad.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 1,
                // operating system -> ios
                SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 5).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 9).Single()
            });
            #endregion attributes

            #endregion product 9,7 iPad


            #endregion category apple

            #region category Gift Cards

            var categoryGiftCards = this._ctx.Set<Category>().First(c => c.Alias == "Gift Cards");

			#region product10GiftCard

			var product10GiftCard = new Product()
			{
				ProductType = ProductType.SimpleProduct,
				VisibleIndividually = true,
				Name = "$10 Virtual Gift Card",
				IsEsd = true,
				ShortDescription = "$10 Gift Card. Gift Cards must be redeemed through our site Web site toward the purchase of eligible products.",
				FullDescription = "<p>Gift Cards must be redeemed through our site Web site toward the purchase of eligible products. Purchases are deducted from the GiftCard balance. Any unused balance will be placed in the recipient's GiftCard account when redeemed. If an order exceeds the amount of the GiftCard, the balance must be paid with a credit card or other available payment method.</p>",
                Sku = "P-1000",
				ProductTemplateId = productTemplate.Id,
				AllowCustomerReviews = true,
				Published = true,
				MetaTitle = "$10 Virtual Gift Card",
				Price = 10M,
				IsGiftCard = true,
				ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
				OrderMinimumQuantity = 1,
				OrderMaximumQuantity = 10000,
				StockQuantity = 10000,
				NotifyAdminForQuantityBelow = 1,
				AllowBackInStockSubscriptions = false,
                DisplayOrder = 1
                
			};

            product10GiftCard.ProductCategories.Add(new ProductCategory() { Category = categoryGiftCards, DisplayOrder = 1 });

            //var productTag = _productTagRepository.Table.Where(pt => pt.Name == "gift").FirstOrDefault();
            //productTag.ProductCount++;
            //productTag.Products.Add(product5GiftCard);
            //_productTagRepository.Update(productTag);

            product10GiftCard.ProductPictures.Add(new ProductPicture()
			{
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_gift_card_10.png"), "image/png", GetSeName(product10GiftCard.Name)),
				//DisplayOrder = 1,
			});

            #endregion product10GiftCard

            #region product25GiftCard

            var product25GiftCard = new Product()
			{
				ProductType = ProductType.SimpleProduct,
				VisibleIndividually = true,
				Name = "$25 Virtual Gift Card",
				IsEsd = true,
				ShortDescription = "$25 Gift Card. Gift Cards must be redeemed through our site Web site toward the purchase of eligible products.",
				FullDescription = "<p>Gift Cards must be redeemed through our site Web site toward the purchase of eligible products. Purchases are deducted from the GiftCard balance. Any unused balance will be placed in the recipient's GiftCard account when redeemed. If an order exceeds the amount of the GiftCard, the balance must be paid with a credit card or other available payment method.</p>",
                Sku = "P-1001",
				ProductTemplateId = productTemplate.Id,
				AllowCustomerReviews = true,
				Published = true,
				MetaTitle = "$25 Virtual Gift Card",
				Price = 25M,
				IsGiftCard = true,
				GiftCardType = GiftCardType.Virtual,
				ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
				OrderMinimumQuantity = 1,
				OrderMaximumQuantity = 10000,
				StockQuantity = 10000,
				NotifyAdminForQuantityBelow = 1,
				AllowBackInStockSubscriptions = false,
                DisplayOrder = 2
            };

            product25GiftCard.ProductCategories.Add(new ProductCategory() { Category = categoryGiftCards, DisplayOrder = 1 });

			product25GiftCard.ProductPictures.Add(new ProductPicture()
			{
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_gift_card_25.png"), "image/png", GetSeName(product25GiftCard.Name)),
				//DisplayOrder = 2,
			});

			#endregion product25GiftCard

			#region product50GiftCard

			var product50GiftCard = new Product()
			{
				ProductType = ProductType.SimpleProduct,
				VisibleIndividually = true,
				Name = "$50 Virtual Gift Card",
				IsEsd = true,
				ShortDescription = "$50 Gift Card. Gift Cards must be redeemed through our site Web site toward the purchase of eligible products.",
				FullDescription = "<p>Gift Cards must be redeemed through our site Web site toward the purchase of eligible products. Purchases are deducted from the GiftCard balance. Any unused balance will be placed in the recipient's GiftCard account when redeemed. If an order exceeds the amount of the GiftCard, the balance must be paid with a credit card or other available payment method.</p>",
                Sku = "P-1002",
				ProductTemplateId = productTemplate.Id,
				AllowCustomerReviews = true,
				Published = true,
				MetaTitle = "$50 Virtual Gift Card",
				Price = 50M,
				IsGiftCard = true,
				GiftCardType = GiftCardType.Virtual,
				ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
				OrderMinimumQuantity = 1,
				OrderMaximumQuantity = 10000,
				StockQuantity = 10000,
				NotifyAdminForQuantityBelow = 1,
				AllowBackInStockSubscriptions = false,
                DisplayOrder = 3
            };

            product50GiftCard.ProductCategories.Add(new ProductCategory() { Category = categoryGiftCards, DisplayOrder = 1 });

			product50GiftCard.ProductPictures.Add(new ProductPicture()
			{
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_gift_card_50.png"), "image/png", GetSeName(product50GiftCard.Name)),
				//DisplayOrder = 3,
			});

            #endregion product50GiftCard

            #region product100GiftCard

            var product100GiftCard = new Product()
            {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "$100 Virtual Gift Card",
                IsEsd = true,
                ShortDescription = "$100 Gift Card. Gift Cards must be redeemed through our site Web site toward the purchase of eligible products.",
                FullDescription = "<p>Gift Cards must be redeemed through our site Web site toward the purchase of eligible products. Purchases are deducted from the GiftCard balance. Any unused balance will be placed in the recipient's GiftCard account when redeemed. If an order exceeds the amount of the GiftCard, the balance must be paid with a credit card or other available payment method.</p>",
                Sku = "P-10033",
                ProductTemplateId = productTemplate.Id,
                AllowCustomerReviews = true,
                Published = true,
                MetaTitle = "$100 Virtual Gift Card",
                Price = 100M,
                IsGiftCard = true,
                GiftCardType = GiftCardType.Virtual,
                ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                DisplayOrder = 4,
            };

            product100GiftCard.ProductCategories.Add(new ProductCategory() { Category = categoryGiftCards, DisplayOrder = 1 });

            product100GiftCard.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_gift_card_100.png"), "image/png", GetSeName(product100GiftCard.Name)),
                //DisplayOrder = 4,
            });

            #endregion product100GiftCard

            #endregion category Gift Cards

            #region category books

            var categorySpiegelBestseller = this._ctx.Set<Category>().First(c => c.Alias == "SPIEGEL-Bestseller");
            var categoryCookAndEnjoy = this._ctx.Set<Category>().First(c => c.Alias == "Cook and enjoy");
            var categoryBooks = this._ctx.Set<Category>().First(c => c.Alias == "Books");

			#region productBooksUberMan

			var productBooksUberMan = new Product()
			{
				ProductType = ProductType.SimpleProduct,
				VisibleIndividually = true,
				Name = "Überman: The novel",
				ShortDescription = "(Hardcover)",
				FullDescription = "<p>From idiots to riches - and back ... Ever since it with my Greek financial advisors were no more delicious cookies to meetings, I should have known something. Was the last cookie it when I bought a Romanian forest funds and leveraged discount certificates on lean hogs - which is sort of a more stringent bet that the price of lean hogs will remain stable, and that's nothing special because it is also available for cattle and cotton and fat pig. Again and again and I joked Kosmas Nikiforos Sarantakos. About all the part-time seer who tremblingly put for fear the euro crisis gold coins under the salami slices of their frozen pizzas And then came the day that revealed to me in almost Sarantakos fraudulent casualness that my plan had not worked out really. 'Why all of a sudden> my plan', 'I heard myself asking yet, but it was in the garage I realized what that really meant minus 211.2 percent in my portfolio report: personal bankruptcy, gutter and Drug Addiction with subsequent loss of the incisors . Not even the study of my friend, I would still be able to finance. The only way out was to me as quickly as secretly again to draw from this unspeakable Greek shit - I had to be Überman! By far the bekloppteste story about 'idiot' Simon Peter! »Tommy Jaud – Deutschlands witzigste Seite.« Alex Dengler, Bild am Sonntag</p>",
                Sku = "P-1003",
				ProductTemplateId = productTemplate.Id,
				AllowCustomerReviews = true,
				Published = true,
				MetaTitle = "Überman: The novel",
				Price = 16.99M,
				ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
				OrderMinimumQuantity = 1,
				OrderMaximumQuantity = 10000,
				StockQuantity = 10000,
				NotifyAdminForQuantityBelow = 1,
				AllowBackInStockSubscriptions = false,
				IsShipEnabled = true
			};

            productBooksUberMan.ProductCategories.Add(new ProductCategory() { Category = categorySpiegelBestseller, DisplayOrder = 1 });

			//pictures
			productBooksUberMan.ProductPictures.Add(new ProductPicture()
			{
				Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "0000932_uberman-der-roman.jpeg"), "image/jpeg", GetSeName(productBooksUberMan.Name)),
				DisplayOrder = 1,
			});

			//attributes
			productBooksUberMan.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
			{
				AllowFiltering = true,
				ShowOnProductPage = true,
				DisplayOrder = 3,
				SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 13).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 1).Single()
			});
			productBooksUberMan.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
			{
				AllowFiltering = true,
				ShowOnProductPage = true,
				DisplayOrder = 3,
				SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 14).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 7).Single()
			});
			productBooksUberMan.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
			{
				AllowFiltering = true,
				ShowOnProductPage = true,
				DisplayOrder = 3,
				SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 12).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 1).Single()
			});
			#endregion productBooksUberMan

			#region productBooksGefangeneDesHimmels

			var productBooksGefangeneDesHimmels = new Product()
			{
				ProductType = ProductType.SimpleProduct,
				VisibleIndividually = true,
				Name = "The Prisoner of Heaven: A Novel",
				ShortDescription = "(Hardcover)",
				FullDescription = "<p>By Shadow of the Wind and The Angel's Game, the new large-Barcelona novel by Carlos Ruiz Zafón. - Barcelona, Christmas 1957th The bookseller Daniel Sempere and his friend Fermín be drawn again into a great adventure. In the continuation of his international success with Carlos Ruiz Zafón takes the reader on a fascinating journey into his Barcelona. Creepy and fascinating, with incredible suction power and humor, the novel, the story of Fermin, who 'rose from the dead, and the key to the future is.' Fermin's life story linking the threads of The Shadow of the Wind with those from The Angel's Game. A masterful puzzle that keeps the reader around the world in thrall. </p> <p> Product Hardcover: 416 pages Publisher: S. Fischer Verlag; 1 edition (October 25, 2012) Language: German ISBN-10: 3,100,954,025 ISBN-13: 978-3100954022 Original title: El prisionero del cielo Size and / or weight: 21.4 x 13.6 cm x 4.4 </p>",
				ProductTemplateId = productTemplate.Id,
                Sku = "P-1004",
				AllowCustomerReviews = true,
				Published = true,
				MetaTitle = "The Prisoner of Heaven: A Novel",
				Price = 22.99M,
				ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
				OrderMinimumQuantity = 1,
				OrderMaximumQuantity = 10000,
				StockQuantity = 10000,
				NotifyAdminForQuantityBelow = 1,
				AllowBackInStockSubscriptions = false,
				IsShipEnabled = true,
				DeliveryTime = _ctx.Set<DeliveryTime>().Where(sa => sa.DisplayOrder == 0).Single()
			};

            productBooksGefangeneDesHimmels.ProductCategories.Add(new ProductCategory() { Category = categorySpiegelBestseller, DisplayOrder = 1 });

			//pictures
			productBooksGefangeneDesHimmels.ProductPictures.Add(new ProductPicture()
			{
				Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "0000935_der-gefangene-des-himmels-roman_300.jpeg"), "image/jpeg", GetSeName(productBooksGefangeneDesHimmels.Name)),
				DisplayOrder = 1,
			});

			//attributes
			productBooksGefangeneDesHimmels.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
			{
				AllowFiltering = true,
				ShowOnProductPage = true,
				DisplayOrder = 3,
				// Edition -> bound
				SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 13).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 1).Single()
			});

			productBooksGefangeneDesHimmels.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
			{
				AllowFiltering = true,
				ShowOnProductPage = true,
				DisplayOrder = 3,
				// Category -> bound
				SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 14).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 7).Single()
			});
			productBooksGefangeneDesHimmels.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
			{
				AllowFiltering = true,
				ShowOnProductPage = true,
				DisplayOrder = 3,
				// Language -> German
				SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 12).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 1).Single()
			});

			#endregion productBooksGefangeneDesHimmels

			#region productBooksBestGrillingRecipes

			var productBooksBestGrillingRecipes = new Product()
			{
				ProductType = ProductType.SimpleProduct,
				VisibleIndividually = true,
				Name = "Best Grilling Recipes",
				ShortDescription = "More Than 100 Regional Favorites Tested and Perfected for the Outdoor Cook (Hardcover)",
				FullDescription = "<p> Take a winding cross-country trip and you'll discover barbecue shacks with offerings like tender-smoky Baltimore pit beef and saucy St. Louis pork steaks. To bring you the best of these hidden gems, along with all the classics, the editors of Cook's Country magazine scoured the country, then tested and perfected their favorites. HEre traditions large and small are brought into the backyard, from Hawaii's rotisserie favorite, the golden-hued Huli Huli Chicken, to fall-off-the-bone Chicago Barbecued Ribs. In Kansas City, they're all about the sauce, and for our saucy Kansas City Sticky Ribs, we found a surprise ingredient-root beer. We also tackle all the best sides. </p> <p> Not sure where or how to start? This cookbook kicks off with an easy-to-follow primer that will get newcomers all fired up. Whether you want to entertain a crowd or just want to learn to make perfect burgers, Best Grilling Recipes shows you the way. </p>",
				ProductTemplateId = productTemplate.Id,
                Sku = "P-1005",
				AllowCustomerReviews = true,
				Published = true,
				MetaTitle = "Best Grilling Recipes",
				Price = 27.00M,
				ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
				OrderMinimumQuantity = 1,
				OrderMaximumQuantity = 10000,
				StockQuantity = 10000,
				NotifyAdminForQuantityBelow = 1,
				AllowBackInStockSubscriptions = false,
				IsShipEnabled = true,
				DeliveryTime = _ctx.Set<DeliveryTime>().Where(sa => sa.DisplayOrder == 0).Single()
			};

            productBooksBestGrillingRecipes.ProductCategories.Add(new ProductCategory() { Category = categoryCookAndEnjoy, DisplayOrder = 1 });
            
			//pictures
			productBooksBestGrillingRecipes.ProductPictures.Add(new ProductPicture()
			{
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_bestgrillingrecipes.jpg"), "image/jpeg", GetSeName(productBooksBestGrillingRecipes.Name)),
				DisplayOrder = 1,
			});

			//attributes
			productBooksBestGrillingRecipes.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
			{
				AllowFiltering = true,
				ShowOnProductPage = true,
				DisplayOrder = 3,
				// Edition -> bound
				SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 13).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 1).Single()
			});
			productBooksBestGrillingRecipes.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
			{
				AllowFiltering = true,
				ShowOnProductPage = true,
				DisplayOrder = 3,
				// Category -> cook & bake
				SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 14).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 8).Single()
			});
			productBooksBestGrillingRecipes.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
			{
				AllowFiltering = true,
				ShowOnProductPage = true,
				DisplayOrder = 3,
				// Language -> German
				SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 12).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 2).Single()
			});

			#endregion productBooksBestGrillingRecipes

			#region productBooksCookingForTwo

			var productBooksCookingForTwo = new Product()
			{
				ProductType = ProductType.SimpleProduct,
				VisibleIndividually = true,
				Name = "Cooking for Two",
				ShortDescription = "More Than 200 Foolproof Recipes for Weeknights and Special Occasions (Hardcover)",
				FullDescription = "<p>In Cooking for Two, the test kitchen's goal was to take traditional recipes and cut them down to size to serve just twowith tailored cooking techniques and smart shopping tips that will cut down on wasted food and wasted money. Great lasagna starts to lose its luster when you're eating the leftovers for the fourth day in a row. While it may seem obvious that a recipe for four can simply be halved to work, our testing has proved that this is not always the case; cooking with smaller amounts of ingredients often requires different preparation techniques, cooking time, temperature, and the proportion of ingredients. This was especially true as we worked on scaled-down desserts; baking is an unforgiving science in which any changes in recipe amounts often called for changes in baking times and temperatures. </p> <p> Hardcover: 352 pages<br> Publisher: America's Test Kitchen (May 2009)<br> Language: English<br> ISBN-10: 1933615435<br> ISBN-13: 978-1933615431<br> </p>",
				ProductTemplateId = productTemplate.Id,
                Sku = "P-1006",
				AllowCustomerReviews = true,
				Published = true,
				MetaTitle = "Cooking for Two",
				Price = 27.00M,
				ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
				OrderMinimumQuantity = 1,
				OrderMaximumQuantity = 10000,
				StockQuantity = 10000,
				NotifyAdminForQuantityBelow = 1,
				AllowBackInStockSubscriptions = false,
				IsShipEnabled = true,
				DeliveryTime = _ctx.Set<DeliveryTime>().Where(sa => sa.DisplayOrder == 1).Single()
			};

            productBooksCookingForTwo.ProductCategories.Add(new ProductCategory() { Category = categoryCookAndEnjoy, DisplayOrder = 1 });

			//pictures
			productBooksCookingForTwo.ProductPictures.Add(new ProductPicture()
			{
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_cookingfortwo.jpg"), "image/jpeg", GetSeName(productBooksCookingForTwo.Name)),
				DisplayOrder = 1,
			});

			//attributes
			productBooksCookingForTwo.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
			{
				AllowFiltering = true,
				ShowOnProductPage = true,
				DisplayOrder = 3,
				// Edition -> bound
				SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 13).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 1).Single()
			});
			productBooksCookingForTwo.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
			{
				AllowFiltering = true,
				ShowOnProductPage = true,
				DisplayOrder = 3,
				// Category -> cook & bake
				SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 14).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 8).Single()
			});
			productBooksCookingForTwo.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
			{
				AllowFiltering = true,
				ShowOnProductPage = true,
				DisplayOrder = 3,
				// Language -> German
				SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 12).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 2).Single()
			});

			#endregion productBooksCookingForTwo

			#region productBooksAutosDerSuperlative

			var productBooksAutosDerSuperlative = new Product()
			{
				ProductType = ProductType.SimpleProduct,
				VisibleIndividually = true,
				Name = "Car of superlatives: the strongest, the first, the most beautiful, the fastest",
				ShortDescription = "Hardcover",
				FullDescription = "<p> For some, the car is only a useful means of transportation. For everyone else, there are 'cars - The Ultimate Guide' of art-connoisseur Michael Doerflinger. With authentic images, all important data and a lot of information can be presented to the fastest, most innovative, the strongest, the most unusual and the most successful examples of automotive history. A comprehensive manual for the specific reference and extensive browsing. </p>",
                Sku = "P-1007",
				ProductTemplateId = productTemplate.Id,
				AllowCustomerReviews = true,
				Published = true,
				MetaTitle = "Car of superlatives",
				Price = 14.95M,
				ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
				OrderMinimumQuantity = 1,
				OrderMaximumQuantity = 10000,
				StockQuantity = 10000,
				NotifyAdminForQuantityBelow = 1,
				AllowBackInStockSubscriptions = false,
				IsShipEnabled = true,
				DeliveryTime = _ctx.Set<DeliveryTime>().Where(sa => sa.DisplayOrder == 2).Single()
			};

            productBooksAutosDerSuperlative.ProductCategories.Add(new ProductCategory() { Category = categoryBooks, DisplayOrder = 1 });
            
			//pictures
			productBooksAutosDerSuperlative.ProductPictures.Add(new ProductPicture()
			{
				Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "0000944_autos-der-superlative-die-starksten-die-ersten-die-schonsten-die-schnellsten.jpeg"), "image/jpeg", GetSeName(productBooksAutosDerSuperlative.Name)),
				DisplayOrder = 1,
			});

			//attributes
			productBooksAutosDerSuperlative.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
			{
				AllowFiltering = true,
				ShowOnProductPage = true,
				DisplayOrder = 3,
				// Edition -> bound
				SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 13).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 1).Single()
			});
			productBooksAutosDerSuperlative.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
			{
				AllowFiltering = true,
				ShowOnProductPage = true,
				DisplayOrder = 3,
				// Category -> cars
				SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 14).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 6).Single()
			});
			productBooksAutosDerSuperlative.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
			{
				AllowFiltering = true,
				ShowOnProductPage = true,
				DisplayOrder = 3,
				// Language -> German
				SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 12).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 1).Single()
			});


			#endregion productBooksAutosDerSuperlative

			#region productBooksBildatlasMotorraeder

			var productBooksBildatlasMotorraeder = new Product()
			{
				ProductType = ProductType.SimpleProduct,
				VisibleIndividually = true,
				Name = "Picture Atlas Motorcycles: With more than 350 brilliant images",
				ShortDescription = "Hardcover",
				FullDescription = "<p> Motorcycles are like no other means of transportation for the great dream of freedom and adventure. This richly illustrated atlas image portrayed in brilliant color photographs and informative text, the most famous bikes of the world's motorcycle history. From the primitive steam engine under the saddle of the late 19th Century up to the hugely powerful, equipped with the latest electronics and computer technology superbikes of today he is an impressive picture of the development and fabrication of noble and fast-paced motorcycles. The myth of the motorcycle is just as much investigated as a motorcycle as a modern lifestyle product of our time. Country-specific, company-historical background information and interesting stories and History about the people who preceded drove one of the seminal inventions of recent centuries and evolved, make this comprehensive illustrated book an incomparable reference for any motorcycle enthusiast and technology enthusiasts. </p> <p> • Extensive history of the legendary models of all major motorcycle manufacturers worldwide<br> • With more than 350 brilliant color photographs and fascinating background information relating<br> • With informative drawings, stunning detail shots and explanatory info-boxes<br> </p> <p> content • 1817 1913: The beginning of a success story<br> • 1914 1945: mass mobility<br> • 1946 1990: Battle for the World Market<br> • In 1991: The modern motorcycle<br> • motorcycle cult object: From Transportation to Lifestyle<br> </p>",
                Sku = "P-1008",
				ProductTemplateId = productTemplate.Id,
				AllowCustomerReviews = true,
				Published = true,
				MetaTitle = "Picture Atlas Motorcycles",
				Price = 14.99M,
				ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
				OrderMinimumQuantity = 1,
				OrderMaximumQuantity = 10000,
				StockQuantity = 10000,
				NotifyAdminForQuantityBelow = 1,
				AllowBackInStockSubscriptions = false,
				IsShipEnabled = true,
				DeliveryTime = _ctx.Set<DeliveryTime>().Where(sa => sa.DisplayOrder == 0).Single()
			};

            productBooksBildatlasMotorraeder.ProductCategories.Add(new ProductCategory() { Category = categoryBooks, DisplayOrder = 1 });

			//pictures
			productBooksBildatlasMotorraeder.ProductPictures.Add(new ProductPicture()
			{
				Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "0000942_bildatlas-motorrader-mit-mehr-als-350-brillanten-abbildungen.jpeg"), "image/jpeg", GetSeName(productBooksBildatlasMotorraeder.Name)),
				DisplayOrder = 1,
			});

			//attributes
			productBooksBildatlasMotorraeder.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
			{
				AllowFiltering = true,
				ShowOnProductPage = true,
				DisplayOrder = 3,
				// Edition -> bound
				SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 13).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 1).Single()
			});
			productBooksBildatlasMotorraeder.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
			{
				AllowFiltering = true,
				ShowOnProductPage = true,
				DisplayOrder = 3,
				// Category -> non-fiction
				SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 14).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 9).Single()
			});
			productBooksBildatlasMotorraeder.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
			{
				AllowFiltering = true,
				ShowOnProductPage = true,
				DisplayOrder = 3,
				// Language -> German
				SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 12).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 1).Single()
			});

			#endregion productBooksBildatlasMotorraeder

			#region productBooksAutoBuch

			var productBooksAutoBuch = new Product()
			{
				ProductType = ProductType.SimpleProduct,
				VisibleIndividually = true,
				Name = "The Car Book. The great history with over 1200 models",
				ShortDescription = "Hardcover",
				FullDescription = "<p> Makes, models, milestones<br> The car - for some, a utensil for other expression of lifestyle, cult object and passion. Few inventions have changed their lives as well as the good of the automobile 125 years ago - one more reason for this extensive chronicle. The car-book brings the history of the automobile to life. It presents more than 1200 important models - Karl Benz 'Motorwagen about legendary cult car to advanced hybrid vehicles. It explains the milestones in engine technology and portrays the big brands and their designers. Characteristics from small cars to limousines and send racing each era invite you to browse and discover. The most comprehensive and bestbebildert illustrated book on the market - it would be any car lover! </p> <p> Hardcover: 360 pages<br> Publisher: Dorling Kindersley Publishing (September 27, 2012)<br> Language: German<br> ISBN-10: 3,831,022,062<br> ISBN-13: 978-3831022069<br> Size and / or weight: 30.6 x 25.8 x 2.8 cm<br> </p>",
                Sku = "P-1009",
				ProductTemplateId = productTemplate.Id,
				AllowCustomerReviews = true,
				Published = true,
				MetaTitle = "The Car Book",
				Price = 29.95M,
				ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
				OrderMinimumQuantity = 1,
				OrderMaximumQuantity = 10000,
				StockQuantity = 10000,
				NotifyAdminForQuantityBelow = 1,
				AllowBackInStockSubscriptions = false,
				IsShipEnabled = true,
				DeliveryTime = _ctx.Set<DeliveryTime>().Where(sa => sa.DisplayOrder == 0).Single()
			};

            productBooksAutoBuch.ProductCategories.Add(new ProductCategory() { Category = categoryBooks, DisplayOrder = 1 });

			//pictures
			productBooksAutoBuch.ProductPictures.Add(new ProductPicture()
			{
				Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "0000947_das-auto-buch-die-grose-chronik-mit-uber-1200-modellen_300.jpeg"), "image/jpeg", GetSeName(productBooksAutoBuch.Name)),
				DisplayOrder = 1,
			});

			//attributes
			productBooksAutoBuch.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
			{
				AllowFiltering = true,
				ShowOnProductPage = true,
				DisplayOrder = 3,
				// Edition -> bound
				SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 13).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 1).Single()
			});
			productBooksAutoBuch.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
			{
				AllowFiltering = true,
				ShowOnProductPage = true,
				DisplayOrder = 3,
				// Category -> non-fiction
				SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 14).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 9).Single()
			});
			productBooksAutoBuch.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
			{
				AllowFiltering = true,
				ShowOnProductPage = true,
				DisplayOrder = 3,
				// Language -> German
				SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 12).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 1).Single()
			});

			#endregion productBooksAutoBuch

			#region productBooksFastCars

			var productBooksFastCars = new Product()
			{
				ProductType = ProductType.SimpleProduct,
				VisibleIndividually = true,
				Name = "Fast Cars, Image Calendar 2013",
				ShortDescription = "spiral bound",
				FullDescription = "<p> Large Size: 48.5 x 34 cm.<br> This impressive picture calendar with silver ring binding thrilled with impressive photographs of exclusive sports cars. Who understands cars not only as a pure commercial vehicles, will find the most sought-after status symbols at all: fast cars are effectively set to the razor sharp and vivid photos in scene and convey freedom, speed, strength and the highest technical perfection. Starting with the 450-horsepower Maserati GranTurismo MC Stradale on the stylish, luxurious Aston Martin Virage Volante accompany up to the produced only in small numbers Mosler Photon MT900S the fast racer with style and elegance through the months. </p> <p> Besides the calendar draws another picture to look at interesting details. There are the essential information on any sports car in the English language. After this year, the high-quality photos are framed an eye-catcher on the wall of every lover of fast cars. Even as a gift this beautiful years companion is wonderfully suited. 12 calendar pages, neutral and discreet held calendar. Printed on paper from sustainable forests. For lovers of luxury vintage cars also available in ALPHA EDITION: the large format image Classic Cars Calendar 2013: ISBN 9,783,840,733,376th </p> <p> Spiral-bound: 14 pages<br> Publisher: Alpha Edition (June 1, 2012)<br> Language: German<br> ISBN-10: 3,840,733,383<br> ISBN-13: 978-3840733383<br> Size and / or weight: 48.8 x 34.2 x 0.6 cm<br> </p>",
                Sku = "P-1010",
				ProductTemplateId = productTemplate.Id,
				AllowCustomerReviews = true,
				Published = true,
				MetaTitle = "Fast Cars",
				Price = 16.95M,
				ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
				OrderMinimumQuantity = 1,
				OrderMaximumQuantity = 10000,
				StockQuantity = 10000,
				NotifyAdminForQuantityBelow = 1,
				AllowBackInStockSubscriptions = false,
				IsShipEnabled = true,
				DeliveryTime = _ctx.Set<DeliveryTime>().Where(sa => sa.DisplayOrder == 0).Single()
			};

            productBooksFastCars.ProductCategories.Add(new ProductCategory() { Category = categoryBooks, DisplayOrder = 1 });

			//pictures
			productBooksFastCars.ProductPictures.Add(new ProductPicture()
			{
				Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "0000946_fast-cars-bildkalender-2013_300.jpeg"), "image/jpeg", GetSeName(productBooksFastCars.Name)),
				DisplayOrder = 1,
			});

			//attributes
			productBooksFastCars.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
			{
				AllowFiltering = true,
				ShowOnProductPage = true,
				DisplayOrder = 3,
				// Edition -> bound
				SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 13).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 1).Single()
			});
			productBooksFastCars.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
			{
				AllowFiltering = true,
				ShowOnProductPage = true,
				DisplayOrder = 3,
				// Category -> cars
				SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 14).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 6).Single()
			});
			productBooksFastCars.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
			{
				AllowFiltering = true,
				ShowOnProductPage = true,
				DisplayOrder = 3,
				// Language -> German
				SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 12).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 1).Single()
			});

			#endregion productBooksFastCars

			#region productBooksMotorradAbenteuer

			var productBooksMotorradAbenteuer = new Product()
			{
				ProductType = ProductType.SimpleProduct,
				VisibleIndividually = true,
				Name = "Motorcycle Adventures: Riding for travel enduros",
				ShortDescription = "Hardcover",
				FullDescription = "<p> Modern travel enduro bikes are ideal for adventure travel. Their technique is complex, their weight considerably. The driving behavior changes depending on the load and distance. </p> <p> Before the tour starts, you should definitely attend a training course. This superbly illustrated book presents practical means of many informative series photos the right off-road driving in mud and sand, gravel and rock with and without luggage. In addition to the driving course full of information and tips on choosing the right motorcycle for travel planning and practical issues may be on the way. </p>",
                Sku = "P-1011",
				ProductTemplateId = productTemplate.Id,
				AllowCustomerReviews = true,
				Published = true,
				MetaTitle = "Motorcycle Adventures",
				Price = 24.90M,
				ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
				OrderMinimumQuantity = 1,
				OrderMaximumQuantity = 10000,
				StockQuantity = 10000,
				NotifyAdminForQuantityBelow = 1,
				AllowBackInStockSubscriptions = false,
				IsShipEnabled = true,
				DeliveryTime = _ctx.Set<DeliveryTime>().Where(sa => sa.DisplayOrder == 1).Single()
			};

            productBooksMotorradAbenteuer.ProductCategories.Add(new ProductCategory() { Category = categoryBooks, DisplayOrder = 1 });

			//pictures
			productBooksMotorradAbenteuer.ProductPictures.Add(new ProductPicture()
			{
				Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "0000943_motorrad-abenteuer-fahrtechnik-fur-reise-enduros.jpeg"), "image/jpeg", GetSeName(productBooksMotorradAbenteuer.Name)),
				DisplayOrder = 1,
			});

			//attributes
			productBooksMotorradAbenteuer.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
			{
				AllowFiltering = true,
				ShowOnProductPage = true,
				DisplayOrder = 3,
				// Edition -> bound
				SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 13).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 1).Single()
			});
			productBooksMotorradAbenteuer.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
			{
				AllowFiltering = true,
				ShowOnProductPage = true,
				DisplayOrder = 3,
				// Category -> cars
				SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 14).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 9).Single()
			});
			productBooksMotorradAbenteuer.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
			{
				AllowFiltering = true,
				ShowOnProductPage = true,
				DisplayOrder = 3,
				// Language -> German
				SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 12).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 1).Single()
			});

			#endregion productBooksMotorradAbenteuer

			#endregion category books

			#region computer

   //         var categoryComputer = this._ctx.Set<Category>().First(c => c.Alias == "Computers");
   //         var categoryNotebooks = this._ctx.Set<Category>().First(c => c.Alias == "Notebooks");
   //         var categoryDesktops = this._ctx.Set<Category>().First(c => c.Alias == "Desktops");

			//#region productComputerDellInspiron23

			//var productComputerDellInspiron23 = new Product()
			//{
			//	ProductType = ProductType.SimpleProduct,
			//	VisibleIndividually = true,
			//	Name = "Dell Inspiron One 23",
			//	ShortDescription = "This 58 cm (23'')-All-in-One PC with Full HD, Windows 8 and powerful Intel ® Core ™ processor third generation allows practical interaction with a touch screen.",
			//	FullDescription = "<p>Ultra high performance all-in-one i7 PC with Windows 8, Intel ® Core ™ processor, huge 2TB hard drive and Blu-Ray drive. </p> <p> Intel® Core™ i7-3770S Processor ( 3,1 GHz, 6 MB Cache)<br> Windows 8 64bit , english<br> 8 GB1 DDR3 SDRAM at 1600 MHz<br> 2 TB-Serial ATA-Harddisk (7.200 rot/min)<br> 1GB AMD Radeon HD 7650<br> </p>",
   //             Sku = "P-1012",
			//	ProductTemplateId = productTemplateSimple.Id,
			//	AllowCustomerReviews = true,
			//	Published = true,
			//	MetaTitle = "Dell Inspiron One 23",
			//	Price = 589.00M,
			//	ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
			//	OrderMinimumQuantity = 1,
			//	OrderMaximumQuantity = 10000,
			//	StockQuantity = 10000,
			//	NotifyAdminForQuantityBelow = 1,
			//	AllowBackInStockSubscriptions = false,
			//	IsShipEnabled = true,
			//	DeliveryTime = _ctx.Set<DeliveryTime>().Where(sa => sa.DisplayOrder == 0).Single()
			//};

   //         productComputerDellInspiron23.ProductCategories.Add(new ProductCategory() { Category = categoryComputer, DisplayOrder = 1 });
   //         productComputerDellInspiron23.ProductCategories.Add(new ProductCategory() { Category = categoryDesktops, DisplayOrder = 1 });

			//#region pictures

			////pictures
			//productComputerDellInspiron23.ProductPictures.Add(new ProductPicture()
			//{
   //             Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_dellinspiron23.png"), "image/png", GetSeName(productComputerDellInspiron23.Name)),
			//	DisplayOrder = 1,
			//});
			//productComputerDellInspiron23.ProductPictures.Add(new ProductPicture()
			//{
			//	Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "0000954_dell-inspiron-one-23.jpeg"), "image/jpeg", GetSeName(productComputerDellInspiron23.Name)),
			//	DisplayOrder = 2,
			//});
			//productComputerDellInspiron23.ProductPictures.Add(new ProductPicture()
			//{
			//	Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "0000956_dell-inspiron-one-23.jpeg"), "image/jpeg", GetSeName(productComputerDellInspiron23.Name)),
			//	DisplayOrder = 3,
			//});

			//#endregion pictures

			//#region manufacturer

			////manufacturer
			//productComputerDellInspiron23.ProductManufacturers.Add(new ProductManufacturer()
			//{
			//	Manufacturer = _ctx.Set<Manufacturer>().Where(c => c.Name == "Dell").Single(),
			//	DisplayOrder = 1,
			//});

   //         #endregion manufacturer

   //         #region SpecificationAttributes
   //         //attributes
   //         productComputerDellInspiron23.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
   //         {
   //             AllowFiltering = true,
   //             ShowOnProductPage = true,
   //             DisplayOrder = 1,
   //             // CPU -> Intel
   //             SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 1).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 2).Single()
   //         });
   //         productComputerDellInspiron23.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
			//{
			//	AllowFiltering = true,
			//	ShowOnProductPage = true,
			//	DisplayOrder = 2,
			//	// RAM -> 4 GB 
			//	SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 4).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 1).Single()
			//});
			//productComputerDellInspiron23.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
			//{
			//	AllowFiltering = true,
			//	ShowOnProductPage = true,
			//	DisplayOrder = 3,
			//	// Harddisk-Typ / HDD
			//	SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 16).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 1).Single()
			//});
			//productComputerDellInspiron23.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
			//{
			//	AllowFiltering = true,
			//	ShowOnProductPage = true,
			//	DisplayOrder = 4,
			//	// Harddisk-Capacity / 750 GB
			//	SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 3).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 3).Single()
			//});
			//productComputerDellInspiron23.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
			//{
			//	AllowFiltering = true,
			//	ShowOnProductPage = true,
			//	DisplayOrder = 5,
			//	// OS / Windows 7 32 Bit
			//	SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 5).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 1).Single()
			//});
			//#endregion SpecificationAttributes

			//#endregion productComputerDellInspiron23

			//#region productComputerDellOptiplex3010

			//var productComputerDellOptiplex3010 = new Product()
			//{
			//	ProductType = ProductType.SimpleProduct,
			//	VisibleIndividually = true,
			//	Name = "Dell Optiplex 3010 DT Base",
			//	ShortDescription = "SPECIAL OFFER: Extra 50 € discount on all Dell OptiPlex desktops from a value of € 549. Online Coupon:? W8DWQ0ZRKTM1, valid until 04/12/2013.",
			//	FullDescription = "<p>Also included in this system include To change these selections, the</p> <p> 1 Year Basic Service - On-Site NBD - No Upgrade Selected<br> No asset tag required </p> <p> The following options are default selections included with your order. <br> German (QWERTY) Dell KB212-B Multimedia USB Keyboard Black<br> X11301001<br> WINDOWS LIVE <br> OptiPlex ™ order - Germany  <br> OptiPlex ™ Intel ® Core ™ i3 sticker <br> Optical software is not required, operating system software sufficiently   <br> </p>",
   //             Sku = "P-1013",
			//	ProductTemplateId = productTemplateSimple.Id,
			//	AllowCustomerReviews = true,
			//	Published = true,
			//	MetaTitle = "Dell Optiplex 3010 DT Base",
			//	Price = 419.00M,
			//	ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
			//	OrderMinimumQuantity = 1,
			//	OrderMaximumQuantity = 10000,
			//	StockQuantity = 10000,
			//	NotifyAdminForQuantityBelow = 1,
			//	AllowBackInStockSubscriptions = false,
			//	IsShipEnabled = true,
			//	DeliveryTime = _ctx.Set<DeliveryTime>().Where(sa => sa.DisplayOrder == 0).Single()
			//};

   //         productComputerDellOptiplex3010.ProductCategories.Add(new ProductCategory() { Category = categoryComputer, DisplayOrder = 1 });
   //         productComputerDellOptiplex3010.ProductCategories.Add(new ProductCategory() { Category = categoryDesktops, DisplayOrder = 1 });

			//#region pictures

			////pictures
			//productComputerDellOptiplex3010.ProductPictures.Add(new ProductPicture()
			//{
   //             Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_dellinspiron23.png"), "image/png", GetSeName(productComputerDellOptiplex3010.Name)),
			//	DisplayOrder = 1,
			//});
			//productComputerDellOptiplex3010.ProductPictures.Add(new ProductPicture()
			//{
			//	Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "0000954_dell-inspiron-one-23.jpeg"), "image/jpeg", GetSeName(productComputerDellOptiplex3010.Name)),
			//	DisplayOrder = 2,
			//});
			//productComputerDellOptiplex3010.ProductPictures.Add(new ProductPicture()
			//{
			//	Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "0000956_dell-inspiron-one-23.jpeg"), "image/jpeg", GetSeName(productComputerDellOptiplex3010.Name)),
			//	DisplayOrder = 3,
			//});

			//#endregion pictures

			//#region manufacturer

			////manufacturer
			//productComputerDellOptiplex3010.ProductManufacturers.Add(new ProductManufacturer()
			//{
			//	Manufacturer = _ctx.Set<Manufacturer>().Where(c => c.Name == "Dell").Single(),
			//	DisplayOrder = 1,
			//});

			//#endregion manufacturer

			//#region SpecificationAttributes
			////attributes
			//productComputerDellOptiplex3010.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
			//{
			//	AllowFiltering = true,
			//	ShowOnProductPage = true,
			//	DisplayOrder = 1,
			//	// CPU -> Intel
			//	SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 1).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 2).Single()
			//});
			//productComputerDellOptiplex3010.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
			//{
			//	AllowFiltering = true,
			//	ShowOnProductPage = true,
			//	DisplayOrder = 2,
			//	// RAM -> 4 GB 
			//	SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 4).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 2).Single()
			//});
			//productComputerDellOptiplex3010.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
			//{
			//	AllowFiltering = true,
			//	ShowOnProductPage = true,
			//	DisplayOrder = 3,
			//	// Harddisk-Typ / HDD
			//	SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 16).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 2).Single()
			//});
			//productComputerDellOptiplex3010.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
			//{
			//	AllowFiltering = true,
			//	ShowOnProductPage = true,
			//	DisplayOrder = 4,
			//	// Harddisk-Capacity / 750 GB
			//	SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 3).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 2).Single()
			//});
			//productComputerDellOptiplex3010.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
			//{
			//	AllowFiltering = true,
			//	ShowOnProductPage = true,
			//	DisplayOrder = 5,
			//	// OS / Windows 7 32 Bit
			//	SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 5).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 4).Single()
			//});
			//#endregion SpecificationAttributes

			//#endregion productComputerDellOptiplex3010

			//#region productComputerAcerAspireOne
			//var productComputerAcerAspireOne = new Product()
			//{
			//	ProductType = ProductType.SimpleProduct,
			//	VisibleIndividually = true,
			//	Name = "Acer Aspire One 8.9\" Mini-Notebook Case - (Black)",
			//	ShortDescription = "Acer Aspire One 8.9\" Mini-Notebook and 6 Cell Battery model (AOA150-1447)",
			//	FullDescription = "<p>Acer Aspire One 8.9&quot; Memory Foam Pouch is the perfect fit for Acer Aspire One 8.9&quot;. This pouch is made out of premium quality shock absorbing memory form and it provides extra protection even though case is very light and slim. This pouch is water resistant and has internal supporting bands for Acer Aspire One 8.9&quot;. Made In Korea.</p>",
   //             Sku = "P-1014",
			//	ProductTemplateId = productTemplateSimple.Id,
			//	AllowCustomerReviews = true,
			//	Published = true,
			//	MetaTitle = "Acer Aspire One 8.9",
			//	ShowOnHomePage = true,
			//	Price = 210.6M,
			//	IsShipEnabled = true,
			//	Weight = 2,
			//	Length = 2,
			//	Width = 2,
			//	Height = 3,
			//	ManageInventoryMethod = ManageInventoryMethod.ManageStock,
			//	StockQuantity = 10000,
			//	NotifyAdminForQuantityBelow = 1,
			//	AllowBackInStockSubscriptions = false,
			//	DisplayStockAvailability = true,
			//	LowStockActivity = LowStockActivity.DisableBuyButton,
			//	BackorderMode = BackorderMode.NoBackorders,
			//	OrderMinimumQuantity = 1,
			//	OrderMaximumQuantity = 10000,
			//	DeliveryTime = _ctx.Set<DeliveryTime>().Where(sa => sa.DisplayOrder == 0).Single()
			//};

   //         productComputerAcerAspireOne.ProductCategories.Add(new ProductCategory() { Category = categoryComputer, DisplayOrder = 1 });
   //         productComputerAcerAspireOne.ProductCategories.Add(new ProductCategory() { Category = categoryNotebooks, DisplayOrder = 1 });

			//#region manufacturer

			////manufacturer
			//productComputerAcerAspireOne.ProductManufacturers.Add(new ProductManufacturer()
			//{
			//	Manufacturer = _ctx.Set<Manufacturer>().Where(c => c.Name == "Acer").Single(),
			//	DisplayOrder = 1,
			//});

			//#endregion manufacturer

			//#region tierPrieces
			//productComputerAcerAspireOne.TierPrices.Add(new TierPrice()
			//{
			//	Quantity = 2,
			//	Price = 205
			//});
			//productComputerAcerAspireOne.TierPrices.Add(new TierPrice()
			//{
			//	Quantity = 5,
			//	Price = 189
			//});
			//productComputerAcerAspireOne.TierPrices.Add(new TierPrice()
			//{
			//	Quantity = 10,
			//	Price = 155
			//});
			//productComputerAcerAspireOne.HasTierPrices = true;
			//#endregion tierPrieces

			//#region pictures
			//productComputerAcerAspireOne.ProductPictures.Add(new ProductPicture()
			//{
   //             Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_aceraspiresl1500.png"), "image/png", GetSeName(productComputerAcerAspireOne.Name)),
			//	DisplayOrder = 1,
			//});
			//productComputerAcerAspireOne.ProductPictures.Add(new ProductPicture()
			//{
			//	Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "01-12Hand_Aspire1.jpg"), "image/jpeg", GetSeName(productComputerAcerAspireOne.Name)),
			//	DisplayOrder = 2,
			//});
			//productComputerAcerAspireOne.ProductPictures.Add(new ProductPicture()
			//{
			//	Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "58_00007561.jpg"), "image/jpeg", GetSeName(productComputerAcerAspireOne.Name)),
			//	DisplayOrder = 3,
			//});

			//#endregion tierPrieces

			//#endregion productComputerAcerAspireOne

			#endregion computer

            #region Instant Download Music / Digital Products

            var categoryDigitalProducts = this._ctx.Set<Category>().First(c => c.Alias == "Digital Products");

            #region product Books Stone of the Wise

            var productBooksStoneOfTheWise = new Product()
            {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Ebook 'Stone of the Wise' in 'Lorem ipsum'",
                IsEsd = true,
                ShortDescription = "E-Book, 465 pages",
                FullDescription = "",
                Sku = "P-6001",
                ProductTemplateId = productTemplate.Id,
                AllowCustomerReviews = true,
                Published = true,
                MetaTitle = "Ebook 'Stone of the Wise' in 'Lorem ipsum'",
                Price = 9.90M,
                ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                IsDownload = true,
                HasSampleDownload = true,
                SampleDownload = new Download
                {
                    DownloadGuid = Guid.NewGuid(),
                    ContentType = "application/pdf",
                    MediaStorage = new MediaStorage
                    {
                        Data = File.ReadAllBytes(sampleDownloadsPath + "Stone_of_the_wise_preview.pdf")
                    },
                    Extension = ".pdf",
                    Filename = "Stone_of_the_wise_preview",
                    IsNew = true,
                    UpdatedOnUtc = DateTime.UtcNow
                }
            };

            productBooksStoneOfTheWise.ProductCategories.Add(new ProductCategory() { Category = categoryDigitalProducts, DisplayOrder = 1 });

            //pictures
            productBooksStoneOfTheWise.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "stone_of_wisdom.jpg"), "image/jpeg", GetSeName(productBooksStoneOfTheWise.Name)),
                DisplayOrder = 1,
            });

            //attributes
            productBooksStoneOfTheWise.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 3,
                // Edition -> bound
                SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 13).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 1).Single()
            });
            productBooksStoneOfTheWise.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 3,
                // Category -> cars
                SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 14).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 6).Single()
            });
            productBooksStoneOfTheWise.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 3,
                // Language -> German
                SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 12).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 1).Single()
            });


            #endregion product Books Stone of the Wise


            #region Antonio Vivaldi: then spring

            var productInstantDownloadVivaldi = new Product
            {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Antonio Vivaldi: spring",
                IsEsd = true,
                ShortDescription = "MP3, 320 kbit/s",
                FullDescription = "<p>Antonio Vivaldi: Spring</p> <p>Antonio Lucio Vivaldi (March 4, 1678 in Venice, &dagger; 28 July 1741 in Vienna) was a Venetian composer and violinist in the Baroque.</p> <p>The Four Seasons (Le quattro stagioni Italian) is perhaps the most famous works of Antonio Vivaldi. It's four violin concertos with extra-musical programs, each portraying a concert season. This is the individual concerts one - probably written by Vivaldi himself - Sonnet preceded by consecutive letters in front of the lines and in the appropriate places in the score arrange the verbal description of the music.</p> <p>Vivaldi had previously always been experimenting with non-musical programs, which often reflected in his tracks, the exact interpretation of the individual points score is unusual for him. His experience as a virtuoso violinist allowed him access to particularly effective playing techniques, as an opera composer, he had developed a strong sense of effects, both of which benefitted from him.</p> <p>As the title suggests, especially to imitate natural phenomena - gentle winds, severe storms and thunderstorms are elements that are common to all four concerts. There are also various birds and even a dog, further human activities such as hunting, a barn dance, ice skating, including stumbling and falling to the heavy sleep of a drunkard.</p> <p>The work dates from 1725 and is available in two print editions, which appeared more or less simultaneously published in Amsterdam and Paris.</p>",
                Sku = "P-1016",
                ProductTemplateId = productTemplate.Id,
                AllowCustomerReviews = true,
                Published = true,
                MetaTitle = "Antonio Vivaldi: spring",
                Price = 1.99M,
                ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                IsDownload = true,
                HasSampleDownload = true,
                SampleDownload = new Download
                {
                    DownloadGuid = Guid.NewGuid(),
                    ContentType = "audio/mp3",
                    MediaStorage = new MediaStorage
                    {
                        Data = File.ReadAllBytes(sampleDownloadsPath + "vivaldi-four-seasons-spring.mp3")
                    },
                    Extension = ".mp3",
                    Filename = "vivaldi-four-seasons-spring",
                    IsNew = true,
                    UpdatedOnUtc = DateTime.UtcNow
                }
            };

            productInstantDownloadVivaldi.ProductCategories.Add(new ProductCategory() { Category = categoryDigitalProducts, DisplayOrder = 1 });

            #region pictures

            //pictures
            productInstantDownloadVivaldi.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "vivaldi.jpg"), "image/jpeg", GetSeName(productInstantDownloadVivaldi.Name)),
                DisplayOrder = 1,
            });

            #endregion pictures

            #region SpecificationAttributes
            //attributes
            productInstantDownloadVivaldi.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 1,
                // mp3 quality > 320 kbit/S
                SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 18).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 3).Single()
            });
            productInstantDownloadVivaldi.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 2,
                // genre > classic
                SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 19).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 6).Single()
            });

            #endregion SpecificationAttributes

            #endregion Antonio Vivildi: then spring

            #region Beethoven für Elise

            var productInstantDownloadBeethoven = new Product()
            {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Ludwig van Beethoven: For Elise",
                IsEsd = true,
                ShortDescription = "Ludwig van Beethoven's most popular compositions",
                FullDescription = "<p> The score was not published until 1867, 40 years after the composer's death in 1827. The discoverer of the piece, Ludwig Nohl, affirmed that the original autographed manuscript, now lost, was dated 27 April 1810.[4] The version of \"Für Elise\" we hear today is an earlier version that was transcribed by Ludwig Nohl. There is a later version, with drastic changes to the accompaniment which was transcribed from a later manuscript by Barry Cooper. The most notable difference is in the first theme, the left-hand arpeggios are delayed by a 16th note beat. There are a few extra bars in the transitional section into the B section; and finally, the rising A minor arpeggio figure is moved later into the piece. The tempo marking Poco Moto is believed to have been on the manuscript that Ludwig Nohl transcribed (now lost). The later version includes the marking Molto Grazioso. It is believed that Beethoven intended to add the piece to a cycle of bagatelles.[citation needed] </p> <p> Therese Malfatti, widely believed to be the dedicatee of \"Für Elise\" The pianist and musicologist Luca Chiantore (es) argued in his thesis and his 2010 book Beethoven al piano that Beethoven might not have been the person who gave the piece the form that we know today. Chiantore suggested that the original signed manuscript, upon which Ludwig Nohl claimed to base his transcription, may never have existed.[5] On the other hand, the musicologist Barry Cooper stated, in a 1984 essay in The Musical Times, that one of two surviving sketches closely resembles the published version.[6] </p>",
                Sku = "P-1017",
                ProductTemplateId = productTemplate.Id,
                AllowCustomerReviews = true,
                Published = true,
                MetaTitle = "Ludwig van Beethoven: Für Elise",
                ShowOnHomePage = true,
                Price = 1.89M,
                ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                IsDownload = true,
                HasSampleDownload = true,
                SampleDownload = new Download()
                {
                    DownloadGuid = Guid.NewGuid(),
                    ContentType = "audio/mp3",
                    MediaStorage = new MediaStorage
                    {
                        Data = File.ReadAllBytes(sampleDownloadsPath + "beethoven-fur-elise.mp3")
                    },
                    Extension = ".mp3",
                    Filename = "beethoven-fur-elise.mp3",
                    IsNew = true,
                    UpdatedOnUtc = DateTime.UtcNow
                }
            };

            productInstantDownloadBeethoven.ProductCategories.Add(new ProductCategory() { Category = categoryDigitalProducts, DisplayOrder = 1 });

            #region pictures

            //pictures
            productInstantDownloadBeethoven.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "Beethoven.jpg"), "image/jpeg", GetSeName(productInstantDownloadBeethoven.Name)),
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
                SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 18).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 3).Single()
            });
            productInstantDownloadBeethoven.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 2,
                // genre > classic
                SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 19).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 6).Single()
            });

            #endregion SpecificationAttributes

            #endregion Beethoven für Elise

            #endregion Instant Download Music

            #region watches

            var categoryWatches = this._ctx.Set<Category>().First(c => c.Alias == "Watches");


            #region productTRANSOCEANCHRONOGRAPH

            var productTRANSOCEANCHRONOGRAPH = new Product()
            {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "TRANSOCEAN CHRONOGRAPH",
                ShortDescription = "The Transocean Chronograph interprets the factual aesthetics of classic chronographs of the 1950s and 1960s in a decidedly contemporary style.",
                FullDescription = "<p>The Transocean Chronograph interprets the factual aesthetics of classic chronographs of the 1950s and 1960s in a decidedly contemporary style. The high-performance caliber 01, designed and manufactured entirely in the Breitling studios, works in its form, which is reduced to the essentials. </p> <p> </p> <table style='width: 425px;'>   <tbody>     <tr>       <td style='width: 185px;'>Caliber       </td>       <td style='width: 237px;'>Breitling 01 (Manufactory caliber)       </td>     </tr>     <tr>       <td style='width: 185px;'>Movement       </td>       <td style='width: 237px;'>Mechanically, Automatic       </td>     </tr>     <tr>       <td style='width: 185px;'>Power reserve       </td>       <td style='width: 237px;'>Min. 70 hour       </td>     </tr>     <tr>       <td style='width: 185px;'>Chronograph       </td>       <td style='width: 237px;'>1/4-Seconds, 30 Minutes, 12 Hours       </td>     </tr>     <tr>       <td style='width: 185px;'>Half vibrations       </td>       <td style='width: 237px;'>28 800 a/h       </td>     </tr>     <tr>       <td style='width: 185px;'>Rubies       </td>       <td style='width: 237px;'>47 Rubies       </td>     </tr>     <tr>       <td style='width: 185px;'>Calendar       </td>       <td style='width: 237px;'>Window       </td>     </tr>   </tbody> </table> ",
                Sku = "P-9001",
                ProductTemplateId = productTemplate.Id,
                AllowCustomerReviews = true,
                Published = true,
                MetaTitle = "TRANSOCEAN CHRONOGRAPH",
                ShowOnHomePage = true,
                Price = 24110.00M,
                OldPrice = 26230.00M,
                ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                IsShipEnabled = true,
                DeliveryTime = _ctx.Set<DeliveryTime>().Where(sa => sa.DisplayOrder == 2).Single()
            };

            productTRANSOCEANCHRONOGRAPH.ProductCategories.Add(new ProductCategory() { Category = categoryWatches, DisplayOrder = 1 });

            #region pictures

            //pictures
            productTRANSOCEANCHRONOGRAPH.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_transocean-chronograph.jpg"), "image/png", GetSeName(productTRANSOCEANCHRONOGRAPH.Name)),
                DisplayOrder = 1,
            });

            #endregion pictures

            #region manufacturer

            //manufacturer
            productTRANSOCEANCHRONOGRAPH.ProductManufacturers.Add(new ProductManufacturer()
            {
                Manufacturer = _ctx.Set<Manufacturer>().Where(c => c.Name == "Breitling").Single(),
                DisplayOrder = 1,
            });

            #endregion manufacturer

            #region SpecificationAttributes
            //attributes
            productTRANSOCEANCHRONOGRAPH.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 1,
                // offer > promotion
                SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 22).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 3).Single()
            });
            productTRANSOCEANCHRONOGRAPH.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 2,
                // manufacturer > Breitling
                SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 20).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 18).Single()
            });
            productTRANSOCEANCHRONOGRAPH.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 1,
                // housing > steel
                SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 8).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 1).Single()
            });
            productTRANSOCEANCHRONOGRAPH.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 5,
                // material -> leather
                SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 8).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 5).Single()
            });
            productTRANSOCEANCHRONOGRAPH.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 5,
                // Gender -> gentlemen
                SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 7).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 1).Single()
            });
            productTRANSOCEANCHRONOGRAPH.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 5,
                // movement -> mechanical, self winding
                SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 9).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 1).Single()
            });
            productTRANSOCEANCHRONOGRAPH.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 5,
                // diameter -> 44mm
                SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 24).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 3).Single()
            });
            productTRANSOCEANCHRONOGRAPH.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 5,
                // closure -> folding clasp
                SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 25).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 2).Single()
            });
            #endregion SpecificationAttributes

            #endregion productTRANSOCEANCHRONOGRAPH

            #region productTissotT-TouchExpertSolar

            var productTissotTTouchExpertSolar = new Product()
            {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Tissot T-Touch Expert Solar",
                ShortDescription = "The beam of the Tissot T-Touch Expert Solar on the dial ensures that the Super-LumiNova®-coated indexes and hands illuminate in the dark, and on the other hand, charges the battery of the watch. This model is a force package in every respect.",
                FullDescription = "<p>The T-Touch Expert Solar is an important new model in the Tissot range. </p> <p>Tissot’s pioneering spirit is what led to the creation of tactile watches in 1999. </p> <p>Today, it is the first to present a touch-screen watch powered by solar energy, confirming its position as leader in tactile technology in watchmaking. </p> <p>Extremely well designed, it showcases clean lines in both sports and timeless pieces. </p> <p>Powered by solar energy with 25 features including weather forecasting, altimeter, second time zone and a compass it is the perfect travel companion. </p> ",
                Sku = "P-9002",
                ProductTemplateId = productTemplate.Id,
                AllowCustomerReviews = true,
                Published = true,
                MetaTitle = "Tissot T-Touch Expert Solar",
                ShowOnHomePage = true,
                Price = 969.00M,
                OldPrice = 990.00M,
                ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                IsShipEnabled = true,
                DeliveryTime = _ctx.Set<DeliveryTime>().Where(sa => sa.DisplayOrder == 2).Single()
            };

            productTissotTTouchExpertSolar.ProductCategories.Add(new ProductCategory() { Category = categoryWatches, DisplayOrder = 1 });

            #region pictures

            //pictures
            productTissotTTouchExpertSolar.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_tissot-t-touch-expert-solar.jpg"), "image/png", GetSeName(productTissotTTouchExpertSolar.Name)),
                DisplayOrder = 1,
            });

            productTissotTTouchExpertSolar.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_tissot-t-touch-expert-solar-t091_2.jpg"), "image/png", GetSeName(productTissotTTouchExpertSolar.Name)),
                DisplayOrder = 1,
            });

            #endregion pictures

            #region manufacturer

            //manufacturer
            productTissotTTouchExpertSolar.ProductManufacturers.Add(new ProductManufacturer()
            {
                Manufacturer = _ctx.Set<Manufacturer>().Where(c => c.Name == "Tissot").Single(),
                DisplayOrder = 1,
            });

            #endregion manufacturer

            #region SpecificationAttributes
            //attributes
            productTissotTTouchExpertSolar.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 1,
                // offer > best price
                SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 22).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 8).Single()
            });
            productTissotTTouchExpertSolar.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 2,
                // manufacturer > Tissot
                SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 20).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 17).Single()
            });
            productTissotTTouchExpertSolar.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 1,
                // housing > steel
                SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 8).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 1).Single()
            });
            productTissotTTouchExpertSolar.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 5,
                // material -> silicone
                SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 8).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 7).Single()
            });
            productTissotTTouchExpertSolar.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 5,
                // Gender -> gentlemen
                SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 7).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 1).Single()
            });
            productTissotTTouchExpertSolar.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 5,
                // movement -> Automatic, self-winding
                SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 9).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 1).Single()
            });
            productTissotTTouchExpertSolar.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 5,
                // diameter -> 44mm
                SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 24).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 3).Single()
            });
            productTissotTTouchExpertSolar.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 5,
                // closure -> thorn close
                SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 25).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 3).Single()
            });
            #endregion SpecificationAttributes

            #endregion productTissotT-TouchExpertSolar

            #region productSeikoSRPA49K1

            var productSeikoSRPA49K1 = new Product()
            {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                Name = "Seiko Mechanical Automatic SRPA49K1",
                ShortDescription = "Seiko Mechanical Automatic SRPA49K1",
                FullDescription = "<p><strong>Seiko 5 Sports Automatic Watch SRPA49K1 SRPA49</strong> </p> <ul>   <li>Unidirectional Rotating Bezel</li>   <li>Day And Date Display</li>   <li>See Through Case Back</li>   <li>100M Water Resistance</li>   <li>Stainless Steel Case</li>   <li>Automatic Movement</li>   <li>24 Jewels</li>   <li>Caliber: 4R36</li> </ul> ",
                Sku = "P-9003",
                ProductTemplateId = productTemplate.Id,
                AllowCustomerReviews = true,
                Published = true,
                MetaTitle = "Seiko Mechanical Automatic SRPA49K1",
                ShowOnHomePage = true,
                Price = 269.00M,
                OldPrice = 329.00M,
                ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                IsShipEnabled = true,
                DeliveryTime = _ctx.Set<DeliveryTime>().Where(sa => sa.DisplayOrder == 2).Single()
            };

            productSeikoSRPA49K1.ProductCategories.Add(new ProductCategory() { Category = categoryWatches, DisplayOrder = 1 });

            #region pictures

            //pictures
            productSeikoSRPA49K1.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_SeikoSRPA49K1.jpg"), "image/png", GetSeName(productSeikoSRPA49K1.Name)),
                DisplayOrder = 1,
            });

            #endregion pictures

            #region manufacturer

            //manufacturer
            productSeikoSRPA49K1.ProductManufacturers.Add(new ProductManufacturer()
            {
                Manufacturer = _ctx.Set<Manufacturer>().Where(c => c.Name == "Seiko").Single(),
                DisplayOrder = 1,
            });

            #endregion manufacturer

            #region SpecificationAttributes
            //attributes
            productSeikoSRPA49K1.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 1,
                // housing > steel
                SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 8).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 1).Single()
            });
            productSeikoSRPA49K1.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 5,
                // material -> stainless steel
                SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 8).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 1).Single()
            });
            productSeikoSRPA49K1.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 2,
                // manufacturer > Seiko
                SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 20).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 16).Single()
            });
            productSeikoSRPA49K1.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 5,
                // Gender -> gentlemen
                SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 7).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 1).Single()
            });
            productSeikoSRPA49K1.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 5,
                // movement -> quarz
                SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 9).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 1).Single()
            });
            productSeikoSRPA49K1.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 5,
                // closure -> folding clasp
                SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 25).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 2).Single()
            });
            productSeikoSRPA49K1.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 5,
                // diameter -> 44mm
                SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 24).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 3).Single()
            });
            #endregion SpecificationAttributes

            #endregion productSeikoSRPA49K1 


            #region productWatchesCertinaDSPodiumBigSize

            var productWatchesCertinaDSPodiumBigSize = new Product()
			{
				ProductType = ProductType.SimpleProduct,
				VisibleIndividually = true,
				Name = "Certina DS Podium Big Size",
				ShortDescription = "C001.617.26.037.00",
				FullDescription = "<p>Since 1888, Certina has maintained an enviable reputation for its excellent watches and reliable movements. From the time of its integration into the SMH (today's Swatch Group) in the early 1980s, every Certina has been equipped with a high-quality ETA movement.</p><p>In a quartz watch movement, high-frequency oscillations are generated in a tiny synthetic crystal, then divided down electronically to provide the extreme accuracy of the Certina internal clock. A battery supplies the necessary energy.</p><p>The quartz movement is sometimes equipped with an end-of-life (EOL) indicator. When the seconds hand begins moving in four-second increments, the battery should be replaced within two weeks.</p><p>An automatic watch movement is driven by a rotor. Arm and wrist movements spin the rotor, which in turn winds the main spring. Energy is continuously produced, eliminating the need for a battery. The rate precision therefore depends on a rigorous manufacturing process and the original calibration, as well as the lifestyle of the user.</p><p>Most automatic movements are driven by an offset rotor. To earn the title of chronometer, a watch must be equipped with a movement that has obtained an official rate certificate from the COSC (Contrôle Officiel Suisse des Chronomètres). To obtain this, precision tests in different positions and at different temperatures must be carried out. These tests take place over a 15-day period. Thermocompensated means that the effective temperature inside the watch is measured and taken into account when improving precision. This allows fluctuations in the rate precision of a normal quartz watch due to temperature variations to be reduced by several seconds a week. The precision is 20 times more accurate than on a normal quartz watch, i.e. +/- 10 seconds per year (0.07 seconds/day).</p>",
                Sku = "P-9004",
				ProductTemplateId = productTemplate.Id,
				AllowCustomerReviews = true,
				Published = true,
				MetaTitle = "Certina DS Podium Big Size",
				ShowOnHomePage = true,
				Price = 479.00M,
				ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
				OrderMinimumQuantity = 1,
				OrderMaximumQuantity = 10000,
				StockQuantity = 10000,
				NotifyAdminForQuantityBelow = 1,
				AllowBackInStockSubscriptions = false,
				IsShipEnabled = true,
				DeliveryTime = _ctx.Set<DeliveryTime>().Where(sa => sa.DisplayOrder == 2).Single()
			};

            productWatchesCertinaDSPodiumBigSize.ProductCategories.Add(new ProductCategory() { Category = categoryWatches, DisplayOrder = 1 });

			#region pictures

			//pictures
			productWatchesCertinaDSPodiumBigSize.ProductPictures.Add(new ProductPicture()
			{
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_certina_ds_podium_big.png"), "image/png", GetSeName(productWatchesCertinaDSPodiumBigSize.Name)),
				DisplayOrder = 1,
			});

			#endregion pictures

			#region manufacturer

			//manufacturer
			productWatchesCertinaDSPodiumBigSize.ProductManufacturers.Add(new ProductManufacturer()
			{
				Manufacturer = _ctx.Set<Manufacturer>().Where(c => c.Name == "Certina").Single(),
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
				SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 8).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 1).Single()
			});
            productWatchesCertinaDSPodiumBigSize.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 5,
                // material -> leather
                SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 8).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 5).Single()
            });
            productWatchesCertinaDSPodiumBigSize.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
			{
				AllowFiltering = true,
				ShowOnProductPage = true,
				DisplayOrder = 2,
				// manufacturer > Certina
				SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 20).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 14).Single()
			});
			productWatchesCertinaDSPodiumBigSize.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
			{
				AllowFiltering = true,
				ShowOnProductPage = true,
				DisplayOrder = 5,
				// Gender -> gentlemen
				SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 7).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 1).Single()
			});
            productWatchesCertinaDSPodiumBigSize.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 5,
                // movement -> quarz
                SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 9).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 3).Single()
            });
            productWatchesCertinaDSPodiumBigSize.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 5,
                // closure -> folding clasp
                SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 25).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 2).Single()
            });
            productWatchesCertinaDSPodiumBigSize.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute()
            {
                AllowFiltering = true,
                ShowOnProductPage = true,
                DisplayOrder = 5,
                // diameter -> 40mm
                SpecificationAttributeOption = _ctx.Set<SpecificationAttribute>().Where(sa => sa.DisplayOrder == 24).Single().SpecificationAttributeOptions.Where(sao => sao.DisplayOrder == 2).Single()
            });
            #endregion SpecificationAttributes

            #endregion productWatchesCertinaDSPodiumBigSize

            #endregion watches

            #region gaming

            var manuSony = _ctx.Set<Manufacturer>().First(c => c.Name == "Sony");
            var manuEASports = _ctx.Set<Manufacturer>().First(c => c.Name == "EA Sports");
            var manuUbisoft = _ctx.Set<Manufacturer>().First(c => c.Name == "Ubisoft");
			var categoryGaming = this._ctx.Set<Category>().First(c => c.Alias == "Gaming");
			var categoryGamingAccessories = this._ctx.Set<Category>().First(c => c.Alias == "Gaming Accessories");
			var categoryGamingGames = this._ctx.Set<Category>().First(c => c.Alias == "Games");
            var manuWarnerHomme = _ctx.Set<Manufacturer>().First(c => c.Name == "Warner Home Video Games");
            
            #region bundlePs3AssassinCreed

            var productPs3 = new Product()
			{
				ProductType = ProductType.SimpleProduct,
				VisibleIndividually = true,
				Sku = "Sony-PS399000",
				Name = "Playstation 4 Pro",
				ShortDescription = "The Sony PlayStation 4 Pro is the multi media console for next-generation digital home entertainment. It offers the Blu-ray technology, which enables you to enjoy movies in high definition.",
				FullDescription = "<ul><li>PowerPC-base Core @5.2GHz</li><li>1 VMX vector unit per core</li><li>512KB L2 cache</li><li>7 x SPE @5.2GHz</li><li>7 x 128b 128 SIMD GPRs</li><li>7 x 256MB SRAM for SPE</li><li>* 1 of 8 SPEs reserved for redundancy total floating point performance: 218 GFLOPS</li><li> 1.8 TFLOPS floating point performance</li><li>Full HD (up to 1080p) x 2 channels</li><li>Multi-way programmable parallel floating point shader pipelines</li><li>GPU: RSX @550MHz</li><li>256MB XDR Main RAM @3.2GHz</li><li>256MB GDDR3 VRAM @700MHz</li><li>Sound: Dolby 5.1ch, DTS, LPCM, etc. (Cell-base processing)</li><li>Wi-Fi: IEEE 802.11 b/g</li><li>USB: Front x 4, Rear x 2 (USB2.0)</li><li>Memory Stick: standard/Duo, PRO x 1</li></ul>",
				ProductTemplateId = productTemplate.Id,
				AllowCustomerReviews = true,
				Published = true,
                //MetaTitle = "Playstation 4 Super Slim",
                MetaTitle = "Playstation 4 Pro",
                Price = 189.00M,
				OldPrice = 199.99M,
				ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
				OrderMinimumQuantity = 1,
				OrderMaximumQuantity = 10000,
				StockQuantity = 10000,
				NotifyAdminForQuantityBelow = 1,
				AllowBackInStockSubscriptions = false,
				IsShipEnabled = true,
				DeliveryTime = firstDeliveryTime
			};

			productPs3.ProductManufacturers.Add(new ProductManufacturer() { Manufacturer = manuSony, DisplayOrder = 1 });
			productPs3.ProductCategories.Add(new ProductCategory() { Category = categoryGaming,	DisplayOrder = 4 });

			productPs3.ProductPictures.Add(new ProductPicture()
			{
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_ps4_w_controller.jpg"), "image/png", GetSeName(productPs3.Name) + "-controller"),
				DisplayOrder = 1
			});
			productPs3.ProductPictures.Add(new ProductPicture()
            {
				Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_ps4_wo_controller.jpg"), "image/jpeg", GetSeName(productPs3.Name) + "-single"),
				DisplayOrder = 2
			});


            var productDualshock4Controller = new Product()
			{
				ProductType = ProductType.SimpleProduct,
				VisibleIndividually = true,
				Sku = "Sony-PS399004",
				Name = "DUALSHOCK 4 Wireless Controller",
				ShortDescription = "Revolutionary. Intuitive. Precise. A revolutionary controller for a new era of gaming, the DualShock 4 Wireless Controller features familiar PlayStation controls and innovative new additions, such as a touch pad, light bar, and more.",
				FullDescription = "<ul>  <li>Precision Controller for PlayStation 4: The feel, shape, and sensitivity of the DualShock 4’s analog sticks and trigger buttons have been enhanced to offer players absolute control for all games</li>  <li>Sharing at your Fingertips: The addition of the Share button makes sharing your greatest gaming moments as easy as a push of a button. Upload gameplay videos and screenshots directly from your system or live-stream your gameplay, all without disturbing the game in progress.</li>  <li>New ways to Play: Revolutionary features like the touch pad, integrated light bar, and built-in speaker offer exciting new ways to experience and interact with your games and its 3.5mm audio jack offers a practical personal audio solution for gamers who want to listen to their games in private.</li>  <li>Charge Efficiently: The DualShock 4 Wireless Controller can easily be recharged by plugging it into your PlayStation 4 system, even when on standby, or with any standard charger with a micro-USB port.</li></ul>",
				ProductTemplateId = productTemplate.Id,
				AllowCustomerReviews = true,
				Published = true,
				MetaTitle = "DUALSHOCK 4 Wireless Controller",
				Price = 54.90M,
                OldPrice = 59.90M,
				ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
				OrderMinimumQuantity = 1,
				OrderMaximumQuantity = 10000,
				StockQuantity = 10000,
				NotifyAdminForQuantityBelow = 1,
				AllowBackInStockSubscriptions = false,
				IsShipEnabled = true,
				DeliveryTime = firstDeliveryTime
			};

            productDualshock4Controller.ProductManufacturers.Add(new ProductManufacturer() { Manufacturer = manuSony, DisplayOrder = 1 });
            productDualshock4Controller.ProductCategories.Add(new ProductCategory() { Category = categoryGamingAccessories, DisplayOrder = 1 });

            productDualshock4Controller.ProductPictures.Add(new ProductPicture()
			{
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_dualshock4.jpg"), "image/png", GetSeName(productDualshock4Controller.Name)),
				DisplayOrder = 1
			});


			var productMinecraft = new Product()
			{
				ProductType = ProductType.SimpleProduct,
				VisibleIndividually = true,
                //Sku = "Ubi-acreed3",
                Sku = "PD-Minecraft4ps4",
                Name = "Minecraft - Playstation 4 Edition",
				ShortDescription = "Third-person action-adventure title set.",
				FullDescription = "<p><strong>Build! Craft! Explore! </strong></p><p>The critically acclaimed Minecraft comes to PlayStation 4, offering bigger worlds and greater draw distance than the PS3 and PS Vita editions.</p><p>Create your own world, then, build, explore and conquer. When night falls the monsters appear, so be sure to build a shelter before they arrive.</p><p>The world is only limited by your imagination! Bigger worlds and greater draw distance than PS3 and PS Vita Editions Includes all features from the PS3 version Import your PS3 and PS Vita worlds to the PS4 Editition.</p>",
				ProductTemplateId = productTemplate.Id,
				AllowCustomerReviews = true,
				Published = true,
				//MetaTitle = "Assassin's Creed III",
                MetaTitle = "Minecraft - Playstation 4 Edition",

                Price = 49.90M,
				ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
				OrderMinimumQuantity = 1,
				OrderMaximumQuantity = 10000,
				StockQuantity = 10000,
				NotifyAdminForQuantityBelow = 1,
				AllowBackInStockSubscriptions = false,
				IsShipEnabled = true,
				DeliveryTime = firstDeliveryTime
			};

            productMinecraft.ProductManufacturers.Add(new ProductManufacturer() {	Manufacturer = manuSony,	DisplayOrder = 1 });
            productMinecraft.ProductCategories.Add(new ProductCategory() { Category = categoryGamingGames, DisplayOrder = 4 });

            productMinecraft.ProductPictures.Add(new ProductPicture()
			{
				Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_minecraft.jpg"), "image/jpeg", GetSeName("Minecraft - Playstation 4 Edition")),
				DisplayOrder = 1
			});


			var productBundlePs3AssassinCreed = new Product()
			{
				ProductType = ProductType.BundledProduct,
				VisibleIndividually = true,
				Sku = "Sony-PS399105",
				Name = "PlayStation 4 Minecraft Bundle",
				ShortDescription = "100GB PlayStation®4 system, 2 × DUALSHOCK®4 wireless controller and Minecraft for PS4 Edition.",
				FullDescription = 
					"<ul><li><h4>Processor</h4><ul><li>Processor Technology : Cell Broadband Engine™</li></ul></li><li><h4>General</h4><ul><li>Communication : Ethernet (10BASE-T, 100BASE-TX, 1000BASE-T IEEE 802.11 b/g Wi-Fi<br tabindex=\"0\">Bluetooth 2.0 (EDR)</li><li>Inputs and Outputs : USB 2.0 X 2</li></ul></li><li><h4>Graphics</h4><ul><li>Graphics Processor : RSX</li></ul></li><li><h4>Memory</h4><ul><li>Internal Memory : 256MB XDR Main RAM<br>256MB GDDR3 VRAM</li></ul></li><li><h4>Power</h4><ul><li>Power Consumption (in Operation) : Approximately 250 watts</li></ul></li><li><h4>Storage</h4><ul><li>Storage Capacity : 2.5' Serial ATA (500GB)</li></ul></li><li><h4>Video</h4><ul><li>Resolution : 480i, 480p, 720p, 1080i, 1080p (24p/60p)</li></ul></li><li><h4>Weights and Measurements</h4><ul><li>Dimensions (Approx.) : Approximately 11.42\" (W) x 2.56\" (H) x 11.42\" (D) (290mm x 65mm x 290mm)</li><li>Weight (Approx.) : Approximately 7.055 lbs (3.2 kg)</li></ul></li></ul>",
				ProductTemplateId = productTemplate.Id,
				AllowCustomerReviews = true,
				Published = true,
                MetaTitle = "PlayStation 4 Minecraft Bundle",
                Price = 269.97M,
				ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
				OrderMinimumQuantity = 1,
				OrderMaximumQuantity = 10000,
				StockQuantity = 10000,
				NotifyAdminForQuantityBelow = 1,
				AllowBackInStockSubscriptions = false,
				IsShipEnabled = true,
				DeliveryTime = firstDeliveryTime,
				ShowOnHomePage = true,
				BundleTitleText = "Bundle includes",
				BundlePerItemPricing = true,
				BundlePerItemShoppingCart = true
			};

			productBundlePs3AssassinCreed.ProductManufacturers.Add(new ProductManufacturer() { Manufacturer = manuSony, DisplayOrder = 1 });
			productBundlePs3AssassinCreed.ProductCategories.Add(new ProductCategory() { Category = categoryGaming, DisplayOrder = 1 });

			productBundlePs3AssassinCreed.ProductPictures.Add(new ProductPicture()
			{
				Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "ps4_bundle_minecraft.jpg"), "image/png", GetSeName(productBundlePs3AssassinCreed.Name)),
				DisplayOrder = 1
			});

			#endregion bundlePs3AssassinCreed

			#region bundlePs4

			var productPs4 = new Product()
			{
				ProductType = ProductType.SimpleProduct,
				VisibleIndividually = true,
				Sku = "Sony-PS410034",
                //Sku = "PS410037",

                Name = "PlayStation 4",
				ShortDescription = "The best place to play. Working with some of the most creative minds in the industry, PlayStation®4 delivers breathtaking and unique gaming experiences.",
				FullDescription = "<p><h4>The power to perform.</h4><div>PlayStation®4 was designed from the ground up to ensure that game creators can unleash their imaginations to develop the very best games and deliver new play experiences never before possible. With ultra-fast customized processors and 8GB of high-performance unified system memory, PS4™ is the home to games with rich, high-fidelity graphics and deeply immersive experiences that shatter expectations.</div></p><p><ul><li><h4>Processor</h4><ul><li>Processor Technology : Low power x86-64 AMD 'Jaguar', 8 cores</li></ul></li><li><h4>Software</h4><ul><li>Processor : Single-chip custom processor</li></ul></li><li><h4>Display</h4><ul><li>Display Technology : HDMI<br />Digital Output (optical)</li></ul></li><li><h4>General</h4><ul><li>Ethernet ports x speed : Ethernet (10BASE-T, 100BASE-TX, 1000BASE-T); IEEE 802.11 b/g/n; Bluetooth® 2.1 (EDR)</li><li>Hard disk : Built-in</li></ul></li><li><h4>General Specifications</h4><ul><li>Video : BD 6xCAV<br />DVD 8xCAV</li></ul></li><li><h4>Graphics</h4><ul><li>Graphics Processor : 1.84 TFLOPS, AMD Radeon™ Graphics Core Next engine</li></ul></li><li><h4>Interface</h4><ul><li>I/O Port : Super-Speed USB (USB 3.0), AUX</li></ul></li><li><h4>Memory</h4><ul><li>Internal Memory : GDDR5 8GB</li></ul></li></ul></p>",
				ProductTemplateId = productTemplate.Id,
				AllowCustomerReviews = true,
				Published = true,
				MetaTitle = "PlayStation 4",
				Price = 399.99M,
				ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
				OrderMinimumQuantity = 1,
				OrderMaximumQuantity = 3,
				StockQuantity = 10000,
				NotifyAdminForQuantityBelow = 1,
				AllowBackInStockSubscriptions = false,
				IsShipEnabled = true,
				DeliveryTime = firstDeliveryTime
			};

			productPs4.ProductManufacturers.Add(new ProductManufacturer() { Manufacturer = manuSony, DisplayOrder = 1 });
			productPs4.ProductCategories.Add(new ProductCategory() { Category = categoryGaming, DisplayOrder = 5 });

			productPs4.ProductPictures.Add(new ProductPicture()
			{
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_sony_ps4.png"), "image/png", GetSeName(productPs4.Name)),
				DisplayOrder = 1
			});
            productPs4.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_sony_dualshock4_wirelesscontroller.png"), "image/png", GetSeName(productPs4.Name)),
                DisplayOrder = 1
            });


            //var productDualshock4Controller = new Product()
            //{
            //	ProductType = ProductType.SimpleProduct,
            //	VisibleIndividually = true,
            //	Sku = "Sony-PS410037",
            //	Name = "DUALSHOCK 4 Wireless Controller",
            //	ShortDescription = "Combining classic controls with innovative new ways to play, the DUALSHOCK®4 wireless controller is an evolutionary controller for a new era of gaming.",
            //	FullDescription = "<p>Keys / Switches : PS button, SHARE button, OPTIONS button, Directional buttons (Up/Down/Left/Right), Action buttons (Triangle, Circle, Cross, Square), R1/L1/R2/L2/R3/L3, Right stick, Left stick, Touch Pad Button. The DualShock 4 is currently available in Jet Black, Magma Red, and Wave Blue.</p><p>The DualShock 4 features the following buttons: PS button, SHARE button, OPTIONS button, directional buttons, action buttons (triangle, circle, cross, square), shoulder buttons (R1/L1), triggers (R2/L2), analog stick click buttons (L3/R3) and a touch pad click button.[25] These mark several changes from the DualShock 3 and other previous PlayStation controllers. The START and SELECT buttons have been merged into a single OPTIONS button.[25][27] A dedicated SHARE button will allow players to upload video from their gameplay experiences.[25] The joysticks and triggers have been redesigned based on developer input.[25] with the ridged surface of the joysticks now featuring an outer ring surrounding the concave dome caps.</p><p>The DualShock 4 is backward compatible with the PlayStation 3, but only via a microUSB cable. Backward compatibility is not supported via Bluetooth.</p>",
            //	ProductTemplateId = productTemplateSimple.Id,
            //	AllowCustomerReviews = true,
            //	Published = true,
            //	MetaTitle = "DUALSHOCK 4 Wireless Controller",
            //	Price = 59.99M,
            //	ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
            //	OrderMinimumQuantity = 1,
            //	OrderMaximumQuantity = 10,
            //	StockQuantity = 10000,
            //	NotifyAdminForQuantityBelow = 1,
            //	AllowBackInStockSubscriptions = false,
            //	IsShipEnabled = true,
            //	DeliveryTime = firstDeliveryTime
            //};

            //productDualshock4Controller.ProductManufacturers.Add(new ProductManufacturer() { Manufacturer = manuSony, DisplayOrder = 1 });
            //productDualshock4Controller.ProductCategories.Add(new ProductCategory() { Category = categoryGamingAccessories, DisplayOrder = 2 });

            //productDualshock4Controller.ProductPictures.Add(new ProductPicture()
            //{
            //             Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_sony_dualshock4_wirelesscontroller.png"), "image/png", GetSeName(productDualshock4Controller.Name)),
            //	DisplayOrder = 1
            //});


            var productPs4Camera = new Product()
			{
				ProductType = ProductType.SimpleProduct,
				VisibleIndividually = true,
				Sku = "Sony-PS410040",
				Name = "PlayStation 4 Camera",
				ShortDescription = "Play, challenge and share your epic gaming moments with PlayStation®Camera and your PS4™. Multiplayer is enhanced through immediate, crystal clear audio and picture-in-picture video sharing.",
				FullDescription = "<ul><li>When combined with the DualShock 4 Wireless Controller's light bar, the evolutionary 3D depth-sensing technology in the PlayStation Camera allows it to precisely track a player's position in the room.</li><li>From navigational voice commands to facial recognition, the PlayStation Camera adds incredible innovation to your gaming.</li><li>Automatically integrate a picture-in-picture video of yourself during gameplay broadcasts, and challenge your friends during play.</li><li>Never leave a friend hanging or miss a chance to taunt your opponents with voice chat that keeps the conversation going, whether it's between rounds, between games, or just while kicking back.</li></ul>",
				ProductTemplateId = productTemplate.Id,
				AllowCustomerReviews = true,
				Published = true,
				MetaTitle = "PlayStation 4 Camera",
				Price = 59.99M,
				ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
				OrderMinimumQuantity = 1,
				OrderMaximumQuantity = 10,
				StockQuantity = 10000,
				NotifyAdminForQuantityBelow = 1,
				AllowBackInStockSubscriptions = false,
				IsShipEnabled = true,
				DeliveryTime = firstDeliveryTime
			};

			productPs4Camera.ProductManufacturers.Add(new ProductManufacturer() { Manufacturer = manuSony, DisplayOrder = 1 });
			productPs4Camera.ProductCategories.Add(new ProductCategory() { Category = categoryGamingAccessories, DisplayOrder = 3 });

			productPs4Camera.ProductPictures.Add(new ProductPicture()
			{
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_sony_ps4_camera.png"), "image/png", GetSeName(productPs4Camera.Name)),
				DisplayOrder = 1
			});


			var productBundlePs4 = new Product()
			{
				ProductType = ProductType.BundledProduct,
				VisibleIndividually = true,
				Sku = "Sony-PS410099",
				Name = "PlayStation 4 Bundle",
				ShortDescription = "PlayStation®4 system, DUALSHOCK®4 wireless controller and PS4 camera.",
				FullDescription =
					"<p><h4>The best place to play</h4><div>PlayStation 4 is the best place to play with dynamic, connected gaming, powerful graphics and speed, intelligent personalization, deeply integrated social capabilities, and innovative second-screen features. Combining unparalleled content, immersive gaming experiences, all of your favorite digital entertainment apps, and PlayStation exclusives, PS4 centers on gamers, enabling them to play when, where and how they want. PS4 enables the greatest game developers in the world to unlock their creativity and push the boundaries of play through a system that is tuned specifically to their needs.</div></p><p><h4>Gamer focused, developer inspired</h4><div>The PS4 system focuses on the gamer, ensuring that the very best games and the most immersive experiences are possible on the platform. The PS4 system enables the greatest game developers in the world to unlock their creativity and push the boundaries of play through a system that is tuned specifically to their needs. The PS4 system is centered around a powerful custom chip that contains eight x86-64 cores and a state of the art 1.84 TFLOPS graphics processor with 8 GB of ultra-fast GDDR5 unified system memory, easing game creation and increasing the richness of content achievable on the platform. The end result is new games with rich, high-fidelity graphics and deeply immersive experiences.</div></p><p><h4>Personalized, curated content</h4><div>The PS4 system has the ability to learn about your preferences. It will learn your likes and dislikes, allowing you to discover content pre-loaded and ready to go on your console in your favorite game genres or by your favorite creators. Players also can look over game-related information shared by friends, view friends’ gameplay with ease, or obtain information about recommended content, including games, TV shows and movies.</div></p><p><h4>New DUALSHOCK controller</h4><div>DUALSHOCK 4 features new innovations to deliver more immersive gaming experiences, including a highly sensitive six-axis sensor as well as a touch pad located on the top of the controller, which offers gamers completely new ways to play and interact with games.</div></p><p><h4>Shared game experiences</h4><div>Engage in endless personal challenges with your community and share your epic triumphs with the press of a button. Simply hit the SHARE button on the controller, scan through the last few minutes of gameplay, tag it and return to the game—the video uploads as you play. The PS4 system also enhances social spectating by enabling you to broadcast your gameplay in real-time.</div></p><p><h4>Remote play</h4><div>Remote Play on the PS4 system fully unlocks the PlayStation Vita system’s potential, making it the ultimate companion device. With the PS Vita system, gamers will be able to seamlessly play a range of PS4 titles on the beautiful 5-inch display over Wi-Fi access points in a local area network.</div></p><p><h4>PlayStation app</h4><div>The PlayStation App will enable iPhone, iPad, and Android-based smartphones and tablets to become second screens for the PS4 system. Once installed on these devices, players can view in-game items, purchase PS4 games and download them directly to the console at home, or remotely watch the gameplay of other gamers playing on their devices.</div></p><p><h4>PlayStation Plus</h4><div>Built to bring games and gamers together and fuel the next generation of gaming, PlayStation Plus helps you discover a world of exceptional gaming experiences. PlayStation Plus is a membership service that takes your gaming experience to the next level. Each month members receive an Instant Game Collection of top rated blockbuster and innovative Indie games, which they can download direct to their console.</div></p>",
				ProductTemplateId = productTemplate.Id,
				AllowCustomerReviews = true,
				Published = true,
				MetaTitle = "PlayStation 4 Bundle",
				Price = 429.99M,
				OldPrice = 449.99M,
				ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
				OrderMinimumQuantity = 1,
				OrderMaximumQuantity = 10000,
				StockQuantity = 10000,
				NotifyAdminForQuantityBelow = 1,
				AllowBackInStockSubscriptions = false,
				IsShipEnabled = true,
				DeliveryTime = firstDeliveryTime,
				BundleTitleText = "Bundle includes"
			};

			productBundlePs4.ProductManufacturers.Add(new ProductManufacturer() { Manufacturer = manuSony, DisplayOrder = 1 });
			productBundlePs4.ProductCategories.Add(new ProductCategory() { Category = categoryGaming, DisplayOrder = 2 });

			productBundlePs4.ProductPictures.Add(new ProductPicture()
			{
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_sony_ps4_bundle.png"), "image/png", GetSeName(productBundlePs4.Name)),
				DisplayOrder = 1
			});

			#endregion bundlePs4

			#region groupAccessories

			var productGroupAccessories = new Product()
			{
				ProductType = ProductType.GroupedProduct,
				VisibleIndividually = true,
				Sku = "Sony-GroupAccessories",
				Name = "Accessories for unlimited gaming experience",
				ShortDescription = "The future of gaming is now with dynamic, connected gaming, powerful graphics and speed, intelligent personalization, deeply integrated social capabilities, and innovative second-screen features. The brilliant culmination of the most creative minds in the industry, PlayStation®4 delivers a unique gaming environment that will take your breath away.",
				FullDescription = "<ul><li>Immerse yourself in a new world of gameplay with powerful graphics and speed.</li><li>Eliminate lengthy load times of saved games with Suspend mode.</li><li>Immediately play digital titles without waiting for them to finish downloading thanks to background downloading and updating capability.</li><li>Instantly share images and videos of your favorite gameplay moments on Facebook with the SHARE button on the DUALSHOCK®4 controller.</li><li>Broadcast while you play in real-time through Ustream.</li></ul>",
				ProductTemplateId = productTemplate.Id,
				AllowCustomerReviews = true,
				Published = true,
				MetaTitle = "Accessories for unlimited gaming experience",
				Price = 0.0M,
				ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
				OrderMinimumQuantity = 1,
				OrderMaximumQuantity = 3,
				StockQuantity = 10000,
				NotifyAdminForQuantityBelow = 1,
				AllowBackInStockSubscriptions = false,
				IsShipEnabled = true,
				ShowOnHomePage = true
			};

			productGroupAccessories.ProductManufacturers.Add(new ProductManufacturer() { Manufacturer = manuSony, DisplayOrder = 1 });
			productGroupAccessories.ProductCategories.Add(new ProductCategory() { Category = categoryGaming, DisplayOrder = 3 });

			productGroupAccessories.ProductPictures.Add(new ProductPicture()
			{
				Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "category_gaming_accessories.png"), "image/png", GetSeName(productGroupAccessories.Name)),
				DisplayOrder = 1
			});

			#endregion groupAccessories

			#region Ps3PlusOneGame

			//var productWatchDogs = new Product()
			//{
			//	ProductType = ProductType.SimpleProduct,
			//	VisibleIndividually = true,
			//	Sku = "Ubi-watchdogs",
			//	Name = "Watch Dogs",
			//	ShortDescription = "Hack and control the city – Use the city systems as weapons: traffic lights, security cameras, movable bridges, gas pipes, electricity grid and more.",
			//	FullDescription = "<p>In today's hyper-connected world, Chicago has the country’s most advanced computer system – one which controls almost every piece of city technology and holds key information on all of the city's residents.</p><p>You play as Aiden Pearce, a brilliant hacker but also a former thug, whose criminal past lead to a violent family tragedy. Now on the hunt for those who hurt your family, you'll be able to monitor and hack all who surround you while manipulating the city's systems to stop traffic lights, download personal information, turn off the electrical grid and more.</p><p>Use the city of Chicago as your ultimate weapon and exact your own style of revenge.</p><p>Monitor the masses – Everyone leaves a digital shadow - access all data on anyone and use it to your advantage.</p><p>State of the art graphics</p>",
			//	ProductTemplateId = productTemplateSimple.Id,
			//	AllowCustomerReviews = true,
			//	Published = true,
			//	MetaTitle = "Watch Dogs",
			//	Price = 49.90M,
			//	ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
			//	OrderMinimumQuantity = 1,
			//	OrderMaximumQuantity = 10000,
			//	StockQuantity = 10000,
			//	NotifyAdminForQuantityBelow = 1,
			//	AllowBackInStockSubscriptions = false,
			//	IsShipEnabled = true,
			//	DeliveryTime = firstDeliveryTime
			//};

			//productWatchDogs.ProductManufacturers.Add(new ProductManufacturer() { Manufacturer = manuUbisoft, DisplayOrder = 1 });
			//productWatchDogs.ProductCategories.Add(new ProductCategory() { Category = categoryGamingGames, DisplayOrder = 1 });

			//productWatchDogs.ProductPictures.Add(new ProductPicture()
			//{
			//	Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "ubisoft-watchdogs.jpg"), "image/jpeg", GetSeName(productWatchDogs.Name)),
			//	DisplayOrder = 1
			//});

			var productPrinceOfPersia = new Product()
			{
				ProductType = ProductType.SimpleProduct,
				VisibleIndividually = true,
				Sku = "Ubi-princepersia",
				Name = "Prince of Persia \"The Forgotten Sands\"",
				ShortDescription = "Play the epic story of the heroic Prince as he fights and outwits his enemies in order to save his kingdom.",
				FullDescription = "<p>This game marks the return to the Prince of Persia® Sands of Time storyline. Prince of Persia: The Forgotten Sands™ will feature many of the fan-favorite elements from the original series as well as new gameplay innovations that gamers have come to expect from Prince of Persia.</p><p>Experience the story, setting, and gameplay in this return to the Sands of Time universe as we follow the original Prince of Persia through a new untold chapter.</p><p>Created by Ubisoft Montreal, the team that brought you various Prince of Persia® and Assassin’s Creed® games, Prince of Persia The Forgotten Sands™ has been over 2 years in the making.</p>",
				ProductTemplateId = productTemplate.Id,
				AllowCustomerReviews = true,
				Published = true,
				MetaTitle = "Prince of Persia",
				Price = 39.90M,
				ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
				OrderMinimumQuantity = 1,
				OrderMaximumQuantity = 10000,
				StockQuantity = 10000,
				NotifyAdminForQuantityBelow = 1,
				AllowBackInStockSubscriptions = false,
				IsShipEnabled = true,
				DeliveryTime = firstDeliveryTime
			};

			productPrinceOfPersia.ProductManufacturers.Add(new ProductManufacturer() { Manufacturer = manuUbisoft, DisplayOrder = 1 });
			productPrinceOfPersia.ProductCategories.Add(new ProductCategory() { Category = categoryGamingGames, DisplayOrder = 2 });

			productPrinceOfPersia.ProductPictures.Add(new ProductPicture()
			{
				Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "products_princeofpersia.jpg"), "image/jpeg", GetSeName(productPrinceOfPersia.Name)),
				DisplayOrder = 1
			});
            #endregion Ps3PlusOneGame

            #region Horizon Zero Down
            var productHorizonZeroDown = new Product()
            {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                //Sku = "Ubi-princepersia",
                Sku = "PD-ZeroDown4PS4",
                Name = "Horizon Zero Dawn - PlayStation 4",
                ShortDescription = "Experience A Vibrant, Lush World Inhabited By Mysterious Mechanized Creatures",
                FullDescription = "<ul>  <li>A Lush Post-Apocalyptic World – How have machines dominated this world, and what is their purpose? What happened to the civilization here before? Scour every corner of a realm filled with ancient relics and mysterious buildings in order to uncover your past and unearth the many secrets of a forgotten land.</li>  <li></li>  <li>Nature and Machines Collide – Horizon Zero Dawn juxtaposes two contrasting elements, taking a vibrant world rich with beautiful nature and filling it with awe-inspiring highly advanced technology. This marriage creates a dynamic combination for both exploration and gameplay.</li>  <li>Defy Overwhelming Odds – The foundation of combat in Horizon Zero Dawn is built upon the speed and cunning of Aloy versus the raw strength and size of the machines. In order to overcome a much larger and technologically superior enemy, Aloy must use every ounce of her knowledge, intelligence, and agility to survive each encounter.</li>  <li>Cutting Edge Open World Tech – Stunningly detailed forests, imposing mountains, and atmospheric ruins of a bygone civilization meld together in a landscape that is alive with changing weather systems and a full day/night cycle.</li></ul>",
                ProductTemplateId = productTemplate.Id,
                AllowCustomerReviews = true,
                Published = true,
                MetaTitle = "Horizon Zero Dawn - PlayStation 4",
                Price = 69.90M,
                OldPrice = 79.90M,
                ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                IsShipEnabled = true,
                DeliveryTime = firstDeliveryTime
            };

            productHorizonZeroDown.ProductManufacturers.Add(new ProductManufacturer() { Manufacturer = manuSony, DisplayOrder = 1 });
            productHorizonZeroDown.ProductCategories.Add(new ProductCategory() { Category = categoryGamingGames, DisplayOrder = 2 });

            productHorizonZeroDown.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_horizon.jpg"), "image/jpeg", GetSeName(productHorizonZeroDown.Name)),
                DisplayOrder = 1
            });
            #endregion Horizon Zero Down

            #region Fifa 17
            var productFifa17 = new Product()
            {
                ProductType = ProductType.SimpleProduct,
                VisibleIndividually = true,
                //Sku = "Ubi-princepersia",
                Sku = "PD-Fifa17",
                Name = "FIFA 17 - PlayStation 4",
                ShortDescription = "Powered by Frostbite",
                FullDescription = "<ul>  <li>Powered by Frostbite: One of the industry’s leading game engines, Frostbite delivers authentic, true-to-life action, takes players to new football worlds, and introduces fans to characters full of depth and emotion in FIFA 17.</li>  <li>The Journey: For the first time ever in FIFA, live your story on and off the pitch as the Premier League’s next rising star, Alex Hunter. Play on any club in the premier league, for authentic managers and alongside some of the best players on the planet. Experience brand new worlds in FIFA 17, all while navigating your way through the emotional highs and lows of The Journey.</li>  <li>Own Every Moment: Complete innovation in the way players think and move, physically interact with opponents, and execute in attack puts you in complete control of every moment on the pitch.</li></ul>",
                ProductTemplateId = productTemplate.Id,
                AllowCustomerReviews = true,
                Published = true,
                MetaTitle = "FIFA 17 - PlayStation 4",
                Price = 79.90M,
                OldPrice = 89.90M,
                ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 10000,
                StockQuantity = 10000,
                NotifyAdminForQuantityBelow = 1,
                AllowBackInStockSubscriptions = false,
                IsShipEnabled = true,
                DeliveryTime = firstDeliveryTime
            };

            productFifa17.ProductManufacturers.Add(new ProductManufacturer() { Manufacturer = manuEASports, DisplayOrder = 1 });
            productFifa17.ProductCategories.Add(new ProductCategory() { Category = categoryGamingGames, DisplayOrder = 2 });

            productFifa17.ProductPictures.Add(new ProductPicture()
            {
                Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_fifa17.jpg"), "image/jpeg", GetSeName(productFifa17.Name)),
                DisplayOrder = 1
            });
            #endregion Fifa 17

            #region Lego Worlds
            //productDriverSanFrancisco
            var productLegoWorlds = new Product()
			{
				ProductType = ProductType.SimpleProduct,
				VisibleIndividually = true,
				//Sku = "Ubi-driversanfrancisco",
                Sku = "Gaming-Lego-001",
                Name = "LEGO Worlds - PlayStation 4",
				ShortDescription = "Experience a galaxy of Worlds made entirely from LEGO bricks.",
				FullDescription = "<ul>  <li>Experience a galaxy of Worlds made entirely from LEGO bricks.</li>  <li>LEGO Worlds is an open environment of procedurally-generated Worlds made entirely of LEGO bricks which you can freely manipulate and dynamically populate with LEGO models.</li>  <li>Create anything you can imagine one brick at a time, or use large-scale landscaping tools to create vast mountain ranges and dot your world with tropical islands.</li>  <li>Explore using helicopters, dragons, motorbikes or even gorillas and unlock treasures that enhance your gameplay.</li>  <li>Watch your creations come to life through characters and creatures that interact with you and each other in unexpected ways.</li></ul><p></p>",
				ProductTemplateId = productTemplate.Id,
				AllowCustomerReviews = true,
				Published = true,
				MetaTitle = "LEGO Worlds - PlayStation 4",
				Price = 29.90M,
                OldPrice = 34.90M,
				ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
				OrderMinimumQuantity = 1,
				OrderMaximumQuantity = 10000,
				StockQuantity = 10000,
				NotifyAdminForQuantityBelow = 1,
				AllowBackInStockSubscriptions = false,
				IsShipEnabled = true,
				DeliveryTime = firstDeliveryTime
			};

            productLegoWorlds.ProductManufacturers.Add(new ProductManufacturer() { Manufacturer = manuWarnerHomme, DisplayOrder = 1 });
            productLegoWorlds.ProductCategories.Add(new ProductCategory() { Category = categoryGamingGames, DisplayOrder = 3 });

            productLegoWorlds.ProductPictures.Add(new ProductPicture()
			{
				Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_legoworlds.jpg"), "image/jpeg", GetSeName(productLegoWorlds.Name)),
				DisplayOrder = 1
			});
            #endregion Lego Worlds

            #region Ps3PlusOneGame
            //var productPs3OneGame = new Product()
            //{
            //    ProductType = ProductType.SimpleProduct,
            //    VisibleIndividually = true,
            //    Sku = "Sony-PS310111",
            //    Name = "PlayStation 3 plus game cheaper",
            //    ShortDescription = "Our special offer: PlayStation 3 plus one game of your choise cheaper.",
            //    FullDescription = productPs3.FullDescription,
            //    ProductTemplateId = productTemplate.Id,
            //    AllowCustomerReviews = true,
            //    Published = true,
            //    MetaTitle = "PlayStation 3 plus game cheaper",
            //    Price = 160.00M,
            //    ManageInventoryMethod = ManageInventoryMethod.DontManageStock,
            //    OrderMinimumQuantity = 1,
            //    OrderMaximumQuantity = 3,
            //    StockQuantity = 10000,
            //    NotifyAdminForQuantityBelow = 1,
            //    AllowBackInStockSubscriptions = false,
            //    IsShipEnabled = true,
            //    DeliveryTime = firstDeliveryTime
            //};

            //productPs3OneGame.ProductManufacturers.Add(new ProductManufacturer() { Manufacturer = manuSony, DisplayOrder = 1 });
            //productPs3OneGame.ProductCategories.Add(new ProductCategory() { Category = categoryGaming, DisplayOrder = 6 });

            //productPs3OneGame.ProductPictures.Add(new ProductPicture()
            //{
            //    Picture = CreatePicture(File.ReadAllBytes(sampleImagesPath + "product_sony_ps3_plus_game.png"), "image/png", GetSeName(productPs3OneGame.Name)),
            //    DisplayOrder = 1
            //});

            #endregion Ps3PlusOneGame


            #endregion gaming

            var entities = new List<Product>
			{
                productTRANSOCEANCHRONOGRAPH,productTissotTTouchExpertSolar,productSeikoSRPA49K1,productTitleistSM6TourChrome,productTitleistProV1x,productGBBEpicSubZeroDriver,productSupremeGolfball,productBooksStoneOfTheWise,productNikeStrikeFootball,productNikeEvoPowerBall,
                productTorfabrikOfficialGameBall,productAdidasTangoSalaBall,productAllCourtBasketball,productEvolutionHighSchoolGameBasketball,productRayBanTopBar,
                productOriginalWayfarer,productCustomFlakSunglasses,productRadarEVPrizmSportsSunglasses,productAppleProHipsterBundle,product97ipad,productAirpods,
                productIphoneplus,productWatchSeries2,product10GiftCard, product25GiftCard, product50GiftCard,product100GiftCard, productBooksUberMan, productBooksGefangeneDesHimmels,
				productBooksBestGrillingRecipes, productBooksCookingForTwo, productBooksAutosDerSuperlative,  productBooksBildatlasMotorraeder, productBooksAutoBuch, productBooksFastCars,
				productBooksMotorradAbenteuer,  
				productInstantDownloadVivaldi,  productInstantDownloadBeethoven, productWatchesCertinaDSPodiumBigSize,
				productPs3, productMinecraft, productBundlePs3AssassinCreed,
				productPs4, productDualshock4Controller, productPs4Camera, productBundlePs4,
				productGroupAccessories,
				productPrinceOfPersia, productLegoWorlds,productHorizonZeroDown,productFifa17
            };

            entities.AddRange(GetFashionProducts());
			entities.AddRange(GetFurnitureProducts());

			this.Alter(entities);
			return entities;
		}

		public IList<ProductBundleItem> ProductBundleItems()
		{
            var utcNow = DateTime.UtcNow;

            #region apple bundles
            var bundleAppleProHipster = _ctx.Set<Product>().First(x => x.Sku == "P-2005-Bundle");

            var bundleItemIproductIphoneplus = new ProductBundleItem()
            {
                BundleProduct = bundleAppleProHipster,
                Product = _ctx.Set<Product>().First(x => x.Sku == "P-2001"),
                Quantity = 1,
                Discount = 40.0M,
                Visible = true,
                Published = true,
                DisplayOrder = 1
            };

            var bundleItemProductWatchSeries2 = new ProductBundleItem()
            {
                BundleProduct = bundleAppleProHipster,
                Product = _ctx.Set<Product>().First(x => x.Sku == "P-2002"),
                Quantity = 2,
                Discount = 30.0M,
                Visible = true,
                Published = true,
                DisplayOrder = 2
            };

            var bundleItemproductAirpods = new ProductBundleItem()
            {
                BundleProduct = bundleAppleProHipster,
                Product = _ctx.Set<Product>().First(x => x.Sku == "P-2003"),
                Quantity = 1,
                Discount = 30.0M,
                Visible = true,
                Published = true,
                DisplayOrder = 3
            };

            var bundleItemproductIpad = new ProductBundleItem()
            {
                BundleProduct = bundleAppleProHipster,
                Product = _ctx.Set<Product>().First(x => x.Sku == "P-2004"),
                Quantity = 1,
                Discount = 30.0M,
                Visible = true,
                Published = true,
                DisplayOrder = 3
            };

            #endregion apple bundles

            #region gaming

            //var bundlePs3AssassinCreed = _ctx.Set<Product>().First(x => x.Sku == "Sony-PS399105");
            var bundlePs4Minecraft = _ctx.Set<Product>().First(x => x.Sku == "Sony-PS399105");

            var bundleItemPs4Minecraft1 = new ProductBundleItem()
			{
				BundleProduct = bundlePs4Minecraft,
				Product = _ctx.Set<Product>().First(x => x.Sku == "Sony-PS399000"),
				Quantity = 1,
				Discount = 20.0M,
				Visible = true,
				Published = true,
				DisplayOrder = 1
			};

			var bundleItemPs4Minecraft2 = new ProductBundleItem()
			{
				BundleProduct = bundlePs4Minecraft,
				Product = _ctx.Set<Product>().First(x => x.Sku == "Sony-PS399004"),
				Quantity = 2,
				Discount = 30.0M,
				Visible = true,
				Published = true,
				DisplayOrder = 2
			};

			var bundleItemPs4Minecraft3 = new ProductBundleItem()
			{
				BundleProduct = bundlePs4Minecraft,
				Product = _ctx.Set<Product>().First(x => x.Sku == "PD-Minecraft4ps4"),
				Quantity = 1,
				Discount = 20.0M,
				Visible = true,
				Published = true,
				DisplayOrder = 3
			};


			var bundlePs4 = _ctx.Set<Product>().First(x => x.Sku == "Sony-PS410099");

			var bundleItemPs41 = new ProductBundleItem
			{
				BundleProduct = bundlePs4,
				Product = _ctx.Set<Product>().First(x => x.Sku == "Sony-PS410034"),
				Quantity = 1,
				Visible = true,
				Published = true,
				DisplayOrder = 1
			};

			var bundleItemPs42 = new ProductBundleItem
			{
				BundleProduct = bundlePs4,
				//Product = _ctx.Set<Product>().First(x => x.Sku == "Sony-PS410037"),
                Product = _ctx.Set<Product>().First(x => x.Sku == "Sony-PS399004"),
                Quantity = 1,
				Visible = true,
				Published = true,
				DisplayOrder = 2
			};

			var bundleItemPs43 = new ProductBundleItem
			{
				BundleProduct = bundlePs4,
				Product = _ctx.Set<Product>().First(x => x.Sku == "Sony-PS410040"),
				Quantity = 1,
				Visible = true,
				Published = true,
				DisplayOrder = 3
			};

			#endregion gaming

			var entities = new List<ProductBundleItem>
			{
                bundleItemPs4Minecraft1, bundleItemPs4Minecraft2, bundleItemPs4Minecraft3,
				bundleItemPs41, bundleItemPs42, bundleItemPs43,bundleItemIproductIphoneplus, bundleItemProductWatchSeries2,bundleItemproductAirpods,bundleItemproductIpad
            };

			this.Alter(entities);
			return entities;
		}

		public void AssignGroupedProducts(IList<Product> savedProducts)
		{
			int productGamingAccessoriesId = savedProducts.First(x => x.Sku == "Sony-GroupAccessories").Id;
			var gamingAccessoriesSkus = new List<string>() { "Sony-PS399004", "PD-Minecraft4ps4", "Sony-PS410037", "Sony-PS410040" };

			savedProducts
				.Where(x => gamingAccessoriesSkus.Contains(x.Sku))
				.ToList()
				.Each(x =>
				{
					x.ParentGroupedProductId = productGamingAccessoriesId;

					//_ctx.Set<Product>().Attach(x);
					//_ctx.Entry(x).State = System.Data.Entity.EntityState.Modified;
				});

			_ctx.SaveChanges();			
		}

		#region ForumGroups
		public IList<ForumGroup> ForumGroups()
		{
			var forumGroupGeneral = new ForumGroup
			{
				Name = "General",
				Description = "",
				DisplayOrder = 1
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
			var newProductsForum = new Forum
			{
				ForumGroup = _ctx.Set<ForumGroup>().Where(c => c.DisplayOrder == 1).Single(),
				Name = "New Products",
				Description = "Discuss new products and industry trends",
				NumTopics = 0,
				NumPosts = 0,
				LastPostCustomerId = 0,
				LastPostTime = null,
				DisplayOrder = 1
			};

			var packagingShippingForum = new Forum
			{
				ForumGroup = _ctx.Set<ForumGroup>().Where(c => c.DisplayOrder == 1).Single(),
				Name = "Packaging & Shipping",
				Description = "Discuss packaging & shipping",
				NumTopics = 0,
				NumPosts = 0,
				LastPostTime = null,
				DisplayOrder = 20
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
					CouponCode = "123"
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
					CouponCode = "456"
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

        #region QuantityUnits

        public IList<QuantityUnit> QuantityUnits()
        {
            var entities = new List<QuantityUnit>()
			{
				new QuantityUnit
					{
						Name = "Piece",        
                        Description = "Piece",
                        IsDefault = true,
						DisplayOrder = 0,
					},
				new QuantityUnit
					{
						Name = "Box",           
                        Description = "Box",
						DisplayOrder = 1,
					},
				new QuantityUnit
					{
						Name = "Parcel",        
                        Description = "Parcel",
						DisplayOrder = 2,
					},
                new QuantityUnit
					{
						Name = "Palette",       
                        Description = "Palette",
						DisplayOrder = 3,
					},
			};
            this.Alter(entities);
            return entities;
        }

        #endregion

		#region BlogPost
		public IList<BlogPost> BlogPosts()
		{
			var defaultLanguage = _ctx.Set<Language>().FirstOrDefault();
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
			var defaultLanguage = _ctx.Set<Language>().FirstOrDefault();
			var news1 = new NewsItem()
			{
				AllowComments = true,
				Language = defaultLanguage,
				Title = "SmartStore.NET new release!",
                Short = "SmartStore.NET includes everything you need to begin your e-commerce online store.",
                Full = "<p>SmartStore.NET includes everything you need to begin your e-commerce online store.<br/>  We have thought of everything and it's all included!<br/><br/SmartStore.NET is a fully customizable shop-system. It's stable and highly usable.<br>  From downloads to documentation, www.smartstore.com offers a comprehensive base of information, resources, and support to the SmartStore.NET community.</p>",
				Published = true,
                MetaTitle = "SmartStore.NET new release!",
				CreatedOnUtc = DateTime.Now
			};
			var news2 = new NewsItem()
			{
				AllowComments = true,
				Language = defaultLanguage,
				Title = "New online store is open!",
                Short = "The new SmartStore.NET store is open now!  We are very excited to offer our new range of products. We will be constantly adding to our range so please register on our site, this will enable you to keep up to date with any new products.",
				Full = "<p>Our online store is officially up and running. Stock up for the holiday season!  We have a great selection of items. We will be constantly adding to our range so please register on our site,  this will enable you to keep up to date with any new products.</p><p>  All shipping is worldwide and will leave the same day an order is placed! Happy Shopping and spread the word!!</p>",
				Published = true,
				MetaTitle = "New online store is open!",
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
			var defaultLanguage = _ctx.Set<Language>().FirstOrDefault();
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

        protected virtual void Alter(IList<QuantityUnit> entities)
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

		protected virtual void Alter(IList<Picture> entities)
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

		protected virtual void Alter(IList<ProductAttributeOptionsSet> entities)
		{
		}

		protected virtual void Alter(IList<ProductAttributeOption> entities)
		{
		}

		protected virtual void Alter(IList<ProductVariantAttribute> entities)
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

		protected virtual void Alter(IList<ProductBundleItem> entities)
		{
		}

		protected virtual void Alter(UrlRecord entity)
		{
		}

		#endregion Alterations

		#endregion Sample data creators

		#region Helpers

		protected SmartObjectContext DbContext
		{
			get
			{
				return _ctx;
			}
		}

		protected string SampleImagesPath
		{
			get
			{
				return this._sampleImagesPath;
			}
		}

		protected string SampleDownloadsPath
		{
			get
			{
				return this._sampleDownloadsPath;
			}
		}

		public virtual UrlRecord CreateUrlRecordFor<T>(T entity) where T : BaseEntity, ISlugSupported, new()
		{
			var name = "";
			var languageId = 0;

			switch (entity)
			{
				case Category x:
					name = x.Name;
					break;
				case Manufacturer x:
					name = x.Name;
					break;
				case Product x:
					name = x.Name;
					break;
				case BlogPost x:
					name = x.Title;
					languageId = x.LanguageId;
					break;
				case NewsItem x:
					name = x.Title;
					languageId = x.LanguageId;
					break;
				case Topic x:
					name = SeoHelper.GetSeName(x.SystemName, true, false).Truncate(400);
					break;
			}

			if (name.HasValue())
			{
				var result = new UrlRecord
				{
					EntityId = entity.Id,
					EntityName = entity.GetUnproxiedType().Name,
					LanguageId = languageId,
					Slug = name,
					IsActive = true
				};

				this.Alter(result);
				return result;
			}
			
			return null;
		}

		protected Picture CreatePicture(byte[] pictureBinary, string mimeType, string seoFilename)
		{
			mimeType = mimeType.EmptyNull().Truncate(20);
			seoFilename = seoFilename.Truncate(100);

			var picture = _ctx.Set<Picture>().Create();
			picture.MimeType = mimeType;
			picture.SeoFilename = seoFilename;
			picture.UpdatedOnUtc = DateTime.UtcNow;
			
			picture.MediaStorage = new MediaStorage
			{
				Data = pictureBinary
			};

			return picture;
		}

		protected string GetSeName(string name)
		{
			return SeoHelper.GetSeName(name, true, false);
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
					currency = new Currency
					{
						DisplayLocale = locale,
						Name = info.CurrencyNativeName,
						CurrencyCode = info.ISOCurrencySymbol,
						Rate = rate,
						CustomFormatting = formatting,
						Published = published,
						DisplayOrder = order
					};
				}
			}
			catch
			{
				return null;
			}

			return currency;
		}

		protected string FormatAttributeXml(int attributeId, int valueId, bool withRootTag = true)
		{
			var xml = $"<ProductVariantAttribute ID=\"{attributeId}\"><ProductVariantAttributeValue><Value>{valueId}</Value></ProductVariantAttributeValue></ProductVariantAttribute>";

			if (withRootTag)
			{
				return string.Concat("<Attributes>", xml, "</Attributes>");
			}

			return xml;
		}
		protected string FormatAttributeXml(int attributeId1, int valueId1, int attributeId2, int valueId2)
		{
			return string.Concat(
				"<Attributes>",
				FormatAttributeXml(attributeId1, valueId1, false),
				FormatAttributeXml(attributeId2, valueId2, false),
				"</Attributes>");
		}
		protected string FormatAttributeXml(int attributeId1, int valueId1, int attributeId2, int valueId2, int attributeId3, int valueId3)
		{
			return string.Concat(
				"<Attributes>",
				FormatAttributeXml(attributeId1, valueId1, false),
				FormatAttributeXml(attributeId2, valueId2, false),
				FormatAttributeXml(attributeId3, valueId3, false),
				"</Attributes>");
		}

		#endregion
	}
}