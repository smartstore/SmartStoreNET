using System;
using System.Web.Mvc;
using System.Web.WebPages;

namespace SmartStore.Web.Framework.UI
{  
    public class TabStripBuilder : ComponentBuilder<TabStrip, TabStripBuilder>
    {
        public TabStripBuilder(TabStrip Component, HtmlHelper htmlHelper)
            : base(Component, htmlHelper)
        {
        }

        public TabStripBuilder Items(Action<TabFactory> addAction)
        {
            var factory = new TabFactory(base.Component.Items, this.HtmlHelper);
            addAction(factory);
            return this;
        }

		public TabStripBuilder Responsive(bool value, string breakpoint = "<lg")
		{
			base.Component.IsResponsive = value;
			base.Component.Breakpoint = breakpoint;
			return this;
		}

		public TabStripBuilder TabContentHeaderContent(string value)
		{
			if (value.IsEmpty())
			{
				// do nothing
				return this;
			}

			return this.TabContentHeaderContent(x => new HelperResult(writer => writer.Write(value)));
		}

		public TabStripBuilder TabContentHeaderContent(Func<dynamic, HelperResult> value)
		{
			return this.TabContentHeaderContent(value(null));
		}

		public TabStripBuilder TabContentHeaderContent(HelperResult value)
		{
			base.Component.TabContentHeaderContent = value;
			return this;
		}

		public TabStripBuilder Position(TabsPosition value)
        {
            base.Component.Position = value;
            return this;
        }

        public TabStripBuilder Style(TabsStyle value)
        {
            base.Component.Style = value;
            return this;
        }

        public TabStripBuilder Stacked(bool value)
        {
            base.Component.Stacked = value;
            return this;
        }

        public TabStripBuilder Fade(bool value)
        {
            base.Component.Fade = value;
            return this;
        }

        public TabStripBuilder SmartTabSelection(bool value)
        {
            base.Component.SmartTabSelection = value;
            return this;
        }

		public TabStripBuilder OnAjaxBegin(string value)
		{
			base.Component.OnAjaxBegin = value;
			return this;
		}

		public TabStripBuilder OnAjaxSuccess(string value)
		{
			base.Component.OnAjaxSuccess = value;
			return this;
		}

		public TabStripBuilder OnAjaxFailure(string value)
		{
			base.Component.OnAjaxFailure = value;
			return this;
		}

		public TabStripBuilder OnAjaxComplete(string value)
		{
			base.Component.OnAjaxComplete = value;
			return this;
		}
    }
}
