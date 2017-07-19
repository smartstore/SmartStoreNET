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

		bool FulfillBillingAddress(AmazonPaySettings settings, Order order, AuthorizationDetails details, out string formattedAddress);

		AuthorizationDetails GetAuthorizationDetails(AmazonPayApiClient client, string authorizationId, out AmazonPayApiData data);

		CaptureDetails GetCaptureDetails(AmazonPayApiClient client, string captureId, out AmazonPayApiData data);

		RefundDetails GetRefundDetails(AmazonPayApiClient client, string refundId, out AmazonPayApiData data);

		string ToInfoString(AmazonPayApiData data);

		AmazonPayApiData ParseNotification(HttpRequestBase request);
	}
}