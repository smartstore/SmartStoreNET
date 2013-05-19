using SmartStore.Core.Configuration;

namespace SmartStore.Plugin.ExternalAuth.Facebook
{
    public class FacebookExternalAuthSettings : ISettings
    {
        public string ClientKeyIdentifier { get; set; }
        public string ClientSecret { get; set; }
    }
}
