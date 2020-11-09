using System;
using System.Collections.Generic;
using SmartStore.Services.Localization;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Web.Models.Customer
{
    public partial class CustomerDownloadableProductsModel : ModelBase
    {
        public CustomerDownloadableProductsModel()
        {
            Items = new List<DownloadableProductsModel>();
        }

        public IList<DownloadableProductsModel> Items { get; set; }

        #region Nested classes

        public partial class DownloadableProductsModel : ModelBase
        {
            public DownloadableProductsModel()
            {
                DownloadVersions = new List<DownloadVersion>();
            }

            public Guid OrderItemGuid { get; set; }

            public int OrderId { get; set; }

            public int ProductId { get; set; }
            public LocalizedValue<string> ProductName { get; set; }
            public string ProductSeName { get; set; }
            public string ProductUrl { get; set; }
            public string ProductAttributes { get; set; }

            public int LicenseId { get; set; }
            public bool IsDownloadAllowed { get; set; }

            public DateTime CreatedOn { get; set; }

            public List<DownloadVersion> DownloadVersions { get; set; }
        }

        #endregion
    }

    public partial class UserAgreementModel : ModelBase
    {
        public Guid OrderItemGuid { get; set; }
        public string UserAgreementText { get; set; }
        public string FileVersion { get; set; }
    }

    public class DownloadVersion
    {
        public int DownloadId { get; set; }
        public string FileName { get; set; }
        public Guid DownloadGuid { get; set; }
        public string FileVersion { get; set; }
        public string Changelog { get; set; }
    }

}