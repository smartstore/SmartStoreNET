using System;
using SmartStore.Core;

namespace SmartStore.MegaMenu.Domain
{
    /// <summary>
    /// Represents a mega menu record
    /// </summary>
    public partial class MegaMenuRecord : BaseEntity
    {
        // represents the categoryId of the record
		public int CategoryId { get; set; }

        public bool IsActive { get; set; }
        

        // PICTURES
        // picture on the right, is linked to category
        public bool ShowPreviewPicture { get; set; }

        // is backgrund for sub catgory container
        public bool ShowBackgroundPicture { get; set; }

        // upload
        public int BackgroundPictureId { get; set; }

        // will be ative for every click inside dropdown whicht doesn't hit a link
        public string BackgroundLink { get; set; }

        // css 
        public string BackgroundAlignmentX { get; set; }

        public string BackgroundAlignmentY { get; set; }

        public int OffsetX { get; set; }

        public int OffsetY { get; set; }


        // MISC
        public int TotalColumnDepth { get; set; }

        public int ThirdLevelDepth { get; set; }


        //TEXTS: all texts must be editable by HTML-Editor in order to reach maximal flexibility

        // text will be defined in a textarea and will only be shown when a preview picture is displayed 
        public string BelowPreviewPicText { get; set; }

        // if Html shall be shown, then no sub cats will be displayed
        public string MenuReplacementText { get; set; }

        // columns 1-4, 4 means no categories will be displayed
        // text will be displayed on the right side, thus the range starts to count on the right side to
        public int MenuReplacementRange { get; set; }

        // enables/disables MenuReplacementText 
        public bool DisplayReplacementText { get; set; }



        // disables MenuReplacementText and displays productrotator. Products will be random products of the current category, or product which are explicitly defined to be displayed in this section
        public bool DisplayProductRotator { get; set; }

        // if active random products will be displayed from the current category
        public bool DisplayRandomProductsInRotator { get; set; }

        // explicitly picked products (EntityPicker)
        public string ProductRotatorProductIds { get; set; }
        
        public string BadgeText { get; set; }

        public string BadgeLabel { get; set; }
        
        // Determines whether third level elements will be displayed comma, separated
        public bool DisplayThirdLevelCommaSeparated { get; set; }

        // Determines whether third-level elements will be wrapped into a new comlumn
        public bool ThirdLevelAllowWrap { get; set; }

        // specifies a treshold
        public int ThirdLevelWrapTolerance { get; set; }


        // CONTROLLER OPTIONS
        // the following settings have to be built into the controller

        // specifies whether the category should be favored when building the nav structure: menuCats.sortBy(x => x.FavorInMegamenu).ThenBy(x. x.Priority)
        public bool FavorInMegamenu { get; set; }

        public DateTime? CreatedOnUtc { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }
    }
}