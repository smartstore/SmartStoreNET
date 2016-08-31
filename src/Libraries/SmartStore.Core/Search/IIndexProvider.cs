using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Core.Search
{
	public interface IIndexProvider
	{
		/// <summary>
		/// Enumerates the names of all existing indexes. 
		/// A name is required for the <see cref="GetIndexStore(string)"/> method.
		/// </summary>
		IEnumerable<string> EnumerateIndexes();

		/// <summary>
		/// Creates an empty document
		/// </summary>
		/// <returns>The document instance</returns>
		IIndexDocument CreateDocument(int id);

		/// <summary>
		/// Returns a provider specific implementation of the <see cref="IIndexStore"/> interface
		/// which allows interaction with the underlying index store for managing the index and containing documents.
		/// </summary>
		/// <param name="scope">The index name</param>
		/// <returns>The index store</returns>
		/// <remarks>
		/// This methods always returns an object instance, even if the index does not exist yet.
		/// </remarks>
		IIndexStore GetIndexStore(string scope);

		/// <summary>
		/// Returns a provider specific implementation of the <see cref="ISearchEngine"/> interface
		/// which allows executing queries against an index store.
		/// </summary>
		/// <param name="store">The index store</param>
		/// <param name="query">The query to execute against the store</param>
		/// <returns>The search engine instance</returns>
		ISearchEngine GetSearchEngine(IIndexStore store, SearchQuery query);
	}
}
