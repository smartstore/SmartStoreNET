using System;

namespace SmartStore.Admin.Models.Messages
{
	[Serializable]
	public class MessageTemplatePreviewModel
	{
		public int EmailAccountId { get; set; }
		public string Token { get; set; }
		public string Error { get; set; }
		public string AccountEmail { get; set; }

		public string From { get; set; }
		public string To { get; set; }
		public string Bcc { get; set; }
		public string ReplyTo { get; set; }
		public string Subject { get; set; }
		public string Body { get; set; }

		public string BodyUrl { get; set; }
	}
}