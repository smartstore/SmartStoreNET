using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Collections;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Services.Media;

namespace SmartStore.Services.DataExchange.Export.Internal
{
    internal class ManufacturerExportContext
    {
        protected List<int> _manufacturerIds;
        protected List<int> _fileIds;

        private Func<int[], Multimap<int, ProductManufacturer>> _funcProductManufacturers;
        private Func<int[], IList<MediaFileInfo>> _funcFiles;

        private LazyMultimap<ProductManufacturer> _productManufacturers;
        private LazyMultimap<MediaFileInfo> _files;

        public ManufacturerExportContext(
            IEnumerable<Manufacturer> manufacturers,
            Func<int[], Multimap<int, ProductManufacturer>> productManufacturers,
            Func<int[], IList<MediaFileInfo>> files)
        {
            if (manufacturers == null)
            {
                _manufacturerIds = new List<int>();
                _fileIds = new List<int>();
            }
            else
            {
                _manufacturerIds = new List<int>(manufacturers.Select(x => x.Id));
                _fileIds = manufacturers
                    .Select(x => x.MediaFileId ?? 0)
                    .Where(x => x != 0)
                    .Distinct()
                    .ToList();
            }

            _funcProductManufacturers = productManufacturers;
            _funcFiles = files;
        }

        public void Clear()
        {
            _productManufacturers?.Clear();
            _files?.Clear();

            _manufacturerIds?.Clear();
            _fileIds?.Clear();
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

        public LazyMultimap<MediaFileInfo> Files
        {
            get
            {
                if (_files == null)
                {
                    _files = new LazyMultimap<MediaFileInfo>(keys => _funcFiles(keys).ToMultimap(x => x.Id, x => x), _fileIds);
                }
                return _files;
            }
        }
    }
}
