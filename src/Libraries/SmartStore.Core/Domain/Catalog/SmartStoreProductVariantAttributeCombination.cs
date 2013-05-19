using System;
using System.Linq;
using System.Collections.Generic;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Directory;

namespace SmartStore.Core.Domain.Catalog
{
    /// <summary>
    /// Represents a product variant attribute combination
    /// </summary>
    public partial class ProductVariantAttributeCombination
    {

        public ProductVariantAttributeCombination()
        {
            this.IsActive = true;
        }

        public string Sku { get; set; }
		public string Gtin { get; set; }
		public string ManufacturerPartNumber { get; set; }

		public decimal? Length { get; set; }
		public decimal? Width { get; set; }
		public decimal? Height { get; set; }

		public decimal? BasePriceAmount { get; set; }
		public int? BasePriceBaseAmount { get; set; }

        public string AssignedPictureIds { get; set; }

        public int? DeliveryTimeId { get; set; }
        public virtual DeliveryTime DeliveryTime { get; set; }

		public bool IsActive { get; set; }
		//public bool IsDefaultCombination { get; set; }

        public int[] GetAssignedPictureIds()
        {
            if (string.IsNullOrEmpty(this.AssignedPictureIds))
            {
                return new int[] { };
            }

            var query = from id in this.AssignedPictureIds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        let idx = id.ToInt()
                        where idx > 0
                        select idx;

            return query.Distinct().ToArray();

        }

        public void SetAssignedPictureIds(int[] ids)
        {
            if (ids == null || ids.Length == 0)
            {
                this.AssignedPictureIds = null;
            }
            else
            {
                this.AssignedPictureIds = String.Join<int>(",", ids);
            }
        }
    }
}
