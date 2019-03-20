using System.ComponentModel.DataAnnotations.Schema;
using SmartStore.Core.Domain.Localization;

namespace SmartStore.Core.Domain.Cms
{
    /// <summary>
    /// Represents a menu item.
    /// </summary>
    public class MenuItem : BaseEntity, ILocalizedEntity
    {
        /// <summary>
        /// Gets or sets the menu identifier.
        /// </summary>
        public int MenuId { get; set; }

        /// <summary>
        /// Gets the menu.
        /// </summary>
        public virtual Menu Menu { get; set; }

        /// <summary>
        /// Gets or sets the parent menu item identifier. 0 if the item has no parent.
        /// </summary>
        [Index("IX_MenuItem_ParentItemId")]
        public int ParentItemId { get; set; }

        /// <summary>
        /// Gets or sets the system name. It identifies the related provider.
        /// </summary>
        public string SystemName { get; set; }

        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        public string Model { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the short description. It is used for the link title attribute.
        /// </summary>
        public string ShortDescription { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the menu item is published.
        /// </summary>
        [Index("IX_MenuItem_Published")]
        public bool Published { get; set; } = true;

        /// <summary>
        /// Gets or sets the display order.
        /// </summary>
        [Index("IX_MenuItem_DisplayOrder")]
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the menu item is a divider.
        /// </summary>
        public bool IsDivider { get; set; }

        /// <summary>
        /// If selected and this menu item has children, the menu will always appear expanded.
        /// </summary>
        public bool ShowExpanded { get; set; }

        /// <summary>
        /// Gets or sets the no-follow link attribute.
        /// </summary>
        public bool NoFollow { get; set; }

        /// <summary>
        /// Gets or sets the blank target link attribute.
        /// </summary>
        public bool NewWindow { get; set; }

        /// <summary>
        /// Gets or sets HTML id attribute.
        /// </summary>
        public string HtmlId { get; set; }

        /// <summary>
        /// Gets or sets the CSS class.
        /// </summary>
        public string CssClass { get; set; }
    }
}
