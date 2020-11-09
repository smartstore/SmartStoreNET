using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Orders
{
    public class BestsellersReportLineModel : ModelBase
    {
        [SmartResourceDisplayName("Admin.SalesReport.Bestsellers.Fields.TotalAmount")]
        public string TotalAmount { get; set; }

        [SmartResourceDisplayName("Admin.SalesReport.Bestsellers.Fields.TotalQuantity")]
        public string TotalQuantity { get; set; }

        [SmartResourceDisplayName("Admin.Common.Entity.Fields.Id")]
        public int ProductId { get; set; }
        public string ProductTypeName { get; set; }
        public string ProductTypeLabelHint { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.PictureThumbnailUrl")]
        public string PictureThumbnailUrl { get; set; }
        public bool NoThumb { get; set; }

        [SmartResourceDisplayName("Admin.SalesReport.Bestsellers.Fields.Name")]
        public string ProductName { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.Sku")]
        public string Sku { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.StockQuantity")]
        public int StockQuantity { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.Price")]
        public decimal Price { get; set; }
    }
}