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

namespace SmartStore.Services.Customers
{
	public partial class GdprTool : IGdprTool
	{
		private readonly IMessageModelProvider _messageModelProvider;
		private readonly IGenericAttributeService _genericAttributeService;
		private readonly IForumService _forumService;
		private readonly IBackInStockSubscriptionService _backInStockSubscriptionService;
		private readonly ICommonServices _services;

		const string AnonymousEmail = "anonymous@example.com";

		public GdprTool(
			IMessageModelProvider messageModelProvider,
			IGenericAttributeService genericAttributeService,
			IForumService forumService,
			IBackInStockSubscriptionService backInStockSubscriptionService,
			ICommonServices services)
		{
			_messageModelProvider = messageModelProvider;
			_genericAttributeService = genericAttributeService;
			_forumService = forumService;
			_backInStockSubscriptionService = backInStockSubscriptionService;
			_services = services;
		}

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

		public virtual void AnonymizeCustomer(Customer customer, bool deleteContent)
		{
			Guard.NotNull(customer, nameof(customer));
		}

		protected string AnonymizeIpAddress(string ipAddress)
		{
			return ipAddress;
		}
	}
}
