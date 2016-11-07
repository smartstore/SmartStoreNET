using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Security;
using SmartStore.Core.Domain.Stores;
using SmartStore.Services.Search;

namespace SmartStore.Services.Tests.Search
{
	[TestFixture]
	public class LinqCatalogSearchServiceTests
	{
		private IRepository<Product> _productRepository;
		private IRepository<LocalizedProperty> _localizedPropertyRepository;
		private IRepository<StoreMapping> _storeMappingRepository;
		private IRepository<AclRecord> _aclRepository;
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
			_productRepository = MockRepository.GenerateMock<IRepository<Product>>();
			_localizedPropertyRepository = MockRepository.GenerateMock<IRepository<LocalizedProperty>>();
			_storeMappingRepository = MockRepository.GenerateMock<IRepository<StoreMapping>>();
			_aclRepository = MockRepository.GenerateMock<IRepository<AclRecord>>();

			_linqCatalogSearchService = new LinqCatalogSearchService(_productRepository, _localizedPropertyRepository, _storeMappingRepository, _aclRepository);
		}

		[Test]
		public void LinqSearch_can_order_by_name()
		{
			var products = new List<Product>();

			for (var i = 97; i <= 110; ++i)
			{
				products.Add(new SearchProduct { Name = Convert.ToChar(i).ToString() });
			}

			InitMocks(products);

			var query = new CatalogSearchQuery();
			query.SortBy(ProductSortingEnum.NameDesc);

			var result = Search(query);
			var chars = string.Join(",", result.Hits.Select(x => x.Name));

			Assert.That(chars, Is.EqualTo("n,m,l,k,j,i,h,g,f,e,d,c,b,a"));
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
			Assert.That(result.Suggestions.Any(), Is.EqualTo(false));
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

			var result = Search(new CatalogSearchQuery(new string[] { "name", "sku" }, "P-6000-2", isExactMatch: true));

			Assert.That(result.Hits.Count, Is.EqualTo(2));
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

		#endregion

		internal class SearchProduct : Product
		{
			internal SearchProduct()
				: this((new Random()).Next(100, int.MaxValue))
			{
			}

			internal SearchProduct(int id)
			{
				Id = id;
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
		}
	}
}
