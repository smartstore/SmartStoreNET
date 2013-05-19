using System;
using System.Collections.Generic;
using System.Web.Routing;
using System.Web.WebPages;

namespace SmartStore.Web.Framework.UI
{

    public enum TabPull
    {
        Left,
        Right
    }

    public class Tab : NavigationItemWithContent
    {

        public Tab()
        {
            // [...]
        }

        public string Name
        {
            get;
            set;
        }

        public TabPull Pull
        {
            get;
            set;
        }

    }

}
