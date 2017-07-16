using System;

namespace SmartStore.Core.Search
{
	/// <summary>
	/// A simple factory for registered search index providers
	/// </summary>
	public interface IIndexManager
	{
		/// <summary>
		/// Whether at least one provider is available, which implements <see cref="IIndexProvider"/>
		/// </summary>
		/// <param name="activeOnly">Whether only active providers should be queried for</param>
		/// <returns><c>true</c> if at least one provider is registered, <c>false</c> ortherwise</returns>
		/// <remarks>Primarily used to skip indexing processes</remarks>
		bool HasAnyProvider(bool activeOnly = true);

		/// <summary>
		/// Returns the instance of the first registered index provider (e.g. a Lucene provider)
		/// </summary>
		/// <param name="activeOnly">Whether only active providers should be queried for</param>
		/// <returns>The index provider implementation instance</returns>
		IIndexProvider GetIndexProvider(bool activeOnly = true);
	}
}
