using System.Collections.Generic;
using System.Data;
using System.Linq;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Stores;

namespace SmartStore.Services.Catalog
{
    public partial class ProductTagService : IProductTagService
    {
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : store ID
        /// {1} : include hidden
        /// </remarks>
        private const string PRODUCTTAG_COUNT_KEY = "producttag:count-{0}-{1}";

		/// <summary>
		/// Key pattern to clear cache
		/// </summary>
		private const string PRODUCTTAG_PATTERN_KEY = "producttag:*";

        private readonly IRepository<ProductTag> _productTagRepository;
		private readonly IRepository<StoreMapping> _storeMappingRepository;
		private readonly IDataProvider _dataProvider;
		private readonly IDbContext _dbContext;
		private readonly CommonSettings _commonSettings;
		private readonly ICacheManager _cacheManager;

        public ProductTagService(
			IRepository<ProductTag> productTagRepository,
			IRepository<StoreMapping> storeMappingRepository,
			IDataProvider dataProvider,
			IDbContext dbContext,
			CommonSettings commonSettings,
			ICacheManager cacheManager)
        {
            _productTagRepository = productTagRepository;
			_storeMappingRepository = storeMappingRepository;
			_dataProvider = dataProvider;
			_dbContext = dbContext;
			_commonSettings = commonSettings;
			_cacheManager = cacheManager;

			QuerySettings = DbQuerySettings.Default;
		}

		public DbQuerySettings QuerySettings { get; set; }

        public virtual void DeleteProductTag(ProductTag productTag)
        {
            if (productTag != null)
            {
                _productTagRepository.Delete(productTag);

                _cacheManager.RemoveByPattern(PRODUCTTAG_PATTERN_KEY);
            }
        }

		public virtual IList<ProductTag> GetAllProductTags(bool includeHidden = false)
        {
            var query = _productTagRepository.Table;

            if (!includeHidden)
            {
                query = query.Where(x => x.Published);
            }

            var productTags = query.ToList();
            return productTags;
        }

        public virtual IList<string> GetAllProductTagNames()
        {
            var query = from pt in _productTagRepository.TableUntracked
                        orderby pt.Name ascending
                        select pt.Name;
            return query.ToList();
        }

        public virtual ProductTag GetProductTagById(int productTagId)
        {
            if (productTagId == 0)
            {
                return null;
            }

            var productTag = _productTagRepository.GetById(productTagId);
            return productTag;
        }

        public virtual ProductTag GetProductTagByName(string name)
        {
            var query = from pt in _productTagRepository.Table
                        where pt.Name == name
                        select pt;

            var productTag = query.FirstOrDefault();
            return productTag;
        }

        public virtual void InsertProductTag(ProductTag productTag)
        {
            Guard.NotNull(productTag, nameof(productTag));

            _productTagRepository.Insert(productTag);

			_cacheManager.RemoveByPattern(PRODUCTTAG_PATTERN_KEY);
        }

        public virtual void UpdateProductTag(ProductTag productTag)
        {
            Guard.NotNull(productTag, nameof(productTag));

            _productTagRepository.Update(productTag);

			_cacheManager.RemoveByPattern(PRODUCTTAG_PATTERN_KEY);
        }

		public virtual int GetProductCount(int productTagId, int storeId, bool includeHidden = false)
		{
			var dictionary = GetProductCount(storeId, includeHidden);

            if (dictionary.TryGetValue(productTagId, out var count))
            {
                return count;
            }

			return 0;
		}

        protected virtual Dictionary<int, int> GetProductCount(int storeId, bool includeHidden)
        {
            var storeToken = QuerySettings.IgnoreMultiStore ? "0" : storeId.ToString();
            var key = string.Format(PRODUCTTAG_COUNT_KEY, storeId, includeHidden.ToString().ToLower());

            return _cacheManager.Get(key, () =>
            {
                IEnumerable<ProductTagWithCount> tagCount = null;

                if (_commonSettings.UseStoredProceduresIfSupported && _dataProvider.StoredProceduresSupported)
                {
                    // Stored procedures are enabled and supported by the database. It's much faster than the LINQ implementation below.
                    var pStoreId = _dataProvider.GetParameter();
                    pStoreId.ParameterName = "StoreId";
                    pStoreId.Value = storeId;
                    pStoreId.DbType = DbType.Int32;

                    var pIncludeHidden = _dataProvider.GetParameter();
                    pIncludeHidden.ParameterName = "IncludeHidden";
                    pIncludeHidden.Value = includeHidden;
                    pIncludeHidden.DbType = DbType.Boolean;

                    tagCount = _dbContext.SqlQuery<ProductTagWithCount>("Exec ProductTagCountLoadAll @StoreId, @IncludeHidden", pStoreId, pIncludeHidden);
                }
                else
                {
                    // Stored procedures aren't supported. Use LINQ.
                    var query = _productTagRepository.TableUntracked;
                    if (!includeHidden)
                    {
                        query = query.Where(x => x.Published);
                    }

                    tagCount = query
                        .Select(pt => new ProductTagWithCount
                        {
                            ProductTagId = pt.Id,
                            ProductCount = (storeId > 0 && !QuerySettings.IgnoreMultiStore) ?
                                (from p in pt.Products
                                 join sm in _storeMappingRepository.Table on new { pid = p.Id, pname = "Product" } equals new { pid = sm.EntityId, pname = sm.EntityName } into psm
                                 from sm in psm.DefaultIfEmpty()
                                 where (!p.LimitedToStores || storeId == sm.StoreId) && !p.Deleted && p.Visibility == ProductVisibility.Full && p.Published && !p.IsSystemProduct && (includeHidden || pt.Published)
                                 select p).Count() :
                                pt.Products.Count(p => !p.Deleted && p.Visibility == ProductVisibility.Full && p.Published && !p.IsSystemProduct && (includeHidden || pt.Published))
                        });
                }

                return tagCount.ToDictionary(x => x.ProductTagId, x => x.ProductCount);
            });
        }

        private class ProductTagWithCount
        {
            public int ProductTagId { get; set; }
            public int ProductCount { get; set; }
        }
    }
}
