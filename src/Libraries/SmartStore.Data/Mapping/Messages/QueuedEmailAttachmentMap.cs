using System.Data.Entity.ModelConfiguration;
using SmartStore.Core.Domain.Messages;

namespace SmartStore.Data.Mapping.Messages
{
	public class QueuedEmailAttachmentMap : EntityTypeConfiguration<QueuedEmailAttachment>
	{
		public QueuedEmailAttachmentMap()
		{
			this.ToTable("QueuedEmailAttachment");
			this.HasKey(x => x.Id);

			this.Property(x => x.Path).IsOptional().HasMaxLength(1000);

#pragma warning disable 612, 618
			this.Property(x => x.Data).IsMaxLength();
#pragma warning restore 612, 618

			this.HasOptional(x => x.File)
				.WithMany()
				.HasForeignKey(x => x.FileId)
				.WillCascadeOnDelete(true);

			this.Property(x => x.Name).IsRequired().HasMaxLength(200);
			this.Property(x => x.MimeType).IsRequired().HasMaxLength(200);

			this.HasRequired(x => x.QueuedEmail)
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
