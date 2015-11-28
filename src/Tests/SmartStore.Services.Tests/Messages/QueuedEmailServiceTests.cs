using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Messages;
using SmartStore.Core.Events;
using SmartStore.Core.Email;
using SmartStore.Services.Messages;
using NUnit.Framework;
using Rhino.Mocks;
using SmartStore.Services.Stores;
using System.Web.Mail;
using SmartStore.Utilities;
using SmartStore.Core.Domain.Media;
using System.Threading.Tasks;

namespace SmartStore.Services.Tests.Messages
{
    [TestFixture]
	public class QueuedEmailServiceTests : ServiceTest
    {
		IRepository<QueuedEmail> _qeRepository;
		IRepository<QueuedEmailAttachment> _qeaRepository;
		IEmailSender _emailSender;
		ICommonServices _commonServices;
		QueuedEmailService _queuedEmailService;

		[SetUp]
		public new void SetUp()
		{
			_qeRepository = MockRepository.GenerateStub<IRepository<QueuedEmail>>();
			_qeaRepository = MockRepository.GenerateStub<IRepository<QueuedEmailAttachment>>();
			_emailSender = MockRepository.GenerateStub<IEmailSender>();
			_commonServices = MockRepository.GenerateStub<ICommonServices>();

			_queuedEmailService = new QueuedEmailService(_qeRepository, _qeaRepository, _emailSender, _commonServices);
		}

        [Test]
        public void Can_convert_email()
        {
			var qe = new QueuedEmail
			{
				Bcc = "bcc1@mail.com;bcc2@mail.com",
				Body = "Body",
				CC = "cc1@mail.com;cc2@mail.com",
				CreatedOnUtc = DateTime.UtcNow,
				From = "from@mail.com",
				FromName = "FromName",
				Priority = 10,
				ReplyTo = "replyto@mail.com",
				ReplyToName = "ReplyToName",
				Subject = "Subject",
				To = "to@mail.com",
				ToName = "ToName"
			};

			// load attachment file resource and save as file
			var asm = typeof(QueuedEmailServiceTests).Assembly;
			var pdfStream = asm.GetManifestResourceStream("{0}.Messages.Attachment.pdf".FormatInvariant(asm.GetName().Name));
			var pdfBinary = pdfStream.ToByteArray();
			pdfStream.Seek(0, SeekOrigin.Begin);
			var path1 = "~/Attachment.pdf";
			var path2 = CommonHelper.MapPath(path1, false);
			Assert.IsTrue(pdfStream.ToFile(path2));

			var attachBlob = new QueuedEmailAttachment 
			{ 
				StorageLocation = EmailAttachmentStorageLocation.Blob, 
				Data = pdfBinary, 
				Name = "blob.pdf", 
				MimeType = "application/pdf" 
			};
			var attachPath1 = new QueuedEmailAttachment 
			{ 
				StorageLocation = EmailAttachmentStorageLocation.Path, 
				Path = path1, 
				Name = "path1.pdf", 
				MimeType = "application/pdf" 
			};
			var attachPath2 = new QueuedEmailAttachment 
			{ 
				StorageLocation = EmailAttachmentStorageLocation.Path, 
				Path = path2, 
				Name = "path2.pdf", 
				MimeType = "application/pdf" 
			};
			var attachFile = new QueuedEmailAttachment
			{
				StorageLocation = EmailAttachmentStorageLocation.FileReference,
				Name = "file.pdf",
				MimeType = "application/pdf",
				File = new Download
				{
					ContentType = "application/pdf",
					DownloadBinary = pdfBinary,
					Extension = ".pdf",
					Filename = "file"
				}
			};

			qe.Attachments.Add(attachBlob);
			qe.Attachments.Add(attachFile);
			qe.Attachments.Add(attachPath1);
			qe.Attachments.Add(attachPath2);

			var msg = _queuedEmailService.ConvertEmail(qe);

			Assert.IsNotNull(msg);
			Assert.IsNotNull(msg.To);
			Assert.IsNotNull(msg.From);

			Assert.AreEqual(msg.ReplyTo.Count, 1);
			Assert.AreEqual(qe.ReplyTo, msg.ReplyTo.First().Address);
			Assert.AreEqual(qe.ReplyToName, msg.ReplyTo.First().DisplayName);

			Assert.AreEqual(msg.Cc.Count, 2);
			Assert.AreEqual(msg.Cc.First().Address, "cc1@mail.com");
			Assert.AreEqual(msg.Cc.ElementAt(1).Address, "cc2@mail.com");

			Assert.AreEqual(msg.Bcc.Count, 2);
			Assert.AreEqual(msg.Bcc.First().Address, "bcc1@mail.com");
			Assert.AreEqual(msg.Bcc.ElementAt(1).Address, "bcc2@mail.com");

			Assert.AreEqual(qe.Subject, msg.Subject);
			Assert.AreEqual(qe.Body, msg.Body);

			Assert.AreEqual(msg.Attachments.Count, 4);

			var attach1 = msg.Attachments.First();
			var attach2 = msg.Attachments.ElementAt(1);
			var attach3 = msg.Attachments.ElementAt(2);
			var attach4 = msg.Attachments.ElementAt(3);

			// test file names
			Assert.AreEqual(attach1.Name, "blob.pdf");
			Assert.AreEqual(attach2.Name, "file.pdf");
			Assert.AreEqual(attach3.Name, "path1.pdf");
			Assert.AreEqual(attach4.Name, "path2.pdf");

			// test file streams
			Assert.AreEqual(attach1.ContentStream.Length, pdfBinary.Length);
			Assert.AreEqual(attach2.ContentStream.Length, pdfBinary.Length);
			Assert.Greater(attach3.ContentStream.Length, 0);
			Assert.Greater(attach4.ContentStream.Length, 0);

			// cleanup
			msg.Attachments.Each(x => x.Dispose());
			msg.Attachments.Clear();

			// delete attachment file
			File.Delete(path2);
        }

    }
}