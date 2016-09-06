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

		public bool HasAnyProvider()
		{
			return _providers.Any();
		}

		public IIndexProvider GetIndexProvider()
		{
			return _providers.FirstOrDefault()?.Value;
		}


	}
}
