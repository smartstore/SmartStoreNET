using System;
using SmartStore.Utilities;

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
			Label = value.Label;
			ParentId = value.ParentId;
			DisplayOrder = value.DisplayOrder;
			Sorting = value.Sorting;
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

		#region Metadata

		public string Label { get; set; }

		public int ParentId { get; set; }

		public int DisplayOrder { get; set; }

		public FacetSorting? Sorting { get; set; }

		public string PictureUrl { get; set; }

		public string Color { get; set; }

		#endregion

		public override int GetHashCode()
		{
			if (Value != null && UpperValue != null)
			{
				var combiner = HashCodeCombiner
					.Start()
					.Add(Value.GetHashCode())
					.Add(UpperValue.GetHashCode());

				return combiner.CombinedHash;
			}
			else if (UpperValue != null)
			{
				return UpperValue.GetHashCode();
			}
			else if (Value != null)
			{
				return Value.GetHashCode();
			}

			return 0;
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
			var result = string.Empty;
			var valueString = Value != null ? Value.ToString().EmptyNull() : string.Empty;

			if (IsRange)
			{
				var upperValueString = UpperValue != null ? UpperValue.ToString().EmptyNull() : string.Empty;

				if (IncludesLower && IncludesUpper)
				{
					result = $"[{valueString} - {upperValueString}]";
				}
				else if (IncludesUpper)
				{
					result = upperValueString;
				}
				else
				{
					result = valueString;
				}
			}
			else
			{
				result = valueString;
			}

			return result;
		}
	}
}
