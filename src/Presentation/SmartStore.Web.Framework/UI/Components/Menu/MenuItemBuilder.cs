using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SmartStore.Utilities;

namespace SmartStore.Web.Framework.UI
{

    public class MenuItemBuilder : NavigationItemBuilder<MenuItem, MenuItemBuilder>
    {
        private readonly IList<string> _permissionNames;

        public MenuItemBuilder(MenuItem item)
            : base(item)
        {
            _permissionNames = new List<string>();
        }

        public MenuItemBuilder Id(string value)
        {
            this.Item.Id = value;
            return this;
        }

        public MenuItemBuilder IsGroupHeader(bool value)
        {
            this.Item.IsGroupHeader = value;
            return this;
        }

        public MenuItemBuilder PermissionNames(string value)
        {
            this.Item.PermissionNames = value;
            return this;
        }

        public MenuItemBuilder ResKey(string value)
        {
            this.Item.ResKey = value;
            return this;
        }

        public static implicit operator MenuItem(MenuItemBuilder builder)
        {
            return builder.ToItem();
        }


    }

}
