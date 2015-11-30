using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Events;
using SmartStore.Services.Stores;
using SmartStore.Utilities;

namespace SmartStore.Services.Tasks
{
    public class ChangeTaskSchedulerBaseUrlConsumer :
        IConsumer<EntityInserted<Store>>,
        IConsumer<EntityUpdated<Store>>,
        IConsumer<EntityDeleted<Store>>
    {
        private readonly ITaskScheduler _taskScheduler;
        private readonly IStoreService _storeService;
        private readonly HttpContextBase _httpContext;
		private readonly bool _shouldChange;

		public ChangeTaskSchedulerBaseUrlConsumer(ITaskScheduler taskScheduler, IStoreService storeService, HttpContextBase httpContext)
        {
			this._taskScheduler = taskScheduler;
            this._storeService = storeService;
            this._httpContext = httpContext;
			this._shouldChange = CommonHelper.GetAppSetting<string>("sm:TaskSchedulerBaseUrl").IsWebUrl() == false;
        }

        public void HandleEvent(EntityInserted<Store> eventMessage)
        {
			HandleEventCore();
        }

        public void HandleEvent(EntityUpdated<Store> eventMessage)
        {
			HandleEventCore();
        }

        public void HandleEvent(EntityDeleted<Store> eventMessage)
        {
			HandleEventCore();
        }

		private void HandleEventCore() 
		{
			if (_shouldChange)
			{
				_taskScheduler.SetBaseUrl(_storeService, _httpContext);
			}
		}
    }
}
