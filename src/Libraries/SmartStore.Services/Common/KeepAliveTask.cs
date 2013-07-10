using System.Net;
using SmartStore.Core.Domain;
using SmartStore.Services.Tasks;

namespace SmartStore.Services.Common
{
    /// <summary>
    /// Represents a task for keeping the site alive
    /// </summary>
    public partial class KeepAliveTask : ITask
    {
        private readonly StoreInformationSettings _storeInformationSettings;
        public KeepAliveTask(StoreInformationSettings storeInformationSettings)
        {
            this._storeInformationSettings = storeInformationSettings;
        }
        /// <summary>
        /// Executes a task
        /// </summary>
        public void Execute()
        {
            var storeUrl = _storeInformationSettings.StoreUrl.TrimEnd('\\').EnsureEndsWith("/");
            string url = storeUrl + "keepalive/index";

            try
            {
                using (var wc = new WebClient())
                {
                    wc.Headers.Add("SmartStore.NET");
                    wc.DownloadString(url);
                }
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.ProtocolError && ex.Response != null)
                {
                    var resp = (HttpWebResponse)ex.Response;
                    if (resp.StatusCode == HttpStatusCode.NotFound) // HTTP 404
                    {
                        // the page was not found (as it can be expected with some webservers)
                        return;
                    }
                }
                // throw any other exception - this should not occur
                throw;
            }
        }
    }
}
