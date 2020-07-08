using System.Linq;
using SmartStore.Rules;
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

        public static RuleValueSelectListOption[] GetDefaultValues()
        {
            return new[]
            {
                "BlackBerry",
                "Generic Feature Phone",
                "Generic Smartphone",
                "Generic Tablet",
                "HP TouchPad",
                "iPad",
                "iPhone",
                "iPod",
                "Kindle",
                "Kindle Fire",
                "Lumia",
                "Motorola",
                "Nokia",
                "Palm",
                "Spider",
                "Other"
            }
            .Select(x => new RuleValueSelectListOption { Value = x, Text = x })
            .ToArray();
        }

        protected override string GetValue(CartRuleContext context)
        {
            return _userAgent.Device.Family.NullEmpty();
        }
    }
}
