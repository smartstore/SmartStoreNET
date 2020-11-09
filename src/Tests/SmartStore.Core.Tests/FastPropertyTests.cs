using System;
using System.Collections.Generic;
using NUnit.Framework;
using SmartStore.ComponentModel;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Tests;

namespace SmartStore.Core.Tests
{
    [TestFixture]
    public class FastPropertyTests
    {
        [Test]
        public void CanCreateFastPropertyByLambda()
        {
            var fastProp = FastProperty.GetProperty<Product>(x => x.Name, PropertyCachingStrategy.Cached);
            fastProp.ShouldNotBeNull();

            Assert.AreEqual(fastProp.Property.Name, "Name");

            // from cache
            var fastProp2 = FastProperty.GetProperty<Product>(x => x.Name);
            Assert.AreSame(fastProp, fastProp2);
        }

        [Test]
        public void CanCreateFastPropertyByPropInfo()
        {
            var pi = typeof(Product).GetProperty("Name");

            var fastProp = FastProperty.GetProperty(pi, PropertyCachingStrategy.Cached);
            fastProp.ShouldNotBeNull();

            Assert.AreEqual(fastProp.Property.Name, "Name");

            // from cache
            var fastProp2 = FastProperty.GetProperty<Product>(x => x.Name);
            Assert.AreSame(fastProp, fastProp2);
            Assert.AreSame(fastProp.Property, pi);
        }

        [Test]
        public void CanCreateFastPropertyByName()
        {
            var fastProp = FastProperty.GetProperty(typeof(Product), "Name", PropertyCachingStrategy.Cached);
            fastProp.ShouldNotBeNull();

            Assert.AreEqual(fastProp.Property.Name, "Name");

            // from cache
            var fastProp2 = FastProperty.GetProperty<Product>(x => x.Name);
            Assert.AreSame(fastProp, fastProp2);

            var product = new Product { Name = "MyName" };
            var name = fastProp.GetValue(product);

            Assert.AreEqual("MyName", name);
        }
    }

    public class TestClass
    {
        public TestClass()
        {
        }
        public TestClass(IEnumerable<Product> param1)
        {
        }
        public TestClass(int param1)
        {
        }
        public TestClass(IEnumerable<Product> param1, int param2)
        {
        }
        public TestClass(IEnumerable<Product> param1, int param2, string param3)
        {
        }
        public TestClass(DateTime param1)
        {
        }
        public TestClass(double param1)
        {
        }
        public TestClass(decimal param1)
        {
        }
        public TestClass(long param1)
        {
        }
    }

}



