using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Web.Models.Common
{
    public partial class FaviconModel : ModelBase
    {
        public bool Uploaded { get; set; }
        public string FaviconUrl { get; set; }
        public int AppleTouchIconId { get; set; }
        public int PngIconId { get; set; }
        public int MsTileIconId { get; set; }
        public string MsTileColor { get; set; }
    }
}