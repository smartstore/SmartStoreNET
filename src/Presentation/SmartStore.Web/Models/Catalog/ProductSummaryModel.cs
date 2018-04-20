using System;
using System.Collections.Generic;
using SmartStore.Core;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Services.Localization;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Framework.UI;
using SmartStore.Web.Models.Media;

namespace SmartStore.Web.Models.Catalog
{
	public partial class ProductSummaryModel : ModelBase, IListActions, IDisposable
    {
		public static ProductSummaryModel Empty = new ProductSummaryModel(new PagedList<Product>(new List<Product>(), 0, int.MaxValue));

		public ProductSummaryModel(IPagedList<Product> products)
		{
			Guard.NotNull(products, nameof(products));

			Items = new List<SummaryItem>();
			AvailableSortOptions = new Dictionary<int, string>();
			AvailablePageSizes = new int[0];
			PagedList = products;
		}

		public int? ThumbSize { get; set; }
		public bool ShowSku { get; set; }
		public bool ShowWeight { get; set; }
		public bool ShowDescription { get; set; }
		public bool ShowFullDescription { get; set; }
		public bool ShowBrand { get; set; }
		public bool ShowDimensions { get; set; }
		public bool ShowLegalInfo { get; set; }
		public bool ShowRatings { get; set; }
		public bool ShowDeliveryTimes { get; set; }
		public bool ShowPrice { get; set; }
		public bool ShowBasePrice { get; set; }
		public bool ShowShippingSurcharge { get; set; }
		public bool ShowButtons { get; set; }
		public bool ShowDiscountBadge { get; set; }
		public bool ShowNewBadge { get; set; }
		public bool BuyEnabled { get; set; }
		public bool WishlistEnabled { get; set; }
		public bool CompareEnabled { get; set; }
		public bool ForceRedirectionAfterAddingToCart { get; set; }

		public IList<SummaryItem> Items { get; set; }

		#region IListActions

		public ProductSummaryViewMode ViewMode { get; set; }
		public GridColumnSpan GridColumnSpan { get; set; }
		public bool BoxedStyleItems { get; set; }

		public bool AllowViewModeChanging { get; set; }
		public bool AllowFiltering { get; set; }
		public bool AllowSorting { get; set; }
		public int? CurrentSortOrder { get; set; }
		public string CurrentSortOrderName { get; set; }
		public string RelevanceSortOrderName { get; set; }
		public IDictionary<int, string> AvailableSortOptions { get; set; }

		public IEnumerable<int> AvailablePageSizes { get; set; }
		public IPageable PagedList { get; set; }

		public void Dispose()
		{
			if (Items != null)
			{
				Items.Clear();
			}
		}

		#endregion

		public class SummaryItem : EntityModelBase
		{
			private readonly WeakReference<ProductSummaryModel> _parent;

			public SummaryItem(ProductSummaryModel parent)
			{
				//Parent = parent;
				_parent = new WeakReference<ProductSummaryModel>(parent);

				Weight = "";
				TransportSurcharge = "";
				Price = new PriceModel();
				Picture = new PictureModel();
				Attributes = new List<Attribute>();
				SpecificationAttributes = new List<ProductSpecificationModel>();
				Badges = new List<Badge>();
			}

			//public ProductSummaryModel Parent { get; private set; }
			public ProductSummaryModel Parent
			{
				get
				{
					ProductSummaryModel parent;
					_parent.TryGetTarget(out parent);
					return parent;
				}
			}

			public LocalizedValue<string> Name { get; set; }
			public LocalizedValue<string> ShortDescription { get; set; }
			public LocalizedValue<string> FullDescription { get; set; }
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
			public LocalizedValue<string> DeliveryTimeName { get; set; }
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
			// TODO: (mc) Let the user specify in attribute manager which spec attributes are
			// important. According to it's importance, show attribute value in grid or list mode.
			// E.g. perfect for "Energy label" > "EEK A++", or special material (e.g. "Leather") etc.
			public IList<ProductSpecificationModel> SpecificationAttributes { get; set; }
			public IList<ColorAttributeValue> ColorAttributes { get; set; }
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
			public string SavingAmount { get; set; }		

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
			public int AttributeId { get; set; }
			public LocalizedValue<string> AttributeName { get; set; }
			public int ProductAttributeId { get; set; }

			public int Id { get; set; }
			public string Color { get; set; }
			public string Alias { get; set; }
			public LocalizedValue<string> FriendlyName { get; set; }
			public string ProductUrl { get; set; }

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
			public LocalizedValue<string> Name { get; set; }
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