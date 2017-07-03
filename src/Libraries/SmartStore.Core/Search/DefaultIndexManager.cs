using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartStore.Core.Search
{
	public class DefaultIndexManager : IIndexManager
	{
		private readonly IEnumerable<Lazy<IIndexProvider>> _providers;

		public DefaultIndexManager(IEnumerable<Lazy<IIndexProvider>> providers)
		{
			_providers = providers;
		}

		public bool HasAnyProvider(bool activeOnly = true)
		{
			return _providers.Any(x => !activeOnly || x.Value.IsActive);
		}

		public IIndexProvider GetIndexProvider(bool activeOnly = true)
		{
			return _providers.FirstOrDefault(x => !activeOnly || x.Value.IsActive)?.Value;
		}
	}
}
