using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Security;
using SmartStore.Core.Domain.Stores;

namespace SmartStore.Core.Domain.Cms
{
    /// <summary>
    /// Represents a menu item.
    /// </summary>
    [Table("MenuItemRecord")]
    public class MenuItemRecord : BaseEntity, ILocalizedEntity, IStoreMappingSupported, IAclSupported
    {
        /// <summary>
        /// Gets or sets the menu identifier.
        /// </summary>
        [Required]
        public int MenuId { get; set; }

        /// <summary>
        /// Gets the menu.
        /// </summary>
        [ForeignKey("MenuId")]
        public virtual MenuRecord Menu { get; set; }

        /// <summary>
        /// Gets or sets the parent menu item identifier. 0 if the item has no parent.
        /// </summary>
        [Index("IX_MenuItem_ParentItemId")]
        public int ParentItemId { get; set; }

        /// <summary>
        /// Gets or sets the provider name.
        /// </summary>
        [StringLength(100)]
        public string ProviderName { get; set; }

        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        [MaxLength]
        public string Model { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        [StringLength(400)]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the short description. It is used for the link title attribute.
        /// </summary>
        [StringLength(400)]
        public string ShortDescription { get; set; }

        /// <summary>
        /// Gets or sets permission names.
        /// </summary>
        [MaxLength]
        public string PermissionNames { get; set; }

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
        /// Gets or sets a value indicating whether the menu item has a divider or a group header.
        /// </summary>
        public bool BeginGroup { get; set; }

        /// <summary>
        /// If selected and this menu item has children, the menu will initially appear expanded.
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
        /// Gets or sets fontawesome icon class.
        /// </summary>
        [StringLength(100)]
        public string Icon { get; set; }

        /// <summary>
        /// Gets or sets fontawesome icon style.
        /// </summary>
        [StringLength(10)]
        public string Style { get; set; }

        /// <summary>
        /// Gets or sets fontawesome icon color.
        /// </summary>
        [StringLength(100)]
        public string IconColor { get; set; }

        /// <summary>
        /// Gets or sets HTML id attribute.
        /// </summary>
        [StringLength(100)]
        public string HtmlId { get; set; }

        /// <summary>
        /// Gets or sets the CSS class.
        /// </summary>
        [StringLength(100)]
        public string CssClass { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity is limited/restricted to certain stores.
        /// </summary>
        [Index("IX_MenuItem_LimitedToStores")]
        public bool LimitedToStores { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity is subject to ACL.
        /// </summary>
        [Index("IX_MenuItem_SubjectToAcl")]
        public bool SubjectToAcl { get; set; }
    }
}
