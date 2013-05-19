using System.Data.Entity.ModelConfiguration;
using SmartStore.Core.Domain.Polls;

namespace SmartStore.Data.Mapping.Polls
{
    public partial class PollMap : EntityTypeConfiguration<Poll>
    {
        public PollMap()
        {
            this.ToTable("Poll");
            this.HasKey(p => p.Id);
            this.Property(p => p.Name).IsRequired();
            
            this.HasRequired(p => p.Language)
                .WithMany()
                .HasForeignKey(p => p.LanguageId).WillCascadeOnDelete(true);
        }
    }
}