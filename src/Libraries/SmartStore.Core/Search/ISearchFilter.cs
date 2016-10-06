using System;

namespace SmartStore.Core.Search
{
	public interface ISearchFilter
	{
		SearchFilterOccurence Occurence { get; }
		float Boost { get; }
	}

	public interface ICompositeSearchFilter : ISearchFilter
	{
		ISearchFilter[] Filters { get; }
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
