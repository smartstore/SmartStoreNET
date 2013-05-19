using System;
using System.Web;

namespace SmartStore.Core.Events
{
	/// <summary>
	/// for Application_BeginRequest
	/// </summary>
	/// <remarks>codehint: sm-add</remarks>
	public class AppBeginRequestEvent
	{
		public HttpContext Context { get; set; }
	}
}
