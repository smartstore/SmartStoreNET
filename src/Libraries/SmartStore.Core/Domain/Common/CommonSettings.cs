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
            MaxScheduleHistoryAgeInDays = 30;
            MaxLogAgeInDays = 7;
            MaxNumberOfScheduleHistoryEntries = 100;
            MaxQueuedMessagesAgeInDays = 14;
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

        /// <summary>
        /// Gets or sets the maximum age of schedule history entries (in days).
        /// </summary>
        public int MaxScheduleHistoryAgeInDays { get; set; }

        /// <summary>
        /// Gets or sets the maximum age of log entries (in days).
        /// </summary>
        public int MaxLogAgeInDays { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of schedule history entries per task.
        /// </summary>
        public int MaxNumberOfScheduleHistoryEntries { get; set; }

        /// <summary>
        /// Gets or sets the maximum age of sent queued messages (in days).
        /// </summary>
        public int MaxQueuedMessagesAgeInDays { get; set; }
    }
}