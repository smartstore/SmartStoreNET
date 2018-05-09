﻿using System;
using System.Web;

namespace SmartStore.Core.Localization
{
	[Serializable]
	public class LocalizedString : IHtmlString
    {
        private readonly string _localized;
        private readonly string _textHint;
        private readonly object[] _args;

        public LocalizedString(string localized)
        {
            _localized = localized;
        }

        public LocalizedString(string localized, string textHint, object[] args)
        {
            _localized = localized;
            _textHint = textHint;
            _args = args;
        }

        public static LocalizedString TextOrDefault(string text, LocalizedString defaultValue)
        {
            if (string.IsNullOrEmpty(text))
                return defaultValue;
            return new LocalizedString(text);
        }

        public string TextHint
        {
            get { return _textHint; }
        }

        public object[] Args
        {
            get { return _args; }
        }

        public string Text
        {
            get { return _localized; }
        }

        public static implicit operator string(LocalizedString obj)
        {
            return obj.Text;
        }

		public static implicit operator LocalizedString(string obj)
		{
			return new LocalizedString(obj);
		}

        public override string ToString()
        {
            return _localized;
        }

		public string ToHtmlString()
        {
            return _localized;
        }

        public override int GetHashCode()
        {
            var hashCode = 0;
            if (_localized != null)
                hashCode ^= _localized.GetHashCode();
            return hashCode;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
                return false;

            var that = (LocalizedString)obj;
            return string.Equals(_localized, that._localized);
        }
    }
}
