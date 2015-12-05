using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Core;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Plugins;

namespace SmartStore.Admin.Models.ExternalAuthentication
{
	public class AuthenticationMethodModel : ProviderModel, IActivatable
    {
        [SmartResourceDisplayName("Admin.Configuration.ExternalAuthenticationMethods.Fields.IsActive")]
        public bool IsActive { get; set; }
    }
}