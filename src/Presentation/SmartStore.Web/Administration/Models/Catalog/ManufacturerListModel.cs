using System.Web.Mvc;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Mvc;
using Telerik.Web.Mvc;

namespace SmartStore.Admin.Models.Catalog
{
    public class ManufacturerListModel : ModelBase
    {
        [SmartResourceDisplayName("Admin.Catalog.Manufacturers.List.SearchManufacturerName")]
        [AllowHtml]
        public string SearchManufacturerName { get; set; }

        public GridModel<ManufacturerModel> Manufacturers { get; set; }
    }
}