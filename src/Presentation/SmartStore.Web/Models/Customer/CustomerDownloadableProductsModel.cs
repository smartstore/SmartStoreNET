using System;
using System.Collections.Generic;
using SmartStore.Web.Framework.Mvc;

namespace SmartStore.Web.Models.Customer
{
    public partial class CustomerDownloadableProductsModel : ModelBase
    {
        public CustomerDownloadableProductsModel()
        {
            Items = new List<DownloadableProductsModel>();
        }

        public IList<DownloadableProductsModel> Items { get; set; }
        public CustomerNavigationModel NavigationModel { get; set; }

        #region Nested classes
        public partial class DownloadableProductsModel : ModelBase
        {
            public Guid OrderItemGuid { get; set; }

            public int OrderId { get; set; }

            public int ProductId { get; set; }
            public string ProductName { get; set; }
            public string ProductSeName { get; set; }
            public string ProductAttributes { get; set; }

            public int DownloadId { get; set; }
            public int LicenseId { get; set; }

            public DateTime CreatedOn { get; set; }
        }
        #endregion
    }

    public partial class UserAgreementModel : ModelBase
    {
        public Guid OrderItemGuid { get; set; }
        public string UserAgreementText { get; set; }
    }
}