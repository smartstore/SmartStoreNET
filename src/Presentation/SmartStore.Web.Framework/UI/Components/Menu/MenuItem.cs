using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;
using SmartStore.Collections;

namespace SmartStore.Web.Framework.UI
{

    public class MenuItem : NavigationItem
    {

        public MenuItem()
        {
            this.Attributes = new RouteValueDictionary();
        }

        public string Id { get; set; }

        public string ResKey { get; set; }

        public string PermissionNames { get; set; }

        public bool IsGroupHeader { get; set; }

        public IDictionary<string, object> Attributes { get; private set; }

        public MenuItemBuilder ToBuilder()
        {
            return new MenuItemBuilder(this);
        }

        public static implicit operator MenuItemBuilder(MenuItem menuItem)
        {
            return menuItem.ToBuilder();
        }

    }

}
