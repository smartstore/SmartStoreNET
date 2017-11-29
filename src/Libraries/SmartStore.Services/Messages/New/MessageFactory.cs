using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Messages;
using SmartStore.Core.Localization;
using SmartStore.Core.Logging;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
using SmartStore.Templating;

namespace SmartStore.Services.Messages
{
	public partial class MessageFactory
	{
		private readonly ICommonServices _services;
		private readonly ITemplateManager _templateManager;
		private readonly IMessageTemplateService _messageTemplateService;
		private readonly ILanguageService _languageService;
		private readonly IEmailAccountService _emailAccountService;
		private readonly EmailAccountSettings _emailAccountSettings;
		private readonly HttpRequestBase _httpRequest;
		private readonly IDownloadService _downloadService;

		public MessageFactory(
			ICommonServices services, 
			ITemplateManager templateManager,
			IMessageTemplateService messageTemplateService,
			ILanguageService languageService,
			IEmailAccountService emailAccountService,
			EmailAccountSettings emailAccountSettings,
			IDownloadService downloadService,
			HttpRequestBase httpRequest)
		{
			_services = services;
			_templateManager = templateManager;
			_messageTemplateService = messageTemplateService;
			_languageService = languageService;
			_emailAccountService = emailAccountService;
			_emailAccountSettings = emailAccountSettings;
			_downloadService = downloadService;
			_httpRequest = httpRequest;

			T = NullLocalizer.Instance;
			Logger = NullLogger.Instance;
		}

		public Localizer T { get; set; }
		public ILogger Logger { get; set; }

		public QueuedEmail CreateMessage(MessageContext context, bool queue, params object[] modelParts)
		{
			Guard.NotNull(context, nameof(context));

			ValidateMessageContext(context);

			// TODO: (mc) Liquid > add models (Store, Contact, Bank, Customer, EmailAccount etc. > all globals)
			// TODO: (mc) Liquid > Handle ctx.ReplyToCustomer, but how?

			return null;
		}

		private void ValidateMessageContext(MessageContext ctx)
		{
			if (!ctx.StoreId.HasValue)
			{
				ctx.Store = _services.StoreContext.CurrentStore;
				ctx.StoreId = ctx.Store.Id;
			}
			else
			{
				ctx.Store = _services.StoreService.GetStoreById(ctx.StoreId.Value);
			}

			if (!ctx.LanguageId.HasValue)
			{
				ctx.Language = _services.WorkContext.WorkingLanguage;
				ctx.LanguageId = ctx.Language.Id;
			}
			else
			{
				ctx.Language = _languageService.GetLanguageById(ctx.LanguageId.Value);
			}

			EnsureLanguageIsActive(ctx);

			if (ctx.Customer == null)
			{
				ctx.Customer = _services.WorkContext.CurrentCustomer;
			}

			if (ctx.Customer.IsSystemAccount)
			{
				throw new ArgumentException("Cannot create messages for customer system accounts.", nameof(ctx));
			}

			if (ctx.MessageTemplate == null)
			{
				if (ctx.MessageTemplateName.IsEmpty())
				{
					throw new ArgumentException("'MessageTemplateName' must not be empty if 'MessageTemplate' is null.", nameof(ctx));
				}

				ctx.MessageTemplate = GetActiveMessageTemplate(ctx.MessageTemplateName, ctx.StoreId.Value);
			}

			ctx.EmailAccount = GetEmailAccountOfMessageTemplate(ctx.MessageTemplate, ctx.LanguageId.Value);
		}

		protected MessageTemplate GetActiveMessageTemplate(string messageTemplateName, int storeId)
		{
			var messageTemplate = _messageTemplateService.GetMessageTemplateByName(messageTemplateName, storeId);
			if (messageTemplate == null || !messageTemplate.IsActive)
				return null;

			return messageTemplate;
		}

		protected EmailAccount GetEmailAccountOfMessageTemplate(MessageTemplate messageTemplate, int languageId)
		{
			var accountId = messageTemplate.GetLocalized(x => x.EmailAccountId, languageId);
			var account = _emailAccountService.GetEmailAccountById(accountId);
			return account ?? _emailAccountService.GetDefaultEmailAccount();
		}

		private void EnsureLanguageIsActive(MessageContext ctx)
		{
			var language = ctx.Language;

			if (language == null || !language.Published)
			{
				// Load any language from the specified store
				language = _languageService.GetLanguageById(_languageService.GetDefaultLanguageId(ctx.StoreId.Value));
			}

			if (language == null || !language.Published)
			{
				// Load any language
				language = _languageService.GetAllLanguages().FirstOrDefault();
			}

			ctx.Language = language ?? throw new SmartException(T("Common.Error.NoActiveLanguage"));
		}
	}
}
