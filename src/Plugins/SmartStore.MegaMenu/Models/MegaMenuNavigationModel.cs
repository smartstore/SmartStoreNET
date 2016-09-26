using SmartStore.Collections;
using SmartStore.MegaMenu.Domain;
using SmartStore.MegaMenu.Settings;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Framework.UI;
using System.Collections.Generic;
using System.Linq;

namespace SmartStore.MegaMenu.Models
{
    public class MegaMenuNavigationModel
    {
        public NavigationModel NavigationModel { get; set; }

        public Dictionary<int, MegaMenuDropdownModel> DropdownModels { get; set; }

        public MegaMenuSettings Settings { get; set; }
    }

    /// <summary>
    /// represents view model of a MegaMenuRecord
    /// </summary>
    public class MegaMenuDropdownModel
    {
        public bool IsActive { get; set; }

        public bool DisplayCategoryPicture { get; set; }

        public bool DisplayBgPicture { get; set; }

        // path to picture
        public string BgPicturePath { get; set; }

        public string BgLink { get; set; }

        // comprised of BgAlignX, BgAlignY, BgOffsetX, BgOffsetY
        public string BgCss { get; set; }

        public int MaxItemsPerColumn { get; set; }

        public int MaxSubItemsPerCategory { get; set; }

        public string Summary { get; set; }

        public string TeaserHtml { get; set; }

        public int HtmlColumnSpan { get; set; }

        public TeaserType TeaserType { get; set; }

        public TeaserRotatorItemSelectType TeaserRotatorItemSelectType { get; set; }

        public string TeaserRotatorProductIds { get; set; }

        public bool DisplaySubItemsInline { get; set; }

        public bool AllowSubItemsColumnWrap { get; set; }

        public int SubItemsWrapTolerance { get; set; }
    }
}