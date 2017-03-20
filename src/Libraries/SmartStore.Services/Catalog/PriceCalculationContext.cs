using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Collections;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Discounts;

namespace SmartStore.Services.Catalog
{
	/// <summary>
	/// Cargo data to reduce database round trips during price calculation
	/// </summary>
	public class PriceCalculationContext
	{
		protected List<int> _productIds;
		private List<int> _productIdsTierPrices;
		private List<int> _productIdsAppliedDiscounts;
		private List<int> _productIdsBundleItems;

		private Func<int[], Multimap<int, ProductVariantAttribute>> _funcAttributes;
		private Func<int[], Multimap<int, ProductVariantAttributeCombination>> _funcAttributeCombinations;
		private Func<int[], Multimap<int, TierPrice>> _funcTierPrices;
		private Func<int[], Multimap<int, ProductCategory>> _funcProductCategories;
		private Func<int[], Multimap<int, ProductManufacturer>> _funcProductManufacturers;
		private Func<int[], Multimap<int, Discount>> _funcAppliedDiscounts;
		private Func<int[], Multimap<int, ProductBundleItem>> _funcProductBundleItems;

		private LazyMultimap<ProductVariantAttribute> _attributes;
		private LazyMultimap<ProductVariantAttributeCombination> _attributeCombinations;
		private LazyMultimap<TierPrice> _tierPrices;
		private LazyMultimap<ProductCategory> _productCategories;
		private LazyMultimap<ProductManufacturer> _productManufacturers;
		private LazyMultimap<Discount> _appliedDiscounts;
		private LazyMultimap<ProductBundleItem> _productBundleItems;

		public PriceCalculationContext(IEnumerable<Product> products,
			Func<int[], Multimap<int, ProductVariantAttribute>> attributes,
			Func<int[], Multimap<int, ProductVariantAttributeCombination>> attributeCombinations,
			Func<int[], Multimap<int, TierPrice>> tierPrices,
			Func<int[], Multimap<int, ProductCategory>> productCategories,
			Func<int[], Multimap<int, ProductManufacturer>> productManufacturers,
			Func<int[], Multimap<int, Discount>> appliedDiscounts,
			Func<int[], Multimap<int, ProductBundleItem>> productBundleItems)
		{
			if (products == null)
			{
				_productIds = new List<int>();
				_productIdsTierPrices = new List<int>();
				_productIdsAppliedDiscounts = new List<int>();
				_productIdsBundleItems = new List<int>();
			}
			else
			{
				_productIds = new List<int>(products.Select(x => x.Id));
				_productIdsTierPrices = new List<int>(products.Where(x => x.HasTierPrices).Select(x => x.Id));
				_productIdsAppliedDiscounts = new List<int>(products.Where(x => x.HasDiscountsApplied).Select(x => x.Id));
				_productIdsBundleItems = new List<int>(products.Where(x => x.ProductType == ProductType.BundledProduct).Select(x => x.Id));
			}

			_funcAttributes = attributes;
			_funcAttributeCombinations = attributeCombinations;
			_funcTierPrices = tierPrices;
			_funcProductCategories = productCategories;
			_funcProductManufacturers = productManufacturers;
			_funcAppliedDiscounts = appliedDiscounts;
			_funcProductBundleItems = productBundleItems;
		}

		public IReadOnlyList<int> ProductIds
		{
			get { return _productIds; }
		}

		public void Clear()
		{
			if (_attributes != null)
				_attributes.Clear();
			if (_attributeCombinations != null)
				_attributeCombinations.Clear();
			if (_tierPrices != null)
				_tierPrices.Clear();
			if (_productCategories != null)
				_productCategories.Clear();
			if (_productManufacturers != null)
				_productManufacturers.Clear();
			if (_appliedDiscounts != null)
				_appliedDiscounts.Clear();
			if (_productBundleItems != null)
				_productBundleItems.Clear();

			_productIdsBundleItems.Clear();
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

		public LazyMultimap<ProductCategory> ProductCategories
		{
			get
			{
				if (_productCategories == null)
				{
					_productCategories = new LazyMultimap<ProductCategory>(keys => _funcProductCategories(keys), _productIds);
				}
				return _productCategories;
			}
		}

		public LazyMultimap<ProductManufacturer> ProductManufacturers
		{
			get
			{
				if (_productManufacturers == null)
				{
					_productManufacturers = new LazyMultimap<ProductManufacturer>(keys => _funcProductManufacturers(keys), _productIds);
				}
				return _productManufacturers;
			}
		}

		public LazyMultimap<Discount> AppliedDiscounts
		{
			get
			{
				if (_appliedDiscounts == null)
				{
					_appliedDiscounts = new LazyMultimap<Discount>(keys => _funcAppliedDiscounts(keys), _productIdsAppliedDiscounts);
				}
				return _appliedDiscounts;
			}
		}

		public LazyMultimap<ProductBundleItem> ProductBundleItems
		{
			get
			{
				if (_productBundleItems == null)
				{
					_productBundleItems = new LazyMultimap<ProductBundleItem>(keys => _funcProductBundleItems(keys), _productIdsBundleItems);
				}
				return _productBundleItems;
			}
		}

		public void Collect(IEnumerable<int> productIds)
		{
			Attributes.Collect(productIds);
			AttributeCombinations.Collect(productIds);
			TierPrices.Collect(productIds);
			ProductCategories.Collect(productIds);
			AppliedDiscounts.Collect(productIds);
			ProductBundleItems.Collect(productIds);
		}
	}
}
