using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Mvc;

namespace SmartStore.FacebookAuth.Models
{
    public class ConfigurationModel : ModelBase
    {
        [SmartResourceDisplayName("Plugins.ExternalAuth.Facebook.ClientKeyIdentifier")]
        public string ClientKeyIdentifier { get; set; }

        [SmartResourceDisplayName("Plugins.ExternalAuth.Facebook.ClientSecret")]
        public string ClientSecret { get; set; }
    }
}