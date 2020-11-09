using System.Web.Mvc;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Catalog
{
    public class ProductAttributeOptionsSetModel : EntityModelBase
    {
        public int ProductAttributeId { get; set; }

        [AllowHtml]
        public string Name { get; set; }
    }
}