using System.Collections.Generic;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Web.Models.Checkout
{
    public partial class CheckoutShippingMethodModel : ModelBase
    {
        public CheckoutShippingMethodModel()
        {
            ShippingMethods = new List<ShippingMethodModel>();
            Warnings = new List<string>();
        }

        public IList<ShippingMethodModel> ShippingMethods { get; set; }

        public IList<string> Warnings { get; set; }

        #region Nested classes

        public partial class ShippingMethodModel : ModelBase
        {
            public int ShippingMethodId { get; set; }
            public string ShippingRateComputationMethodSystemName { get; set; }
            public string Name { get; set; }
            public string BrandUrl { get; set; }
            public string Description { get; set; }
            public string Fee { get; set; }
            public decimal FeeRaw { get; set; }
            public bool Selected { get; set; }
        }
        #endregion
    }
}