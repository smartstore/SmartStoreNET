using System;
using SmartStore.Core.Caching;
using SmartStore.Core.Domain.Configuration;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Themes;
using SmartStore.Core.Events;
using SmartStore.Core.Themes;

namespace SmartStore.Web.Framework
{

    public class FrameworkCacheConsumer :
        IConsumer<EntityInserted<ThemeVariable>>,
        IConsumer<EntityUpdated<ThemeVariable>>,
        IConsumer<EntityDeleted<ThemeVariable>>,
        IConsumer<EntityInserted<CustomerRole>>,
        IConsumer<EntityUpdated<CustomerRole>>,
        IConsumer<EntityDeleted<CustomerRole>>,
        IConsumer<EntityUpdated<Setting>>,
		IConsumer<ThemeTouchedEvent>
    {

        /// <summary>
        /// Key for ThemeVariables caching
        /// </summary>
        /// <remarks>
        /// {0} : theme name
        /// {1} : store identifier
        /// </remarks>
        public const string THEMEVARS_LESSCSS_KEY = "sm.pres.themevars-lesscss-{0}-{1}";
		public const string THEMEVARS_LESSCSS_THEME_KEY = "sm.pres.themevars-lesscss-{0}";
		
        
        /// <summary>
        /// Key for tax display type caching
        /// </summary>
        /// <remarks>
        /// {0} : customer role ids
        /// {1} : store identifier
        /// </remarks>
        public const string CUSTOMERROLES_TAX_DISPLAY_TYPES_KEY = "sm.fw.customerroles.taxdisplaytypes-{0}-{1}";
        public const string CUSTOMERROLES_TAX_DISPLAY_TYPES_PATTERN_KEY = "sm.fw.customerroles.taxdisplaytypes";

        private readonly ICacheManager _cacheManager;
		private readonly ICacheManager _aspCache;

		public FrameworkCacheConsumer(Func<string, ICacheManager> cache)
        {
			this._cacheManager = cache("static");
			this._aspCache = cache("aspnet");
        }

        public void HandleEvent(EntityInserted<ThemeVariable> eventMessage)
        {
			_aspCache.Remove(BuildThemeVarsCacheKey(eventMessage.Entity));
        }

        public void HandleEvent(EntityUpdated<ThemeVariable> eventMessage)
        {
			_aspCache.Remove(BuildThemeVarsCacheKey(eventMessage.Entity));
        }

        public void HandleEvent(EntityDeleted<ThemeVariable> eventMessage)
        {
			_aspCache.Remove(BuildThemeVarsCacheKey(eventMessage.Entity));
        }

		public void HandleEvent(ThemeTouchedEvent eventMessage)
		{
			var cacheKey = BuildThemeVarsCacheKey(eventMessage.ThemeName, 0);
			_aspCache.RemoveByPattern(cacheKey);
		}


        public void HandleEvent(EntityDeleted<CustomerRole> eventMessage)
        {
            _cacheManager.RemoveByPattern(CUSTOMERROLES_TAX_DISPLAY_TYPES_PATTERN_KEY);
        }

        public void HandleEvent(EntityUpdated<CustomerRole> eventMessage)
        {
            _cacheManager.RemoveByPattern(CUSTOMERROLES_TAX_DISPLAY_TYPES_PATTERN_KEY);
        }

        public void HandleEvent(EntityInserted<CustomerRole> eventMessage)
        {
            _cacheManager.RemoveByPattern(CUSTOMERROLES_TAX_DISPLAY_TYPES_PATTERN_KEY);
        }

        public void HandleEvent(EntityUpdated<Setting> eventMessage)
        {
            // clear models which depend on settings
            _cacheManager.RemoveByPattern(CUSTOMERROLES_TAX_DISPLAY_TYPES_PATTERN_KEY); // depends on TaxSettings.TaxDisplayType
        }

        #region Helpers

        private static string BuildThemeVarsCacheKey(ThemeVariable entity)
        {
            return BuildThemeVarsCacheKey(entity.Theme, entity.StoreId);
        }

        internal static string BuildThemeVarsCacheKey(string themeName, int storeId)
        {
			if (storeId > 0)
			{
				return THEMEVARS_LESSCSS_KEY.FormatInvariant(themeName, storeId);
			}

			return THEMEVARS_LESSCSS_THEME_KEY.FormatInvariant(themeName);
        }

        #endregion

	}

}
