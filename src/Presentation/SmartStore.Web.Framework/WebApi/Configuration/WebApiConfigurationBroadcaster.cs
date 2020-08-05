using System.Collections.Generic;
using System.Web.Http;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Routing.Conventions;

namespace SmartStore.Web.Framework.WebApi.Configuration
{
	public class WebApiConfigurationBroadcaster
	{
		public HttpConfiguration Configuration { get; set; }

		public ODataConventionModelBuilder ModelBuilder { get; set; }

		/// <remarks>Use it with Insert() not with Add() otherwise your convention might not take effect.</remarks>
		public IList<IODataRoutingConvention> RoutingConventions { get; set; }
	}
}
