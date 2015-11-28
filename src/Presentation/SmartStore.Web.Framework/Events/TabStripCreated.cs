using System;
using System.Web.Mvc;
using SmartStore.Web.Framework.UI;

namespace SmartStore.Web.Framework.Events
{
    /// <summary>
    /// Admin tabstrip created event
    /// </summary>
    public class TabStripCreated
    {
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
    }
}