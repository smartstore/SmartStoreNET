using System.Web.Routing;
using SmartStore.Web.Framework.Mvc;	
 
namespace SmartStore.Admin.Models.Cms	
{
    public class RenderWidgetModel : ModelBase
    {
        public string ActionName { get; set; }
        public string ControllerName { get; set; }
        public RouteValueDictionary RouteValues { get; set; }
    }
}