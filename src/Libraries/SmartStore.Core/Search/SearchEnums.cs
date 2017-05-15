namespace SmartStore.Core.Search
{
	public enum SearchMode
	{
		/// <summary>
		/// Term search
		/// </summary>
		ExactMatch = 0,
		
		/// <summary>
		/// Prefix term search
		/// </summary>
		StartsWith,

		/// <summary>
		/// Wildcard search
		/// </summary>
		Contains
	}
}
