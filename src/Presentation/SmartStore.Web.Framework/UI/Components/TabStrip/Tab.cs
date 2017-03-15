﻿using System;
using System.Collections.Generic;
using System.Web.Routing;
using System.Web.WebPages;

namespace SmartStore.Web.Framework.UI
{
	[Serializable]
	public class SelectedTabInfo
	{
		public string TabId { get; set; }
		public string Path { get; set; }
	}

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
