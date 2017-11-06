using System;
using System.Web.Mvc;
using System.Web.WebPages;

namespace SmartStore.Web.Framework.UI
{
    public class WindowBuilder<TModel> : ComponentBuilder<Window, WindowBuilder<TModel>, TModel>
    {
        public WindowBuilder(Window Component, HtmlHelper<TModel> htmlHelper)
            : base(Component, htmlHelper)
        {
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

        public WindowBuilder<TModel> Modal(bool value)
        {
            base.Component.Modal = value;
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

        public WindowBuilder<TModel> Visible(bool value)
        {
            base.Component.Visible = value;
            return this;
        }

        public WindowBuilder<TModel> ShowClose(bool value)
        {
            base.Component.ShowClose = value;
            return this;
        }

        public WindowBuilder<TModel> CloseOnEscapePress(bool value)
        {
            base.Component.CloseOnEscapePress = value;
            return this;
        }

        public WindowBuilder<TModel> Width(int value)
        {
            base.Component.Width = value;
            return this;
        }

        public WindowBuilder<TModel> Height(int value)
        {
            base.Component.Height = value;
            return this;
        }
    }
}