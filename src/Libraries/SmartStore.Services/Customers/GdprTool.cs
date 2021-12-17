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
using SmartStore.Services.Customers;
using System.Linq.Expressions;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Common;
using SmartStore.Services.Orders;
using SmartStore.Core.Localization;
using SmartStore.Core.Domain.Localization;
using System.Globalization;
using SmartStore.Services.Localization;
using SmartStore.Utilities;
using System.Text;
using System.Net;
using System.Net.Sockets;
using SmartStore.Core.Logging;
using SmartStore.Core.Domain.Forums;

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
        private readonly ILanguageService _languageService;
        private readonly ICommonServices _services;

        public static DateTime MinDate = new DateTime(1900, 1, 1);

        const string AnonymousEmail = "anonymous@example.com";

        public GdprTool(
            IMessageModelProvider messageModelProvider,
            IGenericAttributeService genericAttributeService,
            IShoppingCartService shoppingCartService,
            IForumService forumService,
            IBackInStockSubscriptionService backInStockSubscriptionService,
            ILanguageService languageService,
            ICommonServices services)
        {
            _messageModelProvider = messageModelProvider;
            _genericAttributeService = genericAttributeService;
            _shoppingCartService = shoppingCartService;
            _forumService = forumService;
            _backInStockSubscriptionService = backInStockSubscriptionService;
            _languageService = languageService;
            _services = services;

            T = NullLocalizer.InstanceEx;
            Logger = NullLogger.Instance;
        }

        public LocalizerEx T { get; set; }

        public ILogger Logger { get; set; }

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
                model["CustomerRoles"] = customer.CustomerRoleMappings.Select(x => x.CustomerRole.Name).ToArray();

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

                // Forum post votes
                var forumPostVotes = customer.CustomerContent.OfType<ForumPostVote>();
                if (forumPostVotes.Any())
                {
                    ignoreMemberNames = new string[] { "CustomerId", "UpdatedOn" };
                    model["ForumPostVotes"] = forumPostVotes.Select(x => _messageModelProvider.CreateModelPart(x, true, ignoreMemberNames)).ToList();
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

            var language = customer.GetLanguage();
            var customerName = customer.GetFullName() ?? customer.Username ?? customer.FindEmail();

            using (var scope = new DbContextScope(_services.DbContext, autoCommit: false))
            {
                // Set to deleted
                customer.Deleted = true;

                // Unassign roles
                var customerService = _services.Resolve<ICustomerService>();
                var roleMappings = customer.CustomerRoleMappings.ToList();
                var guestRole = customerService.GetCustomerRoleBySystemName(SystemCustomerRoleNames.Guests);
                var insertGuestMapping = !roleMappings.Any(x => x.CustomerRoleId == guestRole.Id);

                roleMappings
                    .Where(x => x.CustomerRoleId != guestRole.Id)
                    .Each(x => customerService.DeleteCustomerRoleMapping(x));

                if (insertGuestMapping)
                {
                    customerService.InsertCustomerRoleMapping(new CustomerRoleMapping { CustomerId = customer.Id, CustomerRoleId = guestRole.Id });
                }

                // Delete shopping cart & wishlist (TBD: (mc) Really?!?)
                _shoppingCartService.DeleteExpiredShoppingCartItems(DateTime.UtcNow, customer.Id);

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

                // Generic attributes
                var attributes = _genericAttributeService.GetAttributesForEntity(customer.Id, "Customer");
                foreach (var attr in attributes)
                {
                    // we don't need to mask generic attrs, we just delete them.
                    _genericAttributeService.DeleteAttribute(attr);
                }

                // Customer Data
                AnonymizeData(customer, x => x.Username, IdentifierDataType.UserName, language);
                AnonymizeData(customer, x => x.Email, IdentifierDataType.EmailAddress, language);
                AnonymizeData(customer, x => x.LastIpAddress, IdentifierDataType.IpAddress, language);
                AnonymizeData(customer, x => x.FirstName, IdentifierDataType.Name, language);
                AnonymizeData(customer, x => x.LastName, IdentifierDataType.Name, language);
                AnonymizeData(customer, x => x.BirthDate, IdentifierDataType.DateTime, language);

                if (pseudomyzeContent)
                {
                    AnonymizeData(customer, x => x.AdminComment, IdentifierDataType.LongText, language);
                    AnonymizeData(customer, x => x.LastLoginDateUtc, IdentifierDataType.DateTime, language);
                    AnonymizeData(customer, x => x.LastActivityDateUtc, IdentifierDataType.DateTime, language);
                }

                // Addresses
                foreach (var address in customer.Addresses)
                {
                    AnonymizeAddress(address, language);
                }

                // Private messages
                if (pseudomyzeContent)
                {
                    var privateMessages = _forumService.GetAllPrivateMessages(0, customer.Id, 0, null, null, null, 0, int.MaxValue);
                    foreach (var msg in privateMessages)
                    {
                        AnonymizeData(msg, x => x.Subject, IdentifierDataType.Text, language);
                        AnonymizeData(msg, x => x.Text, IdentifierDataType.LongText, language);
                    }
                }

                // Forum topics
                if (pseudomyzeContent)
                {
                    foreach (var topic in customer.ForumTopics)
                    {
                        AnonymizeData(topic, x => x.Subject, IdentifierDataType.Text, language);
                    }
                }

                // Forum posts
                foreach (var post in customer.ForumPosts)
                {
                    AnonymizeData(post, x => x.IPAddress, IdentifierDataType.IpAddress, language);
                    if (pseudomyzeContent)
                    {
                        AnonymizeData(post, x => x.Text, IdentifierDataType.LongText, language);
                    }
                }

                // Customer Content
                foreach (var item in customer.CustomerContent)
                {
                    AnonymizeData(item, x => x.IpAddress, IdentifierDataType.IpAddress, language);

                    if (pseudomyzeContent)
                    {
                        switch (item)
                        {
                            case ProductReview c:
                                AnonymizeData(c, x => x.ReviewText, IdentifierDataType.LongText, language);
                                AnonymizeData(c, x => x.Title, IdentifierDataType.Text, language);
                                break;
                            case NewsComment c:
                                AnonymizeData(c, x => x.CommentText, IdentifierDataType.LongText, language);
                                AnonymizeData(c, x => x.CommentTitle, IdentifierDataType.Text, language);
                                break;
                            case BlogComment c:
                                AnonymizeData(c, x => x.CommentText, IdentifierDataType.LongText, language);
                                break;
                        }
                    }
                }

                //// Anonymize Order IPs
                //// TBD: Don't! Doesn't feel right because of fraud detection etc.
                //foreach (var order in customer.Orders)
                //{
                //	AnonymizeData(order, x => x.CustomerIp, IdentifierDataType.IpAddress, language);
                //}

                // SAVE!!!
                //_services.DbContext.DetachAll(); // TEST
                scope.Commit();

                // Log
                Logger.Info(T("Gdpr.Anonymize.Success", language.Id, customerName));
            }
        }

        private void AnonymizeAddress(Address address, Language language)
        {
            AnonymizeData(address, x => x.Address1, IdentifierDataType.Address, language);
            AnonymizeData(address, x => x.Address2, IdentifierDataType.Address, language);
            AnonymizeData(address, x => x.City, IdentifierDataType.Address, language);
            AnonymizeData(address, x => x.Company, IdentifierDataType.Address, language);
            AnonymizeData(address, x => x.Email, IdentifierDataType.EmailAddress, language);
            AnonymizeData(address, x => x.FaxNumber, IdentifierDataType.PhoneNumber, language);
            AnonymizeData(address, x => x.FirstName, IdentifierDataType.Name, language);
            AnonymizeData(address, x => x.LastName, IdentifierDataType.Name, language);
            AnonymizeData(address, x => x.PhoneNumber, IdentifierDataType.PhoneNumber, language);
            AnonymizeData(address, x => x.ZipPostalCode, IdentifierDataType.PostalCode, language);
        }

        public virtual void AnonymizeData<TEntity>(TEntity entity, Expression<Func<TEntity, object>> expression, IdentifierDataType type, Language language = null)
            where TEntity : BaseEntity
        {
            Guard.NotNull(entity, nameof(entity));
            Guard.NotNull(expression, nameof(expression));

            var originalValue = expression.Compile().Invoke(entity);
            object maskedValue = null;

            if (originalValue is DateTime d)
            {
                maskedValue = MinDate;
            }
            else if (originalValue is string s)
            {
                if (s.IsEmpty())
                {
                    return;
                }

                language = language ?? (entity as Customer)?.GetLanguage();

                switch (type)
                {
                    case IdentifierDataType.Address:
                    case IdentifierDataType.Name:
                    case IdentifierDataType.Text:
                        maskedValue = T("Gdpr.DeletedText", language.Id).Text;
                        break;
                    case IdentifierDataType.LongText:
                        maskedValue = T("Gdpr.DeletedLongText", language.Id).Text;
                        break;
                    case IdentifierDataType.EmailAddress:
                        //maskedValue = s.Hash(Encoding.ASCII, true) + "@anony.mous";
                        maskedValue = HashCodeCombiner.Start()
                            .Add(entity.GetHashCode())
                            .Add(s)
                            .CombinedHashString + "@anony.mous";
                        break;
                    case IdentifierDataType.Url:
                        maskedValue = "https://anony.mous";
                        break;
                    case IdentifierDataType.IpAddress:
                        maskedValue = AnonymizeIpAddress(s);
                        break;
                    case IdentifierDataType.UserName:
                        maskedValue = T("Gdpr.Anonymous", language.Id).Text.ToLower();
                        break;
                    case IdentifierDataType.PhoneNumber:
                        maskedValue = "555-00000";
                        break;
                    case IdentifierDataType.PostalCode:
                        maskedValue = "00000";
                        break;
                    case IdentifierDataType.DateTime:
                        maskedValue = MinDate.ToString(CultureInfo.InvariantCulture);
                        break;
                }
            }

            if (maskedValue != null)
            {
                var pi = expression.ExtractPropertyInfo();
                pi.SetValue(entity, maskedValue);
            }
        }

        /// <summary>
        /// Returns an anonymized IPv4 or IPv6 address.
        /// </summary>
        /// <param name="ipAddress">The IPv4 or IPv6 address to be anonymized.</param>
        /// <returns>The anonymized IP address.</returns>
        protected virtual string AnonymizeIpAddress(string ipAddress)
        {
            try
            {
                var ip = IPAddress.Parse(ipAddress);

                switch (ip.AddressFamily)
                {
                    case AddressFamily.InterNetwork:
                        break;
                    case AddressFamily.InterNetworkV6:
                        // Map to IPv4 first
                        ip = ip.MapToIPv4();
                        break;
                    default:
                        // we only support IPv4 and IPv6
                        return "0.0.0.0";
                }

                // Keep the first 3 bytes and append ".0"
                return string.Join(".", ip.GetAddressBytes().Take(3)) + ".0";
            }
            catch
            {
                return null;
            }
        }
    }
}
