using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Themes
{
    public class ConfigureThemeModel : ModelBase
    {
        public string ThemeName { get; set; }

        public string ConfigurationActionName { get; set; }
        public string ConfigurationControllerName { get; set; }
        public RouteValueDictionary ConfigurationRouteValues { get; set; }

        [SmartResourceDisplayName("Admin.Common.Store")]
        public int StoreId { get; set; }
        public IList<SelectListItem> AvailableStores { get; set; }
    }
}