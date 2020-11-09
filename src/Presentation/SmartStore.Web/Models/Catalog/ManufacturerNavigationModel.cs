using System.Collections.Generic;
using SmartStore.Services.Localization;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Web.Models.Catalog
{
    public partial class ManufacturerNavigationModel : ModelBase
    {
        public ManufacturerNavigationModel()
        {
            Manufacturers = new List<ManufacturerBriefInfoModel>();
        }

        public IList<ManufacturerBriefInfoModel> Manufacturers { get; set; }

        public bool DisplayAllManufacturersLink { get; set; }
        public bool DisplayManufacturers { get; set; }
        public bool DisplayImages { get; set; }
        public bool HideManufacturerDefaultPictures { get; set; }
        public int ManufacturerThumbPictureSize { get; set; }
    }

    public partial class ManufacturerBriefInfoModel : EntityModelBase
    {
        public LocalizedValue<string> Name { get; set; }
        public string SeName { get; set; }
        public int? FileId { get; set; }
        public int DisplayOrder { get; set; }
        public string AlternateText { get; set; }
        public string Title { get; set; }
    }
}