using SmartStore.Web.Framework.Mvc;

namespace SmartStore.Web.Models.Common
{
    public partial class LanguageModel : EntityModelBase
    {
        public string Name { get; set; }
        // codehint: sm-add
        public string ISOCode { get; set; }
        public string SeoCode { get; set; }
        public string NativeName { get; set; }
        public string ReturnUrl { get; set; }

        public string FlagImageFileName { get; set; }

    }
}