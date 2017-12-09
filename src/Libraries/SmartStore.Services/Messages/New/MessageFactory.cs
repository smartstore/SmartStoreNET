using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Globalization;
using System.Data.Entity;
using Newtonsoft.Json;
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
using SmartStore.Core.Domain.Blogs;
using SmartStore.Core.Domain.Forums;
using SmartStore.Core.Domain.News;
using SmartStore.ComponentModel;

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

		public virtual CreateMessageResult CreateMessage(MessageContext messageContext, bool queue, params object[] modelParts)
		{
			Guard.NotNull(messageContext, nameof(messageContext));

			modelParts = modelParts ?? new object[0];

			// Handle TestMode
			if (messageContext.TestMode && modelParts.Length == 0)
			{
				modelParts = GetTestModels(messageContext);
				//var testCustomer = modelParts.OfType<Customer>().FirstOrDefault();
				//if (testCustomer != null)
				//{
				//	messageContext.Customer = testCustomer;
				//}
			}

			ValidateMessageContext(messageContext, ref modelParts);

			var model = new ExpandoObject() as IDictionary<string, object>;

			// Add all global template model parts
			_modelProvider.AddGlobalModelParts(messageContext, model);

			// Add specific template models for passed parts
			foreach (var part in modelParts)
			{
				if (model != null)
				{
					_modelProvider.AddModelPart(part, messageContext, model);
				}
			}

			// Give implementors the chance to customize the final template model
			_services.EventPublisher.Publish(new MessageModelCreatedEvent(messageContext, (dynamic)model));

			// TODO: (mc) Liquid > Handle ctx.ReplyToCustomer, but how?

			// Get format provider for requested language
			var formatProvider = GetFormatProvider(messageContext);

			var messageTemplate = messageContext.MessageTemplate;
			var languageId = messageContext.Language.Id;

			// Render templates
			var to = RenderTemplate(messageTemplate.To, model, formatProvider).Convert<EmailAddress>();
			var bcc = RenderTemplate(messageTemplate.GetLocalized((x) => x.BccEmailAddresses, languageId), model, formatProvider, false);
			var replyTo = RenderTemplate(messageTemplate.ReplyTo, model, formatProvider, false)?.Convert<EmailAddress>();

			var subject = RenderTemplate(messageTemplate.GetLocalized((x) => x.Subject, languageId), model, formatProvider);
			((dynamic)model).Email.Subject = subject;

			var body = RenderBodyTemplate(messageContext, model, formatProvider);

			// CSS inliner
			body = InlineCss(body, model);

			// Model tree
			var modelTree = _modelProvider.BuildModelTree(model);
			var modelTreeJson = JsonConvert.SerializeObject(modelTree, Formatting.None);
			if (modelTreeJson != messageTemplate.LastModelTree)
			{
				messageContext.MessageTemplate.LastModelTree = modelTreeJson;
				if (!messageTemplate.IsTransientRecord())
				{
					_messageTemplateService.UpdateMessageTemplate(messageContext.MessageTemplate);
				}	
			}

			// Create queued email from template
			var qe = new QueuedEmail
			{
				Priority = 5,
				From = messageContext.EmailAccount.ToEmailAddress(),
				To = to.ToString(),
				Bcc = bcc,
				ReplyTo = replyTo?.ToString(),
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
				QueueMessage(messageContext, qe, (dynamic)model);
			}

			return new CreateMessageResult { Email = qe, Model = (dynamic)model };
		}

		public virtual void QueueMessage(MessageContext messageContext, QueuedEmail queuedEmail, dynamic model)
		{
			Guard.NotNull(messageContext, nameof(messageContext));
			Guard.NotNull(queuedEmail, nameof(queuedEmail));
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
			var source = messageContext.MessageTemplate.GetLocalized((x) => x.Body, messageContext.Language.Id);
			var fromCache = true;
			var template = _templateManager.GetOrAdd(key, GetBodyTemplate);	

			if (fromCache && template.Source != source)
			{
				// The template was resolved from template cache, but it has expired
				// because the source text has changed.
				template = _templateEngine.Compile(source);
				_templateManager.Put(key, template);
			}

			return template.Render(model, formatProvider);

			string GetBodyTemplate()
			{
				fromCache = false;
				return source;
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
			if (ctx.StoreId.GetValueOrDefault() == 0)
			{
				ctx.Store = _services.StoreContext.CurrentStore;
				ctx.StoreId = ctx.Store.Id;
			}
			else
			{
				ctx.Store = _services.StoreService.GetStoreById(ctx.StoreId.Value);
			}

			if (ctx.BaseUri == null)
			{
				ctx.BaseUri = new Uri(_services.StoreService.GetHost(ctx.Store));
			}

			if (ctx.LanguageId.GetValueOrDefault() == 0)
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
						// Exclude the found customer from parts list
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

				ctx.MessageTemplate = GetActiveMessageTemplate(ctx.MessageTemplateName, ctx.Store.Id);
			}

			ctx.EmailAccount = GetEmailAccountOfMessageTemplate(ctx.MessageTemplate, ctx.Language.Id);
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

		#region TestModels

		public virtual object[] GetTestModels(MessageContext messageContext)
		{
			var factories = new Dictionary<string, Func<object>>(StringComparer.OrdinalIgnoreCase)
			{
				{ "BlogComment", CreateBlogCommentTestModel },
				{ "Product", CreateProductTestModel },
				{ "Customer", CreateCustomerTestModel },
				{ "Order", CreateOrderTestModel },
				{ "Shipment", CreateShipmentTestModel },
				{ "OrderNote", CreateOrderNoteTestModel },
				{ "RecurringPayment", CreateRecurringPaymentTestModel },
				{ "NewsLetterSubscription", CreateNewsLetterSubscriptionTestModel },
				{ "ReturnRequest", CreateReturnRequestTestModel },
				{ "OrderItem", CreateOrderItemTestModel },
				{ "ForumTopic", CreateForumTopicTestModel },
				{ "ForumPost", CreateForumPostTestModel },
				{ "PrivateMessage", CreatePrivateMessageTestModel },
				{ "GiftCard", CreateGiftCardTestModel },
				{ "ProductReview", CreateProductReviewTestModel },
				{ "NewsComment", CreateNewsCommentTestModel },
				{ "BackInStockSubscription", CreateBackInStockSubscriptionTestModel }
			};

			var modelNames = messageContext.MessageTemplate.ModelTypes
				.SplitSafe(",")
				.Select(x => x.Trim())
				.Distinct()
				.ToArray();

			var models = new Dictionary<string, object>();
			var result = new List<object>();

			foreach (var modelName in modelNames)
			{
				var model = GetModelFromExpression(modelName, models, factories);
				if (model != null)
				{
					result.Add(model);
				}
			}

			return result.ToArray();
		}

		private object GetModelFromExpression(string expression, IDictionary<string, object> models, IDictionary<string, Func<object>> factories)
		{
			object currentModel = null;
			int dotIndex = 0;
			int len = expression.Length;
			bool bof = true;
			string token = null;

			for (var i = 0; i < len; i++)
			{
				if (expression[i] == '.')
				{
					dotIndex = i;
					bof = false;
					token = expression.Substring(0, i);
				}
				else if (i == len - 1)
				{
					// End reached
					token = expression;
				}
				else
				{
					continue;
				}

				if (!models.TryGetValue(token, out currentModel))
				{
					if (bof)
					{
						// It's a simple dot-less expression where the token
						// is actually the model name
						currentModel = factories.Get(token)?.Invoke();
					}
					else
					{
						// Sub-token, e.g. "Order.Customer"
						// Get "Customer" part, this is our property name, NOT the model name
						var propName = token.Substring(dotIndex + 1);
						// Get parent model "Order"
						var parentModel = models.Get(token.Substring(0, dotIndex));
						if (parentModel == null)
							break;

						// Get "Customer" property of Order
						var fastProp = FastProperty.GetProperty(parentModel.GetType(), propName, PropertyCachingStrategy.Uncached);
						if (fastProp != null)
						{
							// Get "Customer" value
							var propValue = fastProp.GetValue(parentModel);
							if (propValue != null)
							{
								currentModel = propValue;
								//// Resolve logical model name...
								//var modelName = _modelProvider.ResolveModelName(propValue);
								//if (modelName != null)
								//{
								//	// ...and create the value
								//	currentModel = factories.Get(modelName)?.Invoke();
								//}
							}
						}
					}

					if (currentModel == null)
						break;

					// Put it in dict as e.g. "Order.Customer"
					models[token] = currentModel;
				}
			}

			return currentModel;
		}

		private object CreateBlogCommentTestModel()
		{
			return GetRandomEntity<BlogComment>(x => true);
		}

		private object CreateProductTestModel()
		{
			return GetRandomEntity<Product>(x => !x.Deleted && x.VisibleIndividually && x.Published);
		}

		private object CreateCustomerTestModel()
		{
			return GetRandomEntity<Customer>(x => !x.Deleted && !x.IsSystemAccount && !string.IsNullOrEmpty(x.Email));
		}

		private object CreateOrderTestModel()
		{
			return GetRandomEntity<Order>(x => !x.Deleted);
		}

		private object CreateShipmentTestModel()
		{
			return GetRandomEntity<Shipment>(x => !x.Order.Deleted);
		}

		private object CreateOrderNoteTestModel()
		{
			return GetRandomEntity<OrderNote>(x => !x.Order.Deleted);
		}

		private object CreateRecurringPaymentTestModel()
		{
			return GetRandomEntity<RecurringPayment>(x => !x.Deleted);
		}

		private object CreateNewsLetterSubscriptionTestModel()
		{
			return GetRandomEntity<NewsLetterSubscription>(x => true);
		}

		private object CreateReturnRequestTestModel()
		{
			return GetRandomEntity<ReturnRequest>(x => true);
		}

		private object CreateOrderItemTestModel()
		{
			return GetRandomEntity<OrderItem>(x => !x.Order.Deleted);
		}

		private object CreateForumTopicTestModel()
		{
			return GetRandomEntity<ForumTopic>(x => true);
		}

		private object CreateForumPostTestModel()
		{
			return GetRandomEntity<ForumPost>(x => true);
		}

		private object CreatePrivateMessageTestModel()
		{
			return GetRandomEntity<PrivateMessage>(x => true);
		}

		private object CreateGiftCardTestModel()
		{
			return GetRandomEntity<GiftCard>(x => true);
		}

		private object CreateProductReviewTestModel()
		{
			return GetRandomEntity<ProductReview>(x => !x.Product.Deleted && x.Product.VisibleIndividually && x.Product.Published);
		}

		private object CreateNewsCommentTestModel()
		{
			return GetRandomEntity<NewsComment>(x => x.NewsItem.Published);
		}

		private object CreateBackInStockSubscriptionTestModel()
		{
			return GetRandomEntity<BackInStockSubscription>(x => !x.Product.Deleted && x.Product.VisibleIndividually && x.Product.Published);
		}

		private object GetRandomEntity<T>(Expression<Func<T, bool>> predicate) where T : BaseEntity, new()
		{
			var dbSet = _services.DbContext.Set<T>().AsNoTracking();

			var query = dbSet.Where(predicate);

			// Determine how many entities match the given predicate
			var count = query.Count();

			object result;

			if (count > 0)
			{
				// Fetch a random one
				var skip = new Random().Next(count - 1);
				result = query.OrderBy(x => x.Id).Skip(skip).FirstOrDefault();
			}
			else
			{
				// No entity macthes the predicate. Provide a fallback test entity
				var entity = Activator.CreateInstance<T>();
				result = _templateEngine.CreateTestModelFor(entity, entity.GetUnproxiedType().Name);
			}

			return result;
		}

		#endregion
	}
}
