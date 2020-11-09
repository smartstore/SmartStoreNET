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

        public bool IsEmpty => TypeCode == IndexTypeCode.Empty && Value == null;

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
                    .Add(Value)
                    .Add(UpperValue);

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

        protected virtual string ConvertToString(object value)
        {
            if (value != null)
            {
                if (TypeCode == IndexTypeCode.DateTime)
                {
                    // The default conversion is not pretty enough.
                    var dt = (DateTime)value;
                    if (dt.Hour == 0 && dt.Minute == 0 && dt.Second == 0)
                    {
                        return dt.ToString("yyyy-MM-dd");
                    }
                    else
                    {
                        return dt.ToString("yyyy-MM-dd HH:mm:ss");
                    }
                }

                return Convert.ToString(value, CultureInfo.InvariantCulture);
            }

            return string.Empty;
        }

        public override string ToString()
        {
            var result = string.Empty;
            var valueStr = ConvertToString(Value);

            if (IsRange)
            {
                var upperValueStr = ConvertToString(UpperValue);
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
