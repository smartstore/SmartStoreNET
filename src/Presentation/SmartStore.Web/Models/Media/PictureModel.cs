using SmartStore.Web.Framework.Mvc;

namespace SmartStore.Web.Models.Media
{
    public partial class PictureModel : ModelBase
    {
        // codehint: sm-add
        public int PictureId { get; set; }

        public string ThumbImageUrl { get; set; } // codehint: sm-add

        public string ImageUrl { get; set; }

        public string FullSizeImageUrl { get; set; }

        public string Title { get; set; }

        public string AlternateText { get; set; }
    }
}