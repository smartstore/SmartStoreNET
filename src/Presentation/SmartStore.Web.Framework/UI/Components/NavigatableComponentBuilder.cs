using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.WebPages;
using SmartStore.Core;
using SmartStore.Utilities;

namespace SmartStore.Web.Framework.UI
{
    public abstract class NavigatableComponentBuilder<TComponent, TBuilder, TModel> : ComponentBuilder<TComponent, TBuilder, TModel>
        where TComponent : Component, INavigatable
        where TBuilder : ComponentBuilder<TComponent, TBuilder, TModel>
    {

        public NavigatableComponentBuilder(TComponent component, HtmlHelper<TModel> htmlHelper)
            : base(component, htmlHelper)
        {
        }

        public TBuilder Action(RouteValueDictionary routeValues)
        {
            this.Component.Action(routeValues);
            return (this as TBuilder);
        }

        public TBuilder Action(string actionName, string controllerName)
        {
            return this.Action(actionName, controllerName, null);
        }

        public TBuilder Action(string actionName, string controllerName, object routeValues)
        {
            this.Component.Action(actionName, controllerName, routeValues);
            return (this as TBuilder);
        }

        public TBuilder Action(string actionName, string controllerName, RouteValueDictionary routeValues)
        {
            this.Component.Action(actionName, controllerName, routeValues);
            return (this as TBuilder);
        }

        public TBuilder Route(string routeName)
        {
            return this.Route(routeName, null);
        }

        public TBuilder Route(string routeName, object routeValues)
        {
            this.Component.Route(routeName, routeValues);
            return (this as TBuilder);
        }

        public TBuilder Route(string routeName, RouteValueDictionary routeValues)
        {
            this.Component.Route(routeName, routeValues);
            return (this as TBuilder);
        }

        public TBuilder QueryParam(string paramName, params string[] booleanParamNames)
        {
            this.Component.ModifyParam(paramName, booleanParamNames);
            return (this as TBuilder);
        }

        public TBuilder Url(string value)
        {
            this.Component.Url(value);
            return (this as TBuilder);
        }

    }

    public abstract class NavigatableComponentWithContentBuilder<TComponent, TBuilder, TModel> : NavigatableComponentBuilder<TComponent, TBuilder, TModel>
        where TComponent : Component, INavigatable, IContentContainer
        where TBuilder : ComponentBuilder<TComponent, TBuilder, TModel>
    {

        public NavigatableComponentWithContentBuilder(TComponent component, HtmlHelper<TModel> htmlHelper)
            : base(component, htmlHelper)
        {
        }

        public TBuilder Content(string value)
        {
            return this.Content(x => new HelperResult(writer => writer.Write(value)));
        }

        public TBuilder Content(Func<dynamic, HelperResult> value)
        {
            return this.Content(value(null));
        }

        public TBuilder Content(HelperResult value)
        {
            this.Component.Content = value;
            return (this as TBuilder);
        }

        public TBuilder ContentHtmlAttributes(object attributes)
        {
            return this.ContentHtmlAttributes(CommonHelper.ObjectToDictionary(attributes));
        }

        public TBuilder ContentHtmlAttributes(IDictionary<string, object> attributes)
        {
            this.Component.ContentHtmlAttributes.Clear();
            this.Component.ContentHtmlAttributes.Merge(attributes);
            return (this as TBuilder);
        }

    }

}
