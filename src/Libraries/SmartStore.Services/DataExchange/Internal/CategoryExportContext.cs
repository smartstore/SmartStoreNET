using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Collections;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Media;

namespace SmartStore.Services.DataExchange.Export.Internal
{
	internal class CategoryExportContext
	{
		protected List<int> _categoryIds;
		protected List<int> _pictureIds;

		private Func<int[], Multimap<int, ProductCategory>> _funcProductCategories;
		private Func<int[], IList<Picture>> _funcPictures;

		private LazyMultimap<ProductCategory> _productCategories;
		private LazyMultimap<Picture> _pictures;

		public CategoryExportContext(
			IEnumerable<Category> categories,
			Func<int[], Multimap<int, ProductCategory>> productCategories,
			Func<int[], IList<Picture>> pictures)
		{
			if (categories == null)
			{
				_categoryIds = new List<int>();
				_pictureIds = new List<int>();
			}
			else
			{
				_categoryIds = new List<int>(categories.Select(x => x.Id));
				_pictureIds = new List<int>(categories.Where(x => (x.PictureId ?? 0) != 0).Select(x => x.PictureId ?? 0));
			}

			_funcProductCategories = productCategories;
			_funcPictures = pictures;
		}

		public void Clear()
		{
			if (_productCategories != null)
				_productCategories.Clear();
			if (_pictures != null)
				_pictures.Clear();

			_categoryIds.Clear();
			_pictureIds.Clear();
		}

		public LazyMultimap<ProductCategory> ProductCategories
		{
			get
			{
				if (_productCategories == null)
				{
					_productCategories = new LazyMultimap<ProductCategory>(keys => _funcProductCategories(keys), _categoryIds);
				}
				return _productCategories;
			}
		}

		public LazyMultimap<Picture> Pictures
		{
			get
			{
				if (_pictures == null)
				{
					_pictures = new LazyMultimap<Picture>(keys => _funcPictures(keys).ToMultimap(x => x.Id, x => x), _pictureIds);
				}
				return _pictures;
			}
		}
	}
}
