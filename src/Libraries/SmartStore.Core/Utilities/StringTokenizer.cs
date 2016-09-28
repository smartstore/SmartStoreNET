using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SmartStore.Utilities
{

    public class StringTokenizer : IEnumerable<string>
    {
        private const string _defaultDelim = " \t\n\r\f";
        private readonly string _delim;
        private readonly string _text;
        private readonly bool _returnDelims;

        public StringTokenizer(string text)
            : this(text, _defaultDelim, true)
        {
        }

        public StringTokenizer(string text, string delim)
            : this(text, delim, true)
        {
        }

        public StringTokenizer(string text, string delim, bool returnDelims)
        {
            Guard.NotEmpty(text, nameof(text));

            _text = text;
            _delim = delim.NullEmpty() ?? _defaultDelim;
            _returnDelims = returnDelims;
        }

        #region IEnumerable[<string>] Members

        private int _pos;
        private string _token;

        private string GetNext()
        {
            if (_pos >= _text.Length)
            {
                return null;
            }

            // Char an aktueller Cursor-Position
            char ch = _text[_pos];


            if (_delim.IndexOf(ch) != -1) // Ist Char ein Delim-Zeichen?...
            {
                // ...ja!
                _pos++;
                if (_returnDelims)
                {
                    return ch.ToString(); // Char zurückgeben, da gewünscht!
                }
                return this.GetNext(); // Nächsten Token beziehen
            }

            // ...nein, kein Delim-Zeichen!

            int length = _text.IndexOfAny(_delim.ToCharArray(), _pos); // den Index des nächsten Delim-Zeichen ermitteln.
            if (length == -1)
            {
                // gibt kein Delim-Zeichen.
                length = _text.Length;
            }

            string str = _text.Substring(_pos, length - _pos);
            _pos = length;
            return str;
        }

        public IEnumerator<string> GetEnumerator()
        {
            _token = GetNext();
            while (_token != null)
            {
                yield return _token;
                _token = GetNext();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion
    }

}
