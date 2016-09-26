using System;
using SmartStore.Core;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using SmartStore.Web.Framework;

namespace SmartStore.MegaMenu.Domain
{
    /// <summary>
    /// Represents a mega menu record
    /// </summary>
    [Table("MegaMenu")]
    public partial class MegaMenuRecord : BaseEntity
    {
        public MegaMenuRecord()
        {
            IsActive = true;
            MaxItemsPerColumn = 15;
            MaxSubItemsPerCategory = 8;
            AllowSubItemsColumnWrap = false;
            HtmlColumnSpan = 1;
            DisplayCategoryPicture = true;
            BgAlignX = AlignX.Right;
            BgAlignY = AlignY.Bottom;
            TeaserType = TeaserType.None;
            BadgeLabel = BadgeLabelType.Default;
            TeaserRotatorItemSelectType = TeaserRotatorItemSelectType.Top;
        }

        /// <summary>
        /// represents the categoryId of the record
        /// </summary>
        [Required]
        public int CategoryId { get; set; }

        /// <summary>
        /// specifies whether megamenu will be rendered for this menu item (category)
        /// </summary>
        [SmartResourceDisplayName("Plugins.SmartStore.MegaMenu.IsActive")]
        public bool IsActive { get; set; }

        /// <summary>
        /// specifies whether a category picture in the first column (on the right side of dropdown) will be displayed, picture is linked to category
        /// </summary>
        [SmartResourceDisplayName("Plugins.SmartStore.MegaMenu.DisplayCategoryPicture")]
        public bool DisplayCategoryPicture { get; set; }

        /// <summary>
        /// specifies whether a backgrund picture for catgory dropdown will be displayed
        /// </summary>
        [SmartResourceDisplayName("Plugins.SmartStore.MegaMenu.DisplayBgPicture")]
        public bool DisplayBgPicture { get; set; }

        /// <summary>
        /// picture id of background picture of catgory dropdown 
        /// </summary>
        [UIHint("Picture")]
        [SmartResourceDisplayName("Plugins.SmartStore.MegaMenu.BgPictureId")]
        public int BgPictureId { get; set; }

        /// <summary>
        /// Link for background picture of catgory dropdown 
        /// </summary>
        [StringLength(2048)]
        [SmartResourceDisplayName("Plugins.SmartStore.MegaMenu.BgLink")]
        public string BgLink { get; set; }

        /// <summary>
        /// Specifies the horizontal alignment of background picture 
        /// </summary>
        [SmartResourceDisplayName("Plugins.SmartStore.MegaMenu.BgAlignX")]
        public AlignX BgAlignX { get; set; }

        /// <summary>
        /// Specifies the vertical alignment of background picture 
        /// </summary>
        [SmartResourceDisplayName("Plugins.SmartStore.MegaMenu.BgAlignY")]
        public AlignY BgAlignY { get; set; }

        /// <summary>
        /// Specifies the horizontal offset of background picture 
        /// </summary>
        [SmartResourceDisplayName("Plugins.SmartStore.MegaMenu.BgOffsetX")]
        public int BgOffsetX { get; set; }

        /// <summary>
        /// Specifies the vertical offset of background picture 
        /// </summary>
        [SmartResourceDisplayName("Plugins.SmartStore.MegaMenu.BgOffsetY")]
        public int BgOffsetY { get; set; }

        /// <summary>
        /// Specifies the maximum of items in one column
        /// </summary>
        [Required]
        [SmartResourceDisplayName("Plugins.SmartStore.MegaMenu.MaxItemsPerColumn")]
        public int MaxItemsPerColumn { get; set; }

        /// <summary>
        /// Specifies the maximum of sub items
        /// </summary>
        [Required]
        [SmartResourceDisplayName("Plugins.SmartStore.MegaMenu.MaxSubItemsPerCategory")]
        public int MaxSubItemsPerCategory { get; set; }

        /// <summary>
        /// Specifies text that will be displayed below the category picture. This text will only be displayed if the category picture is displayed. Html is allowed.
        /// </summary>
        [StringLength(2048)]
        [SmartResourceDisplayName("Plugins.SmartStore.MegaMenu.Summary")]
        public string Summary { get; set; }

        /// <summary>
        /// replaces menu columns and displays the defined HTML instead
        /// </summary>
        [UIHint("RichEditor")]
        [SmartResourceDisplayName("Plugins.SmartStore.MegaMenu.TeaserHtml")]
        public string TeaserHtml { get; set; }

        /// <summary>
        /// columns 1-4, 4 means no sub categories will be displayed, html display and span count start on the right side
        /// </summary>
        [SmartResourceDisplayName("Plugins.SmartStore.MegaMenu.HtmlColumnSpan")]
        public int HtmlColumnSpan { get; set; }

        /// <summary>
        /// Defines the type of the teaser
        /// </summary>
        [Required]
        [SmartResourceDisplayName("Plugins.SmartStore.MegaMenu.TeaserType")]
        public TeaserType TeaserType { get; set; }

        /// <summary>
        /// enum (rotatortype): custom, topproducts, random, (deep-top, deep-random => implies to include sub categories)
        /// </summary>
        [SmartResourceDisplayName("Plugins.SmartStore.MegaMenu.TeaserRotatorItemSelectType")]
        public TeaserRotatorItemSelectType TeaserRotatorItemSelectType { get; set; }

        /// <summary>
        /// ids of explicitly picked products (EntityPicker)
        /// </summary>
        [StringLength(512)]
        [SmartResourceDisplayName("Plugins.SmartStore.MegaMenu.TeaserRotatorProductIds")]
        public string TeaserRotatorProductIds { get; set; }

        /// <summary>
        /// defines the badge text next to the category
        /// </summary>
        [StringLength(128)]
        [SmartResourceDisplayName("Plugins.SmartStore.MegaMenu.BadgeText")]
        public string BadgeText { get; set; }

        /// <summary>
        /// defines the badge type next to the category, enum: info, danger, warning, usw
        /// </summary>
        [SmartResourceDisplayName("Plugins.SmartStore.MegaMenu.BadgeLabel")]
        public BadgeLabelType BadgeLabel { get; set; }

        /// <summary>
        /// Determines whether sub items (third level elements) will be displayed separated by comma
        /// </summary>
        [SmartResourceDisplayName("Plugins.SmartStore.MegaMenu.DisplaySubItemsInline")]
        public bool DisplaySubItemsInline { get; set; }

        /// <summary>
        /// Determines whether sub items (third-level elements) can be broken into a new comlumn
        /// </summary>
        [SmartResourceDisplayName("Plugins.SmartStore.MegaMenu.AllowSubItemsColumnWrap")]
        public bool AllowSubItemsColumnWrap { get; set; }

        /// <summary>
        /// specifies a treshold which defines a tolarance value
        /// </summary>
        [SmartResourceDisplayName("Plugins.SmartStore.MegaMenu.SubItemsWrapTolerance")]
        public int SubItemsWrapTolerance { get; set; }

        /// <summary>
        /// specifies whether the category should be favored when building the nav structure: menuCats.sortBy(x => x.FavorInMegamenu).ThenBy(x. x.Priority)
        /// </summary>
        [SmartResourceDisplayName("Plugins.SmartStore.MegaMenu.FavorInMegamenu")]
        public bool FavorInMegamenu { get; set; }

        public DateTime? CreatedOnUtc { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }
    }

    public enum BadgeLabelType
    {
        Default,
        Primary,
        Success,
        Info,
        Warning,
        Danger
    }

    public enum TeaserRotatorItemSelectType
    {
        Custom,         // products that are explicitly picked 
        Top,            // top products of category
        Random,         // random products of category
        DeepTop,        // top products of category & subcategories
        DeepRandom      // random products of category & subcategories
    }

    public enum TeaserType
    {
        None,
        Html,
        Rotator
    }

    public enum AlignX
    {
        Left,
        Center,
        Right
    }

    public enum AlignY
    {
        Top,
        Center,
        Bottom
    }
}