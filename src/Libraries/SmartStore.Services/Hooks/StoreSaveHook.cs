using System;
using SmartStore.Services.Media;
using SmartStore.Core.Data.Hooks;
using SmartStore.Core.Domain.Stores;

namespace SmartStore.Services.Hooks
{
	public class StoreSaveHook : DbSaveHook<Store>
	{
		private readonly IPictureService _pictureService;

		public StoreSaveHook(IPictureService pictureService)
		{
			_pictureService = pictureService;
		}

		protected override void OnUpdating(Store entity, IHookedEntity entry)
		{
			if (entry.IsPropertyModified(nameof(entity.ContentDeliveryNetwork)))
			{
				_pictureService.ClearUrlCache(entity.Id);
			}
		}

		protected override void OnDeleted(Store entity, IHookedEntity entry)
		{
			_pictureService.ClearUrlCache(entity.Id);
		}
	}
}
