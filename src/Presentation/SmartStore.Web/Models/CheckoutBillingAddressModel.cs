using System.Collections.Generic;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Models.Common;

namespace SmartStore.Web.Models.Checkout
{
    public partial class CheckoutBillingAddressModel2 : ModelBase
    {
        public CheckoutBillingAddressModel2()
        {
            ExistingAddresses = new List<AddressModel>();
            NewAddress = new AddressModel();
        }

        public IList<AddressModel> ExistingAddresses { get; set; }

        public AddressModel NewAddress { get; set; }

        /// <summary>
        /// Used on one-page checkout page
        /// </summary>
        public bool NewAddressPreselected { get; set; }
    }
}