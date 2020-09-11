using SmartStore.Core;
using SmartStore.Core.Domain.Themes;

namespace SmartStore.Services.Common
{
    public partial class MobileDeviceHelper : IMobileDeviceHelper
    {
        private readonly IUserAgent _userAgent;

        public MobileDeviceHelper(IUserAgent userAgent)
        {
            _userAgent = userAgent;
        }

        public virtual bool IsMobileDevice()
        {
            return _userAgent.IsMobileDevice && !_userAgent.IsTablet;
        }

    }
}