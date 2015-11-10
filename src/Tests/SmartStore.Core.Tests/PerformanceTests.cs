using SmartStore.Tests;
using NUnit.Framework;
using System.Diagnostics;
using System;
using SmartStore.Core.Domain.Catalog;
using System.Collections.Generic;

namespace SmartStore.Core.Tests
{
	[TestFixture]
	public class PerformanceTests
	{
		[Test]
		public void InstantiatePerfTest()
		{
			int cycles = 1000000;

			Chronometer.Measure(cycles, "Create Product NATIVE", i => new Product());
			Chronometer.Measure(cycles, "Create Product Reflection", i => Activator.CreateInstance<Product>());

			var list = new List<Product>();

			Chronometer.Measure(cycles, "Create List<Product> NATIVE", i => new List<Product>(list));
			Chronometer.Measure(cycles, "Create List<Product> Reflection", i => Activator.CreateInstance(typeof(List<Product>), list));
		}
	}
}



