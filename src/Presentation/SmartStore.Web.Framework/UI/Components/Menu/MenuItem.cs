using System;
using System.Collections.Generic;

namespace SmartStore.Web.Framework.UI
{
    public class MenuItem : NavigationItem, ICloneable<MenuItem>
    {
        public MenuItem()
        {
            this.Attributes = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
		}

		/// <summary>
		/// If this menu item refers to an entity, the id of the backed entity (like category, products e.g.)
		/// </summary>
		public int EntityId { get; set; }

		/// <summary>
		/// The total count of contained elements (like the count of products within a category)
		/// </summary>
		public int? ElementsCount { get; set; }

        public string Id { get; set; }

        public string ResKey { get; set; }

        public string PermissionNames { get; set; }

        public bool IsGroupHeader { get; set; }

        public IDictionary<string, object> Attributes { get; set; }

		public MenuItemBuilder ToBuilder()
        {
            return new MenuItemBuilder(this);
        }

        public static implicit operator MenuItemBuilder(MenuItem menuItem)
        {
            return menuItem.ToBuilder();
        }

		public MenuItem Clone()
		{
			return (MenuItem)this.MemberwiseClone();
		}

		object ICloneable.Clone()
		{
			return this.Clone();
		}
    }
}
