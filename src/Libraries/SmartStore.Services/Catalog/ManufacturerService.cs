using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using SmartStore.Collections;
using SmartStore.Core;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Security;
using SmartStore.Core.Domain.Seo;
using SmartStore.Core.Domain.Stores;
using SmartStore.Data.Caching;
using SmartStore.Services.Seo;

namespace SmartStore.Services.Catalog
{
    public partial class ManufacturerService : IManufacturerService, IXmlSitemapPublisher
    {
        private const string PRODUCTMANUFACTURERS_ALLBYMANUFACTURERID_KEY = "productmanufacturer:allbymanufacturerid-{0}-{1}-{2}-{3}-{4}";
        private const string PRODUCTMANUFACTURERS_ALLBYPRODUCTID_KEY = "productmanufacturer:allbyproductid-{0}-{1}-{2}";
        private const string MANUFACTURERS_PATTERN_KEY = "manufacturer:*";
        private const string PRODUCTMANUFACTURERS_PATTERN_KEY = "productmanufacturer:*";

        private readonly IRepository<Manufacturer> _manufacturerRepository;
        private readonly IRepository<ProductManufacturer> _productManufacturerRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<StoreMapping> _storeMappingRepository;
        private readonly IRepository<AclRecord> _aclRepository;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IRequestCache _requestCache;

        public ManufacturerService(IRequestCache requestCache,
            IRepository<Manufacturer> manufacturerRepository,
            IRepository<ProductManufacturer> productManufacturerRepository,
            IRepository<Product> productRepository,
            IRepository<StoreMapping> storeMappingRepository,
            IRepository<AclRecord> aclRepository,
            IWorkContext workContext,
            IStoreContext storeContext)
        {
            _requestCache = requestCache;
            _manufacturerRepository = manufacturerRepository;
            _productManufacturerRepository = productManufacturerRepository;
            _productRepository = productRepository;
            _storeMappingRepository = storeMappingRepository;
            _aclRepository = aclRepository;
            _workContext = workContext;
            _storeContext = storeContext;

            QuerySettings = DbQuerySettings.Default;
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
            var grouping = false;
            var entityName = nameof(Manufacturer);
            var query = _manufacturerRepository.Table.Where(m => !m.Deleted);

            if (!showHidden)
            {
                query = query.Where(m => m.Published);
            }

            // Store mapping.
            if (!showHidden && storeId > 0 && !QuerySettings.IgnoreMultiStore)
            {
                query = from m in query
                        join sm in _storeMappingRepository.Table
                        on new { m1 = m.Id, m2 = entityName } equals new { m1 = sm.EntityId, m2 = sm.EntityName } into m_sm
                        from sm in m_sm.DefaultIfEmpty()
                        where !m.LimitedToStores || storeId == sm.StoreId
                        select m;

                grouping = true;
            }

            // ACL (access control list).
            if (!showHidden && !QuerySettings.IgnoreAcl)
            {
                var allowedCustomerRolesIds = _workContext.CurrentCustomer.GetRoleIds();

                query = from m in query
                        join a in _aclRepository.Table
                        on new { m1 = m.Id, m2 = entityName } equals new { m1 = a.EntityId, m2 = a.EntityName } into ma
                        from a in ma.DefaultIfEmpty()
                        where !m.SubjectToAcl || allowedCustomerRolesIds.Contains(a.CustomerRoleId)
                        select m;

                grouping = true;
            }

            if (grouping)
            {
                query =
                    from m in query
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

        public virtual IList<Manufacturer> GetManufacturersByIds(int[] manufacturerIds)
        {
            if (manufacturerIds == null || !manufacturerIds.Any())
            {
                return new List<Manufacturer>();
            }

            var query = from m in _manufacturerRepository.Table
                        where manufacturerIds.Contains(m.Id)
                        select m;

            var manufacturers = query.ToList();

            // Sort by passed identifier sequence.
            return manufacturers.OrderBySequence(manufacturerIds).ToList();
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
            {
                return new PagedList<ProductManufacturer>(new List<ProductManufacturer>(), pageIndex, pageSize);
            }

            var storeId = _storeContext.CurrentStore.Id;
            var storeToken = QuerySettings.IgnoreMultiStore ? "0" : storeId.ToString();
            var rolesToken = QuerySettings.IgnoreAcl || showHidden ? "0" : _workContext.CurrentCustomer.GetRolesIdent();
            var key = string.Format(PRODUCTMANUFACTURERS_ALLBYMANUFACTURERID_KEY, showHidden, manufacturerId, pageIndex, pageSize, rolesToken, storeToken);

            return _requestCache.Get(key, () =>
            {
                var query = from pm in _productManufacturerRepository.Table
                            join p in _productRepository.Table on pm.ProductId equals p.Id
                            where pm.ManufacturerId == manufacturerId && !p.Deleted && (showHidden || p.Published)
                            select pm;

                query = ApplyHiddenProductManufacturerFilter(query, storeId, showHidden);
                query = query.OrderBy(pm => pm.DisplayOrder);

                var productManufacturers = new PagedList<ProductManufacturer>(query, pageIndex, pageSize);
                return productManufacturers;
            });
        }

        public virtual IList<ProductManufacturer> GetProductManufacturersByProductId(int productId, bool showHidden = false)
        {
            if (productId == 0)
            {
                return new List<ProductManufacturer>();
            }

            var storeId = _storeContext.CurrentStore.Id;
            var storeToken = QuerySettings.IgnoreMultiStore ? "0" : storeId.ToString();
            var rolesToken = QuerySettings.IgnoreAcl || showHidden ? "0" : _workContext.CurrentCustomer.GetRolesIdent();
            var key = string.Format(PRODUCTMANUFACTURERS_ALLBYPRODUCTID_KEY, showHidden, productId, rolesToken, storeToken);

            return _requestCache.Get(key, () =>
            {
                var query = from pm in _productManufacturerRepository.Table
                            join m in _manufacturerRepository.Table on pm.ManufacturerId equals m.Id
                            where pm.ProductId == productId && !m.Deleted && (showHidden || m.Published)
                            select pm;

                query = ApplyHiddenProductManufacturerFilter(query, storeId, showHidden);
                query = query.OrderBy(pm => pm.DisplayOrder);
                query = query.Include(pm => pm.Manufacturer.MediaFile);

                var productManufacturers = query.ToList();
                return productManufacturers;
            });
        }

        public virtual Multimap<int, ProductManufacturer> GetProductManufacturersByManufacturerIds(int[] manufacturerIds, bool showHidden = false)
        {
            Guard.NotNull(manufacturerIds, nameof(manufacturerIds));

            var query = _productManufacturerRepository.TableUntracked
                .Where(x => manufacturerIds.Contains(x.ManufacturerId));

            query = ApplyHiddenProductManufacturerFilter(query, _storeContext.CurrentStore.Id, showHidden);
            query = query.OrderBy(pm => pm.DisplayOrder);

            var map = query
                .ToList()
                .ToMultimap(x => x.ManufacturerId, x => x);

            return map;
        }

        public virtual Multimap<int, ProductManufacturer> GetProductManufacturersByProductIds(int[] productIds, bool showHidden = false)
        {
            Guard.NotNull(productIds, nameof(productIds));

            if (!productIds.Any())
            {
                return new Multimap<int, ProductManufacturer>();
            }

            var query =
                from pm in _productManufacturerRepository.TableUntracked
                    //join m in _manufacturerRepository.TableUntracked on pm.ManufacturerId equals m.Id // Eager loading does not work with this join
                where !pm.Manufacturer.Deleted && productIds.Contains(pm.ProductId)
                select pm;

            query = ApplyHiddenProductManufacturerFilter(query, _storeContext.CurrentStore.Id, showHidden);
            query = query.Include(x => x.Manufacturer.MediaFile);

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

        protected virtual IQueryable<ProductManufacturer> ApplyHiddenProductManufacturerFilter(IQueryable<ProductManufacturer> query, int storeId = 0, bool showHidden = false)
        {
            var entityName = nameof(Manufacturer);
            var grouping = false;

            // Store mapping.
            if (!showHidden && storeId > 0 && !QuerySettings.IgnoreMultiStore)
            {
                query =
                    from pm in query
                    join m in _manufacturerRepository.Table on pm.ManufacturerId equals m.Id
                    join sm in _storeMappingRepository.Table
                    on new { m1 = m.Id, m2 = entityName } equals new { m1 = sm.EntityId, m2 = sm.EntityName } into m_sm
                    from sm in m_sm.DefaultIfEmpty()
                    where !m.LimitedToStores || storeId == sm.StoreId
                    select pm;

                grouping = true;
            }

            // ACL (access control list).
            if (!showHidden && !QuerySettings.IgnoreAcl)
            {
                var allowedCustomerRolesIds = _workContext.CurrentCustomer.GetRoleIds();

                query =
                    from pm in query
                    join m in _manufacturerRepository.Table on pm.ManufacturerId equals m.Id
                    join a in _aclRepository.Table
                    on new { m1 = m.Id, m2 = entityName } equals new { m1 = a.EntityId, m2 = a.EntityName } into ma
                    from a in ma.DefaultIfEmpty()
                    where !m.SubjectToAcl || allowedCustomerRolesIds.Contains(a.CustomerRoleId)
                    select pm;

                grouping = true;
            }

            if (grouping)
            {
                query =
                    from pm in query
                    group pm by pm.Id into pmGroup
                    orderby pmGroup.Key
                    select pmGroup.FirstOrDefault();
            }

            return query;
        }

        #region XML Sitemap

        public XmlSitemapProvider PublishXmlSitemap(XmlSitemapBuildContext context)
        {
            if (!context.LoadSetting<SeoSettings>().XmlSitemapIncludesManufacturers)
                return null;

            var query = GetManufacturers(false, context.RequestStoreId).OrderBy(x => x.DisplayOrder).ThenBy(x => x.Name);
            return new ManufacturerXmlSitemapResult { Query = query };
        }

        class ManufacturerXmlSitemapResult : XmlSitemapProvider
        {
            public IQueryable<Manufacturer> Query { get; set; }

            public override int GetTotalCount()
            {
                return Query.Count();
            }

            public override IEnumerable<NamedEntity> Enlist()
            {
                var topics = Query.Select(x => new { x.Id, x.UpdatedOnUtc }).ToList();
                foreach (var x in topics)
                {
                    yield return new NamedEntity { EntityName = "Manufacturer", Id = x.Id, LastMod = x.UpdatedOnUtc };
                }
            }

            public override int Order => int.MinValue + 100;
        }

        #endregion
    }
}
