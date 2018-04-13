using System.Collections.Generic;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Services.Localization;

namespace SmartStore.Web.Models.Checkout
{
    public partial class CheckoutPaymentMethodModel : ModelBase
    {
        public CheckoutPaymentMethodModel()
        {
            PaymentMethods = new List<PaymentMethodModel>();
        }

        public List<PaymentMethodModel> PaymentMethods { get; set; }

        public bool SkippedSelectShipping { get; set; }

        #region Nested classes

        public partial class PaymentMethodModel : ModelBase
        {
            public string PaymentMethodSystemName { get; set; }
            public string Name { get; set; }
			public string Description { get; set; }
			public LocalizedValue<string> FullDescription { get; set; }
            public string BrandUrl { get; set; }
            public string Fee { get; set; }
            public bool Selected { get; set; }
			public RouteInfo PaymentInfoRoute { get; set; }
			public bool RequiresInteraction { get; set; }
        }

        #endregion
    }
}