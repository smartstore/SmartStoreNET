using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Payments;
using SmartStore.Core.Domain.Stores;
using SmartStore.PayPal.Settings;
using SmartStore.Services.Payments;

namespace SmartStore.PayPal.Services
{
	public interface IPayPalService
	{
		void AddOrderNote(PayPalSettingsBase settings, Order order, string anyString, bool isIpn = false);

		PayPalPaymentInstruction ParsePaymentInstruction(dynamic json);

		string CreatePaymentInstruction(PayPalPaymentInstruction instruct);

		PaymentStatus GetPaymentStatus(string state, string reasonCode, PaymentStatus defaultStatus);

		PayPalResponse CallApi(string method, string path, string accessToken, PayPalApiSettingsBase settings, string data);

		PayPalResponse EnsureAccessToken(PayPalSessionData session, PayPalApiSettingsBase settings);

		PayPalResponse GetPayment(PayPalApiSettingsBase settings, PayPalSessionData session);

		PayPalResponse CreatePayment(
			PayPalApiSettingsBase settings,
			PayPalSessionData session,
			List<OrganizedShoppingCartItem> cart,
			string providerSystemName,
			string returnUrl,
			string cancelUrl);

		PayPalResponse PatchShipping(
			PayPalApiSettingsBase settings,
			PayPalSessionData session,
			List<OrganizedShoppingCartItem> cart,
			string providerSystemName);

		PayPalResponse ExecutePayment(PayPalApiSettingsBase settings, PayPalSessionData session);

		PayPalResponse Refund(PayPalApiSettingsBase settings, PayPalSessionData session, RefundPaymentRequest request);

		PayPalResponse Capture(PayPalApiSettingsBase settings, PayPalSessionData session, CapturePaymentRequest request);

		PayPalResponse Void(PayPalApiSettingsBase settings, PayPalSessionData session, VoidPaymentRequest request);

		PayPalResponse UpsertCheckoutExperience(PayPalApiSettingsBase settings, PayPalSessionData session, Store store);

		PayPalResponse DeleteCheckoutExperience(PayPalApiSettingsBase settings, PayPalSessionData session);

		PayPalResponse CreateWebhook(PayPalApiSettingsBase settings, PayPalSessionData session, string url);

		PayPalResponse DeleteWebhook(PayPalApiSettingsBase settings, PayPalSessionData session);

		HttpStatusCode ProcessWebhook(
			PayPalApiSettingsBase settings,
			NameValueCollection headers,
			string rawJson,
			string providerSystemName);
	}
}