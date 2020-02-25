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

#pragma warning disable 612, 618
			Property(x => x.Data).IsMaxLength();
#pragma warning restore 612, 618

			HasOptional(x => x.File)
				.WithMany()
				.HasForeignKey(x => x.FileId)
				.WillCascadeOnDelete(true);

			Property(x => x.Name).IsRequired().HasMaxLength(200);
			Property(x => x.MimeType).IsRequired().HasMaxLength(200);

			HasRequired(x => x.QueuedEmail)
				.WithMany(qe => qe.Attachments)
				.HasForeignKey(x => x.QueuedEmailId)
				.WillCascadeOnDelete(true);

			HasOptional(x => x.MediaStorage)
				.WithMany()
				.HasForeignKey(x => x.MediaStorageId)
				.WillCascadeOnDelete(false);
		}
	}
}
