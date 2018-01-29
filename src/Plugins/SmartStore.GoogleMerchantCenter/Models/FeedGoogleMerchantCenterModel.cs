using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.GoogleMerchantCenter.Models
{
	public class FeedGoogleMerchantCenterModel
	{
		public int GridPageSize { get; set; }
		public string LanguageSeoCode { get; set; }
		public string[] EnergyEfficiencyClasses { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.Froogle.SearchProductName")]
		public string SearchProductName { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.Froogle.SearchIsTouched")]
		public string SearchIsTouched { get; set; }
	}

	public class GoogleProductModel : ModelBase
	{
		public int TotalCount { get; set; }

		// This attribute is required to disable editing.
		[ScaffoldColumn(false)]
		public int ProductId 
		{ 
			get { return Id; }
			set { Id = value; }
		}
		public int Id { get; set; }

		// This attribute is required to disable editing.
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
						return "secondary d-none";
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

		[SmartResourceDisplayName("Common.Export")]
		public int Export { get; set; }
		[SmartResourceDisplayName("Common.Export")]
		public bool Export2
		{
			get { return Export != 0; }
			set { Export = (value ? 1 : 0); }
		}

		[SmartResourceDisplayName("Plugins.Feed.Froogle.Multipack")]
		public int Multipack { get; set; }
		[SmartResourceDisplayName("Plugins.Feed.Froogle.Multipack")]
		public int? Multipack2
		{
			get { return Multipack > 0 ? Multipack : (int?)null; }
			set { Multipack = (value ?? 0); }
		}

		[SmartResourceDisplayName("Plugins.Feed.Froogle.IsBundle")]
		public bool? IsBundle { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.Froogle.IsAdult")]
		public bool? IsAdult { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.Froogle.EnergyEfficiencyClass")]
		public string EnergyEfficiencyClass { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.Froogle.CustomLabel0")]
		public string CustomLabel0 { get; set; }
		[SmartResourceDisplayName("Plugins.Feed.Froogle.CustomLabel1")]
		public string CustomLabel1 { get; set; }
		[SmartResourceDisplayName("Plugins.Feed.Froogle.CustomLabel2")]
		public string CustomLabel2 { get; set; }
		[SmartResourceDisplayName("Plugins.Feed.Froogle.CustomLabel3")]
		public string CustomLabel3 { get; set; }
		[SmartResourceDisplayName("Plugins.Feed.Froogle.CustomLabel4")]
		public string CustomLabel4 { get; set; }

		public string GenderLocalize { get; set; }
		public string AgeGroupLocalize { get; set; }
		public string Export2Localize { get; set; }
		public string IsBundleLocalize { get; set; }
		public string IsAdultLocalize { get; set; }
	}
}