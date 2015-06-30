using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Messages;
using SmartStore.Core.Events;

namespace SmartStore.Services.Messages
{
	public class QueuingEmailEventConsumer : IConsumer<QueuingEmailEvent>
	{
		private readonly PdfSettings _pdfSettings;

		public QueuingEmailEventConsumer(PdfSettings pdfSettings)
		{
			this._pdfSettings = pdfSettings;
		}

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
			// TODO: (mc) implement
			return null;
		}

	}
}
