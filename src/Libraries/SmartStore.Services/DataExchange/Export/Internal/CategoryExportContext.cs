using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Collections;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Services.Media;

namespace SmartStore.Services.DataExchange.Export.Internal
{
    internal class CategoryExportContext
    {
        protected List<int> _categoryIds;
        protected List<int> _fileIds;

        private Func<int[], Multimap<int, ProductCategory>> _funcProductCategories;
        private Func<int[], IList<MediaFileInfo>> _funcFiles;

        private LazyMultimap<ProductCategory> _productCategories;
        private LazyMultimap<MediaFileInfo> _files;

        public CategoryExportContext(
            IEnumerable<Category> categories,
            Func<int[], Multimap<int, ProductCategory>> productCategories,
            Func<int[], IList<MediaFileInfo>> files)
        {
            if (categories == null)
            {
                _categoryIds = new List<int>();
                _fileIds = new List<int>();
            }
            else
            {
                _categoryIds = new List<int>(categories.Select(x => x.Id));
                _fileIds = categories.Select(x => x.MediaFileId ?? 0)
                    .Where(x => x != 0)
                    .Distinct()
                    .ToList();
            }

            _funcProductCategories = productCategories;
            _funcFiles = files;
        }

        public void Clear()
        {
            _productCategories?.Clear();
            _files?.Clear();

            _categoryIds?.Clear();
            _fileIds?.Clear();
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
