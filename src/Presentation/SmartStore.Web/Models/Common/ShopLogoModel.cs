using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Web.Models.Common
{
    public partial class FaviconModel : ModelBase
    {
        public int FavIconId { get; set; }
        public int AppleTouchIconId { get; set; }
        public int PngIconId { get; set; }
        public int MsTileIconId { get; set; }
        public string MsTileColor { get; set; }
    }
}