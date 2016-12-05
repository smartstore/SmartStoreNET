using System;
using System.Collections.Generic;
using SmartStore.Web.Models.Media;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Framework.UI;
using SmartStore.Core;
using SmartStore.Core.Domain.Catalog;

namespace SmartStore.Web.Models.Catalog
{
    public partial class ProductSummaryModel : ModelBase
    {
		public ProductSummaryModel(IPagedList<Product> products)
		{
			Guard.NotNull(products, nameof(products));

			Items = new List<SummaryItem>();
			Products = products;
		}

		public IPagedList<Product> Products { get; private set; }

		public int? ThumbSize { get; set; }
		public bool ShowSku { get; set; }
		public bool ShowWeight { get; set; }
		public bool ShowDescription { get; set; }
		public bool ShowBrand { get; set; }
		public bool ShowDimensions { get; set; }
		public bool ShowLegalInfo { get; set; }
		public bool ShowReviews { get; set; }
		public bool ShowDeliveryTimes { get; set; }
		public bool ShowBasePrice { get; set; }
		public bool ShowDiscountBadge { get; set; }
		public bool ShowNewBadge { get; set; }
		public bool BuyEnabled { get; set; }
		public bool WishlistEnabled { get; set; }
		public bool CompareEnabled { get; set; }
		public bool ForceRedirectionAfterAddingToCart { get; set; }

		public ProductSummaryViewMode ViewMode { get; set; }
		public bool AllowPagination { get; set; }
		public IEnumerable<int> AvailablePageSizes { get; set; }
		public bool AllowSorting { get; set; }
		public bool AllowViewModeChanging { get; set; }		

		public IList<SummaryItem> Items { get; set; }

		public class SummaryItem : EntityModelBase
		{
			public SummaryItem(ProductSummaryModel parent)
			{
				Parent = parent;

				Weight = "";
				TransportSurcharge = "";
				Price = new PriceModel();
				Picture = new PictureModel();
				Attributes = new List<Attribute>();
				SpecificationAttributes = new List<ProductSpecificationModel>();
				Badges = new List<Badge>();
			}

			public ProductSummaryModel Parent { get; private set; }

			public string Name { get; set; }
			public string ShortDescription { get; set; }
			public string FullDescription { get; set; }
			public string SeName { get; set; }
			public string Sku { get; set; }
			public string Weight { get; set; }
			public string Dimensions { get; set; }
			public string DimensionMeasureUnit { get; set; }
			public string LegalInfo { get; set; }
			public string TransportSurcharge { get; set; }
			public int RatingSum { get; set; }
			public int TotalReviews { get; set; }
			public bool HideDeliveryTime { get; set; }
			public string DeliveryTimeName { get; set; }
			public string DeliveryTimeHexValue { get; set; }
			public bool IsShippingEnabled { get; set; }
			public bool DisplayDeliveryTimeAccordingToStock { get; set; }
			public string StockAvailablity { get; set; }
			public string BasePriceInfo { get; set; }

			public int MinPriceProductId { get; set; } // Internal

			public ManufacturerOverviewModel Manufacturer { get; set; }
			public PriceModel Price { get; set; }
			public PictureModel Picture { get; set; }
			public IList<Attribute> Attributes { get; set; }
			public IList<ProductSpecificationModel> SpecificationAttributes { get; set; }
			public ColorAttribute ColorAttribute { get; set; }
			public IList<Badge> Badges { get; set; }
		}

		public class PriceModel
		{
			public decimal? RegularPriceValue { get; set; }
			public string RegularPrice { get; set; }

			public decimal PriceValue { get; set; }
			public string Price { get; set; }

			public bool HasDiscount { get; set; }
			public float SavingPercent { get; set; }

			public bool DisableBuyButton { get; set; }
			public bool DisableWishlistButton { get; set; }

			public bool AvailableForPreOrder { get; set; }
			public bool CallForPrice { get; set; }
		}

		public class ColorAttribute
		{
			public ColorAttribute(int id, string name, IEnumerable<ColorAttributeValue> values)
			{
				Id = id;
				Name = name;
				Values = new HashSet<ColorAttributeValue>(values);
			}

			public int Id { get; private set; }
			public string Name { get; private set; }
			public ICollection<ColorAttributeValue> Values { get; private set; }
		}

		public class ColorAttributeValue
		{
			public int Id { get; set; }
			public string Color { get; set; }
			public string Alias { get; set; }
			public string FriendlyName { get; set; }

			public override int GetHashCode()
			{
				return this.Color.GetHashCode();
			}

			public override bool Equals(object obj)
			{
				var equals = base.Equals(obj);
				if (!equals)
				{
					var o2 = obj as ColorAttributeValue;
					if (o2 != null)
					{
						equals = this.Color.IsCaseInsensitiveEqual(o2.Color);
					}
				}
				return equals;
			}
		}

		public class Attribute
		{
			public int Id { get; set; }
			public string Name { get; set; }
			public string Alias { get; set; }
		}

		public class Badge
		{
			public string Label { get; set; }
			public BadgeStyle Style { get; set; }
			public int DisplayOrder { get; set; }
		}
	}

	public enum ProductSummaryViewMode
	{
		Mini,
		Grid,
		List,
		Compare
	}
}