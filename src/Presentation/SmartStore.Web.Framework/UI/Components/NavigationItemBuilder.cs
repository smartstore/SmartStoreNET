using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;
using System.Web.WebPages;
using SmartStore.Utilities;

namespace SmartStore.Web.Framework.UI
{
    public abstract class NavigationItemBuilder<TItem, TBuilder> : IHideObjectMembers
        where TItem : NavigationItem
        where TBuilder : NavigationItemBuilder<TItem, TBuilder>
    {

        protected NavigationItemBuilder(TItem item)
        {
            Guard.NotNull(item, nameof(item));

            this.Item = item;
        }

        protected internal TItem Item
        {
            get;
            private set;
        }


        public TBuilder Action(RouteValueDictionary routeValues)
        {
            this.Item.Action(routeValues);
            return (this as TBuilder);
        }

        public TBuilder Action(string actionName)
        {
            return this.Action(actionName, null, null);
        }

        public TBuilder Action(string actionName, object routeValues)
        {
            return this.Action(actionName, null, routeValues);
        }

        public TBuilder Action(string actionName, RouteValueDictionary routeValues)
        {
            return this.Action(actionName, null, routeValues);
        }

        public TBuilder Action(string actionName, string controllerName)
        {
            return this.Action(actionName, controllerName, null);
        }

        public TBuilder Action(string actionName, string controllerName, object routeValues)
        {
            this.Item.Action(actionName, controllerName, routeValues);
            return (this as TBuilder);
        }

        public TBuilder Action(string actionName, string controllerName, RouteValueDictionary routeValues)
        {
            this.Item.Action(actionName, controllerName, routeValues);
            return (this as TBuilder);
        }

        public TBuilder Route(string routeName)
        {
            return this.Route(routeName, null);
        }

        public TBuilder Route(string routeName, object routeValues)
        {
            this.Item.Route(routeName, routeValues);
            return (this as TBuilder);
        }

        public TBuilder Route(string routeName, RouteValueDictionary routeValues)
        {
            this.Item.Route(routeName, routeValues);
            return (this as TBuilder);
        }

        public TBuilder QueryParam(string paramName, params string[] booleanParamNames)
        {
            this.Item.ModifyParam(paramName, booleanParamNames);
            return (this as TBuilder);
        }

        public TBuilder Url(string value)
        {
            this.Item.Url(value);
            return (this as TBuilder);
        }

        public TBuilder HtmlAttributes(object attributes)
        {
            return this.HtmlAttributes(CommonHelper.ObjectToDictionary(attributes));
        }

        public TBuilder HtmlAttributes(IDictionary<string, object> attributes)
        {
            this.Item.HtmlAttributes.Clear();
            this.Item.HtmlAttributes.Merge(attributes);
            return (this as TBuilder);
        }

        public TBuilder LinkHtmlAttributes(object attributes)
        {
            return this.LinkHtmlAttributes(CommonHelper.ObjectToDictionary(attributes));
        }

        public TBuilder LinkHtmlAttributes(IDictionary<string, object> attributes)
        {
            this.Item.LinkHtmlAttributes.Clear();
            this.Item.LinkHtmlAttributes.Merge(attributes);
            return (this as TBuilder);
        }

        public TBuilder ImageUrl(string value)
        {
            this.Item.ImageUrl = value;
            return (this as TBuilder);
        }

        public TBuilder ImageId(int? value)
        {
            this.Item.ImageId = value;
            return (this as TBuilder);
        }

        public TBuilder Icon(string value)
        {
            this.Item.Icon = value;
            return (this as TBuilder);
        }

        public TBuilder Text(string value)
        {
            this.Item.Text = value;
            return (this as TBuilder);
        }

        public TBuilder Summary(string value)
        {
            this.Item.Summary = value;
            return (this as TBuilder);
        }

        public TBuilder Badge(string value, BadgeStyle style = BadgeStyle.Secondary, bool condition = true)
        {
            if (condition)
            {
                this.Item.BadgeText = value;
                this.Item.BadgeStyle = style;
            }
            return (this as TBuilder);
        }

        public TBuilder Visible(bool value)
        {
            this.Item.Visible = value;
            return (this as TBuilder);
        }

        public TBuilder Encoded(bool value)
        {
            this.Item.Encoded = value;
            return (this as TBuilder);
        }

        public TBuilder Selected(bool value)
        {
            this.Item.Selected = value;
            return (this as TBuilder);
        }

        public TBuilder Enabled(bool value)
        {
            this.Item.Enabled = value;
            return (this as TBuilder);
        }

        public TItem ToItem()
        {
            return this.Item;
        }
    }

    public abstract class NavigationItemtWithContentBuilder<TItem, TBuilder> : NavigationItemBuilder<TItem, TBuilder>
        where TItem : NavigationItemWithContent
        where TBuilder : NavigationItemtWithContentBuilder<TItem, TBuilder>
    {

        public NavigationItemtWithContentBuilder(TItem item, HtmlHelper htmlHelper)
            : base(item)
        {
            Guard.NotNull(htmlHelper, nameof(htmlHelper));

            HtmlHelper = htmlHelper;
        }

        protected HtmlHelper HtmlHelper
        {
            get;
            private set;
        }

        /// <summary>
        /// Specifies whether the content should be loaded per AJAX into the content pane.
        /// </summary>
        /// <param name="value">value</param>
        /// <returns>builder</returns>
        /// <remarks>
        ///		This setting has no effect when no route is specified OR
        ///		static content was set.
        /// </remarks>
        public TBuilder Ajax(bool value = true)
        {
            this.Item.Ajax = value;
            return (this as TBuilder);
        }

        public TBuilder Content(string value)
        {
            if (value.IsEmpty())
            {
                // do nothing
                return (this as TBuilder);
            }
            return this.Content(x => new HelperResult(writer => writer.Write(value)));
        }

        public TBuilder Content(Func<dynamic, HelperResult> value)
        {
            return this.Content(value(null));
        }

        public TBuilder Content(HelperResult value)
        {
            this.Item.Content = value;
            return (this as TBuilder);
        }

        /// <summary>
        /// Renders child action as content
        /// </summary>
        /// <param name="action">Action name</param>
        /// <param name="controller">Controller name</param>
        /// <param name="routeValues">Route values</param>
        /// <returns>builder instance</returns>
        public TBuilder Content(string action, string controller, object routeValues)
        {
            return Content(action, controller, new RouteValueDictionary(routeValues));
        }

        /// <summary>
        /// Renders child action as content
        /// </summary>
        /// <param name="action">Action name</param>
        /// <param name="controller">Controller name</param>
        /// <param name="routeValues">Route values</param>
        /// <returns>builder instance</returns>
        public TBuilder Content(string action, string controller, RouteValueDictionary routeValues)
        {
            return this.Content(x => new HelperResult(writer =>
            {
                var value = this.HtmlHelper.Action(action, controller, routeValues);
                writer.Write(value);
            }));
        }

        public TBuilder ContentHtmlAttributes(object attributes)
        {
            return this.ContentHtmlAttributes(CommonHelper.ObjectToDictionary(attributes));
        }

        public TBuilder ContentHtmlAttributes(IDictionary<string, object> attributes)
        {
            this.Item.ContentHtmlAttributes.Clear();
            this.Item.ContentHtmlAttributes.Merge(attributes);
            return (this as TBuilder);
        }
    }
}
