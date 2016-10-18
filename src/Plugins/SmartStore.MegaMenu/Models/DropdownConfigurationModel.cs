using SmartStore.Collections;
using SmartStore.MegaMenu.Domain;
using SmartStore.MegaMenu.Settings;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Localization;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Framework.UI;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace SmartStore.MegaMenu.Models
{
    public class DropdownConfigurationModel : ModelBase, ILocalizedModel<DropdownConfigurationLocalizedModel>
    {
        public DropdownConfigurationModel()
        {
            Locales = new List<DropdownConfigurationLocalizedModel>();
        }

        public IList<DropdownConfigurationLocalizedModel> Locales { get; set; }

        public int EntityId { get; set; }
        public int CategoryId { get; set; }

        [SmartResourceDisplayName("Plugins.SmartStore.MegaMenu.IsActive")]
        public bool IsActive { get; set; }

        [SmartResourceDisplayName("Plugins.SmartStore.MegaMenu.DisplayCategoryPicture")]
        public bool DisplayCategoryPicture { get; set; }

        [SmartResourceDisplayName("Plugins.SmartStore.MegaMenu.DisplayBgPicture")]
        public bool DisplayBgPicture { get; set; }

        [UIHint("Picture")]
        [SmartResourceDisplayName("Plugins.SmartStore.MegaMenu.BgPictureId")]
        public int BgPictureId { get; set; }

        [SmartResourceDisplayName("Plugins.SmartStore.MegaMenu.BgLink")]
        public string BgLink { get; set; }

        [SmartResourceDisplayName("Plugins.SmartStore.MegaMenu.BgAlignX")]
        public AlignX BgAlignX { get; set; }

        [SmartResourceDisplayName("Plugins.SmartStore.MegaMenu.BgAlignY")]
        public AlignY BgAlignY { get; set; }

        [SmartResourceDisplayName("Plugins.SmartStore.MegaMenu.BgOffsetX")]
        public int BgOffsetX { get; set; }

        [SmartResourceDisplayName("Plugins.SmartStore.MegaMenu.BgOffsetY")]
        public int BgOffsetY { get; set; }

        [SmartResourceDisplayName("Plugins.SmartStore.MegaMenu.MaxItemsPerColumn")]
        public int MaxItemsPerColumn { get; set; }

        [SmartResourceDisplayName("Plugins.SmartStore.MegaMenu.MaxSubItemsPerCategory")]
        public int MaxSubItemsPerCategory { get; set; }

        [SmartResourceDisplayName("Plugins.SmartStore.MegaMenu.Summary")]
        public string Summary { get; set; }

        [UIHint("RichEditor")]
        [SmartResourceDisplayName("Plugins.SmartStore.MegaMenu.TeaserHtml")]
        public string TeaserHtml { get; set; }

        [SmartResourceDisplayName("Plugins.SmartStore.MegaMenu.HtmlColumnSpan")]
        public int HtmlColumnSpan { get; set; }

        [SmartResourceDisplayName("Plugins.SmartStore.MegaMenu.TeaserType")]
        public TeaserType TeaserType { get; set; }

        [SmartResourceDisplayName("Plugins.SmartStore.MegaMenu.TeaserRotatorItemSelectType")]
        public TeaserRotatorItemSelectType TeaserRotatorItemSelectType { get; set; }

        [SmartResourceDisplayName("Plugins.SmartStore.MegaMenu.TeaserRotatorProductIds")]
        public string TeaserRotatorProductIds { get; set; }

        [SmartResourceDisplayName("Plugins.SmartStore.MegaMenu.DisplaySubItemsInline")]
        public bool DisplaySubItemsInline { get; set; }

        [SmartResourceDisplayName("Plugins.SmartStore.MegaMenu.AllowSubItemsColumnWrap")]
        public bool AllowSubItemsColumnWrap { get; set; }

        [SmartResourceDisplayName("Plugins.SmartStore.MegaMenu.SubItemsWrapTolerance")]
        public int SubItemsWrapTolerance { get; set; }

        [SmartResourceDisplayName("Plugins.SmartStore.MegaMenu.FavorInMegamenu")]
        public bool FavorInMegamenu { get; set; }

        [SmartResourceDisplayName("Plugins.SmartStore.MegaMenu.MinChildCategoryThreshold")]
        public int MinChildCategoryThreshold { get; set; }

        [SmartResourceDisplayName("Plugins.SmartStore.MegaMenu.MaxRotatorItems")]
        public int MaxRotatorItems { get; set; }

        [SmartResourceDisplayName("Plugins.SmartStore.MegaMenu.RotatorHeading")]
        public string RotatorHeading { get; set; }
    }

    public class DropdownConfigurationLocalizedModel : ILocalizedModelLocal
    {
        public int LanguageId { get; set; }

        [SmartResourceDisplayName("Plugins.SmartStore.MegaMenu.BgLink")]
        public string BgLink { get; set; }

        [SmartResourceDisplayName("Plugins.SmartStore.MegaMenu.Summary")]
        public string Summary { get; set; }

        [UIHint("RichEditor")]
        [SmartResourceDisplayName("Plugins.SmartStore.MegaMenu.TeaserHtml")]
        public string TeaserHtml { get; set; }

        [SmartResourceDisplayName("Plugins.SmartStore.MegaMenu.RotatorHeading")]
        public string RotatorHeading { get; set; }
    }

}