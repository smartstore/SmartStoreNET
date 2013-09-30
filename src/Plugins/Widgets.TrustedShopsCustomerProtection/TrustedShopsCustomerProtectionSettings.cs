using SmartStore.Core.Configuration;

namespace SmartStore.Plugin.Widgets.TrustedShopsCustomerProtection	
{
    public class TrustedShopsCustomerProtectionSettings : ISettings
    {
        public string TrustedShopsId { get; set; }
        public bool IsTestMode { get; set; }
        public bool IsExcellenceMode { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string ProtectionMode { get; set; }
    }
}