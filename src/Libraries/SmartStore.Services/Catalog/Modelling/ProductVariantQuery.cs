using System.Collections.Generic;
using System.Linq;
using SmartStore.Collections;

namespace SmartStore.Services.Catalog.Modelling
{
	public class ProductVariantQuery
	{
		private readonly List<ProductVariantQueryItem> _variants;
		private readonly List<GiftCardQueryItem> _giftCards;
		private readonly List<CheckoutAttributeQueryItem> _checkoutAttributes;

		public ProductVariantQuery()
		{
			_variants = new List<ProductVariantQueryItem>();
			_giftCards = new List<GiftCardQueryItem>();
			_checkoutAttributes = new List<CheckoutAttributeQueryItem>();
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

		public IReadOnlyList<CheckoutAttributeQueryItem> CheckoutAttributes
		{
			get
			{
				return _checkoutAttributes;
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

		public void AddCheckoutAttribute(CheckoutAttributeQueryItem item)
		{
			_checkoutAttributes.Add(item);
		}

		public string GetGiftCardValue(int productId, int bundleItemId, string name)
		{
			return _giftCards.FirstOrDefault(x =>
				x.ProductId == productId &&
				x.BundleItemId == bundleItemId &&
				x.Name.IsCaseInsensitiveEqual(name))
				?.Value;
		}

		public string ToQueryString()
		{
			var qs = new QueryString();

			// Checkout Attributes
			foreach (var item in _checkoutAttributes)
			{
				var name = item.ToString();

				if (item.Date.HasValue)
				{
					qs.Add(name + "-date", string.Join("-", item.Date.Value.Year, item.Date.Value.Month, item.Date.Value.Day));
				}
				else
				{
					qs.Add(name, item.Value);
				}
			}

			// Gift cards
			foreach (var item in _giftCards)
			{
				qs.Add(item.ToString(), item.Value);
			}

			// Variants
			foreach (var item in _variants)
			{
				var name = item.Alias.HasValue()
					? $"{item.Alias}-{item.ProductId}-{item.BundleItemId}-{item.VariantAttributeId}"
					: item.ToString();

				if (item.Date.HasValue)
				{
					// TODO: Code never reached because of ParseProductVariantAttributeValues
					qs.Add(name + "-date", string.Join("-", item.Date.Value.Year, item.Date.Value.Month, item.Date.Value.Day));
				}
				else
				{
					var value = item.ValueAlias.HasValue()
						? $"{item.ValueAlias}-{item.Value}"
						: item.Value;

					qs.Add(name, value);
				}
			}

			return qs.ToString(false);
		}

		public override string ToString()
		{
			var groups = new string[]
			{
				string.Join("&", Variants.Select(x => x.ToString())),
				string.Join("&", GiftCards.Select(x => x.ToString())),
				string.Join("&", CheckoutAttributes.Select(x => x.ToString()))
			};

			return string.Join("&", groups.Where(x => x.HasValue()));
		}
	}
}
