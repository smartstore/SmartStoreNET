using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
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
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Forums;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Domain.Messages;
using SmartStore.Core.Domain.News;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Polls;
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
using SmartStore.Services.Directory;
using SmartStore.Services.Forums;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
using SmartStore.Services.Orders;
using SmartStore.Services.Seo;
using SmartStore.Templating;
using SmartStore.Utilities;

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
		private readonly IMessageTemplateService _messageTemplateService;
		private readonly IEmailAccountService _emailAccountService;
		private readonly UrlHelper _urlHelper;

		public MessageModelProvider(
			ICommonServices services,
			ITemplateEngine templateEngine,
			IMessageTemplateService messageTemplateService,
			IEmailAccountService emailAccountService,
			UrlHelper urlHelper)
		{
			_services = services;
			_templateEngine = templateEngine;
			_messageTemplateService = messageTemplateService;
			_emailAccountService = emailAccountService;
			_urlHelper = urlHelper;

			T = NullLocalizer.InstanceEx;
			Logger = NullLogger.Instance;
		}

		public LocalizerEx T { get; set; }
		public ILogger Logger { get; set; }

		public virtual void AddGlobalModelParts(MessageContext messageContext)
		{
			Guard.NotNull(messageContext, nameof(messageContext));

			var model = messageContext.Model;

			model["Context"] = new Dictionary<string, object>
			{
				{ "TemplateName", messageContext.MessageTemplate.Name },
				{ "LanguageId", messageContext.Language.Id },
				{ "LanguageCulture", messageContext.Language.LanguageCulture },
				{ "LanguageRtl", messageContext.Language.Rtl },
				{ "BaseUrl", messageContext.BaseUri.ToString() }
			};

			dynamic email = new ExpandoObject();
			email.Email = messageContext.EmailAccount.Email;
			email.SenderName = messageContext.EmailAccount.DisplayName;
			email.DisplayName = messageContext.EmailAccount.DisplayName; // Alias
			model["Email"] = email;

			model["Theme"] = CreateThemeModelPart(messageContext);
			model["Customer"] = CreateModelPart(messageContext.Customer, messageContext);
			model["Store"] = CreateModelPart(messageContext.Store, messageContext);
		}

		public object CreateModelPart(object part, bool ignoreNullMembers, params string[] ignoreMemberNames)
		{
			Guard.NotNull(part, nameof(part));

			var store = _services.StoreContext.CurrentStore;
			var messageContext = new MessageContext
			{
				Language = _services.WorkContext.WorkingLanguage,
				Store = store,
				BaseUri = new Uri(_services.StoreService.GetHost(store)),
				Model = new TemplateModel()
			};
			
			if (part is Customer x)
			{
				// This case is not handled in AddModelPart core method.
				messageContext.Customer = x;
				messageContext.Model["Part"] = CreateModelPart(x, messageContext);
			}
			else
			{
				messageContext.Customer = _services.WorkContext.CurrentCustomer;
				AddModelPart(part, messageContext, "Part");
			}

			object result = null;

			if (messageContext.Model.Any())
			{
				result = messageContext.Model.FirstOrDefault().Value;

				if (result is IDictionary<string, object> dict)
				{
					SanitizeModelDictionary(dict, ignoreNullMembers, ignoreMemberNames);
				}
			}

			return result;
		}

		private void SanitizeModelDictionary(IDictionary<string, object> dict, bool ignoreNullMembers, params string[] ignoreMemberNames)
		{
			if (ignoreNullMembers || ignoreMemberNames.Length > 0)
			{
				foreach (var key in dict.Keys.ToArray())
				{
					var expando = dict as HybridExpando;
					var value = dict[key];

					if ((ignoreNullMembers && value == null) || ignoreMemberNames.Contains(key))
					{
						if (expando != null)
							expando.Override(key, null); // INFO: we cannot remove entries from HybridExpando
						else
							dict.Remove(key);
						continue;
					}

					if (value != null && value.GetType().IsSequenceType())
					{
						var ignoreMemberNames2 = ignoreMemberNames
							.Where(x => x.StartsWith(key + ".", StringComparison.OrdinalIgnoreCase))
							.Select(x => x.Substring(key.Length + 1))
							.ToArray();

						if (value is IDictionary<string, object> dict2)
						{
							SanitizeModelDictionary(dict2, ignoreNullMembers, ignoreMemberNames2);
						}
						else
						{
							var list = ((IEnumerable)value).OfType<IDictionary<string, object>>();
							foreach (var dict3 in list)
							{
								SanitizeModelDictionary(dict3, ignoreNullMembers, ignoreMemberNames2);
							}
						}
					}
				}
			}
		}

		public virtual void AddModelPart(object part, MessageContext messageContext, string name = null)
		{
			Guard.NotNull(part, nameof(part));
			Guard.NotNull(messageContext, nameof(messageContext));

			var model = messageContext.Model;

			name = name.NullEmpty() ?? ResolveModelName(part);

			object modelPart = null;

			switch (part)
			{
				case INamedModelPart x:
					modelPart = x;
					break;
				case IModelPart x:
					MergeModelBag(x, model, messageContext);
					break;
				case Order x:
					modelPart = CreateModelPart(x, messageContext);
					break;
				case Product x:
					modelPart = CreateModelPart(x, messageContext);
					break;
				case Customer x:
					//modelPart = CreateModelPart(x, messageContext);
					break;
				case Address x:
					modelPart = CreateModelPart(x, messageContext);
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
				case Campaign x:
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
				case IEnumerable<GenericAttribute> x:
					modelPart = CreateModelPart(x, messageContext);
					break;
				case PollVotingRecord x:
					modelPart = CreateModelPart(x, messageContext);
					break;
				case ProductReviewHelpfulness x:
					modelPart = CreateModelPart(x, messageContext);
					break;
				case ForumSubscription x:
					modelPart = CreateModelPart(x, messageContext);
					break;
				case BackInStockSubscription x:
					modelPart = CreateModelPart(x, messageContext);
					break;
				default:
					var partType = part.GetType();
					modelPart = part;

					if (partType.IsPlainObjectType() && !partType.IsAnonymous())
					{
						var evt = new MessageModelPartMappingEvent(part);
						_services.EventPublisher.Publish(evt);

						if (evt.Result != null && !object.ReferenceEquals(evt.Result, part))
						{
							modelPart = evt.Result;
							name = evt.ModelPartName.NullEmpty() ?? ResolveModelName(evt.Result) ?? name;
						}
						else
						{
							modelPart = part;
						}

						modelPart = evt.Result ?? part;
						name = evt.ModelPartName.NullEmpty() ?? name;
					}

					break;
			}

			if (modelPart != null)
			{
				if (name.IsEmpty())
				{
					throw new SmartException($"Could not resolve a model key for part '{modelPart.GetType().Name}'. Use an instance of 'NamedModelPart' class to pass model with name.");
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
						var he = new HybridExpando(existing, true);
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
				else if (model is INamedModelPart mp)
				{
					name = mp.ModelPartName;
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
			var m = new Dictionary<string, object>
			{
				{ "FontFamily", "-apple-system, system-ui, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif" },
				{ "BodyBg", "#f2f4f6" },
				{ "BodyColor", "#555" },
				{ "TitleColor", "#2f3133" },
				{ "ContentBg", "#fff" },
				{ "ShadeColor", "#e2e2e2" },
				{ "LinkColor", "#0066c0" },
				{ "BrandPrimary", "#3f51b5" },
				{ "BrandSuccess", "#4caf50" },
				{ "BrandWarning", "#ff9800" },
				{ "BrandDanger", "#f44336" },
				{ "MutedColor", "#a5a5a5" },
			};

			return m;
		}

		protected virtual object CreateCompanyModelPart(MessageContext messageContext)
		{
			var settings = _services.Settings.LoadSetting<CompanyInformationSettings>(messageContext.Store.Id);
			dynamic m = new HybridExpando(settings, true);

			m.NameLine = Concat(settings.Salutation, settings.Title, settings.Firstname, settings.Lastname);
			m.StreetLine = Concat(settings.Street, settings.Street2);
			m.CityLine = Concat(settings.ZipCode, settings.City);
			m.CountryLine = Concat(settings.CountryName, settings.Region);

			PublishModelPartCreatedEvent<CompanyInformationSettings>(settings, m);
			return m;
		}

		protected virtual object CreateBankModelPart(MessageContext messageContext)
		{
			var settings = _services.Settings.LoadSetting<BankConnectionSettings>(messageContext.Store.Id);
			var m = new HybridExpando(settings, true);
			PublishModelPartCreatedEvent<BankConnectionSettings>(settings, m);
			return m;
		}

		protected virtual object CreateContactModelPart(MessageContext messageContext)
		{
			var settings = _services.Settings.LoadSetting<ContactDataSettings>(messageContext.Store.Id);
			var contact = new HybridExpando(settings, true) as dynamic;

			// Aliases
			contact.Phone = new
			{
				Company = settings.CompanyTelephoneNumber.NullEmpty(),
				Hotline = settings.HotlineTelephoneNumber.NullEmpty(),
				Mobile = settings.MobileTelephoneNumber.NullEmpty(),
				Fax = settings.CompanyFaxNumber.NullEmpty()
			};

			contact.Email = new
			{
				Company = settings.CompanyEmailAddress.NullEmpty(),
				Webmaster = settings.WebmasterEmailAddress.NullEmpty(),
				Support = settings.SupportEmailAddress.NullEmpty(),
				Contact = settings.ContactEmailAddress.NullEmpty()
			};

			PublishModelPartCreatedEvent<ContactDataSettings>(settings, contact);

			return contact;
		}

		#endregion

		#region Generic model part handlers

		protected virtual void MergeModelBag(IModelPart part, IDictionary<string, object> model, MessageContext messageContext)
		{
			if (!(model.Get("Bag") is IDictionary<string, object> bag))
			{
				model["Bag"] = bag = new Dictionary<string, object>();
			}

			var source = part as IDictionary<string, object>;
			bag.Merge(source);
		}

		#endregion

		#region Entity specific model part handlers

		protected virtual object CreateModelPart(Store part, MessageContext messageContext)
		{
			Guard.NotNull(messageContext, nameof(messageContext));
			Guard.NotNull(part, nameof(part));

			var host = messageContext.BaseUri.ToString();
			var logoInfo = _services.PictureService.GetPictureInfo(messageContext.Store.LogoPictureId);

			// Issue: https://github.com/smartstoreag/SmartStoreNET/issues/1321

			var m = new Dictionary<string, object>
			{
				{ "Email", messageContext.EmailAccount.Email },
				{ "EmailName", messageContext.EmailAccount.DisplayName },
				{ "Name", part.Name },
				{ "Url", host },
				{ "Cdn", part.ContentDeliveryNetwork },
				{ "PrimaryStoreCurrency", part.PrimaryStoreCurrency?.CurrencyCode },
				{ "PrimaryExchangeRateCurrency", part.PrimaryExchangeRateCurrency?.CurrencyCode },
				{ "Logo", CreateModelPart(logoInfo, messageContext, host, null, new Size(400, 75)) },
				{ "Company", CreateCompanyModelPart(messageContext) },
				{ "Contact", CreateContactModelPart(messageContext) },
				{ "Bank", CreateBankModelPart(messageContext) },
				{ "Copyright", T("Content.CopyrightNotice", messageContext.Language.Id, DateTime.Now.Year.ToString(), part.Name).Text }
			};

			var he = new HybridExpando(true);
			he.Merge(m, true);

			PublishModelPartCreatedEvent<Store>(part, he);

			return he;
		}

		protected virtual object CreateModelPart(PictureInfo part, MessageContext messageContext, 
			string href, 
			int? targetSize = null, 
			Size? clientMaxSize = null,
			string alt = null)
		{
			Guard.NotNull(messageContext, nameof(messageContext));
			Guard.NotEmpty(href, nameof(href));

			if (part == null)
				return null;

			var width = part.Width;
			var height = part.Height;

			if (width.HasValue && height.HasValue && (targetSize.HasValue || clientMaxSize.HasValue))
			{
				var maxSize = clientMaxSize ?? new Size(targetSize.Value, targetSize.Value);
				var size = ImagingHelper.Rescale(new Size(width.Value, height.Value), maxSize);
				width = size.Width;
				height = size.Height;
			}

			var m = new
			{
				Src = _services.PictureService.GetUrl(part, targetSize.GetValueOrDefault(), FallbackPictureType.NoFallback, messageContext.BaseUri.ToString()),
				Href = href,
				Width = width,
				Height = height,
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
			var url = BuildUrl(productUrlHelper.GetProductUrl(part.Id, part.GetSeName(messageContext.Language.Id), attributesXml), messageContext);
			var pictureInfo = GetPictureFor(part, attributesXml);
			var name = part.GetLocalized(x => x.Name, messageContext.Language.Id).Value;
			var alt = T("Media.Product.ImageAlternateTextFormat", messageContext.Language.Id, name).Text;
			
			var m = new Dictionary<string, object>
			{
				{ "Id", part.Id },
				{ "Sku", catalogSettings.ShowProductSku ? part.Sku : null },
				{ "Name", name },
				{ "Description", part.GetLocalized(x => x.ShortDescription, messageContext.Language).Value.NullEmpty() },
				{ "StockQuantity", part.StockQuantity },
				{ "AdditionalShippingCharge", additionalShippingChargeFormatted.NullEmpty() },
				{ "Url", url },
				{ "Thumbnail", CreateModelPart(pictureInfo, messageContext, url, mediaSettings.MessageProductThumbPictureSize, new Size(50, 50), alt) },
				{ "ThumbnailLg", CreateModelPart(pictureInfo, messageContext, url, mediaSettings.ProductThumbPictureSize, new Size(120, 120), alt) },
				{ "DeliveryTime", null },
				{ "QtyUnit", null }
			};

			if (shoppingCartSettings.ShowDeliveryTimes && part.IsShipEnabled)
			{
				if (deliveryTimeService.GetDeliveryTime(part) is DeliveryTime dt)
				{
					m["DeliveryTime"] = new Dictionary<string, object>
					{
						{ "Color", dt.ColorHexValue },
						{ "Name", dt.GetLocalized(x => x.Name, messageContext.Language).Value },
					};
				}
			}

			if (quantityUnitService.GetQuantityUnitById(part.QuantityUnitId) is QuantityUnit qu)
			{
				m["QtyUnit"] = qu.GetLocalized(x => x.Name, messageContext.Language).Value;
			}

			PublishModelPartCreatedEvent<Product>(part, m);

			return m;
		}

		protected virtual object CreateModelPart(Customer part, MessageContext messageContext)
		{
			Guard.NotNull(messageContext, nameof(messageContext));
			Guard.NotNull(part, nameof(part));

			var email = part.FindEmail();
			var pwdRecoveryToken = part.GetAttribute<string>(SystemCustomerAttributeNames.PasswordRecoveryToken).NullEmpty();
			var accountActivationToken = part.GetAttribute<string>(SystemCustomerAttributeNames.AccountActivationToken).NullEmpty();

			int rewardPointsBalance = part.GetRewardPointsBalance();
			decimal rewardPointsAmountBase = _services.Resolve<IOrderTotalCalculationService>().ConvertRewardPointsToAmount(rewardPointsBalance);
			decimal rewardPointsAmount = _services.Resolve<ICurrencyService>().ConvertFromPrimaryStoreCurrency(rewardPointsAmountBase, _services.WorkContext.WorkingCurrency);

			var m = new Dictionary<string, object>
			{
				["Id"] = part.Id,
				["CustomerGuid"] = part.CustomerGuid,
				["Username"] = part.Username,
				["Email"] = email,
				["IsTaxExempt"] = part.IsTaxExempt,
				["LastIpAddress"] = part.LastIpAddress,
				["CreatedOn"] = ToUserDate(part.CreatedOnUtc, messageContext),
				["LastLoginOn"] = ToUserDate(part.LastLoginDateUtc, messageContext),
				["LastActivityOn"] = ToUserDate(part.LastActivityDateUtc, messageContext),

				["FullName"] = GetDisplayNameForCustomer(part).NullEmpty(),
				["VatNumber"] = part.GetAttribute<string>(SystemCustomerAttributeNames.VatNumber).NullEmpty(),
				["VatNumberStatus"] = part.GetAttribute<VatNumberStatus>(SystemCustomerAttributeNames.VatNumberStatusId).GetLocalizedEnum(_services.Localization, messageContext.Language.Id).NullEmpty(),
				["CustomerNumber"] = part.GetAttribute<string>(SystemCustomerAttributeNames.CustomerNumber).NullEmpty(),
				["IsRegistered"] = part.IsRegistered(),

				// URLs
				["WishlistUrl"] = BuildRouteUrl("Wishlist", new { customerGuid = part.CustomerGuid }, messageContext),
				["EditUrl"] = BuildActionUrl("Edit", "Customer", new { id = part.Id, area = "admin" }, messageContext),
				["PasswordRecoveryURL"] = pwdRecoveryToken == null ? null : BuildActionUrl("passwordrecoveryconfirm", "customer",
					new { token = part.GetAttribute<string>(SystemCustomerAttributeNames.PasswordRecoveryToken), email, area = "" },
					messageContext),
				["AccountActivationURL"] = accountActivationToken == null ? null : BuildActionUrl("activation", "customer",
					new { token = part.GetAttribute<string>(SystemCustomerAttributeNames.AccountActivationToken), email, area = "" },
					messageContext),

				// Addresses
				["BillingAddress"] = CreateModelPart(part.BillingAddress ?? new Address(), messageContext),
				["ShippingAddress"] = part.ShippingAddress == null ? null : CreateModelPart(part.ShippingAddress, messageContext),		

				// Reward Points
				["RewardPointsAmount"] = rewardPointsAmount,
				["RewardPointsBalance"] = _services.Resolve<IPriceFormatter>().FormatPrice(rewardPointsAmount, true, false),
				["RewardPointsHistory"] = part.RewardPointsHistory.Count == 0 ? null : part.RewardPointsHistory.Select(x => CreateModelPart(x, messageContext)).ToList(),
			};

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
				{ "SenderName", part.SenderName.NullEmpty() },
				{ "SenderEmail", part.SenderEmail.NullEmpty() },
				{ "RecipientName", part.RecipientName.NullEmpty() },
				{ "RecipientEmail", part.RecipientEmail.NullEmpty() },
				{ "Amount", _services.Resolve<IPriceFormatter>().FormatPrice(part.Amount, true, false) },
				{ "CouponCode", part.GiftCardCouponCode.NullEmpty() }
			};

			// Message
			var message = (string)null;
			if (part.Message.HasValue())
			{
				message = HtmlUtils.FormatText(part.Message, true, false, false, false, false, false);
			}
			m["Message"] = message;

			// RemainingAmount
			var remainingAmount = (string)null;
			var order = part?.PurchasedWithOrderItem?.Order;
			if (order != null)
			{
				var amount = _services.Resolve<ICurrencyService>().ConvertCurrency(part.GetGiftCardRemainingAmount(), order.CurrencyRate);
				remainingAmount = _services.Resolve<IPriceFormatter>().FormatPrice(amount, true, false);
			}
			m["RemainingAmount"] = remainingAmount;

			PublishModelPartCreatedEvent<GiftCard>(part, m);

			return m;
		}

		protected virtual object CreateModelPart(NewsLetterSubscription part, MessageContext messageContext)
		{
			Guard.NotNull(messageContext, nameof(messageContext));
			Guard.NotNull(part, nameof(part));

			var gid = part.NewsLetterSubscriptionGuid;

			var m = new Dictionary<string, object>
			{
				{ "Id", part.Id },
				{ "Email", part.Email.NullEmpty() },
				{ "ActivationUrl", gid == Guid.Empty ? null : BuildRouteUrl("NewsletterActivation", new { token = part.NewsLetterSubscriptionGuid, active = true }, messageContext) },
				{ "DeactivationUrl", gid == Guid.Empty ? null : BuildRouteUrl("NewsletterActivation", new { token = part.NewsLetterSubscriptionGuid, active = false }, messageContext) }
			};

			var customer = messageContext.Customer;
			if (customer != null && customer.Email.IsCaseInsensitiveEqual(part.Email.EmptyNull()))
			{
				// Set FullName only if a customer account exists for the subscriber's email address.
				m["FullName"] = customer.GetFullName();
			}

			PublishModelPartCreatedEvent<NewsLetterSubscription>(part, m);

			return m;
		}

		protected virtual object CreateModelPart(Campaign part, MessageContext messageContext)
		{
			Guard.NotNull(messageContext, nameof(messageContext));
			Guard.NotNull(part, nameof(part));

			var protocol = messageContext.BaseUri.Scheme;
			var host = messageContext.BaseUri.Authority;
			var body = HtmlUtils.RelativizeFontSizes(part.Body.EmptyNull());

			// We must render the body separately
			body = _templateEngine.Render(body, messageContext.Model, messageContext.FormatProvider);

			var m = new Dictionary<string, object>
			{
				{ "Id", part.Id },
				{ "Subject", part.Subject.NullEmpty() },
				{ "Body", WebHelper.MakeAllUrlsAbsolute(body, protocol, host).NullEmpty() },
				{ "CreatedOn", ToUserDate(part.CreatedOnUtc, messageContext) }
			};
			
			PublishModelPartCreatedEvent<Campaign>(part, m);

			return m;
		}

		protected virtual object CreateModelPart(ProductReview part, MessageContext messageContext)
		{
			Guard.NotNull(messageContext, nameof(messageContext));
			Guard.NotNull(part, nameof(part));

			var m = new Dictionary<string, object>
			{
				{ "Title", part.Title.NullEmpty() },
				{ "Text", HtmlUtils.FormatText(part.ReviewText, true, false, false, false, false, false).NullEmpty() },
				{ "Rating", part.Rating }
			};

			PublishModelPartCreatedEvent<ProductReview>(part, m);

			return m;
		}

		protected virtual object CreateModelPart(ProductReviewHelpfulness part, MessageContext messageContext)
		{
			Guard.NotNull(messageContext, nameof(messageContext));
			Guard.NotNull(part, nameof(part));

			var m = new Dictionary<string, object>
			{
				{ "ProductReviewId", part.ProductReviewId },
				{ "ReviewTitle", part.ProductReview.Title },
				{ "WasHelpful", part.WasHelpful }
			};

			ApplyCustomerContentPart(m, part, messageContext);

			PublishModelPartCreatedEvent<ProductReviewHelpfulness>(part, m);

			return m;
		}

		protected virtual object CreateModelPart(PollVotingRecord part, MessageContext messageContext)
		{
			Guard.NotNull(messageContext, nameof(messageContext));
			Guard.NotNull(part, nameof(part));

			var m = new Dictionary<string, object>
			{
				{ "PollAnswerId", part.PollAnswerId },
				{ "PollAnswerName", part.PollAnswer.Name },
				{ "PollId", part.PollAnswer.PollId }
			};

			ApplyCustomerContentPart(m, part, messageContext);

			PublishModelPartCreatedEvent<PollVotingRecord>(part, m);

			return m;
		}

		protected virtual object CreateModelPart(PrivateMessage part, MessageContext messageContext)
		{
			Guard.NotNull(messageContext, nameof(messageContext));
			Guard.NotNull(part, nameof(part));

			var m = new Dictionary<string, object>
			{
				{  "Subject", part.Subject.NullEmpty() },
				{  "Text", part.FormatPrivateMessageText().NullEmpty() },
				{  "FromEmail", part.FromCustomer?.FindEmail().NullEmpty() },
				{  "ToEmail", part.ToCustomer?.FindEmail().NullEmpty() },
				{  "FromName", part.FromCustomer?.GetFullName().NullEmpty() },
				{  "ToName", part.ToCustomer?.GetFullName().NullEmpty() },
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
				{  "PostTitle", part.BlogPost.Title.NullEmpty() },
				{  "PostUrl", BuildRouteUrl("BlogPost", new { SeName = part.BlogPost.GetSeName(part.BlogPost.LanguageId, ensureTwoPublishedLanguages: false) }, messageContext) },
				{  "Text", part.CommentText.NullEmpty() }
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
				{  "NewsTitle", part.NewsItem.Title.NullEmpty() },
				{  "Title", part.CommentTitle.NullEmpty() },
				{  "Text", HtmlUtils.FormatText(part.CommentText, true, false, false, false, false, false).NullEmpty() },
				{  "NewsUrl", BuildRouteUrl("NewsItem", new { SeName = part.NewsItem.GetSeName(messageContext.Language.Id) }, messageContext) }
			};

			PublishModelPartCreatedEvent<NewsComment>(part, m);

			return m;
		}

		protected virtual object CreateModelPart(ForumTopic part, MessageContext messageContext)
		{
			Guard.NotNull(messageContext, nameof(messageContext));
			Guard.NotNull(part, nameof(part));

			var pageIndex = messageContext.Model.GetFromBag<int>("TopicPageIndex");

			var url = pageIndex > 0 ?
				BuildRouteUrl("TopicSlugPaged", new { id = part.Id, slug = part.GetSeName(), page = pageIndex }, messageContext) :
				BuildRouteUrl("TopicSlug", new { id = part.Id, slug = part.GetSeName() }, messageContext);

			var m = new Dictionary<string, object>
			{
				{ "Subject", part.Subject.NullEmpty() },
				{ "NumReplies", part.NumReplies },
				{ "NumPosts", part.NumPosts },
				{ "NumViews", part.Views },
				{ "Body", part.GetFirstPost(_services.Resolve<IForumService>())?.FormatPostText().NullEmpty() },
				{ "Url", url },
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
				{ "Author", part.Customer.FormatUserName().NullEmpty() },
				{ "Body", part.FormatPostText().NullEmpty() }
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
				{ "Name", part.GetLocalized(x => x.Name, messageContext.Language).Value.NullEmpty() },
				{ "GroupName", part.ForumGroup?.GetLocalized(x => x.Name, messageContext.Language)?.Value.NullEmpty() },
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

			var settings = _services.Resolve<AddressSettings>();
			var languageId = messageContext.Language?.Id ?? messageContext.LanguageId;

			var salutation = part.Salutation.NullEmpty();
			var title = part.Title.NullEmpty();
			var company = settings.CompanyEnabled ? part.Company : null;
			var firstName = part.FirstName.NullEmpty();
			var lastName = part.LastName.NullEmpty();
			var street1 = settings.StreetAddressEnabled ? part.Address1 : null;
			var street2 = settings.StreetAddress2Enabled ? part.Address2 : null;
			var zip = settings.ZipPostalCodeEnabled ? part.ZipPostalCode : null;
			var city = settings.CityEnabled ? part.City : null;
			var country = settings.CountryEnabled ? part.Country?.GetLocalized(x => x.Name, languageId ?? 0)?.Value.NullEmpty() : null;
			var state = settings.StateProvinceEnabled ? part.StateProvince?.GetLocalized(x => x.Name, languageId ?? 0)?.Value.NullEmpty() : null;
			
			var m = new Dictionary<string, object>
			{
				{ "Title", title },
				{ "Salutation", salutation },
				{ "FullSalutation", part.GetFullSalutaion().NullEmpty() },
				{ "FullName", part.GetFullName(false).NullEmpty() },
				{ "Company", company },
				{ "FirstName", firstName },
				{ "LastName", lastName },
				{ "Street1", street1 },
				{ "Street2", street2 },
				{ "Country", country },
				{ "CountryId", part.Country?.Id },
				{ "CountryAbbrev2", settings.CountryEnabled ? part.Country?.TwoLetterIsoCode.NullEmpty() : null },
				{ "CountryAbbrev3", settings.CountryEnabled ? part.Country?.ThreeLetterIsoCode.NullEmpty() : null },
				{ "State", state },
				{ "StateAbbrev", settings.StateProvinceEnabled ? part.StateProvince?.Abbreviation.NullEmpty() : null },
				{ "City", city },
				{ "ZipCode", zip },
				{ "Email", part.Email.NullEmpty() },
				{ "Phone", settings.PhoneEnabled ? part.PhoneNumber : null },
				{ "Fax", settings.FaxEnabled ? part.FaxNumber : null }
			};

			m["NameLine"] = Concat(salutation, title, firstName, lastName);
			m["StreetLine"] = Concat(street1, street2);
			m["CityLine"] = Concat(zip, city);
			m["CountryLine"] = Concat(country, state);

			PublishModelPartCreatedEvent<Address>(part, m);

			return m;
		}

		protected virtual object CreateModelPart(RewardPointsHistory part, MessageContext messageContext)
		{
			Guard.NotNull(messageContext, nameof(messageContext));
			Guard.NotNull(part, nameof(part));
			
			var m = new Dictionary<string, object>
			{
				{ "Id", part.Id },
				{ "CreatedOn", ToUserDate(part.CreatedOnUtc, messageContext) },
				{ "Message", part.Message.NullEmpty() },
				{ "Points", part.Points },
				{ "PointsBalance", part.PointsBalance },
				{ "UsedAmount", part.UsedAmount }
			};

			PublishModelPartCreatedEvent<RewardPointsHistory>(part, m);

			return m;
		}

		protected virtual object CreateModelPart(IEnumerable<GenericAttribute> part, MessageContext messageContext)
		{
			Guard.NotNull(messageContext, nameof(messageContext));
			Guard.NotNull(part, nameof(part));

			var m = new Dictionary<string, object>();

			foreach (var attr in part)
			{
				m[attr.Key] = attr.Value;
			}

			PublishModelPartCreatedEvent<IEnumerable<GenericAttribute>>(part, m);

			return m;
		}

		protected virtual object CreateModelPart(ForumSubscription part, MessageContext messageContext)
		{
			Guard.NotNull(messageContext, nameof(messageContext));
			Guard.NotNull(part, nameof(part));

			var m = new Dictionary<string, object>
			{
				{  "SubscriptionGuid", part.SubscriptionGuid },
				{  "CustomerId",  part.CustomerId },
				{  "ForumId",  part.ForumId },
				{  "TopicId",  part.TopicId },
				{  "CreatedOn", ToUserDate(part.CreatedOnUtc, messageContext) }
			};

			PublishModelPartCreatedEvent<ForumSubscription>(part, m);

			return m;
		}

		protected virtual object CreateModelPart(BackInStockSubscription part, MessageContext messageContext)
		{
			Guard.NotNull(messageContext, nameof(messageContext));
			Guard.NotNull(part, nameof(part));

			var m = new Dictionary<string, object>
			{
				{  "StoreId", part.StoreId },
				{  "CustomerId",  part.CustomerId },
				{  "ProductId",  part.ProductId },
				{  "CreatedOn", ToUserDate(part.CreatedOnUtc, messageContext) }
			};

			PublishModelPartCreatedEvent<BackInStockSubscription>(part, m);

			return m;
		}

		#endregion

		#region Model Tree

		public TreeNode<ModelTreeMember> GetLastModelTree(string messageTemplateName)
		{
			Guard.NotEmpty(messageTemplateName, nameof(messageTemplateName));

			var template = _messageTemplateService.GetMessageTemplateByName(messageTemplateName, _services.StoreContext.CurrentStore.Id);

			if (template != null)
			{
				return GetLastModelTree(template);
			}

			return null;
		}

		public TreeNode<ModelTreeMember> GetLastModelTree(MessageTemplate template)
		{
			Guard.NotNull(template, nameof(template));

			if (template.LastModelTree.IsEmpty())
			{
				return null;
			}

			return Newtonsoft.Json.JsonConvert.DeserializeObject<TreeNode<ModelTreeMember>>(template.LastModelTree);
		}

		public TreeNode<ModelTreeMember> BuildModelTree(TemplateModel model)
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
