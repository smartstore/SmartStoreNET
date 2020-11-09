using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
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

        [DataMember]
        public string Sku { get; set; }

        [DataMember]
        [Index]
        public string Gtin { get; set; }

        [DataMember]
        [Index]
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
        public string AssignedMediaFileIds { get; set; }

        [DataMember]
        public int? DeliveryTimeId { get; set; }

        [DataMember]
        public virtual DeliveryTime DeliveryTime { get; set; }

        [DataMember]
        public int? QuantityUnitId { get; set; }

        [DataMember]
        public virtual QuantityUnit QuantityUnit { get; set; }

        [DataMember]
        [Index]
        public bool IsActive { get; set; }
        //public bool IsDefaultCombination { get; set; }

        public int[] GetAssignedMediaIds()
        {
            if (string.IsNullOrEmpty(this.AssignedMediaFileIds))
            {
                return new int[] { };
            }

            var query = from id in this.AssignedMediaFileIds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        let idx = id.ToInt()
                        where idx > 0
                        select idx;

            return query.Distinct().ToArray();
        }

        public void SetAssignedMediaIds(int[] ids)
        {
            if (ids == null || ids.Length == 0)
            {
                this.AssignedMediaFileIds = null;
            }
            else
            {
                this.AssignedMediaFileIds = String.Join<int>(",", ids);
            }
        }
    }
}
