using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using SmartStore.Core.Configuration;
using SmartStore.Core.Domain.Configuration;

namespace SmartStore.Services.Configuration
{
    public enum SaveSettingResult
    {
        Unchanged,
        Modified,
        Inserted,
        Deleted
    }

    /// <summary>
    /// Setting service interface
    /// </summary>
    public partial interface ISettingService
    {
        /// <summary>
        /// Creates a unit of work in which cache eviction is suppressed
        /// </summary>
        /// <param name="clearCache">Specifies whether the cache should be evicted completely on batch disposal</param>
        /// <returns>A disposable unit of work</returns>
        IDisposable BeginScope(bool clearCache = true);

        /// <summary>
        /// Gets a value indicating whether settings have changed during a request, making cache eviction necessary.
        /// </summary>
        /// <remarks>
        /// Cache eviction sets this member to <c>false</c>
        /// </remarks>
        bool HasChanges { get; }

        /// <summary>
        /// Gets a setting by identifier
        /// </summary>
        /// <param name="settingId">Setting identifier</param>
        /// <returns>Setting</returns>
        Setting GetSettingById(int settingId);

        /// <summary>
        /// Gets a setting by key
        /// </summary>
        /// <param name="key">Unique setting key</param>
        /// <param name="storeId">Store identifier</param>
        /// <returns>Setting</returns>
        Setting GetSettingEntityByKey(string key, int storeId = 0);

        /// <summary>
        /// Get setting value by key
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="key">Key</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="storeId">Store identifier</param>
        /// <param name="loadSharedValueIfNotFound">A value indicating whether a shared (for all stores) value should be loaded if a value specific for a certain store is not found</param>
        /// <returns>Setting value</returns>
        T GetSettingByKey<T>(string key, T defaultValue = default(T), int storeId = 0, bool loadSharedValueIfNotFound = false);

        /// <summary>
        /// Gets all settings
        /// </summary>
        /// <returns>Settings</returns>
        IList<Setting> GetAllSettings();

        /// <summary>
        /// Determines whether a setting exists
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <typeparam name="TPropType">Property type</typeparam>
        /// <param name="settings">Settings</param>
        /// <param name="keySelector">Key selector</param>
        /// <param name="storeId">Store identifier</param>
        /// <returns>true -setting exists; false - does not exist</returns>
        bool SettingExists<T, TPropType>(T settings,
            Expression<Func<T, TPropType>> keySelector, int storeId = 0)
            where T : ISettings, new();

        /// <summary>
        /// Load settings
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="storeId">Store identifier for which settigns should be loaded</param>
        T LoadSetting<T>(int storeId = 0) where T : ISettings, new();

        /// <summary>
        /// Load settings
        /// </summary>
        /// <param name="settingType">Setting class type</param>
        /// <param name="storeId">Store identifier for which settigns should be loaded</param>
        ISettings LoadSetting(Type settingType, int storeId = 0);

        /// <summary>
        /// Set setting value
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <param name="storeId">Store identifier</param>
        /// <param name="clearCache">A value indicating whether to clear cache after setting update</param>
        SaveSettingResult SetSetting<T>(string key, T value, int storeId = 0, bool clearCache = true);

        /// <summary>
        /// Save settings object
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="settings">Setting instance</param>
        /// <param name="storeId">Store identifier</param>
        /// <returns><c>true</c> when any setting property has been modified.</returns>
        bool SaveSetting<T>(T settings, int storeId = 0) where T : ISettings, new();

        /// <summary>
        /// Save settings object
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <typeparam name="TPropType">Property type</typeparam>
        /// <param name="settings">Settings</param>
        /// <param name="keySelector">Key selector</param>
        /// <param name="storeId">Store ID</param>
        /// <param name="clearCache">A value indicating whether to clear cache after setting update</param>
        /// <returns><c>true</c> when the setting property has been modified.</returns>
        SaveSettingResult SaveSetting<T, TPropType>(
            T settings,
            Expression<Func<T, TPropType>> keySelector,
            int storeId = 0,
            bool clearCache = true) where T : ISettings, new();

        /// <summary>
        /// Updates a setting property
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <typeparam name="TPropType">Property type</typeparam>
        /// <param name="settings">Settings</param>
        /// <param name="keySelector">Key selector</param>
        /// <param name="storeId">Store ID</param>
        /// <returns><c>true</c> when the setting property has been modified.</returns>
        SaveSettingResult UpdateSetting<T, TPropType>(
            T settings,
            Expression<Func<T, TPropType>> keySelector,
            bool overrideForStore,
            int storeId = 0) where T : ISettings, new();

        void InsertSetting(Setting setting, bool clearCache = true);

        void UpdateSetting(Setting setting, bool clearCache = true);

        /// <summary>
        /// Deletes a setting
        /// </summary>
        /// <param name="setting">Setting</param>
        void DeleteSetting(Setting setting);

        /// <summary>
        /// Delete all settings
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        void DeleteSetting<T>() where T : ISettings, new();

        /// <summary>
        /// Delete a settings property from storage
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <typeparam name="TPropType">Property type</typeparam>
        /// <param name="settings">Settings</param>
        /// <param name="keySelector">Key selector</param>
        /// <param name="storeId">Store ID</param>
        /// <returns><c>true</c> when the setting existed and has been deleted</returns>
        bool DeleteSetting<T, TPropType>(
            T settings,
            Expression<Func<T, TPropType>> keySelector,
            int storeId = 0) where T : ISettings, new();

        /// <summary>
        /// Delete a settings property from storage
        /// </summary>
        /// <returns><c>true</c> when the setting existed and has been deleted</returns>
        bool DeleteSetting(string key, int storeId = 0);

        /// <summary>
        /// Deletes all settings with its key beginning with rootKey.
        /// </summary>
        /// <remarks>codehint: sm-add</remarks>
        /// <returns>Number of deleted settings</returns>
        int DeleteSettings(string rootKey);

        /// <summary>
        /// Clear cache
        /// </summary>
        void ClearCache();
    }
}
