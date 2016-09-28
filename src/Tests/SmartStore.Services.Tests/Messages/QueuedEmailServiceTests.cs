using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Domain.Messages;
using SmartStore.Core.Email;
using SmartStore.Core.Events;
using SmartStore.Services.Configuration;
using SmartStore.Services.Media;
using SmartStore.Services.Messages;
using SmartStore.Services.Tests.Configuration;
using SmartStore.Utilities;

namespace SmartStore.Services.Tests.Messages
{
	[TestFixture]
	public class QueuedEmailServiceTests : ServiceTest
    {
		IRepository<QueuedEmail> _qeRepository;
		IRepository<QueuedEmailAttachment> _qeaRepository;
		IRepository<Download> _downloadRepository;
		IEmailSender _emailSender;
		ICommonServices _services;
		IDownloadService _downloadService;
		QueuedEmailService _queuedEmailService;
		ISettingService _settingService;
		IEventPublisher _eventPublisher;

		[SetUp]
		public new void SetUp()
		{
			_qeRepository = MockRepository.GenerateMock<IRepository<QueuedEmail>>();
			_qeaRepository = MockRepository.GenerateMock<IRepository<QueuedEmailAttachment>>();
			_downloadRepository = MockRepository.GenerateMock<IRepository<Download>>();
			_emailSender = MockRepository.GenerateMock<IEmailSender>();
			_services = MockRepository.GenerateMock<ICommonServices>();
			_eventPublisher = MockRepository.GenerateMock<IEventPublisher>();

			_settingService = new ConfigFileSettingService(null, null, null);
			_services.Expect(x => x.Settings).Return(_settingService);

			_downloadService = new DownloadService(_downloadRepository, _eventPublisher, _settingService, ProviderManager);

			_queuedEmailService = new QueuedEmailService(_qeRepository, _qeaRepository, _emailSender, _services, _downloadService, ProviderManager);
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
				MediaStorage = new MediaStorage { Id = 1, Data = pdfBinary },
				MediaStorageId = 1,
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
					MediaStorage = new MediaStorage { Id = 2, Data = pdfBinary },
					MediaStorageId = 2,
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