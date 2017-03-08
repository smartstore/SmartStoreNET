using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Web.Models.Catalog
{
    public partial class OffCanvasMenuModel : EntityModelBase
    {
        public bool DisplayLanguageSelector { get; set; }

        public bool DisplayCurrencySelector { get; set; }
    }
}