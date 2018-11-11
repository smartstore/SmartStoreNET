using SmartStore.Core.Domain.Tax;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Web.Models.Common
{
    public partial class TaxTypeSelectorModel : ModelBase
    {
        public bool Enabled { get; set; }

        public TaxDisplayType CurrentTaxType { get; set; }
    }
}