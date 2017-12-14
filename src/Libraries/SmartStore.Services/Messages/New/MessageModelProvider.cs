using System;
using System.Collections.Generic;
using System.Dynamic;
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
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Domain.Tax;
using SmartStore.Core.Html;
using SmartStore.Core.Localization;
using SmartStore.Core.Logging;
using SmartStore.Services.Catalog;
using SmartStore.Services.Catalog.Extensions;
using SmartStore.Services.Common;
using SmartStore.Services.Customers;
using SmartStore.Services.Localization;
using SmartStore.Services.Orders;
using SmartStore.Services.Forums;
using SmartStore.Services.Seo;
using SmartStore.Templating;
using SmartStore.Services.Directory;
using SmartStore.Services.Media;
using SmartStore.Core.Domain.Directory;
using System.Text;

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
				//case BackInStockSubscription x:
				//	modelPart = CreateModelPart(x, messageContext);
				//	break;
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
			model.FontFamily = "-apple-system, system-ui, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif";
			model.BodyBg = "#f2f4f6";
			model.BodyColor = "#74787e";
			model.TitleColor = "#2f3133";
			model.ContentBg = "#fff";
			model.ShadeColor = "#e2e2e2";
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

			var logoInfo = _services.PictureService.GetPictureInfo(messageContext.Store.LogoPictureId);
			m["Logo"] = CreateModelPart(logoInfo, messageContext, host);

			// TODO: (mc) Liquid > GetSupplierIdentification() as Partial
			// Issue: https://github.com/smartstoreag/SmartStoreNET/issues/1321

			PublishModelPartCreatedEvent<Store>(part, m);

			return m;
		}

		protected virtual object CreateModelPart(PictureInfo part, MessageContext messageContext, string href, int? targetSize = null, string alt = null)
		{
			Guard.NotNull(messageContext, nameof(messageContext));
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

		protected virtual object CreateModelPart(Product part, MessageContext messageContext, string attributesXml = null)
		{
			Guard.NotNull(messageContext, nameof(messageContext));
			Guard.NotNull(part, nameof(part));

			var mediaSettings = _services.Resolve<MediaSettings>();
			var shoppingCartSettings = _services.Resolve<ShoppingCartSettings>();
			var catalogSettings = _services.Resolve<CatalogSettings>();
			var deliveryTimeService = _services.Resolve<IDeliveryTimeService>();
			var quantityUnitService = _services.Resolve<IQuantityUnitService>();
			var productUrlHelper = _services.Resolve<ProductUrlHelper>();

			var currency = _services.WorkContext.WorkingCurrency;
			var additionalShippingCharge = _services.Resolve<ICurrencyService>().ConvertFromPrimaryStoreCurrency(part.AdditionalShippingCharge, currency);
			var additionalShippingChargeFormatted = _services.Resolve<IPriceFormatter>().FormatPrice(additionalShippingCharge, false, currency.CurrencyCode, false, messageContext.Language);
			var url = productUrlHelper.GetProductUrl(part.Id, part.GetSeName(messageContext.Language.Id), attributesXml);
			var pictureInfo = GetPictureFor(part, null);
			var name = part.GetLocalized(x => x.Name, messageContext.Language.Id);
			var alt = T("Media.Product.ImageAlternateTextFormat", messageContext.Language.Id, name);
			
			var m = new Dictionary<string, object>
			{
				{ "Id", part.Id },
				{ "Sku", catalogSettings.ShowProductSku ? part.Sku : "" },
				{ "Name", name },
				{ "Description", part.GetLocalized(x => x.ShortDescription, messageContext.Language.Id) },
				{ "StockQuantity", part.StockQuantity },
				{ "AdditionalShippingCharge", additionalShippingChargeFormatted },
				{ "Url", url },
				{ "Thumbnail", CreateModelPart(pictureInfo, messageContext, url, mediaSettings.MessageProductThumbPictureSize, alt) },
				{ "DeliveryTime", null },
				{ "QtyUnit", null }
			};

			if (shoppingCartSettings.ShowDeliveryTimes && part.IsShipEnabled)
			{
				if (deliveryTimeService.GetDeliveryTimeById(part.DeliveryTimeId ?? 0) is DeliveryTime dt)
				{
					m["DeliveryTime"] = new Dictionary<string, object>
					{
						{ "Color", dt.ColorHexValue },
						{ "Name", dt.GetLocalized(x => x.Name, messageContext.Language.Id) },
					};
				}
			}

			if (quantityUnitService.GetQuantityUnitById(part.QuantityUnitId) is QuantityUnit qu)
			{
				m["QtyUnit"] = qu.GetLocalized(x => x.Name, messageContext.Language.Id);
			}

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

			// Reward Points
			int rewardPointsBalance = part.GetRewardPointsBalance();
			decimal rewardPointsAmountBase = _services.Resolve<IOrderTotalCalculationService>().ConvertRewardPointsToAmount(rewardPointsBalance);
			decimal rewardPointsAmount = _services.Resolve<ICurrencyService>().ConvertFromPrimaryStoreCurrency(rewardPointsAmountBase, _services.WorkContext.WorkingCurrency);
			m["RewardPointsAmount"] = rewardPointsAmount;
			m["RewardPointsBalance"] = _services.Resolve<IPriceFormatter>().FormatPrice(rewardPointsAmount, true, false);

			PublishModelPartCreatedEvent<Customer>(part, m);

			return m;
		}

		protected virtual object CreateModelPart(GiftCard part, MessageContext messageContext)
		{
			Guard.NotNull(messageContext, nameof(messageContext));
			Guard.NotNull(part, nameof(part));

			var m = new Dictionary<string, object>
			{
				{ "Id", part.Id },
				{ "SenderName", part.SenderName },
				{ "SenderEmail", part.SenderEmail },
				{ "RecipientName", part.RecipientName },
				{ "RecipientEmail", part.RecipientEmail },
				{ "Amount", _services.Resolve<IPriceFormatter>().FormatPrice(part.Amount, true, false) },
				{ "CouponCode", part.GiftCardCouponCode }
			};

			// Message
			var message = "";
			if (part.Message.HasValue())
			{
				message = HtmlUtils.FormatText(part.Message, true, false, false, false, false, false);
			}
			m["Message"] = message;

			// RemainingAmount
			var remainingAmount = "";
			var order = part?.PurchasedWithOrderItem?.Order;
			if (order != null)
			{
				var amount = _services.Resolve<ICurrencyService>().ConvertCurrency(part.GetGiftCardRemainingAmount(), order.CurrencyRate);
				remainingAmount = _services.Resolve<IPriceFormatter>().FormatPrice(amount, true, false);
			}
			m["RemainingAmount"] = remainingAmount;

			PublishModelPartCreatedEvent<GiftCard>(part, m);

			return null;
		}

		protected virtual object CreateModelPart(NewsLetterSubscription part, MessageContext messageContext)
		{
			Guard.NotNull(messageContext, nameof(messageContext));
			Guard.NotNull(part, nameof(part));
			
			var m = new Dictionary<string, object>
			{
				{ "Id", part.Id },
				{ "Email", part.Email },
				{ "ActivationUrl", BuildRouteUrl("NewsletterActivation", new { token = part.NewsLetterSubscriptionGuid, active = true }, messageContext) },
				{ "DeactivationUrl", BuildRouteUrl("NewsletterActivation", new { token = part.NewsLetterSubscriptionGuid, active = false }, messageContext) }
			};

			PublishModelPartCreatedEvent<NewsLetterSubscription>(part, m);

			return m;
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

		protected virtual object CreateModelPart(Address part, MessageContext messageContext)
		{
			Guard.NotNull(messageContext, nameof(messageContext));
			Guard.NotNull(part, nameof(part));

			//var disallow = new[] { nameof(part.CreatedOnUtc), nameof(part.StateProvinceId), nameof(part.CountryId), };

			//var m = new HybridExpando(part, disallow, MemberOptMethod.Disallow);

			//m["FullSalutation"] = part.GetFullSalutaion();

			//// Overrides
			//m.Properties["StateProvince"] = part.StateProvince?.GetLocalized(x => x.Name, messageContext.Language.Id).EmptyNull();
			//m.Properties["Country"] = part.Country?.GetLocalized(x => x.Name, messageContext.Language.Id).EmptyNull();

			var settings = _services.Resolve<AddressSettings>();

			var m = new Dictionary<string, object>
			{
				{ "Title", part.Title },
				{ "Salutation", part.Salutation },
				{ "FullSalutation", part.GetFullSalutaion() },
				{ "FullName", part.GetFullName(false) },
				{ "Company", settings.CompanyEnabled ? part.Company : "" },
				{ "FirstName", part.FirstName },
				{ "LastName", part.LastName },
				{ "Address1", settings.StreetAddressEnabled ? part.Address1 : "" },
				{ "Address2", settings.StreetAddress2Enabled ? part.Address2 : "" },
				{ "Country", settings.CountryEnabled ? part.Country?.GetLocalized(x => x.Name, messageContext.Language.Id).EmptyNull() : "" },
				{ "State", settings.StateProvinceEnabled ? part.StateProvince?.GetLocalized(x => x.Name, messageContext.Language.Id).EmptyNull() : "" },
				{ "City", settings.CityEnabled ? part.City : "" },
				{ "ZipCode", settings.ZipPostalCodeEnabled ? part.ZipPostalCode : "" },
				{ "Email", part.Email },
				{ "Phone", settings.PhoneEnabled ? part.PhoneNumber : "" },
				{ "Fax", settings.FaxEnabled ? part.FaxNumber : "" }
			};

			m["FullCity"] = GetFullCity();

			PublishModelPartCreatedEvent<Address>(part, m);

			return m;

			string GetFullCity()
			{
				var zip = m["ZipCode"] as string;
				var city = m["City"] as string;

				var sb = new StringBuilder();

				if (city.HasValue())
				{
					sb.Append(city);
					if (zip.HasValue())
					{
						sb.Append(", ");
					}
				}

				if (zip.HasValue())
				{
					sb.Append(zip);
				}

				return sb.ToString();
			}
		}

		protected virtual object CreateModelPart(RewardPointsHistory part, MessageContext messageContext)
		{
			Guard.NotNull(messageContext, nameof(messageContext));
			Guard.NotNull(part, nameof(part));
			
			var m = new Dictionary<string, object>
			{
				{ "Id", part.Id },
				{ "CreatedOn", ToUserDate(part.CreatedOnUtc, messageContext) },
				{ "Message", part.Message },
				{ "Points", part.Points },
				{ "PointsBalance", part.PointsBalance },
				{ "UsedAmount", part.UsedAmount }
			};

			PublishModelPartCreatedEvent<RewardPointsHistory>(part, m);

			return m;
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
	}
}
