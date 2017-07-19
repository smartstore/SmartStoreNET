using System;
using System.Globalization;
using SmartStore.Utilities;

namespace SmartStore.Core.Search.Facets
{
	[Serializable]
	public class FacetValue : IEquatable<FacetValue>, ICloneable<FacetValue>
	{
		public FacetValue()
		{
		}

		public FacetValue(object value, IndexTypeCode typeCode)
		{
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

		public object Value
		{
			get;
			set;
		}

		public object UpperValue
		{
			get;
			set;
		}

		public IndexTypeCode TypeCode
		{
			get;
			set;
		}

		public bool IncludesLower
		{
			get;
			set;
		}

		public bool IncludesUpper
		{
			get;
			set;
		}

		public bool IsRange 
		{
			get;
			set;
		}

		public bool IsSelected
		{
			get;
			set;
		}

		public bool IsEmpty
		{
			get
			{
				return TypeCode == IndexTypeCode.Empty && Value == null;
			}
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
			{
				return false;
			}

			if (other.IsRange)
			{
				if (other.IncludesLower != IncludesLower || other.IncludesUpper != IncludesUpper)
				{
					return false;
				}

				if (other.Value == null && Value == null && other.UpperValue == null && UpperValue == null)
				{
					return true;
				}

				if (other.UpperValue != null && !other.UpperValue.Equals(UpperValue))
				{
					return false;
				}
			}

			if (other.Value == null && Value == null)
			{
				return true;
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

			var valueStr = Value != null
				? Convert.ToString(Value, CultureInfo.InvariantCulture)
				: string.Empty;

			if (IsRange)
			{
				var upperValueStr = UpperValue != null
					? Convert.ToString(UpperValue, CultureInfo.InvariantCulture)
					: string.Empty;

				if (upperValueStr.HasValue())
				{
					result = string.Concat(valueStr, "~", upperValueStr);
				}
				else
				{
					result = valueStr;
				}
			}
			else
			{
				result = valueStr;
			}

			return result;
		}

		public FacetValue Clone()
		{
			return (FacetValue)this.MemberwiseClone();
		}

		object ICloneable.Clone()
		{
			return this.MemberwiseClone();
		}
	}
}
