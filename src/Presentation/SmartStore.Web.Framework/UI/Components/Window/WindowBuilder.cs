using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.WebPages;
using SmartStore.Utilities;

namespace SmartStore.Web.Framework.UI
{
    public class WindowBuilder<TModel> : ComponentBuilder<Window, WindowBuilder<TModel>, TModel>
    {
        public WindowBuilder(Window Component, HtmlHelper<TModel> htmlHelper)
            : base(Component, htmlHelper)
        {
        }

        public WindowBuilder<TModel> Size(WindowSize value)
        {
            base.Component.Size = value;
            return this;
        }

        public WindowBuilder<TModel> Title(string value)
        {
            base.Component.Title = value;
            return this;
        }

        public WindowBuilder<TModel> LoadContentFrom(string url)
        {
            base.Component.ContentUrl = url;
            return this;
        }

        public WindowBuilder<TModel> Content(string value)
        {
            return this.Content(x => new HelperResult(writer => writer.Write(value)));
        }

        public WindowBuilder<TModel> Content(Func<dynamic, HelperResult> value)
        {
            return this.Content(value(null));
        }

        public WindowBuilder<TModel> Content(HelperResult value)
        {
            this.Component.Content = value;
            return this;
        }

        public WindowBuilder<TModel> FooterContent(string value)
        {
            return this.FooterContent(x => new HelperResult(writer => writer.Write(value)));
        }

        public WindowBuilder<TModel> FooterContent(Func<dynamic, HelperResult> value)
        {
            return this.FooterContent(value(null));
        }

        public WindowBuilder<TModel> FooterContent(HelperResult value)
        {
            this.Component.FooterContent = value;
            return this;
        }

        public WindowBuilder<TModel> Fade(bool value)
        {
            base.Component.Fade = value;
            return this;
        }

        public WindowBuilder<TModel> BackDrop(bool value)
        {
            base.Component.BackDrop = value;
            return this;
        }

        public WindowBuilder<TModel> ShowClose(bool value)
        {
            base.Component.ShowClose = value;
            return this;
        }

        public WindowBuilder<TModel> Focus(bool value)
        {
            base.Component.Focus = value;
            return this;
        }

        public WindowBuilder<TModel> CenterVertically(bool value)
        {
            base.Component.CenterVertically = value;
            return this;
        }

        public WindowBuilder<TModel> Show(bool value)
        {
            base.Component.Show = value;
            return this;
        }

        public WindowBuilder<TModel> CloseOnEscapePress(bool value)
        {
            base.Component.CloseOnEscapePress = value;
            return this;
        }

        public WindowBuilder<TModel> CloseOnBackdropClick(bool value)
        {
            base.Component.CloseOnBackdropClick = value;
            return this;
        }

        public WindowBuilder<TModel> RenderAtPageEnd(bool value)
        {
            base.Component.RenderAtPageEnd = value;
            return this;
        }

        public WindowBuilder<TModel> BodyHtmlAttributes(object attributes)
        {
            return this.BodyHtmlAttributes(CommonHelper.ObjectToDictionary(attributes));
        }

        public WindowBuilder<TModel> BodyHtmlAttributes(IDictionary<string, object> attributes)
        {
            base.Component.BodyHtmlAttributes.Clear();
            base.Component.BodyHtmlAttributes.Merge(attributes);
            return this;
        }
    }
}