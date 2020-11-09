using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Web;
using SmartStore.Utilities.ObjectPools;

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

        public QueryString(NameValueCollection queryString)
            : base(queryString)
        {
        }

        public static QueryString Current => new QueryString().FromCurrent();

        public static QueryString CurrentUnvalidated => new QueryString().FromCurrent(true);

        /// <summary>
        /// Extracts a querystring from a full URL
        /// </summary>
        /// <param name="s">the string to extract the querystring from</param>
        /// <returns>a string representing only the querystring</returns>
        [SuppressMessage("ReSharper", "StringIndexOfIsCultureSpecific.1")]
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
        public QueryString FillFromString(string s, bool urlDecode = false)
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
                base.Add(split[0], split.Length == 2 ? (urlDecode ? HttpUtility.UrlDecode(split[1]) : split[1]) : "");
            }

            return this;
        }

        /// <summary>
        /// returns a QueryString object based on the current querystring of the request
        /// </summary>
		/// <param name="unvalidated"><c>true</c> to get values from the unvalidated query string.</param>
        /// <returns>the QueryString object </returns>
        public QueryString FromCurrent(bool unvalidated = false)
        {
            if (HttpContext.Current != null)
            {
                if (unvalidated)
                {
                    return FillFromString(HttpContext.Current.Request.Unvalidated.QueryString.ToString(), true);
                }
                else
                {
                    return FillFromString(HttpContext.Current.Request.QueryString.ToString(), true);
                }
            }
            base.Clear();
            return this;
        }

        /// <summary>
        /// Add a name value pair to the collection
        /// </summary>
        /// <param name="name">the name</param>
        /// <param name="value">the value associated to the name</param>
        /// <returns>the QueryString object </returns>
        public new QueryString Add(string name, string value)
        {
            return Add(name, value, false);
        }

        /// <summary>
        /// Adds a name value pair to the collection
        /// </summary>
        /// <param name="name">the name</param>
        /// <param name="value">the value associated to the name</param>
        /// <param name="isUnique">true if the name is unique within the querystring. This allows us to override existing values</param>
        /// <returns>the QueryString object </returns>
        public virtual QueryString Add(string name, string value, bool isUnique)
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
        /// Removes a name value pair from the querystring collection
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
        /// Overrides the default indexer
        /// </summary>
        /// <param name="name"></param>
        /// <returns>the associated decoded value for the specified name</returns>
        public new string this[string name] => HttpUtility.UrlDecode(base[name]);

        /// <summary>
        /// overrides the default indexer
        /// </summary>
        /// <param name="index"></param>
        /// <returns>the associated decoded value for the specified index</returns>
        public new string this[int index] => HttpUtility.UrlDecode(base[index]);

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
        /// Outputs the querystring object to a string
        /// </summary>
        /// <returns>the encoded querystring as it would appear in a browser</returns>
        public override string ToString()
        {
            return ToString(true);
        }

        /// <summary>
        /// Outputs the querystring object to a string
        /// </summary>
        /// <param name="splitValues">Whether to create entries for each comma-separated value</param>
        /// <returns>the encoded querystring as it would appear in a browser</returns>
        public string ToString(bool splitValues)
        {
            var psb = PooledStringBuilder.Rent();
            var builder = (StringBuilder)psb;

            for (var i = 0; i < base.Keys.Count; i++)
            {
                var key = base.Keys[i];
                var value = base[key];

                if (!string.IsNullOrEmpty(key))
                {
                    builder.Append((builder.Length == 0) ? "?" : "&");

                    if (splitValues)
                    {
                        foreach (string val in value.EmptyNull().Split(','))
                        {
                            builder.Append(HttpUtility.UrlEncode(key)).Append("=").Append(val);
                        }
                    }
                    else
                    {
                        builder.Append(HttpUtility.UrlEncode(key)).Append("=").Append(value);
                    }
                }
            }

            return psb.ToStringAndReturn();
        }
    }
}
