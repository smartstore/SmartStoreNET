using System.Data.Entity.ModelConfiguration;
using SmartStore.MegaMenu.Domain;

namespace SmartStore.MegaMenu.Data
{
    public partial class MegaMenuRecordMap : EntityTypeConfiguration<MegaMenuRecord>
    {
        public MegaMenuRecordMap()
        {
            this.ToTable("MegaMenu");
            this.HasKey(x => x.Id);
        }
    }
}