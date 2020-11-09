using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Catalog
{
    public class ManufacturerListModel : ModelBase
    {
        [SmartResourceDisplayName("Admin.Catalog.Manufacturers.List.SearchManufacturerName")]
        [AllowHtml]
        public string SearchManufacturerName { get; set; }

        [UIHint("Stores")]
        [SmartResourceDisplayName("Admin.Common.Store.SearchFor")]
        public int SearchStoreId { get; set; }

        public bool IsSingleStoreMode { get; set; }
        public int GridPageSize { get; set; }
    }
}