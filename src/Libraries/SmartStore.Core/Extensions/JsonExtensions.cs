using System;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;


///////////////////////////////////////////////////////////////////////
// Needs JSon.Net (Newtonsoft.Json.dll) from http://json.codeplex.com/
//////////////////////////////////////////////////////////////////////

namespace SmartStore
{
    public static class JsonExtensions
    {
        public static async Task<dynamic> GetDynamicJsonObject(this Uri uri)
        {
            using (WebClient wc = new WebClient())
            {
                wc.Encoding = System.Text.Encoding.UTF8;
                wc.Headers["User-Agent"] = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1; .NET CLR 2.0.50727; .NET4.0C; .NET4.0E)";
                var response = await wc.DownloadStringTaskAsync(uri);
                return JsonConvert.DeserializeObject(response);
            }
        }
    }
}



