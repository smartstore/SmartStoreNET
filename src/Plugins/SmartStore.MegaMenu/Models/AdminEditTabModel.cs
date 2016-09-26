using SmartStore.MegaMenu.Domain;
using SmartStore.Web.Framework.Modelling;
using System.Collections.Generic;
using System.Web.Mvc;

namespace SmartStore.MegaMenu.Models
{
    public class AdminEditTabModel : ModelBase
    {

        //OBSOLETE: DELETE
        
        public AdminEditTabModel() {
            AvailableBadges = new List<SelectListItem>();
            AvailableTeaserTypes = new List<SelectListItem>();
            AvailableTeaserRotatorItemSelectTypes = new List<SelectListItem>();
            AvailableAlignmentsX = new List<SelectListItem>();
            AvailableAlignmentsY = new List<SelectListItem>();
        }
        
        public int CategoryId { get; set; }

        public bool IsActive { get; set; }

        public bool DisplayCategoryPicture { get; set; }
                    
        public bool DisplayBgPicture { get; set; }
        
        public int BgPictureId { get; set; }
        
        public string BgLink { get; set; }
        
        public string BgAlignX { get; set; }
        public IList<SelectListItem> AvailableAlignmentsX { get; set; }

        public string BgAlignY { get; set; }
        public IList<SelectListItem> AvailableAlignmentsY { get; set; }

        public int BgOffsetX { get; set; }

        public int BgOffsetY { get; set; }
        
        public int MaxItemsPerColumn { get; set; }

        public int MaxSubItemsPerCategory { get; set; }
        
        public string Summary { get; set; }

        public string TeaserHtml { get; set; }
        
        public int HtmlColumnSpan { get; set; }
        
        public TeaserType TeaserType { get; set; }
        public IList<SelectListItem> AvailableTeaserTypes { get; set; }
        

        public TeaserRotatorItemSelectType TeaserRotatorItemSelectType { get; set; }
        public IList<SelectListItem> AvailableTeaserRotatorItemSelectTypes { get; set; }

        public string TeaserRotatorProductIds { get; set; }

        public string BadgeText { get; set; }

        public BadgeLabelType BadgeLabel { get; set; }
        public IList<SelectListItem> AvailableBadges { get; set; }

        public bool DisplaySubItemsInline { get; set; }
        
        public bool AllowSubItemsColumnWrap { get; set; }
        
        public int SubItemsWrapTolerance { get; set; }
        
        public bool FavorInMegamenu { get; set; }
    }
}