using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using SmartStore.ComponentModel;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Email;
using SmartStore.Tests;

namespace SmartStore.Core.Tests
{
    [TestFixture]
    public class ConversionTests
    {
        [Test]
        public void CanConvertNullables()
        {
            var r1 = ((double)3).Convert<double?>();
            r1.ShouldBe<double?>();
            Assert.AreEqual(r1.Value, 3);

            var r2 = ((double?)3).Convert<double>();
            r2.ShouldBe<double>();
            Assert.AreEqual(r2, 3);

            var r3 = (true).Convert<bool?>();
            r3.ShouldBe<bool?>();
            Assert.AreEqual(r3.Value, true);

            var r4 = ("1000").Convert<double?>();
            r4.ShouldBe<double?>();
            Assert.AreEqual(r4.Value, 1000);

            var r5 = ((int?)5).Convert<long>();
            r5.ShouldBe<long>();
            Assert.AreEqual(r5, 5);

            var r6 = ((short)5).Convert(typeof(int));
            r6.ShouldBe<int>();
            Assert.AreEqual(r6, 5);
        }

        [Test]
        public void CanConvertEnums()
        {
            var e1 = ("CreateInstance").Convert<BindingFlags>();
            e1.ShouldBe<BindingFlags>();
            Assert.AreEqual(e1, BindingFlags.CreateInstance);

            var e2 = ("CreateInstance").Convert<BindingFlags?>();
            e2.ShouldBe<BindingFlags?>();
            Assert.AreEqual(e2.Value, BindingFlags.CreateInstance);

            BindingFlags flags = BindingFlags.CreateInstance | BindingFlags.GetProperty | BindingFlags.IgnoreCase;

            var e3 = (flags).Convert<string>();
            e3.ShouldBe<string>();
            Assert.AreEqual(e3, "IgnoreCase, CreateInstance, GetProperty");

            var e5 = (e3).Convert<BindingFlags?>();
            e5.ShouldBe<BindingFlags?>();
            Assert.AreEqual(e5.Value, flags);

            var e4 = (flags).Convert<int>();
            e4.ShouldBe<int>();
            Assert.AreEqual(e4, 4609);

            var enu = SmartStore.Core.Domain.Catalog.AttributeControlType.FileUpload;
            Assert.AreEqual((int)enu, enu.Convert<int>());
            Assert.AreEqual("FileUpload", enu.Convert<string>());
        }

        [Test]
        public void CanConvertBoolean()
        {
            var b = ("yes").Convert<bool>();
            Assert.AreEqual(b, true);

            b = ("off").Convert<bool>();
            Assert.AreEqual(b, false);

            b = (1).Convert<bool>();
            Assert.AreEqual(b, true);

            b = (0).Convert<bool>();
            Assert.AreEqual(b, false);

            var s = (true).Convert<string>();
            Assert.AreEqual(s, "True");

            var bn = ("true").Convert<bool?>();
            Assert.AreEqual(bn.Value, true);

            bn = ("wahr").Convert<bool?>();
            Assert.AreEqual(bn.Value, true);

            bn = ("").Convert<bool?>();
            Assert.AreEqual(bn.HasValue, false);
        }

        [Test]
        public void CanConvertNumerics()
        {
            "1000".Convert<int>().ShouldBe<int>();
            "1000".Convert<int>().ShouldEqual(1000);

            var i = ((short?)null).Convert<int?>();
            Assert.AreEqual(i.HasValue, false);

            var sh = ((decimal?)10).Convert<short>();
            sh.ShouldBe<short>();
            Assert.AreEqual(sh, 10);

            var dec = ((double)10).Convert<decimal?>();
            dec.ShouldBe<decimal?>();
            Assert.AreEqual(dec.Value, 10);

            var dbl = ((decimal)10).Convert<double?>();
            dbl.ShouldBe<double?>();
            Assert.AreEqual(dbl.Value, 10);

            var f = (20f).Convert<int?>();
            f.ShouldBe<int?>();
            Assert.AreEqual(f.Value, 20);

            var f2 = ((float?)20f).Convert<int>();
            f2.ShouldBe<int>();
            Assert.AreEqual(f2, 20);

            var culture = CultureInfo.GetCultureInfoByIetfLanguageTag("de-DE");

            ("123567896,54").Convert<decimal>(culture).ShouldBe<decimal>();
        }

        [Test]
        public void CanConvertDateTime()
        {
            var dt = ((double)40248.3926).Convert<DateTime>();
            dt.ShouldBe<DateTime>();
            dt.Year.ShouldEqual(2010);
            dt.Month.ShouldEqual(3);
            dt.Day.ShouldEqual(11);
        }

        [Test]
        public void CanConvertEnumerables()
        {
            var list = "1,2,3,4,5".Convert<IList<int>>();
            list.ShouldBe<List<int>>();
            Assert.AreEqual(5, list.Count);
            Assert.AreEqual(3, list[2]);

            var list2 = "1,0,off,wahr,false,y,n".Convert<ICollection<bool>>();
            list2.ShouldBe<List<bool>>();
            Assert.AreEqual(7, list2.Count);
            Assert.AreEqual(true, list2.ElementAt(3));

            "1,2,3,4,5".Convert<IReadOnlyCollection<int>>().ShouldBe<ReadOnlyCollection<int>>();
            "1,2,3,4,5".Convert<IReadOnlyList<int>>().ShouldBe<ReadOnlyCollection<int>>();
            "1,2,3,4,5".Convert<HashSet<double>>().ShouldBe<HashSet<double>>();
            "1,2,3,4,5".Convert<Stack<int>>().ShouldBe<Stack<int>>();
            "1,2,3,4,5".Convert<ISet<int>>().ShouldBe<HashSet<int>>();
            "1,2,3,4,5".Convert<Queue<int>>().ShouldBe<Queue<int>>();
            "1,2,3,4,5".Convert<LinkedList<string>>().ShouldBe<LinkedList<string>>();
            "1,2,3,4,5".Convert<ConcurrentBag<int>>().ShouldBe<ConcurrentBag<int>>();
            "1,2,3,4,5".Convert<ArraySegment<int>>().ShouldBe<ArraySegment<int>>();

            var list3 = new List<int>(new int[] { 1, 2, 3, 4, 5 });
            var str = list3.Convert<string>();
            Assert.AreEqual("1,2,3,4,5", str);

            var converter = TypeConverterFactory.GetConverter<double[]>();
            converter.ShouldBe<EnumerableConverter<double>>();

            var arr3 = list3.Convert<int[]>();
            arr3.ShouldBe<int[]>();
            Assert.AreEqual(5, list3.Count);
            Assert.AreEqual(3, list3[2]);

            var list4 = ((double)5).Convert<List<int>>();
            list4.ShouldBe<List<int>>();
            Assert.AreEqual(1, list4.Count);
            Assert.AreEqual(5, list4[0]);

            var list5 = new List<string>(new string[] { "1", "2", "3", "4", "5" });
            var arr4 = list5.Convert<float[]>();
            arr4.ShouldBe<float[]>();
            Assert.AreEqual(5, list5.Count);
            Assert.AreEqual("4", list5[3]);
        }

        [Test]
        public void CanConvertShippingOptions()
        {
            var shippingOption = new ShippingOption
            {
                ShippingMethodId = 2,
                Name = "Name",
                Description = "Desc",
                Rate = 1,
                ShippingRateComputationMethodSystemName = "SystemName"
            };
            var soStr = shippingOption.Convert<string>();
            Assert.IsNotEmpty(soStr);

            var arr = (new[] { shippingOption.Convert<string>() }).Convert<ShippingOption[]>();
            arr.ShouldBe<ShippingOption[]>();
            Assert.AreEqual(1, arr.Length);
            Assert.AreEqual(arr[0].Name, "Name");

            shippingOption = soStr.Convert<ShippingOption>();
            Assert.IsNotNull(shippingOption);
            Assert.AreEqual(shippingOption.ShippingMethodId, 2);
            Assert.AreEqual(shippingOption.Name, "Name");
            Assert.AreEqual(shippingOption.Description, "Desc");
            Assert.AreEqual(shippingOption.Rate, 1);
            Assert.AreEqual(shippingOption.ShippingRateComputationMethodSystemName, "SystemName");

            var shippingOptions = new List<ShippingOption>
            {
                new ShippingOption { ShippingMethodId = 1, Name = "Name1", Description = "Desc1" },
                new ShippingOption { ShippingMethodId = 2, Name = "Name2", Description = "Desc2" }
            };
            soStr = shippingOptions.Convert<string>();
            Assert.IsNotEmpty(soStr);

            shippingOptions = soStr.Convert<List<ShippingOption>>();
            Assert.AreEqual(shippingOptions.Count, 2);
            Assert.AreEqual(shippingOptions[1].ShippingMethodId, 2);
            Assert.AreEqual(shippingOptions[1].Description, "Desc2");

            var shippingOptions2 = soStr.Convert<IList<ShippingOption>>();
            Assert.AreEqual(shippingOptions2.Count, 2);
            Assert.AreEqual(shippingOptions[1].ShippingMethodId, 2);
            Assert.AreEqual(shippingOptions2.First().Description, "Desc1");
        }

        [Test]
        public void CanConvertEmailAddress()
        {
            var list = (new[] { new EmailAddress("test@domain.com") }).Convert<IList<string>>();
            list.ShouldBe<IList<string>>();
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual("test@domain.com", list[0]);

            var list2 = (new[] { "test@domain.com", "test2@domain.com" }).Convert<HashSet<EmailAddress>>();
            list2.ShouldBe<HashSet<EmailAddress>>();
            Assert.AreEqual(2, list2.Count);
            Assert.AreEqual("test2@domain.com", list2.ElementAt(1).Address);
        }
    }
}
