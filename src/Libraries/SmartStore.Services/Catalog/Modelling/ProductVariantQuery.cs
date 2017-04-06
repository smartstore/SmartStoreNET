using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SmartStore.Services.Catalog.Modelling
{
	public class ProductVariantQuery
	{
		private readonly List<ProductVariantQueryItem> _variants;
		private readonly List<GiftCardQueryItem> _giftCards;

		public ProductVariantQuery()
		{
			_variants = new List<ProductVariantQueryItem>();
			_giftCards = new List<GiftCardQueryItem>();
		}

		public IReadOnlyList<ProductVariantQueryItem> Variants
		{
			get
			{
				return _variants;
			}
		}

		public IReadOnlyList<GiftCardQueryItem> GiftCards
		{
			get
			{
				return _giftCards;
			}
		}

		public void AddVariant(ProductVariantQueryItem item)
		{
			_variants.Add(item);
		}

		public void AddGiftCard(GiftCardQueryItem item)
		{
			_giftCards.Add(item);
		}

		public ProductVariantQueryItem GetVariant(int productId, int bundleItemId, int attributeId, int variantAttributeId)
		{
			return _variants.FirstOrDefault(x =>
				x.ProductId == productId &&
				x.BundleItemId == bundleItemId &&
				x.AttributeId == attributeId &&
				x.VariantAttributeId == variantAttributeId);
		}

		public string GetGiftCardValue(int productId, int bundleItemId, string name)
		{
			return _giftCards.FirstOrDefault(x =>
				x.ProductId == productId &&
				x.BundleItemId == bundleItemId &&
				x.Name.IsCaseInsensitiveEqual(name))
				?.Value;
		}

		public override string ToString()
		{
			var groups = new string[]
			{
				string.Join("&", Variants.Select(x => string.Concat(x.ToString(), "=", HttpUtility.UrlEncode(x.Value)))),
				string.Join("&", GiftCards.Select(x => string.Concat(x.ToString(), "=", HttpUtility.UrlEncode(x.Value))))
			};

			return string.Join("&", groups.Where(x => x.HasValue()));
		}
	}
}
