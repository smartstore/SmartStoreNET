using System.Data.Entity.ModelConfiguration;
using SmartStore.Core.Domain.Security;

namespace SmartStore.Data.Mapping.Seo
{
    public partial class AclRecordMap : EntityTypeConfiguration<AclRecord>
    {
        public AclRecordMap()
        {
            this.ToTable("AclRecord");
            this.HasKey(lp => lp.Id);

            this.Property(lp => lp.EntityName).IsRequired().HasMaxLength(400);
            this.Property(x => x.IsIdle).IsRequired();

            this.HasRequired(tp => tp.CustomerRole)
                .WithMany()
                .HasForeignKey(tp => tp.CustomerRoleId)
                .WillCascadeOnDelete(true);
        }
    }
}