using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Core.Infrastructure;
using SmartStore.Web.Framework.Localization;
using SmartStore.Web.Framework.UI;

namespace SmartStore.Web.Framework.Events
{
    /// <summary>
    /// Tabstrip created event
    /// </summary>
    public class TabStripCreated
    {
        private IList<RouteInfo> _widgets;

        public TabStripCreated(TabFactory itemFactory, string tabStripName, HtmlHelper html, object model = null)
        {
            this.TabStripName = tabStripName;
            this.Html = html;
            this.Model = model;
            this.ItemFactory = itemFactory;
        }

        public string TabStripName { get; private set; }
        public HtmlHelper Html { get; private set; }
        public object Model { get; private set; }
        public TabFactory ItemFactory { get; private set; }

        /// <summary>
        /// Renders a child action into a dynamically created special tab called 'Plugins' 
        /// </summary>
        /// <param name="routeInfo"></param>
        /// <remarks>Should only be called for admin tabstrips</remarks>
        public void AddWidget(RouteInfo routeInfo)
        {
            Guard.NotNull(routeInfo, nameof(routeInfo));

            if (_widgets == null)
            {
                _widgets = new List<RouteInfo>();
                Html.ViewContext.ViewData["Tab.{0}.Widgets".FormatInvariant(this.TabStripName)] = _widgets;

                CreateWidgetsTab();
            }

            _widgets.Add(routeInfo);
        }

        private Tab CreateWidgetsTab()
        {
            return ItemFactory.Add()
                .Text(EngineContext.Current.Resolve<IText>().Get("Admin.Plugins"))
                .Name("tab-special-plugin-widgets")
                .Icon("fa fa-puzzle-piece fa-lg fa-fw")
                .LinkHtmlAttributes(new { data_tab_name = "PLUGIN_WIDGETS" })
                .Content("TabWidgets", "Widget", new { area = "", model = this.Model, viewDataKey = "Tab.{0}.Widgets".FormatInvariant(this.TabStripName) })
                .Ajax(false)
                .Item;
        }
    }
}