using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Collections;
using SmartStore.Core;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Security;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Events;
using SmartStore.Data.Caching;
using SmartStore.Services.Customers;
using SmartStore.Services.Localization;
using SmartStore.Services.Search;
using SmartStore.Services.Security;
using SmartStore.Services.Stores;

namespace SmartStore.Services.Catalog
{
	/// <summary>
	/// Category service
	/// </summary>
	public partial class CategoryService : ICategoryService
    {
        private const string CATEGORIES_BY_PARENT_CATEGORY_ID_KEY = "SmartStore.category.byparent-{0}-{1}-{2}-{3}";
		private const string PRODUCTCATEGORIES_ALLBYCATEGORYID_KEY = "SmartStore.productcategory.allbycategoryid-{0}-{1}-{2}-{3}-{4}-{5}";
		private const string PRODUCTCATEGORIES_ALLBYPRODUCTID_KEY = "SmartStore.productcategory.allbyproductid-{0}-{1}-{2}-{3}";
        private const string CATEGORIES_PATTERN_KEY = "SmartStore.category.";
        private const string PRODUCTCATEGORIES_PATTERN_KEY = "SmartStore.productcategory.";

        private readonly IRepository<Category> _categoryRepository;
        private readonly IRepository<ProductCategory> _productCategoryRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<AclRecord> _aclRepository;
		private readonly IRepository<StoreMapping> _storeMappingRepository;
        private readonly IWorkContext _workContext;
		private readonly IStoreContext _storeContext;
        private readonly IEventPublisher _eventPublisher;
        private readonly IRequestCache _requestCache;
		private readonly IStoreMappingService _storeMappingService;
		private readonly IAclService _aclService;
        private readonly Lazy<IEnumerable<ICategoryNavigationFilter>> _navigationFilters;
        private readonly ICustomerService _customerService;
        private readonly IStoreService _storeService;
		private readonly ICatalogSearchService _catalogSearchService;

		public CategoryService(IRequestCache requestCache,
            IRepository<Category> categoryRepository,
            IRepository<ProductCategory> productCategoryRepository,
            IRepository<Product> productRepository,
            IRepository<AclRecord> aclRepository,
			IRepository<StoreMapping> storeMappingRepository,
            IWorkContext workContext,
			IStoreContext storeContext,
            IEventPublisher eventPublisher,
			IStoreMappingService storeMappingService,
			IAclService aclService,
            Lazy<IEnumerable<ICategoryNavigationFilter>> navigationFilters,
            ICustomerService customerService,
            IStoreService storeService,
			ICatalogSearchService catalogSearchService)
        {
            _requestCache = requestCache;
            _categoryRepository = categoryRepository;
            _productCategoryRepository = productCategoryRepository;
            _productRepository = productRepository;
            _aclRepository = aclRepository;
			_storeMappingRepository = storeMappingRepository;
            _workContext = workContext;
			_storeContext = storeContext;
            _eventPublisher = eventPublisher;
			_storeMappingService = storeMappingService;
			_aclService = aclService;
            _navigationFilters = navigationFilters;
            _customerService = customerService;
            _storeService = storeService;
			_catalogSearchService = catalogSearchService;

			QuerySettings = DbQuerySettings.Default;
        }

		public DbQuerySettings QuerySettings { get; set; }

		private void DeleteAllCategories(IList<Category> categories, bool delete)
		{
			foreach (var category in categories)
			{				
				if (delete)
					category.Deleted = true;
				else
					category.ParentCategoryId = 0;

				UpdateCategory(category);

				var childCategories = GetAllCategoriesByParentCategoryId(category.Id, true);
				DeleteAllCategories(childCategories, delete);
            }
		}

        public virtual void InheritAclIntoChildren(
			int categoryId, 
            bool touchProductsWithMultipleCategories = false,
            bool touchExistingAcls = false,
            bool categoriesOnly = false)
        {
            var category = GetCategoryById(categoryId);
            var subcategories = GetAllCategoriesByParentCategoryId(categoryId, true);
			var allCustomerRoles = _customerService.GetAllCustomerRoles(true);
			var categoryCustomerRoles = _aclService.GetCustomerRoleIdsWithAccess(category);

			var categoryIds = new HashSet<int>(subcategories.Select(x => x.Id));
			categoryIds.Add(categoryId);

			var searchQuery = new CatalogSearchQuery()
				.WithCategoryIds(null, categoryIds.ToArray());

			var query = _catalogSearchService.PrepareQuery(searchQuery);
			var products = query.OrderBy(p => p.Id).ToList();

			using (var scope = new DbContextScope(ctx: _aclRepository.Context, autoDetectChanges: false, proxyCreation: false, validateOnSave: false))
            {
                _aclRepository.AutoCommitEnabled = false;

                foreach (var subcategory in subcategories)
                {
                    if (subcategory.SubjectToAcl != category.SubjectToAcl)
                    {
                        subcategory.SubjectToAcl = category.SubjectToAcl;
                        _categoryRepository.Update(subcategory);
                    }

                    var existingAclRecords = _aclService.GetAclRecords(subcategory).ToDictionarySafe(x => x.CustomerRoleId);

                    foreach (var customerRole in allCustomerRoles)
                    {
                        if (categoryCustomerRoles.Contains(customerRole.Id))
                        {
                            if (!existingAclRecords.ContainsKey(customerRole.Id))
                            {
                                _aclRepository.Insert(new AclRecord { CustomerRole = customerRole, CustomerRoleId = customerRole.Id, EntityId = subcategory.Id, EntityName = "Category" });
                            }
                        }
                        else
                        {
                            AclRecord aclRecordToDelete;
                            if (existingAclRecords.TryGetValue(customerRole.Id, out aclRecordToDelete))
                            {
                                _aclRepository.Delete(aclRecordToDelete);
                            }
                        }
                    }
                }
                
                _aclRepository.Context.SaveChanges();

                foreach (var product in products)
                {
                    if (product.SubjectToAcl != category.SubjectToAcl)
                    {
                        product.SubjectToAcl = category.SubjectToAcl;
                        _productRepository.Update(product);
                    }

                    var existingAclRecords = _aclService.GetAclRecords(product).ToDictionarySafe(x => x.CustomerRoleId);

                    foreach (var customerRole in allCustomerRoles)
                    {
                        if (categoryCustomerRoles.Contains(customerRole.Id))
                        {
                            if (!existingAclRecords.ContainsKey(customerRole.Id))
                            {
                                _aclRepository.Insert(new AclRecord { CustomerRole = customerRole, CustomerRoleId = customerRole.Id, EntityId = product.Id, EntityName = "Product" });
                            }
                        }
                        else
                        {
                            AclRecord aclRecordToDelete;
                            if (existingAclRecords.TryGetValue(customerRole.Id, out aclRecordToDelete))
                            {
                                _aclRepository.Delete(aclRecordToDelete);
                            }
                        }
                    }
                }

                _aclRepository.Context.SaveChanges();
            }
        }

        public virtual void InheritStoresIntoChildren(
			int categoryId, 
            bool touchProductsWithMultipleCategories = false,
            bool touchExistingAcls = false,
            bool categoriesOnly = false)
        {
            var category = GetCategoryById(categoryId);
            var subcategories = GetAllCategoriesByParentCategoryId(categoryId, true);
			var allStores = _storeService.GetAllStores();
			var categoryStoreMappings = _storeMappingService.GetStoresIdsWithAccess(category);

			var categoryIds = new HashSet<int>(subcategories.Select(x => x.Id));
			categoryIds.Add(categoryId);

			var searchQuery = new CatalogSearchQuery()
				.WithCategoryIds(null, categoryIds.ToArray());

			var query = _catalogSearchService.PrepareQuery(searchQuery);
			var products = query.OrderBy(p => p.Id).ToList();

            using (var scope = new DbContextScope(ctx: _storeMappingRepository.Context, autoDetectChanges: false, proxyCreation: false, validateOnSave: false))
            {
                _storeMappingRepository.AutoCommitEnabled = false;

                foreach (var subcategory in subcategories)
                {
                    if (subcategory.LimitedToStores != category.LimitedToStores)
                    {
                        subcategory.LimitedToStores = category.LimitedToStores;
                        _categoryRepository.Update(subcategory);
                    }

                    var existingStoreMappingsRecords = _storeMappingService.GetStoreMappings(subcategory).ToDictionary(x => x.StoreId);

                    foreach (var store in allStores)
                    {
                        if (categoryStoreMappings.Contains(store.Id))
                        {
                            if (!existingStoreMappingsRecords.ContainsKey(store.Id))
                            {
                                _storeMappingRepository.Insert(new StoreMapping { StoreId = store.Id, EntityId = subcategory.Id, EntityName = "Category" });
                            }
                        }
                        else
                        {
                            StoreMapping storeMappingToDelete;
                            if (existingStoreMappingsRecords.TryGetValue(store.Id, out storeMappingToDelete))
                            {
                                _storeMappingRepository.Delete(storeMappingToDelete);
                            }
                        }
                    }
                }

                _storeMappingRepository.Context.SaveChanges();

                foreach (var product in products)
                {
                    if (product.LimitedToStores != category.LimitedToStores)
                    {
                        product.LimitedToStores = category.LimitedToStores;
                        _productRepository.Update(product);
                    }

                    var existingStoreMappingsRecords = _storeMappingService.GetStoreMappings(product).ToDictionary(x => x.StoreId);

                    foreach (var store in allStores)
                    {
                        if (categoryStoreMappings.Contains(store.Id))
                        {
                            if (!existingStoreMappingsRecords.ContainsKey(store.Id))
                            {
                                _storeMappingRepository.Insert(new StoreMapping { StoreId = store.Id, EntityId = product.Id, EntityName = "Product" });
                            }
                        }
                        else
                        {
                            StoreMapping storeMappingToDelete;
                            if (existingStoreMappingsRecords.TryGetValue(store.Id, out storeMappingToDelete))
                            {
                                _storeMappingRepository.Delete(storeMappingToDelete);
                            }
                        }
                    }
                }

                _storeMappingRepository.Context.SaveChanges();
            }
        }

		public virtual void DeleteCategory(Category category, bool deleteChilds = false)
        {
            if (category == null)
                throw new ArgumentNullException("category");

            category.Deleted = true;
            UpdateCategory(category);

			var childCategories = GetAllCategoriesByParentCategoryId(category.Id, true);
			DeleteAllCategories(childCategories, deleteChilds);
        }

		public virtual IQueryable<Category> GetCategories(
			string categoryName = "",
			bool showHidden = false,
			string alias = null,
			bool applyNavigationFilters = true,
			int storeId = 0)
		{
			var query = _categoryRepository.Table;

			if (!showHidden)
				query = query.Where(c => c.Published);

			if (categoryName.HasValue())
				query = query.Where(c => c.Name.Contains(categoryName) || c.FullName.Contains(categoryName));

			if (alias.HasValue())
				query = query.Where(c => c.Alias.Contains(alias));

			if (showHidden)
			{
				if (!QuerySettings.IgnoreMultiStore && storeId > 0)
				{
					query = from c in query
							join sm in _storeMappingRepository.Table
							on new { c1 = c.Id, c2 = "Category" } equals new { c1 = sm.EntityId, c2 = sm.EntityName } into c_sm
							from sm in c_sm.DefaultIfEmpty()
							where !c.LimitedToStores || storeId == sm.StoreId
							select c;

					query = from c in query
							group c by c.Id into cGroup
							orderby cGroup.Key
							select cGroup.FirstOrDefault();
				}
			}
			else
			{
				query = ApplyHiddenCategoriesFilter(query, applyNavigationFilters, _storeContext.CurrentStore.Id);
			}

			query = query.Where(c => !c.Deleted);

			return query;
		}
        
        public virtual IPagedList<Category> GetAllCategories(string categoryName = "", int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, string alias = null,
			bool applyNavigationFilters = true, bool ignoreCategoriesWithoutExistingParent = true, int storeId = 0)
        {
			var query = GetCategories(categoryName, showHidden, alias, applyNavigationFilters, storeId);

			query = query
				.OrderBy(x => x.ParentCategoryId)
				.ThenBy(x => x.DisplayOrder)
				.ThenBy(x => x.Name);

            var unsortedCategories = query.ToList();

            // sort categories
            var sortedCategories = unsortedCategories.SortCategoriesForTree(ignoreCategoriesWithoutExistingParent: ignoreCategoriesWithoutExistingParent);

            // paging
            return new PagedList<Category>(sortedCategories, pageIndex, pageSize);
        }

        public IList<Category> GetAllCategoriesByParentCategoryId(int parentCategoryId, bool showHidden = false)
        {
			int storeId = _storeContext.CurrentStore.Id;
			string key = string.Format(CATEGORIES_BY_PARENT_CATEGORY_ID_KEY, parentCategoryId, showHidden, _workContext.CurrentCustomer.Id, storeId);
            return _requestCache.Get(key, () =>
            {
                var query = _categoryRepository.Table;

                if (!showHidden)
                    query = query.Where(c => c.Published);

                query = query.Where(c => c.ParentCategoryId == parentCategoryId);
                query = query.Where(c => !c.Deleted);
                query = query.OrderBy(c => c.DisplayOrder);

                if (!showHidden)
                {
					query = ApplyHiddenCategoriesFilter(query, false, storeId);
					query = query.OrderBy(c => c.DisplayOrder);
                }

                var categories = query.ToList();
                return categories;
            });
        }

        protected virtual IQueryable<Category> ApplyHiddenCategoriesFilter(IQueryable<Category> query, bool applyNavigationFilters, int storeId = 0)
        {
            // ACL (access control list)
			if (!QuerySettings.IgnoreAcl)
			{
				var allowedCustomerRolesIds = _workContext.CurrentCustomer.CustomerRoles.Where(x => x.Active).Select(x => x.Id).ToList();

				query = from c in query
						join acl in _aclRepository.Table
						on new { c1 = c.Id, c2 = "Category" } equals new { c1 = acl.EntityId, c2 = acl.EntityName } into c_acl
						from acl in c_acl.DefaultIfEmpty()
						where !c.SubjectToAcl || allowedCustomerRolesIds.Contains(acl.CustomerRoleId)
						select c;
			}

			// Store mapping
			if (!QuerySettings.IgnoreMultiStore && storeId > 0)
			{
				query = from c in query
						join sm in _storeMappingRepository.Table
						on new { c1 = c.Id, c2 = "Category" } equals new { c1 = sm.EntityId, c2 = sm.EntityName } into c_sm
						from sm in c_sm.DefaultIfEmpty()
						where !c.LimitedToStores || storeId == sm.StoreId
						select c;
			}

            // only distinct categories (group by ID)
            query = from c in query
                    group c by c.Id into cGroup
                    orderby cGroup.Key
                    select cGroup.FirstOrDefault();

            if (applyNavigationFilters)
            {
                var filters = _navigationFilters.Value;
                if (filters.Any())
                {
                    filters.Each(x => {
                        query = x.Apply(query);
                    });
                }
            }

			return query;
        }
        
        public virtual IList<Category> GetAllCategoriesDisplayedOnHomePage()
        {
            var query = from c in _categoryRepository.Table
                        orderby c.DisplayOrder
                        where c.Published &&
                        !c.Deleted && 
                        c.ShowOnHomePage
                        select c;

            var categories = query.ToList();
            return categories;
        }
                
        public virtual Category GetCategoryById(int categoryId)
        {
            if (categoryId == 0)
                return null;

			return _categoryRepository.GetByIdCached(categoryId, "db.category.id-" + categoryId);
		}

        public virtual void InsertCategory(Category category)
        {
            if (category == null)
                throw new ArgumentNullException("category");

            _categoryRepository.Insert(category);

            //cache
            _requestCache.RemoveByPattern(CATEGORIES_PATTERN_KEY);
            _requestCache.RemoveByPattern(PRODUCTCATEGORIES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(category);
        }

        public virtual void UpdateCategory(Category category)
        {
            if (category == null)
                throw new ArgumentNullException("category");

            //validate category hierarchy
            var parentCategory = GetCategoryById(category.ParentCategoryId);
            while (parentCategory != null)
            {
                if (category.Id == parentCategory.Id)
                {
                    category.ParentCategoryId = 0;
                    break;
                }
                parentCategory = GetCategoryById(parentCategory.ParentCategoryId);
            }

            _categoryRepository.Update(category);

            //cache
            _requestCache.RemoveByPattern(CATEGORIES_PATTERN_KEY);
            _requestCache.RemoveByPattern(PRODUCTCATEGORIES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityUpdated(category);
        }
        
        public virtual void UpdateHasDiscountsApplied(Category category)
        {
            if (category == null)
                throw new ArgumentNullException("category");

            category.HasDiscountsApplied = category.AppliedDiscounts.Count > 0;
            UpdateCategory(category);
        }

        public virtual void DeleteProductCategory(ProductCategory productCategory)
        {
            if (productCategory == null)
                throw new ArgumentNullException("productCategory");

            _productCategoryRepository.Delete(productCategory);

            //cache
            _requestCache.RemoveByPattern(CATEGORIES_PATTERN_KEY);
            _requestCache.RemoveByPattern(PRODUCTCATEGORIES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityDeleted(productCategory);
        }

        public virtual IPagedList<ProductCategory> GetProductCategoriesByCategoryId(int categoryId, int pageIndex, int pageSize, bool showHidden = false)
        {
            if (categoryId == 0)
                return new PagedList<ProductCategory>(new List<ProductCategory>(), pageIndex, pageSize);

			int storeId = _storeContext.CurrentStore.Id;
			string key = string.Format(PRODUCTCATEGORIES_ALLBYCATEGORYID_KEY, showHidden, categoryId, pageIndex, pageSize, _workContext.CurrentCustomer.Id, storeId);

            return _requestCache.Get(key, () =>
            {
                var query = from pc in _productCategoryRepository.Table
                            join p in _productRepository.Table on pc.ProductId equals p.Id
                            where pc.CategoryId == categoryId && !p.Deleted && (showHidden || p.Published)
                            select pc;

                if (!showHidden)
                {
                    query = ApplyHiddenProductCategoriesFilter(query, storeId);
                }

				query = query
					.OrderBy(pc => pc.DisplayOrder)
					.ThenBy(pc => pc.Id);	// required for paging!

                var productCategories = new PagedList<ProductCategory>(query, pageIndex, pageSize);

                return productCategories;
            });
        }

        public virtual IList<ProductCategory> GetProductCategoriesByProductId(int productId, bool showHidden = false)
        {
            if (productId == 0)
                return new List<ProductCategory>();

			string key = string.Format(PRODUCTCATEGORIES_ALLBYPRODUCTID_KEY, showHidden, productId, _workContext.CurrentCustomer.Id, _storeContext.CurrentStore.Id);
            return _requestCache.Get(key, () =>
            {
				var query = from pc in _productCategoryRepository.Table.Expand(x => x.Category)
                            join c in _categoryRepository.Table on pc.CategoryId equals c.Id
                            where pc.ProductId == productId &&
                                  !c.Deleted &&
                                  (showHidden || c.Published)
                            orderby pc.DisplayOrder
                            select pc;

				var allProductCategories = query.ToList();
				var result = new List<ProductCategory>();
				if (!showHidden)
				{
					foreach (var pc in allProductCategories)
					{
						// ACL (access control list) and store mapping
						var category = pc.Category;
						if (_aclService.Authorize(category) && _storeMappingService.Authorize(category))
							result.Add(pc);
					}
				}
				else
				{
					// No filtering
					result.AddRange(allProductCategories);
				}
				return result;
            });
        }

		public virtual Multimap<int, ProductCategory> GetProductCategoriesByProductIds(int[] productIds, bool? hasDiscountsApplied = null, bool showHidden = false)
		{
			Guard.NotNull(productIds, nameof(productIds));

			var query = 
				from pc in _productCategoryRepository.TableUntracked.Expand(x => x.Category).Expand(x => x.Category.Picture)
				join c in _categoryRepository.Table on pc.CategoryId equals c.Id
				where productIds.Contains(pc.ProductId) && !c.Deleted && (showHidden || c.Published)
				orderby pc.DisplayOrder
				select pc;

			if (hasDiscountsApplied.HasValue)
			{
				query = query.Where(x => x.Category.HasDiscountsApplied == hasDiscountsApplied);
			}

			var list = query.ToList();

			if (!showHidden)
			{
				list = list.Where(x => _aclService.Authorize(x.Category) && _storeMappingService.Authorize(x.Category)).ToList();
			}

			var map = list.ToMultimap(x => x.ProductId, x => x);
				
			return map;
		}

		public virtual Multimap<int, ProductCategory> GetProductCategoriesByCategoryIds(int[] categoryIds)
		{
			Guard.NotNull(categoryIds, nameof(categoryIds));

			var query = _productCategoryRepository.TableUntracked
				.Where(x => categoryIds.Contains(x.CategoryId))
				.OrderBy(x => x.DisplayOrder);

			var map = query
				.ToList()
				.ToMultimap(x => x.CategoryId, x => x);

			return map;
		}

		protected virtual IQueryable<ProductCategory> ApplyHiddenProductCategoriesFilter(IQueryable<ProductCategory> query, int storeId = 0)
        {
			bool group = false;

            //ACL (access control list)
			if (!QuerySettings.IgnoreAcl)
			{
				group = true;
				var allowedCustomerRolesIds = _workContext.CurrentCustomer.CustomerRoles.Where(cr => cr.Active).Select(cr => cr.Id).ToList();

				query = from pc in query
						join c in _categoryRepository.Table on pc.CategoryId equals c.Id
						join acl in _aclRepository.Table
						on new { c1 = c.Id, c2 = "Category" } equals new { c1 = acl.EntityId, c2 = acl.EntityName } into c_acl
						from acl in c_acl.DefaultIfEmpty()
						where !c.SubjectToAcl || allowedCustomerRolesIds.Contains(acl.CustomerRoleId)
						select pc;
			}

            //Store mapping
			if (!QuerySettings.IgnoreMultiStore && storeId > 0)
			{
				group = true;
				query = from pc in query
						join c in _categoryRepository.Table on pc.CategoryId equals c.Id
						join sm in _storeMappingRepository.Table
						on new { c1 = c.Id, c2 = "Category" } equals new { c1 = sm.EntityId, c2 = sm.EntityName } into c_sm
						from sm in c_sm.DefaultIfEmpty()
						where !c.LimitedToStores || storeId == sm.StoreId
						select pc;
			}

			if (group)
			{
				//only distinct categories (group by ID)
				query = from pc in query
						group pc by pc.Id into pcGroup
						orderby pcGroup.Key
						select pcGroup.FirstOrDefault();
			}

			return query;
        }

        public virtual ProductCategory GetProductCategoryById(int productCategoryId)
        {
            if (productCategoryId == 0)
                return null;

            return _productCategoryRepository.GetById(productCategoryId);
        }

        public virtual void InsertProductCategory(ProductCategory productCategory)
        {
            if (productCategory == null)
                throw new ArgumentNullException("productCategory");
            
            _productCategoryRepository.Insert(productCategory);

            //cache
            _requestCache.RemoveByPattern(CATEGORIES_PATTERN_KEY);
            _requestCache.RemoveByPattern(PRODUCTCATEGORIES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(productCategory);
        }

        public virtual void UpdateProductCategory(ProductCategory productCategory)
        {
            if (productCategory == null)
                throw new ArgumentNullException("productCategory");

            _productCategoryRepository.Update(productCategory);

            //cache
            _requestCache.RemoveByPattern(CATEGORIES_PATTERN_KEY);
            _requestCache.RemoveByPattern(PRODUCTCATEGORIES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityUpdated(productCategory);
        }

		public virtual ICollection<Category> GetCategoryTrail(Category category)
		{
			Guard.NotNull(category, nameof(category));

			var trail = new List<Category>(10);

			do
			{
				trail.Add(category);
				category = GetCategoryById(category.ParentCategoryId);
			}
			while (category != null && !category.Deleted && category.Published);

			trail.Reverse();
			return trail;
		}

		public virtual string GetCategoryPath(
			Product product, 
			int? languageId, 
			Func<int, string> pathLookup,
			Action<int, string> addPathToCache, 
			Func<int, Category> categoryLookup,
			ProductCategory prodCategory = null)
		{
			if (product == null)
				return string.Empty;

			pathLookup = pathLookup ?? ((i) => { return string.Empty; });
			categoryLookup = categoryLookup ?? ((i) => { return GetCategoryById(i); });
			addPathToCache = addPathToCache ?? ((i, val) => { });

			var alreadyProcessedCategoryIds = new List<int>();
			var path = new List<string>();

			var productCategory = prodCategory ?? GetProductCategoriesByProductId(product.Id).FirstOrDefault();

			if (productCategory != null && productCategory.Category != null)
			{
				string cached = pathLookup(productCategory.CategoryId);
				if (cached.HasValue()) 
				{
					return cached;
				}

				var category = productCategory.Category;

				path.Add(languageId.HasValue ? category.GetLocalized(x => x.Name, languageId.Value) : category.Name);
				alreadyProcessedCategoryIds.Add(category.Id);

				category = categoryLookup(category.ParentCategoryId);
				while (category != null && !category.Deleted && category.Published && !alreadyProcessedCategoryIds.Contains(category.Id))
				{
					path.Add(languageId.HasValue ? category.GetLocalized(x => x.Name, languageId.Value) : category.Name);
					alreadyProcessedCategoryIds.Add(category.Id);
					category = categoryLookup(category.ParentCategoryId);
				}

				path.Reverse();
				string result = String.Join(" > ", path);
				addPathToCache(productCategory.CategoryId, result);
				return result;
			}

			return string.Empty;
		}
    }
}
