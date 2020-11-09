using System;
using System.Collections.Generic;
using System.Globalization;
using SmartStore.Core;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Html;
using SmartStore.Services.Seo;

namespace SmartStore.Services.Localization
{
    public class LocalizedEntityHelper
    {
        private readonly ILanguageService _languageService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IWorkContext _workContext;

        private readonly int _languageCount;
        private readonly Language _defaultLanguage;

        public LocalizedEntityHelper(
            ILanguageService languageService,
            ILocalizedEntityService localizedEntityService,
            IUrlRecordService urlRecordService,
            IWorkContext workContext)
        {
            _languageService = languageService;
            _localizedEntityService = localizedEntityService;
            _urlRecordService = urlRecordService;
            _workContext = workContext;

            _languageCount = _languageService.GetLanguagesCount(false);
            _defaultLanguage = _languageService.GetLanguageById(_languageService.GetDefaultLanguageId());
        }

        public LocalizedValue<TProp> GetLocalizedValue<T, TProp>(T obj,
            int id, // T is BaseEntity = EntityId, T is ISetting = StoreId
            string localeKeyGroup,
            string localeKey,
            Func<T, TProp> fallback,
            object requestLanguageIdOrObj, // Id or Language
            bool returnDefaultValue = true,
            bool ensureTwoPublishedLanguages = true,
            bool detectEmptyHtml = false)
            where T : class
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            TProp result = default;
            var str = string.Empty;

            Language currentLanguage = null;
            Language requestLanguage = null;

            if (!(requestLanguageIdOrObj is Language))
            {
                if (requestLanguageIdOrObj is int requestLanguageId)
                {
                    requestLanguage = _languageService.GetLanguageById(requestLanguageId);
                }
            }
            else
            {
                requestLanguage = (Language)requestLanguageIdOrObj;
            }

            if (requestLanguage == null)
            {
                requestLanguage = _workContext.WorkingLanguage;
            }

            // Ensure that we have at least two published languages
            var loadLocalizedValue = true;
            if (ensureTwoPublishedLanguages)
            {
                loadLocalizedValue = _languageCount > 1;
            }

            // Localized value
            if (loadLocalizedValue)
            {
                str = _localizedEntityService.GetLocalizedValue(requestLanguage.Id, id, localeKeyGroup, localeKey);

                if (detectEmptyHtml && HtmlUtils.IsEmptyHtml(str))
                {
                    str = string.Empty;
                }

                if (!string.IsNullOrEmpty(str))
                {
                    currentLanguage = requestLanguage;
                    result = str.Convert<TProp>(CultureInfo.InvariantCulture);
                }
            }

            // Set default value if required
            if (returnDefaultValue && string.IsNullOrEmpty(str))
            {
                currentLanguage = _defaultLanguage;
                result = fallback(obj);
            }

            if (currentLanguage == null)
            {
                currentLanguage = requestLanguage;
            }

            return new LocalizedValue<TProp>(result, requestLanguage, currentLanguage);
        }

        public string GetSeName(
            string entityName,
            int entityId,
            int? languageId,
            bool returnDefaultValue = true,
            bool ensureTwoPublishedLanguages = true)
        {
            string result = string.Empty;

            if (languageId == null)
            {
                languageId = _workContext.WorkingLanguage.Id;
            }

            if (languageId > 0)
            {
                // Ensure that we have at least two published languages
                bool loadLocalizedValue = true;
                if (ensureTwoPublishedLanguages)
                {
                    loadLocalizedValue = _languageCount > 1;
                }

                // Localized value
                if (loadLocalizedValue)
                {
                    result = _urlRecordService.GetActiveSlug(entityId, entityName, languageId.Value);
                }
            }

            // Set default value if required
            if (String.IsNullOrEmpty(result) && returnDefaultValue)
            {
                result = _urlRecordService.GetActiveSlug(entityId, entityName, 0);
            }

            return result;
        }
    }
}
