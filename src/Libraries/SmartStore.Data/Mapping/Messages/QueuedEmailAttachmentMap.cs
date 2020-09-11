using System.Data.Entity.ModelConfiguration;
using SmartStore.Core.Domain.Messages;

namespace SmartStore.Data.Mapping.Messages
{
    public class QueuedEmailAttachmentMap : EntityTypeConfiguration<QueuedEmailAttachment>
    {
        public QueuedEmailAttachmentMap()
        {
            ToTable("QueuedEmailAttachment");
            HasKey(x => x.Id);

            Property(x => x.Path).IsOptional().HasMaxLength(1000);

            Property(x => x.Name).IsRequired().HasMaxLength(200);
            Property(x => x.MimeType).IsRequired().HasMaxLength(200);

            HasRequired(x => x.QueuedEmail)
                .WithMany(qe => qe.Attachments)
                .HasForeignKey(x => x.QueuedEmailId)
                .WillCascadeOnDelete(true);

            HasOptional(x => x.MediaFile)
                .WithMany()
                .HasForeignKey(x => x.MediaFileId)
                .WillCascadeOnDelete(false);

            HasOptional(x => x.MediaStorage)
                .WithMany()
                .HasForeignKey(x => x.MediaStorageId)
                .WillCascadeOnDelete(true);
        }
    }
}
