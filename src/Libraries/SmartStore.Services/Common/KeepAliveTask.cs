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
            using (var wc = new WebClient())
            {
                wc.DownloadString(url); 
            }
        }
    }
}
