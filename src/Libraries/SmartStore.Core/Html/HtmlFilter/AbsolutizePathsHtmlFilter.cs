using System;
using System.Collections.Generic;
using System.Web;

namespace SmartStore.Core.Html
{
	public class AbsolutizePathsHtmlFilter : IHtmlFilter
	{
		private readonly HttpContextBase _httpContext;

		public AbsolutizePathsHtmlFilter(HttpContextBase httpContext)
		{
			_httpContext = httpContext;
		}

		public string Flavor
		{
			get { return "html"; }
		}

		public int Ordinal
		{
			get { return 10; }
		}

		public string Process(string input, IDictionary<string, object> parameters)
		{
			if (!parameters.ContainsKey("outbound"))
			{
				return input;
			}

			var protocol = parameters.Get("protocol") as string ?? _httpContext.Request?.Url.Scheme;
			var host = parameters.Get("host") as string ?? _httpContext.Request?.Url.Authority;

			if (protocol.HasValue() && host.HasValue())
			{
				return WebHelper.MakeAllUrlsAbsolute(input, protocol, host);
			}

			return input;
		}
	}
}
