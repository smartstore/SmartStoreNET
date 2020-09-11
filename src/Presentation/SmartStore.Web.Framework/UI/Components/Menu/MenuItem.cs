using System;
using System.Collections.Generic;
using SmartStore.Utilities;

namespace SmartStore.Web.Framework.UI
{
    public class MenuItem : NavigationItem, ICloneable<MenuItem>
    {
        private string _id;

        public MenuItem()
        {
            this.Attributes = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// If this menu item refers to an entity, the id of the backed entity (like category, products e.g.)
        /// </summary>
        public int EntityId { get; set; }
        public string EntityName { get; set; }

        public int MenuItemId { get; set; }

        /// <summary>
        /// The total count of contained elements (like the count of products within a category)
        /// </summary>
        public int? ElementsCount { get; set; }

        /// <summary>
        /// Unique identifier.
        /// </summary>
        public string Id
        {
            get
            {
                if (_id == null)
                {
                    _id = CommonHelper.GenerateRandomDigitCode(10).TrimStart('0');
                }

                return _id;
            }
            set => _id = value;
        }

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
