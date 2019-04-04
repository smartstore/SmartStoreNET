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
		/// The raw expression without query string.
		/// </summary>
		public string Expression { get; set; }

		/// <summary>
		/// The query string part.
		/// </summary>
		public string QueryString { get; set; }

		public LinkType Type { get; set; }
        public object Value { get; set; }

        public LinkStatus Status { get; set; }
        public string Label { get; set; }
        public int Id { get; set; }
        public int? PictureId { get; set; }
        public string Slug { get; set; }

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


    public static class LinkResolverExtensions
    {
        public static (string Icon, string ResKey) GetLinkTypeInfo(this LinkType type)
        {
            switch (type)
            {
                case LinkType.Product:
                    return ("fa fa-cube", "Common.Entity.Product");
                case LinkType.Category:
                    return ("fa fa-sitemap", "Common.Entity.Category");
                case LinkType.Manufacturer:
                    return ("far fa-building", "Common.Entity.Manufacturer");
                case LinkType.Topic:
                    return ("far fa-file", "Common.Entity.Topic");
                case LinkType.Url:
                    return ("fa fa-link", "Common.Url");
                case LinkType.File:
                    return ("far fa-folder-open", "Common.File");
                default:
                    throw new SmartException("Unknown link builder type.");
            }
        }
    }
}
