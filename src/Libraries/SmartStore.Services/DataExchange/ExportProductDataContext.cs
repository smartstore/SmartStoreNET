using System;
using System.Collections.Generic;
using SmartStore.Collections;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Services.Catalog;

namespace SmartStore.Services.DataExchange
{
	public class ExportProductDataContext : PriceCalculationContext
	{
		private Func<int[], Multimap<int, ProductManufacturer>> _funcProductManufacturers;
		private Func<int[], Multimap<int, ProductPicture>> _funcProductPictures;

		private LazyMultimap<ProductManufacturer> _productManufacturers;
		private LazyMultimap<ProductPicture> _productPictures;

		public ExportProductDataContext(IEnumerable<Product> products,
			Func<int[], Multimap<int, ProductVariantAttribute>> attributes,
			Func<int[], Multimap<int, ProductVariantAttributeCombination>> attributeCombinations,
			Func<int[], Multimap<int, TierPrice>> tierPrices,
			Func<int[], Multimap<int, ProductCategory>> productCategories,
			Func<int[], Multimap<int, ProductManufacturer>> productManufacturers,
			Func<int[], Multimap<int, ProductPicture>> productPictures)
			: base(products,
			attributes,
			attributeCombinations,
			tierPrices,
			productCategories)
		{
			_funcProductManufacturers = productManufacturers;
			_funcProductPictures = productPictures;
		}

		public new void Clear()
		{
			if (_productManufacturers != null)
				_productManufacturers.Clear();
			if (_productPictures != null)
				_productPictures.Clear();

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
	}
}
