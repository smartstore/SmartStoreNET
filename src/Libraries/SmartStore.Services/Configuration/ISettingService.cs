using System.Collections.Generic;
using SmartStore.Core.Configuration;
using SmartStore.Core.Domain.Configuration;

namespace SmartStore.Services.Configuration
{
    /// <summary>
    /// Setting service interface
    /// </summary>
    public partial interface ISettingService
    {
        /// <summary>
        /// Gets a setting by identifier
        /// </summary>
        /// <param name="settingId">Setting identifier</param>
        /// <returns>Setting</returns>
        Setting GetSettingById(int settingId);

        /// <summary>
        /// Get setting value by key
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="key">Key</param>
        /// <param name="defaultValue">Default value</param>
		/// <param name="storeId">Store identifier</param>
        /// <returns>Setting value</returns>
		T GetSettingByKey<T>(string key, T defaultValue = default(T), int storeId = 0);

		/// <summary>
		/// Gets all settings
		/// </summary>
		/// <returns>Settings</returns>
		IList<Setting> GetAllSettings();

		/// <summary>
		/// Load settings
		/// </summary>
		/// <typeparam name="T">Type</typeparam>
		/// <param name="storeId">Store identifier for which settigns should be loaded</param>
		T LoadSetting<T>(int storeId = 0) where T : ISettings, new();

        /// <summary>
        /// Set setting value
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
		/// <param name="storeId">Store identifier</param>
        /// <param name="clearCache">A value indicating whether to clear cache after setting update</param>
        void SetSetting<T>(string key, T value, int storeId = 0, bool clearCache = true);

        /// <summary>
        /// Save settings object
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="settingInstance">Setting instance</param>
        void SaveSetting<T>(T settingInstance) where T : ISettings, new();

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
