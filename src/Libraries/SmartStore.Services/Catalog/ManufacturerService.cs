using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Events;

namespace SmartStore.Services.Catalog
{
    /// <summary>
    /// Manufacturer service
    /// </summary>
    public partial class ManufacturerService : IManufacturerService
    {
        #region Constants
        private const string PRODUCTMANUFACTURERS_ALLBYMANUFACTURERID_KEY = "SmartStore.productmanufacturer.allbymanufacturerid-{0}-{1}-{2}-{3}-{4}";
        private const string PRODUCTMANUFACTURERS_ALLBYPRODUCTID_KEY = "SmartStore.productmanufacturer.allbyproductid-{0}-{1}-{2}";
        private const string MANUFACTURERS_PATTERN_KEY = "SmartStore.manufacturer.";
        private const string MANUFACTURERS_BY_ID_KEY = "SmartStore.manufacturer.id-{0}";
        private const string PRODUCTMANUFACTURERS_PATTERN_KEY = "SmartStore.productmanufacturer.";

        #endregion

        #region Fields

        private readonly IRepository<Manufacturer> _manufacturerRepository;
        private readonly IRepository<ProductManufacturer> _productManufacturerRepository;
        private readonly IRepository<Product> _productRepository;
		private readonly IRepository<StoreMapping> _storeMappingRepository;
		private readonly IWorkContext _workContext;
		private readonly IStoreContext _storeContext;
        private readonly IEventPublisher _eventPublisher;
        private readonly ICacheManager _cacheManager;
        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="manufacturerRepository">Category repository</param>
        /// <param name="productManufacturerRepository">ProductCategory repository</param>
        /// <param name="productRepository">Product repository</param>
		/// <param name="storeMappingRepository">Store mapping repository</param>
		/// <param name="workContext">Work context</param>
		/// <param name="storeContext">Store context</param>
        /// <param name="eventPublisher">Event published</param>
        public ManufacturerService(ICacheManager cacheManager,
            IRepository<Manufacturer> manufacturerRepository,
            IRepository<ProductManufacturer> productManufacturerRepository,
            IRepository<Product> productRepository,
			IRepository<StoreMapping> storeMappingRepository,
			IWorkContext workContext,
			IStoreContext storeContext,
            IEventPublisher eventPublisher)
        {
            _cacheManager = cacheManager;
            _manufacturerRepository = manufacturerRepository;
            _productManufacturerRepository = productManufacturerRepository;
            _productRepository = productRepository;
			_storeMappingRepository = storeMappingRepository;
			_workContext = workContext;
			_storeContext = storeContext;
            _eventPublisher = eventPublisher;

			this.QuerySettings = DbQuerySettings.Default;
		}

		public DbQuerySettings QuerySettings { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Deletes a manufacturer
        /// </summary>
        /// <param name="manufacturer">Manufacturer</param>
        public virtual void DeleteManufacturer(Manufacturer manufacturer)
        {
            if (manufacturer == null)
                throw new ArgumentNullException("manufacturer");
            
            manufacturer.Deleted = true;
            UpdateManufacturer(manufacturer);
        }

        /// <summary>
        /// Gets all manufacturers
        /// </summary>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Manufacturer collection</returns>
        public virtual IList<Manufacturer> GetAllManufacturers(bool showHidden = false)
        {
            return GetAllManufacturers(null, showHidden);
        }

        /// <summary>
        /// Gets all manufacturers
        /// </summary>
        /// <param name="manufacturerName">Manufacturer name</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Manufacturer collection</returns>
        public virtual IList<Manufacturer> GetAllManufacturers(string manufacturerName, bool showHidden = false)
        {
            var query = _manufacturerRepository.Table;
            if (!showHidden)
                query = query.Where(m => m.Published);
            if (!String.IsNullOrWhiteSpace(manufacturerName))
                query = query.Where(m => m.Name.Contains(manufacturerName));
            query = query.Where(m => !m.Deleted);
            query = query.OrderBy(m => m.DisplayOrder);

			//Store mapping
			if (!showHidden)
			{
				//Store mapping
				if (!QuerySettings.IgnoreMultiStore)
				{
					var currentStoreId = _storeContext.CurrentStore.Id;
					query = from m in query
							join sm in _storeMappingRepository.Table
							on new { c1 = m.Id, c2 = "Manufacturer" } equals new { c1 = sm.EntityId, c2 = sm.EntityName } into m_sm
							from sm in m_sm.DefaultIfEmpty()
							where !m.LimitedToStores || currentStoreId == sm.StoreId
							select m;
				}

				//only distinct manufacturers (group by ID)
				query = from m in query
						group m by m.Id	into mGroup
						orderby mGroup.Key
						select mGroup.FirstOrDefault();

				query = query.OrderBy(m => m.DisplayOrder);
			}

            var manufacturers = query.ToList();
            return manufacturers;
        }
        
        /// <summary>
        /// Gets all manufacturers
        /// </summary>
        /// <param name="manufacturerName">Manufacturer name</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Manufacturers</returns>
        public virtual IPagedList<Manufacturer> GetAllManufacturers(string manufacturerName,
            int pageIndex, int pageSize, bool showHidden = false)
        {
            var manufacturers = GetAllManufacturers(manufacturerName, showHidden);
            return new PagedList<Manufacturer>(manufacturers, pageIndex, pageSize);
        }

        /// <summary>
        /// Gets a manufacturer
        /// </summary>
        /// <param name="manufacturerId">Manufacturer identifier</param>
        /// <returns>Manufacturer</returns>
        public virtual Manufacturer GetManufacturerById(int manufacturerId)
        {
            if (manufacturerId == 0)
                return null;

            string key = string.Format(MANUFACTURERS_BY_ID_KEY, manufacturerId);
            return _cacheManager.Get(key, () => { 
                return _manufacturerRepository.GetById(manufacturerId); 
            });
        }

        /// <summary>
        /// Inserts a manufacturer
        /// </summary>
        /// <param name="manufacturer">Manufacturer</param>
        public virtual void InsertManufacturer(Manufacturer manufacturer)
        {
            if (manufacturer == null)
                throw new ArgumentNullException("manufacturer");

            _manufacturerRepository.Insert(manufacturer);

            //cache
            _cacheManager.RemoveByPattern(MANUFACTURERS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTMANUFACTURERS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(manufacturer);
        }

        /// <summary>
        /// Updates the manufacturer
        /// </summary>
        /// <param name="manufacturer">Manufacturer</param>
        public virtual void UpdateManufacturer(Manufacturer manufacturer)
        {
            if (manufacturer == null)
                throw new ArgumentNullException("manufacturer");

            _manufacturerRepository.Update(manufacturer);

            //cache
            _cacheManager.RemoveByPattern(MANUFACTURERS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTMANUFACTURERS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityUpdated(manufacturer);
        }

        /// <summary>
        /// Deletes a product manufacturer mapping
        /// </summary>
        /// <param name="productManufacturer">Product manufacturer mapping</param>
        public virtual void DeleteProductManufacturer(ProductManufacturer productManufacturer)
        {
            if (productManufacturer == null)
                throw new ArgumentNullException("productManufacturer");

            _productManufacturerRepository.Delete(productManufacturer);

            //cache
            _cacheManager.RemoveByPattern(MANUFACTURERS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTMANUFACTURERS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityDeleted(productManufacturer);
        }

        /// <summary>
        /// Gets product manufacturer collection
        /// </summary>
        /// <param name="manufacturerId">Manufacturer identifier</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Product manufacturer collection</returns>
        public virtual IPagedList<ProductManufacturer> GetProductManufacturersByManufacturerId(int manufacturerId, int pageIndex, int pageSize, bool showHidden = false)
        {
            if (manufacturerId == 0)
                return new PagedList<ProductManufacturer>(new List<ProductManufacturer>(), pageIndex, pageSize);

			string key = string.Format(PRODUCTMANUFACTURERS_ALLBYMANUFACTURERID_KEY, showHidden, manufacturerId, pageIndex, pageSize, _workContext.CurrentCustomer.Id, _storeContext.CurrentStore.Id);
            return _cacheManager.Get(key, () =>
            {
                var query = from pm in _productManufacturerRepository.Table
                            join p in _productRepository.Table on pm.ProductId equals p.Id
                            where pm.ManufacturerId == manufacturerId &&
                                  !p.Deleted &&
                                  (showHidden || p.Published)
                            orderby pm.DisplayOrder
                            select pm;

				if (!showHidden)
				{
					if (!QuerySettings.IgnoreMultiStore)
					{
						//Store mapping
						var currentStoreId = _storeContext.CurrentStore.Id;
						query = from pm in query
								join m in _manufacturerRepository.Table on pm.ManufacturerId equals m.Id
								join sm in _storeMappingRepository.Table
								on new { c1 = m.Id, c2 = "Manufacturer" } equals new { c1 = sm.EntityId, c2 = sm.EntityName } into m_sm
								from sm in m_sm.DefaultIfEmpty()
								where !m.LimitedToStores || currentStoreId == sm.StoreId
								select pm;
					}

					//only distinct manufacturers (group by ID)
					query = from pm in query
							group pm by pm.Id into pmGroup
							orderby pmGroup.Key
							select pmGroup.FirstOrDefault();

					query = query.OrderBy(pm => pm.DisplayOrder);
				}

                var productManufacturers = new PagedList<ProductManufacturer>(query, pageIndex, pageSize);
                return productManufacturers;
            });
        }

        /// <summary>
        /// Gets a product manufacturer mapping collection
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Product manufacturer mapping collection</returns>
        public virtual IList<ProductManufacturer> GetProductManufacturersByProductId(int productId, bool showHidden = false)
        {
            if (productId == 0)
                return new List<ProductManufacturer>();

			string key = string.Format(PRODUCTMANUFACTURERS_ALLBYPRODUCTID_KEY, showHidden, productId, _workContext.CurrentCustomer.Id, _storeContext.CurrentStore.Id);
            return _cacheManager.Get(key, () =>
				{
					var query = from pm in _productManufacturerRepository.Table.Expand(x => x.Manufacturer.Picture)
								join m in _manufacturerRepository.Table on
									pm.ManufacturerId equals m.Id
								where pm.ProductId == productId &&
									!m.Deleted &&
									(showHidden || m.Published)
								orderby pm.DisplayOrder
								select pm;

					if (!showHidden)
					{
						if (!QuerySettings.IgnoreMultiStore)
						{
							//Store mapping
							var currentStoreId = _storeContext.CurrentStore.Id;
							query = from pm in query
									join m in _manufacturerRepository.Table on pm.ManufacturerId equals m.Id
									join sm in _storeMappingRepository.Table
									on new { c1 = m.Id, c2 = "Manufacturer" } equals new { c1 = sm.EntityId, c2 = sm.EntityName } into m_sm
									from sm in m_sm.DefaultIfEmpty()
									where !m.LimitedToStores || currentStoreId == sm.StoreId
									select pm;
						}

						//only distinct manufacturers (group by ID)
						query = from pm in query
								group pm by pm.Id into mGroup
								orderby mGroup.Key
								select mGroup.FirstOrDefault();

						query = query.OrderBy(pm => pm.DisplayOrder);
					}

					var productManufacturers = query.ToList();
					return productManufacturers;
				});
        }
        
        /// <summary>
        /// Gets a product manufacturer mapping 
        /// </summary>
        /// <param name="productManufacturerId">Product manufacturer mapping identifier</param>
        /// <returns>Product manufacturer mapping</returns>
        public virtual ProductManufacturer GetProductManufacturerById(int productManufacturerId)
        {
            if (productManufacturerId == 0)
                return null;

            return _productManufacturerRepository.GetById(productManufacturerId);
        }

        /// <summary>
        /// Inserts a product manufacturer mapping
        /// </summary>
        /// <param name="productManufacturer">Product manufacturer mapping</param>
        public virtual void InsertProductManufacturer(ProductManufacturer productManufacturer)
        {
            if (productManufacturer == null)
                throw new ArgumentNullException("productManufacturer");

            _productManufacturerRepository.Insert(productManufacturer);

            //cache
            _cacheManager.RemoveByPattern(MANUFACTURERS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTMANUFACTURERS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(productManufacturer);
        }

        /// <summary>
        /// Updates the product manufacturer mapping
        /// </summary>
        /// <param name="productManufacturer">Product manufacturer mapping</param>
        public virtual void UpdateProductManufacturer(ProductManufacturer productManufacturer)
        {
            if (productManufacturer == null)
                throw new ArgumentNullException("productManufacturer");

            _productManufacturerRepository.Update(productManufacturer);

            //cache
            _cacheManager.RemoveByPattern(MANUFACTURERS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTMANUFACTURERS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityUpdated(productManufacturer);
        }

        #endregion
    }
}
