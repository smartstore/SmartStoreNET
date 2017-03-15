using System;
using System.Collections.Generic;
using SmartStore.Core.Plugins;

namespace SmartStore.Core.Caching
{
	[SystemName("OutputCacheProvider.Idle")]
	[FriendlyName("Idle")]
	public class NullOutputCacheProvider : IOutputCacheProvider
	{
		private static readonly IOutputCacheProvider s_instance = new NullOutputCacheProvider();

		public static IOutputCacheProvider Instance
		{
			get { return s_instance; }
		}

		public IPagedList<OutputCacheItem> All(int pageIndex, int pageSize, bool withContent = false)
		{
			return new PagedList<OutputCacheItem>(new List<OutputCacheItem>(), pageIndex, pageSize);
		}

		public int Count()
		{
			return 0;
		}

		public bool Exists(string key)
		{
			return false;
		}

		public OutputCacheItem Get(string key)
		{
			return null;
		}

		public int InvalidateByRoute(params string[] routes)
		{
			return 0;
		}

		public int InvalidateByPrefix(string keyPrefix)
		{
			return 0;
		}

		public int InvalidateByTag(params string[] tags)
		{
			return 0;
		}

		public void Remove(params string[] keys)
		{
		}

		public void RemoveAll()
		{
		}

		public void Set(string key, OutputCacheItem item)
		{
		}
	}
}
