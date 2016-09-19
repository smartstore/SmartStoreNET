using SmartStore.Web.Framework.Modelling;
using System.Collections.Generic;
using System.Web.Mvc;

namespace SmartStore.MegaMenu.Models
{
    public class AdminEditTabModel : ModelBase
    {
        public AdminEditTabModel() {
            AvailableBadges = new List<SelectListItem>();
            AvailableAlignmentsX = new List<SelectListItem>();
            AvailableAlignmentsY = new List<SelectListItem>();
        }
        
        public int CategoryId { get; set; }

        public bool IsActive { get; set; }
        
        public string BadgeText { get; set; }

        public string BadgeLabel { get; set; }
        public IList<SelectListItem> AvailableBadges { get; set; }

        public bool ShowPreviewPicture { get; set; }

        public bool ShowBackgroundPicture { get; set; }
        
        public int BackgroundPictureId { get; set; }
        
        public string BackgroundLink { get; set; }
        
        public string BackgroundAlignmentX { get; set; }
        public IList<SelectListItem> AvailableAlignmentsX { get; set; }

        public string BackgroundAlignmentY { get; set; }
        public IList<SelectListItem> AvailableAlignmentsY { get; set; }

        public int OffsetX { get; set; }

        public int OffsetY { get; set; }
        
        public int TotalColumnDepth { get; set; }

        public int ThirdLevelDepth { get; set; }
        
        public string BelowPreviewPicText { get; set; }

        public bool DisplayReplacementText { get; set; }

        public string MenuReplacementText { get; set; }
        
        public int MenuReplacementRange { get; set; }
        
        public bool DisplayProductRotator { get; set; }
        
        public bool DisplayRandomProductsInRotator { get; set; }
        
        public string ProductRotatorProductIds { get; set; }
        
        public bool DisplayThirdLevelCommaSeparated { get; set; }
        
        public bool ThirdLevelAllowWrap { get; set; }
        
        public int ThirdLevelWrapTolerance { get; set; }
        
        public bool FavorInMegamenu { get; set; }
    }
}