using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Mvc;

namespace SmartStore.GoogleMerchantCenter.Models
{
	public class FeedGoogleMerchantCenterModel
	{
		public int GridPageSize { get; set; }

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

		[SmartResourceDisplayName("Plugins.Feed.Froogle.SearchProductName")]
		public string SearchProductName { get; set; }

		[SmartResourceDisplayName("Plugins.Feed.Froogle.SearchIsTouched")]
		public string SearchIsTouched { get; set; }
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