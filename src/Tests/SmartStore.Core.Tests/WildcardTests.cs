using System;
using NUnit.Framework;
using SmartStore.Utilities;

namespace SmartStore.Core.Tests
{
    [TestFixture]
    public class WildcardTests
    {
        [Test]
        public void Can_match_number_range()
        {
            var w1 = new Wildcard("999-2450", true);
            Console.WriteLine(w1.Pattern);
            Assert.IsTrue(w1.IsMatch("999"));
            Assert.IsTrue(w1.IsMatch("1500"));
            Assert.IsTrue(w1.IsMatch("2450"));
            Assert.IsFalse(w1.IsMatch("500"));
            Assert.IsFalse(w1.IsMatch("2800"));

            w1 = new Wildcard("50000-59999", true);
            Console.WriteLine(w1.Pattern);
            Assert.IsTrue(w1.IsMatch("59192"));
            Assert.IsTrue(w1.IsMatch("55000"));
            Assert.IsFalse(w1.IsMatch("500"));
            Assert.IsFalse(w1.IsMatch("80000"));

            w1 = new Wildcard("3266-3267", true);
            Console.WriteLine(w1.Pattern);
            Assert.IsTrue(w1.IsMatch("3266"));
            Assert.IsTrue(w1.IsMatch("3267"));
            Assert.IsFalse(w1.IsMatch("500"));
            Assert.IsFalse(w1.IsMatch("4000"));

            w1 = new Wildcard("0001000-0005000", true);
            Console.WriteLine(w1.Pattern);
            Assert.IsTrue(w1.IsMatch("0001000"));
            Assert.IsTrue(w1.IsMatch("0002008"));
            Assert.IsFalse(w1.IsMatch("1000"));
            Assert.IsFalse(w1.IsMatch("5000"));
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

        [Test]
        public void Can_match_glob()
        {
            var w1 = new Wildcard("h[ae]llo", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            Console.WriteLine(w1.Pattern);
            Assert.IsTrue(w1.IsMatch("hello"));
            Assert.IsTrue(w1.IsMatch("hallo"));
            Assert.IsTrue(!w1.IsMatch("hillo"));

            w1 = new Wildcard("h[^e]llo", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            Console.WriteLine(w1.Pattern);
            Assert.IsTrue(w1.IsMatch("hallo"));
            Assert.IsTrue(w1.IsMatch("hbllo"));
            Assert.IsTrue(!w1.IsMatch("hello"));

            w1 = new Wildcard("h[a-d]llo", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            Console.WriteLine(w1.Pattern);
            Assert.IsTrue(w1.IsMatch("hallo"));
            Assert.IsTrue(w1.IsMatch("hcllo"));
            Assert.IsTrue(w1.IsMatch("hdllo"));
            Assert.IsTrue(!w1.IsMatch("hgllo"));

            w1 = new Wildcard("entry-[^0]*", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            Console.WriteLine(w1.Pattern);
            Assert.IsTrue(w1.IsMatch("entry-22"));
            Assert.IsTrue(w1.IsMatch("entry-9"));
            Assert.IsTrue(!w1.IsMatch("entry-0"));
        }
    }
}
