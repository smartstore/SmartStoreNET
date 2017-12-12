using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using SmartStore.Collections;
using SmartStore.ComponentModel;
using SmartStore.Core;
using SmartStore.Core.Domain.Blogs;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Forums;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Domain.Messages;
using SmartStore.Core.Domain.News;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Security;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Domain.Tax;
using SmartStore.Core.Html;
using SmartStore.Core.Localization;
using SmartStore.Core.Logging;
using SmartStore.Core.Plugins;
using SmartStore.Services.Catalog;
using SmartStore.Services.Catalog.Extensions;
using SmartStore.Services.Common;
using SmartStore.Services.Customers;
using SmartStore.Services.Localization;
using SmartStore.Services.Orders;
using SmartStore.Services.Payments;
using SmartStore.Services.Topics;
using SmartStore.Services.Forums;
using SmartStore.Services.Seo;
using SmartStore.Templating;
using SmartStore.Services.Directory;
using SmartStore.Services.Media;

namespace SmartStore.Services.Messages
{
	public enum ModelTreeMemberKind
	{
		Primitive,
		Complex,
		Collection,
		Root
	}

	public class ModelTreeMember
	{
		public string Name { get; set; }
		public ModelTreeMemberKind Kind { get; set; }
	}

	public partial class MessageModelProvider : IMessageModelProvider
	{
		private readonly ICommonServices _services;
		private readonly ITemplateEngine _templateEngine;
		private readonly IEmailAccountService _emailAccountService;
		private readonly UrlHelper _urlHelper;

		public MessageModelProvider(
			ICommonServices services,
			ITemplateEngine templateEngine,
			IEmailAccountService emailAccountService,
			UrlHelper urlHelper)
		{
			_services = services;
			_templateEngine = templateEngine;
			_emailAccountService = emailAccountService;
			_urlHelper = urlHelper;

			T = NullLocalizer.InstanceEx;
			Logger = NullLogger.Instance;
		}

		public LocalizerEx T { get; set; }
		public ILogger Logger { get; set; }

		public virtual void AddGlobalModelParts(MessageContext messageContext, IDictionary<string, object> model)
		{
			Guard.NotNull(model, nameof(model));

			model["Context"] = new Dictionary<string, object>
			{
				{ "TemplateName", messageContext.MessageTemplate.Name },
				{ "LanguageId", messageContext.Language.Id },
				{ "LanguageCulture", messageContext.Language.LanguageCulture },
				{ "BaseUrl", messageContext.BaseUri.ToString() }
			};

			dynamic email = new ExpandoObject();
			email.Email = messageContext.EmailAccount.Email;
			email.SenderName = messageContext.EmailAccount.DisplayName;

			model["Email"] = email;
			model["Theme"] = CreateThemeModelPart(messageContext);
			model["Company"] = CreateCompanyModelPart(messageContext);
			model["Contact"] = CreateContactModelPart(messageContext);
			model["Bank"] = CreateBankModelPart(messageContext);

			model["Customer"] = CreateModelPart(messageContext.Customer, messageContext);
			model["Store"] = CreateModelPart(messageContext.Store, messageContext);
		}

		public virtual void AddModelPart(object part, MessageContext messageContext, IDictionary<string, object> model, string name = null)
		{
			Guard.NotNull(part, nameof(part));
			Guard.NotNull(messageContext, nameof(messageContext));
			Guard.NotNull(model, nameof(model));
			
			name = name.NullEmpty() ?? ResolveModelName(part);

			object modelPart = null;

			switch (part)
			{
				case Order x:
					modelPart = CreateModelPart(x, messageContext);
					break;
				case Product x:
					modelPart = CreateModelPart(x, messageContext);
					break;
				case Customer x:
					//modelPart = CreateModelPart(x, messageContext);
					break;
				case Shipment x:
					modelPart = CreateModelPart(x, messageContext);
					break;
				case OrderNote x:
					modelPart = CreateModelPart(x, messageContext);
					break;
				case RecurringPayment x:
					modelPart = CreateModelPart(x, messageContext);
					break;
				case ReturnRequest x:
					modelPart = CreateModelPart(x, messageContext);
					break;
				case GiftCard x:
					modelPart = CreateModelPart(x, messageContext);
					break;
				case NewsLetterSubscription x:
					modelPart = CreateModelPart(x, messageContext);
					break;
				case ProductReview x:
					modelPart = CreateModelPart(x, messageContext);
					break;
				case BlogComment x:
					modelPart = CreateModelPart(x, messageContext);
					break;
				case NewsComment x:
					modelPart = CreateModelPart(x, messageContext);
					break;
				case ForumTopic x:
					modelPart = CreateModelPart(x, messageContext);
					break;
				case ForumPost x:
					modelPart = CreateModelPart(x, messageContext);
					break;
				case Forum x:
					modelPart = CreateModelPart(x, messageContext);
					break;
				case PrivateMessage x:
					modelPart = CreateModelPart(x, messageContext);
					break;
				case BackInStockSubscription x:
					modelPart = CreateModelPart(x, messageContext);
					break;
				default:
					var partType = part.GetType();
					modelPart = part;

					if (!messageContext.TestMode && partType.IsPlainObjectType() && !partType.IsAnonymous())
					{
						var evt = new MessageModelPartMappingEvent(part);
						_services.EventPublisher.Publish(evt);

						if (evt.Result != null && !object.ReferenceEquals(evt.Result, part))
						{
							modelPart = evt.Result;
							if (evt.Name.HasValue())
							{
								name = evt.Name;
							}
							else
							{
								name = ResolveModelName(evt.Result) ?? name;
							}
						}
						else
						{
							modelPart = part;
						}

						modelPart = evt.Result ?? part;
						name = evt.Name.NullEmpty() ?? name;
					}

					break;
			}

			if (modelPart != null)
			{
				if (name.IsEmpty())
				{
					throw new SmartException($"Could not resolve a model key for part '{modelPart.GetType().Name}'. When using dictionaries, anonymous or dynamic objects please specify the key via '__Name' member.");
				}

				if (model.TryGetValue(name, out var existing))
				{
					// A model part with the same name exists in model already...
					if (existing is IDictionary<string, object> x)
					{
						// but it's a dictionary which we can easily merge with
						x.Merge(FastProperty.ObjectToDictionary(modelPart), true);
					}
					else
					{
						// Wrap in HybridExpando and merge
						var he = new HybridExpando(existing);
						he.Merge(FastProperty.ObjectToDictionary(modelPart), true);
						model[name] = he;
					}
				}
				else
				{
					// Put part to model as new property
					model[name] = modelPart;
				}	
			}	
		}

		public string ResolveModelName(object model)
		{
			Guard.NotNull(model, nameof(model));

			string name = null;
			var type = model.GetType();

			try
			{
				if (model is BaseEntity be)
				{
					name = be.GetUnproxiedType().Name;
				}
				else if (model is ITestModel te)
				{
					name = te.ModelName;
				}
				else if (model is IDictionary<string, object> d)
				{
					name = d.Get("__Name") as string;
				}
				else if (model is IDynamicMetaObjectProvider x)
				{
					name = ((dynamic)model).__Name as string;
				}
				else if (type.IsAnonymous())
				{
					var prop = FastProperty.GetProperty(type, "__Name", PropertyCachingStrategy.EagerCached);
					if (prop != null)
					{
						name = prop.GetValue(model) as string;
					}
				}
				else if (type.IsPlainObjectType())
				{
					name = type.Name;
				}
			}
			catch { }

			return name;
		}

		#region Global model part handlers

		protected virtual object CreateThemeModelPart(MessageContext messageContext)
		{
			dynamic model = new ExpandoObject();

			// TODO: (mc) Liquid > make theme variables (?)
			model.FontFamily = "Arial, 'Helvetica Neue', Helvetica, sans-serif";
			model.BodyBg = "#f2f4f6";
			model.BodyColor = "#74787e";
			model.TitleColor = "#2f3133";
			model.ContentBg = "#fff";
			model.ShadeColor = "#edeff2";
			model.LinkColor = "#3869D4";
			model.BrandPrimary = "#3f51b5";
			model.BrandSuccess = "#4caf50";
			model.BrandWarning = "#ff9800";
			model.BrandDanger = "#f44336";
			model.MutedColor = "#9ba2ab";

			return model;
		}

		protected virtual object CreateCompanyModelPart(MessageContext messageContext)
		{
			var settings = _services.Settings.LoadSetting<CompanyInformationSettings>(messageContext.Store.Id);
			var m = new HybridExpando(settings);
			PublishModelPartCreatedEvent<CompanyInformationSettings>(settings, m);
			return m;
		}

		protected virtual object CreateBankModelPart(MessageContext messageContext)
		{
			var settings = _services.Settings.LoadSetting<BankConnectionSettings>(messageContext.Store.Id);
			var m = new HybridExpando(settings);
			PublishModelPartCreatedEvent<BankConnectionSettings>(settings, m);
			return m;
		}

		protected virtual object CreateContactModelPart(MessageContext messageContext)
		{
			var settings = _services.Settings.LoadSetting<ContactDataSettings>(messageContext.Store.Id);
			var contact = new HybridExpando(settings) as dynamic;

			// TODO: (mc) Liquid > Use following aliases in Partials
			// Aliases
			contact.Phone = new
			{
				Company = settings.CompanyTelephoneNumber,
				Hotline = settings.HotlineTelephoneNumber,
				Mobile = settings.MobileTelephoneNumber,
				Fax = settings.CompanyFaxNumber
			};

			contact.Email = new
			{
				Company = settings.CompanyEmailAddress,
				Webmaster = settings.WebmasterEmailAddress,
				Support = settings.SupportEmailAddress,
				Contact = settings.ContactEmailAddress
			};

			PublishModelPartCreatedEvent<ContactDataSettings>(settings, contact);

			return contact;
		}

		#endregion

		#region Entity specific model part handlers

		protected virtual object CreateModelPart(Store part, MessageContext messageContext)
		{
			Guard.NotNull(messageContext, nameof(messageContext));
			Guard.NotNull(part, nameof(part));

			var disallow = new HashSet<string> { nameof(part.PrimaryExchangeRateCurrencyId), nameof(part.PrimaryExchangeRateCurrencyId) };

			var m = new HybridExpando(part, disallow, MemberOptMethod.Disallow);
			m["Email"] = messageContext.EmailAccount.Email;
			m["EmailName"] = messageContext.EmailAccount.DisplayName;

			var host = messageContext.BaseUri.ToString();
			m["URL"] = host;
			m.Override(nameof(part.Url), host);
			m.Override(nameof(part.PrimaryStoreCurrency), part.PrimaryStoreCurrency?.CurrencyCode);
			m.Override(nameof(part.PrimaryExchangeRateCurrency), part.PrimaryExchangeRateCurrency?.CurrencyCode);

			// TODO: (mc) Liquid > Use in templates
			var logoInfo = _services.PictureService.GetPictureInfo(messageContext.Store.LogoPictureId);
			m["Logo"] = CreateModelPart(logoInfo, messageContext, host);

			// TODO: (mc) Liquid > GetSupplierIdentification() as Partial

			PublishModelPartCreatedEvent<Store>(part, m);

			return m;
		}

		protected virtual object CreateModelPart(PictureInfo part, MessageContext messageContext, string href, int? targetSize = null, string alt = null)
		{
			Guard.NotNull(messageContext, nameof(messageContext));
			Guard.NotNull(part, nameof(part));
			Guard.NotEmpty(href, nameof(href));

			if (part == null)
				return null;

			var m = new
			{
				Src = _services.PictureService.GetUrl(part, targetSize.GetValueOrDefault(), FallbackPictureType.NoFallback, messageContext.BaseUri.ToString()),
				Href = href,
				Width = part?.Width,
				Height = part?.Height,
				Alt = alt
			};

			PublishModelPartCreatedEvent<PictureInfo>(part, m);

			return m;
		}

		protected virtual object CreateModelPart(Order part, MessageContext messageContext)
		{
			Guard.NotNull(messageContext, nameof(messageContext));
			Guard.NotNull(part, nameof(part));

			var allow = new HashSet<string>
			{
				nameof(part.Id),
				nameof(part.OrderNumber),
				nameof(part.OrderGuid),
				nameof(part.StoreId),
				nameof(part.OrderStatus),
				nameof(part.PaymentStatus),
				nameof(part.ShippingStatus),
				nameof(part.CustomerTaxDisplayType),
				nameof(part.TaxRatesDictionary),
				nameof(part.VatNumber),
				nameof(part.AffiliateId),
				nameof(part.CustomerIp),
				nameof(part.CardType),
				nameof(part.CardName),
				nameof(part.MaskedCreditCardNumber),
				nameof(part.DirectDebitAccountHolder),
				nameof(part.DirectDebitBankCode), // TODO: (mc) Liquid > Bank data (?)
				nameof(part.PurchaseOrderNumber),
				nameof(part.PaidDateUtc),
				nameof(part.ShippingMethod),
				nameof(part.PaymentMethodSystemName),
				nameof(part.ShippingRateComputationMethodSystemName),
				nameof(part.CreatedOnUtc)
				// TODO: (mc) Liquid > More whitelisting?
			};

			var m = new HybridExpando(part, allow, MemberOptMethod.Allow);
			var d = m as dynamic;
			
			d.ID = part.Id;
			d.Billing = CreateModelPart(part.BillingAddress, messageContext);
			d.Shipping = CreateModelPart(part.BillingAddress ?? new Address(), messageContext);
			d.CustomerFullName = part.BillingAddress.GetFullName();
			d.CustomerEmail = part.BillingAddress.Email;
			d.CustomerComment = part.CustomerOrderComment;
			d.Disclaimer = GetTopic("Disclaimer", messageContext);
			d.ConditionsOfUse = GetTopic("ConditionsOfUse", messageContext);

			// Payment method
			var paymentMethodName = part.PaymentMethodSystemName;
			var paymentMethod = _services.Resolve<IProviderManager>().GetProvider<IPaymentMethod>(part.PaymentMethodSystemName);
			if (paymentMethod != null)
			{
				paymentMethodName = GetLocalizedValue(messageContext, paymentMethod.Metadata, nameof(paymentMethod.Metadata.FriendlyName), x => x.FriendlyName);
			}
			d.PaymentMethod = paymentMethodName;

			// CreatedOn
			var createdOn = part.CreatedOnUtc.ToString("D");
			if (messageContext.Language?.LanguageCulture != null)
			{
				var localDate = _services.DateTimeHelper.ConvertToUserTime(part.CreatedOnUtc, TimeZoneInfo.Utc, _services.DateTimeHelper.GetCustomerTimeZone(part.Customer));
				createdOn = localDate.ToString("D", new CultureInfo(messageContext.Language.LanguageCulture));
			}
			d.CreatedOn = createdOn;

			d.OrderURLForCustomer = part.Customer != null && !part.Customer.IsGuest() 
				? BuildActionUrl("Details", "Order", new { id = part.Id, area = "" }, messageContext) 
				: "";

			// Overrides
			m.Properties["OrderNumber"] = part.GetOrderNumber();
			m.Properties["AcceptThirdPartyEmailHandOver"] = GetBoolResource(part.AcceptThirdPartyEmailHandOver, messageContext);
			m.Properties["OrderNotes"] = part.OrderNotes.Select(x => CreateModelPart(x, messageContext)).ToList();

			// Items, Totals & co.
			d.Items = part.OrderItems.Select(x => CreateModelPart(x, messageContext)).ToList();
			d.Totals = new { }; // TODO: (mc) Liquid

			// Checkout Attributes
			if (part.CheckoutAttributeDescription.HasValue())
			{
				d.CheckoutAttributes = HtmlUtils.ConvertPlainTextToTable(HtmlUtils.ConvertHtmlToPlainText(part.CheckoutAttributeDescription));
			}

			PublishModelPartCreatedEvent<Order>(part, m);

			return m;
		}

		protected virtual object CreateModelPart(OrderItem part, MessageContext messageContext)
		{
			Guard.NotNull(messageContext, nameof(messageContext));
			Guard.NotNull(part, nameof(part));

			// TODO: (mc) Liquid

			var product = part.Product;
			if (product == null)
			{
				return new Dictionary<string, object>();
			}

			product.MergeWithCombination(part.AttributesXml, _services.Resolve<IProductAttributeParser>());

			var m = new Dictionary<string, object>
			{
				{ "Name", product.GetLocalized(x => x.Name, messageContext.Language.Id) },
				{ "Url", _services.Resolve<ProductUrlHelper>().GetProductUrl(product.Id, product.GetSeName(), part.AttributesXml) }
			};

			PublishModelPartCreatedEvent<OrderItem>(part, m);

			return m;
		}

		protected virtual object CreateModelPart(Product part, MessageContext messageContext)
		{
			Guard.NotNull(messageContext, nameof(messageContext));
			Guard.NotNull(part, nameof(part));

			var mediaSettings = _services.Resolve<MediaSettings>();

			var currency = _services.WorkContext.WorkingCurrency;
			var additionalShippingCharge = _services.Resolve<ICurrencyService>().ConvertFromPrimaryStoreCurrency(part.AdditionalShippingCharge, currency);
			var additionalShippingChargeFormatted = _services.Resolve<IPriceFormatter>().FormatPrice(additionalShippingCharge, false, currency.CurrencyCode, false, messageContext.Language);
			var url = BuildRouteUrl("Product", new { SeName = part.GetSeName(messageContext.Language.Id) }, messageContext);
			var pictureInfo = GetPictureFor(part, null);
			var name = part.GetLocalized(x => x.Name, messageContext.Language.Id);
			var alt = T("Media.Product.ImageAlternateTextFormat", messageContext.Language.Id, name);
			
			var m = new Dictionary<string, object>
			{
				{ "Id", part.Id },
				{ "Sku", part.Sku },
				{ "Name", name },
				{ "Description", part.GetLocalized(x => x.ShortDescription, messageContext.Language.Id) },
				{ "StockQuantity", part.StockQuantity },
				{ "AdditionalShippingCharge", additionalShippingChargeFormatted },
				{ "Url", url },
				{ "Thumbnail", CreateModelPart(pictureInfo, messageContext, url, mediaSettings.MessageProductThumbPictureSize, alt) }
			};

			PublishModelPartCreatedEvent<Product>(part, m);

			return m;
		}

		protected virtual object CreateModelPart(Customer part, MessageContext messageContext)
		{
			Guard.NotNull(messageContext, nameof(messageContext));
			Guard.NotNull(part, nameof(part));

			var allow = new HashSet<string> 
			{
				nameof(part.Id),
				nameof(part.CustomerGuid),
				nameof(part.Username),
				nameof(part.Email),
				nameof(part.IsTaxExempt),
				nameof(part.LastIpAddress),
				nameof(part.CreatedOnUtc),
				nameof(part.LastLoginDateUtc),
				nameof(part.LastActivityDateUtc)
			};

			var m = new HybridExpando(part, allow, MemberOptMethod.Allow);

			var email = part.FindEmail();

			m.Properties[nameof(part.Email)] = email;
			m.Properties[nameof(part.RewardPointsHistory)] = part.RewardPointsHistory.Select(x => CreateModelPart(x, messageContext)).ToList();
			m.Properties[nameof(part.BillingAddress)] = CreateModelPart(part.BillingAddress ?? new Address(), messageContext);
			m.Properties[nameof(part.ShippingAddress)] = CreateModelPart(part.ShippingAddress ?? new Address(), messageContext);

			m["FullName"] = GetDisplayNameForCustomer(part);
			m["VatNumber"] = part.GetAttribute<string>(SystemCustomerAttributeNames.VatNumber);
			m["VatNumberStatus"] = part.GetAttribute<VatNumberStatus>(SystemCustomerAttributeNames.VatNumberStatusId).ToString();
			m["CustomerNumber"] = part.GetAttribute<string>(SystemCustomerAttributeNames.CustomerNumber);
			m["IsRegistered"] = part.IsRegistered();

			m["PasswordRecoveryURL"] = BuildActionUrl("passwordrecoveryconfirm", "customer", 
				new { token = part.GetAttribute<string>(SystemCustomerAttributeNames.PasswordRecoveryToken), email = email, area = "" },
				messageContext);

			m["AccountActivationURL"] = BuildActionUrl("activation", "customer",
				new { token = part.GetAttribute<string>(SystemCustomerAttributeNames.AccountActivationToken), email = email, area = "" },
				messageContext);

			m["WishlistUrl"] = BuildRouteUrl("Wishlist", new { customerGuid = part.CustomerGuid }, messageContext);
			m["EditUrl"] = BuildActionUrl("Edit", "Customer", new { id = part.Id, area = "admin" }, messageContext);

			PublishModelPartCreatedEvent<Customer>(part, m);

			return m;
		}

		protected virtual object CreateModelPart(Shipment part, MessageContext messageContext)
		{
			Guard.NotNull(messageContext, nameof(messageContext));
			Guard.NotNull(part, nameof(part));

			return null;
		}

		protected virtual object CreateModelPart(OrderNote part, MessageContext messageContext)
		{
			Guard.NotNull(messageContext, nameof(messageContext));
			Guard.NotNull(part, nameof(part));

			return null;
		}

		protected virtual object CreateModelPart(RecurringPayment part, MessageContext messageContext)
		{
			Guard.NotNull(messageContext, nameof(messageContext));
			Guard.NotNull(part, nameof(part));
			
			return null;
		}

		protected virtual object CreateModelPart(ReturnRequest part, MessageContext messageContext)
		{
			Guard.NotNull(messageContext, nameof(messageContext));
			Guard.NotNull(part, nameof(part));

			OrderItem orderItem = null;
			if (part.OrderItemId > 0)
			{
				orderItem = _services.Resolve<IOrderService>().GetOrderItemById(part.OrderItemId);
			}

			var disallow = new[] { nameof(part.AdminComment), nameof(part.Customer) };

			var m = new HybridExpando(part, disallow, MemberOptMethod.Disallow);
			m["ProductName"] = orderItem?.Product?.Name; // TODO: Liquid > Product.Name > ProductName
			m["Reason"] = part.ReasonForReturn;
			m["Status"] = part.ReturnRequestStatus.GetLocalizedEnum(_services.Localization, _services.WorkContext);

			// TODO: Liquid > WTF?
			m.Override(nameof(part.CustomerComments), HtmlUtils.FormatText(part.CustomerComments, false, true, false, false, false, false));
			m.Override(nameof(part.StaffNotes), HtmlUtils.FormatText(part.StaffNotes, false, true, false, false, false, false));

			PublishModelPartCreatedEvent<ReturnRequest>(part, m);

			return m;
		}

		protected virtual object CreateModelPart(GiftCard part, MessageContext messageContext)
		{
			Guard.NotNull(messageContext, nameof(messageContext));
			Guard.NotNull(part, nameof(part));

			return null;
		}

		protected virtual object CreateModelPart(NewsLetterSubscription part, MessageContext messageContext)
		{
			Guard.NotNull(messageContext, nameof(messageContext));
			Guard.NotNull(part, nameof(part));

			return null;
		}

		protected virtual object CreateModelPart(ProductReview part, MessageContext messageContext)
		{
			Guard.NotNull(messageContext, nameof(messageContext));
			Guard.NotNull(part, nameof(part));

			var m = new Dictionary<string, object>
			{
				{ "Title", part.Title },
				{ "Text", HtmlUtils.FormatText(part.ReviewText, true, false, false, false, false, false) },
				{ "Rating", part.Rating }
			};

			PublishModelPartCreatedEvent<ProductReview>(part, m);

			return m;
		}

		protected virtual object CreateModelPart(PrivateMessage part, MessageContext messageContext)
		{
			Guard.NotNull(messageContext, nameof(messageContext));
			Guard.NotNull(part, nameof(part));

			var m = new Dictionary<string, object>
			{
				{  "Subject", part.Subject },
				{  "Text", part.FormatPrivateMessageText() },
				{  "FromEmail", part.FromCustomer?.FindEmail() },
				{  "ToEmail", part.ToCustomer?.FindEmail() },
				{  "FromName", part.FromCustomer?.GetFullName() },
				{  "ToName", part.ToCustomer?.GetFullName() },
				{  "Url", BuildActionUrl("View", "PrivateMessages", new { id = part.Id, area = "" }, messageContext) }
			};

			PublishModelPartCreatedEvent<PrivateMessage>(part, m);

			return m;
		}

		protected virtual object CreateModelPart(BlogComment part, MessageContext messageContext)
		{
			Guard.NotNull(messageContext, nameof(messageContext));
			Guard.NotNull(part, nameof(part));

			var m = new Dictionary<string, object>
			{
				{  "PostTitle", part.BlogPost.Title },
				{  "PostUrl", BuildRouteUrl("BlogPost", new { SeName = part.BlogPost.GetSeName(part.BlogPost.LanguageId, ensureTwoPublishedLanguages: false) }, messageContext) },
				{  "Text", part.CommentText }
			};

			PublishModelPartCreatedEvent<BlogComment>(part, m);

			return m;
		}

		protected virtual object CreateModelPart(NewsComment part, MessageContext messageContext)
		{
			Guard.NotNull(messageContext, nameof(messageContext));
			Guard.NotNull(part, nameof(part));

			var m = new Dictionary<string, object>
			{
				{  "NewsTitle", part.NewsItem.Title },
				{  "Title", part.CommentTitle },
				{  "Text", HtmlUtils.FormatText(part.CommentText, true, false, false, false, false, false) },
				{  "NewsUrl", BuildRouteUrl("NewsItem", new { SeName = part.NewsItem.GetSeName(messageContext.Language.Id) }, messageContext) }
			};

			PublishModelPartCreatedEvent<NewsComment>(part, m);

			return m;
		}

		protected virtual object CreateModelPart(ForumTopic part, MessageContext messageContext)
		{
			Guard.NotNull(messageContext, nameof(messageContext));
			Guard.NotNull(part, nameof(part));

			// TODO: (mc) Liquid > Implement TopicSlugPaged with friendlyForumTopicPageIndex param!
			
			var m = new Dictionary<string, object>
			{
				{ "Subject", part.Subject },
				{ "NumReplies", part.NumReplies },
				{ "NumPosts", part.NumPosts },
				{ "NumViews", part.Views },
				{ "Body", part.GetFirstPost(_services.Resolve<IForumService>())?.FormatPostText() },
				{ "Url", BuildRouteUrl("TopicSlug", new { id = part.Id, slug = part.GetSeName() }, messageContext) },
			};

			PublishModelPartCreatedEvent<ForumTopic>(part, m);

			return m;
		}

		protected virtual object CreateModelPart(ForumPost part, MessageContext messageContext)
		{
			Guard.NotNull(messageContext, nameof(messageContext));
			Guard.NotNull(part, nameof(part));

			var m = new Dictionary<string, object>
			{
				{ "Author", part.Customer.FormatUserName() },
				{ "Body", part.FormatPostText() }
			};

			PublishModelPartCreatedEvent<ForumPost>(part, m);

			return m;
		}

		protected virtual object CreateModelPart(Forum part, MessageContext messageContext)
		{
			Guard.NotNull(messageContext, nameof(messageContext));
			Guard.NotNull(part, nameof(part));

			var m = new Dictionary<string, object>
			{
				{ "Name", part.GetLocalized(x => x.Name, messageContext.Language.Id) },
				{ "GroupName", part.ForumGroup?.GetLocalized(x => x.Name, messageContext.Language.Id).EmptyNull() },
				{ "NumPosts", part.NumPosts },
				{ "NumTopics", part.NumTopics },
				{ "Url", BuildRouteUrl("ForumSlug", new {  id = part.Id, slug = part.GetSeName(messageContext.Language.Id) }, messageContext) },
			};

			PublishModelPartCreatedEvent<Forum>(part, m);

			return m;
		}

		protected virtual object CreateModelPart(BackInStockSubscription part, MessageContext messageContext)
		{
			Guard.NotNull(messageContext, nameof(messageContext));
			Guard.NotNull(part, nameof(part));

			var m = new Dictionary<string, object>
			{
				{  "ProductName", part.Product?.Name },
				{  "ProductUrl", BuildRouteUrl("Product", new { SeName = part.Product?.GetSeName(messageContext.Language.Id) }, messageContext) }
			};

			PublishModelPartCreatedEvent<BackInStockSubscription>(part, m);

			return m;
		}

		protected virtual object CreateModelPart(Address part, MessageContext messageContext)
		{
			Guard.NotNull(messageContext, nameof(messageContext));
			Guard.NotNull(part, nameof(part));

			var m = new HybridExpando(part);

			m["FullSalutation"] = part.GetFullSalutaion();

			// Overrides
			m.Properties["StateProvince"] = part.StateProvince?.GetLocalized(x => x.Name, messageContext.Language.Id).EmptyNull();
			m.Properties["Country"] = part.Country?.GetLocalized(x => x.Name, messageContext.Language.Id).EmptyNull();

			PublishModelPartCreatedEvent<Address>(part, m);

			return m;
		}

		protected virtual object CreateModelPart(ShoppingCartItem part, MessageContext messageContext)
		{
			Guard.NotNull(messageContext, nameof(messageContext));
			Guard.NotNull(part, nameof(part));

			return null;
		}

		protected virtual object CreateModelPart(RewardPointsHistory part, MessageContext messageContext)
		{
			Guard.NotNull(messageContext, nameof(messageContext));
			Guard.NotNull(part, nameof(part));

			return null;
		}

		#endregion

		#region Model Tree

		public TreeNode<ModelTreeMember> BuildModelTree(IDictionary<string, object> model)
		{
			Guard.NotNull(model, nameof(model));

			var root = new TreeNode<ModelTreeMember>(new ModelTreeMember { Name = "Model", Kind = ModelTreeMemberKind.Root });

			foreach (var kvp in model)
			{
				root.Append(BuildModelTreePart(kvp.Key, kvp.Value));
			}

			return root;
		}

		private TreeNode<ModelTreeMember> BuildModelTreePart(string modelName, object instance)
		{
			var t = instance?.GetType();
			TreeNode<ModelTreeMember> node = null;

			if (t == null || t.IsPredefinedType())
			{
				node = new TreeNode<ModelTreeMember>(new ModelTreeMember { Name = modelName, Kind = ModelTreeMemberKind.Primitive });
			}
			else if (t.IsSequenceType() && !(instance is IDictionary<string, object>))
			{
				node = new TreeNode<ModelTreeMember>(new ModelTreeMember { Name = modelName, Kind = ModelTreeMemberKind.Collection });
			}
			else
			{
				node = new TreeNode<ModelTreeMember>(new ModelTreeMember { Name = modelName, Kind = ModelTreeMemberKind.Complex });

				if (instance is IDictionary<string, object> dict)
				{
					foreach (var kvp in dict)
					{
						node.Append(BuildModelTreePart(kvp.Key, kvp.Value));
					}
				}
				else if (instance is IDynamicMetaObjectProvider dyn)
				{
					foreach (var name in dyn.GetMetaObject(Expression.Constant(dyn)).GetDynamicMemberNames())
					{
						// we don't want to go deeper in "pure" dynamic objects
						node.Append(new TreeNode<ModelTreeMember>(new ModelTreeMember { Name = name, Kind = ModelTreeMemberKind.Primitive }));
					}
				}
				else
				{
					node.AppendRange(BuildModelTreePartForClass(instance));
				}
			}

			return node;
		}

		private IEnumerable<TreeNode<ModelTreeMember>> BuildModelTreePartForClass(object instance)
		{
			var type = instance?.GetType();

			if (type == null)
			{
				yield break;
			}

			foreach (var prop in FastProperty.GetProperties(type).Values)
			{
				var pi = prop.Property;

				if (pi.PropertyType.IsPredefinedType())
				{
					yield return new TreeNode<ModelTreeMember>(new ModelTreeMember { Name = prop.Name, Kind = ModelTreeMemberKind.Primitive });
				}
				else if (typeof(IDictionary<string, object>).IsAssignableFrom(pi.PropertyType))
				{
					yield return BuildModelTreePart(prop.Name, prop.GetValue(instance));
				}
				else if (pi.PropertyType.IsSequenceType())
				{
					yield return new TreeNode<ModelTreeMember>(new ModelTreeMember { Name = prop.Name, Kind = ModelTreeMemberKind.Collection });
				}
				else
				{
					var node = new TreeNode<ModelTreeMember>(new ModelTreeMember { Name = prop.Name, Kind = ModelTreeMemberKind.Complex });
					node.AppendRange(BuildModelTreePartForClass(prop.GetValue(instance)));
					yield return node;
				}
			}
		}

		#endregion

		#region Utils

		private string BuildUrl(string url, MessageContext ctx)
		{
			return ctx.BaseUri.ToString().TrimEnd('/') + url;
		}

		private string BuildRouteUrl(object routeValues, MessageContext ctx)
		{
			return ctx.BaseUri.ToString().TrimEnd('/') + _urlHelper.RouteUrl(routeValues);
		}

		private string BuildRouteUrl(string routeName, object routeValues, MessageContext ctx)
		{
			return ctx.BaseUri.ToString().TrimEnd('/') + _urlHelper.RouteUrl(routeName, routeValues);
		}

		private string BuildActionUrl(string action, string controller, object routeValues, MessageContext ctx)
		{
			return ctx.BaseUri.ToString().TrimEnd('/') + _urlHelper.Action(action, controller, routeValues);
		}

		private void PublishModelPartCreatedEvent<T>(T source, dynamic part) where T : class
		{
			_services.EventPublisher.Publish(new MessageModelPartCreatedEvent<T>(source, part));
		}

		private string GetLocalizedValue(MessageContext messageContext, ProviderMetadata metadata, string propertyName, Expression<Func<ProviderMetadata, string>> fallback)
		{
			// TODO: (mc) this actually belongs to PluginMediator, but we simply cannot add a dependency to framework from here. Refactor later!

			Guard.NotNull(metadata, nameof(metadata));

			string systemName = metadata.SystemName;
			var resourceName = metadata.ResourceKeyPattern.FormatInvariant(metadata.SystemName, propertyName);
			string result = _services.Localization.GetResource(resourceName, messageContext.Language.Id, false, "", true);

			if (result.IsEmpty())
				result = fallback.Compile()(metadata);

			return result;
		}

		private object GetTopic(string topicSystemName, MessageContext ctx)
		{
			var topicService = _services.Resolve<ITopicService>();

			// Load by store
			var topic = topicService.GetTopicBySystemName(topicSystemName, ctx.Store.Id);
			if (topic == null)
			{
				// Not found. Let's find topic assigned to all stores
				topic = topicService.GetTopicBySystemName(topicSystemName, 0);
			}

			return new
			{
				Title = topic?.Title.EmptyNull(),
				Body = topic?.Body.EmptyNull()
			};
		}

		private string GetDisplayNameForCustomer(Customer customer)
		{
			return customer.GetFullName().NullEmpty() ?? customer.Username ?? customer.FindEmail();
		}

		private string GetBoolResource(bool value, MessageContext ctx)
		{
			return _services.Localization.GetResource(value ? "Common.Yes" : "Common.No", ctx.Language.Id);
		}

		private PictureInfo GetPictureFor(Product product, string attributesXml)
		{
			var pictureService = _services.PictureService;
			var attrParser = _services.Resolve<IProductAttributeParser>();

			PictureInfo pictureInfo = null;

			if (attributesXml.HasValue())
			{
				var combination = attrParser.FindProductVariantAttributeCombination(product.Id, attributesXml);

				if (combination != null)
				{
					var picturesIds = combination.GetAssignedPictureIds();
					if (picturesIds != null && picturesIds.Length > 0)
					{
						pictureInfo = pictureService.GetPictureInfo(picturesIds[0]);
					}	
				}
			}

			if (pictureInfo == null)
			{
				pictureInfo = pictureService.GetPictureInfo(product.MainPictureId);
			}

			if (pictureInfo == null && !product.VisibleIndividually && product.ParentGroupedProductId > 0)
			{
				pictureInfo = pictureService.GetPictureInfo(pictureService.GetPicturesByProductId(product.ParentGroupedProductId, 1).FirstOrDefault());
			}

			return pictureInfo;
		}

		#endregion
	}
}
