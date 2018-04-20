using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Events;
using SmartStore.Services.Configuration;
using SmartStore.Services.Stores;
using SmartStore.Collections;
using SmartStore.Core;
using SmartStore.Data.Caching;

namespace SmartStore.Services.Localization
{
    public partial class LanguageService : ILanguageService
    {
        private const string LANGUAGES_COUNT = "SmartStore.language.count-{0}";
        private const string LANGUAGES_PATTERN_KEY = "SmartStore.language.*";

        private readonly IRepository<Language> _languageRepository;
		private readonly IStoreMappingService _storeMappingService;
		private readonly IStoreService _storeService;
		private readonly IStoreContext _storeContext;
        private readonly IRequestCache _requestCache;
		private readonly ICacheManager _cache;
		private readonly ISettingService _settingService;
        private readonly LocalizationSettings _localizationSettings;
        private readonly IEventPublisher _eventPublisher;

        public LanguageService(
			IRequestCache requestCache,
			ICacheManager cache,
			IRepository<Language> languageRepository,
            ISettingService settingService,
            LocalizationSettings localizationSettings,
            IEventPublisher eventPublisher,
			IStoreMappingService storeMappingService,
			IStoreService storeService,
			IStoreContext storeContext)
        {
            _requestCache = requestCache;
			_cache = cache;
            _languageRepository = languageRepository;
            _settingService = settingService;
            _localizationSettings = localizationSettings;
            _eventPublisher = eventPublisher;
			_storeMappingService = storeMappingService;
			_storeService = storeService;
			_storeContext = storeContext;
        }

        public virtual void DeleteLanguage(Language language)
        {
			Guard.NotNull(language, nameof(language));

            // Update default admin area language (if required)
            if (_localizationSettings.DefaultAdminLanguageId == language.Id)
            {
                foreach (var activeLanguage in GetAllLanguages())
                {
                    if (activeLanguage.Id != language.Id)
                    {
                        _localizationSettings.DefaultAdminLanguageId = activeLanguage.Id;
                        _settingService.SaveSetting(_localizationSettings);
                        break;
                    }
                }
            }
            
            _languageRepository.Delete(language);

            // cache
            _requestCache.RemoveByPattern(LANGUAGES_PATTERN_KEY);	
        }

		public virtual IList<Language> GetAllLanguages(bool showHidden = false, int storeId = 0)
        {
			var query = _languageRepository.Table;

			if (!showHidden)
			{
				query = query.Where(x => x.Published);
			}

			query = query.OrderBy(x => x.DisplayOrder);

			var languages = query.ToListCached("db.lang.all.{0}".FormatInvariant(showHidden));

			// store mapping
			if (storeId > 0)
			{
				languages = languages
					.Where(l => _storeMappingService.Authorize(l, storeId))
					.ToList();
			}

			return languages;
        }

        public virtual int GetLanguagesCount(bool showHidden = false)
        {
            string key = string.Format(LANGUAGES_COUNT, showHidden);
            return _requestCache.Get(key, () =>
            {
                var query = _languageRepository.Table;
                if (!showHidden)
                    query = query.Where(l => l.Published);
                return query.Select(x => x.Id).Count();
            });
        }

        public virtual Language GetLanguageById(int languageId)
        {
            if (languageId == 0)
                return null;

			return _languageRepository.GetByIdCached(languageId, "db.lang.id-" + languageId);
		}

        public virtual Language GetLanguageByCulture(string culture)
        {
            if (!culture.HasValue())
                return null;

			return _languageRepository.Table
				.Where(x => culture.Equals(x.LanguageCulture, StringComparison.InvariantCultureIgnoreCase))
				.FirstOrDefaultCached("db.lang.culture-" + culture);
		}

        public virtual Language GetLanguageBySeoCode(string seoCode)
        {
            if (!seoCode.HasValue())
                return null;

			return _languageRepository.Table
				.Where(x => seoCode.Equals(x.UniqueSeoCode, StringComparison.InvariantCultureIgnoreCase))
				.FirstOrDefaultCached("db.lang.seo-" + seoCode);
		}

        public virtual void InsertLanguage(Language language)
        {
            if (language == null)
                throw new ArgumentNullException("language");

            _languageRepository.Insert(language);

            // cache
            _requestCache.RemoveByPattern(LANGUAGES_PATTERN_KEY);
        }

        public virtual void UpdateLanguage(Language language)
        {
            if (language == null)
                throw new ArgumentNullException("language");
            
            //update language
            _languageRepository.Update(language);

            //cache
            _requestCache.RemoveByPattern(LANGUAGES_PATTERN_KEY);
        }

		public virtual bool IsPublishedLanguage(string seoCode, int storeId = 0)
		{
			if (storeId <= 0)
				storeId = _storeContext.CurrentStore.Id;

			var map = this.GetStoreLanguageMap();
			if (map.ContainsKey(storeId))
			{
				return map[storeId].Any(x => x.UniqueSeoCode == seoCode);
			}

			return false;
		}

		public virtual bool IsPublishedLanguage(int languageId, int storeId = 0)
		{
			if (languageId <= 0)
				return false;

			if (storeId <= 0)
				storeId = _storeContext.CurrentStore.Id;

			var map = this.GetStoreLanguageMap();
			if (map.ContainsKey(storeId))
			{
				return map[storeId].Any(x => x.Id == languageId);
			}

			return false;
		}

		public virtual string GetDefaultLanguageSeoCode(int storeId = 0)
		{
			if (storeId <= 0)
				storeId = _storeContext.CurrentStore.Id;

			var map = this.GetStoreLanguageMap();
			if (map.ContainsKey(storeId))
			{
				return map[storeId].FirstOrDefault().UniqueSeoCode;
			}

			return null;
		}

		public virtual int GetDefaultLanguageId(int storeId = 0)
		{
			if (storeId <= 0)
				storeId = _storeContext.CurrentStore.Id;

			var map = this.GetStoreLanguageMap();
			if (map.ContainsKey(storeId))
			{
				return map[storeId].FirstOrDefault().Id;
			}

			return 0;
		}

		/// <summary>
		/// Gets a map of active/published store languages
		/// </summary>
		/// <returns>A map of store languages where key is the store id and values are tuples of language ids and seo codes</returns>
		protected virtual Multimap<int, MinifiedLanguage> GetStoreLanguageMap()
		{
			var result = _cache.Get(ServiceCacheBuster.STORE_LANGUAGE_MAP_KEY, () =>
			{
				var map = new Multimap<int, MinifiedLanguage>();

				var allStores = _storeService.GetAllStores();
				foreach (var store in allStores)
				{
					var languages = GetAllLanguages(false, store.Id);
					if (!languages.Any())
					{
						// language-less stores aren't allowed but could exist accidentally. Correct this.
						var firstStoreLang = GetAllLanguages(true, store.Id).FirstOrDefault();
						if (firstStoreLang == null)
						{
							// absolute fallback
							firstStoreLang = GetAllLanguages(true).FirstOrDefault();
						}
						map.Add(store.Id, new MinifiedLanguage { Id = firstStoreLang.Id, UniqueSeoCode = firstStoreLang.UniqueSeoCode });
					}
					else
					{
						foreach (var lang in languages)
						{
							map.Add(store.Id, new MinifiedLanguage { Id = lang.Id, UniqueSeoCode = lang.UniqueSeoCode });
						}
					}
				}

				return map;
			}, TimeSpan.FromDays(1));

			return result;
		}

		public class MinifiedLanguage
		{
			public int Id { get; set; }
			public string UniqueSeoCode { get; set; }
		}
    }
}
