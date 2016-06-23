using System;
using System.Collections.Generic;

namespace SmartStore.Core.Caching
{
	public interface IOutputCacheProvider
	{
		OutputCacheItem Get(string key);
		void Set(string key, OutputCacheItem item);
		bool Exists(string key);

		void Remove(params string[] keys);
		void RemoveAll();

		IEnumerable<OutputCacheItem> All(int skip, int count);
		int Count();

		int InvalidateByRoute(params string[] routes);
		int InvalidateByTag(params string[] tags);
	}
}
