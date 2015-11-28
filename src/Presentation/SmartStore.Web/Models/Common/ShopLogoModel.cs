using SmartStore.Web.Framework.Mvc;

namespace SmartStore.Web.Models.Common
{
    public partial class FaviconModel : ModelBase
    {
        public bool Uploaded { get; set; }
        public string FaviconUrl { get; set; }
    }
}