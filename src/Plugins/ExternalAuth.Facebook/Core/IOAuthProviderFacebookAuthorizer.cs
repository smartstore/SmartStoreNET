//Contributor:  Nicholas Mayne

using Facebook;
using SmartStore.Core.Domain.Customers;
using SmartStore.Services.Authentication.External;

namespace SmartStore.Plugin.ExternalAuth.Facebook.Core
{
    public interface IOAuthProviderFacebookAuthorizer : IExternalProviderAuthorizer
    {
        FacebookClient GetClient(Customer customer);
    }
}