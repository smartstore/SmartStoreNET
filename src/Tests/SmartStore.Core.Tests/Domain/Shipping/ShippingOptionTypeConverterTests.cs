using System.ComponentModel;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Tests;
using NUnit.Framework;
using SmartStore.Utilities;

namespace SmartStore.Core.Tests.Domain.Shipping
{
    [TestFixture]
    public class ShippingOptionTypeConverterTests
    {
        [SetUp]
        public void SetUp()
        {
        }

        [Test]
        public void Can_get_type_converter()
        {
			var converter = CommonHelper.GetTypeConverter(typeof(ShippingOption));
            converter.GetType().ShouldEqual(typeof(ShippingOptionTypeConverter));
        }

        [Test]
        public void Can_convert_shippingOption_to_string_and_back()
        {
            var shippingOptionInput = new ShippingOption()
            {
                Name = "1",
                Description = "2",
                Rate = 3.57M,
                ShippingRateComputationMethodSystemName = "4"
            };
            var converter = TypeDescriptor.GetConverter(shippingOptionInput.GetType());
            var result = converter.ConvertTo(shippingOptionInput, typeof(string)) as string;

            var shippingOptionOutput = converter.ConvertFrom(result) as ShippingOption;
            shippingOptionOutput.ShouldNotBeNull();
            shippingOptionOutput.Name.ShouldEqual("1");
            shippingOptionOutput.Description.ShouldEqual("2");
            shippingOptionOutput.Rate.ShouldEqual(3.57M);
            shippingOptionOutput.ShippingRateComputationMethodSystemName.ShouldEqual("4");
        }
    }
}
