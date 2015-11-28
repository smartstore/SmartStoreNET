using SmartStore.Tests;
using NUnit.Framework;
using System.Collections.Generic;
using System;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Infrastructure;

namespace SmartStore.Core.Tests
{
    [TestFixture]
    public class ConversionTests
    {
		
		[Test]
        public void Can_get_typed_value()
        {	
			"1000".Convert<int>().ShouldBe<int>();
			"1000".Convert<int>().ShouldEqual(1000);

			var intList = "1,2,3,4,5".Convert<List<int>>();
			intList.ShouldBe<List<int>>();
			Assert.AreEqual(5, intList.Count);
			Assert.AreEqual(3, intList[2]);

			var strList = "one,two,three".Convert<List<string>>();
			strList.ShouldBe<List<string>>();
			Assert.AreEqual(3, strList.Count);
			Assert.AreEqual("two", strList[1]);

			double dbl = 3;
			var r1 = dbl.Convert<int?>();
			r1.ShouldBe<int?>();
			Assert.AreEqual(r1.Value, 3);

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
			Assert.AreEqual(shippingOptions2[1].Description, "Desc2");

			var enu = SmartStore.Core.Domain.Catalog.AttributeControlType.FileUpload;
			Assert.AreEqual((int)enu, enu.Convert<int>());
			Assert.AreEqual("FileUpload", enu.Convert<string>());
        }
    }
}
