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
			SitemapIncludeProducts = false;
			AutoUpdateEnabled = true;
			EntityPickerPageSize = 48;
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
        /// Gets or sets a value indicating whether to display a warning if java-script is disabled
        /// </summary>
        public bool DisplayJavaScriptDisabledWarning { get; set; }

		public bool AutoUpdateEnabled { get; set; }

		/// <summary>
		/// Gets or sets the page size for the entity picker
		/// </summary>
		public int EntityPickerPageSize { get; set; }
	}
}