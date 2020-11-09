using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using SmartStore.Core;
using SmartStore.Core.Configuration;
using SmartStore.Core.Domain.Localization;

namespace SmartStore.Services.Localization
{
    /// <summary>
    /// Localized entity service interface
    /// </summary>
    public partial interface ILocalizedEntityService : IScopedService
    {
        /// <summary>
        /// Deletes a localized property
        /// </summary>
        /// <param name="localizedProperty">Localized property</param>
        void DeleteLocalizedProperty(LocalizedProperty localizedProperty);

        /// <summary>
        /// Gets a localized property
        /// </summary>
        /// <param name="localizedPropertyId">Localized property identifier</param>
        /// <returns>Localized property</returns>
        LocalizedProperty GetLocalizedPropertyById(int localizedPropertyId);

        /// <summary>
        /// Find localized value
        /// </summary>
        /// <param name="languageId">Language identifier</param>
        /// <param name="entityId">Entity identifier</param>
        /// <param name="localeKeyGroup">Locale key group</param>
        /// <param name="localeKey">Locale key</param>
        /// <returns>Found localized value</returns>
        string GetLocalizedValue(int languageId, int entityId, string localeKeyGroup, string localeKey);

        /// <summary>
        /// Gets localized properties
        /// </summary>
        /// <param name="entityId">Entity identifier</param>
        /// <param name="localeKeyGroup">Locale key group</param>
        /// <returns>Localized properties</returns>
        IList<LocalizedProperty> GetLocalizedProperties(int entityId, string localeKeyGroup);

        /// <summary>
        /// Prefetches a collection of localized properties for a range of entities in one go
        /// and caches them for the duration of the current request.
        /// </summary>
        /// <param name="localeKeyGroup">Locale key group (scope)</param>
        /// <param name="entityIds">
        /// The entity ids to prefetch translations for. Can be null,
        /// in which case all translations for the requested scope are loaded.
        /// </param>
        /// <param name="isRange">Whether <paramref name="entityIds"/> represents a range of ids (perf).</param>
        /// <param name="isSorted">Whether <paramref name="entityIds"/> is already sorted (perf).</param>
        /// <returns>Localized property collection</returns>
        /// <remarks>
        /// Be careful not to load large amounts of data at once (e.g. for "Product" scope with large range).
        /// </remarks>
        void PrefetchLocalizedProperties(string localeKeyGroup, int languageId, int[] entityIds, bool isRange = false, bool isSorted = false);

        /// <summary>
        /// Gets a collection of localized properties for a range of entities in one go.
        /// </summary>
        /// <param name="localeKeyGroup">Locale key group (scope)</param>
        /// <param name="entityIds">
        /// The entity ids to load translations for. Can be null,
        /// in which case all translations for the requested scope are loaded.
        /// </param>
        /// <param name="isRange">Whether <paramref name="entityIds"/> represents a range of ids (perf).</param>
        /// <param name="isSorted">Whether <paramref name="entityIds"/> is already sorted (perf).</param>
        /// <returns>Localized property collection</returns>
        /// <remarks>
        /// Be careful not to load large amounts of data at once (e.g. for "Product" scope with large range).
        /// </remarks>
        LocalizedPropertyCollection GetLocalizedPropertyCollection(string localeKeyGroup, int[] entityIds, bool isRange = false, bool isSorted = false);

        /// <summary>
        /// Inserts a localized property
        /// </summary>
        /// <param name="localizedProperty">Localized property</param>
        void InsertLocalizedProperty(LocalizedProperty localizedProperty);

        /// <summary>
        /// Updates the localized property
        /// </summary>
        /// <param name="localizedProperty">Localized property</param>
        void UpdateLocalizedProperty(LocalizedProperty localizedProperty);

        /// <summary>
        /// Save localized value
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="entity">Entity</param>
        /// <param name="keySelector">Key selector</param>
        /// <param name="value">Locale value</param>
        /// <param name="languageId">Language ID</param>
        void SaveLocalizedValue<T>(
            T entity,
            Expression<Func<T, string>> keySelector,
            string value,
            int languageId) where T : BaseEntity, ILocalizedEntity;


        /// <summary>
        /// Save localized value
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <typeparam name="TPropType">Property type</typeparam>
        /// <param name="entity">Entity</param>
        /// <param name="keySelector">Key selector</param>
        /// <param name="value">Locale value</param>
        /// <param name="languageId">Language ID</param>
        void SaveLocalizedValue<T, TPropType>(
           T entity,
           Expression<Func<T, TPropType>> keySelector,
           TPropType value,
           int languageId) where T : BaseEntity, ILocalizedEntity;

        /// <summary>
        /// Save localized setting value
        /// </summary>
        /// <typeparam name="TSetting">Setting impl type</typeparam>
        /// <typeparam name="TPropType">Property type</typeparam>
        /// <param name="settings">Settings instance</param>
        /// <param name="keySelector">Key selector</param>
        /// <param name="value">Locale value</param>
        /// <param name="languageId">Language ID</param>
        /// <param name="storeId">Store ID</param>
        void SaveLocalizedSetting<TSetting, TPropType>(
           TSetting settings,
           Expression<Func<TSetting, TPropType>> keySelector,
           TPropType value,
           int languageId,
           int storeId = 0) where TSetting : class, ISettings;
    }
}
