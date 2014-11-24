using System.Collections.Generic;
using System.Web.Routing;
using SmartStore.Core.Plugins;

namespace SmartStore.Services.Cms
{
    /// <summary>
    /// Provides an interface for creating widgets
    /// </summary>
    public partial interface IWidget : IProvider, IUserEditable
    {
        /// <summary>
        /// Gets widget zones where this widget should be rendered
        /// </summary>
        /// <returns>Widget zones</returns>
        IList<string> GetWidgetZones();

        /// <summary>
        /// Gets a route for displaying a widget
        /// </summary>
        /// <param name="widgetZone">Widget zone where it's displayed</param>
		/// <param name="model">The model of the parent view context</param>
		/// <param name="storeId">The id of the current store</param>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        void GetDisplayWidgetRoute(string widgetZone, object model, int storeId, out string actionName, out string controllerName, out RouteValueDictionary routeValues);
    }
}
