using System.Text;
using System.Web;
using System.Collections.Specialized;

namespace SmartStore.Collections
{
    /// <summary>
    /// http://weblogs.asp.net/bradvincent/archive/2008/10/27/helper-class-querystring-builder-chainable.aspx
    /// </summary>
    public class QueryString : NameValueCollection
    {
        public QueryString()
        {
        }

        public QueryString(string queryString)
        {
            FillFromString(queryString);
        }

        public static QueryString Current
        {
            get { return new QueryString().FromCurrent(); }
        }

        /// <summary>
        /// extracts a querystring from a full URL
        /// </summary>
        /// <param name="s">the string to extract the querystring from</param>
        /// <returns>a string representing only the querystring</returns>
        public static string ExtractQuerystring(string s)
        {
            if (!string.IsNullOrEmpty(s))
            {
                if (s.Contains("?"))
                {
                    return s.Substring(s.IndexOf("?") + 1);
                }
            }
            return s;
        }

        /// <summary>
        /// returns a querystring object based on a string
        /// </summary>
        /// <param name="s">the string to parse</param>
        /// <returns>the QueryString object </returns>
        public QueryString FillFromString(string s)
        {
            base.Clear();
            if (string.IsNullOrEmpty(s))
            {
                return this;
            }
            foreach (string keyValuePair in ExtractQuerystring(s).Split('&'))
            {
                if (string.IsNullOrEmpty(keyValuePair))
                {
                    continue;
                }
                string[] split = keyValuePair.Split('=');
                base.Add(split[0],
                         split.Length == 2 ? split[1] : "");
            }
            return this;
        }

        /// <summary>
        /// returns a QueryString object based on the current querystring of the request
        /// </summary>
        /// <returns>the QueryString object </returns>
        public QueryString FromCurrent()
        {
            if (HttpContext.Current != null)
            {
                return FillFromString(HttpContext.Current.Request.QueryString.ToString());
            }
            base.Clear();
            return this;
        }

        /// <summary>
        /// add a name value pair to the collection
        /// </summary>
        /// <param name="name">the name</param>
        /// <param name="value">the value associated to the name</param>
        /// <returns>the QueryString object </returns>
        public new QueryString Add(string name, string value)
        {
            return Add(name, value, false);
        }

        /// <summary>
        /// adds a name value pair to the collection
        /// </summary>
        /// <param name="name">the name</param>
        /// <param name="value">the value associated to the name</param>
        /// <param name="isUnique">true if the name is unique within the querystring. This allows us to override existing values</param>
        /// <returns>the QueryString object </returns>
        public QueryString Add(string name, string value, bool isUnique)
        {
            string existingValue = base[name];
            if (string.IsNullOrEmpty(existingValue))
            {
                base.Add(name, HttpUtility.UrlEncode(value));
            }
            else if (isUnique)
            {
                base[name] = HttpUtility.UrlEncode(value);
            }
            else
            {
                base[name] += "," + HttpUtility.UrlEncode(value);
            }
            return this;
        }

        /// <summary>
        /// removes a name value pair from the querystring collection
        /// </summary>
        /// <param name="name">name of the querystring value to remove</param>
        /// <returns>the QueryString object</returns>
        public new QueryString Remove(string name)
        {
            string existingValue = base[name];
            if (!string.IsNullOrEmpty(existingValue))
            {
                base.Remove(name);
            }
            return this;
        }

        /// <summary>
        /// clears the collection
        /// </summary>
        /// <returns>the QueryString object </returns>
        public QueryString Reset()
        {
            base.Clear();
            return this;
        }

        /// <summary>
        /// overrides the default
        /// </summary>
        /// <param name="name"></param>
        /// <returns>the associated decoded value for the specified name</returns>
        public new string this[string name]
        {
            get { return HttpUtility.UrlDecode(base[name]); }
        }

        /// <summary>
        /// overrides the default indexer
        /// </summary>
        /// <param name="index"></param>
        /// <returns>the associated decoded value for the specified index</returns>
        public new string this[int index]
        {
            get { return HttpUtility.UrlDecode(base[index]); }
        }

        /// <summary>
        /// checks if a name already exists within the query string collection
        /// </summary>
        /// <param name="name">the name to check</param>
        /// <returns>a boolean if the name exists</returns>
        public bool Contains(string name)
        {
            string existingValue = base[name];
            return !string.IsNullOrEmpty(existingValue);
        }

        /// <summary>
        /// outputs the querystring object to a string
        /// </summary>
        /// <returns>the encoded querystring as it would appear in a browser</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();
            for (var i = 0; i < base.Keys.Count; i++)
            {
                if (!string.IsNullOrEmpty(base.Keys[i]))
                {
                    foreach (string val in base[base.Keys[i]].Split(','))
                    {
                        builder.Append((builder.Length == 0) ? "?" : "&").Append(
                            HttpUtility.UrlEncode(base.Keys[i])).Append("=").Append(val);
                    }
                }
            }
            return builder.ToString();
        }
    }
}
