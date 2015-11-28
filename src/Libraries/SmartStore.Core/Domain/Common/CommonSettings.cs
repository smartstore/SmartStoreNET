
using SmartStore.Core.Configuration;

namespace SmartStore.Core.Domain.Common
{
    public class CommonSettings : ISettings
    {
		public CommonSettings()
		{
			UseStoredProceduresIfSupported = true;
			SitemapEnabled = true;
			SitemapIncludeCategories = true;
			SitemapIncludeManufacturers = true;
			SitemapIncludeTopics = true;
			FullTextMode = FulltextSearchMode.ExactMatch;
			AutoUpdateEnabled = true;
		}
		
		public bool UseSystemEmailForContactUsForm { get; set; }

        public bool UseStoredProceduresIfSupported { get; set; }

        public bool HideAdvertisementsOnAdminArea { get; set; }

        public bool SitemapEnabled { get; set; }
        public bool SitemapIncludeCategories { get; set; }
        public bool SitemapIncludeManufacturers { get; set; }
        public bool SitemapIncludeProducts { get; set; }
        public bool SitemapIncludeTopics { get; set; }

        /// <summary>
        /// Gets a sets a value indicating whether to display a warning if java-script is disabled
        /// </summary>
        public bool DisplayJavaScriptDisabledWarning { get; set; }

        /// <summary>
        /// Gets a sets a value indicating whether to full-text search is supported
        /// </summary>
        public bool UseFullTextSearch { get; set; }

        /// <summary>
        /// Gets a sets a Full-Text search mode
        /// </summary>
        public FulltextSearchMode FullTextMode { get; set; }

		public bool AutoUpdateEnabled { get; set; }

    }
}