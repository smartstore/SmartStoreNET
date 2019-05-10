using System.Data.Entity.ModelConfiguration;
using SmartStore.Core.Domain.Forums;

namespace SmartStore.Data.Mapping.Forums
{
    public partial class ForumTopicMap : EntityTypeConfiguration<ForumTopic>
    {
        public ForumTopicMap()
        {
            ToTable("Forums_Topic");
            HasKey(ft => ft.Id);
            Property(ft => ft.Subject).IsRequired().HasMaxLength(450);

            Ignore(ft => ft.ForumTopicType);
            Ignore(ft => ft.FirstPostId);

            HasRequired(ft => ft.Forum)
                .WithMany()
                .HasForeignKey(ft => ft.ForumId);

            HasRequired(ft => ft.Customer)
               .WithMany(c => c.ForumTopics)
               .HasForeignKey(ft => ft.CustomerId)
               .WillCascadeOnDelete(false);
        }
    }
}
