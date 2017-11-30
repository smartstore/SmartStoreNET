using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Dynamic;
using SmartStore.ComponentModel;
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
using SmartStore.Services.Orders;
using SmartStore.Services.Localization;
using SmartStore.Core.Html;
using SmartStore.Core.Plugins;
using SmartStore.Services.Common;
using SmartStore.Services.Customers;
using SmartStore.Core;

namespace SmartStore.Services.Messages
{
	public partial class MessageModelProvider : IMessageModelProvider
	{
		private readonly ICommonServices _services;
		private readonly IEmailAccountService _emailAccountService;
		private readonly MediaSettings _mediaSettings;
		private readonly ContactDataSettings _contactDataSettings;
		private readonly MessageTemplatesSettings _templatesSettings;
		private readonly CatalogSettings _catalogSettings;
		private readonly TaxSettings _taxSettings;
		private readonly CompanyInformationSettings _companyInfoSettings;
		private readonly BankConnectionSettings _bankConnectionSettings;
		private readonly ShoppingCartSettings _shoppingCartSettings;
		private readonly SecuritySettings _securitySettings;

		public MessageModelProvider(
			ICommonServices services,
			IEmailAccountService emailAccountService,
			MediaSettings mediaSettings,
			ContactDataSettings contactDataSettings,
			MessageTemplatesSettings templatesSettings,
			CatalogSettings catalogSettings,
			TaxSettings taxSettings,
			CompanyInformationSettings companyInfoSettings,
			BankConnectionSettings bankConnectionSettings,
			ShoppingCartSettings shoppingCartSettings,
			SecuritySettings securitySettings)
		{
			_services = services;
			_emailAccountService = emailAccountService;
			_mediaSettings = mediaSettings;
			_contactDataSettings = contactDataSettings;
			_templatesSettings = templatesSettings;
			_catalogSettings = catalogSettings;
			_taxSettings = taxSettings;
			_companyInfoSettings = companyInfoSettings;
			_bankConnectionSettings = bankConnectionSettings;
			_shoppingCartSettings = shoppingCartSettings;
			_securitySettings = securitySettings;
		}

		public virtual void AddGlobalModelParts(MessageContext messageContext, IDictionary<string, object> model)
		{
			Guard.NotNull(model, nameof(model));

			model["Store"] = CreateModelPart(messageContext.Store, messageContext);
			model["Company"] = CreateCompanyModelPart();
			model["Contact"] = CreateContactModelPart();
			model["Bank"] = CreateBankModelPart();
		}

		public virtual void AddModelPart(object part, MessageContext messageContext, IDictionary<string, object> model, string name = null)
		{
			Guard.NotNull(part, nameof(part));
			Guard.NotNull(messageContext, nameof(messageContext));
			Guard.NotNull(model, nameof(model));
			
			name = name.NullEmpty() ?? ResolvePartName(part);

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
					name = "Forum";
					modelPart = CreateModelPart(x, messageContext);
					break;
				case ForumPost x:
					name = "Forum";
					modelPart = CreateModelPart(x, messageContext);
					break;
				case Forum x:
					name = "Forum";
					modelPart = CreateModelPart(x, messageContext);
					break;
				case PrivateMessage x:
					modelPart = CreateModelPart(x, messageContext);
					break;
				case BackInStockSubscription x:
					modelPart = CreateModelPart(x, messageContext);
					break;
				default:
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
							name = ResolvePartName(evt.Result) ?? name;
						}
					}
					else
					{
						modelPart = part;
					}

					modelPart = evt.Result ?? part;
					name = evt.Name.NullEmpty() ?? name;
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

		#region Global model part handlers

		protected virtual object CreateCompanyModelPart()
		{
			var he = new HybridExpando(_companyInfoSettings);
			PublishModelPartCreatedEvent<CompanyInformationSettings>(_companyInfoSettings, he);
			return he;
		}

		protected virtual object CreateBankModelPart()
		{
			var he = new HybridExpando(_bankConnectionSettings);
			PublishModelPartCreatedEvent<BankConnectionSettings>(_bankConnectionSettings, he);
			return he;
		}

		protected virtual object CreateContactModelPart()
		{
			var contact = new HybridExpando(_contactDataSettings) as dynamic;

			// TODO: (mc) Liquid > Use following aliases in Partials
			// Aliases
			contact.Phone = new
			{
				Company = _contactDataSettings.CompanyTelephoneNumber,
				Hotline = _contactDataSettings.HotlineTelephoneNumber,
				Mobile = _contactDataSettings.MobileTelephoneNumber,
				Fax = _contactDataSettings.CompanyFaxNumber
			};

			contact.Email = new
			{
				Company = _contactDataSettings.CompanyEmailAddress,
				Webmaster = _contactDataSettings.WebmasterEmailAddress,
				Support = _contactDataSettings.SupportEmailAddress,
				Contact = _contactDataSettings.ContactEmailAddress
			};

			PublishModelPartCreatedEvent<ContactDataSettings>(_contactDataSettings, contact);

			return contact;
		}

		#endregion

		#region Entity specific model part handlers

		protected virtual object CreateModelPart(Store part, MessageContext messageContext)
		{
			Guard.NotNull(messageContext, nameof(messageContext));
			Guard.NotNull(part, nameof(part));
			
			var he = new HybridExpando(part);
			he["Email"] = _emailAccountService.GetDefaultEmailAccount()?.Email;

			var host = _services.StoreService.GetHost(messageContext.Store);
			he["URL"] = host;
			he.Override(nameof(part.Url), host);

			// TODO: (mc) Liquid > Use in templates
			var logoInfo = _services.PictureService.GetPictureInfo(messageContext.Store.LogoPictureId);
			he["Logo"] = new
			{
				Src = _services.PictureService.GetUrl(logoInfo, 0, FallbackPictureType.NoFallback, host),
				Href = host,
				Width = logoInfo?.Width,
				Height = logoInfo?.Height
			};

			// TODO: (mc) Liquid > GetSupplierIdentification() as Partial

			PublishModelPartCreatedEvent<Store>(part, he);

			return he;
		}

		protected virtual object CreateModelPart(Order part, MessageContext messageContext)
		{
			Guard.NotNull(messageContext, nameof(messageContext));
			Guard.NotNull(part, nameof(part));

			return null;
		}

		protected virtual object CreateModelPart(Product part, MessageContext messageContext)
		{
			Guard.NotNull(messageContext, nameof(messageContext));
			Guard.NotNull(part, nameof(part));

			return null;
		}

		protected virtual object CreateModelPart(Customer part, MessageContext messageContext)
		{
			Guard.NotNull(messageContext, nameof(messageContext));
			Guard.NotNull(part, nameof(part));

			var he = new HybridExpando(part);

			he.Override("Email", part.FindEmail());
			he["FullName"] = GetDisplayNameForCustomer(part);

			// [...]

			PublishModelPartCreatedEvent<Customer>(part, he);

			return he;
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

			var he = new HybridExpando(part);
			he["ProductName"] = orderItem?.Product?.Name; // TODO: Liquid > Product.Name > ProductName
			he["Reason"] = part.ReasonForReturn;
			he["Status"] = part.ReturnRequestStatus.GetLocalizedEnum(_services.Localization, _services.WorkContext);

			// TODO: Liquid > WTF?
			he.Override(nameof(part.CustomerComments), HtmlUtils.FormatText(part.CustomerComments, false, true, false, false, false, false));
			he.Override(nameof(part.StaffNotes), HtmlUtils.FormatText(part.StaffNotes, false, true, false, false, false, false));

			PublishModelPartCreatedEvent<ReturnRequest>(part, he);

			return he;
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

			return null;
		}

		protected virtual object CreateModelPart(PrivateMessage part, MessageContext messageContext)
		{
			Guard.NotNull(messageContext, nameof(messageContext));
			Guard.NotNull(part, nameof(part));

			return null;
		}

		protected virtual object CreateModelPart(BlogComment part, MessageContext messageContext)
		{
			Guard.NotNull(messageContext, nameof(messageContext));
			Guard.NotNull(part, nameof(part));

			return null;
		}

		protected virtual object CreateModelPart(NewsComment part, MessageContext messageContext)
		{
			Guard.NotNull(messageContext, nameof(messageContext));
			Guard.NotNull(part, nameof(part));

			return null;
		}

		protected virtual object CreateModelPart(ForumTopic part, MessageContext messageContext)
		{
			Guard.NotNull(messageContext, nameof(messageContext));
			Guard.NotNull(part, nameof(part));

			return null;
		}

		protected virtual object CreateModelPart(ForumPost part, MessageContext messageContext)
		{
			Guard.NotNull(messageContext, nameof(messageContext));
			Guard.NotNull(part, nameof(part));

			return null;
		}

		protected virtual object CreateModelPart(Forum part, MessageContext messageContext)
		{
			Guard.NotNull(messageContext, nameof(messageContext));
			Guard.NotNull(part, nameof(part));

			return null;
		}

		protected virtual object CreateModelPart(BackInStockSubscription part, MessageContext messageContext)
		{
			Guard.NotNull(messageContext, nameof(messageContext));
			Guard.NotNull(part, nameof(part));

			return null;
		}

		#endregion

		#region Utils

		private void PublishModelPartCreatedEvent<T>(T source, dynamic part) where T : class
		{
			_services.EventPublisher.Publish(new MessageModelPartCreatedEvent<T>(source, part));
		}

		private string ResolvePartName(object part)
		{
			string name = null;
			var type = part.GetType();

			try
			{
				if (part is BaseEntity be)
				{
					name = be.GetUnproxiedType().Name;
				}
				else if (part is IDictionary<string, object> d)
				{
					name = d.Get("__Name") as string;
				}
				else if (part is IDynamicMetaObjectProvider x)
				{
					name = ((dynamic)part).__Name as string;
				}
				else if (type.IsAnonymous())
				{
					var prop = FastProperty.GetProperty(type, "__Name", PropertyCachingStrategy.EagerCached);
					if (prop != null)
					{
						name = prop.GetValue(part) as string;
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

		private string GetDisplayNameForCustomer(Customer customer)
		{
			return customer.GetFullName().NullEmpty() ?? customer?.Username;
		}

		#endregion
	}
}
