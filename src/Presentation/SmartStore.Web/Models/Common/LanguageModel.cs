using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Web.Models.Common
{
    public partial class LanguageModel : EntityModelBase
    {
        public string ISOCode { get; set; }
        public string SeoCode { get; set; }
        public string FlagImageFileName { get; set; }

        public string Name { get; set; }
        public string ShortName { get; set; }

        public string NativeName { get; set; }
        public string ShortNativeName { get; set; }
    }
}