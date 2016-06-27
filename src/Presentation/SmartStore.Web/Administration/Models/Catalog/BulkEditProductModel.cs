using System.Web.Mvc;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Catalog
{
    public class BulkEditProductModel : EntityModelBase
    {
        [SmartResourceDisplayName("Admin.Catalog.BulkEdit.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }

		public string ProductTypeName { get; set; }
		public string ProductTypeLabelHint { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.BulkEdit.Fields.SKU")]
        [AllowHtml]
        public string Sku { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.BulkEdit.Fields.Price")]
        public decimal Price { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.BulkEdit.Fields.OldPrice")]
        public decimal OldPrice { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.BulkEdit.Fields.ManageInventoryMethod")]
        public string ManageInventoryMethod { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.BulkEdit.Fields.StockQuantity")]
        public int StockQuantity { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.BulkEdit.Fields.Published")]
        public bool Published { get; set; }
    }
}