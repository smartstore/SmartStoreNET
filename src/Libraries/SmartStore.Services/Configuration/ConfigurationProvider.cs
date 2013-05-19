using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core;
using SmartStore.Core.Configuration;
using SmartStore.Core.Domain.Configuration;
using SmartStore.Utilities;
using Newtonsoft.Json;
using Fasterflect;

namespace SmartStore.Services.Configuration
{
    public class ConfigurationProvider<TSettings> : IConfigurationProvider<TSettings> where TSettings : ISettings, new()
    {
        readonly ISettingService _settingService;

        public ConfigurationProvider(ISettingService settingService)
        {
            this._settingService = settingService;
            this.BuildConfiguration();
        }

        public TSettings Settings { get; protected set; }

        private void BuildConfiguration()
        {
            // codehint: sm-add
            if (typeof(TSettings).HasAttribute<JsonPersistAttribute>(true))
            {
                BuildConfigurationJson();
                return;
            }

            Settings = (TSettings)typeof(TSettings).CreateInstance();

            // get properties we can write to
            var properties = from prop in typeof(TSettings).Properties(Flags.InstancePublic)
                             where prop.CanWrite && prop.CanRead
                             let setting = _settingService.GetSettingByKey<string>(typeof(TSettings).Name + "." + prop.Name)
                             let converter = CommonHelper.GetCustomTypeConverter(prop.PropertyType)
                             where setting != null
                             where converter.CanConvertFrom(typeof(string)) && converter.IsValid(setting)
                             let value = converter.ConvertFromInvariantString(setting)
                             select new { prop, value };

            // assign properties
            properties.ToList().ForEach(p => Settings.TrySetPropertyValue(p.prop.Name, p.value));
        }

        // codehint: sm-add
        private void BuildConfigurationJson()
        {
            Type t = typeof(TSettings);
            string key = t.Namespace + "." + t.Name;

            this.Settings = Activator.CreateInstance<TSettings>();

            var rawSetting = _settingService.GetSettingByKey<string>(key);
            if (rawSetting.HasValue())
            {
                JsonConvert.PopulateObject(rawSetting, this.Settings);
            }
        }

        public void SaveSettings(TSettings settings)
        {
            // codehint: sm-add
            if (typeof(TSettings).HasAttribute<JsonPersistAttribute>(true))
            {
                SaveSettingsJson(settings);
                return;
            }

            var properties = from prop in typeof(TSettings).Properties(Flags.InstancePublic)
                             where prop.CanWrite && prop.CanRead
                             where CommonHelper.GetCustomTypeConverter(prop.PropertyType).CanConvertFrom(typeof(string))
                             select prop;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            foreach (var prop in properties)
            {
                string key = typeof(TSettings).Name + "." + prop.Name;
                // Duck typing is not supported in C#. That's why we're using dynamic type
                dynamic value = settings.TryGetPropertyValue(prop.Name);

                _settingService.SetSetting(key, value ?? "", false);
   
            }

            //and now clear cache
            _settingService.ClearCache();

            this.Settings = settings;
        }

        // codehint: sm-add
        private void SaveSettingsJson(TSettings settings)
        {
            Type t = typeof(TSettings);
            string key = t.Namespace + "." + t.Name;

            var rawSettings = JsonConvert.SerializeObject(settings);
            _settingService.SetSetting(key, rawSettings, false);

            // and now clear cache
            _settingService.ClearCache();

            this.Settings = settings;
        }

        public void DeleteSettings()
        {
            // codehint: sm-add
            if (typeof(TSettings).HasAttribute<JsonPersistAttribute>(true))
            {
                DeleteSettingsJson();
                return;
            }
            
            var properties = from prop in typeof(TSettings).GetProperties()
                             select prop;

            var settingList = new List<Setting>();
            foreach (var prop in properties)
            {
                string key = typeof(TSettings).Name + "." + prop.Name;
                var setting = _settingService.GetSettingByKey(key);
                if (setting != null)
                    settingList.Add(setting);
            }

            foreach (var setting in settingList)
                _settingService.DeleteSetting(setting);
        }

        // codehint: sm-add
        private void DeleteSettingsJson()
        {
            Type t = typeof(TSettings);
            string key = t.Namespace + "." + t.Name;

            var setting = _settingService.GetSettingByKey(key);
            if (setting != null)
            {
                _settingService.DeleteSetting(setting);
            }
        }
    }
}
