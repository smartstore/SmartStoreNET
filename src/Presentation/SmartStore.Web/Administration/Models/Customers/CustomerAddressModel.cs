using SmartStore.Admin.Models.Common;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Customers
{
    public class CustomerAddressModel : ModelBase
    {
        public int CustomerId { get; set; }
        public string Username { get; set; }

        public AddressModel Address { get; set; }
    }
}