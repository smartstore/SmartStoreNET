using System;
using System.Threading;
using System.Web;
using SmartStore.Core.Configuration;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Stores;
using SmartStore.Services.Configuration;
using SmartStore.Services.Tasks;

namespace SmartStore.Services.Seo
{
    public class XmlSitemapBuildContext
    {
        private readonly ISettingService _settingService;
        private readonly bool _isSingleStoreMode;

        public XmlSitemapBuildContext(Store store, Language[] languages, ISettingService settingService, bool isSingleStoreMode)
        {
            Guard.NotNull(store, nameof(store));
            Guard.NotEmpty(languages, nameof(languages));

            Store = store;
            Languages = languages;
            Protocol = Store.ForceSslForAllPages ? "https" : "http";

            _settingService = settingService;
            _isSingleStoreMode = isSingleStoreMode;

            RequestStoreId = _isSingleStoreMode ? 0 : store.Id;
        }

        public CancellationToken CancellationToken { get; set; }
        public ProgressCallback ProgressCallback { get; set; }
        public Store Store { get; set; }
        public int RequestStoreId { get; private set; }
        public Language[] Languages { get; set; }
        public int MaximumNodeCount { get; set; } = XmlSitemapGenerator.MaximumSiteMapNodeCount;

        public string Protocol { get; private set; }

        public T LoadSetting<T>() where T : ISettings, new()
        {
            return _settingService.LoadSetting<T>(RequestStoreId);
        }
    }
}
