using System;

namespace SmartStore.Core.Search
{
	public abstract class SearchFilterBase : ISearchFilter
	{
		protected SearchFilterBase()
		{
			Occurence = SearchFilterOccurence.Should;
		}

		public float Boost
		{
			get;
			protected internal set;
		}

		public SearchFilterOccurence Occurence
		{
			get;
			protected internal set;
		}

		#region Fluent builder

		// [...]

		#endregion
	}
}
