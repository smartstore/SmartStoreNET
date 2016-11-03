using System;

namespace SmartStore.Core.Search.Facets
{
	[Serializable]
	public class FacetValue
	{
		public FacetValue(bool value)
			: this(value, IndexTypeCode.Boolean)
		{
		}

		public FacetValue(int value)
			: this(value, IndexTypeCode.Int32)
		{
		}

		public FacetValue(double value)
			: this(value, IndexTypeCode.Double)
		{
		}

		public FacetValue(DateTime value)
			: this(value, IndexTypeCode.DateTime)
		{
		}

		public FacetValue(string value)
			: this(value, IndexTypeCode.String)
		{
		}

		public FacetValue(object value, IndexTypeCode typeCode)
		{
			Value = value;
			TypeCode = typeCode;
		}

		public object Value
		{
			get;
			private set;
		}

		public IndexTypeCode TypeCode
		{
			get;
			private set;
		}
	}
}
