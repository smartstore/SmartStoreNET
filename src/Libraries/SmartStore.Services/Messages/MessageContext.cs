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
	/// <summary>
	/// A context object which contains all required and optional information
	/// for the creation of message templates.
	/// </summary>
	public class MessageContext
	{
		private IFormatProvider _formatProvider;

		/// <summary>
		/// The source message template. Required if <see cref="MessageTemplateName"/> is empty.
		/// </summary>
		public MessageTemplate MessageTemplate { get; set; }

		/// <summary>
		/// The source message template name. Required if <see cref="MessageTemplate"/> is null.
		/// </summary>
		public string MessageTemplateName { get; set; }

		/// <summary>
		/// If <c>null</c>, the email account specifies the sender.
		/// </summary>
		public EmailAddress SenderEmailAddress { get; set; }

		/// <summary>
		/// If <c>null</c>, obtained from WorkContext.CurrentCustomer.
		/// </summary>
		public Customer Customer { get; set; }

		/// <summary>
		/// If <c>null</c>, obtained from WorkContext.WorkingLanguage.
		/// </summary>
		public int? LanguageId { get; set; }

		/// <summary>
		/// If <c>null</c>, obtained from StoreContext.CurrentStore.
		/// </summary>
		public int? StoreId { get; set; }

		internal Language Language { get; set; }
		internal Store Store { get; set; }
		public EmailAccount EmailAccount { get; internal set; }

		public bool TestMode { get; set; }

		/// <summary>
		/// If <c>null</c>, obtained from <see cref="SmartStore.Services.Stores.IStoreService.GetHost(Store, bool?)"/>.
		/// </summary>
		public Uri BaseUri { get; set; }

		/// <summary>
		/// The final template model containing all global and template specific model parts.
		/// </summary>
		public TemplateModel Model { get; set; }

		/// <summary>
		/// If <c>null</c>, inferred from <see cref="LanguageId"/>.
		/// </summary>
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
