namespace SmartStore.AmazonPay.Services
{
    public enum AmazonPayRequestType
    {
        None = 0,
        ShoppingCart,
        Address,
        PaymentMethod,
        OrderReviewData,
        ShippingMethod,
        MiniShoppingCart,

        /// <summary>
        /// Amazon Pay button clicked
        /// </summary>
        PayButtonHandler,

        /// <summary>
        /// Display authentication button on login page
        /// </summary>
        AuthenticationPublicInfo
    }

    public enum AmazonPayTransactionType
    {
        None = 0,
        Authorize,
        AuthorizeAndCapture
    }

    public enum AmazonPayAuthorizeMethod
    {
        Omnichronous = 0,
        Asynchronous,
        Synchronous
    }

    public enum AmazonPaySaveDataType
    {
        None = 0,
        OnlyIfEmpty,
        Always
    }

    public enum AmazonPayDataFetchingType
    {
        None = 0,
        Ipn,
        Polling
    }

    public enum AmazonPayResultType
    {
        None = 0,
        PluginView,
        Redirect,
        Unauthorized
    }

    public enum AmazonPayMessage
    {
        MessageTyp = 0,
        MessageId,
        AuthorizationID,
        CaptureID,
        RefundID,
        ReferenceID,
        State,
        StateUpdate,
        Fee,
        AuthorizedAmount,
        CapturedAmount,
        RefundedAmount,
        CaptureNow,
        Creation,
        Expiration
    }
}