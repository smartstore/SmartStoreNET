using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Mail;
using System.Net.Mime;
using System.Xml.Serialization;
using System.Linq;
using System.IO;
using System.Text;
using System.Xml;
using System.Web;
using System.Net;
using System.Threading.Tasks;

namespace SmartStore.Core.Email
{

    public class EmailMessage : ICloneable<EmailMessage>
    {

		public EmailMessage()
		{
			this.BodyFormat = MailBodyFormat.Html;
			this.Priority = MailPriority.Normal;

			this.To = new List<EmailAddress>();
			this.Cc = new List<EmailAddress>();
			this.Bcc = new List<EmailAddress>();
			this.ReplyTo = new List<EmailAddress>();

			this.Attachments = new List<Attachment>();

			this.Headers = new NameValueCollection();
		}

		public EmailMessage(string to, string subject, string body, string from) 
			: this()
		{
			Guard.ArgumentNotEmpty(() => to);
			Guard.ArgumentNotEmpty(() => from);
			Guard.ArgumentNotEmpty(() => subject);
			Guard.ArgumentNotEmpty(() => body);
			
			this.To.Add(new EmailAddress(to));
			this.Subject = subject;
			this.Body = body;
			this.From = new EmailAddress(from);
		}

		public EmailMessage(EmailAddress to, string subject, string body, EmailAddress from)
			: this()
		{
			Guard.ArgumentNotNull(() => to);
			Guard.ArgumentNotNull(() => from);
			Guard.ArgumentNotEmpty(() => subject);
			Guard.ArgumentNotEmpty(() => body);
			
			this.To.Add(to);
			this.Subject = subject;
			this.Body = body;
			this.From = from;
		}
		
		public EmailAddress From { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string AltText { get; set; }

		public MailBodyFormat BodyFormat { get; set; }
        public MailPriority Priority { get; set; }

        public ICollection<EmailAddress> To { get; private set; }
		public ICollection<EmailAddress> Cc { get; private set; }
		public ICollection<EmailAddress> Bcc { get; private set; }
		public ICollection<EmailAddress> ReplyTo { get; private set; }

		public ICollection<Attachment> Attachments { get; private set; }

        public NameValueCollection Headers { get; private set; }

        public async void BodyFromFile(string filePathOrUrl)
        {
            StreamReader sr = null;

            if (filePathOrUrl.ToLower().StartsWith("http"))
            {
                WebClient wc = new WebClient();
                sr = new StreamReader(await wc.OpenReadTaskAsync(filePathOrUrl));
            }
            else
            {
                sr = new StreamReader(filePathOrUrl, Encoding.Default);
            }

            this.Body = await sr.ReadToEndAsync();

            sr.Close();
        }

        #region ICloneable Members

        public EmailMessage Clone()
        {
            var clone = new EmailMessage();

			clone.Attachments.AddRange(this.Attachments);
			clone.To.AddRange(this.To);
			clone.Cc.AddRange(this.Cc);
			clone.Bcc.AddRange(this.Bcc);
			clone.ReplyTo.AddRange(this.ReplyTo);
			clone.Headers.AddRange(this.Headers);

			clone.AltText = this.AltText;
			clone.Body = this.Body;
			clone.BodyFormat = this.BodyFormat;
			clone.From = this.From;
			clone.Priority = this.Priority;
			clone.Subject = this.Subject;

            return clone;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        #endregion
    }

	public enum MailBodyFormat
	{
		Text,
		Html
	}
}