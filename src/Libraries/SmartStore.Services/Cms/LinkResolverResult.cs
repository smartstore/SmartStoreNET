using System;
using System.Web;

namespace SmartStore.Services.Cms
{
    public enum LinkType
    {
        Product = 0,
        Category,
        Manufacturer,
        Topic,
        Url = 20,
        File = 30
    }

    public enum LinkStatus
    {
        Ok,
		Forbidden,
		NotFound,
        Hidden
    }

    public class LinkResolverResult
    {
        private string _link;

		/// <summary>
		/// The raw expression without query string
		/// </summary>
		public string Expression { get; set; }

		/// <summary>
		/// The query string part
		/// </summary>
		public string QueryString { get; set; }

		public LinkType Type { get; set; }
        public object Value { get; set; }
		public string Slug { get; set; }

        public LinkStatus Status { get; set; }
        public string Label { get; set; }
        public int Id { get; set; }

        public string Link
        {
            get
            {
                if (Type != LinkType.Url && !string.IsNullOrWhiteSpace(_link) && !string.IsNullOrWhiteSpace(QueryString))
                {
                    return string.Concat(_link, "?", QueryString);
                }

                return _link;
            }
            set
            {
                _link = value;

                if (_link != null && Type != LinkType.Url)
                {
                    var index = _link.IndexOf('?');
                    if (index != -1)
                    {
                        QueryString = _link.Substring(index + 1);
                        _link = _link.Substring(0, index);
                    }
                }
            }
        }

        /// <summary>
        /// Creates the full link expression including type, value and query string.
        /// </summary>
        /// <param name="includeQueryString">Whether to include the query string.</param>
        /// <returns>Link expression.</returns>
        public string GetExpression(bool includeQueryString = true)
        {
            if (Value == null)
            {
                return string.Empty;
            }

            var result = Type == LinkType.Url
                ? Value.ToString()
                : string.Concat(Type.ToString().ToLower(), ":", Value.ToString());

            if (includeQueryString && Type != LinkType.Url && !string.IsNullOrWhiteSpace(QueryString))
            {
                return string.Concat(result, "?", QueryString);
            }

            return result;
        }

		public override string ToString()
		{
			return this.Link;
		}
	}

    [Serializable]
    public partial class LinkResolverData : LinkResolverResult, ICloneable<LinkResolverData>
    {
        public bool SubjectToAcl { get; set; }
        public bool LimitedToStores { get; set; }

		public LinkResolverData Clone()
		{
			return (LinkResolverData)this.MemberwiseClone();
		}

		object ICloneable.Clone()
		{
			return this.MemberwiseClone();
		}
	}
}
