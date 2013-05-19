using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

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
            TabFactory factory = new TabFactory(base.Component.Items);
            addAction(factory);
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

    }

}
