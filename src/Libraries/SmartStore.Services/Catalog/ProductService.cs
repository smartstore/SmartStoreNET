using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using SmartStore.Collections;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Discounts;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Security;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Events;
using SmartStore.Data.Caching;
using SmartStore.Services.Messages;
using SmartStore.Services.Orders;

namespace SmartStore.Services.Catalog
{
	public partial class ProductService : IProductService
	{
		private readonly IRepository<Product> _productRepository;
        private readonly IRepository<RelatedProduct> _relatedProductRepository;
        private readonly IRepository<CrossSellProduct> _crossSellProductRepository;
        private readonly IRepository<TierPrice> _tierPriceRepository;
        private readonly IRepository<LocalizedProperty> _localizedPropertyRepository;
        private readonly IRepository<AclRecord> _aclRepository;
		private readonly IRepository<StoreMapping> _storeMappingRepository;
        private readonly IRepository<ProductPicture> _productPictureRepository;
        private readonly IRepository<ProductSpecificationAttribute> _productSpecificationAttributeRepository;
        private readonly IRepository<ProductVariantAttributeCombination> _productVariantAttributeCombinationRepository;
		private readonly IRepository<ProductBundleItem> _productBundleItemRepository;
		private readonly IRepository<ShoppingCartItem> _shoppingCartItemRepository;
		private readonly IProductAttributeService _productAttributeService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IDbContext _dbContext;
        private readonly LocalizationSettings _localizationSettings;
		private readonly ICommonServices _services;

        public ProductService(
            IRepository<Product> productRepository,
            IRepository<RelatedProduct> relatedProductRepository,
            IRepository<CrossSellProduct> crossSellProductRepository,
            IRepository<TierPrice> tierPriceRepository,
            IRepository<ProductPicture> productPictureRepository,
            IRepository<LocalizedProperty> localizedPropertyRepository,
            IRepository<AclRecord> aclRepository,
			IRepository<StoreMapping> storeMappingRepository,
            IRepository<ProductSpecificationAttribute> productSpecificationAttributeRepository,
            IRepository<ProductVariantAttributeCombination> productVariantAttributeCombinationRepository,
			IRepository<ProductBundleItem> productBundleItemRepository,
			IRepository<ShoppingCartItem> shoppingCartItemRepository,
			IProductAttributeService productAttributeService,
            IProductAttributeParser productAttributeParser,
            IWorkflowMessageService workflowMessageService,
			IDbContext dbContext,
            LocalizationSettings localizationSettings,
			ICommonServices services)
        {
            _productRepository = productRepository;
            _relatedProductRepository = relatedProductRepository;
            _crossSellProductRepository = crossSellProductRepository;
            _tierPriceRepository = tierPriceRepository;
            _productPictureRepository = productPictureRepository;
            _localizedPropertyRepository = localizedPropertyRepository;
            _aclRepository = aclRepository;
			_storeMappingRepository = storeMappingRepository;
            _productSpecificationAttributeRepository = productSpecificationAttributeRepository;
            _productVariantAttributeCombinationRepository = productVariantAttributeCombinationRepository;
			_productBundleItemRepository = productBundleItemRepository;
			_shoppingCartItemRepository = shoppingCartItemRepository;
            _productAttributeService = productAttributeService;
            _productAttributeParser = productAttributeParser;
            _workflowMessageService = workflowMessageService;
            _dbContext = dbContext;
            _localizationSettings = localizationSettings;
			_services = services;

			QuerySettings = DbQuerySettings.Default;
        }

		public DbQuerySettings QuerySettings { get; set; }

		#region Utilities

		protected virtual int EnsureMutuallyRelatedProducts(List<int> productIds)
		{
			int count = 0;

			foreach (int id1 in productIds)
			{
				var mutualAssociations = (
					from rp in _relatedProductRepository.Table
					join p in _productRepository.Table on rp.ProductId2 equals p.Id
					where !p.Deleted && rp.ProductId2 == id1
					select rp).ToList();

				foreach (int id2 in productIds)
				{
					if (id1 == id2)
						continue;

					if (!mutualAssociations.Any(x => x.ProductId1 == id2))
					{
						int maxDisplayOrder = _relatedProductRepository.TableUntracked
							.Where(x => x.ProductId1 == id2)
							.OrderByDescending(x => x.DisplayOrder)
							.Select(x => x.DisplayOrder)
							.FirstOrDefault();

						var newRelatedProduct = new RelatedProduct
						{
							ProductId1 = id2,
							ProductId2 = id1,
							DisplayOrder = maxDisplayOrder + 1
						};

						InsertRelatedProduct(newRelatedProduct);
						++count;
					}
				}
			}

			return count;
		}

		protected virtual int EnsureMutuallyCrossSellProducts(List<int> productIds)
		{
			int count = 0;

			foreach (int id1 in productIds)
			{
				var mutualAssociations = (
					from rp in _crossSellProductRepository.Table
					join p in _productRepository.Table on rp.ProductId2 equals p.Id
					where !p.Deleted && rp.ProductId2 == id1
					select rp).ToList();

				foreach (int id2 in productIds)
				{
					if (id1 == id2)
						continue;

					if (!mutualAssociations.Any(x => x.ProductId1 == id2))
					{
						var newCrossSellProduct = new CrossSellProduct
						{
							ProductId1 = id2,
							ProductId2 = id1
						};

						InsertCrossSellProduct(newCrossSellProduct);
						++count;
					}
				}
			}

			return count;
		}

		#endregion

		#region Products

		public virtual void DeleteProduct(Product product)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            product.Deleted = true;
			product.DeliveryTimeId = null;
			product.QuantityUnitId = null;
			product.CountryOfOriginId = null;

            UpdateProduct(product);

			if (product.ProductType == ProductType.GroupedProduct)
			{
				var associatedProducts = _productRepository.Table
					.Where(x => x.ParentGroupedProductId == product.Id)
					.ToList();

				associatedProducts.ForEach(x => x.ParentGroupedProductId = 0);

				_dbContext.SaveChanges();
			}
        }

        public virtual IList<Product> GetAllProductsDisplayedOnHomePage()
        {
            var query = 
				from p in _productRepository.Table
				orderby p.HomePageDisplayOrder
				where p.Published && !p.Deleted && p.ShowOnHomePage
				select p;

            var products = query.ToList();
            return products;
        }
        
        public virtual Product GetProductById(int productId)
        {
            if (productId == 0)
                return null;

			return _productRepository.GetByIdCached(productId, "db.product.id-" + productId);
		}

        public virtual IList<Product> GetProductsByIds(int[] productIds, ProductLoadFlags flags = ProductLoadFlags.None)
        {
            if (productIds == null || productIds.Length == 0)
                return new List<Product>();

            var query = from p in _productRepository.Table
                        where productIds.Contains(p.Id)
                        select p;

			if (flags > ProductLoadFlags.None)
			{
				query = ApplyLoadFlags(query, flags);
			}

			var products = query.ToList();

			// sort by passed identifier sequence
			var sortQuery = from i in productIds
							join p in products on i equals p.Id
							select p;

			return sortQuery.ToList();
        }

		private IQueryable<Product> ApplyLoadFlags(IQueryable<Product> query, ProductLoadFlags flags)
		{
			if (flags.HasFlag(ProductLoadFlags.WithAttributeCombinations))
			{
				query = query.Include(x => x.ProductVariantAttributeCombinations);
			}

			if (flags.HasFlag(ProductLoadFlags.WithBundleItems))
			{
				query = query.Include(x => x.ProductBundleItems.Select(y => y.Product));
			}

			if (flags.HasFlag(ProductLoadFlags.WithCategories))
			{
				query = query.Include(x => x.ProductCategories.Select(y => y.Category));
			}

			if (flags.HasFlag(ProductLoadFlags.WithDiscounts))
			{
				query = query.Include(x => x.AppliedDiscounts);
			}

			if (flags.HasFlag(ProductLoadFlags.WithManufacturers))
			{
				query = query.Include(x => x.ProductManufacturers.Select(y => y.Manufacturer));
			}

			if (flags.HasFlag(ProductLoadFlags.WithPictures))
			{
				query = query.Include(x => x.ProductPictures);
			}

			if (flags.HasFlag(ProductLoadFlags.WithReviews))
			{
				query = query.Include(x => x.ProductReviews);
			}

			if (flags.HasFlag(ProductLoadFlags.WithSpecificationAttributes))
			{
				query = query.Include(x => x.ProductSpecificationAttributes.Select(y => y.SpecificationAttributeOption));
			}

			if (flags.HasFlag(ProductLoadFlags.WithTags))
			{
				query = query.Include(x => x.ProductTags);
			}

			if (flags.HasFlag(ProductLoadFlags.WithTierPrices))
			{
				query = query.Include(x => x.TierPrices);
			}

			if (flags.HasFlag(ProductLoadFlags.WithAttributes))
			{
				query = query.Include(x => x.ProductVariantAttributes.Select(y => y.ProductAttribute));
			}

			if (flags.HasFlag(ProductLoadFlags.WithAttributeValues))
			{
				query = query.Include(x => x.ProductVariantAttributes.Select(y => y.ProductVariantAttributeValues));
			}

			if (flags.HasFlag(ProductLoadFlags.WithDeliveryTime))
			{
				query = query.Include(x => x.DeliveryTime);
			}

			return query;
		}

        public virtual void InsertProduct(Product product)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            //insert
            _productRepository.Insert(product);
            
            //event notification
            _services.EventPublisher.EntityInserted(product);
        }

		public virtual void UpdateProduct(Product product, bool publishEvent = true)
        {
            if (product == null)
                throw new ArgumentNullException("product");

			bool modified = false;
			if (publishEvent)
			{
				modified = _productRepository.IsModified(product);
			}

            // update
            _productRepository.Update(product);

            // event notification
			if (publishEvent && modified)
			{
				_services.EventPublisher.EntityUpdated(product);
			}
        }

        public virtual int CountProducts(ProductSearchContext ctx)
        {
            Guard.NotNull(ctx, nameof(ctx));

            var query = PrepareProductSearchQuery(ctx, p => p.Id);
            return query.Distinct().Count();
        }

		public virtual IQueryable<Product> PrepareProductSearchQuery(
			ProductSearchContext ctx,
			IEnumerable<int> allowedCustomerRolesIds = null,
			bool searchLocalizedValue = false)
		{
			return PrepareProductSearchQuery<Product>(ctx, x => x, allowedCustomerRolesIds, searchLocalizedValue);
		}

		public virtual IQueryable<TResult> PrepareProductSearchQuery<TResult>(
			ProductSearchContext ctx,
			Expression<Func<Product, TResult>> selector,
			IEnumerable<int> allowedCustomerRolesIds = null,
			bool searchLocalizedValue = false)
		{
			Guard.NotNull(ctx, nameof(ctx));
			Guard.NotNull(selector, nameof(selector));

			if (allowedCustomerRolesIds == null)
			{
				allowedCustomerRolesIds = _services.WorkContext.CurrentCustomer.CustomerRoles.Where(cr => cr.Active).Select(cr => cr.Id).ToList();
			}
			
			// products
			var query = ctx.Query ?? _productRepository.Table;
			query = query.Where(p => !p.Deleted);

			if (!ctx.IsPublished.HasValue)
			{
				if (!ctx.ShowHidden)
					query = query.Where(p => p.Published);
			}
			else
			{
				query = query.Where(p => p.Published == ctx.IsPublished.Value);
			}

			if (ctx.ParentGroupedProductId > 0)
			{
				query = query.Where(p => p.ParentGroupedProductId == ctx.ParentGroupedProductId);
			}

			if (ctx.VisibleIndividuallyOnly)
			{
				query = query.Where(p => p.VisibleIndividually);
			}

			if (ctx.HomePageProducts.HasValue)
			{
				query = query.Where(p => p.ShowOnHomePage == ctx.HomePageProducts.Value);
			}

			if (ctx.ProductType.HasValue)
			{
				int productTypeId = (int)ctx.ProductType.Value;
				query = query.Where(p => p.ProductTypeId == productTypeId);
			}

			if (ctx.ProductIds != null && ctx.ProductIds.Count > 0)
			{
				query = query.Where(x => ctx.ProductIds.Contains(x.Id));
			}
			else
			{
				if (ctx.IdMin != 0)
					query = query.Where(x => x.Id >= ctx.IdMin);

				if (ctx.IdMax != 0)
					query = query.Where(x => x.Id <= ctx.IdMax);
			}

			if (ctx.AvailabilityMinimum.HasValue)
			{
				query = query.Where(x => x.StockQuantity >= ctx.AvailabilityMinimum.Value);
			}

			if (ctx.AvailabilityMaximum.HasValue)
			{
				query = query.Where(x => x.StockQuantity <= ctx.AvailabilityMaximum.Value);
			}

			if (ctx.CreatedFromUtc.HasValue)
			{
				query = query.Where(x => x.CreatedOnUtc >= ctx.CreatedFromUtc.Value);
			}

			if (ctx.CreatedToUtc.HasValue)
			{
				query = query.Where(x => x.CreatedOnUtc <= ctx.CreatedToUtc.Value);
			}

			//The function 'CurrentUtcDateTime' is not supported by SQL Server Compact. 
			//That's why we pass the date value
			var nowUtc = DateTime.UtcNow;

			if (ctx.PriceMin.HasValue)
			{
				//min price
				query = query.Where(p =>
					//special price (specified price and valid date range)
										((p.SpecialPrice.HasValue &&
										  ((!p.SpecialPriceStartDateTimeUtc.HasValue ||
											p.SpecialPriceStartDateTimeUtc.Value < nowUtc) &&
										   (!p.SpecialPriceEndDateTimeUtc.HasValue ||
											p.SpecialPriceEndDateTimeUtc.Value > nowUtc))) &&
										 (p.SpecialPrice >= ctx.PriceMin.Value))
										||
											//regular price (price isn't specified or date range isn't valid)
										((!p.SpecialPrice.HasValue ||
										  ((p.SpecialPriceStartDateTimeUtc.HasValue &&
											p.SpecialPriceStartDateTimeUtc.Value > nowUtc) ||
										   (p.SpecialPriceEndDateTimeUtc.HasValue &&
											p.SpecialPriceEndDateTimeUtc.Value < nowUtc))) &&
										 (p.Price >= ctx.PriceMin.Value)));
			}
			if (ctx.PriceMax.HasValue)
			{
				//max price
				query = query.Where(p =>
					//special price (specified price and valid date range)
									((p.SpecialPrice.HasValue &&
									  ((!p.SpecialPriceStartDateTimeUtc.HasValue ||
										p.SpecialPriceStartDateTimeUtc.Value < nowUtc) &&
									   (!p.SpecialPriceEndDateTimeUtc.HasValue ||
										p.SpecialPriceEndDateTimeUtc.Value > nowUtc))) &&
									 (p.SpecialPrice <= ctx.PriceMax.Value))
									||
										//regular price (price isn't specified or date range isn't valid)
									((!p.SpecialPrice.HasValue ||
									  ((p.SpecialPriceStartDateTimeUtc.HasValue &&
										p.SpecialPriceStartDateTimeUtc.Value > nowUtc) ||
									   (p.SpecialPriceEndDateTimeUtc.HasValue &&
										p.SpecialPriceEndDateTimeUtc.Value < nowUtc))) &&
									 (p.Price <= ctx.PriceMax.Value)));
			}
			if (!ctx.ShowHidden)
			{
				//available dates
				query = query.Where(p =>
					(!p.AvailableStartDateTimeUtc.HasValue || p.AvailableStartDateTimeUtc.Value < nowUtc) &&
					(!p.AvailableEndDateTimeUtc.HasValue || p.AvailableEndDateTimeUtc.Value > nowUtc));
			}

			// searching by keyword
			if (!String.IsNullOrWhiteSpace(ctx.Keywords))
			{
				query = from p in query
						join lp in _localizedPropertyRepository.Table on p.Id equals lp.EntityId into p_lp
						from lp in p_lp.DefaultIfEmpty()
						from pt in p.ProductTags.DefaultIfEmpty()
						where (p.Name.Contains(ctx.Keywords)) ||
							  (ctx.SearchDescriptions && p.ShortDescription.Contains(ctx.Keywords)) ||
							  (ctx.SearchDescriptions && p.FullDescription.Contains(ctx.Keywords)) ||
							  (ctx.SearchSku && p.Sku.Contains(ctx.Keywords)) ||
							  (ctx.SearchProductTags && pt.Name.Contains(ctx.Keywords)) ||
							//localized values
							  (searchLocalizedValue && lp.LanguageId == ctx.LanguageId && lp.LocaleKeyGroup == "Product" && lp.LocaleKey == "Name" && lp.LocaleValue.Contains(ctx.Keywords)) ||
							  (ctx.SearchDescriptions && searchLocalizedValue && lp.LanguageId == ctx.LanguageId && lp.LocaleKeyGroup == "Product" && lp.LocaleKey == "ShortDescription" && lp.LocaleValue.Contains(ctx.Keywords)) ||
							  (ctx.SearchDescriptions && searchLocalizedValue && lp.LanguageId == ctx.LanguageId && lp.LocaleKeyGroup == "Product" && lp.LocaleKey == "FullDescription" && lp.LocaleValue.Contains(ctx.Keywords))
						//UNDONE search localized values in associated product tags
						select p;
			}

			if (!ctx.ShowHidden && !QuerySettings.IgnoreAcl)
			{
				query =
					from p in query
					join acl in _aclRepository.Table on new { pid = p.Id, pname = "Product" } equals new { pid = acl.EntityId, pname = acl.EntityName } into pacl
					from acl in pacl.DefaultIfEmpty()
					where !p.SubjectToAcl || allowedCustomerRolesIds.Contains(acl.CustomerRoleId)
					select p;
			}

			if (ctx.StoreId > 0 && !QuerySettings.IgnoreMultiStore)
			{
				query =
					from p in query
					join sm in _storeMappingRepository.Table on new { pid = p.Id, pname = "Product" } equals new { pid = sm.EntityId, pname = sm.EntityName } into psm
					from sm in psm.DefaultIfEmpty()
					where !p.LimitedToStores || ctx.StoreId == sm.StoreId
					select p;
			}

			// search by specs
			if (ctx.FilteredSpecs != null && ctx.FilteredSpecs.Count > 0)
			{
				query =
					from p in query
					where !ctx.FilteredSpecs.Except
					(
						p.ProductSpecificationAttributes
							.Where(psa => psa.AllowFiltering)
							.Select(psa => psa.SpecificationAttributeOptionId)
					).Any()
					select p;
			}

			// category filtering
			if (ctx.WithoutCategories.HasValue)
			{
				if (ctx.WithoutCategories.Value)
					query = query.Where(x => x.ProductCategories.Count == 0);
				else
					query = query.Where(x => x.ProductCategories.Count > 0);
			}
			else if (ctx.CategoryIds != null && ctx.CategoryIds.Count > 0)
			{
				//search in subcategories
				if (ctx.MatchAllcategories)
				{
					query = from p in query
							where ctx.CategoryIds.All(i => p.ProductCategories.Any(p2 => p2.CategoryId == i))
							from pc in p.ProductCategories
							where (!ctx.FeaturedProducts.HasValue || ctx.FeaturedProducts.Value == pc.IsFeaturedProduct)
							select p;
				}
				else
				{
					query = from p in query
							from pc in p.ProductCategories.Where(pc => ctx.CategoryIds.Contains(pc.CategoryId))
							where (!ctx.FeaturedProducts.HasValue || ctx.FeaturedProducts.Value == pc.IsFeaturedProduct)
							select p;
				}
			}

			// manufacturer filtering
			if (ctx.WithoutManufacturers.HasValue)
			{
				if (ctx.WithoutManufacturers.Value)
					query = query.Where(x => x.ProductManufacturers.Count == 0);
				else
					query = query.Where(x => x.ProductManufacturers.Count > 0);
			}
			else if (ctx.ManufacturerId > 0)
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

			return query.Select(selector);
		}

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

        public virtual IList<Product> GetLowStockProducts()
        {
			// Track inventory for product
			var query1 = from p in _productRepository.Table
						 orderby p.MinStockQuantity
						 where !p.Deleted &&
							p.ManageInventoryMethodId == (int)ManageInventoryMethod.ManageStock &&
							p.MinStockQuantity >= p.StockQuantity
						 select p;
			var products1 = query1.ToList();

			// Track inventory for product by product attributes
			var query2 = from p in _productRepository.Table
						 from pvac in p.ProductVariantAttributeCombinations
						 where !p.Deleted &&
							p.ManageInventoryMethodId == (int)ManageInventoryMethod.ManageStockByAttributes &&
							pvac.StockQuantity <= 0
						 select p;

			// only distinct products (group by ID)
			// if we use standard Distinct() method, then all fields will be compared (low performance)
			query2 = from p in query2
					 group p by p.Id into pGroup
					 orderby pGroup.Key
					 select pGroup.FirstOrDefault();
			var products2 = query2.ToList();

			var result = new List<Product>();
			result.AddRange(products1);
			result.AddRange(products2);
            return result;
        }

		public virtual Product GetProductBySku(string sku)
		{
			if (String.IsNullOrEmpty(sku))
				return null;

			sku = sku.Trim();

			var query = from p in _productRepository.Table
						orderby p.DisplayOrder, p.Id
						where !p.Deleted && p.Sku == sku
						select p;
			var product = query.FirstOrDefault();
			return product;
		}

        public virtual Product GetProductByGtin(string gtin)
        {
            if (String.IsNullOrEmpty(gtin))
                return null;

            gtin = gtin.Trim();

            var query = from p in _productRepository.Table
                        orderby p.Id
                        where !p.Deleted &&
                        p.Gtin == gtin
                        select p;
            var product = query.FirstOrDefault();
            return product;
        }

		public virtual Product GetProductByManufacturerPartNumber(string manufacturerPartNumber)
		{
			if (manufacturerPartNumber.IsEmpty())
				return null;

			manufacturerPartNumber = manufacturerPartNumber.Trim();

			var product = _productRepository.Table
				.Where(x => !x.Deleted && x.ManufacturerPartNumber == manufacturerPartNumber)
				.OrderBy(x => x.Id)
				.FirstOrDefault();

			return product;
		}

		public virtual Product GetProductByName(string name)
		{
			if (name.IsEmpty())
				return null;

			name = name.Trim();

			var product = _productRepository.Table
				.Where(x => !x.Deleted && x.Name == name)
				.OrderBy(x => x.Id)
				.FirstOrDefault();

			return product;
		}

		public virtual AdjustInventoryResult AdjustInventory(OrganizedShoppingCartItem sci, bool decrease)
		{
			if (sci == null)
				throw new ArgumentNullException("cartItem");

			if (sci.Item.Product.ProductType == ProductType.BundledProduct && sci.Item.Product.BundlePerItemShoppingCart)
			{
				if (sci.ChildItems != null)
				{
					foreach (var child in sci.ChildItems.Where(x => x.Item.Id != sci.Item.Id))
					{
						AdjustInventory(child.Item.Product, decrease, sci.Item.Quantity * child.Item.Quantity, child.Item.AttributesXml);
					}
				}
				return new AdjustInventoryResult();
			}
			else
			{
				return AdjustInventory(sci.Item.Product, decrease, sci.Item.Quantity, sci.Item.AttributesXml);
			}
		}

		public virtual AdjustInventoryResult AdjustInventory(OrderItem orderItem, bool decrease, int quantity)
		{
			if (orderItem == null)
				throw new ArgumentNullException("orderItem");

			if (orderItem.Product.ProductType == ProductType.BundledProduct && orderItem.Product.BundlePerItemShoppingCart)
			{
				if (orderItem.BundleData.HasValue())
				{
					var bundleData = orderItem.GetBundleData();
					if (bundleData.Count > 0)
					{
						var products = GetProductsByIds(bundleData.Select(x => x.ProductId).ToArray());

						foreach (var item in bundleData)
						{
							var product = products.FirstOrDefault(x => x.Id == item.ProductId);
							if (product != null)
								AdjustInventory(product, decrease, quantity * item.Quantity, item.AttributesXml);
						}
					}
				}
				return new AdjustInventoryResult();
			}
			else
			{
				return AdjustInventory(orderItem.Product, decrease, quantity, orderItem.AttributesXml);
			}
		}

		public virtual AdjustInventoryResult AdjustInventory(Product product, bool decrease, int quantity, string attributesXml)
        {
			if (product == null)
				throw new ArgumentNullException("product");

			var result = new AdjustInventoryResult();

			switch (product.ManageInventoryMethod)
            {
                case ManageInventoryMethod.DontManageStock:
                    {
                        //do nothing
                    }
					break;
                case ManageInventoryMethod.ManageStock:
                    {
						result.StockQuantityOld = product.StockQuantity;
                        if (decrease)
							result.StockQuantityNew = product.StockQuantity - quantity;
						else
							result.StockQuantityNew = product.StockQuantity + quantity;

						bool newPublished = product.Published;
						bool newDisableBuyButton = product.DisableBuyButton;
						bool newDisableWishlistButton = product.DisableWishlistButton;

                        //check if minimum quantity is reached
                        switch (product.LowStockActivity)
                        {
                            case LowStockActivity.DisableBuyButton:
								newDisableBuyButton = product.MinStockQuantity >= result.StockQuantityNew;
								newDisableWishlistButton = product.MinStockQuantity >= result.StockQuantityNew;
                                break;
                            case LowStockActivity.Unpublish:
								newPublished = product.MinStockQuantity <= result.StockQuantityNew;
                                break;
                        }

						product.StockQuantity = result.StockQuantityNew;
						product.DisableBuyButton = newDisableBuyButton;
						product.DisableWishlistButton = newDisableWishlistButton;
						product.Published = newPublished;

						UpdateProduct(product);

                        //send email notification
						if (decrease && product.NotifyAdminForQuantityBelow > result.StockQuantityNew)
                            _workflowMessageService.SendQuantityBelowStoreOwnerNotification(product, _localizationSettings.DefaultAdminLanguageId);                        
                    }
                    break;
                case ManageInventoryMethod.ManageStockByAttributes:
                    {
                        var combination = _productAttributeParser.FindProductVariantAttributeCombination(product.Id, attributesXml);
                        if (combination != null)
                        {
							result.StockQuantityOld = combination.StockQuantity;
                            if (decrease)
								result.StockQuantityNew = combination.StockQuantity - quantity;
                            else
								result.StockQuantityNew = combination.StockQuantity + quantity;

							combination.StockQuantity = result.StockQuantityNew;
                            _productAttributeService.UpdateProductVariantAttributeCombination(combination);
                        }
                    }
                    break;
                default:
                    break;
            }

			var attributeValues = _productAttributeParser.ParseProductVariantAttributeValues(attributesXml);

			attributeValues
				.Where(x => x.ValueType == ProductVariantAttributeValueType.ProductLinkage)
				.ToList()
				.Each(x =>
			{
				var linkedProduct = GetProductById(x.LinkedProductId);
				if (linkedProduct != null)
					AdjustInventory(linkedProduct, decrease, quantity * x.Quantity, "");
			});

			return result;
        }
        
		public virtual void UpdateHasTierPricesProperty(Product product)
        {
			if (product == null)
				throw new ArgumentNullException("product");

			var prevValue = product.HasTierPrices;
			product.HasTierPrices = product.TierPrices.Count > 0;
			if (prevValue != product.HasTierPrices)
				UpdateProduct(product);
        }

		public virtual void UpdateLowestAttributeCombinationPriceProperty(Product product)
		{
			if (product == null)
				throw new ArgumentNullException("product");

			var prevValue = product.LowestAttributeCombinationPrice;

			product.LowestAttributeCombinationPrice = _productAttributeService.GetLowestCombinationPrice(product.Id);

			if (prevValue != product.LowestAttributeCombinationPrice)
				UpdateProduct(product);
		}

		public virtual void UpdateHasDiscountsApplied(Product product)
        {
			if (product == null)
				throw new ArgumentNullException("product");

			var prevValue = product.HasDiscountsApplied;
			product.HasDiscountsApplied = product.AppliedDiscounts.Count > 0;
			if (prevValue != product.HasDiscountsApplied)
				UpdateProduct(product);
        }

		public virtual Multimap<int, ProductTag> GetProductTagsByProductIds(int[] productIds)
		{
			Guard.NotNull(productIds, nameof(productIds));

			var query = _productRepository.TableUntracked
				.Expand(x => x.ProductTags)
				.Where(x => productIds.Contains(x.Id))
				.Select(x => new
				{
					ProductId = x.Id,
					Tags = x.ProductTags
				});

			var map = new Multimap<int, ProductTag>();

			foreach (var item in query.ToList())
			{
				foreach (var tag in item.Tags)
					map.Add(item.ProductId, tag);
			}

			return map;
		}

		public virtual Multimap<int, Discount> GetAppliedDiscountsByProductIds(int[] productIds)
		{
			Guard.NotNull(productIds, nameof(productIds));

			var query = _productRepository.TableUntracked
				.Expand(x => x.AppliedDiscounts.Select(y => y.DiscountRequirements))
				.Where(x => productIds.Contains(x.Id))
				.Select(x => new
				{
					ProductId = x.Id,
					Discounts = x.AppliedDiscounts
				});

			var map = new Multimap<int, Discount>();

			foreach (var item in query.ToList())
			{
				foreach (var discount in item.Discounts)
					map.Add(item.ProductId, discount);
			}

			return map;
		}

		public virtual Multimap<int, ProductSpecificationAttribute> GetProductSpecificationAttributesByProductIds(int[] productIds)
		{
			Guard.NotNull(productIds, nameof(productIds));

			var query = _productSpecificationAttributeRepository.TableUntracked
				.Expand(x => x.SpecificationAttributeOption)
				.Expand(x => x.SpecificationAttributeOption.SpecificationAttribute)
				.Where(x => productIds.Contains(x.ProductId));

			var map = query
				.OrderBy(x => x.DisplayOrder)
				.ToList()
				.ToMultimap(x => x.ProductId, x => x);

			return map;
		}

        #endregion

        #region Related products

        public virtual void DeleteRelatedProduct(RelatedProduct relatedProduct)
        {
            if (relatedProduct == null)
                throw new ArgumentNullException("relatedProduct");

            _relatedProductRepository.Delete(relatedProduct);

            //event notification
            _services.EventPublisher.EntityDeleted(relatedProduct);
        }

        public virtual IList<RelatedProduct> GetRelatedProductsByProductId1(int productId1, bool showHidden = false)
        {
            var query = from rp in _relatedProductRepository.Table
                        join p in _productRepository.Table on rp.ProductId2 equals p.Id
                        where rp.ProductId1 == productId1 && !p.Deleted && (showHidden || p.Published)
                        orderby rp.DisplayOrder
                        select rp;

            var relatedProducts = query.ToList();
            return relatedProducts;
        }

        public virtual RelatedProduct GetRelatedProductById(int relatedProductId)
        {
            if (relatedProductId == 0)
                return null;
            
            var relatedProduct = _relatedProductRepository.GetById(relatedProductId);
            return relatedProduct;
        }

        public virtual void InsertRelatedProduct(RelatedProduct relatedProduct)
        {
            if (relatedProduct == null)
                throw new ArgumentNullException("relatedProduct");

            _relatedProductRepository.Insert(relatedProduct);

            //event notification
            _services.EventPublisher.EntityInserted(relatedProduct);
        }

        public virtual void UpdateRelatedProduct(RelatedProduct relatedProduct)
        {
            if (relatedProduct == null)
                throw new ArgumentNullException("relatedProduct");

            _relatedProductRepository.Update(relatedProduct);

            //event notification
            _services.EventPublisher.EntityUpdated(relatedProduct);
        }

		public virtual int EnsureMutuallyRelatedProducts(int productId1)
		{
			var relatedProducts = GetRelatedProductsByProductId1(productId1, true);
			var productIds = relatedProducts.Select(x => x.ProductId2).ToList();

			if (productIds.Count > 0 && !productIds.Any(x => x == productId1))
				productIds.Add(productId1);

			int count = EnsureMutuallyRelatedProducts(productIds);
			return count;
		}

        #endregion

        #region Cross-sell products

        public virtual void DeleteCrossSellProduct(CrossSellProduct crossSellProduct)
        {
            if (crossSellProduct == null)
                throw new ArgumentNullException("crossSellProduct");

            _crossSellProductRepository.Delete(crossSellProduct);

            //event notification
            _services.EventPublisher.EntityDeleted(crossSellProduct);
        }

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

        public virtual CrossSellProduct GetCrossSellProductById(int crossSellProductId)
        {
            if (crossSellProductId == 0)
                return null;

            var crossSellProduct = _crossSellProductRepository.GetById(crossSellProductId);
            return crossSellProduct;
        }

        public virtual void InsertCrossSellProduct(CrossSellProduct crossSellProduct)
        {
            if (crossSellProduct == null)
                throw new ArgumentNullException("crossSellProduct");

            _crossSellProductRepository.Insert(crossSellProduct);

            //event notification
            _services.EventPublisher.EntityInserted(crossSellProduct);
        }

        public virtual void UpdateCrossSellProduct(CrossSellProduct crossSellProduct)
        {
            if (crossSellProduct == null)
                throw new ArgumentNullException("crossSellProduct");

            _crossSellProductRepository.Update(crossSellProduct);

            //event notification
            _services.EventPublisher.EntityUpdated(crossSellProduct);
        }

		public virtual IList<Product> GetCrosssellProductsByShoppingCart(IList<OrganizedShoppingCartItem> cart, int numberOfProducts)
        {
            var result = new List<Product>();

            if (numberOfProducts == 0)
                return result;

            if (cart == null || cart.Count == 0)
                return result;

            var cartProductIds = new List<int>();
            foreach (var sci in cart)
            {
                int prodId = sci.Item.ProductId;
                if (!cartProductIds.Contains(prodId))
                    cartProductIds.Add(prodId);
            }

            foreach (var sci in cart)
            {
                var crossSells = GetCrossSellProductsByProductId1(sci.Item.ProductId);
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

                        //add a product to result
                        result.Add(productToAdd);
                        if (result.Count >= numberOfProducts)
                            return result;
                    }
                }
            }
            return result;
        }

		public virtual int EnsureMutuallyCrossSellProducts(int productId1)
		{
			var crossSellProducts = GetCrossSellProductsByProductId1(productId1, true);
			var productIds = crossSellProducts.Select(x => x.ProductId2).ToList();

			if (productIds.Count > 0 && !productIds.Any(x => x == productId1))
				productIds.Add(productId1);

			int count = EnsureMutuallyCrossSellProducts(productIds);
			return count;
		}

        #endregion
        
        #region Tier prices
        
        public virtual void DeleteTierPrice(TierPrice tierPrice)
        {
            if (tierPrice == null)
                throw new ArgumentNullException("tierPrice");

            _tierPriceRepository.Delete(tierPrice);

            //event notification
            _services.EventPublisher.EntityDeleted(tierPrice);
        }

        public virtual TierPrice GetTierPriceById(int tierPriceId)
        {
            if (tierPriceId == 0)
                return null;
            
            var tierPrice = _tierPriceRepository.GetById(tierPriceId);
            return tierPrice;
        }

		public virtual Multimap<int, TierPrice> GetTierPricesByProductIds(int[] productIds, Customer customer = null, int storeId = 0)
		{
			Guard.NotNull(productIds, nameof(productIds));

			var query =
				from x in _tierPriceRepository.TableUntracked
				where productIds.Contains(x.ProductId)
				select x;

			if (storeId != 0)
				query = query.Where(x => x.StoreId == 0 || x.StoreId == storeId);

			query = query.OrderBy(x => x.ProductId).ThenBy(x => x.Quantity);

			var list = query.ToList();

			if (customer != null)
				list = list.FilterForCustomer(customer).ToList();

			var map = list
				.ToMultimap(x => x.ProductId, x => x);

			return map;
		}

        public virtual void InsertTierPrice(TierPrice tierPrice)
        {
            if (tierPrice == null)
                throw new ArgumentNullException("tierPrice");

            _tierPriceRepository.Insert(tierPrice);

            //event notification
            _services.EventPublisher.EntityInserted(tierPrice);
        }

        public virtual void UpdateTierPrice(TierPrice tierPrice)
        {
            if (tierPrice == null)
                throw new ArgumentNullException("tierPrice");

            _tierPriceRepository.Update(tierPrice);

            //event notification
            _services.EventPublisher.EntityUpdated(tierPrice);
        }

        #endregion

        #region Product pictures

        public virtual void DeleteProductPicture(ProductPicture productPicture)
        {
            if (productPicture == null)
                throw new ArgumentNullException("productPicture");

            UnassignDeletedPictureFromVariantCombinations(productPicture);

            _productPictureRepository.Delete(productPicture);

            //event notification
            _services.EventPublisher.EntityDeleted(productPicture);
        }

        private void UnassignDeletedPictureFromVariantCombinations(ProductPicture productPicture)
        {
            var picId = productPicture.Id;
            bool touched = false;

			var combinations =
				from c in this._productVariantAttributeCombinationRepository.Table
				where c.ProductId == productPicture.Product.Id && !String.IsNullOrEmpty(c.AssignedPictureIds)
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

            // save in one shot!
            if (touched)
            {
                _dbContext.SaveChanges();
            }
        }

        public virtual IList<ProductPicture> GetProductPicturesByProductId(int productId)
        {
            var query = from pp in _productPictureRepository.Table
                        where pp.ProductId == productId
                        orderby pp.DisplayOrder
                        select pp;
            var productPictures = query.ToList();
            return productPictures;
        }

		public virtual Multimap<int, ProductPicture> GetProductPicturesByProductIds(int[] productIds, bool onlyFirstPicture = false)
		{
			var query = 
				from pp in _productPictureRepository.TableUntracked.Expand(x => x.Picture)
				where productIds.Contains(pp.ProductId)
				orderby pp.ProductId, pp.DisplayOrder
				select pp;

			if (onlyFirstPicture)
			{
				var map = query.GroupBy(x => x.ProductId, x => x)
					.Select(x => x.FirstOrDefault())
					.ToList()
					.ToMultimap(x => x.ProductId, x => x);

				return map;
			}
			else
			{
				var map = query
					.ToList()
					.ToMultimap(x => x.ProductId, x => x);

				return map;
			}
		}

        public virtual ProductPicture GetProductPictureById(int productPictureId)
        {
            if (productPictureId == 0)
                return null;

            var pp = _productPictureRepository.GetById(productPictureId);
            return pp;
        }

        public virtual void InsertProductPicture(ProductPicture productPicture)
        {
            if (productPicture == null)
                throw new ArgumentNullException("productPicture");

            _productPictureRepository.Insert(productPicture);

            //event notification
            _services.EventPublisher.EntityInserted(productPicture);
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
            _services.EventPublisher.EntityUpdated(productPicture);
        }

        #endregion

		#region Bundled products

		public virtual void InsertBundleItem(ProductBundleItem bundleItem)
		{
			if (bundleItem == null)
				throw new ArgumentNullException("bundleItem");

			if (bundleItem.BundleProductId == 0)
				throw new SmartException("BundleProductId of a bundle item cannot be 0.");

			if (bundleItem.ProductId == 0)
				throw new SmartException("ProductId of a bundle item cannot be 0.");

			if (bundleItem.ProductId == bundleItem.BundleProductId)
				throw new SmartException("A bundle item cannot be an element of itself.");

			_productBundleItemRepository.Insert(bundleItem);

			//event notification
			_services.EventPublisher.EntityInserted(bundleItem);
		}

		public virtual void UpdateBundleItem(ProductBundleItem bundleItem)
		{
			if (bundleItem == null)
				throw new ArgumentNullException("bundleItem");

			_productBundleItemRepository.Update(bundleItem);

			//event notification
			_services.EventPublisher.EntityUpdated(bundleItem);
		}

		public virtual void DeleteBundleItem(ProductBundleItem bundleItem)
		{
			if (bundleItem == null)
				throw new ArgumentNullException("bundleItem");

			// remove bundles from shopping carts (otherwise bundle item cannot be deleted)
			var parentCartItemIds = _shoppingCartItemRepository.TableUntracked
				.Where(x => x.BundleItemId == bundleItem.Id && x.ParentItemId != null)
				.Select(x => x.ParentItemId)
				.ToList();

			if (parentCartItemIds.Any())
			{
				var cartItems = _shoppingCartItemRepository.Table
					.Where(x => parentCartItemIds.Contains(x.Id))
					.ToList();

				foreach (var parentItem in cartItems)
				{
					var childItems = _shoppingCartItemRepository.Table
						.Where(x => x.ParentItemId != null && x.ParentItemId.Value == parentItem.Id && x.Id != parentItem.Id)
						.ToList();

					childItems.Each(x => _shoppingCartItemRepository.Delete(x));

					_shoppingCartItemRepository.Delete(parentItem);
				}
			}

			// delete bundle item
			_productBundleItemRepository.Delete(bundleItem);

			// event notification
			_services.EventPublisher.EntityDeleted(bundleItem);
		}

		public virtual ProductBundleItem GetBundleItemById(int bundleItemId)
		{
			if (bundleItemId == 0)
				return null;

			return _productBundleItemRepository.GetById(bundleItemId);
		}

		public virtual IList<ProductBundleItemData> GetBundleItems(int bundleProductId, bool showHidden = false)
		{
			var query =
				from pbi in _productBundleItemRepository.Table
				join p in _productRepository.Table on pbi.ProductId equals p.Id
				where pbi.BundleProductId == bundleProductId && !p.Deleted && (showHidden || (pbi.Published && p.Published))
				orderby pbi.DisplayOrder
				select pbi;

			query = query.Expand(x => x.Product);

			var bundleItemData = new List<ProductBundleItemData>();

			query.ToList().Each(x => bundleItemData.Add(new ProductBundleItemData(x)));

			return bundleItemData;
		}

		public virtual Multimap<int, ProductBundleItem> GetBundleItemsByProductIds(int[] productIds, bool showHidden = false)
		{
			Guard.NotNull(productIds, nameof(productIds));

			var query =
				from pbi in _productBundleItemRepository.TableUntracked
				join p in _productRepository.TableUntracked on pbi.ProductId equals p.Id
				where productIds.Contains(pbi.BundleProductId) && !p.Deleted && (showHidden || (pbi.Published && p.Published))
				orderby pbi.DisplayOrder
				select pbi;

			var map = query.Expand(x => x.Product)
				.ToList()
				.ToMultimap(x => x.BundleProductId, x => x);

			return map;
		}

		#endregion
	}
}
