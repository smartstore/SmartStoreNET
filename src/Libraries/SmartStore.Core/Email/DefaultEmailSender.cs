using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using net = System.Net.Mail;
using System.Net.Mime;
using System.Net;
using System.IO;
using System.ComponentModel;
using System.Threading.Tasks;

namespace SmartStore.Core.Email
{
    public class DefaultEmailSender : IEmailSender
    {

        public DefaultEmailSender() 
		{ 
		}

        /// <summary>
        /// Builds System.Net.Mail.Message
        /// </summary>
        /// <param name="original">SmartStore.Email.Message</param>
        /// <returns>System.Net.Mail.Message</returns>        
        protected virtual net.MailMessage BuildMailMessage(EmailMessage original)
        {
            net.MailMessage msg = new net.MailMessage();

			if (String.IsNullOrEmpty(original.Subject))
			{
				throw new MailSenderException("Required subject is missing!");
			}
				
			msg.Subject = original.Subject;
			msg.IsBodyHtml = original.BodyFormat == MailBodyFormat.Html;

            if (original.AltText.HasValue())
            {
                msg.AlternateViews.Add(net.AlternateView.CreateAlternateViewFromString(original.AltText, new ContentType("text/html")));
                msg.AlternateViews.Add(net.AlternateView.CreateAlternateViewFromString(original.Body, new ContentType("text/plain")));
            }
            else
            {
                msg.Body = original.Body;
            }

            msg.DeliveryNotificationOptions = net.DeliveryNotificationOptions.None;

            msg.From = original.From.ToMailAddress();

			msg.To.AddRange(original.To.Where(x => x.Address.HasValue()).Select(x => x.ToMailAddress()));
			msg.CC.AddRange(original.Cc.Where(x => x.Address.HasValue()).Select(x => x.ToMailAddress()));
			msg.Bcc.AddRange(original.Bcc.Where(x => x.Address.HasValue()).Select(x => x.ToMailAddress()));
			msg.ReplyToList.AddRange(original.ReplyTo.Where(x => x.Address.HasValue()).Select(x => x.ToMailAddress()));

            foreach (Attachment attachment in original.Attachments) 
            {
                byte[] byteData;

                if (attachment.ContentTransferEncoding == TransferEncoding.Base64)
                {
					using (var sr = new StreamReader(attachment.Stream))
					{
						byteData = Convert.FromBase64String(sr.ReadToEnd());
					}
                }
                else
                {
                    byteData = attachment.Stream.ToByteArray();
                }

                MemoryStream s = new MemoryStream(byteData);
                net.Attachment att = new net.Attachment(s, attachment.Name, attachment.ContentType.MediaType);

                att.ContentType.MediaType = attachment.MediaType;
                att.TransferEncoding = attachment.ContentTransferEncoding;
                att.ContentDisposition.DispositionType = attachment.ContentDisposition.DispositionType;
 
                msg.Attachments.Add(att);
            }

            if (original.Headers != null)
				msg.Headers.AddRange(original.Headers);
            

            msg.Priority = original.Priority;

            return msg;
        }

        #region IMailSender Members

        public void SendEmail(SmtpContext context, EmailMessage message)
        {
			Guard.ArgumentNotNull(() => context);
			Guard.ArgumentNotNull(() => message);
			
			var msg = this.BuildMailMessage(message);

			using (var client = context.ToSmtpClient())
			{
				client.Send(msg);
			}
        }

		public Task SendEmailAsync(SmtpContext context, EmailMessage message)
		{
			Guard.ArgumentNotNull(() => context);
			Guard.ArgumentNotNull(() => message);

			var msg = this.BuildMailMessage(message);

			using (var client = context.ToSmtpClient())
			{
				return client.SendMailAsync(msg);
			}
		}

        #endregion

    }
}
