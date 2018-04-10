using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using SmartStore.Core.Domain.Localization;

namespace SmartStore.Services.Localization
{
	[Serializable]
	public class LocalizedPropertyString : IHtmlString
	{
		// Regex for all types of brackets which need to be "swapped": ({[]})
		private readonly static Regex _rgBrackets = new Regex(@"\(|\{|\[|\]|\}|\)", RegexOptions.Compiled | RegexOptions.CultureInvariant);

		private readonly string _text;
		private readonly Language _requestLanguage;
		private readonly Language _currentLanguage;

		private string _bidiText;

		public LocalizedPropertyString(string text, Language requestLanguage, Language currentLanguage)
		{
			_text = text;
			_requestLanguage = requestLanguage;
			_currentLanguage = currentLanguage;
		}

		public string Text
		{
			get { return _text; }
		}

		public Language RequestLanguage
		{
			get { return _requestLanguage; }
		}

		public Language CurrenttLanguage
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

		public static implicit operator string(LocalizedPropertyString obj)
		{
			return obj?.Text;
		}

		public override string ToString()
		{
			return _text;
		}

		public string ToHtmlString()
		{
			if (BidiOverride)
			{
				if (_bidiText == null)
				{
					_bidiText = FixBrackets(_currentLanguage.Rtl);
				}
				
				return _bidiText;
			}

			return _text;
		}

		private string FixBrackets(bool rtl)
		{
			var controlChar = rtl ? "&rlm;" : "&lrm;";
			return _rgBrackets.Replace(_text, m => 
			{
				return controlChar + m.Value + controlChar; 
			});
		}
	}
}
