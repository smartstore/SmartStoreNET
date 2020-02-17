using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Collections;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Media;

namespace SmartStore.Services.DataExchange.Export.Internal
{
	internal class ManufacturerExportContext
	{
		protected List<int> _manufacturerIds;
		protected List<int> _pictureIds;

		private Func<int[], Multimap<int, ProductManufacturer>> _funcProductManufacturers;
		private Func<int[], IList<MediaFile>> _funcPictures;

		private LazyMultimap<ProductManufacturer> _productManufacturers;
		private LazyMultimap<MediaFile> _pictures;

		public ManufacturerExportContext(
			IEnumerable<Manufacturer> manufacturers,
			Func<int[], Multimap<int, ProductManufacturer>> productManufacturers,
			Func<int[], IList<MediaFile>> pictures)
		{
			if (manufacturers == null)
			{
				_manufacturerIds = new List<int>();
				_pictureIds = new List<int>();
			}
			else
			{
				_manufacturerIds = new List<int>(manufacturers.Select(x => x.Id));
				_pictureIds = new List<int>(manufacturers.Where(x => (x.MediaFileId ?? 0) != 0).Select(x => x.MediaFileId ?? 0));
			}

			_funcProductManufacturers = productManufacturers;
			_funcPictures = pictures;
		}

		public void Clear()
		{
			if (_productManufacturers != null)
				_productManufacturers.Clear();
			if (_pictures != null)
				_pictures.Clear();

			_manufacturerIds.Clear();
			_pictureIds.Clear();
		}

		public LazyMultimap<ProductManufacturer> ProductManufacturers
		{
			get
			{
				if (_productManufacturers == null)
				{
					_productManufacturers = new LazyMultimap<ProductManufacturer>(keys => _funcProductManufacturers(keys), _manufacturerIds);
				}
				return _productManufacturers;
			}
		}

		public LazyMultimap<MediaFile> Pictures
		{
			get
			{
				if (_pictures == null)
				{
					_pictures = new LazyMultimap<MediaFile>(keys => _funcPictures(keys).ToMultimap(x => x.Id, x => x), _pictureIds);
				}
				return _pictures;
			}
		}
	}
}
