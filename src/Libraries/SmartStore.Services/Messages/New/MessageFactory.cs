using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Globalization;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Messages;
using SmartStore.Core.Localization;
using SmartStore.Core.Logging;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
using SmartStore.Templating;
using SmartStore.Core.Email;
using SmartStore.Core;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Domain.Customers;

namespace SmartStore.Services.Messages
{
	public partial class MessageFactory : IMessageFactory
	{
		private readonly ICommonServices _services;
		private readonly ITemplateEngine _templateEngine;
		private readonly ITemplateManager _templateManager;
		private readonly IMessageModelProvider _modelProvider;
		private readonly IMessageTemplateService _messageTemplateService;
		private readonly IQueuedEmailService _queuedEmailService;
		private readonly ILanguageService _languageService;
		private readonly IEmailAccountService _emailAccountService;
		private readonly EmailAccountSettings _emailAccountSettings;
		private readonly HttpRequestBase _httpRequest;
		private readonly IDownloadService _downloadService;

		public MessageFactory(
			ICommonServices services,
			ITemplateEngine templateEngine,
			ITemplateManager templateManager,
			IMessageModelProvider modelProvider,
			IMessageTemplateService messageTemplateService,
			IQueuedEmailService queuedEmailService,
			ILanguageService languageService,
			IEmailAccountService emailAccountService,
			EmailAccountSettings emailAccountSettings,
			IDownloadService downloadService,
			HttpRequestBase httpRequest)
		{
			_services = services;
			_templateEngine = templateEngine;
			_templateManager = templateManager;
			_modelProvider = modelProvider;
			_messageTemplateService = messageTemplateService;
			_queuedEmailService = queuedEmailService;
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

		public virtual (QueuedEmail Email, dynamic Model) CreateMessage(MessageContext messageContext, bool queue, params object[] modelParts)
		{
			Guard.NotNull(messageContext, nameof(messageContext));

			ValidateMessageContext(messageContext, ref modelParts);

			modelParts = modelParts ?? new object[0];

			var model = new ExpandoObject() as IDictionary<string, object>;

			// Add all global template model parts
			_modelProvider.AddGlobalModelParts(messageContext, model);

			// Handle TestMode
			if (messageContext.TestMode && modelParts.Length == 0)
			{
				modelParts = GetTestEntities(messageContext).ToArray();
			}

			// Add specific template models for passed parts
			foreach (var part in modelParts)
			{
				_modelProvider.AddModelPart(part, messageContext, model);
			}

			// Give implementors the chance to customize the final template model
			_services.EventPublisher.Publish(new MessageModelCreatedEvent(messageContext, (dynamic)model));

			// TODO: (mc) Liquid > Handle ctx.ReplyToCustomer, but how?

			// Get format provider for requested language
			var formatProvider = GetFormatProvider(messageContext);

			var messageTemplate = messageContext.MessageTemplate;
			var languageId = messageContext.Language.Id;

			// Render templates
			var to = RenderTemplate("John Doe <john@doe.com>", model, formatProvider).Convert<EmailAddress>(); // TODO: (mc) Liquid > Make MessageTemplate field
			var bcc = RenderTemplate(messageTemplate.GetLocalized((x) => x.BccEmailAddresses, languageId), model, formatProvider, false);
			var replyTo = RenderTemplate("He Man <he@man.com>", model, formatProvider, false)?.Convert<EmailAddress>(); // TODO: (mc) Liquid > Make MessageTemplate field

			var subject = RenderTemplate(messageTemplate.GetLocalized((x) => x.Subject, languageId), model, formatProvider);
			((dynamic)model).Email.Subject = subject;

			var body = RenderBodyTemplate(messageContext, model, formatProvider);

			// CSS inliner
			body = InlineCss(body, model);

			// Create queued email from template
			var qe = new QueuedEmail
			{
				Priority = 5,
				From = messageContext.EmailAccount.Email,
				FromName = messageContext.EmailAccount.DisplayName,
				To = to.Address,
				ToName = to.DisplayName, // TODO: (mc) Liquid > Combine both Address & DisplayName fields (?) 
				Bcc = bcc,
				ReplyTo = replyTo?.Address,
				ReplyToName = replyTo?.DisplayName,
				Subject = subject,
				Body = body,
				CreatedOnUtc = DateTime.UtcNow,
				EmailAccountId = messageContext.EmailAccount.Id,
				SendManually = messageTemplate.SendManually
			};

			// Create and add attachments (if any)
			CreateAttachments(qe, messageContext);

			if (queue)
			{
				// Put to queue
				QueueMessage(qe, messageContext, (dynamic)model);
			}

			return (qe, (dynamic)model);
		}

		public virtual void QueueMessage(QueuedEmail queuedEmail, MessageContext messageContext, dynamic model)
		{
			Guard.NotNull(queuedEmail, nameof(queuedEmail));
			Guard.NotNull(messageContext, nameof(messageContext));
			Guard.NotNull(model, nameof(model));

			// Publish event so that integrators can add attachments, alter the email etc.
			_services.EventPublisher.Publish(new MessageQueuingEvent
			{
				QueuedEmail = queuedEmail,
				MessageContext = messageContext,
				MessageModel = model
			});

			_queuedEmailService.InsertQueuedEmail(queuedEmail);
		}

		public virtual IEnumerable<BaseEntity> GetTestEntities(MessageContext messageContext)
		{
			// TODO: (mc) Liquid > implement!
			yield return new Product();
			yield return new Order();
			yield return new Shipment();
			yield return new OrderNote();
			yield return new RecurringPayment();
			yield return new GiftCard();
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

		private string RenderTemplate(string template, dynamic model, IFormatProvider formatProvider, bool required = true)
		{
			if (!required && template.IsEmpty())
			{
				return null;
			}			

			return _templateEngine.Render(template, model, formatProvider);
		}

		private string RenderBodyTemplate(MessageContext messageContext, dynamic model, IFormatProvider formatProvider)
		{
			var key = BuildTemplateKey(messageContext);
			var template = _templateManager.GetOrAdd(key, GetBodyTemplate);

			if (true) // TODO: (mc) Liquid > Check TimeSpan and invalidate
			{
				template = _templateEngine.Compile(GetBodyTemplate());
				_templateManager.Put(key, template);
			}

			return template.Render(model, formatProvider);

			string GetBodyTemplate()
			{
				return messageContext.MessageTemplate.GetLocalized((x) => x.Body, messageContext.Language.Id);
			}
		}

		private string BuildTemplateKey(MessageContext messageContext)
		{
			return "MessageTemplate/" + messageContext.MessageTemplate.Name + "/" + messageContext.Language.Id + "/Body";
		}

		private string InlineCss(string html, dynamic model)
		{
			Uri baseUri = null;

			try
			{
				// 'Store' is a global model part, so we pretty can be sure it exists
				baseUri = new Uri((string)model.Store.Url);
			}
			catch { }

			var pm = new PreMailer.Net.PreMailer(html, baseUri);
			var result = pm.MoveCssInline(true, "#ignore");
			return result.Html;
		}

		protected virtual void CreateAttachments(QueuedEmail queuedEmail, MessageContext messageContext)
		{
			var messageTemplate = messageContext.MessageTemplate;
			var languageId = messageContext.Language.Id;

			// create attachments if any
			var fileIds = (new int?[]
				{
					messageTemplate.GetLocalized(x => x.Attachment1FileId, languageId),
					messageTemplate.GetLocalized(x => x.Attachment2FileId, languageId),
					messageTemplate.GetLocalized(x => x.Attachment3FileId, languageId)
				})
				.Where(x => x.HasValue)
				.Select(x => x.Value)
				.ToArray();

			if (fileIds.Any())
			{
				var files = _downloadService.GetDownloadsByIds(fileIds);
				foreach (var file in files)
				{
					queuedEmail.Attachments.Add(new QueuedEmailAttachment
					{
						StorageLocation = EmailAttachmentStorageLocation.FileReference,
						FileId = file.Id,
						Name = (file.Filename.NullEmpty() ?? file.Id.ToString()) + file.Extension.EmptyNull(),
						MimeType = file.ContentType.NullEmpty() ?? "application/octet-stream"
					});
				}
			}
		}


		private void ValidateMessageContext(MessageContext ctx, ref object[] modelParts)
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
				Customer customer = null;
				if (modelParts != null)
				{
					customer = modelParts.OfType<Customer>().FirstOrDefault();
					if (customer != null)
					{
						modelParts = modelParts.Where(x => !object.ReferenceEquals(x, customer)).ToArray();
					}
				}

				ctx.Customer = customer ?? _services.WorkContext.CurrentCustomer;
			}

			if (ctx.Customer.IsSystemAccount)
			{
				throw new ArgumentException("Cannot create messages for system customer accounts.", nameof(ctx));
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
