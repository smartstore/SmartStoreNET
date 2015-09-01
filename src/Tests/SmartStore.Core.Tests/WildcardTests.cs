using SmartStore.Tests;
using NUnit.Framework;
using System.Collections.Generic;
using System;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Infrastructure;
using SmartStore.Utilities;

namespace SmartStore.Core.Tests
{
    [TestFixture]
    public class WildcardTests
    {
		
		[Test]
        public void Can_match_number_range()
        {
			var w1 = new Wildcard("999-2450");
			Console.WriteLine(w1.Pattern);
			Assert.IsTrue(w1.IsMatch("999"));
			Assert.IsTrue(w1.IsMatch("1500"));
			Assert.IsTrue(w1.IsMatch("2450"));
			Assert.IsFalse(w1.IsMatch("500"));
			Assert.IsFalse(w1.IsMatch("2800"));

			w1 = new Wildcard("50000-59999");
			Console.WriteLine(w1.Pattern);
			Assert.IsTrue(w1.IsMatch("59192"));
			Assert.IsTrue(w1.IsMatch("55000"));
			Assert.IsFalse(w1.IsMatch("500"));
			Assert.IsFalse(w1.IsMatch("80000"));

			w1 = new Wildcard("3266-3267");
			Console.WriteLine(w1.Pattern);
			Assert.IsTrue(w1.IsMatch("3266"));
			Assert.IsTrue(w1.IsMatch("3267"));
			Assert.IsFalse(w1.IsMatch("500"));
			Assert.IsFalse(w1.IsMatch("4000"));

            w1 = new Wildcard("01000-01010");
            Console.WriteLine(w1.Pattern);
            Assert.IsTrue(w1.IsMatch("01000"));
            Assert.IsTrue(w1.IsMatch("01008"));
            Assert.IsFalse(w1.IsMatch("02200"));
            Assert.IsFalse(w1.IsMatch("1005"));
        }

		[Test]
		public void Can_match_wildcard()
		{
			var w1 = new Wildcard("H*o ?orld", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			Console.WriteLine(w1.Pattern);
			Assert.IsTrue(w1.IsMatch("Hello World"));
			Assert.IsTrue(w1.IsMatch("hello WORLD"));
			Assert.IsFalse(w1.IsMatch("world"));
			Assert.IsFalse(w1.IsMatch("Hell word"));
		}

    }
}
