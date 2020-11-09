using System;

namespace SmartStore.Services.Cms
{
    public enum LinkType
    {
        Product = 0,
        Category,
        Manufacturer,
        Topic,
        BlogPost,
        NewsItem,
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
            return this.Link.EmptyNull();
        }
    }

    [Serializable]
    public partial class LinkResolverData : LinkResolverResult, ICloneable<LinkResolverData>
    {
        public bool SubjectToAcl { get; set; }
        public bool LimitedToStores { get; set; }
        public bool CheckLimitedToStores { get; set; } = true;

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
                    return ("far fa-file-alt", "Common.Entity.Topic");
                case LinkType.BlogPost:
                    return ("fa fa-blog", "Common.Entity.BlogPost");
                case LinkType.NewsItem:
                    return ("far fa-newspaper", "Common.Entity.NewsItem");
                case LinkType.Url:
                    return ("fa fa-link", "Common.Url");
                case LinkType.File:
                    return ("far fa-folder-open", "Common.File");
                default:
                    throw new SmartException("Unknown link builder type.");
            }
        }

        /// <summary>
        /// Creates the full link expression including type, value and query string.
        /// </summary>
        /// <param name="includeQueryString">Whether to include the query string.</param>
        /// <returns>Link expression.</returns>
        public static string CreateExpression(this LinkResolverResult data, bool includeQueryString = true)
        {
            if (data?.Value == null)
            {
                return string.Empty;
            }

            var result = data.Type == LinkType.Url
                ? data.Value.ToString()
                : string.Concat(data.Type.ToString().ToLower(), ":", data.Value.ToString());

            if (includeQueryString && data.Type != LinkType.Url && !string.IsNullOrWhiteSpace(data.QueryString))
            {
                return string.Concat(result, "?", data.QueryString);
            }

            return result;
        }
    }
}
