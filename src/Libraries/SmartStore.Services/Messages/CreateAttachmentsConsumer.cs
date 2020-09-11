using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Domain.Messages;
using SmartStore.Core.Events;
using SmartStore.Core.Localization;
using SmartStore.Core.Logging;
using SmartStore.Utilities;

namespace SmartStore.Services.Messages
{
    public class CreateAttachmentsConsumer : IConsumer
    {
        private readonly PdfSettings _pdfSettings;
        private readonly HttpRequestBase _httpRequest;
        private readonly Lazy<FileDownloadManager> _fileDownloadManager;

        public CreateAttachmentsConsumer(
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

        public void HandleEvent(MessageQueuingEvent message)
        {
            var qe = message.QueuedEmail;
            var ctx = message.MessageContext;
            var model = message.MessageModel;

            var handledTemplates = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase)
            {
                { "OrderPlaced.CustomerNotification", _pdfSettings.AttachOrderPdfToOrderPlacedEmail },
                { "OrderCompleted.CustomerNotification", _pdfSettings.AttachOrderPdfToOrderCompletedEmail }
            };

            if (handledTemplates.TryGetValue(ctx.MessageTemplate.Name, out var shouldHandle) && shouldHandle)
            {
                if (model.Get("Order") is IDictionary<string, object> order && order.Get("ID") is int orderId)
                {
                    try
                    {
                        var qea = CreatePdfInvoiceAttachment(orderId);
                        qe.Attachments.Add(qea);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, T("Admin.System.QueuedEmails.ErrorCreatingAttachment"));
                    }
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
                MediaStorage = new MediaStorage { Data = fileResponse.Data },
                MimeType = fileResponse.ContentType,
                Name = fileResponse.FileName
            };
        }

    }
}
