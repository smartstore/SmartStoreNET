using System;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Threading.Tasks;
using SmartStore.Core.Domain.Messages;

namespace SmartStore.Core.Email
{
    public class DefaultEmailSender : IEmailSender
    {
        private readonly EmailAccountSettings _emailAccountSettings;

        public DefaultEmailSender(EmailAccountSettings emailAccountSettings)
        {
            _emailAccountSettings = emailAccountSettings;
        }

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
                msg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(original.Body, new ContentType("text/html")));
                msg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(original.AltText, new ContentType("text/plain")));
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
                    ApplySettings(client);
                    client.Send(msg);
                }
            }
        }

        public async Task SendEmailAsync(SmtpContext context, EmailMessage message)
        {
            Guard.NotNull(context, nameof(context));
            Guard.NotNull(message, nameof(message));

            using (var msg = this.BuildMailMessage(message))
            {
                using (var client = context.ToSmtpClient())
                {
                    ApplySettings(client);
                    await client.SendMailAsync(msg);
                }
            }
        }

        private void ApplySettings(SmtpClient client)
        {
            var pickupDirLocation = _emailAccountSettings.PickupDirectoryLocation;
            if (pickupDirLocation.HasValue() && client.DeliveryMethod != SmtpDeliveryMethod.SpecifiedPickupDirectory && Path.IsPathRooted(pickupDirLocation))
            {
                client.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;
                client.PickupDirectoryLocation = pickupDirLocation;
                client.EnableSsl = false;
            }
        }

        #endregion
    }
}
