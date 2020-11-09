using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using SmartStore.Core;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.IO;
using SmartStore.Core.Plugins;

namespace SmartStore.Services.Localization
{
    /// <summary>
    /// Localization manager interface
    /// </summary>
    public partial interface ILocalizationService
    {
        /// <summary>
        /// Deletes a locale string resource
        /// </summary>
        /// <param name="localeStringResource">Locale string resource</param>
        void DeleteLocaleStringResource(LocaleStringResource localeStringResource);

        /// <summary>
        /// Deletes all string resources with its key beginning with rootKey.
        /// </summary>
        /// <param name="key">e.g. Plugins.Import.Biz</param>
        /// <returns>Number of deleted string resources</returns>
        int DeleteLocaleStringResources(string key, bool keyIsRootKey = true);

        /// <summary>
        /// Gets a locale string resource
        /// </summary>
        /// <param name="localeStringResourceId">Locale string resource identifier</param>
        /// <returns>Locale string resource</returns>
        LocaleStringResource GetLocaleStringResourceById(int localeStringResourceId);

        /// <summary>
        /// Gets a locale string resource
        /// </summary>
        /// <param name="resourceName">A string representing a resource name</param>
        /// <returns>Locale string resource</returns>
        LocaleStringResource GetLocaleStringResourceByName(string resourceName);

        /// <summary>
        /// Gets a locale string resource
        /// </summary>
        /// <param name="resourceName">A string representing a resource name</param>
        /// <param name="languageId">Language identifier</param>
        /// <param name="logIfNotFound">A value indicating whether to log error if locale string resource is not found</param>
        /// <returns>Locale string resource</returns>
        LocaleStringResource GetLocaleStringResourceByName(string resourceName, int languageId, bool logIfNotFound = true);

        /// <summary>
        /// Gets all locale string resources by language identifier
        /// </summary>
        /// <param name="languageId">Language identifier</param>
        /// <returns>Locale string resources</returns>
        IQueryable<LocaleStringResource> All(int languageId);

        /// <summary>
        /// Gets all locale string resources by language identifier starting with a prefix
        /// </summary>
        /// <param name="pattern">Pattern</param>
        /// <param name="languageId">Language identifier</param>
        /// <returns>Locale string resources matching language id and starting with <paramref name="pattern"/></returns>
        IList<LocaleStringResource> GetResourcesByPattern(string pattern, int languageId);

        /// <summary>
        /// Inserts a locale string resource
        /// </summary>
        /// <param name="localeStringResource">Locale string resource</param>
        void InsertLocaleStringResource(LocaleStringResource localeStringResource);

        /// <summary>
        /// Updates the locale string resource
        /// </summary>
        /// <param name="localeStringResource">Locale string resource</param>
        void UpdateLocaleStringResource(LocaleStringResource localeStringResource);


        /// <summary>
        /// Gets a resource string based on the specified ResourceKey property.
        /// </summary>
        /// <param name="resourceKey">A string representing a ResourceKey.</param>
        /// <param name="languageId">Language identifier</param>
        /// <param name="logIfNotFound">A value indicating whether to log error if locale string resource is not found</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="returnEmptyIfNotFound">A value indicating whether to empty string will be returned if a resource is not found and default value is set to empty string</param>
        /// <returns>A string representing the requested resource string.</returns>
        string GetResource(string resourceKey, int languageId = 0, bool logIfNotFound = true, string defaultValue = "", bool returnEmptyIfNotFound = false);

        /// <summary>
        /// Export language resources to xml
        /// </summary>
        /// <param name="language">Language</param>
        /// <returns>Result in XML format</returns>
        string ExportResourcesToXml(Language language);

        /// <summary>
        /// Import language resources from XML file
        /// </summary>
        /// <param name="language">Language</param>
        /// <param name="xmlDocument">XML document</param>
        /// <param name="rootKey">Prefix for resource key name</param>
        /// <param name="mode">Specifies whether resource should be inserted or updated (or both)</param>
        /// <param name="updateTouchedResources">Specifies whether user touched resources should also be updated</param>
        /// <returns>The number of processed (added or updated) resource entries</returns>
        int ImportResourcesFromXml(Language language,
            XmlDocument xmlDocument,
            string rootKey = null,
            bool sourceIsPlugin = false,
            ImportModeFlags mode = ImportModeFlags.Insert | ImportModeFlags.Update,
            bool updateTouchedResources = false);

        /// <summary>
        /// Creates a directory hasher used to determine plugin localization changes across app startups.
        /// </summary>
        /// <param name="pluginDescriptor">Descriptor of the plugin</param>
        /// <returns>The hasher impl</returns>
        DirectoryHasher CreatePluginResourcesHasher(PluginDescriptor pluginDescriptor);

        /// <summary>
        /// Import plugin resources from xml files in plugin's localization directory. Notes: Deletes existing resources before importing.
        /// </summary>
        /// <param name="pluginDescriptor">Descriptor of the plugin</param>
        /// <param name="targetList">Load them into the passed list rather than into database</param>
        /// <param name="updateTouchedResources">Specifies whether user touched resources should also be updated</param>	
        /// <param name="filterLanguages">Import only files for particular languages</param>
        void ImportPluginResourcesFromXml(
            PluginDescriptor pluginDescriptor,
            IList<LocaleStringResource> targetList = null,
            bool updateTouchedResources = true,
            IList<Language> filterLanguages = null);

        /// <summary>
        /// Flattens all nested <c>LocaleResource</c> child nodes into a new document
        /// </summary>
        /// <param name="source">The source xml resource file</param>
        /// <returns>
        /// Either a new document with flattened resources or - if no nesting is determined - 
        /// the original document, which was passed as <c>source</c>
        /// </returns>
        XmlDocument FlattenResourceFile(XmlDocument source);
    }
}
