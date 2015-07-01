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

namespace SmartStore.Services.Messages
{
	public class QueuingEmailEventConsumer : IConsumer<QueuingEmailEvent>
	{
		private readonly PdfSettings _pdfSettings;
		private readonly HttpRequestBase _httpRequest;

		public QueuingEmailEventConsumer(PdfSettings pdfSettings, HttpRequestBase httpRequest)
		{
			this._pdfSettings = pdfSettings;
			this._httpRequest = httpRequest;

			Logger = NullLogger.Instance;
		}

		public ILogger Logger { get; set; }

		public void HandleEvent(QueuingEmailEvent eventMessage)
		{
			var qe = eventMessage.QueuedEmail;
			var tpl = eventMessage.MessageTemplate;

			// TODO: (mc) determine and apply PdfSettings

			if (tpl.Name.IsCaseInsensitiveEqual("OrderPlaced.CustomerNotification"))
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
			// TODO: (mc) create common util function 'CreateAttachmentFromUrl()' OR 'DownloadFile()'

			var urlHelper = new UrlHelper(_httpRequest.RequestContext);
			var path = urlHelper.Action("Print", "Order", new { id = orderId, pdf = true });
			var url = WebHelper.GetAbsoluteUrl(path, _httpRequest);
			
			var request = (HttpWebRequest)WebRequest.Create(url);
			request.UserAgent = "SmartStore.NET";
			request.Timeout = 5000;

			request.SetFormsAuthenticationCookie(_httpRequest);

			HttpWebResponse response = null;

			try
			{
				response = (HttpWebResponse)request.GetResponse();

				using (var stream = response.GetResponseStream())
				{
					if (response.StatusCode == HttpStatusCode.OK && response.ContentType == "application/pdf")
					{
						var pdfBinary = stream.ToByteArray();

						var cd = new ContentDisposition(response.Headers["Content-Disposition"]);
						var fileName = cd.FileName;

						return new QueuedEmailAttachment
						{
							StorageLocation = EmailAttachmentStorageLocation.Blob,
							Data = pdfBinary,
							MimeType = "application/pdf",
							Name = fileName
						};
					}
				}
			}
			catch (Exception ex)
			{
				// TODO localize
				Logger.Error("Error occured while creating e-mail attachment", ex);
			}
			finally
			{
				if (response != null)
				{
					response.Close();
					response.Dispose();
				}
			}

			return null;
		}

	}
}
