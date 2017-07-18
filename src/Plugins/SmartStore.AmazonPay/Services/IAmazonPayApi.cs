using System.Collections.Generic;
using System.Web;
using OffAmazonPaymentsService.Model;
using SmartStore.AmazonPay.Services;
using SmartStore.AmazonPay.Settings;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Orders;
using SmartStore.Services.Payments;

namespace SmartStore.AmazonPay.Api
{
	public partial interface IAmazonPayApi
	{
		AmazonPayApiClient CreateClient(AmazonPaySettings settings);

		bool FindAndApplyAddress(OrderReferenceDetails details, Customer customer, bool isShippable, bool forceToTakeAmazonAddress);
		bool FulfillBillingAddress(AmazonPaySettings settings, Order order, AuthorizationDetails details, out string formattedAddress);

		OrderReferenceDetails GetOrderReferenceDetails(AmazonPayApiClient client, string orderReferenceId, string addressConsentToken = null);

		OrderReferenceDetails SetOrderReferenceDetails(AmazonPayApiClient client, string orderReferenceId, decimal? orderTotalAmount,
			string currencyCode, string orderGuid = null, string storeName = null);

		OrderReferenceDetails SetOrderReferenceDetails(AmazonPayApiClient client, string orderReferenceId, string currencyCode, List<OrganizedShoppingCartItem> cart);

		void CancelOrderReference(AmazonPayApiClient client, string orderReferenceId);

		void CloseOrderReference(AmazonPayApiClient client, string orderReferenceId);

		void Authorize(AmazonPayApiClient client, ProcessPaymentResult result, List<string> errors, string orderReferenceId, decimal orderTotalAmount,
			string currencyCode, string orderGuid);

		AuthorizationDetails GetAuthorizationDetails(AmazonPayApiClient client, string authorizationId, out AmazonPayApiData data);

		void Capture(AmazonPayApiClient client, CapturePaymentRequest capture, CapturePaymentResult result);

		CaptureDetails GetCaptureDetails(AmazonPayApiClient client, string captureId, out AmazonPayApiData data);

		string Refund(AmazonPayApiClient client, RefundPaymentRequest refund, RefundPaymentResult result);

		RefundDetails GetRefundDetails(AmazonPayApiClient client, string refundId, out AmazonPayApiData data);

		string ToInfoString(AmazonPayApiData data);

		AmazonPayApiData ParseNotification(HttpRequestBase request);
	}
}