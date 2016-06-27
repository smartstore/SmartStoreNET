using System.Web.Routing;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Web.Framework.UI
{
	public partial class WidgetRouteInfo : ModelBase
    {
        public string ActionName { get; set; }
        public string ControllerName { get; set; }
        public RouteValueDictionary RouteValues { get; set; }
		public int Order { get; set; }
    }
}