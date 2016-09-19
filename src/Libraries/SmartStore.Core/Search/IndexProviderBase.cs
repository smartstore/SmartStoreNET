using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Core.Search
{
	public abstract class IndexProviderBase : DisposableObject, IIndexProvider
	{
		public abstract IEnumerable<string> EnumerateIndexes();

		public virtual IIndexDocument CreateDocument(int id)
		{
			Guard.IsPositive(id, nameof(id));
			return new IndexDocument(id);
		}

		public abstract IIndexStore GetIndexStore(string scope);

		public abstract ISearchEngine GetSearchEngine(IIndexStore store, SearchQuery query);

		protected override void OnDispose(bool disposing)
		{
		}
	}
}
