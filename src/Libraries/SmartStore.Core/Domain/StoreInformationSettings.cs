using SmartStore.Core.Configuration;

namespace SmartStore.Core.Domain
{
    public class StoreInformationSettings : ISettings
    {
		public StoreInformationSettings()
		{
			StoreClosedAllowForAdmins = true;
		}
		
		/// <summary>
        /// Gets or sets a value indicating whether store is closed
        /// </summary>
        public bool StoreClosed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether administrators can visit a closed store
        /// </summary>
        public bool StoreClosedAllowForAdmins { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether mini profiler should be displayed in public store (used for debugging)
        /// </summary>
        public bool DisplayMiniProfilerInPublicStore { get; set; }
    }
}
