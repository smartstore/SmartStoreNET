using System.Collections.Generic;
using System.Web.WebPages;

namespace SmartStore.Web.Framework.UI
{
    public enum TabsPosition
    {
        Top,
        Right,
        Below,
        Left
    }

    public enum TabsStyle
    {
        Tabs,
        Pills,
        Material
    }

    public class TabStrip : Component
    {
        public TabStrip()
        {
            this.Items = new List<Tab>();
            this.Fade = true;
            this.SmartTabSelection = true;
            this.Breakpoint = "<lg";
        }

        public List<Tab> Items
        {
            get;
            private set;
        }

        public bool IsResponsive
        {
            get;
            set;
        }

        public bool HideSingleItem
        {
            get;
            set;
        }

        public string Breakpoint
        {
            get;
            set;
        }

        public HelperResult TabContentHeaderContent
        {
            get;
            set;
        }

        public TabsPosition Position
        {
            get;
            set;
        }

        public TabsStyle Style
        {
            get;
            set;
        }

        public bool Fade
        {
            get;
            set;
        }

        public bool SmartTabSelection
        {
            get;
            set;
        }

        public string OnAjaxBegin
        {
            get;
            set;
        }

        public string OnAjaxSuccess
        {
            get;
            set;
        }

        public string OnAjaxFailure
        {
            get;
            set;
        }

        public string OnAjaxComplete
        {
            get;
            set;
        }

        public override bool NameIsRequired => true;
    }

}
