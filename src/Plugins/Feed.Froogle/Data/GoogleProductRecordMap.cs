using System.Data.Entity.ModelConfiguration;
using SmartStore.Plugin.Feed.Froogle.Domain;

namespace SmartStore.Plugin.Feed.Froogle.Data
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