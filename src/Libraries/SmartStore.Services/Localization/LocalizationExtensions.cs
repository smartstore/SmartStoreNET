using System;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Xml;
using SmartStore.ComponentModel;
using SmartStore.Core;
using SmartStore.Core.Configuration;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Plugins;
using SmartStore.Utilities;

namespace SmartStore.Services.Localization
{
    public static class LocalizationExtensions
    {
        #region GetLocalized BaseEntity

        /// <summary>
        /// Get localized property of an entity
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <param name="entity">Entity</param>
        /// <param name="keySelector">Key selector</param>
        /// <param name="detectEmptyHtml">When <c>true</c>, additionally checks whether the localized value contains empty HTML only and falls back to the default value if so.</param>
        /// <returns>Localized property</returns>
        public static LocalizedValue<string> GetLocalized<T>(this T entity, Expression<Func<T, string>> keySelector, bool detectEmptyHtml = false)
            where T : BaseEntity, ILocalizedEntity
        {
            var invoker = keySelector.CompileFast();
            return EngineContext.Current.Resolve<LocalizedEntityHelper>().GetLocalizedValue(
                entity,
                entity.Id,
                entity.GetEntityName(),
                invoker.Property.Name,
                (Func<T, string>)invoker,
                null,
                detectEmptyHtml: detectEmptyHtml);
        }

        /// <summary>
        /// Get localized property of an entity
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <param name="entity">Entity</param>
        /// <param name="keySelector">Key selector</param>
        /// <param name="languageId">Language identifier</param>
        /// <param name="returnDefaultValue">A value indicating whether to return default value (if localized is not found)</param>
        /// <param name="ensureTwoPublishedLanguages">A value indicating whether to ensure that we have at least two published languages; otherwise, load only default value</param>
        /// <param name="detectEmptyHtml">When <c>true</c>, additionally checks whether the localized value contains empty HTML only and falls back to the default value if so.</param>
        /// <returns>Localized property</returns>
        public static LocalizedValue<string> GetLocalized<T>(this T entity,
            Expression<Func<T, string>> keySelector,
            int languageId,
            bool returnDefaultValue = true,
            bool ensureTwoPublishedLanguages = true,
            bool detectEmptyHtml = false)
            where T : BaseEntity, ILocalizedEntity
        {
            var invoker = keySelector.CompileFast();
            return EngineContext.Current.Resolve<LocalizedEntityHelper>().GetLocalizedValue<T, string>(
                entity,
                entity.Id,
                entity.GetEntityName(),
                invoker.Property.Name,
                invoker,
                languageId,
                returnDefaultValue,
                ensureTwoPublishedLanguages,
                detectEmptyHtml);
        }

        /// <summary>
        /// Get localized property of an entity
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <param name="entity">Entity</param>
        /// <param name="localeKey">Key selector</param>
        /// <param name="languageId">Language identifier</param>
        /// <param name="returnDefaultValue">A value indicating whether to return default value (if localized is not found)</param>
        /// <param name="ensureTwoPublishedLanguages">A value indicating whether to ensure that we have at least two published languages; otherwise, load only default value</param>
        /// <param name="detectEmptyHtml">When <c>true</c>, additionally checks whether the localized value contains empty HTML only and falls back to the default value if so.</param>
        /// <returns>Localized property</returns>
        public static LocalizedValue<TProp> GetLocalized<T, TProp>(this T entity,
            string localeKey,
            TProp fallback,
            Language language,
            bool returnDefaultValue = true,
            bool ensureTwoPublishedLanguages = true,
            bool detectEmptyHtml = false)
            where T : BaseEntity, ILocalizedEntity
        {
            return EngineContext.Current.Resolve<LocalizedEntityHelper>().GetLocalizedValue<T, TProp>(
                entity,
                entity.Id,
                entity.GetEntityName(),
                localeKey,
                x => fallback,
                language,
                returnDefaultValue,
                ensureTwoPublishedLanguages,
                detectEmptyHtml);
        }

        /// <summary>
        /// Get localized property of an entity
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <param name="entity">Entity</param>
        /// <param name="keySelector">Key selector</param>
        /// <param name="language">Language</param>
        /// <param name="returnDefaultValue">A value indicating whether to return default value (if localized is not found)</param>
        /// <param name="ensureTwoPublishedLanguages">A value indicating whether to ensure that we have at least two published languages; otherwise, load only default value</param>
        /// <param name="detectEmptyHtml">When <c>true</c>, additionally checks whether the localized value contains empty HTML only and falls back to the default value if so.</param>
        /// <returns>Localized property</returns>
        public static LocalizedValue<string> GetLocalized<T>(this T entity,
            Expression<Func<T, string>> keySelector,
            Language language,
            bool returnDefaultValue = true,
            bool ensureTwoPublishedLanguages = true,
            bool detectEmptyHtml = false)
            where T : BaseEntity, ILocalizedEntity
        {
            var invoker = keySelector.CompileFast();
            return EngineContext.Current.Resolve<LocalizedEntityHelper>().GetLocalizedValue<T, string>(
                entity,
                entity.Id,
                entity.GetEntityName(),
                invoker.Property.Name,
                invoker,
                language,
                returnDefaultValue,
                ensureTwoPublishedLanguages,
                detectEmptyHtml);
        }

        /// <summary>
        /// Get localized property of an entity
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <typeparam name="TProp">Property type</typeparam>
        /// <param name="entity">Entity</param>
        /// <param name="keySelector">Key selector</param>
        /// <param name="languageId">Language identifier</param>
        /// <param name="returnDefaultValue">A value indicating whether to return default value (if localized is not found)</param>
        /// <param name="ensureTwoPublishedLanguages">A value indicating whether to ensure that we have at least two published languages; otherwise, load only default value</param>
        /// <param name="detectEmptyHtml">When <c>true</c>, additionally checks whether the localized value contains empty HTML only and falls back to the default value if so.</param>
        /// <returns>Localized property</returns>
        public static LocalizedValue<TProp> GetLocalized<T, TProp>(this T entity,
            Expression<Func<T, TProp>> keySelector,
            int languageId,
            bool returnDefaultValue = true,
            bool ensureTwoPublishedLanguages = true,
            bool detectEmptyHtml = false)
            where T : BaseEntity, ILocalizedEntity
        {
            var invoker = keySelector.CompileFast();
            return EngineContext.Current.Resolve<LocalizedEntityHelper>().GetLocalizedValue(
                entity,
                entity.Id,
                entity.GetEntityName(),
                invoker.Property.Name,
                (Func<T, TProp>)invoker,
                languageId,
                returnDefaultValue,
                ensureTwoPublishedLanguages,
                detectEmptyHtml);
        }

        /// <summary>
        /// Get localized property of an entity
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <typeparam name="TProp">Property type</typeparam>
        /// <param name="entity">Entity</param>
        /// <param name="keySelector">Key selector</param>
        /// <param name="language">Language</param>
        /// <param name="returnDefaultValue">A value indicating whether to return default value (if localized is not found)</param>
        /// <param name="ensureTwoPublishedLanguages">A value indicating whether to ensure that we have at least two published languages; otherwise, load only default value</param>
        /// <param name="detectEmptyHtml">When <c>true</c>, additionally checks whether the localized value contains empty HTML only and falls back to the default value if so.</param>
        /// <returns>Localized property</returns>
        public static LocalizedValue<TProp> GetLocalized<T, TProp>(this T entity,
            Expression<Func<T, TProp>> keySelector,
            Language language,
            bool returnDefaultValue = true,
            bool ensureTwoPublishedLanguages = true,
            bool detectEmptyHtml = false)
            where T : BaseEntity, ILocalizedEntity
        {
            var invoker = keySelector.CompileFast();
            return EngineContext.Current.Resolve<LocalizedEntityHelper>().GetLocalizedValue(
                entity,
                entity.Id,
                entity.GetEntityName(),
                invoker.Property.Name,
                (Func<T, TProp>)invoker,
                language,
                returnDefaultValue,
                ensureTwoPublishedLanguages,
                detectEmptyHtml);
        }

        #endregion

        #region GetLocalized ICategoryNode

        /// <summary>
        /// Get localized property of an <see cref="ICategoryNode"/> instance
        /// </summary>
        /// <param name="node">Node</param>
        /// <param name="keySelector">Key selector</param>
        /// <returns>Localized property</returns>
        public static LocalizedValue<string> GetLocalized(this ICategoryNode node, Expression<Func<ICategoryNode, string>> keySelector)
        {
            var invoker = keySelector.CompileFast();
            return EngineContext.Current.Resolve<LocalizedEntityHelper>().GetLocalizedValue(
                node,
                node.Id,
                "Category",
                invoker.Property.Name,
                (Func<ICategoryNode, string>)invoker,
                null);
        }

        /// <summary>
        /// Get localized property of an <see cref="ICategoryNode"/> instance
        /// </summary>
        /// <param name="node">Node</param>
        /// <param name="keySelector">Key selector</param>
        /// /// <param name="languageId">Language identifier</param>
        /// <returns>Localized property</returns>
        public static LocalizedValue<string> GetLocalized(this ICategoryNode node, Expression<Func<ICategoryNode, string>> keySelector, int languageId)
        {
            var invoker = keySelector.CompileFast();
            return EngineContext.Current.Resolve<LocalizedEntityHelper>().GetLocalizedValue(
                node,
                node.Id,
                nameof(Category),
                invoker.Property.Name,
                (Func<ICategoryNode, string>)invoker,
                languageId);
        }

        /// <summary>
        /// Get localized property of an <see cref="ICategoryNode"/> instance
        /// </summary>
        /// <param name="node">Node</param>
        /// <param name="keySelector">Key selector</param>
        /// /// <param name="language">Language</param>
        /// <returns>Localized property</returns>
        public static LocalizedValue<string> GetLocalized(this ICategoryNode node, Expression<Func<ICategoryNode, string>> keySelector, Language language)
        {
            var invoker = keySelector.CompileFast();
            return EngineContext.Current.Resolve<LocalizedEntityHelper>().GetLocalizedValue(
                node,
                node.Id,
                nameof(Category),
                invoker.Property.Name,
                (Func<ICategoryNode, string>)invoker,
                language);
        }

        #endregion

        #region GetLocalized ISettings

        /// <summary>
        /// Get localized property of an <see cref="ISettings"/> implementation
        /// </summary>
        /// <param name="settings">The settings instance</param>
        /// <param name="keySelector">Key selector</param>
        /// <returns>Localized property</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LocalizedValue<string> GetLocalizedSetting<TSetting>(this TSetting settings,
            Expression<Func<TSetting, string>> keySelector,
            int? storeId = null,
            bool returnDefaultValue = true,
            bool ensureTwoPublishedLanguages = true,
            bool detectEmptyHtml = false)
            where TSetting : class, ISettings
        {
            return GetLocalizedSetting(settings, keySelector, null, storeId, returnDefaultValue, ensureTwoPublishedLanguages, detectEmptyHtml);
        }

        /// <summary>
        /// Get localized property of an <see cref="ISettings"/> implementation
        /// </summary>
        /// <param name="settings">The settings instance</param>
        /// <param name="keySelector">Key selector</param>
        /// <param name="requestLanguageIdOrObj">Language id, <see cref="Language"/> object instance or <c>null</c></param>
        /// <returns>Localized property</returns>
        public static LocalizedValue<string> GetLocalizedSetting<TSetting>(this TSetting settings,
            Expression<Func<TSetting, string>> keySelector,
            object requestLanguageIdOrObj, // Id or Language
            int? storeId = null,
            bool returnDefaultValue = true,
            bool ensureTwoPublishedLanguages = true,
            bool detectEmptyHtml = false)
            where TSetting : class, ISettings
        {
            var helper = EngineContext.Current.Resolve<LocalizedEntityHelper>();
            var invoker = keySelector.CompileFast();

            if (storeId == null)
            {
                storeId = EngineContext.Current.Resolve<IStoreContext>().CurrentStore.Id;
            }

            // Make fallback only when storeId is 0 and the paramter says so.
            var localizedValue = GetValue(storeId.Value, storeId == 0 && returnDefaultValue);

            if (storeId > 0 && string.IsNullOrEmpty(localizedValue.Value))
            {
                localizedValue = GetValue(0, returnDefaultValue);
            }

            return localizedValue;

            LocalizedValue<string> GetValue(int id /* storeId */, bool doFallback)
            {
                return helper.GetLocalizedValue(
                    settings,
                    id,
                    typeof(TSetting).Name,
                    invoker.Property.Name,
                    (Func<TSetting, string>)invoker,
                    requestLanguageIdOrObj,
                    doFallback,
                    ensureTwoPublishedLanguages,
                    detectEmptyHtml);
            }
        }

        #endregion

        #region GetLocalizedEnum

        /// <summary>
        /// Get localized value of enum
        /// </summary>
        /// <typeparam name="T">Enum</typeparam>
        /// <param name="enumValue">Enum value</param>
        /// <param name="localizationService">Localization service</param>
        /// <param name="workContext">Work context</param>
        /// <param name="hint">Whether to load the hint.</param>
        /// <returns>Localized value</returns>
        public static string GetLocalizedEnum<T>(this T enumValue, ILocalizationService localizationService, IWorkContext workContext, bool hint = false)
            where T : struct
        {
            Guard.NotNull(workContext, nameof(workContext));

            return GetLocalizedEnum<T>(enumValue, localizationService, workContext.WorkingLanguage.Id, hint);
        }

        /// <summary>
        /// Get localized value of enum
        /// </summary>
        /// <typeparam name="T">Enum</typeparam>
        /// <param name="enumValue">Enum value</param>
        /// <param name="localizationService">Localization service</param>
        /// <param name="languageId">Language identifier</param>
        /// <param name="hint">Whether to load the hint.</param>
        /// <returns>Localized value</returns>
        public static string GetLocalizedEnum<T>(this T enumValue, ILocalizationService localizationService, int languageId = 0, bool hint = false)
            where T : struct
        {
            Guard.NotNull(localizationService, nameof(localizationService));

            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException("T must be an enumerated type.");
            }

            var resourceName = string.Format("Enums.{0}.{1}",
                typeof(T).ToString(),
                enumValue.ToString());

            if (hint)
            {
                resourceName += ".Hint";
            }

            var result = localizationService.GetResource(resourceName, languageId, false, "", true);

            // Set default value if required.
            if (string.IsNullOrEmpty(result))
            {
                result = Inflector.Titleize(enumValue.ToString());
            }

            return result;
        }

        #endregion

        #region Plugin Localization

        /// <summary>
        /// Delete a locale resource
        /// </summary>
        /// <param name="plugin">Plugin</param>
        /// <param name="resourceName">Resource name</param>
        public static void DeletePluginLocaleResource(this BasePlugin plugin, string resourceName)
        {
            var localizationService = EngineContext.Current.Resolve<ILocalizationService>();
            var languageService = EngineContext.Current.Resolve<ILanguageService>();
            DeletePluginLocaleResource(plugin, localizationService, languageService, resourceName);
        }

        /// <summary>
        /// Delete a locale resource
        /// </summary>
        /// <param name="plugin">Plugin</param>
        /// <param name="localizationService">Localization service</param>
        /// <param name="languageService">Language service</param>
        /// <param name="resourceName">Resource name</param>
        public static void DeletePluginLocaleResource(this BasePlugin plugin,
            ILocalizationService localizationService,
            ILanguageService languageService,
            string resourceName)
        {
            // actually plugin instance is not required
            if (plugin == null)
                throw new ArgumentNullException(nameof(plugin));

            if (localizationService == null)
                throw new ArgumentNullException(nameof(localizationService));

            if (languageService == null)
                throw new ArgumentNullException(nameof(languageService));

            foreach (var lang in languageService.GetAllLanguages(true))
            {
                var lsr = localizationService.GetLocaleStringResourceByName(resourceName, lang.Id, false);
                if (lsr != null)
                    localizationService.DeleteLocaleStringResource(lsr);
            }
        }

        /// <summary>
        /// Add a locale resource (if new) or update an existing one
        /// </summary>
        /// <param name="plugin">Plugin</param>
        /// <param name="resourceName">Resource name</param>
        /// <param name="resourceValue">Resource value</param>
        public static void AddOrUpdatePluginLocaleResource(this BasePlugin plugin,
            string resourceName, string resourceValue)
        {
            var localizationService = EngineContext.Current.Resolve<ILocalizationService>();
            var languageService = EngineContext.Current.Resolve<ILanguageService>();
            AddOrUpdatePluginLocaleResource(plugin, localizationService,
                languageService, resourceName, resourceValue);
        }

        /// <summary>
        /// Add a locale resource (if new) or update an existing one
        /// </summary>
        /// <param name="plugin">Plugin</param>
        /// <param name="localizationService">Localization service</param>
        /// <param name="languageService">Language service</param>
        /// <param name="resourceName">Resource name</param>
        /// <param name="resourceValue">Resource value</param>
        public static void AddOrUpdatePluginLocaleResource(this BasePlugin plugin,
            ILocalizationService localizationService,
            ILanguageService languageService,
            string resourceName,
            string resourceValue)
        {
            // Actually plugin instance is not required
            if (plugin == null)
                throw new ArgumentNullException("plugin");
            if (localizationService == null)
                throw new ArgumentNullException("localizationService");
            if (languageService == null)
                throw new ArgumentNullException("languageService");

            foreach (var lang in languageService.GetAllLanguages(true))
            {
                var lsr = localizationService.GetLocaleStringResourceByName(resourceName, lang.Id, false);
                if (lsr == null)
                {
                    lsr = new LocaleStringResource()
                    {
                        LanguageId = lang.Id,
                        ResourceName = resourceName,
                        ResourceValue = resourceValue,
                        IsFromPlugin = true
                    };
                    localizationService.InsertLocaleStringResource(lsr);
                }
                else
                {
                    lsr.ResourceValue = resourceValue;
                    localizationService.UpdateLocaleStringResource(lsr);
                }
            }
        }

        public static void AddPluginLocaleResource(this BasePlugin plugin, ILocalizationService localizationService, string resourceName, string value, int languageID)
        {
            if (languageID != 0)
            {
                localizationService.InsertLocaleStringResource(new LocaleStringResource
                {
                    LanguageId = languageID,
                    ResourceName = resourceName,
                    ResourceValue = value,
                    IsFromPlugin = true
                });
            }
        }

        /// <summary>
        /// Get localized property value of a plugin
        /// </summary>
        /// <typeparam name="T">Plugin</typeparam>
        /// <param name="plugin">Plugin</param>
        /// <param name="localizationService">Localization service</param>
        /// <param name="propertyName">Name of the property</param>
        /// <param name="languageId">Language identifier</param>
        /// <param name="returnDefaultValue">A value indicating whether to return default value (if localized is not found)</param>
        /// <returns>Localized value</returns>
        public static string GetLocalizedValue<T>(this T plugin, ILocalizationService localizationService, string propertyName, int languageId = 0, bool returnDefaultValue = true)
            where T : IPlugin
        {
            if (plugin == null)
                throw new ArgumentNullException(nameof(plugin));

            if (plugin.PluginDescriptor == null)
                throw new ArgumentNullException("PluginDescriptor cannot be loaded");

            return plugin.PluginDescriptor.GetLocalizedValue(localizationService, propertyName, languageId, returnDefaultValue);
        }

        /// <summary>
        /// Get localized property value of a plugin
        /// </summary>
        /// <param name="descriptor">Plugin descriptor</param>
        /// <param name="localizationService">Localization service</param>
        /// <param name="propertyName">Name of the property</param>
        /// <param name="languageId">Language identifier</param>
        /// <param name="returnDefaultValue">A value indicating whether to return default value (if localized is not found)</param>
        /// <returns>Localized value</returns>
        public static string GetLocalizedValue(this PluginDescriptor descriptor, ILocalizationService localizationService, string propertyName, int languageId = 0, bool returnDefaultValue = true)
        {
            if (localizationService == null)
                throw new ArgumentNullException(nameof(localizationService));

            if (descriptor == null)
                throw new ArgumentNullException(nameof(descriptor));

            if (propertyName == null)
                throw new ArgumentNullException(nameof(propertyName));

            string systemName = descriptor.SystemName;
            string resourceName = string.Format("Plugins.{0}.{1}", propertyName, systemName);
            string result = localizationService.GetResource(resourceName, languageId, false, "", true);

            if (String.IsNullOrEmpty(result) && returnDefaultValue)
            {
                var fastProp = FastProperty.GetProperty(descriptor.GetType(), propertyName);
                if (fastProp != null)
                {
                    result = fastProp.GetValue(descriptor) as string;
                }
            }

            return result;
        }

        /// <summary>
        /// Save localized plugin descriptor value
        /// </summary>
        /// <typeparam name="T">Plugin</typeparam>
        /// <param name="plugin">Plugin</param>
        /// <param name="localizationService">Localization service</param>
        /// <param name="languageId">Language identifier</param>
		/// <param name="propertyName">Name of the property</param>
        /// <param name="value">Localized value</param>
        public static void SaveLocalizedValue<T>(this T plugin, ILocalizationService localizationService, int languageId,
            string propertyName, string value) where T : IPlugin
        {
            if (plugin == null)
                throw new ArgumentNullException("plugin");

            if (plugin.PluginDescriptor == null)
                throw new ArgumentNullException("PluginDescriptor cannot be loaded");

            plugin.PluginDescriptor.SaveLocalizedValue(localizationService, languageId, propertyName, value);
        }

        /// <summary>
		/// Save localized plugin descriptor value
        /// </summary>
        /// <param name="descriptor">Plugin</param>
        /// <param name="localizationService">Localization service</param>
        /// <param name="languageId">Language identifier</param>
		/// <param name="propertyName">Name of the property</param>
        /// <param name="value">Localized value</param>
        public static void SaveLocalizedValue(this PluginDescriptor descriptor, ILocalizationService localizationService, int languageId,
            string propertyName, string value)
        {
            if (localizationService == null)
                throw new ArgumentNullException(nameof(localizationService));

            if (languageId == 0)
                throw new ArgumentOutOfRangeException(nameof(languageId), "Language ID should not be 0");

            if (descriptor == null)
                throw new ArgumentNullException(nameof(descriptor));

            if (propertyName == null)
                throw new ArgumentNullException(nameof(propertyName));

            string systemName = descriptor.SystemName;
            string resourceName = string.Format("Plugins.{0}.{1}", propertyName, systemName);
            var resource = localizationService.GetLocaleStringResourceByName(resourceName, languageId, false);

            if (resource != null)
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    //delete
                    localizationService.DeleteLocaleStringResource(resource);
                }
                else
                {
                    //update
                    resource.ResourceValue = value;
                    localizationService.UpdateLocaleStringResource(resource);
                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    //insert
                    resource = new LocaleStringResource()
                    {
                        LanguageId = languageId,
                        ResourceName = resourceName,
                        ResourceValue = value,
                    };
                    localizationService.InsertLocaleStringResource(resource);
                }
            }
        }

        /// <summary>
        /// Import language resources from XML file
        /// </summary>
        /// <param name="language">Language</param>
        /// <param name="xml">XML</param>
        public static void ImportResourcesFromXml(this ILocalizationService service,
            Language language,
            string xml,
            string rootKey = null,
            bool sourceIsPlugin = false,
            ImportModeFlags mode = ImportModeFlags.Insert | ImportModeFlags.Update,
            bool updateTouchedResources = false)
        {
            if (language == null)
                throw new ArgumentNullException(nameof(language));

            if (String.IsNullOrEmpty(xml))
                return;

            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);

            service.ImportResourcesFromXml(language, xmlDoc, rootKey, sourceIsPlugin, mode, updateTouchedResources);
        }

        #endregion
    }
}
