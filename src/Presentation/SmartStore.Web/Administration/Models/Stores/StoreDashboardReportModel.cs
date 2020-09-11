using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Store
{
    public class StoreDashboardReportModel : ModelBase
    {
        public string ProductsCount { get; set; }

        public string CategoriesCount { get; set; }

        public string ManufacturersCount { get; set; }

        public string MediaCount { get; set; }

        public string AttributesCount { get; set; }

        public string AttributeCombinationsCount { get; set; }

        public string OrdersCount { get; set; }

        public string Sales { get; set; }

        public string CustomersCount { get; set; }

        public string OnlineCustomersCount { get; set; }

        public string WishlistsValue { get; set; }

        public string CartsValue { get; set; }
    }
}