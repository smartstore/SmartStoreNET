using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using Newtonsoft.Json;
using SmartStore.ComponentModel;
using SmartStore.Core;
using SmartStore.Core.Domain.Blogs;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Forums;
using SmartStore.Core.Domain.Messages;
using SmartStore.Core.Domain.News;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Email;
using SmartStore.Core.Localization;
using SmartStore.Core.Logging;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
using SmartStore.Templating;

namespace SmartStore.Services.Messages
{
	public partial class MessageFactory : IMessageFactory
	{
		const string LoremIpsum = "Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua.";

		private readonly ICommonServices _services;
		private readonly ITemplateEngine _templateEngine;
		private readonly ITemplateManager _templateManager;
		private readonly IMessageModelProvider _modelProvider;
		private readonly IMessageTemplateService _messageTemplateService;
		private readonly IQueuedEmailService _queuedEmailService;
		private readonly ILanguageService _languageService;
		private readonly IEmailAccountService _emailAccountService;
		private readonly EmailAccountSettings _emailAccountSettings;
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
			IDownloadService downloadService)
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
			}

			ValidateMessageContext(messageContext, ref modelParts);

			// Create and assign model
			var model = messageContext.Model = new TemplateModel();

			// Do not create message if the template does not exist, is not authorized or not active.
			if (messageContext.MessageTemplate == null)
			{
				return new CreateMessageResult { Model = model, MessageContext = messageContext };
			}

			// Add all global template model parts
			_modelProvider.AddGlobalModelParts(messageContext);

			// Add specific template models for passed parts
			foreach (var part in modelParts)
			{
				if (model != null)
				{
					_modelProvider.AddModelPart(part, messageContext);
				}
			}

			// Give implementors the chance to customize the final template model
			_services.EventPublisher.Publish(new MessageModelCreatedEvent(messageContext, model));

			var messageTemplate = messageContext.MessageTemplate;
			var languageId = messageContext.Language.Id;

			// Render templates
			var to = RenderEmailAddress(messageTemplate.To, messageContext);
			var replyTo = RenderEmailAddress(messageTemplate.ReplyTo, messageContext, false);
			var bcc = RenderTemplate(messageTemplate.GetLocalized((x) => x.BccEmailAddresses, languageId), messageContext, false);

			var subject = RenderTemplate(messageTemplate.GetLocalized((x) => x.Subject, languageId), messageContext);
			((dynamic)model).Email.Subject = subject;

			var body = RenderBodyTemplate(messageContext);

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
				From = messageContext.SenderEmailAddress ?? messageContext.EmailAccount.ToEmailAddress(),
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
				QueueMessage(messageContext, qe);
			}

			return new CreateMessageResult { Email = qe, Model = model, MessageContext = messageContext };
		}

		public virtual void QueueMessage(MessageContext messageContext, QueuedEmail queuedEmail)
		{
			Guard.NotNull(messageContext, nameof(messageContext));
			Guard.NotNull(queuedEmail, nameof(queuedEmail));

			// Publish event so that integrators can add attachments, alter the email etc.
			_services.EventPublisher.Publish(new MessageQueuingEvent
			{
				QueuedEmail = queuedEmail,
				MessageContext = messageContext,
				MessageModel = messageContext.Model
			});

			_queuedEmailService.InsertQueuedEmail(queuedEmail);
		}

		private EmailAddress RenderEmailAddress(string email, MessageContext ctx, bool required = true)
		{
			string parsed = null;

			try
			{
				parsed = RenderTemplate(email, ctx, required);

				if (required || parsed != null)
				{
					return parsed.Convert<EmailAddress>();
				}
				else
				{
					return null;
				}
			}
			catch (Exception ex)
			{
				if (ctx.TestMode)
				{
					return new EmailAddress("john@doe.com", "John Doe");
				}

				var ex2 = new SmartException($"Failed to parse email address for variable '{email}'. Value was '{parsed.EmptyNull()}': {ex.Message}", ex);
				_services.Notifier.Error(ex2.Message);
				throw ex2;
			}
		}

		private string RenderTemplate(string template, MessageContext ctx, bool required = true)
		{
			if (!required && template.IsEmpty())
			{
				return null;
			}			

			return _templateEngine.Render(template, ctx.Model, ctx.FormatProvider);
		}

		private string RenderBodyTemplate(MessageContext ctx)
		{
			var key = BuildTemplateKey(ctx);
			var source = ctx.MessageTemplate.GetLocalized((x) => x.Body, ctx.Language);
			var fromCache = true;
			var template = _templateManager.GetOrAdd(key, GetBodyTemplate);	

			if (fromCache && template.Source != source)
			{
				// The template was resolved from template cache, but it has expired
				// because the source text has changed.
				template = _templateEngine.Compile(source);
				_templateManager.Put(key, template);
			}

			return template.Render(ctx.Model, ctx.FormatProvider);

			string GetBodyTemplate()
			{
				fromCache = false;
				return source;
			}
		}

		private string BuildTemplateKey(MessageContext messageContext)
		{
			var prefix = messageContext.MessageTemplate.IsTransientRecord() ? "TransientTemplate/" : "MessageTemplate/";
			return prefix + messageContext.MessageTemplate.Name + "/" + messageContext.Language.Id + "/Body";
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
			var t = ctx.MessageTemplate;
			if (t != null)
			{
				if (t.To.IsEmpty() || t.Subject.IsEmpty() || t.Name.IsEmpty())
				{
					throw new InvalidOperationException("Message template validation failed, because at least one of the following properties has not been set: Name, To, Subject.");
				}
			}

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

			var parts = modelParts?.AsEnumerable() ?? Enumerable.Empty<object>();

			if (ctx.Customer == null)
			{
				// Try to move Customer from parts to MessageContext
				var customer = parts.OfType<Customer>().FirstOrDefault();
				if (customer != null)
				{
					// Exclude the found customer from parts list
					parts = parts.Where(x => !object.ReferenceEquals(x, customer));
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

				ctx.MessageTemplate = _messageTemplateService.GetMessageTemplateByName(ctx.MessageTemplateName, ctx.Store.Id);

				if (ctx.MessageTemplate != null && !ctx.TestMode && !ctx.MessageTemplate.IsActive)
				{
					ctx.MessageTemplate = null;
				}
			}

			if (ctx.EmailAccount == null && ctx.MessageTemplate != null)
			{
				ctx.EmailAccount = GetEmailAccountOfMessageTemplate(ctx.MessageTemplate, ctx.Language.Id);
			}			

			// Sort parts: "IModelPart" instances must come first
			var bagParts = parts.OfType<IModelPart>();
			if (bagParts.Any())
			{
				parts = bagParts.Concat(parts.Except(bagParts));
			}		

			modelParts = parts.ToArray();
		}

		protected EmailAccount GetEmailAccountOfMessageTemplate(MessageTemplate messageTemplate, int languageId)
		{
			var accountId = messageTemplate.GetLocalized(x => x.EmailAccountId, languageId);
			var account = _emailAccountService.GetEmailAccountById(accountId);

			if (account == null)
			{
				account = _emailAccountService.GetDefaultEmailAccount();
			}

			if (account == null)
			{
				throw new SmartException(T("Common.Error.NoEmailAccount"));
			}

			return account;
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
			var templateName = (messageContext.MessageTemplate?.Name ?? messageContext.MessageTemplateName);

			var factories = new Dictionary<string, Func<object>>(StringComparer.OrdinalIgnoreCase)
			{
				{ "BlogComment", () => GetRandomEntity<BlogComment>(x => true) },
				{ "Product", () => GetRandomEntity<Product>(x => !x.Deleted && !x.IsSystemProduct && x.VisibleIndividually && x.Published) },
				{ "Customer", () => GetRandomEntity<Customer>(x => !x.Deleted && !x.IsSystemAccount && !string.IsNullOrEmpty(x.Email)) },
				{ "Order", () => GetRandomEntity<Order>(x => !x.Deleted) },
				{ "Shipment", () => GetRandomEntity<Shipment>(x => !x.Order.Deleted) },
				{ "OrderNote", () => GetRandomEntity<OrderNote>(x => !x.Order.Deleted) },
				{ "RecurringPayment", () => GetRandomEntity<RecurringPayment>(x => !x.Deleted) },
				{ "NewsLetterSubscription", () => GetRandomEntity<NewsLetterSubscription>(x => true) },
				{ "Campaign", () => GetRandomEntity<Campaign>(x => true) },
				{ "ReturnRequest", () => GetRandomEntity<ReturnRequest>(x => true) },
				{ "OrderItem", () => GetRandomEntity<OrderItem>(x => !x.Order.Deleted) },
				{ "ForumTopic", () => GetRandomEntity<ForumTopic>(x => true) },
				{ "ForumPost", () => GetRandomEntity<ForumPost>(x => true) },
				{ "PrivateMessage", () => GetRandomEntity<PrivateMessage>(x => true) },
				{ "GiftCard", () => GetRandomEntity<GiftCard>(x => true) },
				{ "ProductReview", () => GetRandomEntity<ProductReview>(x => !x.Product.Deleted && !x.Product.IsSystemProduct && x.Product.VisibleIndividually && x.Product.Published) },
				{ "NewsComment", () => GetRandomEntity<NewsComment>(x => x.NewsItem.Published) },
				{ "WalletHistory", () => GetRandomEntity<WalletHistory>(x => true) }
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

			// Some models are special
			var isTransientTemplate = messageContext.MessageTemplate != null && messageContext.MessageTemplate.IsTransientRecord();

			if (!isTransientTemplate)
			{
				switch (templateName)
				{
					case MessageTemplateNames.SystemContactUs:
						result.Add(new NamedModelPart("Message")
						{
							["Subject"] = "Test subject",
							["Message"] = LoremIpsum,
							["SenderEmail"] = "jane@doe.com",
							["SenderName"] = "Jane Doe"
						});
						break;
					case MessageTemplateNames.ProductQuestion:
						result.Add(new NamedModelPart("Message")
						{
							["Message"] = LoremIpsum,
							["SenderEmail"] = "jane@doe.com",
							["SenderName"] = "Jane Doe",
							["SenderPhone"] = "123456789"
						});
						break;
					case MessageTemplateNames.ShareProduct:
						result.Add(new NamedModelPart("Message")
						{
							["Body"] = LoremIpsum,
							["From"] = "jane@doe.com",
							["To"] = "john@doe.com",
						});
						break;
					case MessageTemplateNames.ShareWishlist:
						result.Add(new NamedModelPart("Wishlist")
						{
							["PersonalMessage"] = LoremIpsum,
							["From"] = "jane@doe.com",
							["To"] = "john@doe.com",
						});
						break;
					case MessageTemplateNames.NewVatSubmittedStoreOwner:
						result.Add(new NamedModelPart("VatValidationResult")
						{
							["Name"] = "VatName",
							["Address"] = "VatAddress"
						});
						break;
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

						if (parentModel is ITestModel)
						{
							// When the parent model is a test model, we need to create a random instance
							// instead of using the property value (which is null/void in this case)
							currentModel = factories.Get(propName)?.Invoke();
						}
						else
						{
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
					}

					if (currentModel == null)
						break;

					// Put it in dict as e.g. "Order.Customer"
					models[token] = currentModel;
				}

				if (!bof)
				{
					dotIndex = i;
				}
			}

			return currentModel;
		}

		private object GetRandomEntity<T>(Expression<Func<T, bool>> predicate) where T : BaseEntity, new()
		{
			var dbSet = _services.DbContext.Set<T>();

			var query = dbSet.Where(predicate);

			// Determine how many entities match the given predicate
			var count = query.Count();

			object result;

			if (count > 0)
			{
				// Fetch a random one
				var skip = new Random().Next(count);
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
