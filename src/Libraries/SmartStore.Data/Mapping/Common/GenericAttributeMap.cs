using System.Data.Entity.ModelConfiguration;
using SmartStore.Core.Domain.Common;

namespace SmartStore.Data.Mapping.Common
{
    public partial class GenericAttributeMap : EntityTypeConfiguration<GenericAttribute>
    {
        public GenericAttributeMap()
        {
            this.ToTable("GenericAttribute");
            this.HasKey(ga => ga.Id);

            this.Property(ga => ga.KeyGroup).IsRequired().HasMaxLength(400);
            this.Property(ga => ga.Key).IsRequired().HasMaxLength(400);
            this.Property(ga => ga.Value).IsRequired().IsMaxLength();
        }
    }
}