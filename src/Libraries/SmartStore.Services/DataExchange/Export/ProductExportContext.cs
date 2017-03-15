using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Collections;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Discounts;
using SmartStore.Core.Domain.Media;
using SmartStore.Services.Catalog;

namespace SmartStore.Services.DataExchange.Export
{
	/// <summary>
	/// Cargo data to reduce database round trips during work with product batches (export, list model creation etc.)
	/// </summary>
	public class ProductExportContext : PriceCalculationContext
	{
		private Func<int[], Multimap<int, ProductPicture>> _funcProductPictures;
		private Func<int[], Multimap<int, ProductTag>> _funcProductTags;
		private Func<int[], Multimap<int, ProductSpecificationAttribute>> _funcSpecificationAttributes;
		private Func<int[], Multimap<int, Picture>> _funcPictures;

		private LazyMultimap<ProductPicture> _productPictures;
		private LazyMultimap<ProductTag> _productTags;
		private LazyMultimap<ProductSpecificationAttribute> _specificationAttributes;
		private LazyMultimap<Picture> _pictures;

		public ProductExportContext(
			IEnumerable<Product> products,
			Func<int[], Multimap<int, ProductVariantAttribute>> attributes,
			Func<int[], Multimap<int, ProductVariantAttributeCombination>> attributeCombinations,
			Func<int[], Multimap<int, ProductSpecificationAttribute>> specificationAttributes,
			Func<int[], Multimap<int, TierPrice>> tierPrices,
			Func<int[], Multimap<int, ProductCategory>> productCategories,
			Func<int[], Multimap<int, ProductManufacturer>> productManufacturers,
			Func<int[], Multimap<int, Discount>> appliedDiscounts,
			Func<int[], Multimap<int, ProductBundleItem>> productBundleItems,
			Func<int[], Multimap<int, Picture>> pictures,
			Func<int[], Multimap<int, ProductPicture>> productPictures,
			Func<int[], Multimap<int, ProductTag>> productTags)
			: base(products,
				attributes,
				attributeCombinations,
				tierPrices,
				productCategories,
				productManufacturers,
				appliedDiscounts,
				productBundleItems)
		{
			_funcPictures = pictures;
			_funcProductPictures = productPictures;
			_funcProductTags = productTags;
			_funcSpecificationAttributes = specificationAttributes;
		}

		public new void Clear()
		{
			if (_productPictures != null)
				_productPictures.Clear();
			if (_productTags != null)
				_productTags.Clear();
			if (_specificationAttributes != null)
				_specificationAttributes.Clear();
			if (_pictures != null)
				_pictures.Clear();

			base.Clear();
		}

		public LazyMultimap<Picture> Pictures
		{
			get
			{
				if (_pictures == null)
				{
					_pictures = new LazyMultimap<Picture>(keys => _funcPictures(keys), _productIds);
				}
				return _pictures;
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

		public LazyMultimap<ProductSpecificationAttribute> SpecificationAttributes
		{
			get
			{
				if (_specificationAttributes == null)
				{
					_specificationAttributes = new LazyMultimap<ProductSpecificationAttribute>(keys => _funcSpecificationAttributes(keys), _productIds);
				}
				return _specificationAttributes;
			}
		}
	}
}
