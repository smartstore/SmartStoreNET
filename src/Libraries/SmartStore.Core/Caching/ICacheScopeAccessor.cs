using System;

namespace SmartStore.Core.Caching
{
	public interface ICacheScopeAccessor
	{
		CacheScope Current { get; }
		void PropagateKey(string key);
		IDisposable BeginScope(string key, bool independent = false);
	}
}
