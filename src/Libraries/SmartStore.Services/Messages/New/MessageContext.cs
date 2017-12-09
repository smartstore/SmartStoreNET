using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

		public Customer Customer { get; set; }

		public int? LanguageId { get; set; }
		public int? StoreId { get; set; }
		internal Language Language { get; set; }
		internal Store Store { get; set; }

		internal EmailAccount EmailAccount { get; set; }
		//public EmailAddress ToAddress { get; set; }
		//public EmailAddress ReplyAddress { get; set; }

		/// <summary>
		/// Gets or sets a value specifying whether customer's email should be used as reply address
		/// </summary>
		/// <remarks>Value is ignored, if <c>Customer</c> property is <c>null</c></remarks>
		public bool ReplyToCustomer { get; set; } // TBD: (mc) Liquid > obsolete now 'cause of MessageTemplate.ReplyTo (?)

		public bool TestMode { get; set; }

		public Uri BaseUri { get; set; }

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
