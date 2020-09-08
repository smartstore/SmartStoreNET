using System;
using SmartStore.Web.Framework.Localization;

namespace SmartStore.Web.Framework.Seo
{
    public interface ISeoModel : ILocalizedModel<SeoModelLocal>
    {
        [SmartResourceDisplayName("Admin.Configuration.Seo.MetaTitle")]
        string MetaTitle { get; }

        [SmartResourceDisplayName("Admin.Configuration.Seo.MetaDescription")]
        string MetaDescription { get; }

        [SmartResourceDisplayName("Admin.Configuration.Seo.MetaKeywords")]
        string MetaKeywords { get; }
    }

    public class SeoModelLocal : ILocalizedModelLocal
    {
        public int LanguageId { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Seo.MetaTitle")]
        public string MetaTitle { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Seo.MetaDescription")]
        public string MetaDescription { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Seo.MetaKeywords")]
        public string MetaKeywords { get; set; }
    }
}
