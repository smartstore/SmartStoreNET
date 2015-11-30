using System;
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

		public int ProductId { get; set; }

        public string Taxonomy { get; set; }
        public string Gender { get; set; }
        public string AgeGroup { get; set; }
        public string Color { get; set; }
        public string Size { get; set; }
        public string Material { get; set; }
        public string Pattern { get; set; }
        public string ItemGroupId { get; set; }

		public bool IsTouched { get; set; }
		public DateTime CreatedOnUtc { get; set; }
		public DateTime UpdatedOnUtc { get; set; }

		public bool Export { get; set; }
    }
}