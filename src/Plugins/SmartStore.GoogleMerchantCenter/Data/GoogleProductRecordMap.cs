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

            this.Property(x => x.EnergyEfficiencyClass).HasMaxLength(50);

            this.Property(x => x.CustomLabel0).HasMaxLength(100);
            this.Property(x => x.CustomLabel1).HasMaxLength(100);
            this.Property(x => x.CustomLabel2).HasMaxLength(100);
            this.Property(x => x.CustomLabel3).HasMaxLength(100);
            this.Property(x => x.CustomLabel4).HasMaxLength(100);
        }
    }
}