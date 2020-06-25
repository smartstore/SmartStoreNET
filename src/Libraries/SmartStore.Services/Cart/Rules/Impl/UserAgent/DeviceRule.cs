using SmartStore.Services.Common;

namespace SmartStore.Services.Cart.Rules.Impl
{
    public class DeviceRule : ListRuleBase<string>
    {
        private readonly IUserAgent _userAgent;

        public DeviceRule(IUserAgent userAgent)
        {
            _userAgent = userAgent;
        }

        protected override string GetValue(CartRuleContext context)
        {
            return _userAgent.Device.Family.NullEmpty();
        }
    }
}
