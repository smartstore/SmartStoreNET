﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;
using System.Net.Mime;
using System.Net;
using System.IO;
using System.ComponentModel;
using System.Threading.Tasks;

namespace SmartStore.Core.Email
{
    public class DefaultEmailSender : IEmailSender
    {

        /// <summary>
        /// Builds System.Net.Mail.Message
        /// </summary>
        /// <param name="original">SmartStore.Email.Message</param>
        /// <returns>System.Net.Mail.Message</returns>        
        protected virtual MailMessage BuildMailMessage(EmailMessage original)
        {
            MailMessage msg = new MailMessage();

			if (String.IsNullOrEmpty(original.Subject))
			{
				throw new MailSenderException("Required subject is missing!");
			}
				
			msg.Subject = original.Subject;
			msg.IsBodyHtml = original.BodyFormat == MailBodyFormat.Html;

            if (original.AltText.HasValue())
            {
                msg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(original.AltText, new ContentType("text/html")));
                msg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(original.Body, new ContentType("text/plain")));
            }
            else
            {
                msg.Body = original.Body;
            }

            msg.DeliveryNotificationOptions = DeliveryNotificationOptions.None;

            msg.From = original.From.ToMailAddress();

			msg.To.AddRange(original.To.Where(x => x.Address.HasValue()).Select(x => x.ToMailAddress()));
			msg.CC.AddRange(original.Cc.Where(x => x.Address.HasValue()).Select(x => x.ToMailAddress()));
			msg.Bcc.AddRange(original.Bcc.Where(x => x.Address.HasValue()).Select(x => x.ToMailAddress()));
			msg.ReplyToList.AddRange(original.ReplyTo.Where(x => x.Address.HasValue()).Select(x => x.ToMailAddress()));

			msg.Attachments.AddRange(original.Attachments);

            if (original.Headers != null)
				msg.Headers.AddRange(original.Headers);
            

            msg.Priority = original.Priority;

            return msg;
        }

        #region IMailSender Members

        public void SendEmail(SmtpContext context, EmailMessage message)
        {
			Guard.NotNull(context, nameof(context));
			Guard.NotNull(message, nameof(message));
			
			using (var msg = this.BuildMailMessage(message))
			{
				using (var client = context.ToSmtpClient())
				{
					client.Send(msg);
				}
			}
        }

		public Task SendEmailAsync(SmtpContext context, EmailMessage message)
		{
			Guard.NotNull(context, nameof(context));
			Guard.NotNull(message, nameof(message));

			var client = context.ToSmtpClient();
			var msg = this.BuildMailMessage(message);

			return client.SendMailAsync(msg).ContinueWith(t => 
			{
				client.Dispose();
				msg.Dispose();
			});
		}

        #endregion

    }
}
