using SmartStore.Core.Data;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Plugins;
using SmartStore.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Caching;

namespace SmartStore.Web.Framework.WebApi
{
	public static class WebApiCaching
	{
		private static object _lock = new object();

		private static T _get<T>(string key, Action fetch)
		{
			if (HttpRuntime.Cache[key] == null)
			{
				lock (_lock)
				{
					if (HttpRuntime.Cache[key] == null)
					{
						fetch();
					}
				}
			}
			return (T)HttpRuntime.Cache[key];
		}

		/// <remarks>
		/// Lazy storing... fired on app shut down. Note that items with CacheItemPriority.NotRemovable are not removed when the cache is emptied.
		/// We're beyond infrastructure and cannot use IOC objects here. It would lead to ComponentNotRegisteredException from autofac.
		/// </remarks>
		private static void OnDataRemoved(string key, object value, CacheItemRemovedReason reason)
		{
			try
			{
				if (key == WebApiUserCacheData.Key)
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

		public static void Remove(string key)
		{
			try
			{
				if (HttpRuntime.Cache[key] != null)
					HttpRuntime.Cache.Remove(key);
			}
			catch (Exception) { }
		}
		public static List<WebApiUserCacheData> UserData()
		{
			return _get<List<WebApiUserCacheData>>(WebApiUserCacheData.Key, () =>
			{
				var engine = EngineContext.Current;
				var genericAttributes = engine.Resolve<IRepository<GenericAttribute>>();
				var customers = engine.Resolve<IRepository<Customer>>();

				var attributes = (
					from a in genericAttributes.Table
					join c in customers.Table on a.EntityId equals c.Id
					where !c.Deleted && c.Active && a.KeyGroup == "Customer" && a.Key == WebApiUserCacheData.Key
					select new
					{
						a.Id,
						a.EntityId,
						a.Value
					}).ToList();

				var data = new List<WebApiUserCacheData>();

				foreach (var attribute in attributes)
				{
					if (!string.IsNullOrWhiteSpace(attribute.Value) && !data.Exists(x => x.CustomerId == attribute.EntityId))
					{
						string[] arr = attribute.Value.SplitSafe("¶");

						if (arr.Length > 2)
						{
							var apiUser = new WebApiUserCacheData()
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

				HttpRuntime.Cache.Add(WebApiUserCacheData.Key, data, null, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable,
					new CacheItemRemovedCallback(OnDataRemoved));
			});
		}
		public static WebApiControllingCacheData ControllingData()
		{
			return _get<WebApiControllingCacheData>(WebApiControllingCacheData.Key, () =>
			{
				var engine = EngineContext.Current;
				var plugin = engine.Resolve<IPluginFinder>().GetPluginDescriptorBySystemName(WebApiGlobal.PluginSystemName);
				var settings = engine.Resolve<WebApiSettings>();

				var data = new WebApiControllingCacheData()
				{
					ValidMinutePeriod = settings.ValidMinutePeriod,
					LogUnauthorized = settings.LogUnauthorized,
					ApiUnavailable = (plugin == null || !plugin.Installed),
					PluginVersion = (plugin == null ? "1.0" : plugin.Version.ToString())
				};

				HttpRuntime.Cache.Add(WebApiControllingCacheData.Key, data, null, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable, null);
			});
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

		public static string Key { get { return "WebApiUserData"; } }

		public bool IsValid
		{
			get
			{
				return GenericAttributeId != 0 && CustomerId != 0 && !string.IsNullOrWhiteSpace(PublicKey) && !string.IsNullOrWhiteSpace(SecretKey);
			}
		}
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


	public partial class WebApiControllingCacheData
	{
		public bool ApiUnavailable { get; set; }
		public int ValidMinutePeriod { get; set; }
		public bool LogUnauthorized { get; set; }
		public string PluginVersion { get; set; }

		public static string Key { get { return "WebApiControllingData"; } }

		public string Version
		{
			get
			{
				return "{0} {1}".FormatWith(WebApiGlobal.MaxApiVersion, PluginVersion);
			}
		}
	}
}
