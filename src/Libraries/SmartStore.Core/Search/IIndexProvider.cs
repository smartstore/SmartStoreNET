using System.Collections.Generic;

namespace SmartStore.Core.Search
{
    public interface IIndexProvider
    {
        /// <summary>
        /// Gets a value indicating whether the search index given by scope is active
        /// </summary>
        bool IsActive(string scope);

        /// <summary>
        /// Enumerates the names of all EXISTING indexes. 
        /// A name is required for the <see cref="GetIndexStore(string)"/> method.
        /// </summary>
        IEnumerable<string> EnumerateIndexes();

        /// <summary>
        /// Creates an empty document
        /// </summary>
        /// <param name="id">The primary key of the indexed entity</param>
        /// <param name="documentType">Identifies the type of a document, can be <c>null</c></param>
        /// <returns>The document instance</returns>
        IIndexDocument CreateDocument(int id, SearchDocumentType? documentType);

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
        ISearchEngine GetSearchEngine(IIndexStore store, ISearchQuery query);
    }
}
