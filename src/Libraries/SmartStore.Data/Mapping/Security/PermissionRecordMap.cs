using System.Data.Entity.ModelConfiguration;
using SmartStore.Core.Domain.Security;

namespace SmartStore.Data.Mapping.Security
{
    public partial class PermissionRecordMap : EntityTypeConfiguration<PermissionRecord>
    {
        public PermissionRecordMap()
        {
            ToTable("PermissionRecord");
            HasKey(pr => pr.Id);
            Property(pr => pr.SystemName).IsRequired().HasMaxLength(255);
        }
    }
}