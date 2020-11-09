using System;

namespace SmartStore.Core.Search
{
    public class SearchFilter : SearchFilterBase, IAttributeSearchFilter
    {
        protected SearchFilter()
            : base()
        {
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

        public int ParentId
        {
            get;
            protected set;
        }

        #region Fluent builder

        /// <summary>
        /// Mark a clause as a mandatory match. By default all clauses are optional.
        /// </summary>
        /// <param name="mandatory">Whether the clause is mandatory or not.</param>
        public SearchFilter Mandatory(bool mandatory = true)
        {
            Occurence = mandatory ? SearchFilterOccurence.Must : SearchFilterOccurence.MustNot;
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
        /// Applies a parent identifier.
        /// </summary>
        /// <param name="parentId">Parent identifier</param>
        public SearchFilter HasParent(int parentId)
        {
            ParentId = parentId;
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

        public static CombinedSearchFilter Combined(params ISearchFilter[] filters)
        {
            var filter = new CombinedSearchFilter(filters);
            filter.Occurence = SearchFilterOccurence.Must;
            return filter;
        }

        public static SearchFilter ByField(string fieldName, string term)
        {
            return ByField(fieldName, term, IndexTypeCode.String);
        }

        public static SearchFilter ByField(string fieldName, int term)
        {
            return ByField(fieldName, term, IndexTypeCode.Int32);
        }

        public static SearchFilter ByField(string fieldName, bool term)
        {
            return ByField(fieldName, term, IndexTypeCode.Boolean);
        }

        public static SearchFilter ByField(string fieldName, double term)
        {
            return ByField(fieldName, term, IndexTypeCode.Double);
        }

        public static SearchFilter ByField(string fieldName, DateTime term)
        {
            return ByField(fieldName, term, IndexTypeCode.DateTime);
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


        public static RangeSearchFilter ByRange(string fieldName, string lower, string upper, bool includeLower = true, bool includeUpper = true)
        {
            return ByRange(fieldName, lower, upper, IndexTypeCode.String, includeLower, includeUpper);
        }

        public static RangeSearchFilter ByRange(string fieldName, int? lower, int? upper, bool includeLower = true, bool includeUpper = true)
        {
            return ByRange(fieldName, lower, upper, IndexTypeCode.Int32, includeLower, includeUpper);
        }

        public static RangeSearchFilter ByRange(string fieldName, double? lower, double? upper, bool includeLower = true, bool includeUpper = true)
        {
            return ByRange(fieldName, lower, upper, IndexTypeCode.Double, includeLower, includeUpper);
        }

        public static RangeSearchFilter ByRange(string fieldName, DateTime? lower, DateTime? upper, bool includeLower = true, bool includeUpper = true)
        {
            return ByRange(fieldName, lower, upper, IndexTypeCode.DateTime, includeLower, includeUpper);
        }

        private static RangeSearchFilter ByRange(
            string fieldName,
            object lowerTerm,
            object upperTerm,
            IndexTypeCode typeCode,
            bool includeLower,
            bool includeUpper)
        {
            Guard.NotEmpty(fieldName, nameof(fieldName));

            return new RangeSearchFilter
            {
                FieldName = fieldName,
                Term = lowerTerm,
                UpperTerm = upperTerm,
                TypeCode = typeCode,
                IncludesLower = includeLower,
                IncludesUpper = includeUpper
            };
        }

        #endregion

        public override string ToString()
        {
            return $"{FieldName}: {Term.ToString()}";
        }
    }

    public enum SearchFilterOccurence
    {
        Must = 0,
        Should = 1,
        MustNot = 2
    }
}
