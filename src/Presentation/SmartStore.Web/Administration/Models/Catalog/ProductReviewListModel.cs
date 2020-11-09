using System;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Catalog
{
    public class ProductReviewListModel : ModelBase
    {
        [SmartResourceDisplayName("Admin.Catalog.ProductReviews.List.CreatedOnFrom")]
        public DateTime? CreatedOnFrom { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.ProductReviews.List.CreatedOnTo")]
        public DateTime? CreatedOnTo { get; set; }
    }
}