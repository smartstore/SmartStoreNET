using System;
using System.Linq;
using NUnit.Framework;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Services.DataExchange;
using SmartStore.Tests;

namespace SmartStore.Services.Tests.DataExchange
{
    [TestFixture]
    public class SyncMappingServiceTests : ServiceTest
    {
        IRepository<SyncMapping> _rs;
        ISyncMappingService _service;

        [SetUp]
        public new void SetUp()
        {
            _rs = new MemoryRepository<SyncMapping>();
            _service = new SyncMappingService(_rs);

            _service.InsertSyncMappings(
                "App1", "Product",
                new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 },
                new string[] { "e1", "e2", "e3", "e4", "e5", "e6", "e7", "e8", "e9", "e10" }
            );

            _service.InsertSyncMappings(
                "App1", "Order",
                new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 },
                new string[] { "e1", "e2", "e3", "e4", "e5", "e6", "e7", "e8", "e9", "e10" }
            );

            _service.InsertSyncMappings(
                "App2", "Product",
                new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 },
                new string[] { "e1", "e2", "e3", "e4", "e5", "e6", "e7", "e8", "e9", "e10" }
            );

            _service.InsertSyncMappings(
                "App2", "Order",
                new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 },
                new string[] { "e1", "e2", "e3", "e4", "e5", "e6", "e7", "e8", "e9", "e10" }
            );

            int i = 1;
            foreach (var m in _rs.Table)
            {
                m.Id = i;
                i++;
            }
        }

        [Test]
        public void Throws_on_invalid_sequences()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                _service.InsertSyncMappings(
                    "App", "Entity",
                    new int[] { 1, 2, 3, 4, 5, 6, 7, 8 },
                    new string[] { "e1", "e2", "e3", "e4", "e5", "e6", "e7", "e8", "e9", "e10" }
                );
            });

            Assert.Throws<InvalidOperationException>(() =>
            {
                _service.InsertSyncMappings(
                    "App", "Entity",
                    new int[] { },
                    new string[] { }
                );
            });
        }

        [Test]
        public void Can_load_all_mappings()
        {
            var mappings = _service.GetAllSyncMappings();
            mappings.Count.ShouldEqual(40);

            var mappings2 = _service.GetAllSyncMappings("App1");
            mappings2.Count.ShouldEqual(20);

            var mappings3 = _service.GetAllSyncMappings(null, "Product");
            mappings3.Count.ShouldEqual(20);

            var mappings4 = _service.GetAllSyncMappings("App2", "Order");
            mappings4.Count.ShouldEqual(10);
        }

        [Test]
        public void Can_load_mapping_by_entity()
        {
            var product = new Product { Id = 5 };

            var mapping = _service.GetSyncMappingByEntity(product, "App2");
            Assert.NotNull(mapping);
            mapping.EntityName.ShouldEqual("Product");
            mapping.EntityId.ShouldEqual(5);
            mapping.SourceKey.ShouldEqual("e5");
        }

        [Test]
        public void Can_load_mapping_by_source()
        {
            var mapping = _service.GetSyncMappingBySource("e3", "Order", "App1");
            Assert.NotNull(mapping);
            mapping.EntityName.ShouldEqual("Order");
            mapping.ContextName.ShouldEqual("App1");
            mapping.EntityId.ShouldEqual(3);
            mapping.SourceKey.ShouldEqual("e3");
        }

        [Test]
        public void Can_delete_mappings()
        {
            _rs.Table.Count().ShouldEqual(40);

            _service.DeleteSyncMappings("App1");
            _rs.Table.Count().ShouldEqual(20);

            _service.DeleteSyncMappings("App2", "Product");
            _rs.Table.Count().ShouldEqual(10);

            _service.DeleteSyncMappings("App2", "Order");
            _rs.Table.Count().ShouldEqual(0);
        }

        [Test]
        public void Can_delete_mappings_for_entity()
        {
            var product = new Product { Id = 5 };

            _service.DeleteSyncMappingsFor(product);
            _rs.Table.Count().ShouldEqual(38);
        }

    }
}
