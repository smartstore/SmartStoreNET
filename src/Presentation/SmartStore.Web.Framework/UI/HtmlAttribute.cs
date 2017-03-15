/*
 * Source: http://stackoverflow.com/questions/3800473/how-to-concisely-create-optional-html-attributes-with-razor-view-engine/4232630#4232630
*/
using System;
using System.Web;

namespace SmartStore.Web.Framework
{
    public class HtmlAttribute : IHtmlString
    {
        private string _internalValue = String.Empty;
        private string _seperator;

        public string Name { get; set; }
        public string Value { get; set; }
        public bool Condition { get; set; }

        public HtmlAttribute(string name)
            : this(name, null)
        {
        }

        public HtmlAttribute(string name, string separator)
        {
            Name = name;
            _seperator = separator ?? " ";
        }

        public HtmlAttribute Add(string value)
        {
            return Add(value, true);
        }

        public HtmlAttribute Add(string value, bool condition)
        {
            if (!String.IsNullOrWhiteSpace(value) && condition)
                _internalValue += value + _seperator;

            return this;
        }

        public override string ToString()
        {
            if (!String.IsNullOrWhiteSpace(_internalValue))
                _internalValue = String.Format("{0}=\"{1}\"", Name, _internalValue.Substring(0, _internalValue.Length - _seperator.Length));

            return _internalValue;
        }

        public string ToHtmlString()
        {
            return this.ToString();
        }
    }
}
