using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Collections;
using SmartStore.Core.Domain.Catalog;

namespace SmartStore.Services.Catalog
{
	/// <summary>
	/// Cargo data to reduce database round trips during price calculation
	/// </summary>
	public class PriceCalculationContext
	{
		private List<int> _productIds;
		private List<int> _productIdsTierPrices;

		private Func<int[], Multimap<int, ProductVariantAttribute>> _funcAttributes;
		private Func<int[], Multimap<int, ProductVariantAttributeCombination>> _funcAttributeCombinations;
		private Func<int[], Multimap<int, TierPrice>> _funcTierPrices;

		private LazyMultimap<ProductVariantAttribute> _attributes;
		private LazyMultimap<ProductVariantAttributeCombination> _attributeCombinations;
		private LazyMultimap<TierPrice> _tierPrices;

		public PriceCalculationContext(IEnumerable<Product> products,
			Func<int[], Multimap<int, ProductVariantAttribute>> attributes,
			Func<int[], Multimap<int, ProductVariantAttributeCombination>> attributeCombinations,
			Func<int[], Multimap<int, TierPrice>> tierPrices)
		{
			if (products == null)
			{
				_productIds = new List<int>();
				_productIdsTierPrices = new List<int>();
			}
			else
			{
				_productIds = new List<int>(products.Select(x => x.Id));
				_productIdsTierPrices = new List<int>(products.Where(x => x.HasTierPrices).Select(x => x.Id));
			}

			_funcAttributes = attributes;
			_funcAttributeCombinations = attributeCombinations;
			_funcTierPrices = tierPrices;
		}

		public LazyMultimap<ProductVariantAttribute> Attributes
		{
			get
			{
				if (_attributes == null)
				{
					_attributes = new LazyMultimap<ProductVariantAttribute>(keys => _funcAttributes(keys), _productIds);
				}
				return _attributes;
			}
		}

		public LazyMultimap<ProductVariantAttributeCombination> AttributeCombinations
		{
			get
			{
				if (_attributeCombinations == null)
				{
					_attributeCombinations = new LazyMultimap<ProductVariantAttributeCombination>(keys => _funcAttributeCombinations(keys), _productIds);
				}
				return _attributeCombinations;
			}
		}

		public LazyMultimap<TierPrice> TierPrices
		{
			get
			{
				if (_tierPrices == null)
				{
					_tierPrices = new LazyMultimap<TierPrice>(keys => _funcTierPrices(keys), _productIdsTierPrices);
				}
				return _tierPrices;
			}
		}

		public void Collect(IEnumerable<int> productIds)
		{
			Attributes.Collect(productIds);
			AttributeCombinations.Collect(productIds);
			TierPrices.Collect(productIds);
		}
	}
}
