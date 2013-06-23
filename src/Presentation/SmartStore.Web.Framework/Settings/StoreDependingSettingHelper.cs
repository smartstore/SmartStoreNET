using System.Linq;
using System.Web.Mvc;
using SmartStore.Services.Configuration;
using Fasterflect;
using System.Collections.Generic;

namespace SmartStore.Web.Framework.Settings
{
	/// <remarks>codehint: sm-add</remarks>
	public class StoreDependingSettingHelper
	{
		private ViewDataDictionary _viewData;

		public StoreDependingSettingHelper(ViewDataDictionary viewData)
		{
			_viewData = viewData;
		}

		public StoreDependingSettingData Data
		{
			get
			{
				return _viewData["StoreDependingSettingData"] as StoreDependingSettingData;
			}
		}

		private bool? IsOverrideChecked(string settingKey, FormCollection form)
		{
			var rawOverride = form.AllKeys.FirstOrDefault(k => k.IsCaseInsensitiveEqual(settingKey + "_OverrideForStore"));

			if (rawOverride.HasValue())
				return rawOverride.ToLower().Contains("true");

			return null;
		}
		public bool? IsOverrideChecked(object settings, string name, FormCollection form)
		{
			var key = settings.GetType().Name + "." + name;
			return IsOverrideChecked(key, form);
		}
		public void AddOverrideKey(object settings, string name)
		{
			var key = settings.GetType().Name + "." + name;
			Data.OverrideSettingKeys.Add(key);
		}

		public void GetOverrideKeys(object settings, object model, int storeId, ISettingService settingService, bool isRootModel = true)
		{
			if (storeId <= 0)
				return;		// single store mode -> there are no overrides

			var data = Data;
			if (data == null)
				data = new StoreDependingSettingData();

			var settingName = settings.GetType().Name;
			var properties = settings.GetType().GetProperties();

			foreach (var prop in properties)
			{
				var name = prop.Name;
				var modelProperty = model.GetType().GetProperty(name);

				if (modelProperty == null)
					continue;	// setting is not configurable or missing or whatever... however we don't need the override info

				var key = settingName + "." + name;
				var setting = settingService.GetSettingByKey<string>(key, storeId: storeId);

				if (setting != null)
					data.OverrideSettingKeys.Add(key);
			}

			if (isRootModel)
			{
				data.ActiveStoreScopeConfiguration = storeId;
				data.RootSettingClass = settingName;

				_viewData["StoreDependingSettingData"] = data;
			}
		}
		public void UpdateSettings(object settings, FormCollection form, int storeId, ISettingService settingService)
		{
			var settingName = settings.GetType().Name;
			var properties = settings.GetType().GetProperties();

			foreach (var prop in properties)
			{
				var name = prop.Name;
				var key = settingName + "." + name;
				bool? doOverride = IsOverrideChecked(key, form);

				if (doOverride.HasValue)	// false cases: setting is not store dependend, pseudo-override that controlls multiple settings
				{
					dynamic value = settings.TryGetPropertyValue(name);

					if (doOverride.Value || storeId == 0)
						settingService.SetSetting(key, value == null ? "" : value, storeId, false);
					else if (storeId > 0)
						settingService.DeleteSetting(settings, key, storeId);
				}
			}
		}
	}
}
