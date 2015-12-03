using System.IO.Compression;
using System.Web;
using System.Web.Mvc;

namespace SmartStore.Web.Framework.Filters
{
	public class CompressAttribute : ActionFilterAttribute
	{
		public bool PreferDeflate { get; set; }

		public override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			var response = HttpContext.Current.Response;
			string acceptEncoding = HttpContext.Current.Request.Headers["Accept-Encoding"].EmptyNull().ToLower();

			if (acceptEncoding.Contains("gzip") && !(PreferDeflate && acceptEncoding.Contains("deflate")))
			{
				response.Filter = new GZipStream(response.Filter, CompressionMode.Compress);

				response.Headers.Remove("Content-Encoding");
				response.AppendHeader("Content-Encoding", "gzip");
			}
			else if (acceptEncoding.Contains("deflate"))
			{
				response.Filter = new DeflateStream(response.Filter, CompressionMode.Compress);

				response.Headers.Remove("Content-Encoding");
				response.AppendHeader("Content-Encoding", "deflate");
			}

			//response.AppendHeader("Vary", "Content-Encoding");
		}
	}
}
