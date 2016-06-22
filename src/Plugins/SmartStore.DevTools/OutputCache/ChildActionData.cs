using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;
using Newtonsoft.Json;

namespace SmartStore.DevTools.OutputCache
{
	public class ChildActionData
	{
		public string ActionName { get; set; }

		public string ControllerName { get; set; }

		public RouteValueDictionary RouteValues { get; set; }
	}
}