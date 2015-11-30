using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SmartStore.Core.Caching;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Events;
using SmartStore.Services.Tasks;
using SmartStore.Services.Stores;

namespace SmartStore.Services
{
	public class ServiceCacheConsumer :
		IConsumer<EntityInserted<Store>>,
        IConsumer<EntityDeleted<Store>>,
        IConsumer<EntityInserted<Language>>,
        IConsumer<EntityUpdated<Language>>,
        IConsumer<EntityDeleted<Language>>
	{
		public const string STORE_LANGUAGE_MAP_KEY = "sm.svc.storelangmap";

		private readonly ICacheManager _cacheManager;

        public ServiceCacheConsumer(Func<string, ICacheManager> cache)
        {
			this._cacheManager = cache("static");
        }

		public void HandleEvent(EntityInserted<Store> eventMessage)
		{
			_cacheManager.Remove(STORE_LANGUAGE_MAP_KEY);
		}

		public void HandleEvent(EntityDeleted<Store> eventMessage)
		{
			_cacheManager.Remove(STORE_LANGUAGE_MAP_KEY);
		}

		public void HandleEvent(EntityInserted<Language> eventMessage)
		{
			_cacheManager.Remove(STORE_LANGUAGE_MAP_KEY);
		}

		public void HandleEvent(EntityUpdated<Language> eventMessage)
		{
			_cacheManager.Remove(STORE_LANGUAGE_MAP_KEY);
		}

		public void HandleEvent(EntityDeleted<Language> eventMessage)
		{
			_cacheManager.Remove(STORE_LANGUAGE_MAP_KEY);
		}
    }
}
