using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Domain.Messages;
using SmartStore.Tests;
using NUnit.Framework;
using SmartStore.Core.Domain.Media;

namespace SmartStore.Data.Tests.Messages
{
    [TestFixture]
    public class QueuedEmailPersistenceTests : PersistenceTest
    {
        [Test]
        public void Can_save_and_load_queuedEmail()
        {
            var qe = new QueuedEmail
            {
                Priority = 1,
                From = "From",
                FromName = "FromName",
                To = "To",
                ToName = "ToName",
                CC = "CC",
                Bcc = "Bcc",
                Subject = "Subject",
                Body = "Body",
                CreatedOnUtc = new DateTime(2010, 01, 01),
                SentTries = 5,
                SentOnUtc = new DateTime(2010, 02, 02),
                EmailAccount = new EmailAccount
                {
                    Email = "admin@yourstore.com",
                    DisplayName = "Administrator",
                    Host = "127.0.0.1",
                    Port = 125,
                    Username = "John",
                    Password = "111",
                    EnableSsl = true,
                    UseDefaultCredentials = true
                },
            };

            var fromDb = SaveAndLoadEntity(qe);
            fromDb.ShouldNotBeNull();
            fromDb.Priority.ShouldEqual(1);
            fromDb.From.ShouldEqual("From");
            fromDb.FromName.ShouldEqual("FromName");
            fromDb.To.ShouldEqual("To");
            fromDb.ToName.ShouldEqual("ToName");
            fromDb.CC.ShouldEqual("CC");
            fromDb.Bcc.ShouldEqual("Bcc");
            fromDb.Subject.ShouldEqual("Subject");
            fromDb.Body.ShouldEqual("Body");
            fromDb.CreatedOnUtc.ShouldEqual(new DateTime(2010, 01, 01));
            fromDb.SentTries.ShouldEqual(5);
            fromDb.SentOnUtc.Value.ShouldEqual(new DateTime(2010, 02, 02));

            fromDb.EmailAccount.ShouldNotBeNull();
            fromDb.EmailAccount.DisplayName.ShouldEqual("Administrator");
        }

		[Test]
		public void Can_cascade_delete_attachment()
		{
			var account = new EmailAccount
			{
				Email = "admin@yourstore.com",
				Host = "127.0.0.1",
				Username = "John",
				Password = "111"
			};

			var download = new Download
			{
				ContentType = "text/plain",
				DownloadBinary = new byte[10],
				DownloadGuid = Guid.NewGuid(),
				Extension = "txt",
				Filename = "file"
			};

			// add attachment
			var attach = new QueuedEmailAttachment
			{
				StorageLocation = EmailAttachmentStorageLocation.FileReference,
				Name = "file.txt",
				MimeType = "text/plain",
				File = download
			};
			
			var qe = new QueuedEmail
			{
				Priority = 1,
				From = "From",
				To = "To",
				Subject = "Subject",
				CreatedOnUtc = DateTime.UtcNow,
				EmailAccount = account
			};

			qe.Attachments.Add(attach);

			var fromDb = SaveAndLoadEntity(qe);
			fromDb.ShouldNotBeNull();

			Assert.AreEqual(fromDb.Attachments.Count, 1);
			attach = fromDb.Attachments.FirstOrDefault();
			attach.ShouldNotBeNull();

			download = attach.File;
			download.ShouldNotBeNull();

			var attachId = attach.Id;
			var downloadId = download.Id;

			// delete Attachment.Download
			context.Set<Download>().Remove(download);
			context.SaveChanges();
			base.ReloadContext();

			attach = context.Set<QueuedEmailAttachment>().Find(attachId);
			attach.ShouldBeNull();

			// add new attachment
			attach = new QueuedEmailAttachment
			{
				StorageLocation = EmailAttachmentStorageLocation.FileReference,
				Name = "file.txt",
				MimeType = "text/plain"
			};

			qe = context.Set<QueuedEmail>().FirstOrDefault();
			qe.Attachments.Add(attach);

			fromDb = SaveAndLoadEntity(qe);
			fromDb.ShouldNotBeNull();

			// delete QueuedEmail
			context.Set<QueuedEmail>().Remove(fromDb);
			context.SaveChanges();
			base.ReloadContext();

			// Attachment should also be gone now
			attach = context.Set<QueuedEmailAttachment>().FirstOrDefault();
			attach.ShouldBeNull();
		}
    }
}