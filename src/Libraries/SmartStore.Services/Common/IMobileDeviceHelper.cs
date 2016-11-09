using System.Web;

namespace SmartStore.Services.Common
{
    /// <summary>
    /// Mobile device helper interface
    /// </summary>
    public partial interface IMobileDeviceHelper
    {
        /// <summary>
        /// Returns a value indicating whether request is made by a mobile device
        /// </summary>
        /// <returns>Result</returns>
        bool IsMobileDevice();
    }
}