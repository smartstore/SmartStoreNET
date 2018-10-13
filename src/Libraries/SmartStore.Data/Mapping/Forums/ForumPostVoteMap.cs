using System.Data.Entity.ModelConfiguration;
using SmartStore.Core.Domain.Forums;

namespace SmartStore.Data.Mapping.Forums
{
    public partial class ForumPostVoteMap : EntityTypeConfiguration<ForumPostVote>
    {
        public ForumPostVoteMap()
        {
            ToTable("ForumPostVote");

            HasRequired(fpl => fpl.ForumPost)
                .WithMany(fp => fp.ForumPostVotes)
                .HasForeignKey(fpl => fpl.ForumPostId)
                .WillCascadeOnDelete(true);
        }
    }
}
