using System.Data.Entity.ModelConfiguration;
using SmartStore.GoogleMerchantCenter.Domain;

namespace SmartStore.GoogleMerchantCenter.Data
{
    public partial class GoogleProductRecordMap : EntityTypeConfiguration<GoogleProductRecord>
    {
        public GoogleProductRecordMap()
        {
            this.ToTable("GoogleProduct");
            this.HasKey(x => x.Id);
        }
    }
}