using System;
using System.Linq;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Plugins;

namespace SmartStore.Services.Authentication.External
{
    public static class OpenAuthenticationExtentions
    {
        public static bool IsMethodActive(this Provider<IExternalAuthenticationMethod> method, ExternalAuthenticationSettings settings)
        {
            if (method == null)
                throw new ArgumentNullException("method");

            if (settings == null)
                throw new ArgumentNullException("settings");

            if (settings.ActiveAuthenticationMethodSystemNames == null)
                return false;

            return settings.ActiveAuthenticationMethodSystemNames.Contains(method.Metadata.SystemName, StringComparer.OrdinalIgnoreCase);
        }
    }
}
