using System.Data.Entity.ModelConfiguration;
using SmartStore.Core.Domain.Polls;

namespace SmartStore.Data.Mapping.Polls
{
    public partial class PollVotingRecordMap : EntityTypeConfiguration<PollVotingRecord>
    {
        public PollVotingRecordMap()
        {
            this.ToTable("PollVotingRecord");
            //commented because it's already configured by CustomerContentMap class
            //this.HasKey(pr => pr.Id);

            this.HasRequired(pvr => pvr.PollAnswer)
                .WithMany(pa => pa.PollVotingRecords)
                .HasForeignKey(pvr => pvr.PollAnswerId);
        }
    }
}