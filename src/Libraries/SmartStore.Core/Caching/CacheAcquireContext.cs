using System;
using System.Collections.Generic;
using SmartStore.Utilities;

namespace SmartStore.Core.Caching
{
	public class CacheAcquireContext
	{
		private readonly CacheAcquireContext _parent;
		private readonly HashSet<string> _dependentKeys = new HashSet<string>();

		private CacheAcquireContext(string key, CacheAcquireContext parent)
		{
			Key = key;
			_parent = parent;

			if (parent != null)
				parent._dependentKeys.Add(key);
		}

		public static CacheAcquireContext Current { get; set; }

		public static IDisposable Begin(string key)
		{
			Current = new CacheAcquireContext(key, Current);
			var action = new ActionDisposable(() => 
			{
				Current = Current?._parent;
			});

			return action;
		}

		public string Key { get; set; }

		public IEnumerable<string> DependentKeys
		{
			get { return _dependentKeys; }
		}
	}
}
