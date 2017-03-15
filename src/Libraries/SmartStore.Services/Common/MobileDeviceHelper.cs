using SmartStore.Core;
using SmartStore.Core.Domain.Themes;

namespace SmartStore.Services.Common
{
    public partial class MobileDeviceHelper : IMobileDeviceHelper
    {
        private readonly ThemeSettings _themeSettings;
        private readonly IWorkContext _workContext;
		private readonly IStoreContext _storeContext;
		private readonly IUserAgent _userAgent;

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

        public virtual bool IsMobileDevice()
        {
			return _userAgent.IsMobileDevice && !_userAgent.IsTablet;
        }

    }
}