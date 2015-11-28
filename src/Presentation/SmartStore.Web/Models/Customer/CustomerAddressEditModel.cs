using SmartStore.Web.Framework.Mvc;
using SmartStore.Web.Models.Common;

namespace SmartStore.Web.Models.Customer
{
    public partial class CustomerAddressEditModel : ModelBase
    {
        public CustomerAddressEditModel()
        {
            this.Address = new AddressModel();
        }
        public AddressModel Address { get; set; }
        public CustomerNavigationModel NavigationModel { get; set; }
    }
}