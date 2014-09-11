using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace SmartStore.Web.Framework.Mvc
{
	public class ProviderModel : ModelBase
	{
		[SmartResourceDisplayName("Common.SystemName")]
		public string SystemName { get; set; }

		[SmartResourceDisplayName("Common.FriendlyName")]
		[AllowHtml]
		public string FriendlyName { get; set; }

		[SmartResourceDisplayName("Common.Description")]
		[AllowHtml]
		public string Description { get; set; }

		[SmartResourceDisplayName("Common.DisplayOrder")]
		public int DisplayOrder { get; set; }

		public bool IsConfigurable { get; set; }

		public RouteInfo ConfigurationRoute { get; set; }
	}

}