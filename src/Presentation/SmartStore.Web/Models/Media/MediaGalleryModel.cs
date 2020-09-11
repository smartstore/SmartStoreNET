using System.Collections.Generic;
using SmartStore.Services.Media;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Web.Models.Media
{
    public partial class MediaGalleryModel : ModelBase
    {
        public IList<MediaFileInfo> Files { get; set; } = new List<MediaFileInfo>();
        public int GalleryStartIndex { get; set; }
        public int ThumbSize { get; set; } = 72;
        public int ImageSize { get; set; } = 600;
        public string FallbackUrl { get; set; }

        public string ModelName { get; set; }
        public string DefaultAlt { get; set; }

        public bool BoxEnabled { get; set; }
        public bool ImageZoomEnabled { get; set; }
        public string ImageZoomType { get; set; }
    }
}