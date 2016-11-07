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

		private static Product GetSimpleProduct(string name, string shortDescription = null)
		{
			var product = new Product
			{
				Name = name.EmptyNull(),
				ShortDescription = shortDescription.EmptyNull(),
				FullDescription = "Enthusiastically utilize compelling systems with vertical collaboration and idea-sharing. Interactively incubate bleeding-edge innovation with future-proof catalysts for change. Distinctively exploit parallel paradigms rather than progressive scenarios. Compellingly synergize visionary ROI after process-centric resources. Objectively negotiate performance based best practices with 24/7 vortals. Globally pontificate reliable processes for innovative services. Monotonectally enable mission - critical information and quality.",
				Sku = "P-6000-2",
				Published = true,
				VisibleIndividually = true,
				ProductTypeId = (int)ProductType.SimpleProduct,
				StockQuantity = 10000,
				CreatedOnUtc = new DateTime(2016, 8, 24)
			};

			return product;
		}

		private void InitMocks(List<Product> products)
		{
			InitMocks(products, new List<LocalizedProperty>(), new List<StoreMapping>(), new List<AclRecord>());
		}
		private void InitMocks(List<Product> products, List<LocalizedProperty> localized, List<StoreMapping> storeMappings, List<AclRecord> aclRecords)
		{
			_productRepository.Expect(x => x.Table).Return(products.AsQueryable());
			_productRepository.Expect(x => x.TableUntracked).Return(products.AsQueryable());

			_localizedPropertyRepository.Expect(x => x.Table).Return(localized.AsQueryable());
			_localizedPropertyRepository.Expect(x => x.TableUntracked).Return(localized.AsQueryable());

			_storeMappingRepository.Expect(x => x.Table).Return(storeMappings.AsQueryable());
			_storeMappingRepository.Expect(x => x.TableUntracked).Return(storeMappings.AsQueryable());

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

		#region Term search

		[Test]
		public void LinqSearch_not_find_anything()
		{
			var products = new List<Product>
			{
				GetSimpleProduct("SmartStore.biz 6"),
				GetSimpleProduct("Apple iPhone Smartphone 6"),
				GetSimpleProduct("Energistically recaptiualize superior e-markets without next-generation platforms"),
				GetSimpleProduct("Rapidiously conceptualize future-proof imperatives", "Shopping System powered by SmartStore")
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
				GetSimpleProduct("SmartStore.biz 6"),
				GetSimpleProduct("Apple iPhone Smartphone 6"),
				GetSimpleProduct("Energistically recaptiualize superior e-markets without next-generation platforms"),
				GetSimpleProduct("Rapidiously conceptualize future-proof imperatives", "Shopping System powered by SmartStore")
			};

			InitMocks(products);

			var result = Search(new CatalogSearchQuery(new string[] { "name", "shortdescription" }, "Smart"));

			Assert.That(result.Hits.Count, Is.EqualTo(3));
		}

		#endregion
	}
}
