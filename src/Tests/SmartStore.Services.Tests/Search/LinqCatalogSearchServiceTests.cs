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
using SmartStore.Services.Search;

namespace SmartStore.Services.Tests.Search
{
	[TestFixture]
	public class LinqCatalogSearchServiceTests
	{
		private IProductService _productService;
		private IRepository<Product> _productRepository;
		private IRepository<LocalizedProperty> _localizedPropertyRepository;
		private IRepository<StoreMapping> _storeMappingRepository;
		private IRepository<AclRecord> _aclRepository;
		private IEventPublisher _eventPublisher;
		private LinqCatalogSearchService _linqCatalogSearchService;

		private void InitMocks(List<Product> products)
		{
			InitMocks(products, new List<LocalizedProperty>());
		}

		private void InitMocks(List<Product> products, List<LocalizedProperty> localized)
		{
			_productRepository.Expect(x => x.Table).Return(products.AsQueryable());
			_productRepository.Expect(x => x.TableUntracked).Return(products.AsQueryable());

			_localizedPropertyRepository.Expect(x => x.Table).Return(localized.AsQueryable());
			_localizedPropertyRepository.Expect(x => x.TableUntracked).Return(localized.AsQueryable());

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
		}

		private CatalogSearchResult Search(CatalogSearchQuery searchQuery)
		{
			Trace.WriteLine(searchQuery.ToString());

			return _linqCatalogSearchService.Search(searchQuery);
		}

		[SetUp]
		public virtual void Setup()
		{
			_productService = MockRepository.GenerateMock<IProductService>();
			_productRepository = MockRepository.GenerateMock<IRepository<Product>>();
			_localizedPropertyRepository = MockRepository.GenerateMock<IRepository<LocalizedProperty>>();
			_storeMappingRepository = MockRepository.GenerateMock<IRepository<StoreMapping>>();
			_aclRepository = MockRepository.GenerateMock<IRepository<AclRecord>>();
			_eventPublisher = MockRepository.GenerateMock<IEventPublisher>();

			_linqCatalogSearchService = new LinqCatalogSearchService(
				_productService, 
				_productRepository, 
				_localizedPropertyRepository, 
				_storeMappingRepository, 
				_aclRepository, 
				_eventPublisher);
		}

		[Test]
		public void LinqSearch_can_order_by_name()
		{
			var products = new List<Product>();

			for (var i = 97; i <= 110; ++i)
			{
				products.Add(new SearchProduct { Name = Convert.ToChar(i).ToString(), ShortDescription = "smart" });
			}

			InitMocks(products);

			var query = new CatalogSearchQuery(new string[] { "shortdescription" }, "smart");
			query.SortBy(ProductSortingEnum.NameDesc);

			var result = Search(query);

			Assert.That(string.Join(",", result.Hits.Select(x => x.Name)), Is.EqualTo("n,m,l,k,j,i,h,g,f,e,d,c,b,a"));
		}

		[Test]
		public void LinqSearch_can_page_result()
		{
			var products = new List<Product>();

			for (var i = 1; i <= 20; ++i)
			{
				products.Add(new SearchProduct { Name = "smart", Sku = i.ToString() });
			}

			InitMocks(products);

			var query = new CatalogSearchQuery(new string[] { "name" }, "smart");
			query.Slice(10, 5);

			var result = Search(query);

			Assert.That(result.Hits.Count(), Is.EqualTo(5));
			Assert.That(result.Hits.Select(x => x.Sku), Is.EqualTo(new string[] { "11", "12", "13", "14", "15" }));
		}

		#region Term search

		[Test]
		public void LinqSearch_not_find_anything()
		{
			var products = new List<Product>
			{
				new SearchProduct { Name = "SmartStore.NET" },
				new SearchProduct { Name = "Apple iPhone Smartphone 6" },
				new SearchProduct { Name = "Energistically recaptiualize superior e-markets without next-generation platforms" },
				new SearchProduct { Name = "Rapidiously conceptualize future-proof imperatives", ShortDescription = "Shopping System powered by SmartStore" }
			};

			InitMocks(products);

			var result = Search(new CatalogSearchQuery(new string[] { "name", "shortdescription" }, "cook"));

			Assert.That(result.Hits.Count, Is.EqualTo(0));
			Assert.That(result.SpellCheckerSuggestions.Any(), Is.EqualTo(false));
		}

		[Test]
		public void LinqSearch_find_term()
		{
			var products = new List<Product>
			{
				new SearchProduct { Name = "SmartStore.NET" },
				new SearchProduct { Name = "Apple iPhone Smartphone 6" },
				new SearchProduct { Name = "Energistically recaptiualize superior e-markets without next-generation platforms" },
				new SearchProduct { Name = "Rapidiously conceptualize future-proof imperatives", ShortDescription = "Shopping System powered by SmartStore" }
			};

			InitMocks(products);

			var result = Search(new CatalogSearchQuery(new string[] { "name", "shortdescription" }, "Smart"));

			Assert.That(result.Hits.Count, Is.EqualTo(3));
		}

		[Test]
		public void LinqSearch_find_exact_match()
		{
			var products = new List<Product>
			{
				new SearchProduct { Name = "P-6000-2" },
				new SearchProduct { Name = "Apple iPhone Smartphone 6" },
				new SearchProduct { Name = "Energistically recaptiualize superior e-markets without next-generation platforms" },
				new SearchProduct { Name = "Rapidiously conceptualize future-proof imperatives", ShortDescription = "Shopping System powered by SmartStore", Sku = "P-6000-2" }
			};

			InitMocks(products);

			var result = Search(new CatalogSearchQuery(new string[] { "name", "sku" }, "P-6000-2", SearchMode.ExactMatch));

			Assert.That(result.Hits.Count, Is.EqualTo(2));
		}

		#endregion

		#region Spell Checking

		[Test]
		public void LinqSearch_can_spellchecking()
		{
			var products = new List<Product>
			{
				new SearchProduct { Name = "SmartStore.NET" },
				new SearchProduct { Name = "Apple iPhone Smartphone 6" },
				new SearchProduct { Name = "Energistically recaptiualize superior e-markets without next-generation platforms" },
				new SearchProduct { Name = "Rapidiously SmartPhone conceptualize future-proof imperatives" },
				new SearchProduct { Name = "Apple iPhone Smartphone 5", LimitedToStores = true, Id = 99 },
			};

			InitMocks(products);

			var result = Search(new CatalogSearchQuery(new string[] { "name" }, "Smart").CheckSpelling(10).Slice(0, 0).HasStoreId(1));

			Assert.That(result.SpellCheckerSuggestions.Length, Is.EqualTo(2));
			Assert.That(result.SpellCheckerSuggestions[0].IsCaseInsensitiveEqual("Smartphone"));
		}

		#endregion

		#region Filter

		[Test]
		public void LinqSearch_filter_visible_only()
		{
			var products = new List<Product>
			{
				new SearchProduct { Published = true },
				new SearchProduct { Published = false },
				new SearchProduct { Published = true, AvailableStartDateTimeUtc = new DateTime(2016, 1, 1), AvailableEndDateTimeUtc = new DateTime(2016, 1, 20) },
				new SearchProduct { Published = true, Id = 99, SubjectToAcl = true }
			};

			InitMocks(products);

			Assert.That(Search(new CatalogSearchQuery().VisibleOnly(new int[0])).Hits.Count, Is.EqualTo(2));
			Assert.That(Search(new CatalogSearchQuery().VisibleOnly(new int[] { 1, 5, 6 })).Hits.Count, Is.EqualTo(1));
		}

		[Test]
		public void LinqSearch_filter_published_only()
		{
			var products = new List<Product>
			{
				new SearchProduct { Published = true },
				new SearchProduct { Published = false },
				new SearchProduct { Published = true }
			};

			InitMocks(products);

			var result = Search(new CatalogSearchQuery().PublishedOnly(true));

			Assert.That(result.Hits.Count, Is.EqualTo(2));
		}

		[Test]
		public void LinqSearch_filter_visible_individually_only()
		{
			var products = new List<Product>
			{
				new SearchProduct { },
				new SearchProduct { VisibleIndividually = false },
				new SearchProduct { }
			};

			InitMocks(products);

			var result = Search(new CatalogSearchQuery().VisibleIndividuallyOnly(true));

			Assert.That(result.Hits.Count, Is.EqualTo(2));
		}

		[Test]
		public void LinqSearch_filter_homepage_products_only()
		{
			var products = new List<Product>
			{
				new SearchProduct { },
				new SearchProduct { ShowOnHomePage = true },
				new SearchProduct { }
			};

			InitMocks(products);

			var result = Search(new CatalogSearchQuery().HomePageProductsOnly(true));

			Assert.That(result.Hits.Count, Is.EqualTo(1));
		}

		[Test]
		public void LinqSearch_filter_has_parent_grouped_product_id()
		{
			var products = new List<Product>
			{
				new SearchProduct { },
				new SearchProduct { ParentGroupedProductId = 16, VisibleIndividually = false },
				new SearchProduct { ParentGroupedProductId = 36, VisibleIndividually = false },
				new SearchProduct { ParentGroupedProductId = 9 },
				new SearchProduct { ParentGroupedProductId = 36 }
			};

			InitMocks(products);

			var result = Search(new CatalogSearchQuery().HasParentGroupedProductId(36));

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

			InitMocks(products);

			Assert.That(Search(new CatalogSearchQuery().HasStoreId(1)).Hits.Count, Is.EqualTo(1));
			Assert.That(Search(new CatalogSearchQuery().HasStoreId(3)).Hits.Count, Is.EqualTo(2));
		}

		[Test]
		public void LinqSearch_filter_is_product_type()
		{
			var products = new List<Product>
			{
				new SearchProduct { },
				new SearchProduct { ProductType = ProductType.BundledProduct },
				new SearchProduct { ProductType = ProductType.GroupedProduct }
			};

			InitMocks(products);

			Assert.That(Search(new CatalogSearchQuery().IsProductType(ProductType.SimpleProduct)).Hits.Count, Is.EqualTo(1));
			Assert.That(Search(new CatalogSearchQuery().IsProductType(ProductType.GroupedProduct)).Hits.Count, Is.EqualTo(1));
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

			InitMocks(products);

			Assert.That(Search(new CatalogSearchQuery().WithProductIds(2, 3, 4, 99)).Hits.Count, Is.EqualTo(3));
			Assert.IsNull(Search(new CatalogSearchQuery().WithProductIds(98)).Hits.FirstOrDefault());
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

			InitMocks(products);

			Assert.That(Search(new CatalogSearchQuery().WithProductId(4, 7)).Hits.Count(), Is.EqualTo(4));
			Assert.That(Search(new CatalogSearchQuery().WithProductId(6, null)).Hits.Count(), Is.EqualTo(5));
			Assert.That(Search(new CatalogSearchQuery().WithProductId(null, 3)).Hits.Count(), Is.EqualTo(3));
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

			InitMocks(products);

			Assert.That(Search(new CatalogSearchQuery().WithCategoryIds(null, 68, 98)).Hits.Count(), Is.EqualTo(0));
			Assert.That(Search(new CatalogSearchQuery().WithCategoryIds(null, 12, 15, 18, 24)).Hits.Count(), Is.EqualTo(3));
			Assert.That(Search(new CatalogSearchQuery().WithCategoryIds(true, 12, 15, 18, 24)).Hits.Count(), Is.EqualTo(1));
			Assert.That(Search(new CatalogSearchQuery().WithCategoryIds(false, 12, 15, 18, 24)).Hits.Count(), Is.EqualTo(2));
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

			InitMocks(products);

			Assert.That(Search(new CatalogSearchQuery().HasAnyCategory(true)).Hits.Count(), Is.EqualTo(5));
			Assert.That(Search(new CatalogSearchQuery().HasAnyCategory(false)).Hits.Count(), Is.EqualTo(3));
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

			InitMocks(products);

			Assert.That(Search(new CatalogSearchQuery().WithManufacturerIds(null, 68, 98)).Hits.Count(), Is.EqualTo(0));
			Assert.That(Search(new CatalogSearchQuery().WithManufacturerIds(null, 12, 15, 18, 24)).Hits.Count(), Is.EqualTo(3));
			Assert.That(Search(new CatalogSearchQuery().WithManufacturerIds(true, 12, 15, 18, 24)).Hits.Count(), Is.EqualTo(1));
			Assert.That(Search(new CatalogSearchQuery().WithManufacturerIds(false, 12, 15, 18, 24)).Hits.Count(), Is.EqualTo(2));
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

			InitMocks(products);

			Assert.That(Search(new CatalogSearchQuery().HasAnyManufacturer(true)).Hits.Count(), Is.EqualTo(5));
			Assert.That(Search(new CatalogSearchQuery().HasAnyManufacturer(false)).Hits.Count(), Is.EqualTo(3));
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

			InitMocks(products);

			Assert.That(Search(new CatalogSearchQuery().WithProductTagIds(16, 32)).Hits.Count(), Is.EqualTo(3));
			Assert.IsNull(Search(new CatalogSearchQuery().WithProductTagIds(22)).Hits.FirstOrDefault());
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

			InitMocks(products);

			Assert.That(Search(new CatalogSearchQuery().WithStockQuantity(10001, 10003)).Hits.Count(), Is.EqualTo(3));
			Assert.That(Search(new CatalogSearchQuery().WithStockQuantity(10003, null)).Hits.Count(), Is.EqualTo(2));
			Assert.That(Search(new CatalogSearchQuery().WithStockQuantity(null, 10002)).Hits.Count(), Is.EqualTo(6));
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

			InitMocks(products);

			var eur = new Currency { CurrencyCode = "EUR" };

			Assert.That(Search(new CatalogSearchQuery().PriceBetween(100M, 200M, eur)).Hits.Count(), Is.EqualTo(1));
			Assert.That(Search(new CatalogSearchQuery().PriceBetween(100M, null, eur)).Hits.Count(), Is.EqualTo(2));
			Assert.That(Search(new CatalogSearchQuery().PriceBetween(null, 100M, eur)).Hits.Count(), Is.EqualTo(3));
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

			InitMocks(products);

			Assert.That(Search(new CatalogSearchQuery().CreatedBetween(new DateTime(2016, 1, 1), new DateTime(2016, 3, 1))).Hits.Count(), Is.EqualTo(2));
			Assert.That(Search(new CatalogSearchQuery().CreatedBetween(new DateTime(2016, 4, 1), null)).Hits.Count(), Is.EqualTo(3));
			Assert.That(Search(new CatalogSearchQuery().CreatedBetween(null, new DateTime(2016, 7, 1))).Hits.Count(), Is.EqualTo(5));
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
				Id = (id == 0 ? (new Random()).Next(100, int.MaxValue) : id);
				ProductCategories = categories ?? new HashSet<ProductCategory>();
				ProductManufacturers = manufacturers ?? new HashSet<ProductManufacturer>();
				ProductTags = tags ?? new HashSet<ProductTag>();

				Name = "Holisticly implement optimal web services";
				ShortDescription = "Continually synthesize fully researched benefits with granular benefits.";
				FullDescription = "Enthusiastically utilize compelling systems with vertical collaboration and idea-sharing. Interactively incubate bleeding-edge innovation with future-proof catalysts for change. Distinctively exploit parallel paradigms rather than progressive scenarios. Compellingly synergize visionary ROI after process-centric resources. Objectively negotiate performance based best practices with 24/7 vortals. Globally pontificate reliable processes for innovative services. Monotonectally enable mission - critical information and quality.";
				Sku = "X-" + id.ToString();
				Published = true;
				VisibleIndividually = true;
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
