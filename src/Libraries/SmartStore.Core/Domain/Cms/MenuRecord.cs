using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Security;
using SmartStore.Core.Domain.Stores;

namespace SmartStore.Core.Domain.Cms
{
    /// <summary>
    /// Represents a menu.
    /// </summary>
    [Table("MenuRecord")]
    public class MenuRecord : BaseEntity, ILocalizedEntity, IStoreMappingSupported, IAclSupported
    {
        private ICollection<MenuItemRecord> _items;

        /// <summary>
        /// /// Gets or sets the menu items.
        /// </summary>
        public virtual ICollection<MenuItemRecord> Items
        {
            get => _items ?? (_items = new HashSet<MenuItemRecord>());
            protected set => _items = value;
        }

        /// <summary>
        /// Gets or sets the system name. It identifies the menu.
        /// </summary>
        [Required, StringLength(400), Index("IX_Menu_SystemName_IsSystemMenu", Order = 0)]
        public string SystemName { get; set; }

        /// <summary>
        /// Gets or sets the value indicating whether this menu is deleteable by a user.
        /// </summary>
        [Index("IX_Menu_SystemName_IsSystemMenu", Order = 1)]
        public bool IsSystemMenu { get; set; }

        /// <summary>
        /// Gets or sets the menu template name.
        /// </summary>
        [StringLength(400)]
        public string Template { get; set; }

        /// <summary>
        /// Gets or sets the widget zone name.
        /// </summary>
        [StringLength(4000)]
        public string WidgetZone { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        [StringLength(400)]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the menu is published.
        /// </summary>
        [Index("IX_Menu_Published")]
        public bool Published { get; set; } = true;

        /// <summary>
        /// Gets or sets the order for widget registration.
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity is limited/restricted to certain stores.
        /// </summary>
        [Index("IX_Menu_LimitedToStores")]
        public bool LimitedToStores { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity is subject to ACL.
        /// </summary>
        [Index("IX_Menu_SubjectToAcl")]
        public bool SubjectToAcl { get; set; }

        /// <summary>
        /// Helper function to get the comma-separated <c>WidgetZone</c> property as an enumerable of strings.
        /// </summary>
        /// <returns>Widget zones.</returns>
        public IEnumerable<string> GetWidgetZones()
        {
            if (WidgetZone.IsEmpty())
            {
                return Enumerable.Empty<string>();
            }

            return WidgetZone.Trim().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim());
        }
    }
}
