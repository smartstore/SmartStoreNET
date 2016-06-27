using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartStore.Core.Plugins;

namespace SmartStore.Core.Caching
{
	[SystemName("NullOutputCacheProvider")]
	[FriendlyName("Idle")]
	public class NullOutputCacheProvider : IOutputCacheProvider
	{
		public IEnumerable<OutputCacheItem> All(int skip, int count)
		{
			return Enumerable.Empty<OutputCacheItem>();
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
