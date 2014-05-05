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
			this.Property(t => t.Body).IsMaxLength();
        }
    }
}
