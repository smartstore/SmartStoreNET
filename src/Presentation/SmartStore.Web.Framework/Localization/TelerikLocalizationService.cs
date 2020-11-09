using System.Collections.Generic;
using SmartStore.Core.Caching;

namespace SmartStore.Web.Framework.Localization
{
    public class TelerikLocalizationService : Telerik.Web.Mvc.Infrastructure.ILocalizationService
    {
        private readonly string _componentName;
        private readonly SmartStore.Services.Localization.ILocalizationService _localizationService;
        private readonly IRequestCache _requestCache;
        private readonly int _currentLanguageId;

        public TelerikLocalizationService(
            string componentName,
            int currentLanguageId,
            Services.Localization.ILocalizationService localizationService,
            IRequestCache requestCache)
        {
            _componentName = componentName;
            _currentLanguageId = currentLanguageId;
            _localizationService = localizationService;
            _requestCache = requestCache;
        }

        public IDictionary<string, string> All()
        {
            var scope = "Admin.Telerik." + _componentName;

            var cacheKey = scope + ".Resources";

            return _requestCache.Get(cacheKey, () =>
            {
                var resources = _localizationService.GetResourcesByPattern(scope, _currentLanguageId);
                var dict = resources.ToDictionarySafe(
                    x => x.ResourceName.Substring(scope.Length).ToLowerInvariant(),
                    x => x.ResourceValue);

                return dict;
            });
        }

        public bool IsDefault => true;

        public string One(string key)
        {
            var resourceName = "Admin.Telerik." + _componentName + "." + key;
            var result = _localizationService.GetResource(resourceName, _currentLanguageId, true, resourceName);
            return result;
        }
    }
}