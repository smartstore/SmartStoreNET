using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        public static string ViewDataKey => "StoreDependingSettingData";

        public StoreDependingSettingData Data => _viewData[ViewDataKey] as StoreDependingSettingData;

        public bool IsOverrideChecked(object settings, string name, FormCollection form)
        {
            var key = settings.GetType().Name + "." + name;
            return IsOverrideChecked(key, form);
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

        public void AddOverrideKey(object settings, string name)
        {
            if (Data == null)
            {
                throw new SmartException("You must call GetOverrideKeys or CreateViewDataObject before AddOverrideKey.");
            }

            var key = settings.GetType().Name + "." + name;
            Data.OverrideSettingKeys.Add(key);
        }

        public void CreateViewDataObject(int activeStoreScopeConfiguration, string rootSettingClass = null)
        {
            _viewData[ViewDataKey] = new StoreDependingSettingData
            {
                ActiveStoreScopeConfiguration = activeStoreScopeConfiguration,
                RootSettingClass = rootSettingClass
            };
        }

        public void GetOverrideKeys(
            object settings,
            object model,
            int storeId,
            ISettingService settingService,
            bool isRootModel = true,
            Func<string, string> propertyNameMapper = null)
        {
            GetOverrideKeysInternal(settings, model, storeId, settingService, isRootModel, propertyNameMapper, null);
        }

        private void GetOverrideKeysInternal(
            object settings,
            object model,
            int storeId,
            ISettingService settingService,
            bool isRootModel,
            Func<string, string> propertyNameMapper,
            int? localeIndex)
        {
            if (storeId <= 0)
            {
                // Single store mode -> there are no overrides.
                return;
            }

            var data = Data ?? new StoreDependingSettingData();
            var settingType = settings.GetType();
            var modelType = model.GetType();
            var settingName = settingType.Name;
            var modelProperties = modelType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var localizedModelLocal = model as ILocalizedModelLocal;
            var localizedEntityService = EngineContext.Current.Resolve<ILocalizedEntityService>();

            foreach (var prop in modelProperties)
            {
                var name = propertyNameMapper?.Invoke(prop.Name) ?? prop.Name;

                var settingProperty = settingType.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);

                if (settingProperty == null)
                {
                    // Setting is not configurable or missing or whatever... however we don't need the override info.
                    continue;
                }

                string key = null;

                if (localizedModelLocal == null)
                {
                    key = settingName + "." + name;
                    if (settingService.GetSettingByKey<string>(key, storeId: storeId) == null)
                    {
                        key = null;
                    }
                }
                else if (localeIndex.HasValue)
                {
                    var value = localizedEntityService.GetLocalizedValue(localizedModelLocal.LanguageId, storeId, settingName, name);
                    if (!string.IsNullOrEmpty(value))
                    {
                        key = settingName + "." + "Locales[" + localeIndex.ToString() + "]." + name;
                    }
                }

                if (key != null)
                {
                    data.OverrideSettingKeys.Add(key);
                }
            }

            if (isRootModel)
            {
                data.ActiveStoreScopeConfiguration = storeId;
                data.RootSettingClass = settingName;

                _viewData[ViewDataKey] = data;
            }

            if (model is ILocalizedModel)
            {
                var localesProperty = modelType.GetProperty("Locales", BindingFlags.Public | BindingFlags.Instance);
                if (localesProperty != null)
                {
                    if (localesProperty.GetValue(model) is IEnumerable<ILocalizedModelLocal> locales)
                    {
                        int i = 0;
                        foreach (var locale in locales)
                        {
                            GetOverrideKeysInternal(settings, locale, storeId, settingService, false, propertyNameMapper, i);
                            i++;
                        }
                    }
                }
            }
        }

        public void GetOverrideKey(string formKey, string settingName, object settings, int storeId, ISettingService settingService)
        {
            if (storeId <= 0)
            {
                // Single store mode -> there are no overrides.
                return;
            }

            var key = formKey;
            if (settingService.GetSettingByKey<string>(string.Concat(settings.GetType().Name, ".", settingName), storeId: storeId) == null)
            {
                key = null;
            }

            if (key != null)
            {
                var data = Data ?? new StoreDependingSettingData();
                data.OverrideSettingKeys.Add(key);
            }
        }

        /// <summary>
        /// Updates settings for a store.
        /// </summary>
        /// <param name="settings">Settings class instance.</param>
        /// <param name="form">Form value collection.</param>
        /// <param name="storeId">Store identifier.</param>
        /// <param name="settingService">Setting service.</param>
        /// <param name="propertyNameMapper">Function to map property names. Return <c>null</c> to skip a property.</param>
        public void UpdateSettings(
            object settings,
            FormCollection form,
            int storeId,
            ISettingService settingService,
            Func<string, string> propertyNameMapper = null)
        {
            var settingType = settings.GetType();
            var settingName = settingType.Name;
            var settingProperties = FastProperty.GetProperties(settingType).Values;

            foreach (var prop in settingProperties)
            {
                var name = propertyNameMapper?.Invoke(prop.Name) ?? prop.Name;

                if (string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                var key = string.Concat(settingName, ".", name);

                if (storeId == 0 || IsOverrideChecked(key, form))
                {
                    dynamic value = prop.GetValue(settings);
                    settingService.SetSetting(key, value ?? string.Empty, storeId, false);
                }
                else if (storeId > 0)
                {
                    settingService.DeleteSetting(key, storeId);
                }
            }
        }

        public void UpdateSetting(
            string formKey,
            string settingName,
            object settings,
            FormCollection form,
            int storeId,
            ISettingService settingService)
        {
            var settingType = settings.GetType();

            if (storeId == 0 || IsOverrideChecked(formKey, form))
            {
                var prop = FastProperty.GetProperty(settingType, settingName);
                if (prop != null)
                {
                    dynamic value = prop.GetValue(settings);
                    var key = string.Concat(settingType.Name, ".", settingName);
                    settingService.SetSetting(key, value ?? string.Empty, storeId, false);
                }
            }
            else if (storeId > 0)
            {
                var key = string.Concat(settingType.Name, ".", settingName);
                settingService.DeleteSetting(key, storeId);
            }
        }
    }
}
