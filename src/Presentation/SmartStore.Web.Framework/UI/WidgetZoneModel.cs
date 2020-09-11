using System.Collections.Generic;
using Newtonsoft.Json;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Web.Framework.UI
{
    public class WidgetZoneModel : ModelBase
    {
        public List<WidgetRouteInfo> Widgets { get; set; }

        public string WidgetZone { get; set; }

        [JsonIgnore]
        public object Model { get; set; }
    }
}