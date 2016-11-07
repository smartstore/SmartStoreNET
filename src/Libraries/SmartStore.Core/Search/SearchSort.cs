namespace SmartStore.Core.Search
{
	public class SearchSort
	{
		private SearchSort(string name, IndexTypeCode typeCode, bool descending)
		{
			FieldName = name;
			TypeCode = typeCode;
			Descending = descending;
		}

		public string FieldName
		{
			get;
			private set;
		}

		/// <summary>
		/// In this context, <see cref="IndexTypeCode.Empty"/> actually means <c>Score</c>
		/// </summary>
		public IndexTypeCode TypeCode
		{
			get;
			private set;
		}

		public bool Descending
		{
			get;
			private set;
		}

		public override string ToString()
		{
			if (FieldName.IsEmpty())
			{
				return "RELEVANCE";
			}
			else
			{
				return "{0} {1}".FormatInvariant(FieldName, Descending ? "DESC" : "ASC");
			}
		}

		public static SearchSort ByRelevance(bool descending = false)
		{
			return new SearchSort(null, IndexTypeCode.Empty, descending);
		}

		public static SearchSort ByStringField(string fieldName, bool descending = false)
		{
			return ByField(fieldName, IndexTypeCode.String, descending);
		}

		public static SearchSort ByIntField(string fieldName, bool descending = false)
		{
			return ByField(fieldName, IndexTypeCode.Int32, descending);
		}

		public static SearchSort ByBooleanField(string fieldName, bool descending = false)
		{
			return ByField(fieldName, IndexTypeCode.Boolean, descending);
		}

		public static SearchSort ByDoubleField(string fieldName, bool descending = false)
		{
			return ByField(fieldName, IndexTypeCode.Double, descending);
		}

		public static SearchSort ByDateTimeField(string fieldName, bool descending = false)
		{
			return ByField(fieldName, IndexTypeCode.DateTime, descending);
		}

		private static SearchSort ByField(string fieldName, IndexTypeCode typeCode, bool descending = false)
		{
			Guard.NotEmpty(fieldName, nameof(fieldName));

			return new SearchSort(fieldName, typeCode, descending);
		}
	}
}
