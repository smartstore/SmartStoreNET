using System;
using System.Web.Mvc;
using SmartStore.Web.Framework.Localization;

namespace SmartStore.Web.Framework.Seo
{
    public interface IUrlAlias 
    {
        [SmartResourceDisplayName("Admin.Configuration.Seo.SeName")]
        [AllowHtml]
        string SeName { get; set; }
    }
}
