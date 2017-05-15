using SmartStore.Core.Configuration;

namespace SmartStore.Core.Domain.Common
{
    public class CommonSettings : ISettings
    {
		public CommonSettings()
		{
			UseStoredProceduresIfSupported = true;
			AutoUpdateEnabled = true;
			EntityPickerPageSize = 48;
		}
		
		public bool UseSystemEmailForContactUsForm { get; set; }

        public bool UseStoredProceduresIfSupported { get; set; }

        public bool HideAdvertisementsOnAdminArea { get; set; }

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