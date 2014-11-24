//Contributor:  Nicholas Mayne

using System.Web.Routing;
using SmartStore.Core.Plugins;

namespace SmartStore.Services.Authentication.External
{
    /// <summary>
    /// Provides an interface for creating external authentication methods
    /// </summary>
    public partial interface IExternalAuthenticationMethod : IProvider, IUserEditable
    {   
        /// <summary>
        /// Gets a route for displaying plugin in public store
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        void GetPublicInfoRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues);
    }
}
