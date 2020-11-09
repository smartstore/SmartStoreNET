using SmartStore.Core.Configuration;

namespace SmartStore.Core.Domain.Common
{
    public class CommonSettings : ISettings
    {
        public bool UseSystemEmailForContactUsForm { get; set; }

        public bool UseStoredProceduresIfSupported { get; set; } = true;

        public bool HideAdvertisementsOnAdminArea { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to display a warning if java-script is disabled
        /// </summary>
        public bool DisplayJavaScriptDisabledWarning { get; set; }

        public bool AutoUpdateEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets the page size for the entity picker
        /// </summary>
        public int EntityPickerPageSize { get; set; } = 48;

        /// <summary>
        /// Gets or sets the maximum age of schedule history entries (in days).
        /// </summary>
        public int MaxScheduleHistoryAgeInDays { get; set; } = 30;

        /// <summary>
        /// Gets or sets the maximum age of log entries (in days).
        /// </summary>
        public int MaxLogAgeInDays { get; set; } = 7;

        /// <summary>
        /// Gets or sets the maximum number of schedule history entries per task.
        /// </summary>
        public int MaxNumberOfScheduleHistoryEntries { get; set; } = 100;

        /// <summary>
        /// Gets or sets the maximum age of sent queued messages (in days).
        /// </summary>
        public int MaxQueuedMessagesAgeInDays { get; set; } = 14;

        /// <summary>
        /// Gets or sets the maximum registration age (in minutes) for automatic deletion of guests customers.
        /// </summary>
        public int MaxGuestsRegistrationAgeInMinutes { get; set; } = 1440;  // 1 day (60 * 24).
    }
}