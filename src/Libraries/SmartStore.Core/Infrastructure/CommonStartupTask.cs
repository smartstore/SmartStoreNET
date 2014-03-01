using System;
using System.Collections.Generic;
using System.ComponentModel;
using SmartStore.Core.ComponentModel;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Shipping;

namespace SmartStore.Core.Infrastructure
{
	public class CommonStartupTask : IStartupTask
	{
		public void Execute()
		{
			// register type  converters
			TypeDescriptor.AddAttributes(typeof(List<int>), new TypeConverterAttribute(typeof(GenericListTypeConverter<int>)));
			TypeDescriptor.AddAttributes(typeof(List<decimal>), new TypeConverterAttribute(typeof(GenericListTypeConverter<decimal>)));
			TypeDescriptor.AddAttributes(typeof(List<string>), new TypeConverterAttribute(typeof(GenericListTypeConverter<string>)));
			TypeDescriptor.AddAttributes(typeof(ShippingOption), new TypeConverterAttribute(typeof(ShippingOptionTypeConverter)));
			TypeDescriptor.AddAttributes(typeof(List<ShippingOption>), new TypeConverterAttribute(typeof(ShippingOptionListTypeConverter)));
			TypeDescriptor.AddAttributes(typeof(IList<ShippingOption>), new TypeConverterAttribute(typeof(ShippingOptionListTypeConverter)));
			TypeDescriptor.AddAttributes(typeof(List<ProductBundleItemOrderData>), new TypeConverterAttribute(typeof(ProductBundleDataListTypeConverter)));
			TypeDescriptor.AddAttributes(typeof(IList<ProductBundleItemOrderData>), new TypeConverterAttribute(typeof(ProductBundleDataListTypeConverter)));
		}

		public int Order
		{
			get { return Int32.MinValue; }
		}
	}
}
