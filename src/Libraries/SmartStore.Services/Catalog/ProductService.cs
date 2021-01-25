using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using SmartStore.Collections;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Discounts;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Orders;
using SmartStore.Data.Caching;
using SmartStore.Services.Media;
using SmartStore.Services.Orders;

namespace SmartStore.Services.Catalog
{
    public partial class ProductService : IProductService
    {
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<RelatedProduct> _relatedProductRepository;
        private readonly IRepository<CrossSellProduct> _crossSellProductRepository;
        private readonly IRepository<TierPrice> _tierPriceRepository;
        private readonly IRepository<ProductMediaFile> _productFileRepository;
        private readonly IRepository<ProductVariantAttributeCombination> _productVariantAttributeCombinationRepository;
        private readonly IRepository<ProductBundleItem> _productBundleItemRepository;
        private readonly IRepository<ShoppingCartItem> _shoppingCartItemRepository;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IDbContext _dbContext;
        private readonly LocalizationSettings _localizationSettings;
        private readonly ICommonServices _services;

        public ProductService(
            IRepository<Product> productRepository,
            IRepository<RelatedProduct> relatedProductRepository,
            IRepository<CrossSellProduct> crossSellProductRepository,
            IRepository<TierPrice> tierPriceRepository,
            IRepository<ProductMediaFile> productPictureRepository,
            IRepository<ProductVariantAttributeCombination> productVariantAttributeCombinationRepository,
            IRepository<ProductBundleItem> productBundleItemRepository,
            IRepository<ShoppingCartItem> shoppingCartItemRepository,
            IProductAttributeService productAttributeService,
            IProductAttributeParser productAttributeParser,
            IDbContext dbContext,
            LocalizationSettings localizationSettings,
            ICommonServices services)
        {
            _productRepository = productRepository;
            _relatedProductRepository = relatedProductRepository;
            _crossSellProductRepository = crossSellProductRepository;
            _tierPriceRepository = tierPriceRepository;
            _productFileRepository = productPictureRepository;
            _productVariantAttributeCombinationRepository = productVariantAttributeCombinationRepository;
            _productBundleItemRepository = productBundleItemRepository;
            _shoppingCartItemRepository = shoppingCartItemRepository;
            _productAttributeService = productAttributeService;
            _productAttributeParser = productAttributeParser;
            _dbContext = dbContext;
            _localizationSettings = localizationSettings;
            _services = services;
        }

        #region Utilities

        protected virtual int EnsureMutuallyRelatedProducts(List<int> productIds)
        {
            int count = 0;

            foreach (int id1 in productIds)
            {
                var mutualAssociations = (
                    from rp in _relatedProductRepository.Table
                    join p in _productRepository.Table on rp.ProductId2 equals p.Id
                    where rp.ProductId2 == id1 && !p.Deleted
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
                    where rp.ProductId2 == id1 && !p.Deleted
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

        public virtual int CountAllProducts()
        {
            var query = _productRepository.Table;
            query = query.Where(x => !x.Deleted);

            return query.Count();

        }

        public virtual int CountAllProductVariants()
        {
            var query = _productVariantAttributeCombinationRepository.Table;
            query = query.Where(x => x.IsActive);

            return query.Count();
        }

        public virtual void DeleteProduct(Product product)
        {
            Guard.NotNull(product, nameof(product));

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
            if (productIds == null || !productIds.Any())
            {
                return new List<Product>();
            }

            var query = from p in _productRepository.Table
                        where productIds.Contains(p.Id)
                        select p;

            if (flags > ProductLoadFlags.None)
            {
                query = ApplyLoadFlags(query, flags);
            }

            var products = query.ToList();

            // Sort by passed identifier sequence.
            return products.OrderBySequence(productIds).ToList();
        }

        public virtual Product GetProductBySystemName(string systemName)
        {
            if (systemName.IsEmpty())
            {
                return null;
            }

            var product = _productRepository.Table.FirstOrDefault(x => x.SystemName == systemName && x.IsSystemProduct);
            return product;
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
            Guard.NotNull(product, nameof(product));

            _productRepository.Insert(product);
        }

        public virtual void UpdateProduct(Product product)
        {
            Guard.NotNull(product, nameof(product));

            _productRepository.Update(product);
        }

        public virtual void UpdateProductReviewTotals(Product product)
        {
            Guard.NotNull(product, nameof(product));

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
                    approvedTotalReviews++;
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
                        where !p.Deleted && p.Gtin == gtin
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

        public virtual Product GetProductByIdentificationNumber(
            string identificationNumber,
            out ProductVariantAttributeCombination attributeCombination,
            bool includeHidden = false,
            bool untracked = true)
        {
            attributeCombination = null;

            if (string.IsNullOrWhiteSpace(identificationNumber))
            {
                return null;
            }

            identificationNumber = identificationNumber.Trim();

            var pq =
                from x in untracked ? _productRepository.TableUntracked : _productRepository.Table
                where !x.Deleted && (x.Sku == identificationNumber || x.ManufacturerPartNumber == identificationNumber || x.Gtin == identificationNumber)
                select x;

            if (!includeHidden)
            {
                pq = pq.Where(x => x.Visibility <= ProductVisibility.SearchResults && x.Published);
            }

            var product = pq.FirstOrDefault();
            if (product != null)
            {
                return product;
            }

            var pvaq =
                from x in untracked ? _productVariantAttributeCombinationRepository.TableUntracked : _productVariantAttributeCombinationRepository.Table
                where !x.Product.Deleted && (x.Sku == identificationNumber || x.ManufacturerPartNumber == identificationNumber || x.Gtin == identificationNumber)
                select x;

            if (!includeHidden)
            {
                pvaq = pvaq.Where(x => x.Product.Visibility <= ProductVisibility.SearchResults && x.Product.Published && x.IsActive);
            }

            attributeCombination = pvaq.FirstOrDefault();

            return attributeCombination?.Product;
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
            Guard.NotNull(orderItem, nameof(orderItem));

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
            Guard.NotNull(product, nameof(product));

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
                            _services.MessageFactory.SendQuantityBelowStoreOwnerNotification(product, _localizationSettings.DefaultAdminLanguageId);
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
            Guard.NotNull(product, nameof(product));

            var prevValue = product.HasTierPrices;
            product.HasTierPrices = product.TierPrices.Count > 0;
            if (prevValue != product.HasTierPrices)
                UpdateProduct(product);
        }

        public virtual void UpdateLowestAttributeCombinationPriceProperty(Product product)
        {
            Guard.NotNull(product, nameof(product));

            var prevValue = product.LowestAttributeCombinationPrice;

            product.LowestAttributeCombinationPrice = _productAttributeService.GetLowestCombinationPrice(product.Id);

            if (prevValue != product.LowestAttributeCombinationPrice)
                UpdateProduct(product);
        }

        public virtual void UpdateHasDiscountsApplied(Product product)
        {
            Guard.NotNull(product, nameof(product));

            var prevValue = product.HasDiscountsApplied;
            product.HasDiscountsApplied = product.AppliedDiscounts.Count > 0;
            if (prevValue != product.HasDiscountsApplied)
                UpdateProduct(product);
        }

        public virtual Multimap<int, ProductTag> GetProductTagsByProductIds(int[] productIds, bool includeHidden = false)
        {
            Guard.NotNull(productIds, nameof(productIds));

            var map = new Multimap<int, ProductTag>();
            if (!productIds.Any())
            {
                return map;
            }

            /// <seealso cref="ProductTagService.GetProductCount(int, bool)"/>
            var productQuery = _productRepository.TableUntracked
                .Expand(x => x.ProductTags)
                .Where(x => productIds.Contains(x.Id) && !x.Deleted && !x.IsSystemProduct);

            if (!includeHidden)
            {
                productQuery = productQuery.Where(x => x.Visibility == ProductVisibility.Full && x.Published);
            }

            var items = productQuery
                .Select(x => new
                {
                    ProductId = x.Id,
                    Tags = x.ProductTags.Where(y => includeHidden || y.Published)
                })
                .ToList();

            foreach (var item in items)
            {
                foreach (var tag in item.Tags)
                {
                    map.Add(item.ProductId, tag);
                }
            }

            return map;
        }

        public virtual Multimap<int, Product> GetAssociatedProductsByProductIds(int[] productIds, bool showHidden = false)
        {
            Guard.NotNull(productIds, nameof(productIds));

            if (!productIds.Any())
            {
                return new Multimap<int, Product>();
            }

            // Ignore multistore. Expect multistore setting for associated products is the same as for parent grouped product.
            var query = _productRepository.TableUntracked
                .Where(x => productIds.Contains(x.ParentGroupedProductId) && !x.Deleted && (showHidden || x.Published))
                .OrderBy(x => x.ParentGroupedProductId)
                .ThenBy(x => x.DisplayOrder);

            var associatedProducts = query.ToList();

            var map = associatedProducts
                .ToMultimap(x => x.ParentGroupedProductId, x => x);

            return map;
        }

        public virtual Multimap<int, Discount> GetAppliedDiscountsByProductIds(int[] productIds)
        {
            Guard.NotNull(productIds, nameof(productIds));

            var map = new Multimap<int, Discount>();
            if (!productIds.Any())
            {
                return map;
            }

            var query = _productRepository.Table // .TableUntracked does not seem to eager load
                .Expand(x => x.AppliedDiscounts.Select(y => y.RuleSets))
                .Where(x => productIds.Contains(x.Id))
                .Select(x => new
                {
                    ProductId = x.Id,
                    Discounts = x.AppliedDiscounts
                });

            foreach (var item in query.ToList())
            {
                map.AddRange(item.ProductId, item.Discounts);
            }

            return map;
        }

        #endregion

        #region Related products

        public virtual void DeleteRelatedProduct(RelatedProduct relatedProduct)
        {
            Guard.NotNull(relatedProduct, nameof(relatedProduct));

            _relatedProductRepository.Delete(relatedProduct);
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
            Guard.NotNull(relatedProduct, nameof(relatedProduct));

            _relatedProductRepository.Insert(relatedProduct);
        }

        public virtual void UpdateRelatedProduct(RelatedProduct relatedProduct)
        {
            Guard.NotNull(relatedProduct, nameof(relatedProduct));

            _relatedProductRepository.Update(relatedProduct);
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
            Guard.NotNull(crossSellProduct, nameof(crossSellProduct));

            _crossSellProductRepository.Delete(crossSellProduct);
        }

        public virtual IList<CrossSellProduct> GetCrossSellProductsByProductId1(int productId1, bool showHidden = false)
        {
            var query = from csp in _crossSellProductRepository.Table
                        join p in _productRepository.Table on csp.ProductId2 equals p.Id
                        where csp.ProductId1 == productId1 && !p.Deleted && (showHidden || p.Published)
                        orderby csp.Id
                        select csp;

            var crossSellProducts = query.ToList();
            return crossSellProducts;
        }

        public virtual IList<CrossSellProduct> GetCrossSellProductsByProductIds(IEnumerable<int> productIds, bool showHidden = false)
        {
            Guard.NotNull(productIds, nameof(productIds));

            var query = from csp in _crossSellProductRepository.Table
                        join p in _productRepository.Table on csp.ProductId2 equals p.Id
                        where productIds.Contains(csp.ProductId1) && !p.Deleted && (showHidden || p.Published)
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
            Guard.NotNull(crossSellProduct, nameof(crossSellProduct));

            _crossSellProductRepository.Insert(crossSellProduct);
        }

        public virtual void UpdateCrossSellProduct(CrossSellProduct crossSellProduct)
        {
            Guard.NotNull(crossSellProduct, nameof(crossSellProduct));

            _crossSellProductRepository.Update(crossSellProduct);
        }

        public virtual IList<Product> GetCrosssellProductsByShoppingCart(IList<OrganizedShoppingCartItem> cart, int numberOfProducts)
        {
            var result = new List<Product>();

            if (numberOfProducts == 0)
                return result;

            if (cart == null || cart.Count == 0)
                return result;

            var cartProductIds = new HashSet<int>(cart.Select(x => x.Item.ProductId));
            var csItems = GetCrossSellProductsByProductIds(cartProductIds);
            var productIdsToLoad = new HashSet<int>(csItems.Select(x => x.ProductId2).Except(cartProductIds));

            if (productIdsToLoad.Count > 0)
            {
                result.AddRange(GetProductsByIds(productIdsToLoad.Take(numberOfProducts).ToArray()));
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
            Guard.NotNull(tierPrice, nameof(tierPrice));

            _tierPriceRepository.Delete(tierPrice);
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

            if (!productIds.Any())
            {
                return new Multimap<int, TierPrice>();
            }

            var query = _tierPriceRepository.TableUntracked
                .Include(x => x.CustomerRole)
                .Where(x => productIds.Contains(x.ProductId));

            if (storeId != 0)
                query = query.Where(x => x.StoreId == 0 || x.StoreId == storeId);

            var list = query
                .ToList()
                // Sorting locally is most likely faster
                .OrderBy(x => x.ProductId)
                .ThenBy(x => x.Quantity)
                .AsEnumerable();

            if (customer != null)
                list = list.FilterForCustomer(customer).ToList();

            var map = list.ToMultimap(x => x.ProductId, x => x);

            return map;
        }

        public virtual void InsertTierPrice(TierPrice tierPrice)
        {
            Guard.NotNull(tierPrice, nameof(tierPrice));

            _tierPriceRepository.Insert(tierPrice);
        }

        public virtual void UpdateTierPrice(TierPrice tierPrice)
        {
            Guard.NotNull(tierPrice, nameof(tierPrice));

            _tierPriceRepository.Update(tierPrice);
        }

        #endregion

        #region Product pictures

        public virtual void DeleteProductPicture(ProductMediaFile productPicture)
        {
            Guard.NotNull(productPicture, nameof(productPicture));

            UnassignDeletedPictureFromVariantCombinations(productPicture);

            _productFileRepository.Delete(productPicture);
        }

        private void UnassignDeletedPictureFromVariantCombinations(ProductMediaFile productPicture)
        {
            var picId = productPicture.MediaFileId;
            bool touched = false;

            var combinations =
                from c in _productVariantAttributeCombinationRepository.Table
                where c.ProductId == productPicture.Product.Id && !String.IsNullOrEmpty(c.AssignedMediaFileIds)
                select c;

            foreach (var c in combinations)
            {
                var ids = c.GetAssignedMediaIds().ToList();
                if (ids.Contains(picId))
                {
                    ids.Remove(picId);
                    //c.AssignedPictureIds = ids.Count > 0 ? String.Join<int>(",", ids) : null;
                    c.SetAssignedMediaIds(ids.ToArray());
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

        public virtual IList<ProductMediaFile> GetProductPicturesByProductId(int productId, int numberOfPictures = 0)
        {
            if (productId == 0)
            {
                return new List<ProductMediaFile>();
            }

            var query = _productFileRepository.Table
                .Include(x => x.MediaFile)
                .Where(x => x.ProductId == productId);

            if (numberOfPictures > 0)
            {
                query = query
                    .OrderBy(x => x.DisplayOrder)
                    .Take(() => numberOfPictures);
            }
            else
            {
                query = query.OrderBy(x => x.DisplayOrder);
            }

            var productFiles = query.ToList();
            return productFiles;
        }

        public virtual Multimap<int, ProductMediaFile> GetProductPicturesByProductIds(
            int[] productIds,
            int? maxPicturesPerProduct = null,
            MediaLoadFlags flags = MediaLoadFlags.None)
        {
            if (!(productIds?.Any() ?? false))
            {
                return new Multimap<int, ProductMediaFile>();
            }

            var take = maxPicturesPerProduct ?? int.MaxValue;

            var query = _productFileRepository.TableUntracked
                .Where(pf => productIds.Contains(pf.ProductId))
                .GroupBy(pf => pf.ProductId, x => x)
                .SelectMany(pf => pf.OrderBy(x => x.DisplayOrder).Take(take));

            // For eager loading apply Include() after GroupBy().
            if (flags == MediaLoadFlags.None)
            {
                query = query.Include(pp => pp.MediaFile);
            }
            else
            {
                if (flags.HasFlag(MediaLoadFlags.WithBlob))
                {
                    query = query.Include(pp => pp.MediaFile.MediaStorage);
                }
                if (flags.HasFlag(MediaLoadFlags.WithFolder))
                {
                    query = query.Include(pp => pp.MediaFile.Folder);
                }
                if (flags.HasFlag(MediaLoadFlags.WithTags))
                {
                    query = query.Include(pp => pp.MediaFile.Tags);
                }
                if (flags.HasFlag(MediaLoadFlags.WithTracks))
                {
                    query = query.Include(pp => pp.MediaFile.Tracks);
                }
            }

            var map = query
                .ToList()
                .ToMultimap(x => x.ProductId, x => x);

            return map;
        }

        public virtual ProductMediaFile GetProductPictureById(int productPictureId)
        {
            if (productPictureId == 0)
                return null;

            var pp = _productFileRepository.GetById(productPictureId);
            return pp;
        }

        public virtual void InsertProductPicture(ProductMediaFile productPicture)
        {
            Guard.NotNull(productPicture, nameof(productPicture));

            _productFileRepository.Insert(productPicture);
        }

        /// <summary>
        /// Updates a product picture
        /// </summary>
        /// <param name="productPicture">Product picture</param>
        public virtual void UpdateProductPicture(ProductMediaFile productPicture)
        {
            Guard.NotNull(productPicture, nameof(productPicture));

            _productFileRepository.Update(productPicture);
        }

        #endregion

        #region Bundled products

        public virtual void InsertBundleItem(ProductBundleItem bundleItem)
        {
            Guard.NotNull(bundleItem, nameof(bundleItem));

            if (bundleItem.BundleProductId == 0)
                throw new SmartException("BundleProductId of a bundle item cannot be 0.");

            if (bundleItem.ProductId == 0)
                throw new SmartException("ProductId of a bundle item cannot be 0.");

            if (bundleItem.ProductId == bundleItem.BundleProductId)
                throw new SmartException("A bundle item cannot be an element of itself.");

            _productBundleItemRepository.Insert(bundleItem);
        }

        public virtual void UpdateBundleItem(ProductBundleItem bundleItem)
        {
            Guard.NotNull(bundleItem, nameof(bundleItem));

            _productBundleItemRepository.Update(bundleItem);
        }

        public virtual void DeleteBundleItem(ProductBundleItem bundleItem)
        {
            Guard.NotNull(bundleItem, nameof(bundleItem));

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

            query = query.Include(x => x.Product);

            var bundleItemData = new List<ProductBundleItemData>();

            query.ToList().Each(x => bundleItemData.Add(new ProductBundleItemData(x)));

            return bundleItemData;
        }

        public virtual Multimap<int, ProductBundleItem> GetBundleItemsByProductIds(int[] productIds, bool showHidden = false)
        {
            Guard.NotNull(productIds, nameof(productIds));

            if (!productIds.Any())
            {
                return new Multimap<int, ProductBundleItem>();
            }

            var query =
                from pbi in _productBundleItemRepository.TableUntracked
                join p in _productRepository.TableUntracked on pbi.ProductId equals p.Id
                where productIds.Contains(pbi.BundleProductId) && !p.Deleted && (showHidden || (pbi.Published && p.Published))
                orderby pbi.DisplayOrder
                select pbi;

            var map = query
                .Include(x => x.Product)
                .Include(x => x.BundleProduct)
                .ToList()
                .ToMultimap(x => x.BundleProductId, x => x);

            return map;
        }

        public virtual bool IsBundleItem(int productId)
        {
            if (productId == 0)
            {
                return false;
            }

            var query =
                from pbi in _productBundleItemRepository.TableUntracked
                join bundle in _productRepository.TableUntracked on pbi.BundleProductId equals bundle.Id
                where pbi.ProductId == productId && !bundle.Deleted
                select pbi;

            var result = query.Any();
            return result;
        }

        #endregion
    }
}
