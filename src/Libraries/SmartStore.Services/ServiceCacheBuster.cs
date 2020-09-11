using System;
using SmartStore.Core.Caching;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Data.Hooks;
using SmartStore.Core;

namespace SmartStore.Services
{
    public partial class ServiceCacheBuster : DbSaveHook<BaseEntity>
    {
        public const string STORE_LANGUAGE_MAP_KEY = "svc:storelangmap*";

        private readonly ICacheManager _cacheManager;

        public ServiceCacheBuster(ICacheManager cacheManager)
        {
            _cacheManager = cacheManager;
        }

        protected override void OnInserted(BaseEntity entity, IHookedEntity entry)
        {
            if (entry.EntityType != typeof(Store) && entry.EntityType != typeof(Language))
                throw new NotSupportedException();

            _cacheManager.Remove(STORE_LANGUAGE_MAP_KEY);
        }

        protected override void OnDeleted(BaseEntity entity, IHookedEntity entry)
        {
            if (entry.EntityType != typeof(Store) && entry.EntityType != typeof(Language))
                throw new NotImplementedException();

            _cacheManager.Remove(STORE_LANGUAGE_MAP_KEY);
        }

        protected override void OnUpdated(BaseEntity entity, IHookedEntity entry)
        {
            if (entry.EntityType != typeof(Language))
                throw new NotImplementedException();

            _cacheManager.Remove(STORE_LANGUAGE_MAP_KEY);
        }
    }
}
