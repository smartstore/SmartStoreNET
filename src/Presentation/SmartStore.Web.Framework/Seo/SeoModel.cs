using System.Collections.Generic;

namespace SmartStore.Web.Framework.Seo
{
    public partial class SeoModel : ISeoModel
    {
        public SeoModel()
        {
            Locales = new List<SeoModelLocal>();
        }

        public string MetaTitle { get; set; }
        public string MetaDescription { get; set; }
        public string MetaKeywords { get; set; }
        public IList<SeoModelLocal> Locales { get; set; }
    }

    public partial class UrlAliasSeoModel : SeoModel, IUrlAlias
    {
        public string SeName { get; set; }
    } 
}
