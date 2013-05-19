using System.Web.Routing;
using SmartStore.Web.Framework.Mvc;

namespace SmartStore.Admin.Models.Plugins
{
    public class MiscPluginModel : ModelBase
    {
        public string FriendlyName { get; set; }

        public string ConfigurationActionName { get; set; }
        public string ConfigurationControllerName { get; set; }
        public RouteValueDictionary ConfigurationRouteValues { get; set; }
    }
}