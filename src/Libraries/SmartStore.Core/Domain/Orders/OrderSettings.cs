using SmartStore.Core.Configuration;

namespace SmartStore.Core.Domain.Orders
{
    public class OrderSettings : ISettings
    {
        public OrderSettings()
        {
            IsReOrderAllowed = true;
            AnonymousCheckoutAllowed = true;
            TermsOfServiceEnabled = true;
            ReturnRequestsEnabled = true;
            ReturnRequestActions = "Repair,Replacement,Store Credit";
            ReturnRequestReasons = "Received Wrong Product,Wrong Product Ordered,There Was A Problem With The Product";
            NumberOfDaysReturnRequestAvailable = 365;
            MinimumOrderPlacementInterval = 30;
            OrderListPageSize = 10;
            RecurringPaymentListPageSize = 10;
        }

        /// <summary>
        /// Gets or sets a value indicating whether customer can make re-order
        /// </summary>
        public bool IsReOrderAllowed { get; set; }

        /// <summary>
        /// Gets or sets a minimum order total amount
        /// </summary>
        public decimal? OrderTotalMinimum { get; set; }

        /// <summary>
        /// Gets or sets a maximum order total amount
        /// </summary>
        public decimal? OrderTotalMaximum { get; set; }

        /// <summary>
        /// Gets or sets a value indicating how customer group restrictions are applied with each other
        /// true - lowest possible total amount span gets applied, false - highest possible total amount span gets applied
        /// </summary>
        public bool MultipleOrderTotalRestrictionsExpandRange { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether anonymous checkout allowed
        /// </summary>
        public bool AnonymousCheckoutAllowed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether 'Terms of service' enabled
        /// </summary>
        public bool TermsOfServiceEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether "Order completed" page should be skipped
        /// </summary>
        public bool DisableOrderCompletedPage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether "Return requests" are allowed
        /// </summary>
        public bool ReturnRequestsEnabled { get; set; }

        /// <summary>
        /// Gets or sets a list of return request reasons
        /// </summary>
        public string ReturnRequestReasons { get; set; }

        /// <summary>
        /// Gets or sets a list of return request actions
        /// </summary>
        public string ReturnRequestActions { get; set; }

        /// <summary>
        /// Gets or sets a number of days that the Return Request Link will be available for customers after order placing.
        /// </summary>
        public int NumberOfDaysReturnRequestAvailable { get; set; }

        /// <summary>
        ///  Gift cards are activated when the order status is
        /// </summary>
        public int GiftCards_Activated_OrderStatusId { get; set; }

        /// <summary>
        ///  Gift cards are deactivated when the order status is
        /// </summary>
        public int GiftCards_Deactivated_OrderStatusId { get; set; }

        /// <summary>
        /// Gets or sets an order placement interval in seconds (prevent 2 orders being placed within an X seconds time frame).
        /// </summary>
        public int MinimumOrderPlacementInterval { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to display all orders of all stores to a customer
        /// </summary>
        public bool DisplayOrdersOfAllStores { get; set; }

        /// <summary>
        /// Page size of the order list
        /// </summary>
        public int OrderListPageSize { get; set; }

        /// <summary>
        /// Page size of the recurring payment list
        /// </summary>
        public int RecurringPaymentListPageSize { get; set; }
    }
}