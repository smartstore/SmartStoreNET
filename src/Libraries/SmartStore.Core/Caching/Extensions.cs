using System;

namespace SmartStore.Core.Caching
{
    /// <summary>
    /// Extensions
    /// </summary>
    public static class CacheExtensions
    {

        public static T Get<T>(this ICacheManager cacheManager, string key, int? cacheTime = 60)
        {
            return cacheManager.Get<T>(key, () => { return default(T); }, cacheTime);
        }
        
        //public static T Get<T>(this ICacheManager cacheManager, string key, Func<T> acquire)
        //{
        //    return Get(cacheManager, key, 60, acquire);
        //}

        //public static T Get<T>(this ICacheManager cacheManager, string key, int cacheTime, Func<T> acquire) 
        //{
        //    if (cacheManager.Contains(key))
        //    {
        //        return cacheManager.Get<T>(key);
        //    }
        //    else
        //    {
        //        var result = acquire();
        //        if (!cacheManager.Contains(key))
        //        {
        //            cacheManager.Set(key, result, cacheTime);
        //            return result;
        //        }

        //        return cacheManager.Get<T>(key);
        //    }
        //}

        //// codehint: sm-add
        //public static void Set(this ICacheManager cacheManager, string key, object data)
        //{
        //    cacheManager.Set(key, data, 60);
        //}
    }
}
