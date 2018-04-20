using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Collections;
using SmartStore.Core;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Events;
using SmartStore.Data.Caching;

namespace SmartStore.Services.Catalog
{
	public partial class ManufacturerService : IManufacturerService
    {
        private const string PRODUCTMANUFACTURERS_ALLBYMANUFACTURERID_KEY = "SmartStore.productmanufacturer.allbymanufacturerid-{0}-{1}-{2}-{3}-{4}";
        private const string PRODUCTMANUFACTURERS_ALLBYPRODUCTID_KEY = "SmartStore.productmanufacturer.allbyproductid-{0}-{1}-{2}";
        private const string MANUFACTURERS_PATTERN_KEY = "SmartStore.manufacturer.*";
        private const string PRODUCTMANUFACTURERS_PATTERN_KEY = "SmartStore.productmanufacturer.*";

        private readonly IRepository<Manufacturer> _manufacturerRepository;
        private readonly IRepository<ProductManufacturer> _productManufacturerRepository;
        private readonly IRepository<Product> _productRepository;
		private readonly IRepository<StoreMapping> _storeMappingRepository;
		private readonly IWorkContext _workContext;
		private readonly IStoreContext _storeContext;
        private readonly IEventPublisher _eventPublisher;
        private readonly IRequestCache _requestCache;

		public ManufacturerService(IRequestCache requestCache,
            IRepository<Manufacturer> manufacturerRepository,
            IRepository<ProductManufacturer> productManufacturerRepository,
            IRepository<Product> productRepository,
			IRepository<StoreMapping> storeMappingRepository,
			IWorkContext workContext,
			IStoreContext storeContext,
            IEventPublisher eventPublisher)
        {
            _requestCache = requestCache;
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

        public virtual void DeleteManufacturer(Manufacturer manufacturer)
        {
            if (manufacturer == null)
                throw new ArgumentNullException("manufacturer");
            
            manufacturer.Deleted = true;
            UpdateManufacturer(manufacturer);
        }

		public virtual IQueryable<Manufacturer> GetManufacturers(bool showHidden = false, int storeId = 0)
		{
			var query = _manufacturerRepository.Table
				.Where(m => !m.Deleted);

			if (!showHidden)
				query = query.Where(m => m.Published);

			if (!QuerySettings.IgnoreMultiStore && storeId > 0)
			{
				query = from m in query
						join sm in _storeMappingRepository.Table
						on new { c1 = m.Id, c2 = "Manufacturer" } equals new { c1 = sm.EntityId, c2 = sm.EntityName } into m_sm
						from sm in m_sm.DefaultIfEmpty()
						where !m.LimitedToStores || storeId == sm.StoreId
						select m;

				query = from m in query
						group m by m.Id into mGroup
						orderby mGroup.Key
						select mGroup.FirstOrDefault();
			}

			return query;
		}

        public virtual IList<Manufacturer> GetAllManufacturers(bool showHidden = false)
        {
            return GetAllManufacturers(null, 0, showHidden);
        }

        public virtual IList<Manufacturer> GetAllManufacturers(string manufacturerName, int storeId = 0, bool showHidden = false)
        {
			var query = GetManufacturers(showHidden, storeId);

			if (manufacturerName.HasValue())
				query = query.Where(m => m.Name.Contains(manufacturerName));

			query = query.OrderBy(m => m.DisplayOrder)
				.ThenBy(m => m.Name);

            var manufacturers = query.ToList();
            return manufacturers;
        }

		public virtual IPagedList<Manufacturer> GetAllManufacturers(string manufacturerName,
            int pageIndex, int pageSize, int storeId = 0, bool showHidden = false)
        {
            var manufacturers = GetAllManufacturers(manufacturerName, storeId, showHidden);
            return new PagedList<Manufacturer>(manufacturers, pageIndex, pageSize);
        }

        public virtual Manufacturer GetManufacturerById(int manufacturerId)
        {
            if (manufacturerId == 0)
                return null;

			return _manufacturerRepository.GetByIdCached(manufacturerId, "db.manu.id-" + manufacturerId);
		}

        public virtual void InsertManufacturer(Manufacturer manufacturer)
        {
            if (manufacturer == null)
                throw new ArgumentNullException("manufacturer");

            _manufacturerRepository.Insert(manufacturer);

            //cache
            _requestCache.RemoveByPattern(MANUFACTURERS_PATTERN_KEY);
            _requestCache.RemoveByPattern(PRODUCTMANUFACTURERS_PATTERN_KEY);
        }

        public virtual void UpdateManufacturer(Manufacturer manufacturer)
        {
            if (manufacturer == null)
                throw new ArgumentNullException("manufacturer");

            _manufacturerRepository.Update(manufacturer);

            //cache
            _requestCache.RemoveByPattern(MANUFACTURERS_PATTERN_KEY);
            _requestCache.RemoveByPattern(PRODUCTMANUFACTURERS_PATTERN_KEY);
        }

		public virtual void UpdateHasDiscountsApplied(Manufacturer manufacturer)
		{
			Guard.NotNull(manufacturer, nameof(manufacturer));

			manufacturer.HasDiscountsApplied = manufacturer.AppliedDiscounts.Count > 0;
			UpdateManufacturer(manufacturer);
		}

		public virtual void DeleteProductManufacturer(ProductManufacturer productManufacturer)
        {
            if (productManufacturer == null)
                throw new ArgumentNullException("productManufacturer");

            _productManufacturerRepository.Delete(productManufacturer);

            _requestCache.RemoveByPattern(MANUFACTURERS_PATTERN_KEY);
            _requestCache.RemoveByPattern(PRODUCTMANUFACTURERS_PATTERN_KEY);
        }

        public virtual IPagedList<ProductManufacturer> GetProductManufacturersByManufacturerId(int manufacturerId, int pageIndex, int pageSize, bool showHidden = false)
        {
            if (manufacturerId == 0)
                return new PagedList<ProductManufacturer>(new List<ProductManufacturer>(), pageIndex, pageSize);

			string key = string.Format(PRODUCTMANUFACTURERS_ALLBYMANUFACTURERID_KEY, showHidden, manufacturerId, pageIndex, pageSize, _workContext.CurrentCustomer.Id, _storeContext.CurrentStore.Id);
            return _requestCache.Get(key, () =>
            {
                var query = from pm in _productManufacturerRepository.Table
                            join p in _productRepository.Table on pm.ProductId equals p.Id
                            where pm.ManufacturerId == manufacturerId &&
                                  !p.Deleted && !p.IsSystemProduct &&
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

        public virtual IList<ProductManufacturer> GetProductManufacturersByProductId(int productId, bool showHidden = false)
        {
            if (productId == 0)
                return new List<ProductManufacturer>();

			string key = string.Format(PRODUCTMANUFACTURERS_ALLBYPRODUCTID_KEY, showHidden, productId, _workContext.CurrentCustomer.Id, _storeContext.CurrentStore.Id);
            return _requestCache.Get(key, () =>
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
							// Store mapping
							var currentStoreId = _storeContext.CurrentStore.Id;
							query = from pm in query
									join m in _manufacturerRepository.Table on pm.ManufacturerId equals m.Id
									join sm in _storeMappingRepository.Table
									on new { c1 = m.Id, c2 = "Manufacturer" } equals new { c1 = sm.EntityId, c2 = sm.EntityName } into m_sm
									from sm in m_sm.DefaultIfEmpty()
									where !m.LimitedToStores || currentStoreId == sm.StoreId
									select pm;
						}

						// Only distinct manufacturers (group by ID)
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

		public virtual Multimap<int, ProductManufacturer> GetProductManufacturersByManufacturerIds(int[] manufacturerIds)
		{
			Guard.NotNull(manufacturerIds, nameof(manufacturerIds));

			var query = _productManufacturerRepository.TableUntracked
				.Where(x => manufacturerIds.Contains(x.ManufacturerId))
				.OrderBy(x => x.DisplayOrder);

			var map = query
				.ToList()
				.ToMultimap(x => x.ManufacturerId, x => x);

			return map;
		}

		public virtual Multimap<int, ProductManufacturer> GetProductManufacturersByProductIds(int[] productIds)
		{
			Guard.NotNull(productIds, nameof(productIds));

			var query =
				from pm in _productManufacturerRepository.TableUntracked.Expand(x => x.Manufacturer).Expand(x => x.Manufacturer.Picture)
				//join m in _manufacturerRepository.TableUntracked on pm.ManufacturerId equals m.Id // Eager loading does not work with this join
				where !pm.Manufacturer.Deleted && productIds.Contains(pm.ProductId)
				select pm;

			var map = query
				.OrderBy(x => x.ProductId)
				.ThenBy(x => x.DisplayOrder)
				.ToList()
				.ToMultimap(x => x.ProductId, x => x);

			return map;
		}
        
        public virtual ProductManufacturer GetProductManufacturerById(int productManufacturerId)
        {
            if (productManufacturerId == 0)
                return null;

            return _productManufacturerRepository.GetById(productManufacturerId);
        }

        public virtual void InsertProductManufacturer(ProductManufacturer productManufacturer)
        {
            if (productManufacturer == null)
                throw new ArgumentNullException("productManufacturer");

            _productManufacturerRepository.Insert(productManufacturer);

            _requestCache.RemoveByPattern(MANUFACTURERS_PATTERN_KEY);
            _requestCache.RemoveByPattern(PRODUCTMANUFACTURERS_PATTERN_KEY);

        }

        public virtual void UpdateProductManufacturer(ProductManufacturer productManufacturer)
        {
            if (productManufacturer == null)
                throw new ArgumentNullException("productManufacturer");

            _productManufacturerRepository.Update(productManufacturer);

            _requestCache.RemoveByPattern(MANUFACTURERS_PATTERN_KEY);
            _requestCache.RemoveByPattern(PRODUCTMANUFACTURERS_PATTERN_KEY);
        }
    }
}
