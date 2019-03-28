using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using SmartStore.Utilities;

namespace SmartStore.Core.Caching
{
	public class CacheAcquireContext
	{
		private readonly CacheAcquireContext _parent;
		private readonly HashSet<string> _dependentKeys = new HashSet<string>();

		private readonly static ConcurrentStack<CacheAcquireContext> _stack = new ConcurrentStack<CacheAcquireContext>();

		private CacheAcquireContext(string key, CacheAcquireContext parent)
		{
			Key = key;
			_parent = parent;

			if (_parent != null && !string.IsNullOrWhiteSpace(key))
				_parent._dependentKeys.Add(key);
		}

		public static CacheAcquireContext Current
		{
			get
			{
				_stack.TryPeek(out var current);
				return current;
			}
		}

		public static IDisposable Begin(string key)
		{
			return ActionDisposable.Empty;

			//_stack.TryPeek(out var current);
			//_stack.Push(new CacheAcquireContext(key, current));

			//var action = new ActionDisposable(() =>
			//{
			//	_stack.TryPop(out _);
			//});

			//return action;
		}

		public string Key { get; set; }

		public IEnumerable<string> DependentKeys
		{
			get { return _dependentKeys; }
		}
	}
}
