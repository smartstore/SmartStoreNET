using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using SmartStore.ComponentModel;
using SmartStore.Core.Caching;
using SmartStore.Core.Configuration;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Configuration;
using SmartStore.Core.Events;
using SmartStore.Core.Logging;

namespace SmartStore.Services.Configuration
{
    public partial class SettingService : ScopedServiceBase, ISettingService
    {
        private const string SETTINGS_ALL_KEY = "setting:all";

        private readonly IRepository<Setting> _settingRepository;
        private readonly ICacheManager _cacheManager;

        public SettingService(ICacheManager cacheManager, IRepository<Setting> settingRepository)
        {
            _cacheManager = cacheManager;
            _settingRepository = settingRepository;
        }

        public ILogger Logger { get; set; } = NullLogger.Instance;

        protected virtual IDictionary<string, CachedSetting> GetAllCachedSettings()
        {
            return _cacheManager.Get(SETTINGS_ALL_KEY, () =>
            {
                var query = from s in _settingRepository.TableUntracked
                            orderby s.Name, s.StoreId
                            select s;

                var settings = query.ToList();
                var dictionary = new Dictionary<string, CachedSetting>(StringComparer.OrdinalIgnoreCase);
                foreach (var s in settings)
                {
                    var settingKey = CreateCacheKey(s.Name, s.StoreId);

                    var cachedSetting = new CachedSetting
                    {
                        Id = s.Id,
                        Name = s.Name,
                        Value = s.Value,
                        StoreId = s.StoreId
                    };

                    dictionary[settingKey] = cachedSetting;
                }

                return dictionary;
            }, independent: true, allowRecursion: true);
        }

        protected virtual PropertyInfo GetPropertyInfo<T, TPropType>(Expression<Func<T, TPropType>> keySelector)
        {
            var member = keySelector.Body as MemberExpression;
            if (member == null)
            {
                throw new ArgumentException($"Expression '{keySelector}' refers to a method, not a property.");
            }

            var propInfo = member.Member as PropertyInfo;
            if (propInfo == null)
            {
                throw new ArgumentException($"Expression '{keySelector}' refers to a field, not a property.");
            }

            return propInfo;
        }

        public virtual void InsertSetting(Setting setting, bool clearCache = true)
        {
            Guard.NotNull(setting, nameof(setting));

            _settingRepository.Insert(setting);

            HasChanges = true;

            if (clearCache)
                ClearCache();
        }

        public virtual void UpdateSetting(Setting setting, bool clearCache = true)
        {
            Guard.NotNull(setting, nameof(setting));

            _settingRepository.Update(setting);

            HasChanges = true;

            if (clearCache)
                ClearCache();
        }

        private ISettings LoadSettingsJson(Type settingType, int storeId = 0)
        {
            string key = settingType.Namespace + "." + settingType.Name;

            var settings = (ISettings)Activator.CreateInstance(settingType);

            var rawSetting = GetSettingByKey<string>(key, storeId: storeId, loadSharedValueIfNotFound: true);
            if (rawSetting.HasValue())
            {
                JsonConvert.PopulateObject(rawSetting, settings);
            }

            return settings;
        }

        private void SaveSettingsJson(ISettings settings)
        {
            Type t = settings.GetType();
            string key = t.Namespace + "." + t.Name;
            var storeId = 0;

            var rawSettings = JsonConvert.SerializeObject(settings);
            SetSetting(key, rawSettings, storeId, true);
        }

        private void DeleteSettingsJson<T>()
        {
            Type t = typeof(T);
            string key = t.Namespace + "." + t.Name;

            var setting = GetAllSettings()
                .FirstOrDefault(x => x.Name.Equals(key, StringComparison.InvariantCultureIgnoreCase));

            if (setting != null)
            {
                DeleteSetting(setting);
            }
        }


        public virtual Setting GetSettingById(int settingId)
        {
            if (settingId == 0)
                return null;

            var setting = _settingRepository.GetById(settingId);
            return setting;
        }

        public virtual Setting GetSettingEntityByKey(string key, int storeId = 0)
        {
            Guard.NotEmpty(key, nameof(key));

            var query = _settingRepository.Table.Where(x => x.Name == key);

            if (storeId > 0)
            {
                query = query.Where(x => x.StoreId == storeId || x.StoreId == 0).OrderByDescending(x => x.StoreId);
            }
            else
            {
                query = query.Where(x => x.StoreId == 0);
            }

            return query.FirstOrDefault();
        }

        public virtual T GetSettingByKey<T>(
            string key,
            T defaultValue = default,
            int storeId = 0,
            bool loadSharedValueIfNotFound = false)
        {
            Guard.NotEmpty(key, nameof(key));

            var settings = GetAllCachedSettings();

            var cacheKey = CreateCacheKey(key, storeId);

            if (settings.TryGetValue(cacheKey, out CachedSetting cachedSetting))
            {
                return cachedSetting.Value.Convert<T>();
            }

            // fallback to shared (storeId = 0) if desired
            if (storeId > 0 && loadSharedValueIfNotFound)
            {
                cacheKey = CreateCacheKey(key, 0);
                if (settings.TryGetValue(cacheKey, out cachedSetting))
                {
                    return cachedSetting.Value.Convert<T>();
                }
            }

            return defaultValue;
        }

        public virtual IList<Setting> GetAllSettings()
        {
            var query = from s in _settingRepository.Table
                        orderby s.Name, s.StoreId
                        select s;
            var settings = query.ToList();
            return settings;
        }

        public virtual bool SettingExists<T, TPropType>(
            T settings,
            Expression<Func<T, TPropType>> keySelector,
            int storeId = 0)
            where T : ISettings, new()
        {
            var propInfo = GetPropertyInfo(keySelector);
            var key = string.Concat(typeof(T).Name, ".", propInfo.Name);

            string setting = GetSettingByKey<string>(key, storeId: storeId);
            return setting != null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T LoadSetting<T>(int storeId = 0) where T : ISettings, new()
        {
            return (T)LoadSettingCore(typeof(T), storeId);
        }

        public ISettings LoadSetting(Type settingType, int storeId = 0)
        {
            Guard.NotNull(settingType, nameof(settingType));
            Guard.HasDefaultConstructor(settingType);

            if (!typeof(ISettings).IsAssignableFrom(settingType))
            {
                throw new ArgumentException($"The type to load settings for must be a subclass of the '{typeof(ISettings).FullName}' interface", nameof(settingType));
            }

            return LoadSettingCore(settingType, storeId);
        }

        protected virtual ISettings LoadSettingCore(Type settingType, int storeId = 0)
        {
            var allSettings = GetAllCachedSettings();
            var instance = (ISettings)Activator.CreateInstance(settingType);
            var prefix = settingType.Name;

            foreach (var fastProp in FastProperty.GetProperties(settingType).Values)
            {
                var prop = fastProp.Property;

                // Get properties we can read and write to
                if (!prop.CanWrite)
                    continue;

                string key = prefix + "." + prop.Name;

                if (!allSettings.TryGetValue(CreateCacheKey(key, storeId), out var cachedSetting) && storeId > 0)
                {
                    // Fallback to shared (storeId = 0)
                    allSettings.TryGetValue(CreateCacheKey(key, 0), out cachedSetting);
                }

                string setting = cachedSetting?.Value;

                if (setting == null)
                {
                    if (fastProp.IsSequenceType)
                    {
                        if ((fastProp.GetValue(instance) as IEnumerable) != null)
                        {
                            // Instance of IEnumerable<> was already created, most likely in the constructor of the settings concrete class.
                            // In this case we shouldn't let the EnumerableConverter create a new instance but keep this one.
                            continue;
                        }
                    }
                    else
                    {
                        continue;
                    }

                }

                var converter = TypeConverterFactory.GetConverter(prop.PropertyType);

                if (converter == null || !converter.CanConvertFrom(typeof(string)))
                    continue;

                try
                {
                    object value = converter.ConvertFrom(setting);

                    // Set property
                    fastProp.SetValue(instance, value);
                }
                catch (Exception ex)
                {
                    var msg = "Could not convert setting '{0}' to type '{1}'".FormatInvariant(key, prop.PropertyType.Name);
                    Logger.Error(ex, msg);
                }
            }

            return instance;
        }

        public virtual SaveSettingResult SetSetting<T>(string key, T value, int storeId = 0, bool clearCache = true)
        {
            Guard.NotEmpty(key, nameof(key));

            var str = value.Convert<string>();
            var allSettings = GetAllCachedSettings();
            var cacheKey = CreateCacheKey(key, storeId);
            var insert = false;

            if (allSettings.TryGetValue(cacheKey, out CachedSetting cachedSetting))
            {
                var setting = GetSettingById(cachedSetting.Id);
                if (setting != null)
                {
                    // Update
                    if (setting.Value != str)
                    {
                        setting.Value = str;
                        UpdateSetting(setting, clearCache);
                        return SaveSettingResult.Modified;
                    }
                }
                else
                {
                    insert = true;
                }
            }
            else
            {
                insert = true;
            }

            if (insert)
            {
                // Insert
                var setting = new Setting
                {
                    Name = key.ToLowerInvariant(),
                    Value = str,
                    StoreId = storeId
                };
                InsertSetting(setting, clearCache);
                return SaveSettingResult.Inserted;
            }

            return SaveSettingResult.Unchanged;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool SaveSetting<T>(T settings, int storeId = 0) where T : ISettings, new()
        {
            Guard.NotNull(settings, nameof(settings));

            using (BeginScope(clearCache: true))
            {
                var hasChanges = false;
                var modifiedProperties = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                var settingType = typeof(T);
                var prefix = settingType.Name;

                /* We do not clear cache after each setting update.
				 * This behavior can increase performance because cached settings will not be cleared 
				 * and loaded from database after each update */
                foreach (var prop in FastProperty.GetProperties(settingType).Values)
                {
                    // get properties we can read and write to
                    if (!prop.IsPublicSettable)
                        continue;

                    var converter = TypeConverterFactory.GetConverter(prop.Property.PropertyType);
                    if (converter == null || !converter.CanConvertFrom(typeof(string)))
                        continue;

                    string key = prefix + "." + prop.Name;
                    // Duck typing is not supported in C#. That's why we're using dynamic type
                    dynamic currentValue = prop.GetValue(settings);

                    if (SetSetting(key, currentValue ?? "", storeId, false) > SaveSettingResult.Unchanged)
                    {
                        hasChanges = true;
                    }
                }

                return hasChanges;
            }
        }

        public virtual SaveSettingResult SaveSetting<T, TPropType>(
            T settings,
            Expression<Func<T, TPropType>> keySelector,
            int storeId = 0,
            bool clearCache = true) where T : ISettings, new()
        {
            var propInfo = GetPropertyInfo(keySelector);
            var key = string.Concat(typeof(T).Name, ".", propInfo.Name);

            // Duck typing is not supported in C#. That's why we're using dynamic type.
            var fastProp = FastProperty.GetProperty(propInfo, PropertyCachingStrategy.EagerCached);
            dynamic currentValue = fastProp.GetValue(settings);

            return SetSetting(key, currentValue ?? "", storeId, clearCache);
        }

        public virtual SaveSettingResult UpdateSetting<T, TPropType>(
            T settings,
            Expression<Func<T, TPropType>> keySelector,
            bool overrideForStore,
            int storeId = 0) where T : ISettings, new()
        {
            if (overrideForStore || storeId == 0)
            {
                return SaveSetting(settings, keySelector, storeId, false);
            }
            else if (storeId > 0 && DeleteSetting(settings, keySelector, storeId))
            {
                return SaveSettingResult.Deleted;
            }

            return SaveSettingResult.Unchanged;
        }

        public virtual void DeleteSetting(Setting setting)
        {
            if (setting == null)
                throw new ArgumentNullException("setting");

            _settingRepository.Delete(setting);

            HasChanges = true;

            ClearCache();
        }

        public virtual void DeleteSetting<T>() where T : ISettings, new()
        {
            using (BeginScope())
            {
                var settingsToDelete = new List<Setting>();
                var allSettings = GetAllSettings();
                foreach (var prop in typeof(T).GetProperties())
                {
                    string key = typeof(T).Name + "." + prop.Name;
                    settingsToDelete.AddRange(allSettings.Where(x => x.Name.Equals(key, StringComparison.InvariantCultureIgnoreCase)));
                }

                foreach (var setting in settingsToDelete)
                {
                    DeleteSetting(setting);
                }
            }
        }

        public virtual bool DeleteSetting<T, TPropType>(
            T settings,
            Expression<Func<T, TPropType>> keySelector,
            int storeId = 0) where T : ISettings, new()
        {
            var propInfo = GetPropertyInfo(keySelector);
            var key = string.Concat(typeof(T).Name, ".", propInfo.Name);

            return DeleteSetting(key, storeId);
        }

        public virtual bool DeleteSetting(string key, int storeId = 0)
        {
            if (key.HasValue())
            {
                key = key.Trim().ToLowerInvariant();

                var setting = (
                    from s in _settingRepository.Table
                    where s.StoreId == storeId && s.Name == key
                    select s).FirstOrDefault();

                if (setting != null)
                {
                    DeleteSetting(setting);
                    return true;
                }
            }

            return false;
        }

        public virtual int DeleteSettings(string rootKey)
        {
            int result = 0;

            if (rootKey.IsEmpty())
                return 0;

            try
            {
                string sqlDelete = "DELETE FROM [Setting] WHERE [Name] LIKE '{0}%'".FormatWith(rootKey.EndsWith(".") ? rootKey : rootKey + ".");
                result = _settingRepository.Context.ExecuteSqlCommand(sqlDelete);

                if (result > 0)
                    HasChanges = true;

                ClearCache();
            }
            catch (Exception exc)
            {
                exc.Dump();
            }

            return result;
        }

        protected override void OnClearCache()
        {
            _cacheManager.Remove(SETTINGS_ALL_KEY);
        }

        protected string CreateCacheKey(string name, int storeId)
        {
            return name.Trim().ToLowerInvariant() + "/" + storeId.ToString();
        }
    }

    [Serializable]
    public class CachedSetting
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public int StoreId { get; set; }
    }
}