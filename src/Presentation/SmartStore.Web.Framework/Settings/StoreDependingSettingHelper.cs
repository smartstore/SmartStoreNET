using System;
using System.Linq;
using System.Web.Mvc;
using SmartStore.ComponentModel;
using SmartStore.Core.Infrastructure;
using SmartStore.Services.Configuration;
using SmartStore.Services.Localization;
using SmartStore.Web.Framework.Localization;

namespace SmartStore.Web.Framework.Settings
{
	public class StoreDependingSettingHelper
	{
		private ViewDataDictionary _viewData;

		public StoreDependingSettingHelper(ViewDataDictionary viewData)
		{
			_viewData = viewData;
		}

		public static string ViewDataKey { get { return "StoreDependingSettingData"; } }

		public StoreDependingSettingData Data
		{
			get
			{
				return _viewData[ViewDataKey] as StoreDependingSettingData;
			}
		}

		private bool IsOverrideChecked(string settingKey, FormCollection form)
		{
			var rawOverrideKey = form.AllKeys.FirstOrDefault(k => k.IsCaseInsensitiveEqual(settingKey + "_OverrideForStore"));

			if (rawOverrideKey.HasValue())
			{
				var checkboxValue = form[rawOverrideKey].EmptyNull().ToLower();
				return checkboxValue.Contains("on") || checkboxValue.Contains("true");
			}
			return false;
		}

		public bool IsOverrideChecked(object settings, string name, FormCollection form)
		{
			var key = settings.GetType().Name + "." + name;
			return IsOverrideChecked(key, form);
		}

		public void AddOverrideKey(object settings, string name)
		{
			var key = settings.GetType().Name + "." + name;
			Data.OverrideSettingKeys.Add(key);
		}

		public void CreateViewDataObject(int activeStoreScopeConfiguration, string rootSettingClass = null)
		{
			_viewData[ViewDataKey] = new StoreDependingSettingData()
			{
				ActiveStoreScopeConfiguration = activeStoreScopeConfiguration,
				RootSettingClass = rootSettingClass
			};
		}

        public void GetOverrideKeys(object settings, object model, int storeId, ISettingService settingService, bool isRootModel = true, ILocalizedModelLocal localized = null, int? index = null)
        {
            if (storeId <= 0)
                return;		// single store mode -> there are no overrides

            var data = Data;
            if (data == null)
                data = new StoreDependingSettingData();

            var settingName = settings.GetType().Name;
            var properties = settings.GetType().GetProperties();
            var localizedEntityService = EngineContext.Current.Resolve<ILocalizedEntityService>();

            var modelType = model.GetType();

            foreach (var prop in properties)
            {
                var name = prop.Name;
                var modelProperty = modelType.GetProperty(name);

                if (modelProperty == null)
                    continue;	// setting is not configurable or missing or whatever... however we don't need the override info

                var key = String.Empty;
                var setting = String.Empty;

                if (localized == null)
                {
                    key = settingName + "." + name;
                    setting = settingService.GetSettingByKey<string>(key, storeId: storeId);
                }
                else
                {
                    key = "Locales[" + index.ToString() + "]." + name;
                    setting = localizedEntityService.GetLocalizedValue(localized.LanguageId, 0, settingName, name);
                }

                if (!String.IsNullOrEmpty(setting))
                    data.OverrideSettingKeys.Add(key);
            }

            if (isRootModel)
            {
                data.ActiveStoreScopeConfiguration = storeId;
                data.RootSettingClass = settingName;

                _viewData[ViewDataKey] = data;
            }
        }

        public void UpdateSettings(object settings, FormCollection form, int storeId, ISettingService settingService, ILocalizedModelLocal localized = null)
        {
            var settingName = settings.GetType().Name;
            var properties = FastProperty.GetProperties(localized == null ? settings.GetType() : localized.GetType()).Values;

			using (settingService.BeginScope())
			{
				foreach (var prop in properties)
				{
					var name = prop.Name;
					var key = settingName + "." + name;

					if (storeId == 0 || IsOverrideChecked(key, form))
					{
						dynamic value = prop.GetValue(localized == null ? settings : localized);
						settingService.SetSetting(key, value == null ? "" : value, storeId, false);
					}
					else if (storeId > 0)
					{
						settingService.DeleteSetting(key, storeId);
					}
				}
			}
        }
	}
}
