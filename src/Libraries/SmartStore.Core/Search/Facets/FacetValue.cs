using System;

namespace SmartStore.Core.Search.Facets
{
	[Serializable]
	public class FacetValue : IEquatable<FacetValue>
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
			Guard.NotNull(value, nameof(value));

			Value = value;
			TypeCode = typeCode;
			IsRange = false;
		}

		public FacetValue(object value, object upperValue, IndexTypeCode typeCode, bool includesLower, bool includesUpper)
		{
			Value = value;
			UpperValue = upperValue;
			TypeCode = typeCode;
			IncludesLower = includesLower;
			IncludesUpper = includesUpper;
			IsRange = true;
		}

		public FacetValue(FacetValue value)
		{
			Guard.NotNull(value, nameof(value));

			Value = value.Value;
			UpperValue = value.UpperValue;
			TypeCode = value.TypeCode;
			IncludesLower = value.IncludesLower;
			IncludesUpper = value.IncludesUpper;
			IsRange = value.IsRange;
			IsSelected = value.IsSelected;
		}

		public object Value
		{
			get;
			private set;
		}

		public object UpperValue
		{
			get;
			private set;
		}

		public IndexTypeCode TypeCode
		{
			get;
			private set;
		}

		public bool IncludesLower
		{
			get;
			private set;
		}

		public bool IncludesUpper
		{
			get;
			private set;
		}

		public bool IsRange
		{
			get;
			private set;
		}

		public bool IsSelected
		{
			get;
			set;
		}

		public override int GetHashCode()
		{
			return Value.GetHashCode();
		}

		public bool Equals(FacetValue other)
		{
			if (other == null || other.TypeCode != TypeCode || other.IsRange != IsRange)
				return false;

			if (other.IsRange)
			{
				if (other.IncludesLower != IncludesLower || other.IncludesUpper != IncludesUpper)
					return false;

				if (other.IncludesLower && other.IncludesUpper)
				{
					return (other.Value != null && other.Value.Equals(Value) &&
						other.UpperValue != null && other.UpperValue.Equals(UpperValue));
				}
				else if (other.IncludesUpper)
				{
					return other.UpperValue != null && other.UpperValue.Equals(UpperValue);
				}
			}

			return other.Value != null && other.Value.Equals(Value);
		}

		public override bool Equals(object obj)
		{
			return this.Equals(obj as FacetValue);
		}

		public override string ToString()
		{
			return this.GetStringValue();
		}
	}
}
