using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Services.Seo
{
    public partial interface IXmlSitemapPublisher
    {
        XmlSitemapResult PublishXmlSitemap(XmlSitemapBuildContext context);
    }

    public abstract class XmlSitemapResult
    {
        public abstract int GetTotalCount();
        public abstract IEnumerable<NamedEntity> Enlist();
        public virtual int Order { get; }
    }
}
