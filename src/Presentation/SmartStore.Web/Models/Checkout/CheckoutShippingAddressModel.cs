using System.Collections.Generic;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Models.Common;

namespace SmartStore.Web.Models.Checkout
{
    public partial class CheckoutShippingAddressModel : ModelBase
    {
        public CheckoutShippingAddressModel()
        {
            ExistingAddresses = new List<AddressModel>();
            NewAddress = new AddressModel();
        }

        public IList<AddressModel> ExistingAddresses { get; set; }

        public AddressModel NewAddress { get; set; }

        public bool NewAddressPreselected { get; set; }
    }
}