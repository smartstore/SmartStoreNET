using System.Text;

namespace SmartStore.Web.Framework.WebApi
{
    public static class WebApiGlobal
    {
        public static int MaxApiVersion => 1;
        public static int DefaultMaxTop => 120;
        public static int DefaultMaxExpansionDepth => 8;
        public static int DefaultTimePeriodMinutes => 15;
        public static string RouteNameDefaultApi => "WebApi.Default";
        public static string RouteNameDefaultOdata => "WebApi.OData.Default";
        public static string RouteNameUploads => "WebApi.Uploads";
        public static string MostRecentOdataPath => "odata/v1";
        public static string PluginSystemName => "SmartStore.WebApi";

        /// <see cref="http://tools.ietf.org/html/rfc6648"/>
        public static class Header
        {
            private static string Prefix => "SmartStore-Net-Api-";

            public static string Date => Prefix + "Date";
            public static string PublicKey => Prefix + "PublicKey";
            public static string MaxTop => Prefix + "MaxTop";
            public static string AppVersion => Prefix + "AppVersion";
            public static string Version => Prefix + "Version";
            public static string CustomerId => Prefix + "CustomerId";
            public static string HmacResultId => Prefix + "HmacResultId";
            public static string HmacResultDescription => Prefix + "HmacResultDesc";
            public static string MissingPermission => Prefix + "MissingPermission";
            //public static string LastRequest { get { return Prefix + "LastRequest"; } }

            public static string CorsExposed
            {
                get
                {
                    string headers = string.Join(",", Date, MaxTop, HmacResultId, HmacResultDescription);
                    return headers;
                }
            }
        }

        public static class Error
        {
            public static string PropertyNotFound => "Entity does not own property '{0}'.";
            public static string NoKeyFromPath => "Cannot retrieve valid entity key from OData path.";
            public static string NoRelatedKeyFromPath => "Cannot retrieve valid related entity key from OData path.";
            public static string NoNavigationFromPath => "Cannot retrieve the navigation property from OData path.";
            public static string EntityNotFound => "Entity with key '{0}' could not be found.";
        }

        public static class QueryOption
        {
            public static string Fulfill => "SmNetFulfill";
        }
    }


    public class WebApiRequestContext
    {
        public string PublicKey { get; set; }
        public string SecretKey { get; set; }

        public string Url { get; set; }
        public string HttpMethod { get; set; }
        public string HttpAcceptType { get; set; }

        public bool IsValid => !string.IsNullOrWhiteSpace(PublicKey) && !string.IsNullOrWhiteSpace(SecretKey) &&
                    !string.IsNullOrWhiteSpace(Url) &&
                    !string.IsNullOrWhiteSpace(HttpMethod) && !string.IsNullOrWhiteSpace(HttpAcceptType);
        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine("PublicKey: " + PublicKey.EmptyNull());
            sb.AppendLine("Url: " + Url.EmptyNull());
            sb.AppendLine("HttpMethod: " + HttpMethod.EmptyNull());
            sb.AppendLine("HttpAcceptType: " + HttpAcceptType.EmptyNull());

            return sb.ToString();
        }
    }
}
