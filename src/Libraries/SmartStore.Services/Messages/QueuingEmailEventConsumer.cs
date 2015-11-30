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
using SmartStore.Core.Localization;
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
			T = NullLocalizer.Instance;
		}

		public ILogger Logger { get; set; }
		public Localizer T { get; set; }

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
				try
				{
					var qea = CreatePdfInvoiceAttachment(orderId);
					qe.Attachments.Add(qea);
				}
				catch (Exception ex)
				{
					Logger.Error(T("Admin.System.QueuedEmails.ErrorCreatingAttachment"), ex);
				}
			}
		}

		private QueuedEmailAttachment CreatePdfInvoiceAttachment(int orderId)
		{
			var urlHelper = new UrlHelper(_httpRequest.RequestContext);
			var path = urlHelper.Action("Print", "Order", new { id = orderId, pdf = true, area = "" });

			var fileResponse = _fileDownloadManager.Value.DownloadFile(path, true, 5000);

			if (fileResponse == null)
			{
				throw new InvalidOperationException(T("Admin.System.QueuedEmails.ErrorEmptyAttachmentResult", path));
			}

			if (!fileResponse.ContentType.IsCaseInsensitiveEqual("application/pdf"))
			{
				throw new InvalidOperationException(T("Admin.System.QueuedEmails.ErrorNoPdfAttachment"));
			}

			return new QueuedEmailAttachment
			{
				StorageLocation = EmailAttachmentStorageLocation.Blob,
				Data = fileResponse.Data,
				MimeType = fileResponse.ContentType,
				Name = fileResponse.FileName
			};
		}

	}
}
