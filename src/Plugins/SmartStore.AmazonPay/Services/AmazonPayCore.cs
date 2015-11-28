using System;
using System.Collections.Generic;
using System.Globalization;
using SmartStore.AmazonPay.Api;
using SmartStore.AmazonPay.Settings;
using SmartStore.Core.Domain.Orders;

namespace SmartStore.AmazonPay.Services
{
	public static class AmazonPayCore
	{
		public static string PlatformId { get { return "A3OJ83WFYM72IY"; } }
		public static string AppName { get { return "SmartStore.Net " + SystemName; } }
		public static string SystemName { get { return "SmartStore.AmazonPay"; } }
		public static string AmazonPayCheckoutStateKey { get { return SystemName + ".CheckoutState"; } }
		public static string AmazonPayOrderAttributeKey { get { return SystemName + ".OrderAttribute"; } }
		public static string AmazonPayRefundIdKey { get { return SystemName + ".RefundId"; } }
		public static string DataPollingTaskType { get { return "SmartStore.AmazonPay.DataPollingTask, SmartStore.AmazonPay"; } }

		public static string UrlApiEuSandbox { get { return "https://mws-eu.amazonservices.com/OffAmazonPayments_Sandbox/2013-01-01/"; } }
		public static string UrlApiEuProduction { get { return "https://mws-eu.amazonservices.com/OffAmazonPayments/2013-01-01/"; } }

		public static string UrlWidgetSandbox { get { return "https://static-eu.payments-amazon.com/OffAmazonPayments/{0}/sandbox/js/Widgets.js"; } }
		public static string UrlWidgetProduction { get { return "https://static-eu.payments-amazon.com/OffAmazonPayments/{0}/js/Widgets.js"; } }

		public static string UrlButtonSandbox { get { return "https://payments-sandbox.amazon.{0}/gp/widgets/button"; } }
		public static string UrlButtonProduction { get { return "https://payments.amazon.{0}/gp/widgets/button"; } }

		public static string UrlIpnSchema { get { return "https://amazonpayments.s3.amazonaws.com/documents/payments_ipn.xsd"; } }
	}


	public class AmazonPayCheckoutState
	{
		public string OrderReferenceId { get; set; }
	}


	public class AmazonPayActionState
	{
		public Guid OrderGuid { get; set; }
		public List<string> Errors { get; set; }
	}


	[Serializable]
	public class AmazonPayOrderAttribute
	{
		public string OrderReferenceId { get; set; }
		public bool IsBillingAddressApplied { get; set; }
	}


	public class PollingLoopData
	{
		public PollingLoopData(int orderId)
		{
			OrderId = orderId;
		}

		public int OrderId { get; private set; }
		public Order Order { get; set; }
		public AmazonPaySettings Settings { get; set; }
		public AmazonPayClient Client { get; set; }
	}


	public class AmazonPayApiData
	{
		public string MessageType { get; set; }
		public string MessageId { get; set; }
		public string AuthorizationId { get; set; }
		public string CaptureId { get; set; }
		public string RefundId { get; set; }
		public string ReferenceId { get; set; }

		public string ReasonCode { get; set; }
		public string ReasonDescription { get; set; }
		public string State { get; set; }
		public DateTime StateLastUpdate { get; set; }

		public AmazonPayApiPrice Fee { get; set; }
		public AmazonPayApiPrice AuthorizedAmount { get; set; }
		public AmazonPayApiPrice CapturedAmount { get; set; }
		public AmazonPayApiPrice RefundedAmount { get; set; }

		public bool? CaptureNow { get; set; }
		public DateTime Creation { get; set; }
		public DateTime? Expiration { get; set; }

		public string AnyAmazonId
		{
			get
			{
				if (CaptureId.HasValue())
					return CaptureId;
				if (AuthorizationId.HasValue())
					return AuthorizationId;
				return RefundId;
			}
		}
	}


	public class AmazonPayApiPrice
	{
		public AmazonPayApiPrice()
		{
		}
		public AmazonPayApiPrice(double amount, string currenycCode)
		{
			Amount = amount;
			CurrencyCode = currenycCode;
		}
		public AmazonPayApiPrice(string amount, string currenycCode)
		{
			double d;
			if (amount.HasValue() && double.TryParse(amount, NumberStyles.Any, CultureInfo.InvariantCulture, out d))
				Amount = d;

			CurrencyCode = currenycCode;
		}

		public double Amount { get; set; }
		public string CurrencyCode { get; set; }

		public override string ToString()
		{
			string str = Amount.ToString("0.00", CultureInfo.InvariantCulture);
			return str.Grow(CurrencyCode, " ");
		}
	}


	public enum AmazonPayRequestType : int
	{
		None = 0,
		ShoppingCart,
		Address,
		Payment,
		OrderReviewData,
		ShippingMethod,
		MiniShoppingCart,
		LoginHandler
	}

	public enum AmazonPayTransactionType : int
	{
		None = 0,
		Authorize,
		AuthorizeAndCapture
	}

	public enum AmazonPaySaveDataType : int
	{
		None = 0,
		OnlyIfEmpty,
		Always
	}

	public enum AmazonPayDataFetchingType : int
	{
		None = 0,
		Ipn,
		Polling
	}

	public enum AmazonPayResultType : int
	{
		None = 0,
		PluginView,
		Redirect,
		Unauthorized
	}

	public enum AmazonPayOrderNote : int
	{
		FunctionExecuted = 0,
		Answer,
		BillingAddressApplied,
		AmazonMessageProcessed,
		BillingAddressCountryNotAllowed
	}

	public enum AmazonPayMessage : int
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
