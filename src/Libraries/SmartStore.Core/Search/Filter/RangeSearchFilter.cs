namespace SmartStore.Core.Search
{
	public class RangeSearchFilter : SearchFilter, IRangeSearchFilter
	{
		public object UpperTerm
		{
			get;
			protected internal set;
		}

		public bool IncludesLower
		{
			get;
			protected internal set;
		}

		public bool IncludesUpper
		{
			get;
			protected internal set;
		}

		public override string ToString()
		{
			return "{0}: {1} - {2}".FormatInvariant(
				FieldName,
				Term != null ? Term.ToString() : "*",
				UpperTerm != null ? UpperTerm.ToString() : "*");
		}
	}
}
