using System;
using System.Linq;
using System.Collections.Generic;
using System.Configuration;
using SmartStore.Core;
using SmartStore.Core.Configuration;
using SmartStore.Core.Domain.Configuration;
using SmartStore.Services.Configuration;
using SmartStore.Core.Caching;
using SmartStore.Core.Events;
using SmartStore.Core.Data;

namespace SmartStore.Services.Tests.Configuration
{
    public class ConfigFileSettingService : SettingService
    {
		public ConfigFileSettingService(ICacheManager cacheManager,
			 IEventPublisher eventPublisher,
			 IRepository<Setting> settingRepository) :
			base(cacheManager, eventPublisher, settingRepository)
		{
		}

		public override Setting GetSettingById(int settingId)
        {
            throw new InvalidOperationException("Get setting by id is not supported");
        }

		public override T GetSettingByKey<T>(string key, T defaultValue = default(T), int storeId = 0, bool loadSharedValueIfNotFound = false)
        {
			if (String.IsNullOrEmpty(key))
				return defaultValue;

			var settings = GetAllSettings();
			key = key.Trim().ToLowerInvariant();

			var setting = settings.FirstOrDefault(x => x.Name.Equals(key, StringComparison.InvariantCultureIgnoreCase) &&
				x.StoreId == storeId);

			//load shared value?
			if (setting == null && storeId > 0 && loadSharedValueIfNotFound)
			{
				setting = settings.FirstOrDefault(x => x.Name.Equals(key, StringComparison.InvariantCultureIgnoreCase) &&
					x.StoreId == 0);
			}

			if (setting != null)
				return setting.Value.Convert<T>();

			return defaultValue;
        }

		public override void DeleteSetting(Setting setting)
        {
            throw new InvalidOperationException("Deleting settings is not supported");
        }

		public override void SetSetting<T>(string key, T value, int storeId = 0, bool clearCache = true)
        {
            throw new NotImplementedException();
        }

		public override IList<Setting> GetAllSettings()
        {
			var settings = new List<Setting>();
			var appSettings = ConfigurationManager.AppSettings;
			foreach (var setting in appSettings.AllKeys)
			{
				settings.Add(new Setting()
				{
					Name = setting.ToLowerInvariant(),
					Value = appSettings[setting]
				});
			}

            return settings;
        }

		public override void ClearCache()
        {
            throw new NotImplementedException();
        }
    }
}
