using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Events;

namespace SmartStore.Services.Catalog
{
	/// <summary>
	/// Product tag service
	/// </summary>
	public partial class ProductTagService : IProductTagService
    {
		#region Constants

		/// <summary>
		/// Key for caching
		/// </summary>
		/// <remarks>
		/// {0} : store ID
		/// </remarks>
		private const string PRODUCTTAG_COUNT_KEY = "producttag:count-{0}";

		/// <summary>
		/// Key pattern to clear cache
		/// </summary>
		private const string PRODUCTTAG_PATTERN_KEY = "producttag:*";

		#endregion

        #region Fields

        private readonly IRepository<ProductTag> _productTagRepository;
		private readonly IRepository<StoreMapping> _storeMappingRepository;
		private readonly IDataProvider _dataProvider;
		private readonly IDbContext _dbContext;
		private readonly CommonSettings _commonSettings;
		private readonly ICacheManager _cacheManager;
        private readonly IEventPublisher _eventPublisher;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="productTagRepository">Product tag repository</param>
		/// <param name="dataProvider">Data provider</param>
		/// <param name="dbContext">Database Context</param>
		/// <param name="commonSettings">Common settings</param>
		/// <param name="cacheManager">Cache manager</param>
        /// <param name="eventPublisher">Event published</param>
        public ProductTagService(
			IRepository<ProductTag> productTagRepository,
			IRepository<StoreMapping> storeMappingRepository,
			IDataProvider dataProvider,
			IDbContext dbContext,
			CommonSettings commonSettings,
			ICacheManager cacheManager,
            IEventPublisher eventPublisher)
        {
            _productTagRepository = productTagRepository;
			_storeMappingRepository = storeMappingRepository;
			_dataProvider = dataProvider;
			_dbContext = dbContext;
			_commonSettings = commonSettings;
			_cacheManager = cacheManager;
            _eventPublisher = eventPublisher;

			QuerySettings = DbQuerySettings.Default;
		}

		public DbQuerySettings QuerySettings { get; set; }

		#endregion

		#region Nested classes

		private class ProductTagWithCount
		{
			public int ProductTagId { get; set; }
			public int ProductCount { get; set; }
		}

		#endregion

		#region Utilities

		/// <summary>
		/// Get product count for each of existing product tag
		/// </summary>
		/// <param name="storeId">Store identifier</param>
		/// <returns>Dictionary of "product tag ID : product count"</returns>
		private Dictionary<int, int> GetProductCount(int storeId)
		{
			string key = string.Format(PRODUCTTAG_COUNT_KEY, storeId);
			return _cacheManager.Get(key, () =>
			{
				IEnumerable<ProductTagWithCount> tagCount = null;

				if (_commonSettings.UseStoredProceduresIfSupported && _dataProvider.StoredProceduresSupported)
				{
					//stored procedures are enabled and supported by the database. It's much faster than the LINQ implementation below 

					var pStoreId = _dataProvider.GetParameter();
					pStoreId.ParameterName = "StoreId";
					pStoreId.Value = storeId;
					pStoreId.DbType = DbType.Int32;

					tagCount = _dbContext.SqlQuery<ProductTagWithCount>("Exec ProductTagCountLoadAll @StoreId", pStoreId);
				}
				else
				{
					//stored procedures aren't supported. Use LINQ

					tagCount = _productTagRepository.Table
						.Select(pt => new ProductTagWithCount
						{
							ProductTagId = pt.Id,
							ProductCount = (storeId > 0 && !QuerySettings.IgnoreMultiStore) ?
								(from p in pt.Products
								join sm in _storeMappingRepository.Table on new { pid = p.Id, pname = "Product" } equals new { pid = sm.EntityId, pname = sm.EntityName } into psm
								from sm in psm.DefaultIfEmpty()
								where (!p.LimitedToStores || storeId == sm.StoreId) && !p.Deleted && p.Published
								select p).Count() :
								pt.Products.Count(p => !p.Deleted && p.Published)
						});
				}

				return tagCount.ToDictionary(x => x.ProductTagId, x => x.ProductCount);
			});
		}

		#endregion

        #region Methods

        /// <summary>
        /// Delete a product tag
        /// </summary>
        /// <param name="productTag">Product tag</param>
        public virtual void DeleteProductTag(ProductTag productTag)
        {
            if (productTag == null)
                throw new ArgumentNullException("productTag");

            _productTagRepository.Delete(productTag);

			//cache
			_cacheManager.RemoveByPattern(PRODUCTTAG_PATTERN_KEY);
        }

        /// <summary>
        /// Gets all product tags
        /// </summary>
        /// <returns>Product tags</returns>
		public virtual IList<ProductTag> GetAllProductTags()
        {
            var query = _productTagRepository.Table;
            var productTags = query.ToList();
            return productTags;
        }

        /// <summary>
        /// Gets all product tag names
        /// </summary>
        /// <returns>Product tag names as list</returns>
        public virtual IList<string> GetAllProductTagNames()
        {
            var query = from pt in _productTagRepository.Table
                        orderby pt.Name ascending
                        select pt.Name;
            return query.ToList();
        }

        /// <summary>
        /// Gets product tag
        /// </summary>
        /// <param name="productTagId">Product tag identifier</param>
        /// <returns>Product tag</returns>
        public virtual ProductTag GetProductTagById(int productTagId)
        {
            if (productTagId == 0)
                return null;

            var productTag = _productTagRepository.GetById(productTagId);
            return productTag;
        }

        /// <summary>
        /// Gets product tag by name
        /// </summary>
        /// <param name="name">Product tag name</param>
        /// <returns>Product tag</returns>
        public virtual ProductTag GetProductTagByName(string name)
        {
            var query = from pt in _productTagRepository.Table
                        where pt.Name == name
                        select pt;

            var productTag = query.FirstOrDefault();
            return productTag;
        }

        /// <summary>
        /// Inserts a product tag
        /// </summary>
        /// <param name="productTag">Product tag</param>
        public virtual void InsertProductTag(ProductTag productTag)
        {
            if (productTag == null)
                throw new ArgumentNullException("productTag");

            _productTagRepository.Insert(productTag);

			_cacheManager.RemoveByPattern(PRODUCTTAG_PATTERN_KEY);
        }

        /// <summary>
        /// Updates the product tag
        /// </summary>
        /// <param name="productTag">Product tag</param>
        public virtual void UpdateProductTag(ProductTag productTag)
        {
            if (productTag == null)
                throw new ArgumentNullException("productTag");

            _productTagRepository.Update(productTag);

			_cacheManager.RemoveByPattern(PRODUCTTAG_PATTERN_KEY);
        }

		/// <summary>
		/// Get number of products
		/// </summary>
		/// <param name="productTagId">Product tag identifier</param>
		/// <param name="storeId">Store identifier</param>
		/// <returns>Number of products</returns>
		public virtual int GetProductCount(int productTagId, int storeId)
		{
			var dictionary = GetProductCount(storeId);
			
			if (dictionary.ContainsKey(productTagId))
				return dictionary[productTagId];

			return 0;
		}
        
        #endregion
    }
}
