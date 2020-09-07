using System;
using SmartStore.Web.Framework.Localization;

namespace SmartStore.Web.Framework.Seo
{
    public interface ISeoModel : ILocalizedModel<SeoModelLocal>
    {
        // TODO: (mh) Annotate these props with "SmartResourceDisplayNameAttribute" here, not on impl model level
        string MetaTitle { get; }
        string MetaDescription { get; }
        string MetaKeywords { get; }
    }

    public class SeoModelLocal : ILocalizedModelLocal
    {
        public int LanguageId { get; set; }

        // TODO: (mh) Annotate these props with "SmartResourceDisplayNameAttribute" here, not on impl model level
        public string MetaTitle { get; set; }
        public string MetaDescription { get; set; }
        public string MetaKeywords { get; set; }
    }
}
