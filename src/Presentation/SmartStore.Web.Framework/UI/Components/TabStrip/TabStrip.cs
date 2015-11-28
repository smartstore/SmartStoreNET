using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        Pills
    }

    public class TabStrip : Component
    {

        public TabStrip()
        {
            this.Items = new List<Tab>();
            this.Fade = true;
            this.SmartTabSelection = true;
        }

        public IList<Tab> Items
        {
            get;
            private set;
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

        public bool Stacked
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

        public override bool NameIsRequired
        {
            get
            {
                return true;
            }
        }


    }

}
