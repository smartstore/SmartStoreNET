//Contributor:  Nicholas Mayne

using System.Collections.Generic;
using System.Web;
using SmartStore.Core.Infrastructure;

namespace SmartStore.Services.Authentication.External
{
    public static partial class ExternalAuthorizerHelper
    {
        private static HttpSessionStateBase GetSession()
        {
            var session = EngineContext.Current.Resolve<HttpSessionStateBase>();
            return session;
        }

        public static void StoreParametersForRoundTrip(OpenAuthenticationParameters parameters)
        {
            var session = GetSession();
            session["sm.externalauth.parameters"] = parameters;
        }
        public static OpenAuthenticationParameters RetrieveParametersFromRoundTrip(bool removeOnRetrieval)
        {
            var session = GetSession();
            var parameters = session["sm.externalauth.parameters"];
            if (parameters != null && removeOnRetrieval)
                RemoveParameters();

            return parameters as OpenAuthenticationParameters;
        }

        public static void RemoveParameters()
        {
            var session = GetSession();
            session.Remove("sm.externalauth.parameters");
        }
    }
}