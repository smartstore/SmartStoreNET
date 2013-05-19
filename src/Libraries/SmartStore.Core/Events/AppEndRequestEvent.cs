using System;
using System.Web;

namespace SmartStore.Core.Events
{
	/// <summary>
	/// for Application_EndRequest
	/// </summary>
	/// <remarks>codehint: sm-add</remarks>
	public class AppEndRequestEvent
	{
		public HttpContext Context { get; set; }
	}
}
