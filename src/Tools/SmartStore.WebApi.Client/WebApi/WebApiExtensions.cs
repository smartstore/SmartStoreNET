using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace SmartStore.WebApi.Client
{
    public static class WebApiExtensions
    {
        private static readonly DateTime BeginOfEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>Epoch time. Number of seconds since midnight (UTC) on 1st January 1970.</summary>
        public static long ToUnixTime(this DateTime date)
        {
            return Convert.ToInt64((date.ToUniversalTime() - BeginOfEpoch).TotalSeconds);
        }

        /// <summary>UTC date based on number of seconds since midnight (UTC) on 1st January 1970.</summary>
        public static DateTime FromUnixTime(this long unixTime)
        {
            return BeginOfEpoch.AddSeconds(unixTime);
        }

        /// <summary>Converts bytes into a hex string.</summary>
        public static string ToHexString(this byte[] bytes, int length = 0)
        {
            if (bytes == null || bytes.Length <= 0)
            {
                return "";
            }

            var sb = new StringBuilder();

            foreach (byte b in bytes)
            {
                sb.Append(b.ToString("x2"));

                if (length > 0 && sb.Length >= length)
                    break;
            }

            return sb.ToString();
        }

        /// <seealso cref="http://weblog.west-wind.com/posts/2012/Aug/30/Using-JSONNET-for-dynamic-JSON-parsing" />
        /// <seealso cref="http://james.newtonking.com/json/help/index.html?topic=html/QueryJsonDynamic.htm" />
        /// <seealso cref="http://james.newtonking.com/json/help/index.html?topic=html/LINQtoJSON.htm" />
        public static List<Customer> TryParseCustomers(this WebApiConsumerResponse response)
        {
            if (response == null || string.IsNullOrWhiteSpace(response.Content))
            {
                return null;
            }

            //        dynamic dynamicJson = JObject.Parse(response.Content);

            //        foreach (dynamic customer in dynamicJson.value)
            //        {
            //            string str = string.Format("{0} {1} {2}", customer.Id, customer.CustomerGuid, customer.Email);
            //str.Dump();
            //        }

            var json = JObject.Parse(response.Content);
            string metadata = (string)json["@odata.context"];

            if (!string.IsNullOrWhiteSpace(metadata) && metadata.EndsWith("#Customers"))
            {
                var customers = json["value"].Select(x => x.ToObject<Customer>()).ToList();

                return customers;
            }

            return null;
        }
    }
}
