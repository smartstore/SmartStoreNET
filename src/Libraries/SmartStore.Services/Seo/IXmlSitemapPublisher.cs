using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Seo;

namespace SmartStore.Services.Seo
{
    public partial interface IXmlSitemapPublisher
    {
        XmlSitemapProvider PublishXmlSitemap(XmlSitemapBuildContext context);
    }

    public class XmlSitemapProvider
    {
        public virtual int GetTotalCount()
        {
            return 0;
        }

        public virtual IEnumerable<NamedEntity> Enlist()
        {
            return Enumerable.Empty<NamedEntity>();
        }

        public virtual IEnumerable<XmlSitemapNode> EnlistNodes(Language language)
        {
            return Enumerable.Empty<XmlSitemapNode>();
        }

        public virtual XmlSitemapNode CreateNode(UrlHelper urlHelper, string baseUrl, NamedEntity entity, UrlRecordCollection slugs, Language language)
        {
            var slug = slugs.GetSlug(language.Id, entity.Id, true);
            var path = urlHelper.RouteUrl(entity.EntityName, new { SeName = slug }).EmptyNull().TrimStart('/');
            var loc = baseUrl + path;

            return new XmlSitemapNode
            {
                LastMod = entity.LastMod,
                Loc = loc
            };
        }

        public virtual int Order { get; }
    }
}
