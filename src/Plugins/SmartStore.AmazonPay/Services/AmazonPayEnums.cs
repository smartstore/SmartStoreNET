namespace SmartStore.AmazonPay.Services
{
	public enum AmazonPayRequestType
	{
		None = 0,
		ShoppingCart,
		Address,
		Payment,
		OrderReviewData,
		ShippingMethod,
		MiniShoppingCart,
		LoginHandler,
		LoginPage
	}

	public enum AmazonPayTransactionType
	{
		None = 0,
		Authorize,
		AuthorizeAndCapture
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

	public enum AmazonPayOrderNote
	{
		FunctionExecuted = 0,
		Answer,
		BillingAddressApplied,
		AmazonMessageProcessed,
		BillingAddressCountryNotAllowed
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