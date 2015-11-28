using System.Collections.Generic;
using SmartStore.Web.Framework.Mvc;

namespace SmartStore.Web.Models.Catalog
{
    public partial class ManufacturerNavigationModel : ModelBase
    {
        public ManufacturerNavigationModel()
        {
            this.Manufacturers = new List<ManufacturerBriefInfoModel>();
        }

        public IList<ManufacturerBriefInfoModel> Manufacturers { get; set; }

        public int TotalManufacturers { get; set; }

        public bool DisplayManufacturers { get; set; }

        public bool DisplayImages { get; set; }
    }

    public partial class ManufacturerBriefInfoModel : EntityModelBase
    {
        public string Name { get; set; }

        public string SeName { get; set; }

        public string PictureUrl { get; set; }

        public bool IsActive { get; set; }
    }
}