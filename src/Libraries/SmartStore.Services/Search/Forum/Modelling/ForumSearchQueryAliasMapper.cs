using System;
using System.Collections.Generic;
using SmartStore.Core.Caching;
using SmartStore.Core.Search.Facets;
using SmartStore.Services.Configuration;
using SmartStore.Services.Localization;
using SmartStore.Services.Search.Extensions;

namespace SmartStore.Services.Search.Modelling
{
    public class ForumSearchQueryAliasMapper : IForumSearchQueryAliasMapper
    {
        private const string ALL_FORUM_COMMONFACET_ALIAS_BY_KIND_KEY = "search.forum.commonfacet.alias.kind.mappings.all";

        private readonly ICacheManager _cacheManager;
        private readonly ISettingService _settingService;
        private readonly ILanguageService _languageService;

        public ForumSearchQueryAliasMapper(
            ICacheManager cacheManager,
            ISettingService settingService,
            ILanguageService languageService)
        {
            _cacheManager = cacheManager;
            _settingService = settingService;
            _languageService = languageService;
        }

        protected virtual IDictionary<string, string> GetCommonFacetAliasByGroupKindMappings()
        {
            return _cacheManager.Get(ALL_FORUM_COMMONFACET_ALIAS_BY_KIND_KEY, () =>
            {
                var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                var groupKinds = new FacetGroupKind[]
                {
                    FacetGroupKind.Forum,
                    FacetGroupKind.Customer,
                    FacetGroupKind.Date
                };

                foreach (var language in _languageService.GetAllLanguages())
                {
                    foreach (var groupKind in groupKinds)
                    {
                        var key = FacetUtility.GetFacetAliasSettingKey(groupKind, language.Id, "Forum");
                        var value = _settingService.GetSettingByKey<string>(key);
                        if (value.HasValue())
                        {
                            result.Add(key, value);
                        }
                    }
                }

                return result;
            });
        }

        public void ClearCommonFacetCache()
        {
            _cacheManager.Remove(ALL_FORUM_COMMONFACET_ALIAS_BY_KIND_KEY);
        }

        public string GetCommonFacetAliasByGroupKind(FacetGroupKind kind, int languageId)
        {
            var mappings = GetCommonFacetAliasByGroupKindMappings();

            return mappings.Get(FacetUtility.GetFacetAliasSettingKey(kind, languageId, "Forum"));
        }
    }
}
