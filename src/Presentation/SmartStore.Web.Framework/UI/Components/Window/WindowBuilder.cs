using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.WebPages;

namespace SmartStore.Web.Framework.UI
{

    public class WindowBuilder : ComponentBuilder<Window, WindowBuilder>
    {

        public WindowBuilder(Window Component, HtmlHelper htmlHelper)
            : base(Component, htmlHelper)
        {
        }

        public WindowBuilder Title(string value)
        {
            base.Component.Title = value;
            return this;
        }

        public WindowBuilder LoadContentFrom(string url)
        {
            base.Component.ContentUrl = url;
            return this;
        }

        public WindowBuilder Content(string value)
        {
            return this.Content(x => new HelperResult(writer => writer.Write(value)));
        }

        public WindowBuilder Content(Func<dynamic, HelperResult> value)
        {
            return this.Content(value(null));
        }

        public WindowBuilder Content(HelperResult value)
        {
            this.Component.Content = value;
            return this;
        }

        public WindowBuilder FooterContent(string value)
        {
            return this.FooterContent(x => new HelperResult(writer => writer.Write(value)));
        }

        public WindowBuilder FooterContent(Func<dynamic, HelperResult> value)
        {
            return this.FooterContent(value(null));
        }

        public WindowBuilder FooterContent(HelperResult value)
        {
            this.Component.FooterContent = value;
            return this;
        }

        public WindowBuilder Modal(bool value)
        {
            base.Component.Modal = value;
            return this;
        }

        public WindowBuilder Fade(bool value)
        {
            base.Component.Fade = value;
            return this;
        }

        public WindowBuilder BackDrop(bool value)
        {
            base.Component.BackDrop = value;
            return this;
        }

        public WindowBuilder Visible(bool value)
        {
            base.Component.Visible = value;
            return this;
        }

        public WindowBuilder ShowClose(bool value)
        {
            base.Component.ShowClose = value;
            return this;
        }

        public WindowBuilder CloseOnEscapePress(bool value)
        {
            base.Component.CloseOnEscapePress = value;
            return this;
        }

        public WindowBuilder Width(int value)
        {
            base.Component.Width = value;
            return this;
        }

        public WindowBuilder Height(int value)
        {
            base.Component.Height = value;
            return this;
        }

    }

}
