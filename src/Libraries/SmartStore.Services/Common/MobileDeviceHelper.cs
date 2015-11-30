using System.Web;
using SmartStore.Core;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Themes;

namespace SmartStore.Services.Common
{
    /// <summary>
    /// Mobile device helper
    /// </summary>
    public partial class MobileDeviceHelper : IMobileDeviceHelper
    {
        #region Fields

        private readonly ThemeSettings _themeSettings;
        private readonly IWorkContext _workContext;
		private readonly IStoreContext _storeContext;
		private readonly IUserAgent _userAgent;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="workContext">Work context</param>
		/// <param name="storeContext">Store context</param>
        public MobileDeviceHelper(
			ThemeSettings themeSettings, 
			IWorkContext workContext,
			IStoreContext storeContext, 
			IUserAgent userAgent)
        {
			this._themeSettings = themeSettings;
            this._workContext = workContext;
			this._storeContext = storeContext;
			this._userAgent = userAgent;
        }

        #endregion

        #region Methods


        /// <summary>
        /// Returns a value indicating whether request is made by a mobile device
        /// </summary>
        /// <param name="httpContext">HTTP context</param>
        /// <returns>Result</returns>
        public virtual bool IsMobileDevice()
        {
			return _themeSettings.EmulateMobileDevice || (_userAgent.IsMobileDevice && !_userAgent.IsTablet);
        }

        /// <summary>
        /// Returns a value indicating whether mobile devices support is enabled
        /// </summary>
        public virtual bool MobileDevicesSupported()
        {
            return _themeSettings.MobileDevicesSupported;
        }

        /// <summary>
        /// Returns a value indicating whether current customer prefer to use full desktop version (even request is made by a mobile device)
        /// </summary>
        public virtual bool CustomerDontUseMobileVersion()
        {
			return _workContext.CurrentCustomer.GetAttribute<bool>(SystemCustomerAttributeNames.DontUseMobileVersion, _storeContext.CurrentStore.Id);
        }

        #endregion
    }
}