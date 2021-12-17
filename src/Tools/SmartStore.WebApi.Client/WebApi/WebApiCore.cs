using System.Text;

namespace SmartStore.WebApi.Client
{
    public class WebApiRequestContext
    {
        public string PublicKey { get; set; }
        public string SecretKey { get; set; }

        public string Url { get; set; }
        public int ProxyPort { get; set; }
        public string HttpMethod { get; set; }
        public string HttpAcceptType { get; set; }
        public string AdditionalHeaders { get; set; }

        public bool IsValid => !string.IsNullOrWhiteSpace(PublicKey) && !string.IsNullOrWhiteSpace(SecretKey) &&
                    !string.IsNullOrWhiteSpace(Url) &&
                    !string.IsNullOrWhiteSpace(HttpMethod) && !string.IsNullOrWhiteSpace(HttpAcceptType);

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine("PublicKey: " + PublicKey);
            sb.AppendLine("SecretKey: " + SecretKey);
            sb.AppendLine("Url: " + Url);
            sb.AppendLine("Proxy Port: " + (ProxyPort > 0 ? ProxyPort.ToString() : ""));
            sb.AppendLine("HttpMethod: " + HttpMethod);
            sb.AppendLine("HttpAcceptType: " + HttpAcceptType);

            return sb.ToString();
        }
    }


    public static class WebApiGlobal
    {
        public static int MaxTop => 120;
        public static int DefaultTimePeriodMinutes => 15;
        public static string RouteNameDefaultApi => "WebApi.Default";
        public static string RouteNameDefaultOdata => "WebApi.OData.Default";

        /// <remarks>see http://tools.ietf.org/html/rfc6648 </remarks>
        public static class HeaderName
        {
            private static string Prefix => "SmartStore-Net-Api-";

            public static string Date => Prefix + "Date";
            public static string PublicKey => Prefix + "PublicKey";
            public static string MaxTop => Prefix + "MaxTop";
            public static string HmacResultId => Prefix + "HmacResultId";
            public static string HmacResultDescription => Prefix + "HmacResultDesc";
            //public static string LastRequest { get { return Prefix + "LastRequest"; } }
        }

        public static class QueryOption
        {
            public static string Fulfill => "SmNetFulfill";
        }
    }
}
