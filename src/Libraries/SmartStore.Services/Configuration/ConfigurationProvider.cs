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
			this.LoadSettings(0);
        }

		public TSettings Settings { get; protected set; }

		public void LoadSettings(int storeId)
        {
            // codehint: sm-add
            if (typeof(TSettings).HasAttribute<JsonPersistAttribute>(true))
            {
                BuildConfigurationJson(storeId);
                return;
            }

            Settings = (TSettings)typeof(TSettings).CreateInstance();

			foreach (var prop in typeof(TSettings).GetProperties())
			{
				// get properties we can read and write to
				if (!prop.CanRead || !prop.CanWrite)
					continue;

				var key = typeof(TSettings).Name + "." + prop.Name;
				//load by store
				string setting = _settingService.GetSettingByKey<string>(key, storeId: storeId);
				if (setting == null && storeId > 0)
				{
					//load for all stores if not found
					setting = _settingService.GetSettingByKey<string>(key, storeId: 0);
				}

				if (setting == null)
					continue;

				if (!CommonHelper.GetCustomTypeConverter(prop.PropertyType).CanConvertFrom(typeof(string)))
					continue;

				if (!CommonHelper.GetCustomTypeConverter(prop.PropertyType).IsValid(setting))
					continue;

				object value = CommonHelper.GetCustomTypeConverter(prop.PropertyType).ConvertFromInvariantString(setting);

				//set property
				prop.SetValue(Settings, value, null);
			}
        }

        // codehint: sm-add
		private void BuildConfigurationJson(int storeId)
        {
            Type t = typeof(TSettings);
            string key = t.Namespace + "." + t.Name;

            this.Settings = Activator.CreateInstance<TSettings>();

            var rawSetting = _settingService.GetSettingByKey<string>(key, storeId: storeId);
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
				var storeId = 0;
                // Duck typing is not supported in C#. That's why we're using dynamic type
                dynamic value = settings.TryGetPropertyValue(prop.Name);

                _settingService.SetSetting(key, value ?? "", storeId, false);
   
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
			var storeId = 0;

            var rawSettings = JsonConvert.SerializeObject(settings);
            _settingService.SetSetting(key, rawSettings, storeId, false);

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

			var settingsToDelete = new List<Setting>();
			var allSettings = _settingService.GetAllSettings();
			foreach (var prop in properties)
			{
				string key = typeof(TSettings).Name + "." + prop.Name;
				settingsToDelete.AddRange(allSettings.Where(x => x.Name.Equals(key, StringComparison.InvariantCultureIgnoreCase)));
			}

			foreach (var setting in settingsToDelete)
				_settingService.DeleteSetting(setting);
        }

        // codehint: sm-add
        private void DeleteSettingsJson()
        {
            Type t = typeof(TSettings);
            string key = t.Namespace + "." + t.Name;

			var setting = _settingService.GetAllSettings()
				.FirstOrDefault(x => x.Name.Equals(key, StringComparison.InvariantCultureIgnoreCase));

			if (setting != null)
			{
				_settingService.DeleteSetting(setting);
			}
        }
    }
}
