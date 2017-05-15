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
		/// <returns></returns>
		/// <remarks>Primarily used to skip indexing processes</remarks>
		bool HasAnyProvider();

		/// <summary>
		/// Returns the instance of the first registered index provider (e.g. a Lucene provider)
		/// </summary>
		/// <returns></returns>
		IIndexProvider GetIndexProvider();
	}
}
