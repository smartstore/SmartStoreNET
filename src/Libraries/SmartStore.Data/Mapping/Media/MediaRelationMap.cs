using System;
using System.Data.Entity.ModelConfiguration;
using SmartStore.Core.Domain.Media;

namespace SmartStore.Data.Mapping.Media
{
    public partial class MediaRelationMap : EntityTypeConfiguration<MediaRelation>
    {
        public MediaRelationMap()
        {
            ToTable("MediaRelation");
            HasKey(c => c.Id);

            Property(c => c.EntityId);
            Property(c => c.EntityName).IsRequired().HasMaxLength(255);
        }
    }
}