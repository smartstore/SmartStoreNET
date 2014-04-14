using System.Text;

namespace SmartStore.Net.WebApi
{
	public class WebApiRequestContext
	{
		public string PublicKey { get; set; }
		public string SecretKey { get; set; }

		public string Url { get; set; }
		public string HttpMethod { get; set; }
		public string HttpAcceptType { get; set; }

		public bool IsValid
		{
			get
			{
				return 
					!string.IsNullOrWhiteSpace(PublicKey) && !string.IsNullOrWhiteSpace(SecretKey) && 
					!string.IsNullOrWhiteSpace(Url) &&
					!string.IsNullOrWhiteSpace(HttpMethod) && !string.IsNullOrWhiteSpace(HttpAcceptType);
			}
		}
		public override string ToString()
		{
			var sb = new StringBuilder();

			sb.AppendLine(string.Format("PublicKey: ", PublicKey));
			sb.AppendLine(string.Format("SecretKey: ", SecretKey));
			sb.AppendLine(string.Format("Url: ", Url));
			sb.AppendLine(string.Format("HttpMethod: ", HttpMethod));
			sb.AppendLine(string.Format("HttpAcceptType: ", HttpAcceptType));

			return sb.ToString();
		}
	}


	public static class WebApiGlobal
	{
		public static int MaxTop { get { return 120; } }
		public static int DefaultTimePeriodMinutes { get { return 15; } }
		public static string RouteNameDefaultApi { get { return "WebApi.Default"; } }
		public static string RouteNameDefaultOdata { get { return "WebApi.OData.Default"; } }

		/// <remarks>see http://tools.ietf.org/html/rfc6648 </remarks>
		public static class HeaderName
		{
			private static string Prefix { get { return "SmartStore-Net-Api-"; } }

			public static string Date { get { return Prefix + "Date"; } }
			public static string PublicKey { get { return Prefix + "PublicKey"; } }
			public static string MaxTop { get { return Prefix + "MaxTop"; } }
			public static string HmacResultId { get { return Prefix + "HmacResultId"; } }
			public static string HmacResultDescription { get { return Prefix + "HmacResultDesc"; } }
			//public static string LastRequest { get { return Prefix + "LastRequest"; } }
		}

		public static class QueryOption
		{
			public static string Fulfill { get { return "SmNetFulfill"; } }
		}
	}
}
