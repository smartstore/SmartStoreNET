using System;
using System.Collections.Generic;
using SmartStore.Web.Framework.Mvc;

namespace SmartStore.Web.Framework.UI
{
	public class WidgetZoneModel : ModelBase
	{
		public IEnumerable<WidgetRouteInfo> Widgets { get; set; }
		public string WidgetZone { get; set; }
		public object Model { get; set; }
	}
}