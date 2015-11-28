using SmartStore.Web.Framework.Mvc;

namespace SmartStore.Web.Models.Media
{
    public partial class PictureModel : ModelBase
    {
        public int PictureId { get; set; }

        public string ThumbImageUrl { get; set; }

        public string ImageUrl { get; set; }

        public string FullSizeImageUrl { get; set; }

        public string Title { get; set; }

        public string AlternateText { get; set; }
    }
}