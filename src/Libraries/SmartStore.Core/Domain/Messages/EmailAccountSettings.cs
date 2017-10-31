using SmartStore.Core.Configuration;

namespace SmartStore.Core.Domain.Messages
{
    public class EmailAccountSettings : ISettings
    {
        /// <summary>
        /// Gets or sets a store default email account identifier
        /// </summary>
        public int DefaultEmailAccountId { get; set; }

		/// <summary>
		/// Gets or sets a folder where mail messages should be saved (instead of sending them).
		/// For debug and test purposes only.
		/// </summary>
		public string PickupDirectoryLocation { get; set; }
	}
}
