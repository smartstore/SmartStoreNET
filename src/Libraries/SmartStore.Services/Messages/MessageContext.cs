using System;
using System.Collections.Generic;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Messages;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Email;

namespace SmartStore.Services.Messages
{
	public class MessageContext
	{
		public MessageTemplate MessageTemplate { get; set; }
		public string MessageTemplateName { get; set; }

		/// <summary>
		/// If <c>null</c>, the email account specifies the sender.
		/// </summary>
		public EmailAddress SenderEmailAddress { get; set; }
		public Customer Customer { get; set; }

		public int? LanguageId { get; set; }
		public int? StoreId { get; set; }

		internal Language Language { get; set; }
		internal Store Store { get; set; }
		internal EmailAccount EmailAccount { get; set; }

		public bool TestMode { get; set; }

		public Uri BaseUri { get; set; }

		public TemplateModel Model { get; set; }
		public IFormatProvider FormatProvider { get; set; }

		public static MessageContext Create(string messageTemplateName, int languageId, int? storeId = null, Customer customer = null)
		{
			return new MessageContext
			{
				MessageTemplateName = messageTemplateName,
				LanguageId = languageId,
				StoreId = storeId,
				Customer = customer
			};
		}
	}
}
