using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using SmartStore.Core;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Messages;
using SmartStore.Core.Events;
using SmartStore.Core.Logging;
using SmartStore.Utilities;

namespace SmartStore.Services.Messages
{
	public class QueuingEmailEventConsumer : IConsumer<QueuingEmailEvent>
	{
		private readonly PdfSettings _pdfSettings;
		private readonly HttpRequestBase _httpRequest;
		private readonly Lazy<FileDownloadManager> _fileDownloadManager;

		public QueuingEmailEventConsumer(
			PdfSettings pdfSettings, 
			HttpRequestBase httpRequest, 
			Lazy<FileDownloadManager> fileDownloadManager)
		{
			this._pdfSettings = pdfSettings;
			this._httpRequest = httpRequest;
			this._fileDownloadManager = fileDownloadManager;

			Logger = NullLogger.Instance;
		}

		public ILogger Logger { get; set; }

		public void HandleEvent(QueuingEmailEvent eventMessage)
		{
			var qe = eventMessage.QueuedEmail;
			var tpl = eventMessage.MessageTemplate;

			var handledTemplates = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase)
			{
				{ "OrderPlaced.CustomerNotification", _pdfSettings.AttachOrderPdfToOrderPlacedEmail },
				{ "OrderCompleted.CustomerNotification", _pdfSettings.AttachOrderPdfToOrderCompletedEmail }
			};
			
			bool shouldHandle = false;
			if (handledTemplates.TryGetValue(tpl.Name, out shouldHandle) && shouldHandle)
			{
				var orderId = eventMessage.Tokens.First(x => x.Key.IsCaseInsensitiveEqual("Order.ID")).Value.ToInt();
				var qea = CreatePdfInvoiceAttachment(orderId);
				if (qea != null)
				{
					qe.Attachments.Add(qea);
				}
			}
		}

		private QueuedEmailAttachment CreatePdfInvoiceAttachment(int orderId)
		{
			var urlHelper = new UrlHelper(_httpRequest.RequestContext);
			var path = urlHelper.Action("Print", "Order", new { id = orderId, pdf = true, area = "" });

			try
			{
				var fileResponse = _fileDownloadManager.Value.DownloadFile(path, true, 5000);

				if (fileResponse == null)
				{
					// ...
				}

				if (!fileResponse.ContentType.IsCaseInsensitiveEqual("application/pdf"))
				{
					// ...
				}

				return new QueuedEmailAttachment
				{
					StorageLocation = EmailAttachmentStorageLocation.Blob,
					Data = fileResponse.Data,
					MimeType = fileResponse.ContentType,
					Name = fileResponse.FileName
				};
			}
			catch (Exception ex)
			{
				// TODO localize
				Logger.Error("Error occured while creating e-mail attachment", ex);
			}

			return null;
		}

	}
}
