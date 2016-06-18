using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;
using Newtonsoft.Json;

namespace SmartStore.DevTools.OutputCache
{
	public class ChildActionDataSerializer
	{
		public string Serialise(ChildActionData actionData)
		{
			return JsonConvert.SerializeObject(actionData);
		}

		public ChildActionData Deserialise(string serialisedActionData)
		{
			return JsonConvert.DeserializeObject<ChildActionData>(serialisedActionData);
		}
	}

	public class ChildActionData
	{
		public string ActionName { get; set; }

		public string ControllerName { get; set; }

		public RouteValueDictionary RouteValues { get; set; }
	}
}