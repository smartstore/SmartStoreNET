using System.Data.Entity.ModelConfiguration;
using SmartStore.Core.Domain.Topics;

namespace SmartStore.Data.Mapping.Topics
{
    public class TopicMap : EntityTypeConfiguration<Topic>
    {
        public TopicMap()
        {
            this.ToTable("Topic");
            this.HasKey(t => t.Id);
            this.Property(t => t.ShortTitle).HasMaxLength(50);
            this.Property(t => t.Intro).HasMaxLength(255);
            this.Property(t => t.Body).IsMaxLength();
            //this.Property(t => t.IsPublished).HasColumnAnnotation("defaultValue", true);
        }
    }
}
