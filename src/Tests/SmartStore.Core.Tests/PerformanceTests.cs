//using System;
//using System.Collections.Generic;
//using NUnit.Framework;
//using SmartStore.Core.Domain.Catalog;
//using SmartStore.Tests;

//namespace SmartStore.Core.Tests
//{
//	[TestFixture]
//	public class PerformanceTests
//	{
//		[Test]
//		public void InstantiatePerfTest()
//		{
//			int cycles = 500000;
			
//			Chronometer.Measure(cycles, "Create Product NATIVE", i => new Product());
//			Chronometer.Measure(cycles, "Create Product Reflection", i => Activator.CreateInstance(typeof(Product)));
//			Chronometer.Measure(cycles, "Create Product FASTACTIVATOR", i => FastActivator.CreateInstance(typeof(Product)));

//			var list = new List<Product>();

//			Chronometer.Measure(cycles, "Create List<Product> NATIVE", i => new TestClass(list));
//			Chronometer.Measure(cycles, "Create List<Product> Reflection", i => Activator.CreateInstance(typeof(TestClass), list));
//			Chronometer.Measure(cycles, "Create List<Product> FASTACTIVATOR.CreateInstance()", i => FastActivator.CreateInstance(typeof(TestClass), list) );

//			var ctor = typeof(TestClass).GetConstructor(new Type[] { typeof(List<Product>) });
//			//var activator = new FastActivator(ctor);
//			var activator = FastActivator.FindMatchingActivator(typeof(TestClass), list);
//			Chronometer.Measure(cycles, "Create List<Product> FASTACTIVATOR.Activate()", i => activator.Activate(list));

//			Chronometer.Measure(cycles, "Create List<Product> CTOR.Invoke()", i => ctor.Invoke(new object[] { list }));
//		}
//	}

//	public class TestClass
//	{
//		public TestClass()
//		{
//		}
//		public TestClass(IEnumerable<Product> param1)
//		{
//		}
//		public TestClass(int param1)
//		{
//		}
//		public TestClass(IEnumerable<Product> param1, int param2)
//		{
//		}
//		public TestClass(IEnumerable<Product> param1, int param2, string param3)
//		{
//		}
//		public TestClass(DateTime param1)
//		{
//		}
//		public TestClass(double param1)
//		{
//		}
//		public TestClass(decimal param1)
//		{
//		}
//		public TestClass(long param1)
//		{
//		}
//	}

//}



