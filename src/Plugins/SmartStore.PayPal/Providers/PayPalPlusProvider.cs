using System;
using System.Web;
using SmartStore.Core.Domain.Payments;
using SmartStore.Core.Plugins;
using SmartStore.PayPal.Controllers;
using SmartStore.PayPal.Services;
using SmartStore.PayPal.Settings;
using SmartStore.Services.Payments;

namespace SmartStore.PayPal
{
	[SystemName("Payments.PayPalPlus")]
    [FriendlyName("PayPal Plus")]
    [DisplayOrder(1)]
    public partial class PayPalPlusProvider : PayPalRestApiProviderBase<PayPalPlusPaymentSettings>
    {
		private readonly HttpContextBase _httpContext;
		private readonly IPayPalService _payPalService;

		public PayPalPlusProvider(
			HttpContextBase httpContext,
			IPayPalService payPalService)
        {
			_httpContext = httpContext;
			_payPalService = payPalService;
        }

		public static string SystemName
		{
			get { return "Payments.PayPalPlus"; }
		}

		public override PaymentMethodType PaymentMethodType
		{
			get
			{
				return PaymentMethodType.StandardAndRedirection;
			}
		}

		public override Type GetControllerType()
		{
			return typeof(PayPalPlusController);
		}

		public override ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
			var result = new ProcessPaymentResult
			{
				NewPaymentStatus = PaymentStatus.Pending
			};

			var settings = Services.Settings.LoadSetting<PayPalPlusPaymentSettings>(processPaymentRequest.StoreId);
			var session = _httpContext.GetPayPalSessionData();

			processPaymentRequest.OrderGuid = session.OrderGuid;

			var apiResult = _payPalService.ExecutePayment(settings, session);

			if (apiResult.Success && apiResult.Json != null)
			{
				var state = (string)apiResult.Json.state;		

				if (!state.IsCaseInsensitiveEqual("failed"))
				{
					result.AuthorizationTransactionCode = apiResult.Id;     // payment id

					var sale = apiResult.Json.transactions[0].related_resources[0].sale;

					if (sale != null)
					{
						state = (string)sale.state;

						result.AuthorizationTransactionResult = state;
						result.AuthorizationTransactionId = (string)sale.id;

						if (state.IsCaseInsensitiveEqual("completed") || state.IsCaseInsensitiveEqual("processed"))
						{
							result.CaptureTransactionResult = state;
							result.CaptureTransactionId = (string)sale.id;

							result.NewPaymentStatus = PaymentStatus.Paid;
						}
						else
						{
							result.NewPaymentStatus = PaymentStatus.Authorized;
						}
					}
				}
				else
				{
					result.Errors.Add(T("Plugins.SmartStore.PayPal.PaymentExecuteFailed"));
				}
			}
			else
			{
				result.Errors.Add(apiResult.ErrorMessage);
			}

			return result;
        }
    }
}