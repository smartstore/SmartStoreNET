using System;
using System.Collections.Generic;

namespace SmartStore.Core.Search
{
	public interface ISearchFilter
	{
		SearchFilterOccurence Occurence { get; }
		float Boost { get; }
	}

	public interface ICombinedSearchFilter : ISearchFilter
	{
		IEnumerable<ISearchFilter> Filters { get; }
	}

	public interface ITermSearchFilter : ISearchFilter
	{
		string FieldName { get; }
		IndexTypeCode TypeCode { get; }
		object Term { get; }
		bool IsExactMatch { get; }
		bool IsNotAnalyzed { get; }
	}

	public interface IRangeSearchFilter : ITermSearchFilter
	{
		object UpperTerm { get; }
		bool IncludesLower { get; }
		bool IncludesUpper { get; }
	}
}
