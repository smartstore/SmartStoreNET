using System;
using System.Web;
using System.Linq;
using SmartStore.Core;
using SmartStore.Services.Stores;

namespace SmartStore.Services.Tasks
{
    public interface ITaskSweeper
    {
        TimeSpan Interval { get; set; }
        string BaseUrl { get; set; }
        bool IsRunning { get; }

        void Start();
        void Stop();
        void ExecuteSingleTask(int scheduleTaskId);
        bool VerifyAuthToken(string authToken);
    }

    public static class ITaskSweeperExtensions
    {
        internal static void SetBaseUrl(this ITaskSweeper sweeper, IStoreService storeService, HttpContextBase httpContext)
        {
            var path = VirtualPathUtility.ToAbsolute("~/TaskScheduler");
            string url = "";

            if (!httpContext.Request.IsLocal)
            {
                var defaultStore = storeService.GetAllStores().FirstOrDefault(x => storeService.IsStoreDataValid(x));
                if (defaultStore != null)
                {
                    url = defaultStore.Url;
                }
            }

            if (url.IsEmpty())
            {
                url = WebHelper.GetAbsoluteUrl(path, httpContext.Request);
            }

            sweeper.BaseUrl = url;
        }
    }
}
