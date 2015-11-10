using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using NUnit.Framework;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Tests;
using SmartStore.Utilities.Reflection;

namespace SmartStore.Core.Tests
{
	[TestFixture]
	public class PerformanceTests
	{
		[Test]
		public void InstantiatePerfTest()
		{
			int cycles = 500000;

			Chronometer.Measure(cycles, "Create Product NATIVE", i => new Product());
			Chronometer.Measure(cycles, "Create Product Reflection", i => Activator.CreateInstance(typeof(Product)));

			var list = new List<Product>();

			Chronometer.Measure(cycles, "Create List<Product> NATIVE", i => new List<Product>(list));
			Chronometer.Measure(cycles, "Create List<Product> Reflection", i => Activator.CreateInstance(typeof(List<Product>), list));

			var ctor = typeof(List<Product>).GetConstructor(new Type[] { typeof(List<Product>) });
			var activator = new FastActivator(ctor);
			Chronometer.Measure(cycles, "Create List<Product> FASTACTIVATOR", i => activator.Activate(list) );
		}
	}
}



