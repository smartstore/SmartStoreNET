using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using SmartStore.Core;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Security;
using SmartStore.Core.Domain.Stores;
using SmartStore.Data;
using SmartStore.Services.Events;
using SmartStore.Services.Localization;
using SmartStore.Services.Messages;

namespace SmartStore.Services.Catalog
{
    /// <summary>
    /// Product service
    /// </summary>
    public partial class ProductService : IProductService
    {
        #region Constants
        private const string PRODUCTS_BY_ID_KEY = "SmartStore.product.id-{0}";
        private const string PRODUCTVARIANTS_ALL_KEY = "SmartStore.productvariant.all-{0}-{1}";
        private const string PRODUCTVARIANTS_BY_ID_KEY = "SmartStore.productvariant.id-{0}";
        private const string PRODUCTS_PATTERN_KEY = "SmartStore.product.";
        private const string PRODUCTVARIANTS_PATTERN_KEY = "SmartStore.productvariant.";
        private const string TIERPRICES_PATTERN_KEY = "SmartStore.tierprice.";
        #endregion

        #region Fields

        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<ProductVariant> _productVariantRepository;
        private readonly IRepository<RelatedProduct> _relatedProductRepository;
        private readonly IRepository<CrossSellProduct> _crossSellProductRepository;
        private readonly IRepository<TierPrice> _tierPriceRepository;
        private readonly IRepository<LocalizedProperty> _localizedPropertyRepository;
        private readonly IRepository<AclRecord> _aclRepository;
		private readonly IRepository<StoreMapping> _storeMappingRepository;
        private readonly IRepository<ProductPicture> _productPictureRepository;
        private readonly IRepository<ProductSpecificationAttribute> _productSpecificationAttributeRepository;
        private readonly IRepository<ProductVariantAttributeCombination> _productVariantAttributeCombinationRepository; // codehint: sm-add
        private readonly IProductAttributeService _productAttributeService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly ILanguageService _languageService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IDataProvider _dataProvider;
        private readonly IDbContext _dbContext;
        private readonly ICacheManager _cacheManager;
        private readonly IWorkContext _workContext;
		private readonly IStoreContext _storeContext;
        private readonly LocalizationSettings _localizationSettings;
        private readonly CommonSettings _commonSettings;
        private readonly IEventPublisher _eventPublisher;

		private List<int> _allowedRoleIds;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="productRepository">Product repository</param>
        /// <param name="productVariantRepository">Product variant repository</param>
        /// <param name="relatedProductRepository">Related product repository</param>
        /// <param name="crossSellProductRepository">Cross-sell product repository</param>
        /// <param name="tierPriceRepository">Tier price repository</param>
        /// <param name="localizedPropertyRepository">Localized property repository</param>
        /// <param name="aclRepository">ACL record repository</param>
		/// <param name="storeMappingRepository">Store mapping repository</param>
        /// <param name="productPictureRepository">Product picture repository</param>
        /// <param name="productSpecificationAttributeRepository">Product specification attribute repository</param>
        /// <param name="productAttributeService">Product attribute service</param>
        /// <param name="productAttributeParser">Product attribute parser service</param>
        /// <param name="languageService">Language service</param>
        /// <param name="workflowMessageService">Workflow message service</param>
        /// <param name="dataProvider">Data provider</param>
        /// <param name="dbContext">Database Context</param>
        /// <param name="workContext">Work context</param>
		/// <param name="storeContext">Store context</param>
        /// <param name="localizationSettings">Localization settings</param>
        /// <param name="commonSettings">Common settings</param>
        /// <param name="eventPublisher">Event published</param>
        public ProductService(ICacheManager cacheManager,
            IRepository<Product> productRepository,
            IRepository<ProductVariant> productVariantRepository,
            IRepository<RelatedProduct> relatedProductRepository,
            IRepository<CrossSellProduct> crossSellProductRepository,
            IRepository<TierPrice> tierPriceRepository,
            IRepository<ProductPicture> productPictureRepository,
            IRepository<LocalizedProperty> localizedPropertyRepository,
            IRepository<AclRecord> aclRepository,
			IRepository<StoreMapping> storeMappingRepository,
            IRepository<ProductSpecificationAttribute> productSpecificationAttributeRepository,
            IRepository<ProductVariantAttributeCombination> productVariantAttributeCombinationRepository,
            IProductAttributeService productAttributeService,
            IProductAttributeParser productAttributeParser,
            ILanguageService languageService,
            IWorkflowMessageService workflowMessageService,
            IDataProvider dataProvider, IDbContext dbContext,
            IWorkContext workContext,
			IStoreContext storeContext,
            LocalizationSettings localizationSettings, CommonSettings commonSettings,
            IEventPublisher eventPublisher)
        {
            this._cacheManager = cacheManager;
            this._productRepository = productRepository;
            this._productVariantRepository = productVariantRepository;
            this._relatedProductRepository = relatedProductRepository;
            this._crossSellProductRepository = crossSellProductRepository;
            this._tierPriceRepository = tierPriceRepository;
            this._productPictureRepository = productPictureRepository;
            this._localizedPropertyRepository = localizedPropertyRepository;
            this._aclRepository = aclRepository;
			this._storeMappingRepository = storeMappingRepository;
            this._productSpecificationAttributeRepository = productSpecificationAttributeRepository;
            this._productVariantAttributeCombinationRepository = productVariantAttributeCombinationRepository;
            this._productAttributeService = productAttributeService;
            this._productAttributeParser = productAttributeParser;
            this._languageService = languageService;
            this._workflowMessageService = workflowMessageService;
            this._dataProvider = dataProvider;
            this._dbContext = dbContext;
            this._workContext = workContext;
			this._storeContext = storeContext;
            this._localizationSettings = localizationSettings;
            this._commonSettings = commonSettings;
            this._eventPublisher = eventPublisher;
        }

        #endregion
        
		private List<int> AllowedRoleIds {
			get {
				if (_allowedRoleIds == null) {
					_allowedRoleIds = _workContext.CurrentCustomer.CustomerRoles
						.Where(cr => cr.Active)
						.Select(cr => cr.Id)
						.ToList();	// TODO: cache that on session basis
				}
				return _allowedRoleIds;
			}
		}

        #region Methods

        #region Products

        /// <summary>
        /// Delete a product
        /// </summary>
        /// <param name="product">Product</param>
        public virtual void DeleteProduct(Product product)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            product.Deleted = true;
            //delete product
            UpdateProduct(product);

            //delete product variants
            foreach (var productVariant in product.ProductVariants)
                DeleteProductVariant(productVariant);
        }

		/// <remarks>codehint: sm-add</remarks>
		public virtual IQueryable<Product> GetAllProducts(List<int> categoryIds, bool? includeFeatured, int storeId = 0)
		{
			var allowedRoleIds = AllowedRoleIds;

			var query =
				from p in _productRepository.Table
				join acl in _aclRepository.Table on p.Id equals acl.EntityId into p_acl
				from acl in p_acl.DefaultIfEmpty()
				where p.Published && !p.Deleted && (!p.SubjectToAcl || (acl.EntityName == "Product" && allowedRoleIds.Contains(acl.CustomerRoleId)))
				select p;

			if (storeId > 0)
			{
				query = 
					from p in query
					join sm in _storeMappingRepository.Table on p.Id equals sm.EntityId into p_sm
					from sm in p_sm.DefaultIfEmpty()
					where !p.LimitedToStores || (sm.EntityName == "Product" && storeId == sm.StoreId)
					select p;
			}

			if (categoryIds != null && categoryIds.Count > 0)
			{
				query =
					from p in query
					from pc in p.ProductCategories.Where(pc => categoryIds.Contains(pc.CategoryId))
					where (!includeFeatured.HasValue || includeFeatured.Value == pc.IsFeaturedProduct)
					select p;
			}
			return query;
		}


        /// <summary>
        /// Gets all products displayed on the home page
        /// </summary>
        /// <returns>Product collection</returns>
        public virtual IList<Product> GetAllProductsDisplayedOnHomePage()
        {
            var query = from p in _productRepository.Table
                        orderby p.Name
                        where p.Published &&
                        !p.Deleted &&
                        p.ShowOnHomePage
                        select p;
            var products = query.ToList();
            return products;
        }
        
        /// <summary>
        /// Gets product
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <returns>Product</returns>
        public virtual Product GetProductById(int productId)
        {
            if (productId == 0)
                return null;

            string key = string.Format(PRODUCTS_BY_ID_KEY, productId);
            return _cacheManager.Get(key, () =>
            {
                var product = _productRepository.GetById(productId);
                return product;
            });
        }

        /// <summary>
        /// Get products by identifiers
        /// </summary>
        /// <param name="productIds">Product identifiers</param>
        /// <returns>Products</returns>
        public virtual IList<Product> GetProductsByIds(int[] productIds)
        {
            if (productIds == null || productIds.Length == 0)
                return new List<Product>();

            var query = from p in _productRepository.Table
                        where productIds.Contains(p.Id)
                        select p;
            var products = query.ToList();
            //sort by passed identifiers
            var sortedProducts = new List<Product>();
            foreach (int id in productIds)
            {
                var product = products.Find(x => x.Id == id);
                if (product != null)
                    sortedProducts.Add(product);
            }
            return sortedProducts;
        }

        /// <summary>
        /// Inserts a product
        /// </summary>
        /// <param name="product">Product</param>
        public virtual void InsertProduct(Product product)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            //insert
            _productRepository.Insert(product);

            //clear cache
            _cacheManager.RemoveByPattern(PRODUCTS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTVARIANTS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(TIERPRICES_PATTERN_KEY);
            
            //event notification
            _eventPublisher.EntityInserted(product);
        }

        /// <summary>
        /// Updates the product
        /// </summary>
        /// <param name="product">Product</param>
        public virtual void UpdateProduct(Product product)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            //update
            _productRepository.Update(product);

            //cache
            _cacheManager.RemoveByPattern(PRODUCTS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTVARIANTS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(TIERPRICES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityUpdated(product);
        }


        public virtual int CountProducts(ProductSearchContext ctx)
        {
            Guard.ArgumentNotNull(() => ctx);

            var query = this.PrepareProductSearchQuery(ctx);
            return query.Distinct().Count();
        }

        /// <summary>
        /// Search products
        /// <remarks>codehint: sm-edit (changed input parameter)</remarks>
        /// </summary>
        public virtual IPagedList<Product> SearchProducts(ProductSearchContext ctx)
        {
            // codehint: sm-edit (turn off nc filtering, we have our own)
            ctx.LoadFilterableSpecificationAttributeOptionIds = false;

            ctx.FilterableSpecificationAttributeOptionIds = new List<int>();

			//search by keyword
            bool searchLocalizedValue = false;
            if (ctx.LanguageId > 0)
            {
                if (ctx.ShowHidden)
                {
                    searchLocalizedValue = true;
                }
                else
                {
                    //ensure that we have at least two published languages
					var totalPublishedLanguages = _languageService.GetAllLanguages(storeId: ctx.StoreId).Count;
                    searchLocalizedValue = totalPublishedLanguages >= 2;
                }
            }

			//validate "categoryIds" parameter
			if (ctx.CategoryIds != null && ctx.CategoryIds.Contains(0))
				ctx.CategoryIds.Remove(0);

            //Access control list. Allowed customer roles
            var allowedCustomerRolesIds = _workContext.CurrentCustomer.CustomerRoles
                .Where(cr => cr.Active).Select(cr => cr.Id).ToList();

            if (_commonSettings.UseStoredProceduresIfSupported && _dataProvider.StoredProceduredSupported)
            {
                //stored procedures are enabled and supported by the database. 
                //It's much faster than the LINQ implementation below 

                #region Use stored procedure

                //pass categry identifiers as comma-delimited string
                string commaSeparatedCategoryIds = "";
                if (ctx.CategoryIds != null)
                {
                    for (int i = 0; i < ctx.CategoryIds.Count; i++)
                    {
                        commaSeparatedCategoryIds += ctx.CategoryIds[i].ToString();
                        if (i != ctx.CategoryIds.Count - 1)
                        {
                            commaSeparatedCategoryIds += ",";
                        }
                    }
                }

                //pass customer role identifiers as comma-delimited string
                string commaSeparatedAllowedCustomerRoleIds = "";
                for (int i = 0; i < allowedCustomerRolesIds.Count; i++)
                {
                    commaSeparatedAllowedCustomerRoleIds += allowedCustomerRolesIds[i].ToString();
                    if (i != allowedCustomerRolesIds.Count - 1)
                    {
                        commaSeparatedAllowedCustomerRoleIds += ",";
                    }
                }

                //pass specification identifiers as comma-delimited string
                string commaSeparatedSpecIds = "";
                if (ctx.FilteredSpecs != null)
                {
                    ((List<int>)ctx.FilteredSpecs).Sort();
                    for (int i = 0; i < ctx.FilteredSpecs.Count; i++)
                    {
                        commaSeparatedSpecIds += ctx.FilteredSpecs[i].ToString();
                        if (i != ctx.FilteredSpecs.Count - 1)
                        {
                            commaSeparatedSpecIds += ",";
                        }
                    }
                }

                //some databases don't support int.MaxValue
                if (ctx.PageSize == int.MaxValue)
                    ctx.PageSize = int.MaxValue - 1;

                //prepare parameters
                var pCategoryIds = _dataProvider.GetParameter();
                pCategoryIds.ParameterName = "CategoryIds";
                pCategoryIds.Value = commaSeparatedCategoryIds != null ? (object)commaSeparatedCategoryIds : DBNull.Value;
                pCategoryIds.DbType = DbType.String;

                var pManufacturerId = _dataProvider.GetParameter();
                pManufacturerId.ParameterName = "ManufacturerId";
                pManufacturerId.Value = ctx.ManufacturerId;
                pManufacturerId.DbType = DbType.Int32;

				var pStoreId = _dataProvider.GetParameter();
				pStoreId.ParameterName = "StoreId";
				pStoreId.Value = ctx.StoreId;
				pStoreId.DbType = DbType.Int32;

                var pProductTagId = _dataProvider.GetParameter();
                pProductTagId.ParameterName = "ProductTagId";
                pProductTagId.Value = ctx.ProductTagId;
                pProductTagId.DbType = DbType.Int32;

                var pFeaturedProducts = _dataProvider.GetParameter();
                pFeaturedProducts.ParameterName = "FeaturedProducts";
                pFeaturedProducts.Value = ctx.FeaturedProducts.HasValue ? (object)ctx.FeaturedProducts.Value : DBNull.Value;
                pFeaturedProducts.DbType = DbType.Boolean;

                var pPriceMin = _dataProvider.GetParameter();
                pPriceMin.ParameterName = "PriceMin";
                pPriceMin.Value = ctx.PriceMin.HasValue ? (object)ctx.PriceMin.Value : DBNull.Value;
                pPriceMin.DbType = DbType.Decimal;

                var pPriceMax = _dataProvider.GetParameter();
                pPriceMax.ParameterName = "PriceMax";
                pPriceMax.Value = ctx.PriceMax.HasValue ? (object)ctx.PriceMax.Value : DBNull.Value;
                pPriceMax.DbType = DbType.Decimal;

                var pKeywords = _dataProvider.GetParameter();
                pKeywords.ParameterName = "Keywords";
                pKeywords.Value = ctx.Keywords != null ? (object)ctx.Keywords : DBNull.Value;
                pKeywords.DbType = DbType.String;

                var pSearchDescriptions = _dataProvider.GetParameter();
                pSearchDescriptions.ParameterName = "SearchDescriptions";
                pSearchDescriptions.Value = ctx.SearchDescriptions;
                pSearchDescriptions.DbType = DbType.Boolean;

                var pSearchProductTags = _dataProvider.GetParameter();
                pSearchProductTags.ParameterName = "SearchProductTags";
                pSearchProductTags.Value = ctx.SearchDescriptions;
                pSearchProductTags.DbType = DbType.Boolean;

                var pUseFullTextSearch = _dataProvider.GetParameter();
                pUseFullTextSearch.ParameterName = "UseFullTextSearch";
                pUseFullTextSearch.Value = _commonSettings.UseFullTextSearch;
                pUseFullTextSearch.DbType = DbType.Boolean;

                var pFullTextMode = _dataProvider.GetParameter();
                pFullTextMode.ParameterName = "FullTextMode";
                pFullTextMode.Value = (int)_commonSettings.FullTextMode;
                pFullTextMode.DbType = DbType.Int32;

                var pFilteredSpecs = _dataProvider.GetParameter();
                pFilteredSpecs.ParameterName = "FilteredSpecs";
                pFilteredSpecs.Value = commaSeparatedSpecIds != null ? (object)commaSeparatedSpecIds : DBNull.Value;
                pFilteredSpecs.DbType = DbType.String;

                var pLanguageId = _dataProvider.GetParameter();
                pLanguageId.ParameterName = "LanguageId";
                pLanguageId.Value = searchLocalizedValue ? ctx.LanguageId : 0;
                pLanguageId.DbType = DbType.Int32;

                var pOrderBy = _dataProvider.GetParameter();
                pOrderBy.ParameterName = "OrderBy";
                pOrderBy.Value = (int)ctx.OrderBy;
                pOrderBy.DbType = DbType.Int32;

				var pAllowedCustomerRoleIds = _dataProvider.GetParameter();
				pAllowedCustomerRoleIds.ParameterName = "AllowedCustomerRoleIds";
				pAllowedCustomerRoleIds.Value = commaSeparatedAllowedCustomerRoleIds;
				pAllowedCustomerRoleIds.DbType = DbType.String;

                var pPageIndex = _dataProvider.GetParameter();
                pPageIndex.ParameterName = "PageIndex";
                pPageIndex.Value = ctx.PageIndex;
                pPageIndex.DbType = DbType.Int32;

                var pPageSize = _dataProvider.GetParameter();
                pPageSize.ParameterName = "PageSize";
                pPageSize.Value = ctx.PageSize;
                pPageSize.DbType = DbType.Int32;

                var pShowHidden = _dataProvider.GetParameter();
                pShowHidden.ParameterName = "ShowHidden";
                pShowHidden.Value = ctx.ShowHidden;
                pShowHidden.DbType = DbType.Boolean;

                var pLoadFilterableSpecificationAttributeOptionIds = _dataProvider.GetParameter();
                pLoadFilterableSpecificationAttributeOptionIds.ParameterName = "LoadFilterableSpecificationAttributeOptionIds";
                pLoadFilterableSpecificationAttributeOptionIds.Value = ctx.LoadFilterableSpecificationAttributeOptionIds;
                pLoadFilterableSpecificationAttributeOptionIds.DbType = DbType.Boolean;

                var pFilterableSpecificationAttributeOptionIds = _dataProvider.GetParameter();
                pFilterableSpecificationAttributeOptionIds.ParameterName = "FilterableSpecificationAttributeOptionIds";
                pFilterableSpecificationAttributeOptionIds.Direction = ParameterDirection.Output;
                pFilterableSpecificationAttributeOptionIds.Size = int.MaxValue - 1;
                pFilterableSpecificationAttributeOptionIds.DbType = DbType.String;

                var pTotalRecords = _dataProvider.GetParameter();
                pTotalRecords.ParameterName = "TotalRecords";
                pTotalRecords.Direction = ParameterDirection.Output;
                pTotalRecords.DbType = DbType.Int32;

                //invoke stored procedure
                var products = _dbContext.ExecuteStoredProcedureList<Product>(
                    "ProductLoadAllPaged",
                    pCategoryIds,
                    pManufacturerId,
					pStoreId,
                    pProductTagId,
                    pFeaturedProducts,
                    pPriceMin,
                    pPriceMax,
                    pKeywords,
                    pSearchDescriptions,
                    pSearchProductTags,
                    pUseFullTextSearch,
                    pFullTextMode,
                    pFilteredSpecs,
                    pLanguageId,
                    pOrderBy,
					pAllowedCustomerRoleIds,
                    pPageIndex,
                    pPageSize,
                    pShowHidden,
                    pLoadFilterableSpecificationAttributeOptionIds,
                    pFilterableSpecificationAttributeOptionIds,
                    pTotalRecords);
                //get filterable specification attribute option identifier
                string filterableSpecificationAttributeOptionIdsStr = (pFilterableSpecificationAttributeOptionIds.Value != DBNull.Value) ? (string)pFilterableSpecificationAttributeOptionIds.Value : "";
                if (ctx.LoadFilterableSpecificationAttributeOptionIds &&
                    !string.IsNullOrWhiteSpace(filterableSpecificationAttributeOptionIdsStr))
                {
                    // codehint: sm-edit
                    ctx.FilterableSpecificationAttributeOptionIds.AddRange(filterableSpecificationAttributeOptionIdsStr
                       .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                       .Select(x => Convert.ToInt32(x.Trim())));
                }
                //return products
                int totalRecords = (pTotalRecords.Value != DBNull.Value) ? Convert.ToInt32(pTotalRecords.Value) : 0;
                return new PagedList<Product>(products, ctx.PageIndex, ctx.PageSize, totalRecords);

                #endregion
            }
            else
            {
                //stored procedures aren't supported. Use LINQ

                #region Search products

                var query = this.PrepareProductSearchQuery(ctx, allowedCustomerRolesIds, searchLocalizedValue);

                // only distinct products (group by ID)
                // if we use standard Distinct() method, then all fields will be compared (low performance)
                // it'll not work in SQL Server Compact when searching products by a keyword)
                query = from p in query
                        group p by p.Id into pGroup
						orderby pGroup.Key
						select pGroup.FirstOrDefault();

                //sort products
                if (ctx.OrderBy == ProductSortingEnum.Position && ctx.CategoryIds != null && ctx.CategoryIds.Count > 0)
                {
                    //category position
                    var firstCategoryId = ctx.CategoryIds[0];
                    query = query.OrderBy(p => p.ProductCategories.Where(pc => pc.CategoryId == firstCategoryId).FirstOrDefault().DisplayOrder);
                }
                else if (ctx.OrderBy == ProductSortingEnum.Position && ctx.ManufacturerId > 0)
                {
                    //manufacturer position
                    query = query.OrderBy(p => p.ProductManufacturers.Where(pm => pm.ManufacturerId == ctx.ManufacturerId).FirstOrDefault().DisplayOrder);
                }
                //else if (orderBy == ProductSortingEnum.Position && relatedToProductId > 0)
                //{
                //    //sort by related product display order
                //    query = from p in query
                //            join rp in _relatedProductRepository.Table on p.Id equals rp.ProductId2
                //            where (relatedToProductId == rp.ProductId1)
                //            orderby rp.DisplayOrder
                //            select p;
                //}
                else if (ctx.OrderBy == ProductSortingEnum.Position)
                {
                    //sort by name (there's no any position if category or manufactur is not specified)
                    query = query.OrderBy(p => p.Name);
                }
                else if (ctx.OrderBy == ProductSortingEnum.NameAsc)
                {
                    //Name: A to Z
                    query = query.OrderBy(p => p.Name);
                }
                else if (ctx.OrderBy == ProductSortingEnum.NameDesc)
                {
                    //Name: Z to A
                    query = query.OrderByDescending(p => p.Name);
                }
                else if (ctx.OrderBy == ProductSortingEnum.PriceAsc)
                {
                    //Price: Low to High
                    query = query.OrderBy(p => p.ProductVariants.FirstOrDefault().Price);
                }
                else if (ctx.OrderBy == ProductSortingEnum.PriceDesc)
                {
                    //Price: High to Low
                    query = query.OrderByDescending(p => p.ProductVariants.FirstOrDefault().Price);
                }
                else if (ctx.OrderBy == ProductSortingEnum.CreatedOn)
                {
                    //creation date
                    query = query.OrderByDescending(p => p.CreatedOnUtc);
                }
                else if (ctx.OrderBy == ProductSortingEnum.CreatedOnAsc)
                {
                    // codehing: sm-add
                    // creation date: old to new
                    query = query.OrderBy(p => p.CreatedOnUtc);
                }
                else
                {
                    //actually this code is not reachable
                    query = query.OrderBy(p => p.Name);
                }

                var products = new PagedList<Product>(query, ctx.PageIndex, ctx.PageSize);

                //get filterable specification attribute option identifier
                if (ctx.LoadFilterableSpecificationAttributeOptionIds)
                {
                    var querySpecs = from p in query
                                     join psa in _productSpecificationAttributeRepository.Table on p.Id equals psa.ProductId
                                     where psa.AllowFiltering
                                     select psa.SpecificationAttributeOptionId;
                    //only distinct attributes
                    ctx.FilterableSpecificationAttributeOptionIds = querySpecs
                        .Distinct()
                        .ToList();
                }

                //return products
                return products;

                #endregion
            }
        }

        protected virtual IQueryable<Product> PrepareProductSearchQuery(ProductSearchContext ctx, IEnumerable<int> allowedCustomerRolesIds = null, bool searchLocalizedValue = false)
        {

            if (allowedCustomerRolesIds == null)
            {
                allowedCustomerRolesIds = _workContext.CurrentCustomer.CustomerRoles.Where(cr => cr.Active).Select(cr => cr.Id).ToList();
            }
            
            // products
            var query = _productRepository.Table;
            query = query.Where(p => !p.Deleted);
            if (!ctx.ShowHidden)
            {
                query = query.Where(p => p.Published);
            }

            // searching by keyword
            if (!String.IsNullOrWhiteSpace(ctx.Keywords))
            {
                query = from p in query
                        join lp in _localizedPropertyRepository.Table on p.Id equals lp.EntityId into p_lp
                        from lp in p_lp.DefaultIfEmpty()
                        from pv in p.ProductVariants.DefaultIfEmpty()
                        from pt in p.ProductTags.DefaultIfEmpty()
                        where (p.Name.Contains(ctx.Keywords)) ||
                              (ctx.SearchDescriptions && p.ShortDescription.Contains(ctx.Keywords)) ||
                              (ctx.SearchDescriptions && p.FullDescription.Contains(ctx.Keywords)) ||
                              (pv.Name.Contains(ctx.Keywords)) ||
                              (ctx.SearchDescriptions && pv.Description.Contains(ctx.Keywords)) ||
                              (ctx.SearchProductTags && pt.Name.Contains(ctx.Keywords)) ||
                            //localized values
                              (searchLocalizedValue && lp.LanguageId == ctx.LanguageId && lp.LocaleKeyGroup == "Product" && lp.LocaleKey == "Name" && lp.LocaleValue.Contains(ctx.Keywords)) ||
                              (ctx.SearchDescriptions && searchLocalizedValue && lp.LanguageId == ctx.LanguageId && lp.LocaleKeyGroup == "Product" && lp.LocaleKey == "ShortDescription" && lp.LocaleValue.Contains(ctx.Keywords)) ||
                              (ctx.SearchDescriptions && searchLocalizedValue && lp.LanguageId == ctx.LanguageId && lp.LocaleKeyGroup == "Product" && lp.LocaleKey == "FullDescription" && lp.LocaleValue.Contains(ctx.Keywords))
                        //UNDONE search localized values in associated product tags
                        select p;
            }

            if (!ctx.ShowHidden)
            {
				// ACL
                query = from p in query
                        join acl in _aclRepository.Table on p.Id equals acl.EntityId into p_acl
                        from acl in p_acl.DefaultIfEmpty()
                        where !p.SubjectToAcl || (acl.EntityName == "Product" && allowedCustomerRolesIds.Contains(acl.CustomerRoleId))
                        select p;
            }

			if (ctx.StoreId > 0)
			{
				//Store mapping
				query = from p in query
						join sm in _storeMappingRepository.Table on p.Id equals sm.EntityId into p_sm
						from sm in p_sm.DefaultIfEmpty()
						where !p.LimitedToStores || (sm.EntityName == "Product" && ctx.StoreId == sm.StoreId)
						select p;
			}

            // product variants
            // The function 'CurrentUtcDateTime' is not supported by SQL Server Compact. 
            // That's why we pass the date value
            var nowUtc = DateTime.UtcNow;
            query = from p in query
                    from pv in p.ProductVariants.DefaultIfEmpty()
                    where
                        //deleted
                        (ctx.ShowHidden || !pv.Deleted) &&
                        //published
                        (ctx.ShowHidden || pv.Published) &&
                        //price min
                        (
                            !ctx.PriceMin.HasValue
                            ||
                        //special price (specified price and valid date range)
                            ((pv.SpecialPrice.HasValue && ((!pv.SpecialPriceStartDateTimeUtc.HasValue || pv.SpecialPriceStartDateTimeUtc.Value <= nowUtc) && (!pv.SpecialPriceEndDateTimeUtc.HasValue || pv.SpecialPriceEndDateTimeUtc.Value >= nowUtc))) && (pv.SpecialPrice >= ctx.PriceMin.Value))
                            ||
                        //regular price (price isn't specified or date range isn't valid)
                            ((!pv.SpecialPrice.HasValue || ((pv.SpecialPriceStartDateTimeUtc.HasValue && pv.SpecialPriceStartDateTimeUtc.Value >= nowUtc) || (pv.SpecialPriceEndDateTimeUtc.HasValue && pv.SpecialPriceEndDateTimeUtc.Value <= nowUtc))) && (pv.Price >= ctx.PriceMin.Value))
                        ) &&
                        //price max
                        (
                            !ctx.PriceMax.HasValue
                            ||
                        //special price (specified price and valid date range)
                            ((pv.SpecialPrice.HasValue && ((!pv.SpecialPriceStartDateTimeUtc.HasValue || pv.SpecialPriceStartDateTimeUtc.Value <= nowUtc) && (!pv.SpecialPriceEndDateTimeUtc.HasValue || pv.SpecialPriceEndDateTimeUtc.Value >= nowUtc))) && (pv.SpecialPrice <= ctx.PriceMax.Value))
                            ||
                        //regular price (price isn't specified or date range isn't valid)
                            ((!pv.SpecialPrice.HasValue || ((pv.SpecialPriceStartDateTimeUtc.HasValue && pv.SpecialPriceStartDateTimeUtc.Value >= nowUtc) || (pv.SpecialPriceEndDateTimeUtc.HasValue && pv.SpecialPriceEndDateTimeUtc.Value <= nowUtc))) && (pv.Price <= ctx.PriceMax.Value))
                        ) &&
                        //available dates
                        (ctx.ShowHidden || (!pv.AvailableStartDateTimeUtc.HasValue || pv.AvailableStartDateTimeUtc.Value <= nowUtc)) &&
                        (ctx.ShowHidden || (!pv.AvailableEndDateTimeUtc.HasValue || pv.AvailableEndDateTimeUtc.Value >= nowUtc))
                    select p;


            // search by specs
            if (ctx.FilteredSpecs != null && ctx.FilteredSpecs.Count > 0)
            {
                query = from p in query
                        where !ctx.FilteredSpecs
                                   .Except(
                                       p.ProductSpecificationAttributes.Where(psa => psa.AllowFiltering).Select(
                                           psa => psa.SpecificationAttributeOptionId))
                                   .Any()
                        select p;
            }

            // category filtering
            if (ctx.CategoryIds != null && ctx.CategoryIds.Count > 0)
            {
                //search in subcategories
                query = from p in query
                        from pc in p.ProductCategories.Where(pc => ctx.CategoryIds.Contains(pc.CategoryId))
                        where (!ctx.FeaturedProducts.HasValue || ctx.FeaturedProducts.Value == pc.IsFeaturedProduct)
                        select p;
            }

            // manufacturer filtering
            if (ctx.ManufacturerId > 0)
            {
                query = from p in query
                        from pm in p.ProductManufacturers.Where(pm => pm.ManufacturerId == ctx.ManufacturerId)
                        where (!ctx.FeaturedProducts.HasValue || ctx.FeaturedProducts.Value == pm.IsFeaturedProduct)
                        select p;
            }

            // related products filtering
            //if (relatedToProductId > 0)
            //{
            //    query = from p in query
            //            join rp in _relatedProductRepository.Table on p.Id equals rp.ProductId2
            //            where (relatedToProductId == rp.ProductId1)
            //            select p;
            //}

            // tag filtering
            if (ctx.ProductTagId > 0)
            {
                query = from p in query
                        from pt in p.ProductTags.Where(pt => pt.Id == ctx.ProductTagId)
                        select p;
            }

            //// only distinct products (group by ID)
            //// if we use standard Distinct() method, then all fields will be compared (low performance)
            //// it'll not work in SQL Server Compact when searching products by a keyword)
            //query = from p in query
            //        group p by p.Id
            //            into pGroup
            //            orderby pGroup.Key
            //            select pGroup.FirstOrDefault();

            return query;
        }

        /// <summary>
        /// Update product review totals
        /// </summary>
        /// <param name="product">Product</param>
        public virtual void UpdateProductReviewTotals(Product product)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            int approvedRatingSum = 0;
            int notApprovedRatingSum = 0; 
            int approvedTotalReviews = 0;
            int notApprovedTotalReviews = 0;
            var reviews = product.ProductReviews;
            foreach (var pr in reviews)
            {
                if (pr.IsApproved)
                {
                    approvedRatingSum += pr.Rating;
                    approvedTotalReviews ++;
                }
                else
                {
                    notApprovedRatingSum += pr.Rating;
                    notApprovedTotalReviews++;
                }
            }

            product.ApprovedRatingSum = approvedRatingSum;
            product.NotApprovedRatingSum = notApprovedRatingSum;
            product.ApprovedTotalReviews = approvedTotalReviews;
            product.NotApprovedTotalReviews = notApprovedTotalReviews;
            UpdateProduct(product);
        }

        #endregion

        #region Product variants
        
        /// <summary>
        /// Get low stock product variants
        /// </summary>
        /// <returns>Result</returns>
        public virtual IList<ProductVariant> GetLowStockProductVariants()
        {
            //Track inventory for product variant
            var query1 = from pv in _productVariantRepository.Table
                         orderby pv.MinStockQuantity
                         where !pv.Deleted &&
                         pv.ManageInventoryMethodId == (int)ManageInventoryMethod.ManageStock &&
                         pv.MinStockQuantity >= pv.StockQuantity
                         select pv;
            var productVariants1 = query1.ToList();
            //Track inventory for product variant by product attributes
            var query2 = from pv in _productVariantRepository.Table
                         from pvac in pv.ProductVariantAttributeCombinations
                         where !pv.Deleted &&
                         pv.ManageInventoryMethodId == (int)ManageInventoryMethod.ManageStockByAttributes &&
                         pvac.StockQuantity <= 0
                         select pv;
            //only distinct products (group by ID)
            //if we use standard Distinct() method, then all fields will be compared (low performance)
            query2 = from pv in query2
                    group pv by pv.Id into pGroup
                    orderby pGroup.Key
                    select pGroup.FirstOrDefault();
            var productVariants2 = query2.ToList();

            var result = new List<ProductVariant>();
            result.AddRange(productVariants1);
            result.AddRange(productVariants2);
            return result;
        }
        
        /// <summary>
        /// Gets a product variant
        /// </summary>
        /// <param name="productVariantId">Product variant identifier</param>
        /// <returns>Product variant</returns>
        public virtual ProductVariant GetProductVariantById(int productVariantId)
        {
            if (productVariantId == 0)
                return null;

            string key = string.Format(PRODUCTVARIANTS_BY_ID_KEY, productVariantId);
            return _cacheManager.Get(key, () =>
            {
                var pv = _productVariantRepository.GetById(productVariantId);
                return pv;
            });
        }
        
        /// <summary>
        /// Get product variants by product identifiers
        /// </summary>
        /// <param name="productIds">Product identifiers</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Product variants</returns>
        public virtual IList<ProductVariant> GetProductVariantsByProductIds(int[] productIds, bool showHidden = false)
        {
            if (productIds == null || productIds.Length == 0)
                return new List<ProductVariant>();

            var query = _productVariantRepository.Table;
            if (!showHidden)
            {
                query = query.Where(pv => pv.Published);
            }
            if (!showHidden)
            {
                //The function 'CurrentUtcDateTime' is not supported by SQL Server Compact. 
                //That's why we pass the date value
                var nowUtc = DateTime.UtcNow;
                query = query.Where(pv =>
                        !pv.AvailableStartDateTimeUtc.HasValue ||
                        pv.AvailableStartDateTimeUtc <= nowUtc);
                query = query.Where(pv =>
                        !pv.AvailableEndDateTimeUtc.HasValue ||
                        pv.AvailableEndDateTimeUtc >= nowUtc);
            }
            query = query.Where(pv => !pv.Deleted);
            query = query.Where(pv => productIds.Contains(pv.ProductId));
            query = query.OrderBy(pv => pv.DisplayOrder);

            var productVariants = query.ToList();
            return productVariants;
        }
        
        /// <summary>
        /// Gets a product variant by SKU
        /// </summary>
        /// <param name="sku">SKU</param>
        /// <returns>Product variant</returns>
        public virtual ProductVariant GetProductVariantBySku(string sku)
        {
            if (String.IsNullOrEmpty(sku))
                return null;

            sku = sku.Trim();

            var query = from pv in _productVariantRepository.Table
                        orderby pv.DisplayOrder, pv.Id
                        where !pv.Deleted &&
                        pv.Sku == sku
                        select pv;
            var productVariant = query.FirstOrDefault();
            return productVariant;
        }

        /// <summary>
        /// Gets a product variant by GTIN
        /// </summary>
        /// <param name="sku">GTIN</param>
        /// <returns>Product variant</returns>
        public virtual ProductVariant GetProductVariantByGtin(string gtin)
        {
            if (String.IsNullOrEmpty(gtin))
                return null;

            gtin = gtin.Trim();

            var query = from pv in _productVariantRepository.Table
                        orderby pv.DisplayOrder, pv.Id
                        where !pv.Deleted &&
                        pv.Gtin == gtin
                        select pv;
            var productVariant = query.FirstOrDefault();
            return productVariant;
        }

        /// <summary>
        /// Inserts a product variant
        /// </summary>
        /// <param name="productVariant">The product variant</param>
        public virtual void InsertProductVariant(ProductVariant productVariant)
        {
            if (productVariant == null)
                throw new ArgumentNullException("productVariant");

            _productVariantRepository.Insert(productVariant);

            _cacheManager.RemoveByPattern(PRODUCTS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTVARIANTS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(TIERPRICES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(productVariant);
        }

        /// <summary>
        /// Updates the product variant
        /// </summary>
        /// <param name="productVariant">The product variant</param>
        public virtual void UpdateProductVariant(ProductVariant productVariant)
        {
            if (productVariant == null)
                throw new ArgumentNullException("productVariant");

            _productVariantRepository.Update(productVariant);

            _cacheManager.RemoveByPattern(PRODUCTS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTVARIANTS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(TIERPRICES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityUpdated(productVariant);
        }
        
        /// <summary>
        /// Gets product variants by product identifier
        /// </summary>
        /// <param name="productId">The product identifier</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Product variant collection</returns>
        public virtual IList<ProductVariant> GetProductVariantsByProductId(int productId, bool showHidden = false)
        {
            string key = string.Format(PRODUCTVARIANTS_ALL_KEY, showHidden, productId);
            return _cacheManager.Get(key, () =>
            {
                var query = _productVariantRepository.Table;
                if (!showHidden)
                {
                    query = query.Where(pv => pv.Published);
                }
                if (!showHidden)
                {
                    //The function 'CurrentUtcDateTime' is not supported by SQL Server Compact. 
                    //That's why we pass the date value
                    var nowUtc = DateTime.UtcNow;
                    query = query.Where(pv =>
                            !pv.AvailableStartDateTimeUtc.HasValue ||
                            pv.AvailableStartDateTimeUtc <= nowUtc);
                    query = query.Where(pv =>
                            !pv.AvailableEndDateTimeUtc.HasValue ||
                            pv.AvailableEndDateTimeUtc >= nowUtc);
                }
                query = query.Where(pv => !pv.Deleted);
                query = query.Where(pv => pv.ProductId == productId);
                query = query.OrderBy(pv => pv.DisplayOrder);

                var productVariants = query.ToList();
                return productVariants;
            });
        }

        /// <summary>
        /// Delete a product variant
        /// </summary>
        /// <param name="productVariant">Product variant</param>
        public virtual void DeleteProductVariant(ProductVariant productVariant)
        {
            if (productVariant == null)
                throw new ArgumentNullException("productVariant");

            productVariant.Deleted = true;
            UpdateProductVariant(productVariant);
        }
        
        /// <summary>
        /// Adjusts inventory
        /// </summary>
        /// <param name="productVariant">Product variant</param>
        /// <param name="decrease">A value indicating whether to increase or descrease product variant stock quantity</param>
        /// <param name="quantity">Quantity</param>
        /// <param name="attributesXml">Attributes in XML format</param>
        public virtual void AdjustInventory(ProductVariant productVariant, bool decrease,
            int quantity, string attributesXml)
        {
            if (productVariant == null)
                throw new ArgumentNullException("productVariant");

            var prevStockQuantity = productVariant.StockQuantity;

            switch (productVariant.ManageInventoryMethod)
            {
                case ManageInventoryMethod.DontManageStock:
                    {
                        //do nothing
                        return;
                    }
                case ManageInventoryMethod.ManageStock:
                    {
                        int newStockQuantity = 0;
                        if (decrease)
                            newStockQuantity = productVariant.StockQuantity - quantity;
                        else
                            newStockQuantity = productVariant.StockQuantity + quantity;

                        bool newPublished = productVariant.Published;
                        bool newDisableBuyButton = productVariant.DisableBuyButton;
                        bool newDisableWishlistButton = productVariant.DisableWishlistButton;

                        //check if minimum quantity is reached
                        if (decrease)
                        {
                            if (productVariant.MinStockQuantity >= newStockQuantity)
                            {
                                switch (productVariant.LowStockActivity)
                                {
                                    case LowStockActivity.DisableBuyButton:
                                        newDisableBuyButton = true;
                                        newDisableWishlistButton = true;
                                        break;
                                    case LowStockActivity.Unpublish:
                                        newPublished = false;
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }

                        productVariant.StockQuantity = newStockQuantity;
                        productVariant.DisableBuyButton = newDisableBuyButton;
                        productVariant.DisableWishlistButton = newDisableWishlistButton;
                        productVariant.Published = newPublished;
                        UpdateProductVariant(productVariant);

                        //send email notification
                        if (decrease && productVariant.NotifyAdminForQuantityBelow > newStockQuantity)
                            _workflowMessageService.SendQuantityBelowStoreOwnerNotification(productVariant, _localizationSettings.DefaultAdminLanguageId);
                        
                        if (decrease)
                        {
                            var product = productVariant.Product;
                            bool allProductVariantsUnpublished = true;
                            foreach (var pv2 in GetProductVariantsByProductId(product.Id))
                            {
                                if (pv2.Published)
                                {
                                    allProductVariantsUnpublished = false;
                                    break;
                                }
                            }

                            if (allProductVariantsUnpublished)
                            {
                                product.Published = false;
                                UpdateProduct(product);
                            }
                        }
                    }
                    break;
                case ManageInventoryMethod.ManageStockByAttributes:
                    {
                        var combination = _productAttributeParser.FindProductVariantAttributeCombination(productVariant, attributesXml);
                        if (combination != null)
                        {
                            int newStockQuantity = 0;
                            if (decrease)
                                newStockQuantity = combination.StockQuantity - quantity;
                            else
                                newStockQuantity = combination.StockQuantity + quantity;

                            combination.StockQuantity = newStockQuantity;
                            _productAttributeService.UpdateProductVariantAttributeCombination(combination);
                        }
                    }
                    break;
                default:
                    break;
            }

            //TODO send back in stock notifications?
            //if (productVariant.ManageInventoryMethod == ManageInventoryMethod.ManageStock &&
            //    productVariant.BackorderMode == BackorderMode.NoBackorders &&
            //    productVariant.AllowBackInStockSubscriptions &&
            //    productVariant.StockQuantity > 0 &&
            //    prevStockQuantity <= 0 &&
            //    productVariant.Published &&
            //    !productVariant.Deleted)
            //{
            //    //_backInStockSubscriptionService.SendNotificationsToSubscribers(productVariant);
            //}
        }

        /// <summary>
        /// Search product variants
        /// </summary>
        /// <param name="categoryId">Category identifier; 0 to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; 0 to load all records</param>
        /// <param name="keywords">Keywords</param>
        /// <param name="searchDescriptions">A value indicating whether to search in descriptions</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Product variants</returns>
        public virtual IPagedList<ProductVariant> SearchProductVariants(int categoryId, int manufacturerId,
             string keywords, bool searchDescriptions, int pageIndex, int pageSize, bool showHidden = false)
        {
            //products
            var query = _productVariantRepository.Table;
            query = query.Where(pv => !pv.Deleted);
            if (!showHidden)
            {
                query = query.Where(pv => pv.Published);
            }
            query = query.Where(pv => !pv.Product.Deleted);
            if (!showHidden)
            {
                query = query.Where(pv => pv.Product.Published);
            }

            //searching by keyword
            if (!String.IsNullOrWhiteSpace(keywords))
            {
                query = from pv in query
                        where (pv.Product.Name.Contains(keywords)) ||
                        (searchDescriptions && pv.Product.ShortDescription.Contains(keywords)) ||
                        (searchDescriptions && pv.Product.FullDescription.Contains(keywords)) ||
                        (pv.Name.Contains(keywords)) ||
                        (searchDescriptions && pv.Description.Contains(keywords))
                        select pv;
            }

            //category filtering
            if (categoryId > 0)
            {
                query = from pv in query
                        from pc in pv.Product.ProductCategories.Where(pc => pc.CategoryId == categoryId)
                        select pv;
            }

            //manufacturer filtering
            if (manufacturerId > 0)
            {
                query = from pv in query
                        from pm in pv.Product.ProductManufacturers.Where(pm => pm.ManufacturerId == manufacturerId)
                        select pv;
            }

            //only distinct products (group by ID)
            //if we use standard Distinct() method, then all fields will be compared (low performance)
            //it'll not work in SQL Server Compact when searching products by a keyword)
            query = from pv in query
                    group pv by pv.Id into pvGroup
                    orderby pvGroup.Key
                    select pvGroup.FirstOrDefault();

            query = query.OrderBy(pv => pv.Product.Name).ThenBy(pv => pv.DisplayOrder);
            var productVariants = new PagedList<ProductVariant>(query, pageIndex, pageSize);
            return productVariants;
        }
        
        /// <summary>
        /// Update HasTierPrices property (used for performance optimization)
        /// </summary>
        /// <param name="productVariant">Product variant</param>
        public virtual void UpdateHasTierPricesProperty(ProductVariant productVariant)
        {
            if (productVariant == null)
                throw new ArgumentNullException("productVariant");

            productVariant.HasTierPrices = productVariant.TierPrices.Count > 0;
            UpdateProductVariant(productVariant);
        }

        /// <summary>
        /// Update HasDiscountsApplied property (used for performance optimization)
        /// </summary>
        /// <param name="productVariant">Product variant</param>
        public virtual void UpdateHasDiscountsApplied(ProductVariant productVariant)
        {
            if (productVariant == null)
                throw new ArgumentNullException("productVariant");

            productVariant.HasDiscountsApplied = productVariant.AppliedDiscounts.Count > 0;
            UpdateProductVariant(productVariant);
        }

        #endregion

        #region Related products

        /// <summary>
        /// Deletes a related product
        /// </summary>
        /// <param name="relatedProduct">Related product</param>
        public virtual void DeleteRelatedProduct(RelatedProduct relatedProduct)
        {
            if (relatedProduct == null)
                throw new ArgumentNullException("relatedProduct");

            _relatedProductRepository.Delete(relatedProduct);

            //event notification
            _eventPublisher.EntityDeleted(relatedProduct);
        }

        /// <summary>
        /// Gets a related product collection by product identifier
        /// </summary>
        /// <param name="productId1">The first product identifier</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Related product collection</returns>
        public virtual IList<RelatedProduct> GetRelatedProductsByProductId1(int productId1, bool showHidden = false)
        {
            var query = from rp in _relatedProductRepository.Table
                        join p in _productRepository.Table on rp.ProductId2 equals p.Id
                        where rp.ProductId1 == productId1 &&
                        !p.Deleted &&
                        (showHidden || p.Published)
                        orderby rp.DisplayOrder
                        select rp;
            var relatedProducts = query.ToList();

            return relatedProducts;
        }

        /// <summary>
        /// Gets a related product
        /// </summary>
        /// <param name="relatedProductId">Related product identifier</param>
        /// <returns>Related product</returns>
        public virtual RelatedProduct GetRelatedProductById(int relatedProductId)
        {
            if (relatedProductId == 0)
                return null;
            
            var relatedProduct = _relatedProductRepository.GetById(relatedProductId);
            return relatedProduct;
        }

        /// <summary>
        /// Inserts a related product
        /// </summary>
        /// <param name="relatedProduct">Related product</param>
        public virtual void InsertRelatedProduct(RelatedProduct relatedProduct)
        {
            if (relatedProduct == null)
                throw new ArgumentNullException("relatedProduct");

            _relatedProductRepository.Insert(relatedProduct);

            //event notification
            _eventPublisher.EntityInserted(relatedProduct);
        }

        /// <summary>
        /// Updates a related product
        /// </summary>
        /// <param name="relatedProduct">Related product</param>
        public virtual void UpdateRelatedProduct(RelatedProduct relatedProduct)
        {
            if (relatedProduct == null)
                throw new ArgumentNullException("relatedProduct");

            _relatedProductRepository.Update(relatedProduct);

            //event notification
            _eventPublisher.EntityUpdated(relatedProduct);
        }

        #endregion

        #region Cross-sell products

        /// <summary>
        /// Deletes a cross-sell product
        /// </summary>
        /// <param name="crossSellProduct">Cross-sell identifier</param>
        public virtual void DeleteCrossSellProduct(CrossSellProduct crossSellProduct)
        {
            if (crossSellProduct == null)
                throw new ArgumentNullException("crossSellProduct");

            _crossSellProductRepository.Delete(crossSellProduct);

            //event notification
            _eventPublisher.EntityDeleted(crossSellProduct);
        }

        /// <summary>
        /// Gets a cross-sell product collection by product identifier
        /// </summary>
        /// <param name="productId1">The first product identifier</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Cross-sell product collection</returns>
        public virtual IList<CrossSellProduct> GetCrossSellProductsByProductId1(int productId1, bool showHidden = false)
        {
            var query = from csp in _crossSellProductRepository.Table
                        join p in _productRepository.Table on csp.ProductId2 equals p.Id
                        where csp.ProductId1 == productId1 &&
                        !p.Deleted &&
                        (showHidden || p.Published)
                        orderby csp.Id
                        select csp;
            var crossSellProducts = query.ToList();
            return crossSellProducts;
        }

        /// <summary>
        /// Gets a cross-sell product
        /// </summary>
        /// <param name="crossSellProductId">Cross-sell product identifier</param>
        /// <returns>Cross-sell product</returns>
        public virtual CrossSellProduct GetCrossSellProductById(int crossSellProductId)
        {
            if (crossSellProductId == 0)
                return null;

            var crossSellProduct = _crossSellProductRepository.GetById(crossSellProductId);
            return crossSellProduct;
        }

        /// <summary>
        /// Inserts a cross-sell product
        /// </summary>
        /// <param name="crossSellProduct">Cross-sell product</param>
        public virtual void InsertCrossSellProduct(CrossSellProduct crossSellProduct)
        {
            if (crossSellProduct == null)
                throw new ArgumentNullException("crossSellProduct");

            _crossSellProductRepository.Insert(crossSellProduct);

            //event notification
            _eventPublisher.EntityInserted(crossSellProduct);
        }

        /// <summary>
        /// Updates a cross-sell product
        /// </summary>
        /// <param name="crossSellProduct">Cross-sell product</param>
        public virtual void UpdateCrossSellProduct(CrossSellProduct crossSellProduct)
        {
            if (crossSellProduct == null)
                throw new ArgumentNullException("crossSellProduct");

            _crossSellProductRepository.Update(crossSellProduct);

            //event notification
            _eventPublisher.EntityUpdated(crossSellProduct);
        }

        /// <summary>
        /// Gets a cross-sells
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <param name="numberOfProducts">Number of products to return</param>
        /// <returns>Cross-sells</returns>
        public virtual IList<Product> GetCrosssellProductsByShoppingCart(IList<ShoppingCartItem> cart, int numberOfProducts)
        {
            var result = new List<Product>();

            if (numberOfProducts == 0)
                return result;

            if (cart == null || cart.Count == 0)
                return result;

            var cartProductIds = new List<int>();
            foreach (var sci in cart)
            {
                int prodId = sci.ProductVariant.ProductId;
                if (!cartProductIds.Contains(prodId))
                    cartProductIds.Add(prodId);
            }

            foreach (var sci in cart)
            {
                var crossSells = GetCrossSellProductsByProductId1(sci.ProductVariant.ProductId);
                foreach (var crossSell in crossSells)
                {
                    //validate that this product is not added to result yet
                    //validate that this product is not in the cart
                    if (result.Find(p => p.Id == crossSell.ProductId2) == null &&
                        !cartProductIds.Contains(crossSell.ProductId2))
                    {
                        var productToAdd = GetProductById(crossSell.ProductId2);
                        //validate product
                        if (productToAdd == null || productToAdd.Deleted || !productToAdd.Published)
                            continue;
                        //at least one variant should be valid and available
                        if (GetProductVariantsByProductId(productToAdd.Id).Count == 0)
                            continue;

                        //add a product to result
                        result.Add(productToAdd);
                        if (result.Count >= numberOfProducts)
                            return result;
                    }
                }
            }
            return result;
        }
        #endregion
        
        #region Tier prices
        
        /// <summary>
        /// Deletes a tier price
        /// </summary>
        /// <param name="tierPrice">Tier price</param>
        public virtual void DeleteTierPrice(TierPrice tierPrice)
        {
            if (tierPrice == null)
                throw new ArgumentNullException("tierPrice");

            _tierPriceRepository.Delete(tierPrice);

            _cacheManager.RemoveByPattern(PRODUCTS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTVARIANTS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(TIERPRICES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityDeleted(tierPrice);
        }

        /// <summary>
        /// Gets a tier price
        /// </summary>
        /// <param name="tierPriceId">Tier price identifier</param>
        /// <returns>Tier price</returns>
        public virtual TierPrice GetTierPriceById(int tierPriceId)
        {
            if (tierPriceId == 0)
                return null;
            
            var tierPrice = _tierPriceRepository.GetById(tierPriceId);
            return tierPrice;
        }

        /// <summary>
        /// Inserts a tier price
        /// </summary>
        /// <param name="tierPrice">Tier price</param>
        public virtual void InsertTierPrice(TierPrice tierPrice)
        {
            if (tierPrice == null)
                throw new ArgumentNullException("tierPrice");

            _tierPriceRepository.Insert(tierPrice);

            _cacheManager.RemoveByPattern(PRODUCTS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTVARIANTS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(TIERPRICES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(tierPrice);
        }

        /// <summary>
        /// Updates the tier price
        /// </summary>
        /// <param name="tierPrice">Tier price</param>
        public virtual void UpdateTierPrice(TierPrice tierPrice)
        {
            if (tierPrice == null)
                throw new ArgumentNullException("tierPrice");

            _tierPriceRepository.Update(tierPrice);

            _cacheManager.RemoveByPattern(PRODUCTS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTVARIANTS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(TIERPRICES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityUpdated(tierPrice);
        }

        #endregion

        #region Product pictures

        /// <summary>
        /// Deletes a product picture
        /// </summary>
        /// <param name="productPicture">Product picture</param>
        public virtual void DeleteProductPicture(ProductPicture productPicture)
        {
            if (productPicture == null)
                throw new ArgumentNullException("productPicture");

            // codehint: sm-add
            UnassignDeletedPictureFromVariantCombinations(productPicture);

            _productPictureRepository.Delete(productPicture);

            //event notification
            _eventPublisher.EntityDeleted(productPicture);
        }

        // codehint: sm-add
        private void UnassignDeletedPictureFromVariantCombinations(ProductPicture productPicture)
        {
            var picId = productPicture.Id;
            var rs = this._productVariantAttributeCombinationRepository;
            bool touched = false;

            foreach (var variant in productPicture.Product.ProductVariants)
            {
                var combinations = from c in rs.Table
                                    where c.ProductVariantId == variant.Id && !String.IsNullOrEmpty(c.AssignedPictureIds)
                                    select c;

                foreach (var c in combinations)
                {
                    var ids = c.GetAssignedPictureIds().ToList();
                    if (ids.Contains(picId)) 
                    {
                        ids.Remove(picId);
                        //c.AssignedPictureIds = ids.Count > 0 ? String.Join<int>(",", ids) : null;
                        c.SetAssignedPictureIds(ids.ToArray());
                        touched = true;
                        // we will save after we're done. It's faster.
                    }
                }
            }

            // save in one shot!
            if (touched)
            {
                _dbContext.SaveChanges();
            }
        }

        /// <summary>
        /// Gets a product pictures by product identifier
        /// </summary>
        /// <param name="productId">The product identifier</param>
        /// <returns>Product pictures</returns>
        public virtual IList<ProductPicture> GetProductPicturesByProductId(int productId)
        {
            var query = from pp in _productPictureRepository.Table
                        where pp.ProductId == productId
                        orderby pp.DisplayOrder
                        select pp;
            var productPictures = query.ToList();
            return productPictures;
        }

        /// <summary>
        /// Gets a product picture
        /// </summary>
        /// <param name="productPictureId">Product picture identifier</param>
        /// <returns>Product picture</returns>
        public virtual ProductPicture GetProductPictureById(int productPictureId)
        {
            if (productPictureId == 0)
                return null;

            var pp = _productPictureRepository.GetById(productPictureId);
            return pp;
        }

        /// <summary>
        /// Inserts a product picture
        /// </summary>
        /// <param name="productPicture">Product picture</param>
        public virtual void InsertProductPicture(ProductPicture productPicture)
        {
            if (productPicture == null)
                throw new ArgumentNullException("productPicture");

            _productPictureRepository.Insert(productPicture);

            //event notification
            _eventPublisher.EntityInserted(productPicture);
        }

        /// <summary>
        /// Updates a product picture
        /// </summary>
        /// <param name="productPicture">Product picture</param>
        public virtual void UpdateProductPicture(ProductPicture productPicture)
        {
            if (productPicture == null)
                throw new ArgumentNullException("productPicture");

            _productPictureRepository.Update(productPicture);

            //event notification
            _eventPublisher.EntityUpdated(productPicture);
        }

        #endregion

        #endregion
    }
}
