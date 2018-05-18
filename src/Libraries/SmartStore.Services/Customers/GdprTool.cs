using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Domain.Customers;
using SmartStore.Services.Messages;
using SmartStore.Services.Common;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.News;
using SmartStore.Core.Domain.Blogs;
using SmartStore.Core.Domain.Polls;
using SmartStore.Services.Catalog;
using SmartStore.Services.Forums;
using System.Linq.Expressions;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Common;
using SmartStore.Services.Orders;
using SmartStore.Core.Localization;
using SmartStore.Core.Domain.Localization;
using System.Globalization;

namespace SmartStore.Services.Customers
{
	public enum IdentifierDataType
	{
		Text,
		LongText,
		Name,
		UserName,
		EmailAddress,
		Url,
		IpAddress,
		PhoneNumber,
		Address,
		PostalCode,
		DateTime
	}

	public partial class GdprTool : IGdprTool
	{
		private readonly IMessageModelProvider _messageModelProvider;
		private readonly IGenericAttributeService _genericAttributeService;
		private readonly IShoppingCartService _shoppingCartService;
		private readonly IForumService _forumService;
		private readonly IBackInStockSubscriptionService _backInStockSubscriptionService;
		private readonly ICommonServices _services;

		const string AnonymousEmail = "anonymous@example.com";

		public GdprTool(
			IMessageModelProvider messageModelProvider,
			IGenericAttributeService genericAttributeService,
			IShoppingCartService shoppingCartService,
			IForumService forumService,
			IBackInStockSubscriptionService backInStockSubscriptionService,
			ICommonServices services)
		{
			_messageModelProvider = messageModelProvider;
			_genericAttributeService = genericAttributeService;
			_shoppingCartService = shoppingCartService;
			_forumService = forumService;
			_backInStockSubscriptionService = backInStockSubscriptionService;
			_services = services;

			T = NullLocalizer.InstanceEx;
		}

		public LocalizerEx T { get; set; }

		public virtual IDictionary<string, object> ExportCustomer(Customer customer)
		{
			Guard.NotNull(customer, nameof(customer));

			var ignoreMemberNames = new string[] 
			{
				"WishlistUrl", "EditUrl", "PasswordRecoveryURL",
				"BillingAddress.NameLine", "BillingAddress.StreetLine", "BillingAddress.CityLine", "BillingAddress.CountryLine",
				"ShippingAddress.NameLine", "ShippingAddress.StreetLine", "ShippingAddress.CityLine", "ShippingAddress.CountryLine"
			};

			var model = _messageModelProvider.CreateModelPart(customer, true, ignoreMemberNames) as IDictionary<string, object>;

			if (model != null)
			{
				// Roles
				model["CustomerRoles"] = customer.CustomerRoles.Select(x => x.Name).ToArray();
				
				// Generic attributes
				var attributes = _genericAttributeService.GetAttributesForEntity(customer.Id, "Customer");
				if (attributes.Any())
				{
					model["Attributes"] = _messageModelProvider.CreateModelPart(attributes, true);
				}

				// Order history
				var orders = customer.Orders.Where(x => !x.Deleted);
				if (orders.Any())
				{
					ignoreMemberNames = new string[]
					{
						"Disclaimer", "ConditionsOfUse", "Url", "CheckoutAttributes",
						"Items.DownloadUrl",
						"Items.Product.Description", "Items.Product.Url", "Items.Product.Thumbnail", "Items.Product.ThumbnailLg",
						"Items.BundleItems.Product.Description", "Items.BundleItems.Product.Url", "Items.BundleItems.Product.Thumbnail", "Items.BundleItems.Product.ThumbnailLg",
						"Billing.NameLine", "Billing.StreetLine", "Billing.CityLine", "Billing.CountryLine",
						"Shipping.NameLine", "Shipping.StreetLine", "Shipping.CityLine", "Shipping.CountryLine"
					};
					model["Orders"] = orders.Select(x => _messageModelProvider.CreateModelPart(x, true, ignoreMemberNames)).ToList();
				}

				// Return Request
				var returnRequests = customer.ReturnRequests;
				if (returnRequests.Any())
				{
					model["ReturnRequests"] = returnRequests.Select(x => _messageModelProvider.CreateModelPart(x, true, "Url")).ToList();
				}

				// Wallet
				var walletHistory = customer.WalletHistory;
				if (walletHistory.Any())
				{
					model["WalletHistory"] = walletHistory.Select(x => _messageModelProvider.CreateModelPart(x, true, "WalletUrl")).ToList();
				}
				
				// Forum topics
				var forumTopics = customer.ForumTopics;
				if (forumTopics.Any())
				{
					model["ForumTopics"] = forumTopics.Select(x => _messageModelProvider.CreateModelPart(x, true, "Url")).ToList();
				}

				// Forum posts
				var forumPosts = customer.ForumPosts;
				if (forumPosts.Any())
				{
					model["ForumPosts"] = forumPosts.Select(x => _messageModelProvider.CreateModelPart(x, true)).ToList();
				}

				// Product reviews
				var productReviews = customer.CustomerContent.OfType<ProductReview>();
				if (productReviews.Any())
				{
					model["ProductReviews"] = productReviews.Select(x => _messageModelProvider.CreateModelPart(x, true)).ToList();
				}

				// News comments
				var newsComments = customer.CustomerContent.OfType<NewsComment>();
				if (newsComments.Any())
				{
					model["NewsComments"] = newsComments.Select(x => _messageModelProvider.CreateModelPart(x, true)).ToList();
				}

				// Blog comments
				var blogComments = customer.CustomerContent.OfType<BlogComment>();
				if (blogComments.Any())
				{
					model["BlogComments"] = blogComments.Select(x => _messageModelProvider.CreateModelPart(x, true)).ToList();
				}

				// Product review helpfulness
				var helpfulness = customer.CustomerContent.OfType<ProductReviewHelpfulness>();
				if (helpfulness.Any())
				{
					ignoreMemberNames = new string[] { "CustomerId", "UpdatedOn" };
					model["ProductReviewHelpfulness"] = helpfulness.Select(x => _messageModelProvider.CreateModelPart(x, true, ignoreMemberNames)).ToList();
				}

				// Poll voting
				var pollVotings = customer.CustomerContent.OfType<PollVotingRecord>();
				if (pollVotings.Any())
				{
					ignoreMemberNames = new string[] { "CustomerId", "UpdatedOn" };
					model["PollVotings"] = pollVotings.Select(x => _messageModelProvider.CreateModelPart(x, true, ignoreMemberNames)).ToList();
				}

				// Forum subscriptions
				var forumSubscriptions = _forumService.GetAllSubscriptions(customer.Id, 0, 0, 0, int.MaxValue);
				if (forumSubscriptions.Any())
				{
					model["ForumSubscriptions"] = forumSubscriptions.Select(x => _messageModelProvider.CreateModelPart(x, true, "CustomerId")).ToList();
				}

				// BackInStock subscriptions
				var backInStockSubscriptions = _backInStockSubscriptionService.GetAllSubscriptionsByCustomerId(customer.Id, 0, 0, int.MaxValue);
				if (backInStockSubscriptions.Any())
				{
					model["BackInStockSubscriptions"] = backInStockSubscriptions.Select(x => _messageModelProvider.CreateModelPart(x, true, "CustomerId")).ToList();
				}

				// INFO: we're not going to export: 
				// - Private messages
				// - Activity log
				// It doesn't feel right and GDPR rules are not very clear about this. Let's wait and see :-)

				// Publish event to give plugin devs a chance to attach external data.
				_services.EventPublisher.Publish(new CustomerExportedEvent(customer, model));
			}

			return model;
		}

		public virtual void AnonymizeCustomer(Customer customer, bool pseudomyzeContent)
		{
			Guard.NotNull(customer, nameof(customer));

			using (var scope = new DbContextScope(_services.DbContext, autoCommit: false))
			{
				// Customer Data
				AnonymizeData(customer, x => x.Username, IdentifierDataType.UserName);
				AnonymizeData(customer, x => x.Email, IdentifierDataType.EmailAddress);
				AnonymizeData(customer, x => x.LastIpAddress, IdentifierDataType.IpAddress);
				if (pseudomyzeContent)
				{
					AnonymizeData(customer, x => x.AdminComment, IdentifierDataType.LongText);
					AnonymizeData(customer, x => x.LastLoginDateUtc, IdentifierDataType.DateTime);
					AnonymizeData(customer, x => x.LastActivityDateUtc, IdentifierDataType.DateTime);
				}

				// Generic attributes
				var attributes = _genericAttributeService.GetAttributesForEntity(customer.Id, "Customer");
				foreach (var attr in attributes)
				{
					// we don't need to mask generic attrs, we just delete them.
					_genericAttributeService.DeleteAttribute(attr);
				}

				// Addresses
				foreach (var address in customer.Addresses)
				{
					AnonymizeAddress(address);
				}

				// Delete shopping cart & wishlist (TBD: (mc) Really?!?)
				_shoppingCartService.DeleteExpiredShoppingCartItems(DateTime.UtcNow);

				// Delete forum subscriptions
				var forumSubscriptions = _forumService.GetAllSubscriptions(customer.Id, 0, 0, 0, int.MaxValue);
				foreach (var forumSub in forumSubscriptions)
				{
					_forumService.DeleteSubscription(forumSub);
				}

				// Delete BackInStock subscriptions
				var backInStockSubscriptions = _backInStockSubscriptionService.GetAllSubscriptionsByCustomerId(customer.Id, 0, 0, int.MaxValue);
				foreach (var stockSub in backInStockSubscriptions)
				{
					_backInStockSubscriptionService.DeleteSubscription(stockSub);
				}

				if (pseudomyzeContent)
				{
					// Private messages
					var privateMessages = _forumService.GetAllPrivateMessages(0, customer.Id, 0, null, null, null, null, 0, int.MaxValue);
					foreach (var msg in privateMessages)
					{
						AnonymizeData(msg, x => x.Subject, IdentifierDataType.Text);
						AnonymizeData(msg, x => x.Text, IdentifierDataType.LongText);
					}

					// Forum topics
					var forumTopic = customer.ForumTopics;
					foreach (var topic in forumTopic)
					{
						AnonymizeData(topic, x => x.Subject, IdentifierDataType.Text);
					}

					// Forum posts
					var forumPosts = customer.ForumPosts;
					foreach (var post in forumPosts)
					{
						AnonymizeData(post, x => x.IPAddress, IdentifierDataType.IpAddress);
						AnonymizeData(post, x => x.Text, IdentifierDataType.LongText);
					}

					// Customer Content
					var content = customer.CustomerContent;
					foreach (var item in content)
					{
						AnonymizeData(item, x => x.IpAddress, IdentifierDataType.IpAddress);

						switch (item)
						{
							case ProductReview c:
								AnonymizeData(c, x => x.ReviewText, IdentifierDataType.LongText);
								AnonymizeData(c, x => x.Title, IdentifierDataType.Text);
								break;
							case NewsComment c:
								AnonymizeData(c, x => x.CommentText, IdentifierDataType.LongText);
								AnonymizeData(c, x => x.CommentTitle, IdentifierDataType.Text);
								break;
							case BlogComment c:
								AnonymizeData(c, x => x.CommentText, IdentifierDataType.LongText);
								break;
						}
					}
				}

				// SAVE!!!
				scope.Commit();
			}
		}

		private void AnonymizeAddress(Address address)
		{
			AnonymizeData(address, x => x.Address1, IdentifierDataType.Address);
			AnonymizeData(address, x => x.Address2, IdentifierDataType.Address);
			AnonymizeData(address, x => x.City, IdentifierDataType.Address);
			AnonymizeData(address, x => x.Company, IdentifierDataType.Address);
			AnonymizeData(address, x => x.Email, IdentifierDataType.EmailAddress);
			AnonymizeData(address, x => x.FaxNumber, IdentifierDataType.PhoneNumber);
			AnonymizeData(address, x => x.FirstName, IdentifierDataType.Name);
			AnonymizeData(address, x => x.LastName, IdentifierDataType.Name);
			AnonymizeData(address, x => x.PhoneNumber, IdentifierDataType.PhoneNumber);
			AnonymizeData(address, x => x.ZipPostalCode, IdentifierDataType.PostalCode);
		}

		public virtual void AnonymizeData<T>(T entity, Expression<Func<T, object>> expression, IdentifierDataType type)
			where T : BaseEntity
		{
			Guard.NotNull(entity, nameof(entity));
			Guard.NotNull(expression, nameof(expression));

			var originalValue = expression.Compile().Invoke(entity);
			object maskedValue = null;

			if (originalValue is DateTime d)
			{
				maskedValue = DateTime.MinValue;
			}
			else if (originalValue is string s)
			{
				if (s.IsEmpty())
				{
					return;
				}

				Language language = null;
				var culture = CultureInfo.GetCultureInfo(language.LanguageCulture);
				//customerLanguage = _languageService.GetLanguageById(customer.GetAttribute<int>(SystemCustomerAttributeNames.LanguageId, processPaymentRequest.StoreId));
				//if (customerLanguage == null || !customerLanguage.Published)
				//{
				//	customerLanguage = _workContext.WorkingLanguage;
				//}

				switch (type)
				{
					case IdentifierDataType.DateTime:
						maskedValue = DateTime.MinValue.ToString(CultureInfo.InvariantCulture);
						break;
					case IdentifierDataType.EmailAddress:
						// TODO
						break;
					case IdentifierDataType.IpAddress:
						// TODO
						break;
					case IdentifierDataType.LongText:
						// TODO
						break;
					case IdentifierDataType.PhoneNumber:
						// TODO
						break;
					case IdentifierDataType.Text:
						// TODO
						break;
					case IdentifierDataType.Url:
						// TODO
						break;
					case IdentifierDataType.UserName:
						// TODO
						break;
					case IdentifierDataType.PostalCode:
						// TODO
						break;
				}
			}

			if (maskedValue != null)
			{
				var pi = expression.ExtractPropertyInfo();
				pi.SetValue(entity, maskedValue);
			}
		}
	}
}
