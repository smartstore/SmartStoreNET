//Contributor:  Nicholas Mayne


namespace SmartStore.Services.Authentication.External
{
    public partial interface IExternalProviderAuthorizer
    {
        AuthorizeState Authorize(string returnUrl);
    }
}