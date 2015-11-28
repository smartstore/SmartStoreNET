using System.Collections.Generic;
using SmartStore.Core.Domain.Blogs;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Forums;
using SmartStore.Core.Domain.Messages;
using SmartStore.Core.Domain.News;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Domain.Stores;

namespace SmartStore.Services.Messages
{
    public partial interface IMessageTokenProvider
    {
		void AddStoreTokens(IList<Token> tokens, Store store);

        void AddOrderTokens(IList<Token> tokens, Order order, int languageId);

        void AddShipmentTokens(IList<Token> tokens, Shipment shipment, int languageId);

        void AddOrderNoteTokens(IList<Token> tokens, OrderNote orderNote);

        void AddRecurringPaymentTokens(IList<Token> tokens, RecurringPayment recurringPayment);

        void AddReturnRequestTokens(IList<Token> tokens, ReturnRequest returnRequest, OrderItem orderItem);

        void AddGiftCardTokens(IList<Token> tokens, GiftCard giftCard);

        void AddCustomerTokens(IList<Token> tokens, Customer customer);

        void AddNewsLetterSubscriptionTokens(IList<Token> tokens, NewsLetterSubscription subscription);

        void AddProductReviewTokens(IList<Token> tokens, ProductReview productReview);

        void AddBlogCommentTokens(IList<Token> tokens, BlogComment blogComment);

        void AddNewsCommentTokens(IList<Token> tokens, NewsComment newsComment);

		void AddProductTokens(IList<Token> tokens, Product product, int languageId);

		void AddForumTokens(IList<Token> tokens, Forum forum, int languageId);

        void AddForumTopicTokens(IList<Token> tokens, ForumTopic forumTopic,
            int? friendlyForumTopicPageIndex = null, int? appendedPostIdentifierAnchor = null);

        void AddForumPostTokens(IList<Token> tokens, ForumPost forumPost);

        void AddPrivateMessageTokens(IList<Token> tokens, PrivateMessage privateMessage);

        void AddBackInStockTokens(IList<Token> tokens, BackInStockSubscription subscription);

        string[] GetListOfCampaignAllowedTokens();

        string[] GetListOfAllowedTokens();

        //codehint: sm-add begin
        void AddBankConnectionTokens(IList<Token> tokens);
        
        void AddCompanyTokens(IList<Token> tokens);

        void AddContactDataTokens(IList<Token> tokens);
        //codehint: sm-add end

    }
}
