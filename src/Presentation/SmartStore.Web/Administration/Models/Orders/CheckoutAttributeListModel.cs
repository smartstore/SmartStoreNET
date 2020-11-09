using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Orders
{
    public class CheckoutAttributeListModel : ModelBase
    {
        public bool IsSingleStoreMode { get; set; }
        public int GridPageSize { get; set; }

        public IList<SelectListItem> AvailableStores { get; set; }
    }
}