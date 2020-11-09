using System;
using System.Web.Mvc;
using System.Web.WebPages;

namespace SmartStore.Web.Framework.UI
{
    public class TabStripBuilder<TModel> : ComponentBuilder<TabStrip, TabStripBuilder<TModel>, TModel>
    {
        public TabStripBuilder(TabStrip Component, HtmlHelper<TModel> htmlHelper)
            : base(Component, htmlHelper)
        {
        }

        public TabStripBuilder<TModel> Items(Action<TabFactory> addAction)
        {
            var factory = new TabFactory(base.Component.Items, this.HtmlHelper);
            addAction(factory);
            return this;
        }

        public TabStripBuilder<TModel> Responsive(bool value, string breakpoint = "<lg")
        {
            base.Component.IsResponsive = value;
            base.Component.Breakpoint = breakpoint;
            return this;
        }

        public TabStripBuilder<TModel> HideSingleItem(bool value)
        {
            base.Component.HideSingleItem = value;
            return this;
        }

        public TabStripBuilder<TModel> TabContentHeaderContent(string value)
        {
            if (value.IsEmpty())
            {
                // do nothing
                return this;
            }

            return this.TabContentHeaderContent(x => new HelperResult(writer => writer.Write(value)));
        }

        public TabStripBuilder<TModel> TabContentHeaderContent(Func<dynamic, HelperResult> value)
        {
            return this.TabContentHeaderContent(value(null));
        }

        public TabStripBuilder<TModel> TabContentHeaderContent(HelperResult value)
        {
            base.Component.TabContentHeaderContent = value;
            return this;
        }

        public TabStripBuilder<TModel> Position(TabsPosition value)
        {
            base.Component.Position = value;
            return this;
        }

        public TabStripBuilder<TModel> Style(TabsStyle value)
        {
            base.Component.Style = value;
            return this;
        }

        public TabStripBuilder<TModel> Fade(bool value)
        {
            base.Component.Fade = value;
            return this;
        }

        public TabStripBuilder<TModel> SmartTabSelection(bool value)
        {
            base.Component.SmartTabSelection = value;
            return this;
        }

        public TabStripBuilder<TModel> OnAjaxBegin(string value)
        {
            base.Component.OnAjaxBegin = value;
            return this;
        }

        public TabStripBuilder<TModel> OnAjaxSuccess(string value)
        {
            base.Component.OnAjaxSuccess = value;
            return this;
        }

        public TabStripBuilder<TModel> OnAjaxFailure(string value)
        {
            base.Component.OnAjaxFailure = value;
            return this;
        }

        public TabStripBuilder<TModel> OnAjaxComplete(string value)
        {
            base.Component.OnAjaxComplete = value;
            return this;
        }
    }
}
