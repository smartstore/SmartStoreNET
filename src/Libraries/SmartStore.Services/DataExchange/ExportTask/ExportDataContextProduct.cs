using System;
using System.Linq;
using System.Collections.Generic;
using SmartStore.Collections;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Discounts;
using SmartStore.Services.Catalog;

namespace SmartStore.Services.DataExchange.ExportTask
{
	internal class ExportDataContextProduct : PriceCalculationContext
	{
		private List<int> _productIdsBundleItems;

		private Func<int[], Multimap<int, ProductManufacturer>> _funcProductManufacturers;
		private Func<int[], Multimap<int, ProductPicture>> _funcProductPictures;
		private Func<int[], Multimap<int, ProductTag>> _funcProductTags;
		private Func<int[], Multimap<int, ProductSpecificationAttribute>> _funcProductSpecificationAttributes;
		private Func<int[], Multimap<int, ProductBundleItem>> _funcProductBundleItems;

		private LazyMultimap<ProductManufacturer> _productManufacturers;
		private LazyMultimap<ProductPicture> _productPictures;
		private LazyMultimap<ProductTag> _productTags;
		private LazyMultimap<ProductSpecificationAttribute> _productSpecificationAttributes;
		private LazyMultimap<ProductBundleItem> _productBundleItems;

		public ExportDataContextProduct(IEnumerable<Product> products,
			Func<int[], Multimap<int, ProductVariantAttribute>> attributes,
			Func<int[], Multimap<int, ProductVariantAttributeCombination>> attributeCombinations,
			Func<int[], Multimap<int, TierPrice>> tierPrices,
			Func<int[], Multimap<int, ProductCategory>> productCategories,
			Func<int[], Multimap<int, ProductManufacturer>> productManufacturers,
			Func<int[], Multimap<int, ProductPicture>> productPictures,
			Func<int[], Multimap<int, ProductTag>> productTags,
			Func<int[], Multimap<int, Discount>> productAppliedDiscounts,
			Func<int[], Multimap<int, ProductSpecificationAttribute>> productSpecificationAttributes,
			Func<int[], Multimap<int, ProductBundleItem>> productBundleItems)
			: base(products,
				attributes,
				attributeCombinations,
				tierPrices,
				productCategories,
				productAppliedDiscounts)
		{
			if (products == null)
			{
				_productIdsBundleItems = new List<int>();
			}
			else
			{
				_productIdsBundleItems = new List<int>(products.Where(x => x.ProductType == ProductType.BundledProduct).Select(x => x.Id));
			}

			_funcProductManufacturers = productManufacturers;
			_funcProductPictures = productPictures;
			_funcProductTags = productTags;
			_funcProductSpecificationAttributes = productSpecificationAttributes;
			_funcProductBundleItems = productBundleItems;
		}

		public new void Clear()
		{
			if (_productManufacturers != null)
				_productManufacturers.Clear();
			if (_productPictures != null)
				_productPictures.Clear();
			if (_productTags != null)
				_productTags.Clear();
			if (_productSpecificationAttributes != null)
				_productSpecificationAttributes.Clear();
			if (_productBundleItems != null)
				_productBundleItems.Clear();

			base.Clear();
		}

		//public new void Collect(IEnumerable<int> productIds)
		//{
		//	ProductManufacturers.Collect(productIds);
		//	ProductPictures.Collect(productIds);

		//	base.Collect(productIds);
		//}

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

		public LazyMultimap<ProductPicture> ProductPictures
		{
			get
			{
				if (_productPictures == null)
				{
					_productPictures = new LazyMultimap<ProductPicture>(keys => _funcProductPictures(keys), _productIds);
				}
				return _productPictures;
			}
		}

		public LazyMultimap<ProductTag> ProductTags
		{
			get
			{
				if (_productTags == null)
				{
					_productTags = new LazyMultimap<ProductTag>(keys => _funcProductTags(keys), _productIds);
				}
				return _productTags;
			}
		}

		public LazyMultimap<ProductSpecificationAttribute> ProductSpecificationAttributes
		{
			get
			{
				if (_productSpecificationAttributes == null)
				{
					_productSpecificationAttributes = new LazyMultimap<ProductSpecificationAttribute>(keys => _funcProductSpecificationAttributes(keys), _productIds);
				}
				return _productSpecificationAttributes;
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
	}
}
