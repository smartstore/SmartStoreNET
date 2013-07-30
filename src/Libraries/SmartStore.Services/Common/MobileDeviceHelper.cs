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
        private readonly HttpContextBase _httpConttext;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="workContext">Work context</param>
		/// <param name="storeContext">Store context</param>
        public MobileDeviceHelper(ThemeSettings themeSettings, IWorkContext workContext,
			IStoreContext storeContext, HttpContextBase httpContext)
        {
			this._themeSettings = themeSettings;
            this._workContext = workContext;
			this._storeContext = storeContext;
            this._httpConttext = httpContext;
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
            if (_themeSettings.EmulateMobileDevice)
                return true;

            //comment the code below if you want tablets to be recognized as mobile devices.
            //SmartStore.NET uses the free edition of the 51degrees.mobi library for detecting browser mobile properties.
            //by default this property (IsTablet) is always false. you will need the premium edition in order to get it supported.
            bool isTablet = false;
            if (bool.TryParse(_httpConttext.Request.Browser["IsTablet"], out isTablet) && isTablet)
                return false;

            if (_httpConttext.Request.Browser.IsMobileDevice)
                return true;

            return false;
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