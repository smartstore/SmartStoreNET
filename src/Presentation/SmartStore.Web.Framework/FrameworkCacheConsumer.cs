using System;
using System.Web;
using SmartStore.Core.Caching;
using SmartStore.Core.Domain.Configuration;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Seo;
using SmartStore.Core.Domain.Tax;
using SmartStore.Core.Domain.Themes;
using SmartStore.Core.Events;
using SmartStore.Core.Themes;
using SmartStore.Utilities;
using SmartStore.Web.Framework.Theming.Assets;

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
<<<<<<< HEAD
        public const string THEMEVARS_KEY = "pres:themevars-{0}-{1}";
		public const string THEMEVARS_THEME_KEY = "pres:themevars-{0}";
		
=======
        public const string THEMEVARS_KEY = "fw:themevars-{0}-{1}";
		public const string THEMEVARS_THEME_KEY = "fw:themevars-{0}";	
>>>>>>> upstream/3.x
        
        /// <summary>
        /// Key for tax display type caching
        /// </summary>
        /// <remarks>
        /// {0} : customer role ids
        /// {1} : store identifier
        /// </remarks>
        public const string CUSTOMERROLES_TAX_DISPLAY_TYPES_KEY = "fw:customerroles:taxdisplaytypes-{0}-{1}";
        public const string CUSTOMERROLES_TAX_DISPLAY_TYPES_PATTERN_KEY = "fw:customerroles:taxdisplaytypes";

		/// <summary>
		/// Key for Topic SE names
		/// </summary>
		/// <remarks>
		/// {0} : systemname
		/// {1} : language id
		/// {2} : store id
		/// </remarks>
		public const string TOPIC_SENAME_BY_SYSTEMNAME = "fw:topic:sename.bysystemname-{0}-{1}-{2}";
		public const string TOPIC_SENAME_PATTERN_KEY = "fw:topic:sename*";

		private readonly ICacheManager _cacheManager;
		private readonly IAssetCache _assetCache;

		public FrameworkCacheConsumer(ICacheManager cacheManager, IAssetCache assetCache)
        {
			_cacheManager = cacheManager;
			_assetCache = assetCache;
        }

        public void HandleEvent(EntityInserted<ThemeVariable> eventMessage)
        {
			HttpRuntime.Cache.Remove(BuildThemeVarsCacheKey(eventMessage.Entity));
        }

        public void HandleEvent(EntityUpdated<ThemeVariable> eventMessage)
        {
			HttpRuntime.Cache.Remove(BuildThemeVarsCacheKey(eventMessage.Entity));
        }

        public void HandleEvent(EntityDeleted<ThemeVariable> eventMessage)
        {
			HttpRuntime.Cache.Remove(BuildThemeVarsCacheKey(eventMessage.Entity));
        }

		public void HandleEvent(ThemeTouchedEvent eventMessage)
		{
			var cacheKey = BuildThemeVarsCacheKey(eventMessage.ThemeName, 0);
			HttpRuntime.Cache.RemoveByPattern(cacheKey);
		}

<<<<<<< HEAD
=======
		public override void OnAfterSave(IHookedEntity entry)
		{
			if (entry.Entity is ThemeVariable)
			{
				var themeVar = entry.Entity as ThemeVariable;
				AddEvictableThemeScope(themeVar.Theme, themeVar.StoreId);
			}
			else if (entry.Entity is CustomerRole)
			{
				_cacheManager.RemoveByPattern(CUSTOMERROLES_TAX_DISPLAY_TYPES_PATTERN_KEY);
			}
			else if (entry.Entity is UrlRecord ur)
			{
				var entityName = ur.EntityName.ToLowerInvariant();
				if (entityName == "topic")
				{
					_cacheManager.RemoveByPattern(TOPIC_SENAME_PATTERN_KEY);
				}
			}
			else if (entry.Entity is Setting && entry.InitialState == EntityState.Modified)
			{
				var setting = entry.Entity as Setting;
				if (setting.Name.IsCaseInsensitiveEqual(TypeHelper.NameOf<TaxSettings>(x => x.TaxDisplayType, true)))
				{
					_cacheManager.RemoveByPattern(CUSTOMERROLES_TAX_DISPLAY_TYPES_PATTERN_KEY); // depends on TaxSettings.TaxDisplayType
				}
			}
			else
			{
				throw new NotImplementedException();
			}	
		}
>>>>>>> upstream/3.x

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
				return HttpRuntime.Cache.BuildScopedKey(THEMEVARS_KEY.FormatInvariant(themeName, storeId));
			}

			return HttpRuntime.Cache.BuildScopedKey(THEMEVARS_THEME_KEY.FormatInvariant(themeName));
        }

        #endregion

	}

}
