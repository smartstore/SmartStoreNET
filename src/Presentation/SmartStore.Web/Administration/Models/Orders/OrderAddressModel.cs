using SmartStore.Admin.Models.Common;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Orders
{
    public class OrderAddressModel : ModelBase
    {
        public int OrderId { get; set; }

        public AddressModel Address { get; set; }
    }
}