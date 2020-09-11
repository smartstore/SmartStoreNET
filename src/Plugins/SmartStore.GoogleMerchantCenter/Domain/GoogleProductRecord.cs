using System;
using System.ComponentModel.DataAnnotations.Schema;
using SmartStore.Core;

namespace SmartStore.GoogleMerchantCenter.Domain
{
    /// <summary>
    /// Represents a Google product record
    /// </summary>
    public partial class GoogleProductRecord : BaseEntity
    {
        public GoogleProductRecord()
        {
            Export = true;
        }

        [Index]
        public int ProductId { get; set; }

        public string Taxonomy { get; set; }
        public string Gender { get; set; }
        public string AgeGroup { get; set; }
        public string Color { get; set; }
        public string Size { get; set; }
        public string Material { get; set; }
        public string Pattern { get; set; }
        public string ItemGroupId { get; set; }

        [Index]
        public bool IsTouched { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public DateTime UpdatedOnUtc { get; set; }

        [Index]
        public bool Export { get; set; }

        public int Multipack { get; set; }
        public bool? IsBundle { get; set; }
        public bool? IsAdult { get; set; }
        public string EnergyEfficiencyClass { get; set; }

        public string CustomLabel0 { get; set; }
        public string CustomLabel1 { get; set; }
        public string CustomLabel2 { get; set; }
        public string CustomLabel3 { get; set; }
        public string CustomLabel4 { get; set; }
    }
}