using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Caching;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Infrastructure;
using SmartStore.Data;

namespace SmartStore.Web.Framework.WebApi.Caching
{
    public static class WebApiCachingUserData
    {
        private static readonly object _lock = new object();

        /// <remarks>
        /// Lazy storing... fired on app shut down. Note that items with CacheItemPriority.NotRemovable are not removed when the cache is emptied.
        /// We're beyond infrastructure and cannot use IOC objects here. It would lead to ComponentNotRegisteredException from autofac.
        /// </remarks>
        private static void OnDataRemoved(string key, object value, CacheItemRemovedReason reason)
        {
            try
            {
                if (key == Key)
                {
                    var cacheData = value as List<WebApiUserCacheData>;

                    if (cacheData != null)
                    {
                        var dataToStore = cacheData.Where(x => x.LastRequest.HasValue && x.IsValid);

                        if (dataToStore.Count() > 0)
                        {
                            if (DataSettings.Current.IsValid())
                            {
                                var dbContext = new SmartObjectContext(DataSettings.Current.DataConnectionString);

                                foreach (var user in dataToStore)
                                {
                                    try
                                    {
                                        dbContext.Execute("Update GenericAttribute Set Value = {1} Where Id = {0}", user.GenericAttributeId, user.ToString());
                                    }
                                    catch (Exception exc)
                                    {
                                        exc.Dump();
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                exc.Dump();
            }
        }

        public static string Key { get; } = "WebApiUserData";

        public static void Remove()
        {
            try
            {
                HttpRuntime.Cache.Remove(Key);
            }
            catch { }
        }

        public static List<WebApiUserCacheData> Data()
        {
            var data = HttpRuntime.Cache[Key] as List<WebApiUserCacheData>;
            if (data == null)
            {
                lock (_lock)
                {
                    data = HttpRuntime.Cache[Key] as List<WebApiUserCacheData>;

                    if (data == null)
                    {
                        var engine = EngineContext.Current;
                        var genericAttributes = engine.Resolve<IRepository<GenericAttribute>>();
                        var customers = engine.Resolve<IRepository<Customer>>();

                        var attributes = (
                            from a in genericAttributes.Table
                            join c in customers.Table on a.EntityId equals c.Id
                            where !c.Deleted && c.Active && a.KeyGroup == "Customer" && a.Key == Key
                            select new
                            {
                                a.Id,
                                a.EntityId,
                                a.Value
                            }).ToList();

                        data = new List<WebApiUserCacheData>();

                        foreach (var attribute in attributes)
                        {
                            if (!string.IsNullOrWhiteSpace(attribute.Value) && !data.Exists(x => x.CustomerId == attribute.EntityId))
                            {
                                string[] arr = attribute.Value.SplitSafe("¶");

                                if (arr.Length > 2)
                                {
                                    var apiUser = new WebApiUserCacheData
                                    {
                                        GenericAttributeId = attribute.Id,
                                        CustomerId = attribute.EntityId,
                                        Enabled = bool.Parse(arr[0]),
                                        PublicKey = arr[1],
                                        SecretKey = arr[2]
                                    };

                                    if (arr.Length > 3)
                                        apiUser.LastRequest = DateTime.ParseExact(arr[3], "o", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);

                                    if (apiUser.IsValid)
                                        data.Add(apiUser);
                                }
                            }
                        }

                        HttpRuntime.Cache.Add(Key, data, null, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable,
                            new CacheItemRemovedCallback(OnDataRemoved));
                    }
                }
            }
            return data;
        }
    }

    public partial class WebApiUserCacheData
    {
        public int GenericAttributeId { get; set; }
        public int CustomerId { get; set; }
        public bool Enabled { get; set; }
        public string PublicKey { get; set; }
        public string SecretKey { get; set; }
        public DateTime? LastRequest { get; set; }

        public bool IsValid => GenericAttributeId != 0 && CustomerId != 0 && !string.IsNullOrWhiteSpace(PublicKey) && !string.IsNullOrWhiteSpace(SecretKey);
        public override string ToString()
        {
            if (!string.IsNullOrWhiteSpace(PublicKey) && !string.IsNullOrWhiteSpace(SecretKey))
            {
                if (!LastRequest.HasValue)
                    return string.Join("¶", Enabled, PublicKey, SecretKey);

                return string.Join("¶", Enabled, PublicKey, SecretKey, LastRequest.Value.ToString("o"));
            }
            return "";
        }
    }
}
