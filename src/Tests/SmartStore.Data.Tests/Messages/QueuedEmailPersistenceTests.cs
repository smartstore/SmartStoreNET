using System;
using System.Linq;
using NUnit.Framework;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Domain.Messages;
using SmartStore.Tests;

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
                To = "To",
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
            fromDb.To.ShouldEqual("To");
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

            var file = new MediaFile
            {
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                Extension = "txt",
                Name = "file.txt",
                MimeType = "text/plain",
                MediaType = "image",
                Version = 1,
                MediaStorage = new MediaStorage { Data = new byte[10] }
            };

            // Add attachment.
            var attach = new QueuedEmailAttachment
            {
                StorageLocation = EmailAttachmentStorageLocation.FileReference,
                Name = "file.txt",
                MimeType = "text/plain",
                MediaFile = file
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

            file = attach.MediaFile;
            file.ShouldNotBeNull();

            // Delete Attachment.Download. Commented out because foreign key has no cascade delete anymore.
            // var attachId = attach.Id;
            // context.Set<MediaFile>().Remove(file);
            //context.SaveChanges();
            //base.ReloadContext();

            //attach = context.Set<QueuedEmailAttachment>().Find(attachId);
            //attach.ShouldBeNull();

            // Add new attachment.
            qe = context.Set<QueuedEmail>().FirstOrDefault();

            qe.Attachments.Add(new QueuedEmailAttachment
            {
                StorageLocation = EmailAttachmentStorageLocation.FileReference,
                Name = "file.txt",
                MimeType = "text/plain"
            });

            context.SaveChanges();
            ReloadContext();

            fromDb = context.Set<QueuedEmail>().FirstOrDefault();
            fromDb.ShouldNotBeNull();

            // Delete QueuedEmail.
            context.Set<QueuedEmail>().Remove(fromDb);
            context.SaveChanges();

            // Attachment should also be gone now.
            attach = context.Set<QueuedEmailAttachment>().FirstOrDefault();
            attach.ShouldBeNull();
        }
    }
}