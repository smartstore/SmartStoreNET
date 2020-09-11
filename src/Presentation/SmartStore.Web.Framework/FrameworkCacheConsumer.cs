using System;
using System.Collections.Generic;
using System.Web;
using SmartStore.Core;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Core.Data.Hooks;
using SmartStore.Core.Domain.Configuration;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Tax;
using SmartStore.Core.Domain.Themes;
using SmartStore.Core.Events;
using SmartStore.Core.Themes;
using SmartStore.Utilities;
using SmartStore.Web.Framework.Theming.Assets;

namespace SmartStore.Web.Framework
{
    public partial class FrameworkCacheConsumer : DbSaveHook<BaseEntity>, IConsumer
    {
        /// <summary>
        /// Key for ThemeVariables caching
        /// </summary>
        /// <remarks>
        /// {0} : theme name
        /// {1} : store identifier
        /// </remarks>
        public const string THEMEVARS_KEY = "fw:themevars-{0}-{1}";
        public const string THEMEVARS_THEME_KEY = "fw:themevars-{0}";

        /// <summary>
        /// Key for tax display type caching
        /// </summary>
        /// <remarks>
        /// {0} : customer role ids
        /// {1} : store identifier
        /// </remarks>
        public const string CUSTOMERROLES_TAX_DISPLAY_TYPES_KEY = "fw:customerroles:taxdisplaytypes-{0}-{1}";
        public const string CUSTOMERROLES_TAX_DISPLAY_TYPES_PATTERN_KEY = "fw:customerroles:taxdisplaytypes*";

        private readonly ICacheManager _cacheManager;
        private readonly IAssetCache _assetCache;

        // Item1 = ThemeName, Item2 = StoreId
        private HashSet<Tuple<string, int>> _themeScopes;

        public FrameworkCacheConsumer(ICacheManager cacheManager, IAssetCache assetCache)
        {
            _cacheManager = cacheManager;
            _assetCache = assetCache;
        }

        public void HandleEvent(ThemeTouchedEvent eventMessage)
        {
            var cacheKey = BuildThemeVarsCacheKey(eventMessage.ThemeName, 0);
            HttpRuntime.Cache.RemoveByPattern(cacheKey);
        }

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

        public override void OnAfterSaveCompleted()
        {
            FlushThemeVarsCacheEviction();
        }

        #region Helpers

        private void AddEvictableThemeScope(string themeName, int storeId)
        {
            if (_themeScopes == null)
                _themeScopes = new HashSet<Tuple<string, int>>();
            _themeScopes.Add(new Tuple<string, int>(themeName, storeId));
        }

        private void FlushThemeVarsCacheEviction()
        {
            if (_themeScopes == null || _themeScopes.Count == 0)
                return;

            foreach (var scope in _themeScopes)
            {
                HttpRuntime.Cache.Remove(BuildThemeVarsCacheKey(scope.Item1 /* ThemeName */, scope.Item2 /* StoreId */));
            }

            _themeScopes.Clear();
        }

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
