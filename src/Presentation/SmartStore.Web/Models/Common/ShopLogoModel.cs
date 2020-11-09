using SmartStore.Services.Media;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Web.Models.Common
{
    public partial class FaviconModel : ModelBase
    {
        public MediaFileInfo FavIcon { get; set; }
        public MediaFileInfo AppleTouchIcon { get; set; }
        public MediaFileInfo PngIcon { get; set; }
        public MediaFileInfo MsTileIcon { get; set; }
        public string MsTileColor { get; set; }
    }
}