using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Security;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Events;
using SmartStore.Core.Search;
using SmartStore.Services.Catalog;
using SmartStore.Services.Directory;
using SmartStore.Services.Search;

namespace SmartStore.Services.Tests.Search
{
    [TestFixture]
    public class LinqCatalogSearchServiceTests
    {
        private ICommonServices _services;
        private LinqCatalogSearchService _linqCatalogSearchService;
        private IProductService _productService;
        private IRepository<Product> _productRepository;
        private IRepository<LocalizedProperty> _localizedPropertyRepository;
        private IRepository<StoreMapping> _storeMappingRepository;
        private IRepository<AclRecord> _aclRepository;
        private IDeliveryTimeService _deliveryTimeService;
        private IManufacturerService _manufacturerService;
        private ICategoryService _categoryService;
        private IEventPublisher _eventPublisher;

        private void InitMocks(CatalogSearchQuery query, IEnumerable<Product> products, IEnumerable<LocalizedProperty> localized)
        {
            _productRepository.Expect(x => x.Table).Return(products.AsQueryable());
            _productRepository.Expect(x => x.TableUntracked).Return(products.AsQueryable());

            _localizedPropertyRepository.Expect(x => x.Table).Return((localized ?? new List<LocalizedProperty>()).AsQueryable());
            _localizedPropertyRepository.Expect(x => x.TableUntracked).Return((localized ?? new List<LocalizedProperty>()).AsQueryable());

            var storeMappings = new List<StoreMapping>
            {
                new StoreMapping { Id = 1, StoreId = 3, EntityName = "Product", EntityId = 99 }
            };

            _storeMappingRepository.Expect(x => x.Table).Return(storeMappings.AsQueryable());
            _storeMappingRepository.Expect(x => x.TableUntracked).Return(storeMappings.AsQueryable());

            var aclRecords = new List<AclRecord>
            {
                new AclRecord { Id = 1, CustomerRoleId = 3, EntityName = "Product", EntityId = 99 }
            };

            _aclRepository.Expect(x => x.Table).Return(aclRecords.AsQueryable());
            _aclRepository.Expect(x => x.TableUntracked).Return(aclRecords.AsQueryable());

            _productService
                .Expect(x => x.GetProductsByIds(Arg<int[]>.Is.Anything, Arg<ProductLoadFlags>.Is.Anything))
                .WhenCalled(x =>
                {
                    var ids = (int[])x.Arguments[0];
                    //string.Join(", ", ids).Dump();
                    var entities = products.Where(y => ids.Contains(y.Id)).ToList();

                    x.ReturnValue = (
                        from i in ids
                        join p in entities on i equals p.Id
                        select p).ToList();
                });
        }

        private CatalogSearchResult Search(CatalogSearchQuery query, IEnumerable<Product> products, IEnumerable<LocalizedProperty> localized = null)
        {
            Trace.WriteLine(query.ToString());

            InitMocks(query, products, localized);

            return _linqCatalogSearchService.Search(query);
        }

        [SetUp]
        public virtual void Setup()
        {
            _eventPublisher = MockRepository.GenerateMock<IEventPublisher>();
            _eventPublisher.Expect(x => x.Publish(Arg<object>.Is.Anything));

            _services = MockRepository.GenerateMock<ICommonServices>();
            _services.Expect(x => x.EventPublisher).Return(_eventPublisher);

            _productService = MockRepository.GenerateMock<IProductService>();
            _productRepository = MockRepository.GenerateMock<IRepository<Product>>();
            _localizedPropertyRepository = MockRepository.GenerateMock<IRepository<LocalizedProperty>>();
            _storeMappingRepository = MockRepository.GenerateMock<IRepository<StoreMapping>>();
            _aclRepository = MockRepository.GenerateMock<IRepository<AclRecord>>();
            _deliveryTimeService = MockRepository.GenerateMock<IDeliveryTimeService>();
            _manufacturerService = MockRepository.GenerateMock<IManufacturerService>();
            _categoryService = MockRepository.GenerateMock<ICategoryService>();

            _linqCatalogSearchService = new LinqCatalogSearchService(
                _services,
                _productService,
                _productRepository,
                _localizedPropertyRepository,
                _storeMappingRepository,
                _aclRepository,
                _deliveryTimeService,
                _manufacturerService,
                _categoryService);
        }

        [Test]
        public void LinqSearch_can_order_by_name()
        {
            var products = new List<Product>();

            for (var i = 97; i <= 110; ++i)
            {
                products.Add(new SearchProduct(i) { Name = Convert.ToChar(i).ToString(), ShortDescription = "smart" });
            }

            var query = new CatalogSearchQuery(new string[] { "shortdescription" }, "smart");
            query.SortBy(ProductSortingEnum.NameDesc);

            var result = Search(query, products);

            Assert.That(string.Join(",", result.Hits.Select(x => x.Name)), Is.EqualTo("n,m,l,k,j,i,h,g,f,e,d,c,b,a"));
        }

        [Test]
        public void LinqSearch_can_page_result()
        {
            var products = new List<Product>();

            for (var i = 1; i <= 20; ++i)
            {
                products.Add(new SearchProduct(i) { Name = "smart", Sku = i.ToString() });
            }

            var result = Search(new CatalogSearchQuery(new string[] { "name" }, "smart").Slice(10, 5), products);

            Assert.That(result.Hits.Count(), Is.EqualTo(5));
            Assert.That(result.Hits.Select(x => x.Sku), Is.EqualTo(new string[] { "11", "12", "13", "14", "15" }));
        }

        #region Term search

        [Test]
        public void LinqSearch_not_find_anything()
        {
            var products = new List<Product>
            {
                new SearchProduct { Name = "Smartstore" },
                new SearchProduct { Name = "Apple iPhone Smartphone 6" },
                new SearchProduct { Name = "Energistically recaptiualize superior e-markets without next-generation platforms" },
                new SearchProduct { Name = "Rapidiously conceptualize future-proof imperatives", ShortDescription = "Shopping System powered by Smartstore" }
            };

            var result = Search(new CatalogSearchQuery(new string[] { "name", "shortdescription" }, "cook"), products);

            Assert.That(result.Hits.Count, Is.EqualTo(0));
            Assert.That(result.SpellCheckerSuggestions.Any(), Is.EqualTo(false));
        }

        [Test]
        public void LinqSearch_find_term()
        {
            var products = new List<Product>
            {
                new SearchProduct(1) { Name = "Smartstore" },
                new SearchProduct(2) { Name = "Apple iPhone Smartphone 6" },
                new SearchProduct(3) { Name = "Energistically recaptiualize superior e-markets without next-generation platforms" },
                new SearchProduct(4) { Name = "Rapidiously conceptualize future-proof imperatives", ShortDescription = "Shopping System powered by Smartstore" }
            };

            var result = Search(new CatalogSearchQuery(new string[] { "name", "shortdescription" }, "Smart", SearchMode.Contains), products);

            Assert.That(result.Hits.Count, Is.EqualTo(3));
        }

        [Test]
        public void LinqSearch_find_exact_match()
        {
            var products = new List<Product>
            {
                new SearchProduct(1) { Name = "P-6000-2" },
                new SearchProduct(2) { Name = "Apple iPhone Smartphone 6" },
                new SearchProduct(3) { Name = "Energistically recaptiualize superior e-markets without next-generation platforms" },
                new SearchProduct(4) { Name = "Rapidiously conceptualize future-proof imperatives", ShortDescription = "Shopping System powered by SmartStore", Sku = "P-6000-2" }
            };

            var result = Search(new CatalogSearchQuery(new string[] { "name", "sku" }, "P-6000-2", SearchMode.ExactMatch), products);

            Assert.That(result.Hits.Count, Is.EqualTo(2));
        }

        #endregion

        #region Filter

        [Test]
        public void LinqSearch_filter_visible_only()
        {
            var products = new List<Product>
            {
                new SearchProduct(1) { Published = true },
                new SearchProduct(2) { Published = false },
                new SearchProduct(3) { Published = true, AvailableStartDateTimeUtc = new DateTime(2016, 1, 1), AvailableEndDateTimeUtc = new DateTime(2016, 1, 20) },
                new SearchProduct(4) { Published = true, Id = 99, SubjectToAcl = true }
            };

            var result = Search(new CatalogSearchQuery().VisibleOnly(new int[0]), products);
            Assert.That(result.Hits.Count, Is.EqualTo(2));

            result = Search(new CatalogSearchQuery().VisibleOnly(new int[] { 1, 5, 6 }), products);
            Assert.That(result.Hits.Count, Is.EqualTo(1));
        }

        [Test]
        public void LinqSearch_filter_published_only()
        {
            var products = new List<Product>
            {
                new SearchProduct(1) { Published = true },
                new SearchProduct(2) { Published = false },
                new SearchProduct(3) { Published = true }
            };

            var result = Search(new CatalogSearchQuery().PublishedOnly(true), products);

            Assert.That(result.Hits.Count, Is.EqualTo(2));
        }

        [Test]
        public void LinqSearch_filter_visibility()
        {
            var products = new List<Product>
            {
                new SearchProduct(1),
                new SearchProduct(2) { Visibility = ProductVisibility.Hidden },
                new SearchProduct(3),
                new SearchProduct(4) { Visibility = ProductVisibility.SearchResults }
            };

            var result = Search(new CatalogSearchQuery().WithVisibility(ProductVisibility.Full), products);
            Assert.That(result.Hits.Count, Is.EqualTo(2));

            result = Search(new CatalogSearchQuery().WithVisibility(ProductVisibility.SearchResults), products);
            Assert.That(result.Hits.Count, Is.EqualTo(3));

            result = Search(new CatalogSearchQuery().WithVisibility(ProductVisibility.Hidden), products);
            Assert.That(result.Hits.Count, Is.EqualTo(1));
        }

        [Test]
        public void LinqSearch_filter_homepage_products_only()
        {
            var products = new List<Product>
            {
                new SearchProduct(1),
                new SearchProduct(2) { ShowOnHomePage = true },
                new SearchProduct(3)
            };

            var result = Search(new CatalogSearchQuery().HomePageProductsOnly(true), products);

            Assert.That(result.Hits.Count, Is.EqualTo(1));
        }

        [Test]
        public void LinqSearch_filter_has_parent_grouped_product_id()
        {
            var products = new List<Product>
            {
                new SearchProduct(1),
                new SearchProduct(2) { ParentGroupedProductId = 16, Visibility = ProductVisibility.Hidden },
                new SearchProduct(3) { ParentGroupedProductId = 36, Visibility = ProductVisibility.Hidden },
                new SearchProduct(4) { ParentGroupedProductId = 9 },
                new SearchProduct(5) { ParentGroupedProductId = 36 }
            };

            var result = Search(new CatalogSearchQuery().HasParentGroupedProduct(36), products);
            Assert.That(result.Hits.Count, Is.EqualTo(2));
        }

        [Test]
        public void LinqSearch_filter_has_store_id()
        {
            var products = new List<Product>
            {
                new SearchProduct { },
                new SearchProduct { LimitedToStores = true, Id = 99 }
            };

            var result = Search(new CatalogSearchQuery().HasStoreId(1), products);
            Assert.That(result.Hits.Count, Is.EqualTo(1));

            result = Search(new CatalogSearchQuery().HasStoreId(3), products);
            Assert.That(result.Hits.Count, Is.EqualTo(2));
        }

        [Test]
        public void LinqSearch_filter_is_product_type()
        {
            var products = new List<Product>
            {
                new SearchProduct(1),
                new SearchProduct(2) { ProductType = ProductType.BundledProduct },
                new SearchProduct(3) { ProductType = ProductType.GroupedProduct }
            };

            var result = Search(new CatalogSearchQuery().IsProductType(ProductType.SimpleProduct), products);
            Assert.That(result.Hits.Count, Is.EqualTo(1));

            result = Search(new CatalogSearchQuery().IsProductType(ProductType.GroupedProduct), products);
            Assert.That(result.Hits.Count, Is.EqualTo(1));
        }

        [Test]
        public void LinqSearch_filter_with_product_ids()
        {
            var products = new List<Product>
            {
                new SearchProduct(1),
                new SearchProduct(2),
                new SearchProduct(3),
                new SearchProduct(4),
                new SearchProduct(5)
            };

            var result = Search(new CatalogSearchQuery().WithProductIds(2, 3, 4, 99), products);
            Assert.That(result.Hits.Count, Is.EqualTo(3));

            result = Search(new CatalogSearchQuery().WithProductIds(98), products);
            Assert.IsNull(result.Hits.FirstOrDefault());
        }

        [Test]
        public void LinqSearch_filter_with_product_id()
        {
            var products = new List<Product>
            {
                new SearchProduct(1),
                new SearchProduct(2),
                new SearchProduct(3),
                new SearchProduct(4),
                new SearchProduct(5),
                new SearchProduct(6),
                new SearchProduct(7),
                new SearchProduct(8),
                new SearchProduct(9),
                new SearchProduct(10)
            };

            var result = Search(new CatalogSearchQuery().WithProductId(4, 7), products);
            Assert.That(result.Hits.Count(), Is.EqualTo(4));

            result = Search(new CatalogSearchQuery().WithProductId(6, null), products);
            Assert.That(result.Hits.Count(), Is.EqualTo(5));

            result = Search(new CatalogSearchQuery().WithProductId(null, 3), products);
            Assert.That(result.Hits.Count(), Is.EqualTo(3));
        }

        [Test]
        public void LinqSearch_filter_with_category_ids()
        {
            var products = new List<Product>
            {
                new SearchProduct(new ProductCategory[] { new ProductCategory { CategoryId = 11 } }) { Id = 1 },
                new SearchProduct(new ProductCategory[] { new ProductCategory { CategoryId = 12, IsFeaturedProduct = true } }) { Id = 2 },
                new SearchProduct(new ProductCategory[] { new ProductCategory { CategoryId = 13 } }) { Id = 3 },
                new SearchProduct(new ProductCategory[] { new ProductCategory { CategoryId = 14 } }) { Id = 4 },
                new SearchProduct(new ProductCategory[] { new ProductCategory { CategoryId = 15 } }) { Id = 5 },
                new SearchProduct(new ProductCategory[] { new ProductCategory { CategoryId = 16, IsFeaturedProduct = true } }) { Id = 6 },
                new SearchProduct(new ProductCategory[] { new ProductCategory { CategoryId = 17 } }) { Id = 7 },
                new SearchProduct(new ProductCategory[] { new ProductCategory { CategoryId = 18 } }) { Id = 8 }
            };

            var result = Search(new CatalogSearchQuery().WithCategoryIds(null, 68, 98), products);
            Assert.That(result.Hits.Count(), Is.EqualTo(0));

            result = Search(new CatalogSearchQuery().WithCategoryIds(null, 12, 15, 18, 24), products);
            Assert.That(result.Hits.Count(), Is.EqualTo(3));

            result = Search(new CatalogSearchQuery().WithCategoryIds(true, 12, 15, 18, 24), products);
            Assert.That(result.Hits.Count(), Is.EqualTo(1));

            result = Search(new CatalogSearchQuery().WithCategoryIds(false, 12, 15, 18, 24), products);
            Assert.That(result.Hits.Count(), Is.EqualTo(2));
        }

        [Test]
        public void LinqSearch_filter_has_any_category()
        {
            var products = new List<Product>
            {
                new SearchProduct(new ProductCategory[] { new ProductCategory { CategoryId = 11 } }) { Id = 1 },
                new SearchProduct(new ProductCategory[] { new ProductCategory { CategoryId = 12, IsFeaturedProduct = true } }) { Id = 2 },
                new SearchProduct(new ProductCategory[] { }) { Id = 3 },
                new SearchProduct(new ProductCategory[] { new ProductCategory { CategoryId = 14 } }) { Id = 4 },
                new SearchProduct(new ProductCategory[] { new ProductCategory { CategoryId = 15 } }) { Id = 5 },
                new SearchProduct(new ProductCategory[] { }) { Id = 6 },
                new SearchProduct(new ProductCategory[] { }) { Id = 7 },
                new SearchProduct(new ProductCategory[] { new ProductCategory { CategoryId = 18 } }) { Id = 8 }
            };

            var result = Search(new CatalogSearchQuery().HasAnyCategory(true), products);
            Assert.That(result.Hits.Count(), Is.EqualTo(5));

            result = Search(new CatalogSearchQuery().HasAnyCategory(false), products);
            Assert.That(result.Hits.Count(), Is.EqualTo(3));
        }

        [Test]
        public void LinqSearch_filter_with_manufacturer_ids()
        {
            var products = new List<Product>
            {
                new SearchProduct(new ProductManufacturer[] { new ProductManufacturer { ManufacturerId = 11 } }) { Id = 1 },
                new SearchProduct(new ProductManufacturer[] { new ProductManufacturer { ManufacturerId = 12, IsFeaturedProduct = true } }) { Id = 2 },
                new SearchProduct(new ProductManufacturer[] { new ProductManufacturer { ManufacturerId = 13 } }) { Id = 3 },
                new SearchProduct(new ProductManufacturer[] { new ProductManufacturer { ManufacturerId = 14 } }) { Id = 4 },
                new SearchProduct(new ProductManufacturer[] { new ProductManufacturer { ManufacturerId = 15 } }) { Id = 5 },
                new SearchProduct(new ProductManufacturer[] { new ProductManufacturer { ManufacturerId = 16, IsFeaturedProduct = true } }) { Id = 6 },
                new SearchProduct(new ProductManufacturer[] { new ProductManufacturer { ManufacturerId = 17 } }) { Id = 7 },
                new SearchProduct(new ProductManufacturer[] { new ProductManufacturer { ManufacturerId = 18 } }) { Id = 8 }
            };

            var result = Search(new CatalogSearchQuery().WithManufacturerIds(null, 68, 98), products);
            Assert.That(result.Hits.Count(), Is.EqualTo(0));

            result = Search(new CatalogSearchQuery().WithManufacturerIds(null, 12, 15, 18, 24), products);
            Assert.That(result.Hits.Count(), Is.EqualTo(3));

            result = Search(new CatalogSearchQuery().WithManufacturerIds(true, 12, 15, 18, 24), products);
            Assert.That(result.Hits.Count(), Is.EqualTo(1));

            result = Search(new CatalogSearchQuery().WithManufacturerIds(false, 12, 15, 18, 24), products);
            Assert.That(result.Hits.Count(), Is.EqualTo(2));
        }

        [Test]
        public void LinqSearch_filter_has_any_manufacturer()
        {
            var products = new List<Product>
            {
                new SearchProduct(new ProductManufacturer[] { new ProductManufacturer { ManufacturerId = 11 } }) { Id = 1 },
                new SearchProduct(new ProductManufacturer[] { new ProductManufacturer { ManufacturerId = 12, IsFeaturedProduct = true } }) { Id = 2 },
                new SearchProduct(new ProductManufacturer[] { }) { Id = 3 },
                new SearchProduct(new ProductManufacturer[] { new ProductManufacturer { ManufacturerId = 14 } }) { Id = 4 },
                new SearchProduct(new ProductManufacturer[] { new ProductManufacturer { ManufacturerId = 15 } }) { Id = 5 },
                new SearchProduct(new ProductManufacturer[] { }) { Id = 6 },
                new SearchProduct(new ProductManufacturer[] { }) { Id = 7 },
                new SearchProduct(new ProductManufacturer[] { new ProductManufacturer { ManufacturerId = 18 } }) { Id = 8 }
            };

            var result = Search(new CatalogSearchQuery().HasAnyManufacturer(true), products);
            Assert.That(result.Hits.Count(), Is.EqualTo(5));

            result = Search(new CatalogSearchQuery().HasAnyManufacturer(false), products);
            Assert.That(result.Hits.Count(), Is.EqualTo(3));
        }

        [Test]
        public void LinqSearch_filter_with_product_tag_ids()
        {
            var products = new List<Product>
            {
                new SearchProduct(new ProductTag[] { new ProductTag { Id = 16 } }) { Id = 1 },
                new SearchProduct(new ProductTag[] { }) { Id = 2 },
                new SearchProduct(new ProductTag[] { new ProductTag { Id = 32 } }) { Id = 3 },
                new SearchProduct(new ProductTag[] { new ProductTag { Id = 16 } }) { Id = 4 },
                new SearchProduct(new ProductTag[] { }) { Id = 5 }
            };

            var result = Search(new CatalogSearchQuery().WithProductTagIds(16, 32), products);
            Assert.That(result.Hits.Count(), Is.EqualTo(3));

            result = Search(new CatalogSearchQuery().WithProductTagIds(22), products);
            Assert.IsNull(result.Hits.FirstOrDefault());
        }

        [Test]
        public void LinqSearch_filter_with_stock_quantity()
        {
            var products = new List<Product>
            {
                new SearchProduct(1) { StockQuantity = 10000 },
                new SearchProduct(2) { StockQuantity = 10001 },
                new SearchProduct(3) { StockQuantity = 10002 },
                new SearchProduct(4) { StockQuantity = 10003 },
                new SearchProduct(5) { StockQuantity = 10004 },
                new SearchProduct(6) { StockQuantity = 0 },
                new SearchProduct(7) { StockQuantity = 650 },
                new SearchProduct(8) { StockQuantity = 0 }
            };

            var result = Search(new CatalogSearchQuery().WithStockQuantity(10001, 10003), products);
            Assert.That(result.Hits.Count(), Is.EqualTo(3));

            result = Search(new CatalogSearchQuery().WithStockQuantity(10003, null), products);
            Assert.That(result.Hits.Count(), Is.EqualTo(2));

            result = Search(new CatalogSearchQuery().WithStockQuantity(10003, null, false), products);
            Assert.That(result.Hits.Count(), Is.EqualTo(1));

            result = Search(new CatalogSearchQuery().WithStockQuantity(null, 10002), products);
            Assert.That(result.Hits.Count(), Is.EqualTo(6));

            result = Search(new CatalogSearchQuery().WithStockQuantity(null, 10002, null, false), products);
            Assert.That(result.Hits.Count(), Is.EqualTo(5));


            result = Search(new CatalogSearchQuery().WithStockQuantity(10000, 10000), products);
            Assert.That(result.Hits.Count(), Is.EqualTo(1));

            result = Search(new CatalogSearchQuery().WithStockQuantity(20000, 20000), products);
            Assert.That(result.Hits.Count(), Is.EqualTo(0));

            result = Search(new CatalogSearchQuery().WithStockQuantity(0, 0, false, false), products);
            Assert.That(result.Hits.Count(), Is.EqualTo(6));
        }

        [Test]
        public void LinqSearch_filter_with_price()
        {
            var products = new List<Product>
            {
                new SearchProduct(1) { Price = 102.0M },
                new SearchProduct(2) { Price = 22.5M },
                new SearchProduct(3) { Price = 658.99M },
                new SearchProduct(4) { Price = 25.3M },
                new SearchProduct(5) { Price = 14.9M }
            };

            var eur = new Currency { CurrencyCode = "EUR" };

            var result = Search(new CatalogSearchQuery().WithCurrency(eur).PriceBetween(100M, 200M), products);
            Assert.That(result.Hits.Count(), Is.EqualTo(1));

            result = Search(new CatalogSearchQuery().WithCurrency(eur).PriceBetween(100M, null), products);
            Assert.That(result.Hits.Count(), Is.EqualTo(2));

            result = Search(new CatalogSearchQuery().WithCurrency(eur).PriceBetween(null, 100M), products);
            Assert.That(result.Hits.Count(), Is.EqualTo(3));

            result = Search(new CatalogSearchQuery().WithCurrency(eur).PriceBetween(14.90M, 14.90M), products);
            Assert.That(result.Hits.Count(), Is.EqualTo(1));

            result = Search(new CatalogSearchQuery().WithCurrency(eur).PriceBetween(59.90M, 59.90M), products);
            Assert.That(result.Hits.Count(), Is.EqualTo(0));

            result = Search(new CatalogSearchQuery().WithCurrency(eur).PriceBetween(14.90M, 14.90M, false, false), products);
            Assert.That(result.Hits.Count(), Is.EqualTo(4));
        }

        [Test]
        public void LinqSearch_filter_with_created_utc()
        {
            var products = new List<Product>
            {
                new SearchProduct(1) { CreatedOnUtc = new DateTime(2016, 2, 16) },
                new SearchProduct(2) { CreatedOnUtc = new DateTime(2016, 2, 23) },
                new SearchProduct(3) { CreatedOnUtc = new DateTime(2016, 3, 20) },
                new SearchProduct(4) { CreatedOnUtc = new DateTime(2016, 4, 5) },
                new SearchProduct(5) { CreatedOnUtc = new DateTime(2016, 6, 25) },
                new SearchProduct(6) { CreatedOnUtc = new DateTime(2016, 8, 4) }
            };

            var result = Search(new CatalogSearchQuery().CreatedBetween(new DateTime(2016, 1, 1), new DateTime(2016, 3, 1)), products);
            Assert.That(result.Hits.Count(), Is.EqualTo(2));

            result = Search(new CatalogSearchQuery().CreatedBetween(new DateTime(2016, 4, 1), null), products);
            Assert.That(result.Hits.Count(), Is.EqualTo(3));

            result = Search(new CatalogSearchQuery().CreatedBetween(null, new DateTime(2016, 7, 1)), products);
            Assert.That(result.Hits.Count(), Is.EqualTo(5));

            result = Search(new CatalogSearchQuery().CreatedBetween(new DateTime(2016, 8, 4), new DateTime(2016, 8, 4)), products);
            Assert.That(result.Hits.Count(), Is.EqualTo(1));

            result = Search(new CatalogSearchQuery().CreatedBetween(new DateTime(2012, 8, 4), new DateTime(2012, 8, 4)), products);
            Assert.That(result.Hits.Count(), Is.EqualTo(0));

            result = Search(new CatalogSearchQuery().CreatedBetween(new DateTime(2016, 8, 4), new DateTime(2016, 8, 4), false, false), products);
            Assert.That(result.Hits.Count(), Is.EqualTo(5));
        }

        [Test]
        public void LinqSearch_filter_available_only()
        {
            var products = new List<Product>
            {
                new SearchProduct(1),
                new SearchProduct(2)
                {
                    StockQuantity = 0,
                    ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                    BackorderMode = BackorderMode.NoBackorders
                },
                new SearchProduct(3)
                {
                    StockQuantity = 0,
                    ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                    BackorderMode = BackorderMode.AllowQtyBelow0AndNotifyCustomer
                }
            };

            var result = Search(new CatalogSearchQuery().AvailableOnly(true), products);
            Assert.That(result.Hits.Count, Is.EqualTo(2));
        }

        [Test]
        public void LinqSearch_filter_with_rating()
        {
            var products = new List<Product>
            {
                new SearchProduct(1),
                new SearchProduct(2) { ApprovedRatingSum = 14, ApprovedTotalReviews = 3 },  // 4.66
                new SearchProduct(3) { ApprovedRatingSum = 9, ApprovedTotalReviews = 3 },   // 3.00
                new SearchProduct(4) { ApprovedRatingSum = 17, ApprovedTotalReviews = 4 },  // 4.25
                new SearchProduct(5) { ApprovedRatingSum = 20, ApprovedTotalReviews = 10 }  // 2.00
            };

            var result = Search(new CatalogSearchQuery().WithRating(3.0, null), products);
            Assert.That(result.Hits.Count, Is.EqualTo(3));

            result = Search(new CatalogSearchQuery().WithRating(4.0, 5.0), products);
            Assert.That(result.Hits.Count, Is.EqualTo(2));

            result = Search(new CatalogSearchQuery().WithRating(3.0, 3.0), products);
            Assert.That(result.Hits.Count, Is.EqualTo(1));

            result = Search(new CatalogSearchQuery().WithRating(4.0, 4.0), products);
            Assert.That(result.Hits.Count, Is.EqualTo(0));

            result = Search(new CatalogSearchQuery().WithRating(2.0, 2.0, false, false), products);
            Assert.That(result.Hits.Count, Is.EqualTo(3));
        }

        [Test]
        public void LinqSearch_filter_with_deliverytime_ids()
        {
            var products = new List<Product>
            {
                new SearchProduct(1),
                new SearchProduct(2) { DeliveryTimeId = 16 },
                new SearchProduct(3) { DeliveryTimeId = 16 },
                new SearchProduct(4) { DeliveryTimeId = 9 }
            };

            var result = Search(new CatalogSearchQuery().WithDeliveryTimeIds(new int[] { 16, 9 }), products);
            Assert.That(result.Hits.Count, Is.EqualTo(3));

            result = Search(new CatalogSearchQuery().WithDeliveryTimeIds(new int[] { 9 }), products);
            Assert.That(result.Hits.Count, Is.EqualTo(1));
        }

        [Test]
        public void LinqSearch_filter_with_condition()
        {
            var products = new List<Product>
            {
                new SearchProduct(1) { Condition = ProductCondition.New },
                new SearchProduct(2) { Condition = ProductCondition.Used },
                new SearchProduct(3) { Condition = ProductCondition.New },
                new SearchProduct(4) { Condition = ProductCondition.Damaged },
                new SearchProduct(5) { Condition = ProductCondition.New },
                new SearchProduct(6) { Condition = ProductCondition.Refurbished }
            };

            var result = Search(new CatalogSearchQuery().WithCondition(ProductCondition.New), products);
            Assert.That(result.Hits.Count, Is.EqualTo(3));

            result = Search(new CatalogSearchQuery().WithCondition(ProductCondition.Used, ProductCondition.Damaged), products);
            Assert.That(result.Hits.Count, Is.EqualTo(2));

            result = Search(new CatalogSearchQuery().WithCondition(ProductCondition.Refurbished), products);
            Assert.That(result.Hits.Count, Is.EqualTo(1));

            result = Search(new CatalogSearchQuery().WithCondition(ProductCondition.New, ProductCondition.Used), products);
            Assert.That(result.Hits.Count, Is.EqualTo(4));
        }

        #endregion

        #region SearchProduct

        internal class SearchProduct : Product
        {
            internal SearchProduct()
                : this(0, null, null, null)
            {
            }

            internal SearchProduct(int id)
                : this(id, null, null, null)
            {
            }

            internal SearchProduct(ICollection<ProductCategory> categories)
                : this(0, categories, null, null)
            {
            }

            internal SearchProduct(ICollection<ProductManufacturer> manufacturers)
                : this(0, null, manufacturers, null)
            {
            }

            internal SearchProduct(ICollection<ProductTag> tags)
                : this(0, null, null, tags)
            {
            }

            internal SearchProduct(
                int id,
                ICollection<ProductCategory> categories,
                ICollection<ProductManufacturer> manufacturers,
                ICollection<ProductTag> tags)
            {
                Id = id == 0 ? (new Random()).Next(100, int.MaxValue) : id;
                ProductCategories = categories ?? new HashSet<ProductCategory>();
                ProductManufacturers = manufacturers ?? new HashSet<ProductManufacturer>();
                ProductTags = tags ?? new HashSet<ProductTag>();

                Name = "Holisticly implement optimal web services";
                ShortDescription = "Continually synthesize fully researched benefits with granular benefits.";
                FullDescription = "Enthusiastically utilize compelling systems with vertical collaboration and idea-sharing. Interactively incubate bleeding-edge innovation with future-proof catalysts for change. Distinctively exploit parallel paradigms rather than progressive scenarios. Compellingly synergize visionary ROI after process-centric resources. Objectively negotiate performance based best practices with 24/7 vortals. Globally pontificate reliable processes for innovative services. Monotonectally enable mission - critical information and quality.";
                Sku = "X-" + id.ToString();
                Published = true;
                Visibility = ProductVisibility.Full;
                ProductTypeId = (int)ProductType.SimpleProduct;
                StockQuantity = 10000;
                CreatedOnUtc = new DateTime(2016, 8, 24);
            }

            public override ICollection<ProductCategory> ProductCategories { get; protected set; }
            public override ICollection<ProductManufacturer> ProductManufacturers { get; protected set; }
            public override ICollection<ProductTag> ProductTags { get; protected set; }
        }

        #endregion
    }
}
