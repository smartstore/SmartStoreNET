using System;
using System.Collections.Generic;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Stores;
using SmartStore.PayPal.Settings;
using SmartStore.Services.Payments;

namespace SmartStore.PayPal.Services
{
	public interface IPayPalService
	{
		void AddOrderNote(PayPalSettingsBase settings, Order order, string anyString);

		void LogError(Exception exception, string shortMessage = null, string fullMessage = null, bool notify = false, IList<string> errors = null, bool isWarning = false);

		PayPalPaymentInstruction ParsePaymentInstruction(dynamic json);

		string CreatePaymentInstruction(PayPalPaymentInstruction instruct);

		PayPalResponse CallApi(string method, string path, string accessToken, PayPalApiSettingsBase settings, string data, IList<string> errors = null);

		PayPalResponse EnsureAccessToken(PayPalSessionData session, PayPalApiSettingsBase settings);

		PayPalResponse UpsertCheckoutExperience(PayPalApiSettingsBase settings, PayPalSessionData session, Store store, string profileId);

		PayPalResponse GetPayment(PayPalApiSettingsBase settings, PayPalSessionData session);

		PayPalResponse CreatePayment(
			PayPalApiSettingsBase settings,
			PayPalSessionData session,
			List<OrganizedShoppingCartItem> cart,
			string providerSystemName,
			string returnUrl,
			string cancelUrl);

		PayPalResponse ExecutePayment(PayPalApiSettingsBase settings, PayPalSessionData session);

		PayPalResponse Refund(PayPalApiSettingsBase settings, PayPalSessionData session, RefundPaymentRequest request);
	}
}