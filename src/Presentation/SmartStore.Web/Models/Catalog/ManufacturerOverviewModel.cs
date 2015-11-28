using System.Collections.Generic;
using SmartStore.Web.Framework.Mvc;
using SmartStore.Web.Models.Media;

namespace SmartStore.Web.Models.Catalog
{
    public partial class ManufacturerOverviewModel : EntityModelBase
    {
        public ManufacturerOverviewModel()
        {
            PictureModel = new PictureModel();
        }

        public string Name { get; set; }
        public string SeName { get; set; }
        public string Description { get; set; }
        
        //picture
        public PictureModel PictureModel { get; set; }
    }
}