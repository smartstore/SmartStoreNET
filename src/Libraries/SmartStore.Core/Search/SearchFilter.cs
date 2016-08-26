using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Core.Search
{
	public class SearchFilter
	{
		protected SearchFilter()
		{
			Occurence = SearchFilterOccurence.Should;
		}

		public string FieldName
		{
			get;
			protected set;
		}

		public IndexTypeCode TypeCode
		{
			get;
			protected set;
		}

		public object Term
		{
			get;
			protected set;
		}

		public object UpperTerm
		{
			get;
			protected set;
		}

		public bool IsRangeFilter
		{
			get;
			protected set;
		}

		public bool IncludesLower
		{
			get;
			protected set;
		}

		public bool IncludesUpper
		{
			get;
			protected set;
		}

		public SearchFilterOccurence Occurence
		{
			get;
			protected set;
		}

		public bool IsExactMatch
		{
			get;
			protected set;
		}

		public bool IsNotAnalyzed
		{
			get;
			protected set;
		}

		public float Boost
		{
			get;
			protected set;
		}

		#region Fluent builder

		/// <summary>
		/// Mark a clause as a mandatory match. By default all clauses are optional.
		/// </summary>
		public SearchFilter Mandatory()
		{
			Occurence = SearchFilterOccurence.Must;
			return this;
		}

		/// <summary>
		/// Mark a clause as a forbidden match (MustNot).
		/// </summary>
		public SearchFilter Forbidden()
		{
			Occurence = SearchFilterOccurence.MustNot;
			return this;
		}

		/// <summary>
		/// Specifies whether the clause should be matched exactly, like 'app' won't match 'apple' (applied on string clauses only).
		/// </summary>
		public SearchFilter ExactMatch()
		{
			IsExactMatch = true;
			return this;
		}

		/// <summary>
		/// Specifies that the searched value will not be tokenized (applied on string clauses only)
		/// </summary>
		public SearchFilter NotAnalyzed()
		{
			IsNotAnalyzed = true;
			return this;
		}

		/// <summary>
		/// Applies a specific boost to a clause.
		/// </summary>
		/// <param name="weight">A value greater than zero, by which the score will be multiplied. 
		/// If greater than 1, it will improve the weight of a clause</param>
		public SearchFilter Weighted(float weight)
		{
			Boost = weight;
			return this;
		}

		#endregion

		#region Static factories

		public static SearchFilter ByField(string fieldName, string term)
		{
			return ByField(fieldName, term, IndexTypeCode.String);
		}

		public static SearchFilter ByField(string fieldName, int term)
		{
			return ByField(fieldName, term, IndexTypeCode.String);
		}

		public static SearchFilter ByField(string fieldName, bool term)
		{
			return ByField(fieldName, term, IndexTypeCode.String);
		}

		public static SearchFilter ByField(string fieldName, double term)
		{
			return ByField(fieldName, term, IndexTypeCode.String);
		}

		public static SearchFilter ByField(string fieldName, DateTime term)
		{
			return ByField(fieldName, term, IndexTypeCode.String);
		}

		private static SearchFilter ByField(string fieldName, object term, IndexTypeCode typeCode)
		{
			Guard.NotEmpty(fieldName, nameof(fieldName));

			return new SearchFilter
			{
				FieldName = fieldName,
				Term = term,
				TypeCode = typeCode
			};
		}


		public static SearchFilter ByRange(string fieldName, string lower, string upper, bool includeLower = true, bool includeUpper = true)
		{
			return ByRange(fieldName, lower, upper, IndexTypeCode.String, includeLower, includeUpper);
		}

		public static SearchFilter ByRange(string fieldName, int? lower, int? upper, bool includeLower = true, bool includeUpper = true)
		{
			return ByRange(fieldName, lower, upper, IndexTypeCode.Int32, includeLower, includeUpper);
		}

		public static SearchFilter ByRange(string fieldName, double? lower, double? upper, bool includeLower = true, bool includeUpper = true)
		{
			return ByRange(fieldName, lower, upper, IndexTypeCode.Double, includeLower, includeUpper);
		}

		public static SearchFilter ByRange(string fieldName, DateTime? lower, DateTime? upper, bool includeLower = true, bool includeUpper = true)
		{
			return ByRange(fieldName, lower, upper, IndexTypeCode.Double, includeLower, includeUpper);
		}

		private static SearchFilter ByRange(
			string fieldName, 
			object lowerTerm, 
			object upperTerm, 
			IndexTypeCode typeCode, 
			bool includeLower, 
			bool includeUpper)
		{
			Guard.NotEmpty(fieldName, nameof(fieldName));

			return new SearchFilter
			{
				FieldName = fieldName,
				Term = lowerTerm,
				UpperTerm = upperTerm,
				TypeCode = typeCode,
				IncludesLower = includeLower,
				IncludesUpper = includeUpper,
				IsRangeFilter = true
			};
		}

		#endregion
	}

	public enum SearchFilterOccurence
	{
		Must = 0,
		Should = 1,
		MustNot = 2
	}
}
