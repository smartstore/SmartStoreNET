//Contributor:  Nicholas Mayne


namespace SmartStore.Services.Authentication.External
{
    public partial interface IClaimsTranslator<T>
    {
        UserClaims Translate(T response);
    }
}