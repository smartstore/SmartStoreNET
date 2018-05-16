using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Common;
using SmartStore.Services.Messages;
using SmartStore.Services.Common;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.News;
using SmartStore.Core.Domain.Blogs;

namespace SmartStore.Services.Customers
{
	public partial class GdprTool : IGdprTool
	{
		private readonly IMessageModelProvider _messageModelProvider;
		private readonly IGenericAttributeService _genericAttributeService;

		public GdprTool(
			IMessageModelProvider messageModelProvider,
			IGenericAttributeService genericAttributeService)
		{
			_messageModelProvider = messageModelProvider;
			_genericAttributeService = genericAttributeService;
		}

		public IDictionary<string, object> ExportCustomer(Customer customer)
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

				// TODO: PollVotingRecord, ReviewHelpfulness
			}

			return model;
		}
	}
}
