using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Mvc;
using Newtonsoft.Json;
using SmartStore.Web.Framework.Plugins;
using SmartStore.Core.Domain.Catalog;
using FluentValidation.Attributes;
using SmartStore.GoogleMerchantCenter.Validators;

namespace SmartStore.GoogleMerchantCenter.Models
{
	[Validator(typeof(ConfigurationValidator))]
	public class FeedFroogleModel : PromotionFeedConfigModel
	{
		public string GridEditUrl { get; set; }
		public int GridPageSize { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.Froogle.ProductPictureSize")]
		public int ProductPictureSize { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.Froogle.Currency")]
		public int CurrencyId { get; set; }
		public List<SelectListItem> AvailableCurrencies { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.Froogle.DefaultGoogleCategory")]
		public string DefaultGoogleCategory { get; set; }

		public string[] AvailableGoogleCategories { get; set; }
		public string AvailableGoogleCategoriesAsJson
		{
			get
			{
				if (AvailableGoogleCategories != null && AvailableGoogleCategories.Length > 0)
					return JsonConvert.SerializeObject(AvailableGoogleCategories);
				return "";
			}
		}

		[SmartResourceDisplayName("Plugins.Feed.Froogle.TaskEnabled")]
		public bool TaskEnabled { get; set; }
		[SmartResourceDisplayName("Plugins.Feed.Froogle.GenerateStaticFileEachMinutes")]
		public int GenerateStaticFileEachMinutes { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.Froogle.BuildDescription")]
		public string BuildDescription { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.Froogle.AppendDescriptionText")]
		public string AppendDescriptionText1 { get; set; }
		public string AppendDescriptionText2 { get; set; }
		public string AppendDescriptionText3 { get; set; }
		public string AppendDescriptionText4 { get; set; }
		public string AppendDescriptionText5 { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.Froogle.AdditionalImages")]
		public bool AdditionalImages { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.Froogle.Condition")]
		public string Condition { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.Froogle.Availability")]
		public string Availability { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.Froogle.SpecialPrice")]
		public bool SpecialPrice { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.Froogle.Brand")]
		public string Brand { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.Froogle.UseOwnProductNo")]
		public bool UseOwnProductNo { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.Froogle.Gender")]
		public string Gender { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.Froogle.AgeGroup")]
		public string AgeGroup { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.Froogle.Color")]
		public string Color { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.Froogle.Size")]
		public string Size { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.Froogle.Material")]
		public string Material { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.Froogle.Pattern")]
		public string Pattern { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.Froogle.OnlineOnly")]
		public bool OnlineOnly { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.Froogle.DescriptionToPlainText")]
		public bool DescriptionToPlainText { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.Froogle.SearchProductName")]
		public string SearchProductName { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.Froogle.SearchIsTouched")]
		public string SearchIsTouched { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.Froogle.Store")]
		public int StoreId { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.Froogle.ExpirationDays")]
		public int ExpirationDays { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.Froogle.ExportShipping")]
		public bool ExportShipping { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.Froogle.ExportBasePrice")]
		public bool ExportBasePrice { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.Froogle.ConvertNetToGrossPrices")]
		public bool ConvertNetToGrossPrices { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.Froogle.LanguageId")]
		public int LanguageId { get; set; }

		public int ScheduleTaskId { get; set; }

		public void Copy(FroogleSettings settings, bool fromSettings)
		{
			if (fromSettings)
			{
				AppendDescriptionText1 = settings.AppendDescriptionText1;
				AppendDescriptionText2 = settings.AppendDescriptionText2;
				AppendDescriptionText3 = settings.AppendDescriptionText3;
				AppendDescriptionText4 = settings.AppendDescriptionText4;
				AppendDescriptionText5 = settings.AppendDescriptionText5;
				ProductPictureSize = settings.ProductPictureSize;
				CurrencyId = settings.CurrencyId;
				DefaultGoogleCategory = settings.DefaultGoogleCategory;
				BuildDescription = settings.BuildDescription;
				AdditionalImages = settings.AdditionalImages;
				Condition = settings.Condition;
				Availability = settings.Availability;
				SpecialPrice = settings.SpecialPrice;
				Brand = settings.Brand;
				UseOwnProductNo = settings.UseOwnProductNo;
				Gender = settings.Gender;
				AgeGroup = settings.AgeGroup;
				Color = settings.Color;
				Size = settings.Size;
				Material = settings.Material;
				Pattern = settings.Pattern;
				OnlineOnly = settings.OnlineOnly;
				DescriptionToPlainText = settings.DescriptionToPlainText;
				StoreId = settings.StoreId;
				ExpirationDays = settings.ExpirationDays;
				ExportShipping = settings.ExportShipping;
				ExportBasePrice = settings.ExportBasePrice;
				ConvertNetToGrossPrices = settings.ConvertNetToGrossPrices;
				LanguageId = settings.LanguageId;
			}
			else
			{
				settings.AppendDescriptionText1 = AppendDescriptionText1;
				settings.AppendDescriptionText2 = AppendDescriptionText2;
				settings.AppendDescriptionText3 = AppendDescriptionText3;
				settings.AppendDescriptionText4 = AppendDescriptionText4;
				settings.AppendDescriptionText5 = AppendDescriptionText5;
				settings.ProductPictureSize = ProductPictureSize;
				settings.CurrencyId = CurrencyId;
				settings.DefaultGoogleCategory = DefaultGoogleCategory;
				settings.BuildDescription = BuildDescription;
				settings.AdditionalImages = AdditionalImages;
				settings.Condition = Condition;
				settings.Availability = Availability;
				settings.SpecialPrice = SpecialPrice;
				settings.Brand = Brand;
				settings.UseOwnProductNo = UseOwnProductNo;
				settings.Gender = Gender;
				settings.AgeGroup = AgeGroup;
				settings.Color = Color;
				settings.Size = Size;
				settings.Material = Material;
				settings.Pattern = Pattern;
				settings.OnlineOnly = OnlineOnly;
				settings.DescriptionToPlainText = DescriptionToPlainText;
				settings.StoreId = StoreId;
				settings.ExpirationDays = ExpirationDays;
				settings.ExportShipping = ExportShipping;
				settings.ExportBasePrice = ExportBasePrice;
				settings.ConvertNetToGrossPrices = ConvertNetToGrossPrices;
				settings.LanguageId = LanguageId;
			}
		}
	}


	public class GoogleProductModel : ModelBase
	{
		public int TotalCount { get; set; }

		//this attribute is required to disable editing
		[ScaffoldColumn(false)]
		public int ProductId 
		{ 
			get { return Id; }
			set { Id = value; }
		}
		public int Id { get; set; }

		//this attribute is required to disable editing
		[ReadOnly(true)]
		[ScaffoldColumn(false)]
		[SmartResourceDisplayName("Plugins.Feed.Froogle.Products.ProductName")]
		public string Name { get; set; }

		public string SKU { get; set; }
		public int ProductTypeId { get; set; }
		public ProductType ProductType { get { return (ProductType)ProductTypeId; } }
		public string ProductTypeName { get; set; }
		public string ProductTypeLabelHint
		{
			get
			{
				switch (ProductType)
				{
					case ProductType.SimpleProduct:
						return "smnet-hide";
					case ProductType.GroupedProduct:
						return "success";
					case ProductType.BundledProduct:
						return "info";
					default:
						return "";
				}
			}
		}

		[SmartResourceDisplayName("Plugins.Feed.Froogle.Products.GoogleCategory")]
		public string Taxonomy { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.Froogle.Products.Gender")]
		public string Gender { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.Froogle.Products.AgeGroup")]
		public string AgeGroup { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.Froogle.Products.Color")]
		public string Color { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.Froogle.Products.Size")]
		public string Size { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.Froogle.Products.Material")]
		public string Material { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.Froogle.Products.Pattern")]
		public string Pattern { get; set; }

		[SmartResourceDisplayName("Common.Export")]
		public int Export { get; set; }
		[SmartResourceDisplayName("Common.Export")]
		public bool Exporting
		{
			get { return Export != 0; }
			set { Export = (value ? 1 : 0); }
		}

		public string GenderLocalize { get; set; }
		public string AgeGroupLocalize { get; set; }
		public string ExportingLocalize { get; set; }
	}
}