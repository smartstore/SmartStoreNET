//Contributor:  Nicholas Mayne


using SmartStore.Services.Authentication.External;

namespace SmartStore.Plugin.ExternalAuth.OpenId.Core
{
    public interface IOpenIdProviderAuthorizer : IExternalProviderAuthorizer
    {
        string EnternalIdentifier { get; set; } // mayne - refactor this out
        bool IsOpenIdCallback { get; }
    }
}