using System;

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
