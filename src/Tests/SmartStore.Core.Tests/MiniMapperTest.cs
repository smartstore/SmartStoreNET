using System;
using System.Collections.Generic;
using NUnit.Framework;
using SmartStore.ComponentModel;

namespace SmartStore.Core.Tests
{
    [TestFixture]
    public class MiniMapperTest
    {
        [Test]
        public void CanMap1()
        {
            var from = new MapClass1
            {
                Prop1 = "Prop1",
                Prop2 = "Prop2",
                Prop3 = "Prop3",
                Prop4 = 99,
                Prop5 = new ConsoleKey[] { ConsoleKey.Backspace, ConsoleKey.Tab, ConsoleKey.Clear }
            };
            from.Address.FirstName = "John";
            from.Address.LastName = "Doe";
            from.Address.Age = 24;

            var to = MiniMapper.Map<MapClass1, MapClass2>(from);

            Assert.AreEqual(from.Prop1, to.Prop1);
            Assert.AreEqual(from.Prop2, to.Prop2);
            Assert.AreEqual(from.Prop3, to.Prop3);
            Assert.AreEqual(from.Prop4, to.Prop4);
            Assert.AreEqual(from.Prop5.Length, to.Prop5.Count);
            Assert.AreEqual((int)from.Prop5[0], to.Prop5[0]);
            Assert.AreEqual((int)from.Prop5[1], to.Prop5[1]);
            Assert.AreEqual((int)from.Prop5[2], to.Prop5[2]);

            var dict = to.Address;
            Assert.AreEqual(dict.Count, 3);
            Assert.AreEqual(dict["FirstName"], from.Address.FirstName);
            Assert.AreEqual(dict["LastName"], from.Address.LastName);
            Assert.AreEqual(dict["Age"], from.Address.Age);
        }

        [Test]
        public void CanMap2()
        {
            var from = new MapClass2
            {
                Prop1 = "Prop1",
                Prop2 = "Prop2",
                Prop3 = "Prop3"
            };
            from.Address["FirstName"] = "John";
            from.Address["LastName"] = "Doe";
            from.Address["Age"] = 24;

            var to = MiniMapper.Map<MapClass2, MapClass1>(from);

            Assert.AreEqual(from.Prop1, to.Prop1);
            Assert.AreEqual(from.Prop2, to.Prop2);
            Assert.AreEqual(from.Prop3, to.Prop3);

            var dict = from.Address;
            Assert.AreEqual(dict.Count, 3);
            Assert.AreEqual(dict["FirstName"], to.Address.FirstName);
            Assert.AreEqual(dict["LastName"], to.Address.LastName);
            Assert.AreEqual(dict["Age"], to.Address.Age);
        }

        [Test]
        public void CanMapAnonymousType()
        {
            var from = new
            {
                Prop1 = "Prop1",
                Prop2 = "Prop2",
                Prop3 = "Prop3",
                Prop4 = 99f,
                Address = new
                {
                    FirstName = "John",
                    LastName = "Doe",
                    Age = 24
                }
            };

            var to = new MapClass2();
            MiniMapper.Map(from, to);

            Assert.AreEqual(from.Prop1, to.Prop1);
            Assert.AreEqual(from.Prop2, to.Prop2);
            Assert.AreEqual(from.Prop3, to.Prop3);
            Assert.AreEqual(from.Prop4, to.Prop4);

            var dict = to.Address;
            Assert.AreEqual(dict.Count, 3);
            Assert.AreEqual(dict["FirstName"], from.Address.FirstName);
            Assert.AreEqual(dict["LastName"], from.Address.LastName);
            Assert.AreEqual(dict["Age"], from.Address.Age);
        }
    }

    public class MapClass1
    {
        public string Prop1 { get; set; }
        public string Prop2 { get; set; }
        public string Prop3 { get; set; }
        public float? Prop4 { get; set; }
        public ConsoleKey[] Prop5 { get; set; }
        public MapNestedClass Address { get; set; } = new MapNestedClass();
    }

    public class MapClass2
    {
        public string Prop1 { get; set; }
        public string Prop2 { get; set; }
        public string Prop3 { get; set; }
        public int Prop4 { get; set; }
        public List<int> Prop5 { get; set; }
        public IDictionary<string, object> Address { get; set; } = new Dictionary<string, object>();
    }

    public class MapNestedClass
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
    }
}
