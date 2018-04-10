using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using SmartStore.Core.Domain.Localization;

namespace SmartStore.Services.Localization
{
	[Serializable]
	public class LocalizedPropertyValue<T> : IHtmlString
	{
		// Regex for all types of brackets which need to be "swapped": ({[]})
		private readonly static Regex _rgBrackets = new Regex(@"\(|\{|\[|\]|\}|\)", RegexOptions.Compiled | RegexOptions.CultureInvariant);

		private readonly T _value;
		private readonly Language _requestLanguage;
		private readonly Language _currentLanguage;

		private string _bidiStr;

		public LocalizedPropertyValue(T text, Language requestLanguage, Language currentLanguage)
		{
			_value = text;
			_requestLanguage = requestLanguage;
			_currentLanguage = currentLanguage;
		}

		public T Value
		{
			get { return _value; }
		}

		public Language RequestLanguage
		{
			get { return _requestLanguage; }
		}

		public Language CurrentLanguage
		{
			get { return _currentLanguage; }
		}

		public bool IsFallback
		{
			get { return _requestLanguage != _currentLanguage; }
		}

		public bool BidiOverride
		{
			get { return _requestLanguage != _currentLanguage && _requestLanguage.Rtl != _currentLanguage.Rtl; }
		}

		public static implicit operator T(LocalizedPropertyValue<T> obj)
		{
			return obj.Value;
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

		public string ToHtmlString()
		{
			var str = ToString();

			if (BidiOverride)
			{
				if (_bidiStr == null)
				{
					_bidiStr = FixBrackets(str, _currentLanguage.Rtl);
				}
				
				return _bidiStr;
			}

			return str;
		}

		private string FixBrackets(string str, bool rtl)
		{
			var controlChar = rtl ? "&rlm;" : "&lrm;";
			return _rgBrackets.Replace(str, m => 
			{
				return controlChar + m.Value + controlChar; 
			});
		}

		public override int GetHashCode()
		{
			var hashCode = 0;
			if (_value != null)
				hashCode ^= _value.GetHashCode();
			return hashCode;
		}

		public override bool Equals(object obj)
		{
			if (obj == null || obj.GetType() != GetType())
				return false;

			var that = (LocalizedPropertyValue<T>)obj;
			return string.Equals(_value, that._value);
		}
	}
}
