using System.Web;
using SmartStore.Core.Data.Hooks;
using SmartStore.Core.Domain.Stores;
using SmartStore.Services.Stores;
using SmartStore.Services.Tasks;
using SmartStore.Utilities;

namespace SmartStore.Services.Hooks
{
    public class StoreSaveHook : DbSaveHook<Store>
    {
        private readonly ITaskScheduler _taskScheduler;
        private readonly IStoreService _storeService;
        private readonly HttpContextBase _httpContext;

        public StoreSaveHook(ITaskScheduler taskScheduler, IStoreService storeService, HttpContextBase httpContext)
        {
            _taskScheduler = taskScheduler;
            _storeService = storeService;
            _httpContext = httpContext;
        }

        protected override void OnUpdating(Store entity, IHookedEntity entry)
        {
        }

        protected override void OnInserted(Store entity, IHookedEntity entry)
        {
            TryChangeSchedulerBaseUrl();
        }

        protected override void OnUpdated(Store entity, IHookedEntity entry)
        {
            TryChangeSchedulerBaseUrl();
        }

        protected override void OnDeleted(Store entity, IHookedEntity entry)
        {
            TryChangeSchedulerBaseUrl();
        }

        private void TryChangeSchedulerBaseUrl()
        {
            if (CommonHelper.GetAppSetting<string>("sm:TaskSchedulerBaseUrl").IsWebUrl() == false)
            {
                _taskScheduler.SetBaseUrl(_storeService, _httpContext);
            }
        }
    }
}
