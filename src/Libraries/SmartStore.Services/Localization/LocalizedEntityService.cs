using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using SmartStore.Core;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Localization;
using System.Collections.Concurrent;

namespace SmartStore.Services.Localization
{
    /// <summary>
    /// Provides information about localizable entities
    /// </summary>
    public partial class LocalizedEntityService : ILocalizedEntityService
    {
		private const string LOCALIZEDPROPERTY_ALL_KEY = "SmartStore.localizedproperty.all";

        private readonly IRepository<LocalizedProperty> _localizedPropertyRepository;
        private readonly ICacheManager _cacheManager;
		private readonly LocalizationSettings _localizationSettings;

        public LocalizedEntityService(
			ICacheManager cacheManager, 
			IRepository<LocalizedProperty> localizedPropertyRepository, 
			LocalizationSettings localizationSettings)
        {
            this._cacheManager = cacheManager;
            this._localizedPropertyRepository = localizedPropertyRepository;
			this._localizationSettings = localizationSettings;
        }

		protected virtual ConcurrentDictionary<string, string> GetAllProperties()
		{
			var result = _cacheManager.Get(LOCALIZEDPROPERTY_ALL_KEY, () =>
			{
				if (_localizationSettings.LoadAllLocalizedPropertiesOnStartup)
				{
					var props = _localizedPropertyRepository.TableUntracked.ToDictionarySafe(
						x => GenerateKey(x.LanguageId, x.LocaleKeyGroup, x.LocaleKey, x.EntityId),
						x => x.LocaleValue.EmptyNull());
					return new ConcurrentDictionary<string, string>(props);
				}
				else
				{
					return new ConcurrentDictionary<string, string>();
				}
			});

			return result;
		}

		public virtual string GetLocalizedValue(int languageId, int entityId, string localeKeyGroup, string localeKey)
		{
			var props = GetAllProperties();
			string key = GenerateKey(languageId, localeKeyGroup, localeKey, entityId);
			string val = null;

			if (_localizationSettings.LoadAllLocalizedPropertiesOnStartup)
			{
				if (!props.TryGetValue(key, out val))
				{
					return string.Empty;
				}
			}
			else
			{
				val = props.GetOrAdd(key, k => {
					var query = from lp in _localizedPropertyRepository.TableUntracked
								where
									lp.EntityId == entityId &&
									lp.LocaleKey == localeKey &&
									lp.LocaleKeyGroup == localeKeyGroup &&
									lp.LanguageId == languageId
								select lp.LocaleValue;

					return query.FirstOrDefault().EmptyNull();
				});
			}

			return val;
		}

        public virtual IList<LocalizedProperty> GetLocalizedProperties(int entityId, string localeKeyGroup)
        {
            if (localeKeyGroup.IsEmpty())
                return new List<LocalizedProperty>();

            var query = from lp in _localizedPropertyRepository.Table
                        orderby lp.Id
                        where lp.EntityId == entityId &&
                              lp.LocaleKeyGroup == localeKeyGroup
                        select lp;

            var props = query.ToList();
            return props;
        }

		protected virtual LocalizedProperty GetLocalizedProperty(int languageId, int entityId, string localeKeyGroup, string localeKey)
		{
			var query = from lp in _localizedPropertyRepository.Table
						where
							lp.EntityId == entityId &&
							lp.LocaleKey == localeKey &&
							lp.LocaleKeyGroup == localeKeyGroup &&
							lp.LanguageId == languageId
						select lp;

			return query.FirstOrDefault();
		}

		public virtual void InsertLocalizedProperty(LocalizedProperty localizedProperty)
        {
			Guard.ArgumentNotNull(() => localizedProperty);

			try
			{
				// db
				_localizedPropertyRepository.Insert(localizedProperty);

				// cache
				var key = GenerateKey(localizedProperty);
				var val = localizedProperty.LocaleValue.EmptyNull();
				GetAllProperties().AddOrUpdate(
					key,
					val, 
					(k, v) => val);
			}
			catch { }
        }

        public virtual void UpdateLocalizedProperty(LocalizedProperty localizedProperty)
        {
			Guard.ArgumentNotNull(() => localizedProperty);

			try
			{
				// db
				_localizedPropertyRepository.Update(localizedProperty);

				// cache
				var key = GenerateKey(localizedProperty);
				var val = localizedProperty.LocaleValue.EmptyNull();
				GetAllProperties().AddOrUpdate(
					key,
					val,
					(k, v) => val);
			}
			catch { }
		}

		public virtual void DeleteLocalizedProperty(LocalizedProperty localizedProperty)
		{
			Guard.ArgumentNotNull(() => localizedProperty);

			try
			{
				// cache
				var key = GenerateKey(localizedProperty);
				string val = null;
				GetAllProperties().TryRemove(key, out val);

				// db
				_localizedPropertyRepository.Delete(localizedProperty);
			}
			catch { }
		}

		public virtual LocalizedProperty GetLocalizedPropertyById(int localizedPropertyId)
		{
			if (localizedPropertyId == 0)
				return null;

			var localizedProperty = _localizedPropertyRepository.GetById(localizedPropertyId);
			return localizedProperty;
		}

		public virtual void SaveLocalizedValue<T>(
			T entity,
            Expression<Func<T, string>> keySelector,
            string localeValue,
            int languageId) where T : BaseEntity, ILocalizedEntity
        {
            SaveLocalizedValue<T, string>(entity, keySelector, localeValue, languageId);
        }

        public virtual void SaveLocalizedValue<T, TPropType>(
			T entity,
            Expression<Func<T, TPropType>> keySelector,
            TPropType localeValue,
            int languageId) where T : BaseEntity, ILocalizedEntity
        {
			Guard.ArgumentNotNull(() => entity);
			Guard.ArgumentNotZero(languageId, "languageId");

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

            string localeKeyGroup = typeof(T).Name;
            string localeKey = propInfo.Name;

			var prop = GetLocalizedProperty(languageId, entity.Id, localeKeyGroup, localeKey);

            string localeValueStr = localeValue.Convert<string>();

            if (prop != null)
            {
                if (localeValueStr.IsEmpty())
                {
                    // delete
                    DeleteLocalizedProperty(prop);
                }
                else
                {
                    // update
					if (prop.LocaleValue != localeValueStr)
					{
						prop.LocaleValue = localeValueStr;
						UpdateLocalizedProperty(prop);
					}
                }
            }
            else
            {
                if (localeValueStr.HasValue())
                {
                    // insert
                    prop = new LocalizedProperty
                    {
                        EntityId = entity.Id,
                        LanguageId = languageId,
                        LocaleKey = localeKey,
                        LocaleKeyGroup = localeKeyGroup,
                        LocaleValue = localeValueStr
                    };
                    InsertLocalizedProperty(prop);
                }
            }
        }

		private string GenerateKey(LocalizedProperty prop)
		{
			return GenerateKey(prop.LanguageId, prop.LocaleKeyGroup, prop.LocaleKey, prop.EntityId);
		}

		private string GenerateKey(int languageId, string localeKeyGroup, string localeKey, int entityId)
		{
			return "{0}.{1}.{2}.{3}".FormatInvariant(languageId, localeKeyGroup, localeKey, entityId);
		}
    }
}