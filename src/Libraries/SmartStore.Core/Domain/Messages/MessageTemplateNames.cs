namespace SmartStore.Core.Domain.Messages
{
    public class MessageTemplateNames
    {
        public const string CustomerRegistered = "NewCustomer.Notification";
        public const string CustomerWelcome = "Customer.WelcomeMessage";
        public const string CustomerEmailValidation = "Customer.EmailValidationMessage";
        public const string CustomerPasswordRecovery = "Customer.PasswordRecovery";
        public const string OrderPlacedStoreOwner = "OrderPlaced.StoreOwnerNotification";
        public const string OrderPlacedCustomer = "OrderPlaced.CustomerNotification";
        public const string ShipmentSentCustomer = "ShipmentSent.CustomerNotification";
        public const string ShipmentDeliveredCustomer = "ShipmentDelivered.CustomerNotification";
        public const string OrderCompletedCustomer = "OrderCompleted.CustomerNotification";
        public const string OrderCancelledCustomer = "OrderCancelled.CustomerNotification";
        public const string OrderNoteAddedCustomer = "Customer.NewOrderNote";
        public const string RecurringPaymentCancelledStoreOwner = "RecurringPaymentCancelled.StoreOwnerNotification";
        public const string NewsLetterSubscriptionActivation = "NewsLetterSubscription.ActivationMessage";
        public const string NewsLetterSubscriptionDeactivation = "NewsLetterSubscription.DeactivationMessage";
        public const string ShareProduct = "Service.EmailAFriend";
        public const string ShareWishlist = "Wishlist.EmailAFriend";
        public const string ProductQuestion = "Product.AskQuestion";
        public const string NewReturnRequestStoreOwner = "NewReturnRequest.StoreOwnerNotification";
        public const string ReturnRequestStatusChangedCustomer = "ReturnRequestStatusChanged.CustomerNotification";
        public const string NewForumTopic = "Forums.NewForumTopic";
        public const string NewForumPost = "Forums.NewForumPost";
        public const string NewPrivateMessage = "Customer.NewPM";
        public const string GiftCardCustomer = "GiftCard.Notification";
        public const string ProductReviewStoreOwner = "Product.ProductReview";
        public const string QuantityBelowStoreOwner = "QuantityBelow.StoreOwnerNotification";
        public const string NewVatSubmittedStoreOwner = "NewVATSubmitted.StoreOwnerNotification";
        public const string BlogCommentStoreOwner = "Blog.BlogComment";
        public const string NewsCommentStoreOwner = "News.NewsComment";
        public const string BackInStockCustomer = "Customer.BackInStock";
        public const string SystemCampaign = "System.Campaign";
        public const string SystemContactUs = "System.ContactUs";
        public const string SystemGeneric = "System.Generic";
    }
}
