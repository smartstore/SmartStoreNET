using System;
using System.Collections.Generic;
using System.Globalization;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Messages;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Email;
using SmartStore.Core.Localization;

namespace SmartStore.Services.Messages
{
	public class MessageContext
	{
		private IFormatProvider _formatProvider;

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

		public IFormatProvider FormatProvider
		{
			get
			{
				if (_formatProvider == null)
				{
					var culture = this.Language?.LanguageCulture;
					if (culture != null && LocalizationHelper.IsValidCultureCode(culture))
					{
						_formatProvider = CultureInfo.GetCultureInfo(culture);
					}
				}

				return _formatProvider ?? CultureInfo.CurrentCulture;
			}
			set
			{
				_formatProvider = value;
			}
		}

		private IFormatProvider GetFormatProvider(MessageContext messageContext)
		{
			var culture = messageContext.Language.LanguageCulture;

			if (LocalizationHelper.IsValidCultureCode(culture))
			{
				return CultureInfo.GetCultureInfo(culture);
			}

			return CultureInfo.CurrentCulture;
		}

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
