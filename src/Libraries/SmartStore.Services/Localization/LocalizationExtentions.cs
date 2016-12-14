using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Xml;
using SmartStore.ComponentModel;
using SmartStore.Core;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Plugins;
using SmartStore.Utilities;

namespace SmartStore.Services.Localization
{
	public static class LocalizationExtentions
    {
		/// <summary>
        /// Get localized property of an entity
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <param name="entity">Entity</param>
        /// <param name="keySelector">Key selector</param>
        /// <returns>Localized property</returns>
        public static string GetLocalized<T>(this T entity, Expression<Func<T, string>> keySelector)
            where T : BaseEntity, ILocalizedEntity
        {
            var workContext = EngineContext.Current.Resolve<IWorkContext>();
            return GetLocalized(entity, keySelector, workContext.WorkingLanguage.Id);
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
        /// <returns>Localized property</returns>
        public static string GetLocalized<T>(this T entity, 
            Expression<Func<T, string>> keySelector, int languageId, 
            bool returnDefaultValue = true, bool ensureTwoPublishedLanguages = true) 
            where T : BaseEntity, ILocalizedEntity
        {
			return GetLocalized<T, string>(entity, keySelector, languageId, returnDefaultValue, ensureTwoPublishedLanguages);
        }
        /// <summary>
        /// Get localized property of an entity
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <typeparam name="TPropType">Property type</typeparam>
        /// <param name="entity">Entity</param>
        /// <param name="keySelector">Key selector</param>
        /// <param name="languageId">Language identifier</param>
        /// <param name="returnDefaultValue">A value indicating whether to return default value (if localized is not found)</param>
        /// <param name="ensureTwoPublishedLanguages">A value indicating whether to ensure that we have at least two published languages; otherwise, load only default value</param>
        /// <returns>Localized property</returns>
        public static TPropType GetLocalized<T, TPropType>(this T entity,
            Expression<Func<T, TPropType>> keySelector, int languageId, 
            bool returnDefaultValue = true, bool ensureTwoPublishedLanguages = true)
            where T : BaseEntity, ILocalizedEntity
        {
			if (entity == null)
                throw new ArgumentNullException("entity");

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

            TPropType result = default(TPropType);
            string resultStr = string.Empty;

            //load localized value
            string localeKeyGroup = typeof(T).Name;
            string localeKey = propInfo.Name;

            if (languageId > 0)
            {
                //ensure that we have at least two published languages
                bool loadLocalizedValue = true;
                if (ensureTwoPublishedLanguages)
                {
                    var lService = EngineContext.Current.Resolve<ILanguageService>();
                    var totalPublishedLanguages = lService.GetLanguagesCount(false);
                    loadLocalizedValue = totalPublishedLanguages >= 2;
                }

                //localized value
                if (loadLocalizedValue)
                {
                    var leService = EngineContext.Current.Resolve<ILocalizedEntityService>();
                    resultStr = leService.GetLocalizedValue(languageId, entity.Id, localeKeyGroup, localeKey);
                    if (!String.IsNullOrEmpty(resultStr))
						result = resultStr.Convert<TPropType>();
                }
            }

            //set default value if required
            if (String.IsNullOrEmpty(resultStr) && returnDefaultValue)
            {
				//var localizer = (Func<T, TPropType>)_compiledExpressions.GetOrAdd(keySelector, exp => exp.Compile()); // --> MEM LEAK
                var localizer = keySelector.Compile();
                result = localizer(entity);
            }
            
            return result;
        }

        /// <summary>
        /// Get localized value of enum
        /// </summary>
        /// <typeparam name="T">Enum</typeparam>
        /// <param name="enumValue">Enum value</param>
        /// <param name="localizationService">Localization service</param>
        /// <param name="workContext">Work context</param>
        /// <returns>Localized value</returns>
        public static string GetLocalizedEnum<T>(this T enumValue, ILocalizationService localizationService, IWorkContext workContext)
            where T : struct
        {
			Guard.NotNull(workContext, nameof(workContext));

            return GetLocalizedEnum<T>(enumValue, localizationService, workContext.WorkingLanguage.Id);
        }
        /// <summary>
        /// Get localized value of enum
        /// </summary>
        /// <typeparam name="T">Enum</typeparam>
        /// <param name="enumValue">Enum value</param>
        /// <param name="localizationService">Localization service</param>
        /// <param name="languageId">Language identifier</param>
        /// <returns>Localized value</returns>
        public static string GetLocalizedEnum<T>(this T enumValue, ILocalizationService localizationService, int languageId = 0)
            where T : struct
        {
			Guard.NotNull(localizationService, nameof(localizationService));

			if (!typeof(T).IsEnum) throw new ArgumentException("T must be an enumerated type");

            //localized value
            string resourceName = string.Format("Enums.{0}.{1}", 
                typeof(T).ToString(), 
                //Convert.ToInt32(enumValue)
                enumValue.ToString());

            string result = localizationService.GetResource(resourceName, languageId, false, "", true);

            //set default value if required
            if (String.IsNullOrEmpty(result))
                result = Inflector.Titleize(enumValue.ToString());

            return result;
        }

        /// <summary>
        /// Delete a locale resource
        /// </summary>
        /// <param name="plugin">Plugin</param>
        /// <param name="resourceName">Resource name</param>
        public static void DeletePluginLocaleResource(this BasePlugin plugin,
            string resourceName)
        {
            var localizationService = EngineContext.Current.Resolve<ILocalizationService>();
            var languageService = EngineContext.Current.Resolve<ILanguageService>();
            DeletePluginLocaleResource(plugin, localizationService,
                languageService, resourceName);
        }
        /// <summary>
        /// Delete a locale resource
        /// </summary>
        /// <param name="plugin">Plugin</param>
        /// <param name="localizationService">Localization service</param>
        /// <param name="languageService">Language service</param>
        /// <param name="resourceName">Resource name</param>
        public static void DeletePluginLocaleResource(this BasePlugin plugin,
            ILocalizationService localizationService, ILanguageService languageService,
            string resourceName)
        {
            //actually plugin instance is not required
            if (plugin == null)
                throw new ArgumentNullException("plugin");
            if (localizationService == null)
                throw new ArgumentNullException("localizationService");
            if (languageService == null)
                throw new ArgumentNullException("languageService");

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
            ILocalizationService localizationService, ILanguageService languageService, 
            string resourceName, string resourceValue)
        {
            //actually plugin instance is not required
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
				throw new ArgumentNullException("plugin");

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
				throw new ArgumentNullException("localizationService");

			if (descriptor == null)
				throw new ArgumentNullException("descriptor");

			if (propertyName == null)
				throw new ArgumentNullException("name");

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
                throw new ArgumentNullException("localizationService");

            if (languageId == 0)
                throw new ArgumentOutOfRangeException("languageId", "Language ID should not be 0");

            if (descriptor == null)
                throw new ArgumentNullException("descriptor");

			if (propertyName == null)
				throw new ArgumentNullException("name");

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
                throw new ArgumentNullException("language");

            if (String.IsNullOrEmpty(xml))
                return;

            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);

            service.ImportResourcesFromXml(language, xmlDoc, rootKey, sourceIsPlugin, mode, updateTouchedResources);
        }
    }
}
