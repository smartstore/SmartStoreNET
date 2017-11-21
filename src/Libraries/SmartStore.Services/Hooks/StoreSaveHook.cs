using System;
using SmartStore.Services.Media;
using SmartStore.Core.Data.Hooks;
using SmartStore.Core.Domain.Stores;
using SmartStore.Services.Tasks;
using SmartStore.Services.Stores;
using System.Web;
using SmartStore.Utilities;

namespace SmartStore.Services.Hooks
{
	public class StoreSaveHook : DbSaveHook<Store>
	{
		private readonly IPictureService _pictureService;
		private readonly ITaskScheduler _taskScheduler;
		private readonly IStoreService _storeService;
		private readonly HttpContextBase _httpContext;

		public StoreSaveHook(IPictureService pictureService, ITaskScheduler taskScheduler, IStoreService storeService, HttpContextBase httpContext)
		{
			_pictureService = pictureService;
			_taskScheduler = taskScheduler;
			_storeService = storeService;
			_httpContext = httpContext;
		}

		protected override void OnUpdating(Store entity, IHookedEntity entry)
		{
			if (entry.IsPropertyModified(nameof(entity.ContentDeliveryNetwork)))
			{
				_pictureService.ClearUrlCache();
			}
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
			_pictureService.ClearUrlCache();
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
