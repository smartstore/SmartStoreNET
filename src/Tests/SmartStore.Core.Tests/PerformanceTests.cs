//using System;
//using System.Collections.Generic;
//using System.Runtime.Caching;
//using System.Threading;
//using NUnit.Framework;
//using System.Linq;
//using SmartStore.ComponentModel;
//using SmartStore.Core.Domain.Catalog;
//using SmartStore.Tests;
//using System.Collections.Concurrent;
//using System.Threading.Tasks;

//namespace SmartStore.Core.Tests
//{
//	[TestFixture]
//	public class PerformanceTests
//	{
//		//[Test]
//		//public void InstantiatePerfTest()
//		//{
//		//	int cycles = 500000;

//		//	Chronometer.Measure(cycles, "Create Product NATIVE", i => new Product());
//		//	Chronometer.Measure(cycles, "Create Product Reflection", i => Activator.CreateInstance(typeof(Product)));
//		//	Chronometer.Measure(cycles, "Create Product FASTACTIVATOR", i => FastActivator.CreateInstance(typeof(Product)));

//		//	var list = new List<Product>();

//		//	Chronometer.Measure(cycles, "Create List<Product> NATIVE", i => new TestClass(list));
//		//	Chronometer.Measure(cycles, "Create List<Product> Reflection", i => Activator.CreateInstance(typeof(TestClass), list));
//		//	Chronometer.Measure(cycles, "Create List<Product> FASTACTIVATOR.CreateInstance()", i => FastActivator.CreateInstance(typeof(TestClass), list));

//		//	var ctor = typeof(TestClass).GetConstructor(new Type[] { typeof(List<Product>) });
//		//	//var activator = new FastActivator(ctor);
//		//	var activator = FastActivator.FindMatchingActivator(typeof(TestClass), list);
//		//	Chronometer.Measure(cycles, "Create List<Product> FASTACTIVATOR.Activate()", i => activator.Activate(list));

//		//	Chronometer.Measure(cycles, "Create List<Product> CTOR.Invoke()", i => ctor.Invoke(new object[] { list }));
//		//}

//		[Test]
//		public void MeasureMemCachePerformance()
//		{
//			var cache = new MemCache();
//			Chronometer.Measure(100, "MemCache", x => StressCache(cache, x));
//		}

//		[Test]
//		public void MeasureCdCachePerformance()
//		{
//			var cache = new MemCache();
//			Chronometer.Measure(100, "CdCache", x => StressCache(cache, x));
//		}

//		private void StressCache(ICache cache, int cycle)
//		{
//			for (int i = 0; i < 1000; i++)
//			{
//				var key = "key" + i;
//				if (!cache.TryGet("key" + i, out var obj))
//				{
//					cache.Put(key, new object());
//				}

//				if (i % 100 == 100)
//				{
//					var keys = cache.Keys();
//				}
//			}

//			for (int i = 0; i < 1000; i++)
//			{
//				var key = "key" + i;
//				if (cache.Contains(key))
//				{
//					cache.TryGet("key" + i, out var obj);
//				}

//				if (i % 10 == 10)
//				{
//					var keys = cache.Keys();
//				}
//			}

//			cache.Clear();

//			//for (int i = 0; i < 1000000; i++)
//			//{
//			//	if (i < 1000)
//			//	{

//			//	}
//			//}
//		}
//	}

//	internal interface ICache
//	{
//		bool TryGet(string key, out object value);
//		void Put(string key, object value, TimeSpan? duration = null);
//		bool Contains(string key);
//		void Remove(string key);
//		IEnumerable<string> Keys();
//		void Clear();
//	}

//	internal class CdCache : ICache
//	{
//		private readonly ConcurrentDictionary<string, object> _cache = new ConcurrentDictionary<string, object>(StringComparer.OrdinalIgnoreCase);

//		public void Clear()
//		{
//			_cache.Clear();
//		}

//		public bool Contains(string key)
//		{
//			return _cache.ContainsKey(key);
//		}

//		public IEnumerable<string> Keys()
//		{
//			return _cache.Keys;
//		}

//		public void Put(string key, object value, TimeSpan? duration = null)
//		{
//			_cache.AddOrUpdate(key, value, (k, v) => value);
//		}

//		public void Remove(string key)
//		{
//			_cache.TryRemove(key, out _);
//		}

//		public bool TryGet(string key, out object value)
//		{
//			return _cache.TryGetValue(key, out value);
//		}
//	}

//	internal class MemCache : ICache
//	{
//		private MemoryCache _cache = new MemoryCache("SmartStore");

//		public void Clear()
//		{
//			var oldCache = Interlocked.Exchange(ref _cache, new MemoryCache("SmartStore"));
//			oldCache.Dispose();
//			GC.Collect();
//		}

//		public bool Contains(string key)
//		{
//			return _cache.Contains(key);
//		}

//		public IEnumerable<string> Keys()
//		{
//			return _cache.AsParallel().Select(x => x.Key).ToArray();
//		}

//		public void Put(string key, object value, TimeSpan? duration = null)
//		{
//			var absoluteExpiration = ObjectCache.InfiniteAbsoluteExpiration;
//			if (duration.HasValue)
//			{
//				absoluteExpiration = DateTime.UtcNow + duration.Value;
//			}

//			var cacheItemPolicy = new CacheItemPolicy
//			{
//				AbsoluteExpiration = absoluteExpiration,
//				SlidingExpiration = ObjectCache.NoSlidingExpiration
//			};

//			_cache.Set(key, value, cacheItemPolicy);
//		}

//		public void Remove(string key)
//		{
//			_cache.Remove(key);
//		}

//		public bool TryGet(string key, out object value)
//		{
//			value = null;
//			object obj = _cache.Get(key);

//			if (obj != null)
//			{
//				value = obj;
//				return true;
//			}

//			return false;
//		}
//	}

//	//public class TestClass
//	//{
//	//	public TestClass()
//	//	{
//	//	}
//	//	public TestClass(IEnumerable<Product> param1)
//	//	{
//	//	}
//	//	public TestClass(int param1)
//	//	{
//	//	}
//	//	public TestClass(IEnumerable<Product> param1, int param2)
//	//	{
//	//	}
//	//	public TestClass(IEnumerable<Product> param1, int param2, string param3)
//	//	{
//	//	}
//	//	public TestClass(DateTime param1)
//	//	{
//	//	}
//	//	public TestClass(double param1)
//	//	{
//	//	}
//	//	public TestClass(decimal param1)
//	//	{
//	//	}
//	//	public TestClass(long param1)
//	//	{
//	//	}
//	//}

//}



