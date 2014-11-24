using SmartStore.Core.Configuration;

namespace SmartStore.FacebookAuth
{
    public class FacebookExternalAuthSettings : ISettings
    {
        public string ClientKeyIdentifier { get; set; }
        public string ClientSecret { get; set; }
    }
}
