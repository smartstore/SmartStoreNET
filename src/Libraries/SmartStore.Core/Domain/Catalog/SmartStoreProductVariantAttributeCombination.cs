using System;
using System.Linq;
using SmartStore.Core.Domain.Directory;
using System.Runtime.Serialization;

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

		[DataMember]
        public string Sku { get; set; }

		[DataMember]
		public string Gtin { get; set; }

		[DataMember]
		public string ManufacturerPartNumber { get; set; }

		[DataMember]
		public decimal? Price { get; set; }

		[DataMember]
		public decimal? Length { get; set; }

		[DataMember]
		public decimal? Width { get; set; }

		[DataMember]
		public decimal? Height { get; set; }

		[DataMember]
		public decimal? BasePriceAmount { get; set; }

		[DataMember]
		public int? BasePriceBaseAmount { get; set; }

		[DataMember]
        public string AssignedPictureIds { get; set; }

		[DataMember]
        public int? DeliveryTimeId { get; set; }

		[DataMember]
        public virtual DeliveryTime DeliveryTime { get; set; }

        [DataMember]
        public int? QuantityUnitId { get; set; }

        [DataMember]
        public virtual QuantityUnit QuantityUnit { get; set; }

		[DataMember]
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
