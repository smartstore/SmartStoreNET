using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using SmartStore.ComponentModel;
using SmartStore.Core.Caching;
using SmartStore.Core.Configuration;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Configuration;
using SmartStore.Services.Configuration;

namespace SmartStore.Services.Tests.Configuration
{
    public class ConfigFileSettingService : SettingService
    {
        public ConfigFileSettingService(
            ICacheManager cacheManager,
            IRepository<Setting> settingRepository) :
            base(cacheManager, settingRepository)
        {
        }

        public override Setting GetSettingById(int settingId)
        {
            throw new InvalidOperationException("Get setting by id is not supported.");
        }

        public override T GetSettingByKey<T>(string key, T defaultValue = default(T), int storeId = 0, bool loadSharedValueIfNotFound = false)
        {
            if (string.IsNullOrEmpty(key))
            {
                return defaultValue;
            }

            var settings = GetAllSettings();
            key = key.Trim().ToLowerInvariant();

            var setting = settings.FirstOrDefault(x => x.Name.Equals(key, StringComparison.InvariantCultureIgnoreCase) &&
                x.StoreId == storeId);

            // Load shared value?
            if (setting == null && storeId > 0 && loadSharedValueIfNotFound)
            {
                setting = settings.FirstOrDefault(x => x.Name.Equals(key, StringComparison.InvariantCultureIgnoreCase) &&
                    x.StoreId == 0);
            }

            if (setting != null)
            {
                return setting.Value.Convert<T>();
            }

            return defaultValue;
        }

        public override void DeleteSetting(Setting setting)
        {
            throw new InvalidOperationException("Deleting settings is not supported.");
        }

        public override SaveSettingResult SetSetting<T>(string key, T value, int storeId = 0, bool clearCache = true)
        {
            throw new NotImplementedException();
        }

        protected override ISettings LoadSettingCore(Type settingType, int storeId = 0)
        {
            var appSettings = ConfigurationManager.AppSettings;
            var result = Activator.CreateInstance(settingType);

            var properties = FastProperty.GetCandidateProperties(settingType)
                .Select(pi => FastProperty.GetProperty(pi, PropertyCachingStrategy.Uncached))
                .Where(pi => pi.IsPublicSettable);

            foreach (var prop in properties)
            {
                var key = string.Concat(settingType.Name, ".", prop.Name);
                if (appSettings.AllKeys.Contains(key))
                {
                    var val = appSettings.Get(key);
                    var convertedVal = val.Convert(prop.Property.PropertyType, CultureInfo.CurrentCulture);
                    prop.SetValue(result, convertedVal);
                }
            }

            return result as ISettings;
        }

        public override IList<Setting> GetAllSettings()
        {
            var settings = new List<Setting>();
            var appSettings = ConfigurationManager.AppSettings;
            foreach (var setting in appSettings.AllKeys)
            {
                settings.Add(new Setting
                {
                    Name = setting.ToLowerInvariant(),
                    Value = appSettings[setting]
                });
            }

            return settings;
        }

        protected override void OnClearCache()
        {
            throw new NotImplementedException();
        }
    }
}
