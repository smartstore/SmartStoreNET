using System.Net;
using SmartStore.Core;
using SmartStore.Services.Tasks;

namespace SmartStore.Services.Common
{
    /// <summary>
    /// Represents a task for keeping the site alive
    /// </summary>
    public partial class KeepAliveTask : ITask
    {
		private readonly IStoreContext _storeContext;

		public KeepAliveTask(IStoreContext storeContext)
        {
			this._storeContext = storeContext;
        }

        public void Execute()
        {
			var storeUrl = _storeContext.CurrentStore.Url.TrimEnd('\\').EnsureEndsWith("/");
            string url = storeUrl + "keepalive/index";

            try
            {
                using (var wc = new WebClient())
                {
                    //wc.Headers.Add("SmartStore.NET"); // makes problems
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
