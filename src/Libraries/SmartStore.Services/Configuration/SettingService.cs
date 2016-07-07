using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using SmartStore.Core.Caching;
using SmartStore.Core.Configuration;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Configuration;
using SmartStore.Core.Events;
using System.Linq.Expressions;
using System.Reflection;
using SmartStore.ComponentModel;
using System.Collections;
using SmartStore.Utilities;

namespace SmartStore.Services.Configuration
{
    public partial class SettingService : ISettingService
    {
        private const string SETTINGS_ALL_KEY = "setting:all";

        private readonly IRepository<Setting> _settingRepository;
        private readonly IEventPublisher _eventPublisher;
        private readonly ICacheManager _cacheManager;

		private bool _isInBatch;
		private bool _hasChanges;

        public SettingService(ICacheManager cacheManager, IEventPublisher eventPublisher, IRepository<Setting> settingRepository)
        {
            this._cacheManager = cacheManager;
            this._eventPublisher = eventPublisher;
            this._settingRepository = settingRepository;
        }

		protected virtual IDictionary<string, CachedSetting> GetAllCachedSettings()
		{
			string key = string.Format(SETTINGS_ALL_KEY);
			return _cacheManager.Get(key, () =>
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
			});
		}

		public IDisposable BeginBatch(bool clearCache = true)
		{
			if (_isInBatch)
			{
				// nested batches are not supported
				return ActionDisposable.Empty;
			}	

			_isInBatch = true;

			return new ActionDisposable(() => 
			{
				_isInBatch = false;
				if (clearCache && _hasChanges)
				{
					ClearCache();
				}
			});
		}

		public bool HasChanges
		{
			get { return _hasChanges; }
		}

		public virtual void InsertSetting(Setting setting, bool clearCache = true)
        {
			Guard.ArgumentNotNull(() => setting);

			_settingRepository.Insert(setting);

			_hasChanges = true;

			if (clearCache)
				ClearCache();

            _eventPublisher.EntityInserted(setting);
        }

        public virtual void UpdateSetting(Setting setting, bool clearCache = true)
        {
			Guard.ArgumentNotNull(() => setting);

            _settingRepository.Update(setting);

			_hasChanges = true;

			if (clearCache)
				ClearCache();

            _eventPublisher.EntityUpdated(setting);
        }

		private T LoadSettingsJson<T>(int storeId = 0)
		{
			Type t = typeof(T);
			string key = t.Namespace + "." + t.Name;

			T settings = Activator.CreateInstance<T>();

			var rawSetting = GetSettingByKey<string>(key, storeId: storeId, loadSharedValueIfNotFound: true);
			if (rawSetting.HasValue())
			{
				JsonConvert.PopulateObject(rawSetting, settings);
			}

			return settings;
		}

		private void SaveSettingsJson<T>(T settings)
		{
			Type t = typeof(T);
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

		public virtual T GetSettingByKey<T>(
			string key, 
			T defaultValue = default(T), 
			int storeId = 0, 
			bool loadSharedValueIfNotFound = false)
        {
			Guard.ArgumentNotEmpty(() => key);

			var settings = GetAllCachedSettings();

			var cacheKey = CreateCacheKey(key, storeId);

			CachedSetting cachedSetting;

			if (settings.TryGetValue(cacheKey, out cachedSetting))
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
			var member = keySelector.Body as MemberExpression;
			if (member == null)
			{
				throw new ArgumentException(string.Format(
					"Expression '{0}' refers to a method, not a property.",
					keySelector));
			}

			var propInfo = member.Member as PropertyInfo;
			if (propInfo == null)
			{
				throw new ArgumentException(string.Format(
					   "Expression '{0}' refers to a field, not a property.",
					   keySelector));
			}

			string key = typeof(T).Name + "." + propInfo.Name;

			string setting = GetSettingByKey<string>(key, storeId: storeId);
			return setting != null;
		}

		public virtual T LoadSetting<T>(int storeId = 0) where T : ISettings, new()
		{
			if (typeof(T).HasAttribute<JsonPersistAttribute>(true))
			{
				return LoadSettingsJson<T>(storeId);
			}

			var settings = Activator.CreateInstance<T>();

			var prefix = typeof(T).Name;

			if (storeId == 0)
			{
				
			}

			foreach (var fastProp in FastProperty.GetProperties(typeof(T)).Values)
			{
				var prop = fastProp.Property;

				// get properties we can read and write to
				if (!prop.CanWrite)
					continue;

				var key = prefix + "." + prop.Name;
				// load by store
				string setting = GetSettingByKey<string>(key, storeId: storeId, loadSharedValueIfNotFound: true);

				if (setting == null)
				{
					if (fastProp.IsSequenceType)
                    {
						if ((fastProp.GetValue(settings) as IEnumerable) != null)
                        {
                            // Instance of IEnumerable<> was already created, most likely in the constructor of the settings concrete class.
                            // In this case we shouldn't let the EnumerableConverter create a new instance but keep this one.
                            continue;
                        }
                    }
                    else
                    {
                        #region Obsolete ('EnumerableConverter' can handle this case now)
                        //if (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
                        //{
                        //	// convenience: don't return null for simple list types
                        //	var listArg = prop.PropertyType.GetGenericArguments()[0];
                        //	object list = null;

                        //	if (listArg == typeof(int))
                        //		list = new List<int>();
                        //	else if (listArg == typeof(decimal))
                        //		list = new List<decimal>();
                        //	else if (listArg == typeof(string))
                        //		list = new List<string>();

                        //	if (list != null)
                        //	{
                        //		fastProp.SetValue(settings, list);
                        //	}
                        //}
                        #endregion

                        continue;
                    }

				}

				var converter = TypeConverterFactory.GetConverter(prop.PropertyType);

                if (converter == null || !converter.CanConvertFrom(typeof(string)))
					continue;

                object value = converter.ConvertFrom(setting);

				//set property
				fastProp.SetValue(settings, value);
			}

			return settings;
		}

		public virtual void SetSetting<T>(string key, T value, int storeId = 0, bool clearCache = true)
        {
            Guard.ArgumentNotEmpty(() => key);

			var str = value.Convert<string>();

			var allSettings = GetAllCachedSettings();

			var cacheKey = CreateCacheKey(key, storeId);
			CachedSetting cachedSetting;

			if (allSettings.TryGetValue(cacheKey, out cachedSetting))
			{
				// Update
				var setting = GetSettingById(cachedSetting.Id);
				if (setting != null && setting.Value != str)
				{
					setting.Value = str;
					UpdateSetting(setting, clearCache);
					_hasChanges = true;
				}
			}
			else
			{
				// Insert
				var setting = new Setting
				{
					Name = key.ToLowerInvariant(),
					Value = str,
					StoreId = storeId
				};
				InsertSetting(setting, clearCache);
				_hasChanges = true;
			}
        }

		public virtual void SaveSetting<T>(T settings, int storeId = 0) where T : ISettings, new()
        {
			using (BeginBatch())
			{
				if (typeof(T).HasAttribute<JsonPersistAttribute>(true))
				{
					SaveSettingsJson<T>(settings);
					return;
				}

				/* We do not clear cache after each setting update.
				 * This behavior can increase performance because cached settings will not be cleared 
				 * and loaded from database after each update */
				foreach (var prop in FastProperty.GetProperties(typeof(T)).Values)
				{
					// get properties we can read and write to
					if (!prop.IsPublicSettable)
						continue;

					var converter = TypeConverterFactory.GetConverter(prop.Property.PropertyType);
					if (converter == null || !converter.CanConvertFrom(typeof(string)))
						continue;

					string key = typeof(T).Name + "." + prop.Name;
					// Duck typing is not supported in C#. That's why we're using dynamic type
					dynamic value = prop.GetValue(settings);

					SetSetting(key, value ?? "", storeId, false);
				}
			}
        }

		public virtual void SaveSetting<T, TPropType>(
			T settings,
			Expression<Func<T, TPropType>> keySelector,
			int storeId = 0, 
			bool clearCache = true) where T : ISettings, new()
		{
			var member = keySelector.Body as MemberExpression;
			if (member == null)
			{
				throw new ArgumentException(string.Format(
					"Expression '{0}' refers to a method, not a property.",
					keySelector));
			}

			var propInfo = member.Member as PropertyInfo;
			if (propInfo == null)
			{
				throw new ArgumentException(string.Format(
					   "Expression '{0}' refers to a field, not a property.",
					   keySelector));
			}

			string key = typeof(T).Name + "." + propInfo.Name;
			// Duck typing is not supported in C#. That's why we're using dynamic type
			var fastProp = FastProperty.GetProperty(propInfo, PropertyCachingStrategy.EagerCached);
			dynamic value = fastProp.GetValue(settings);

			SetSetting(key, value ?? "", storeId, clearCache);
		}

		public virtual void UpdateSetting<T, TPropType>(
			T settings, 
			Expression<Func<T, TPropType>> keySelector, 
			bool overrideForStore, 
			int storeId = 0)  where T : ISettings, new()
		{
			if (overrideForStore || storeId == 0)
				SaveSetting(settings, keySelector, storeId, true);
			else if (storeId > 0)
				DeleteSetting(settings, keySelector, storeId);
		}

		public virtual void DeleteSetting(Setting setting)
		{
			if (setting == null)
				throw new ArgumentNullException("setting");

			_settingRepository.Delete(setting);

			_hasChanges = true;

			ClearCache();

			_eventPublisher.EntityDeleted(setting);
		}

        public virtual void DeleteSetting<T>() where T : ISettings, new()
        {
			using (BeginBatch())
			{
				if (typeof(T).HasAttribute<JsonPersistAttribute>(true))
				{
					DeleteSettingsJson<T>();
					return;
				}

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

		public virtual void DeleteSetting<T, TPropType>(
			T settings,
			Expression<Func<T, TPropType>> keySelector, 
			int storeId = 0) where T : ISettings, new()
		{
			var member = keySelector.Body as MemberExpression;
			if (member == null)
			{
				throw new ArgumentException(string.Format(
					"Expression '{0}' refers to a method, not a property.",
					keySelector));
			}

			var propInfo = member.Member as PropertyInfo;
			if (propInfo == null)
			{
				throw new ArgumentException(string.Format(
					   "Expression '{0}' refers to a field, not a property.",
					   keySelector));
			}

			string key = typeof(T).Name + "." + propInfo.Name;

			DeleteSetting(key, storeId);
		}

		public virtual void DeleteSetting(string key, int storeId = 0)
		{
			if (key.HasValue())
			{
				key = key.Trim().ToLowerInvariant();

				var setting = (
					from s in _settingRepository.Table
					where s.StoreId == storeId && s.Name == key
					select s).FirstOrDefault();

				if (setting != null)
					DeleteSetting(setting);
			}
		}

		public virtual int DeleteSettings(string rootKey) {
			int result = 0;

			if (rootKey.IsEmpty())
				return 0;

			try
			{
				string sqlDelete = "DELETE FROM [Setting] WHERE [Name] LIKE '{0}%'".FormatWith(rootKey.EndsWith(".") ? rootKey : rootKey + ".");
				result = _settingRepository.Context.ExecuteSqlCommand(sqlDelete);

				if (result > 0)
					_hasChanges = true;

				ClearCache();
			}
			catch (Exception exc)
			{
				exc.Dump();
			}

			return result;
		}

        public virtual void ClearCache()
        {
			if (!_isInBatch)
			{
				_cacheManager.RemoveByPattern(SETTINGS_ALL_KEY);
				_hasChanges = false;
			}
		}

		protected string CreateCacheKey(string name, int storeId)
		{
			return name.Trim().ToLowerInvariant() + "/" + storeId.ToString();
		} 
    }

	//[Serializable]
	//public class SettingKey : ComparableObject
	//{
	//	[ObjectSignature]
	//	public string Name { get; set; }

	//	[ObjectSignature]
	//	public int StoreId { get; set; }

	//	public override string ToString()
	//	{
	//		return Name + "@__!__@" + StoreId;
	//	}
	//}

	[Serializable]
	public class CachedSetting
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Value { get; set; }
		public int StoreId { get; set; }
	}
}