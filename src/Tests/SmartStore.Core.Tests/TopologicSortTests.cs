using System;
using System.Linq;
using NUnit.Framework;
using SmartStore.Collections;

namespace SmartStore.Core.Tests
{
    [TestFixture]
    public class TopologicSortTests
    {
        private class SortableItem : ITopologicSortable<string>
        {
            public string Key { get; set; }
            public string[] DependsOn { get; set; }
        }

        [Test]
        public void Can_sort_topological()
        {
            /*
					A
					|
				----------
				|        |
				B        C
						 |
					-----------
					|         |
					D         E
			 		|		  |
			 		H		  F
			 		|		  |
			 		I		  G
			*/

            var a = new SortableItem { Key = "A" };
            var b = new SortableItem { Key = "B", DependsOn = new string[] { "A" } };
            var c = new SortableItem { Key = "C", DependsOn = new string[] { "a" } };
            var d = new SortableItem { Key = "D", DependsOn = new string[] { "C" } };
            var e = new SortableItem { Key = "E", DependsOn = new string[] { "c" } };
            var f = new SortableItem { Key = "F", DependsOn = new string[] { "e" } };
            var g = new SortableItem { Key = "G", DependsOn = new string[] { "f" } };
            var h = new SortableItem { Key = "H", DependsOn = new string[] { "D" } };
            var i = new SortableItem { Key = "I", DependsOn = new string[] { "H" } };

            var items = new SortableItem[] { c, e, b, d, a, i, g, f, h };

            var sortedItems = items.SortTopological(StringComparer.OrdinalIgnoreCase);
            Console.WriteLine(String.Join(", ", sortedItems.Select(x => x.Key).ToArray()));

            Assert.AreEqual(items.Length, sortedItems.Length);
            Assert.Less(Array.IndexOf(sortedItems, a), Array.IndexOf(sortedItems, b));
            Assert.Less(Array.IndexOf(sortedItems, a), Array.IndexOf(sortedItems, c));
            Assert.Less(Array.IndexOf(sortedItems, a), Array.IndexOf(sortedItems, d));
            Assert.Less(Array.IndexOf(sortedItems, a), Array.IndexOf(sortedItems, e));
            Assert.Less(Array.IndexOf(sortedItems, c), Array.IndexOf(sortedItems, d));
            Assert.Less(Array.IndexOf(sortedItems, c), Array.IndexOf(sortedItems, e));
            Assert.Less(Array.IndexOf(sortedItems, d), Array.IndexOf(sortedItems, h));
            Assert.Less(Array.IndexOf(sortedItems, h), Array.IndexOf(sortedItems, i));
            Assert.Less(Array.IndexOf(sortedItems, e), Array.IndexOf(sortedItems, f));
            Assert.Less(Array.IndexOf(sortedItems, f), Array.IndexOf(sortedItems, g));
        }

        [Test]
        public void Can_detect_cycles()
        {
            /*
					A
					|
				----------
				|        |
				B        C
						 |
					-----------
					|         
					D         E<-
			 		|		  |  |
			 		G		  F--
			*/

            var a = new SortableItem { Key = "A" };
            var b = new SortableItem { Key = "B", DependsOn = new string[] { "A" } };
            var c = new SortableItem { Key = "C", DependsOn = new string[] { "a" } };
            var d = new SortableItem { Key = "D", DependsOn = new string[] { "C" } };
            var e = new SortableItem { Key = "E", DependsOn = new string[] { "f" } };
            var f = new SortableItem { Key = "F", DependsOn = new string[] { "e" } };
            var g = new SortableItem { Key = "G", DependsOn = new string[] { "D" } };

            var items = new SortableItem[] { c, e, b, d, a, g, f };

            Assert.Throws<CyclicDependencyException>(() => items.SortTopological(StringComparer.OrdinalIgnoreCase));
        }
    }
}
