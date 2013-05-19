using System;
using System.Data.Entity;
using SmartStore.Tests;
using NUnit.Framework;

namespace SmartStore.Data.Tests
{
    [TestFixture]
    public class SchemaTests
    {
        [Test]
        public void Can_generate_schema()
        {
            Database.SetInitializer<SmartObjectContext>(null);
            var ctx = new SmartObjectContext("Test");
            string result = ctx.CreateDatabaseScript();
            result.ShouldNotBeNull();
            Console.Write(result);
        }
    }
}
