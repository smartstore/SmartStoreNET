using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Core.Search
{
	public class DefaultIndexManager : IIndexManager
	{
		private readonly IEnumerable<IIndexProvider> _providers;

		public DefaultIndexManager(IEnumerable<IIndexProvider> providers)
		{
			_providers = providers;
		}

		public bool HasAnyProvider()
		{
			return _providers.Any();
		}

		public IIndexProvider GetProvider()
		{
			return _providers.FirstOrDefault();
		}


	}
}
