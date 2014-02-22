//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Net.Mail;
//using System.Net;
//using System.Threading.Tasks;

//namespace SmartStore.Core.Email
//{
//	public static class IEmailSenderExtensions
//	{

//		public static bool SendEmail(this IEmailSender sender, string from, string to, string subject)
//		{
//			EmailMessage emailMessage = new EmailMessage();
//			EmailAddress toEmailAddress = new EmailAddress(to);
//			emailMessage.To.Add(toEmailAddress);
//			emailMessage.Subject = subject;

//			return sender.SendEmail(new SmtpContext(), emailMessage);
//		}

//		public static bool SendEmail(this IEmailSender sender, string from, string to, string subject, string message)
//		{
//			EmailMessage emailMessage = new EmailMessage();
//			EmailAddress toEmailAddress = new EmailAddress(to);
//			emailMessage.To.Add(toEmailAddress);
//			emailMessage.Subject = subject;
//			emailMessage.Body = message;

//			return sender.SendEmail(new SmtpContext(), emailMessage);
//		}

//	}
//}
