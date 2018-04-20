using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using SmartStore.Core;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Localization;

namespace SmartStore.Services.Localization
{
	/// <summary>
	/// Provides information about localizable entities
	/// </summary>
	public partial class LocalizedEntityService : ScopedServiceBase, ILocalizedEntityService
    {
		/// <summary>
		/// 0 = segment (keygroup.key.idrange), 1 = language id
		/// </summary>
		const string LOCALIZEDPROPERTY_SEGMENT_KEY = "localizedproperty:{0}-lang-{1}";
		const string LOCALIZEDPROPERTY_SEGMENT_PATTERN = "localizedproperty:{0}";
		const string LOCALIZEDPROPERTY_ALLSEGMENTS_PATTERN = "localizedproperty:*";

		private readonly IRepository<LocalizedProperty> _localizedPropertyRepository;
        private readonly ICacheManager _cacheManager;

        public LocalizedEntityService(
			ICacheManager cacheManager, 
			IRepository<LocalizedProperty> localizedPropertyRepository)
        {
            _cacheManager = cacheManager;
            _localizedPropertyRepository = localizedPropertyRepository;
		}

		protected override void OnClearCache()
		{
			_cacheManager.RemoveByPattern(LOCALIZEDPROPERTY_ALLSEGMENTS_PATTERN);
		}

		protected virtual IDictionary<int, string> GetCachedPropertySegment(string localeKeyGroup, string localeKey, int entityId, int languageId)
		{
			Guard.NotEmpty(localeKeyGroup, nameof(localeKeyGroup));
			Guard.NotEmpty(localeKey, nameof(localeKey));

			var segmentKey = GetSegmentKey(localeKeyGroup, localeKey, entityId, out var minEntityId, out var maxEntityId);
			var cacheKey = BuildCacheSegmentKey(segmentKey, languageId);

			// TODO: (MC) skip caching product.fulldescription (?), OR
			// ...additionally segment by entity id ranges.

			return _cacheManager.Get(cacheKey, () =>
			{
				var properties = _localizedPropertyRepository.TableUntracked
					.Where(x => x.EntityId >= minEntityId && x.EntityId <= maxEntityId && x.LocaleKey == localeKey && x.LocaleKeyGroup == localeKeyGroup && x.LanguageId == languageId)
					.ToList();

				var dict = new Dictionary<int, string>(properties.Count);

				foreach (var prop in properties)
				{
					dict[prop.EntityId] = prop.LocaleValue.EmptyNull();
				}

				return dict;
			});
		}

		/// <summary>
		/// Clears the cached segment from the cache
		/// </summary>
		protected virtual void ClearCachedPropertySegment(string localeKeyGroup, string localeKey, int entityId, int? languageId = null)
		{
			if (IsInScope)
				return;

			var segmentKey = GetSegmentKey(localeKeyGroup, localeKey, entityId);

			if (languageId.HasValue && languageId.Value > 0)
			{
				_cacheManager.Remove(BuildCacheSegmentKey(segmentKey, languageId.Value));
			}
			else
			{
				_cacheManager.RemoveByPattern(LOCALIZEDPROPERTY_SEGMENT_PATTERN.FormatInvariant(segmentKey));
			}
		}

		public virtual string GetLocalizedValue(int languageId, int entityId, string localeKeyGroup, string localeKey)
		{
			if (IsInScope)
			{
				return GetLocalizedValueUncached(languageId, entityId, localeKeyGroup, localeKey);
			}

			if (languageId <= 0)
				return string.Empty;

			var props = GetCachedPropertySegment(localeKeyGroup, localeKey, entityId, languageId);

			if (!props.TryGetValue(entityId, out var val))
			{
				return string.Empty;
			}

			return val;
		}

		protected string GetLocalizedValueUncached(int languageId, int entityId, string localeKeyGroup, string localeKey)
		{
			if (languageId <= 0)
				return string.Empty;

			var query = from lp in _localizedPropertyRepository.TableUntracked
						where
							lp.EntityId == entityId &&
							lp.LocaleKey == localeKey &&
							lp.LocaleKeyGroup == localeKeyGroup &&
							lp.LanguageId == languageId
						select lp.LocaleValue;

			return query.FirstOrDefault().EmptyNull();
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

		public virtual void InsertLocalizedProperty(LocalizedProperty property)
        {
			Guard.NotNull(property, nameof(property));

			// db
			_localizedPropertyRepository.Insert(property);
			HasChanges = true;

			try
			{
				// cache
				ClearCachedPropertySegment(property.LocaleKeyGroup, property.LocaleKey, property.EntityId, property.LanguageId);
			}
			catch { }
        }

        public virtual void UpdateLocalizedProperty(LocalizedProperty property)
        {
			Guard.NotNull(property, nameof(property));

			// db
			_localizedPropertyRepository.Update(property);
			HasChanges = true;

			try
			{
				// cache
				ClearCachedPropertySegment(property.LocaleKeyGroup, property.LocaleKey, property.EntityId, property.LanguageId);
			}
			catch { }
		}

		public virtual void DeleteLocalizedProperty(LocalizedProperty property)
		{
			Guard.NotNull(property, nameof(property));

			try
			{
				// cache
				ClearCachedPropertySegment(property.LocaleKeyGroup, property.LocaleKey, property.EntityId, property.LanguageId);
			}
			catch { }

			// db
			_localizedPropertyRepository.Delete(property);
			HasChanges = true;
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
			Guard.NotNull(entity, nameof(entity));
			Guard.NotZero(languageId, nameof(languageId));

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

            var keyGroup = typeof(T).Name;
            var key = propInfo.Name;
			var valueStr = localeValue.Convert<string>();
			var prop = GetLocalizedProperty(languageId, entity.Id, keyGroup, key);

            if (prop != null)
            {
                if (valueStr.IsEmpty())
                {
                    // delete
                    DeleteLocalizedProperty(prop);
                }
                else
                {
                    // update
					if (prop.LocaleValue != valueStr)
					{
						prop.LocaleValue = valueStr;
						UpdateLocalizedProperty(prop);
					}
                }
            }
            else
            {
                if (valueStr.HasValue())
                {
                    // insert
                    prop = new LocalizedProperty
                    {
                        EntityId = entity.Id,
                        LanguageId = languageId,
                        LocaleKey = key,
                        LocaleKeyGroup = keyGroup,
                        LocaleValue = valueStr
                    };
                    InsertLocalizedProperty(prop);
                }
            }
        }

		private string BuildCacheSegmentKey(string segment, int languageId)
		{
			return String.Format(LOCALIZEDPROPERTY_SEGMENT_KEY, segment, languageId);
		}

		private string GetSegmentKey(string localeKeyGroup, string localeKey, int entityId)
		{
			return GetSegmentKey(localeKeyGroup, localeKey, entityId, out var minId, out var maxId);
		}

		private string GetSegmentKey(string localeKeyGroup, string localeKey, int entityId, out int minId, out int maxId)
		{
			minId = 0;
			maxId = 0;

			// max 500 values per cache item
			var entityRange = Math.Ceiling((decimal)entityId / 500) * 500;

			maxId = (int)entityRange;
			minId = maxId - 499;

			return (localeKeyGroup + "." + localeKey + "." + entityRange.ToString()).ToLowerInvariant();
		}
	}
}