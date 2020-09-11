using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Newtonsoft.Json;
using SmartStore.Core.Domain.Localization;

namespace SmartStore.Services.Localization
{
    public class LocalizedValue
    {
        // Regex for all types of brackets which need to be "swapped": ({[]})
        private readonly static Regex _rgBrackets = new Regex(@"\(|\{|\[|\]|\}|\)", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        /// <summary>
        /// Fixes the flow of brackets within a text if the current page language has RTL flow.
        /// </summary>
        /// <param name="str">The test to fix.</param>
        /// <param name="currentLanguage">Current language</param>
        /// <returns></returns>
        public static string FixBrackets(string str, Language currentLanguage)
        {
            if (!currentLanguage.Rtl || str.IsEmpty())
            {
                return str;
            }

            var controlChar = "&lrm;";
            return _rgBrackets.Replace(str, m =>
            {
                return controlChar + m.Value + controlChar;
            });
        }
    }

    [Serializable]
    public class LocalizedValue<T> : IHtmlString, IEquatable<LocalizedValue<T>>, IComparable, IComparable<LocalizedValue<T>>
    {
        private T _value;
        private readonly Language _requestLanguage;
        private readonly Language _currentLanguage;

        public LocalizedValue(T value, Language requestLanguage, Language currentLanguage)
        {
            _value = value;
            _requestLanguage = requestLanguage;
            _currentLanguage = currentLanguage;
        }

        public T Value => _value;

        [JsonIgnore]
        public Language RequestLanguage => _requestLanguage;

        [JsonIgnore]
        public Language CurrentLanguage => _currentLanguage;

        public bool IsFallback => _requestLanguage != _currentLanguage;

        public bool BidiOverride => _requestLanguage != _currentLanguage && _requestLanguage.Rtl != _currentLanguage.Rtl;

        public void ChangeValue(T value)
        {
            _value = value;
        }

        public static implicit operator T(LocalizedValue<T> obj)
        {
            if (obj == null)
            {
                return default;
            }

            return obj.Value;
        }

        public string ToHtmlString()
        {
            return this.ToString();
        }

        public override string ToString()
        {
            if (_value == null)
            {
                return null;
            }

            if (typeof(T) == typeof(string))
            {
                return _value as string;
            }

            return _value.Convert<string>(CultureInfo.GetCultureInfo(_currentLanguage.LanguageCulture));
        }

        public override int GetHashCode()
        {
            var hashCode = 0;
            if (_value != null)
                hashCode ^= _value.GetHashCode();
            return hashCode;
        }

        public override bool Equals(object other)
        {
            return this.Equals(other as LocalizedValue<T>);
        }

        public bool Equals(LocalizedValue<T> other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;

            return object.Equals(_value, other._value);
        }

        public int CompareTo(object other)
        {
            return CompareTo(other as LocalizedValue<T>);
        }

        public int CompareTo(LocalizedValue<T> other)
        {
            if (other == null)
            {
                return 1;
            }

            if (Value is IComparable<T> val1)
            {
                return val1.CompareTo(other.Value);
            }

            if (Value is IComparable val2)
            {
                return val2.CompareTo(other.Value);
            }

            return 0;
        }
    }
}
