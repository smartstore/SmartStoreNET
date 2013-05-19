using SmartStore.Core.Configuration;

namespace SmartStore.Core.Domain
{
    public class StoreInformationSettings : ISettings
    {

        /// <summary>
        /// Gets or sets a store name
        /// </summary>
        public int LogoPictureId { get; set; }

        /// <summary>
        /// Gets or sets a store name
        /// </summary>
        public string StoreName { get; set; }

        /// <summary>
        /// Gets or sets a store URL
        /// </summary>
        public string StoreUrl { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether store is closed
        /// </summary>
        public bool StoreClosed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether administrators can visit a closed store
        /// </summary>
        public bool StoreClosedAllowForAdmins { get; set; }

        // codehint: sm-delete

        /// <summary>
        /// Gets or sets a value indicating whether mini profiler should be displayed in public store (used for debugging)
        /// </summary>
        public bool DisplayMiniProfilerInPublicStore { get; set; }

    }
}
