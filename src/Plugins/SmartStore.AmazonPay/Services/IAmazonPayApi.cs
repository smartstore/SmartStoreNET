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
		void GetConstraints(OrderReferenceDetails details, IList<string> warnings);

		bool FindAndApplyAddress(OrderReferenceDetails details, Customer customer, bool isShippable, bool forceToTakeAmazonAddress);
		bool FulfillBillingAddress(AmazonPaySettings settings, Order order, AuthorizationDetails details, out string formattedAddress);

		OrderReferenceDetails GetOrderReferenceDetails(AmazonPayClient client, string orderReferenceId, string addressConsentToken = null);

		OrderReferenceDetails SetOrderReferenceDetails(AmazonPayClient client, string orderReferenceId, decimal? orderTotalAmount,
			string currencyCode, string orderGuid = null, string storeName = null);

		OrderReferenceDetails SetOrderReferenceDetails(AmazonPayClient client, string orderReferenceId, string currencyCode, List<OrganizedShoppingCartItem> cart);

		void ConfirmOrderReference(AmazonPayClient client, string orderReferenceId);

		void CancelOrderReference(AmazonPayClient client, string orderReferenceId);

		void CloseOrderReference(AmazonPayClient client, string orderReferenceId);

		void Authorize(AmazonPayClient client, ProcessPaymentResult result, List<string> errors, string orderReferenceId, decimal orderTotalAmount,
			string currencyCode, string orderGuid);

		AuthorizationDetails GetAuthorizationDetails(AmazonPayClient client, string authorizationId, out AmazonPayApiData data);

		void Capture(AmazonPayClient client, CapturePaymentRequest capture, CapturePaymentResult result);

		CaptureDetails GetCaptureDetails(AmazonPayClient client, string captureId, out AmazonPayApiData data);

		string Refund(AmazonPayClient client, RefundPaymentRequest refund, RefundPaymentResult result);

		RefundDetails GetRefundDetails(AmazonPayClient client, string refundId, out AmazonPayApiData data);

		string ToInfoString(AmazonPayApiData data);

		AmazonPayApiData ParseNotification(HttpRequestBase request);
	}
}