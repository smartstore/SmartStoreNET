using System;
using System.Web;
using System.Web.Mvc;
using System.Collections.Generic;
using OffAmazonPaymentsService;
using SmartStore.AmazonPay.Models;
using SmartStore.AmazonPay.Settings;
using SmartStore.Core.Domain.Orders;
using SmartStore.Services.Payments;

namespace SmartStore.AmazonPay.Services
{
	public partial interface IAmazonPayService
	{
		void LogError(Exception exception, string shortMessage = null, string fullMessage = null, bool notify = false, IList<string> errors = null);
		void LogAmazonError(OffAmazonPaymentsServiceException exception, bool notify = false, IList<string> errors = null);

		void AddOrderNote(AmazonPaySettings settings, Order order, AmazonPayOrderNote note, string anyString = null, bool isIpn = false);

		void SetupConfiguration(ConfigurationModel model);

		string GetWidgetUrl();

		AmazonPayViewModel ProcessPluginRequest(AmazonPayRequestType type, TempDataDictionary tempData, string orderReferenceId = null);

		void ApplyRewardPoints(bool useRewardPoints);

		void AddCustomerOrderNoteLoop(AmazonPayActionState state);

		PreProcessPaymentResult PreProcessPayment(ProcessPaymentRequest request);

		ProcessPaymentResult ProcessPayment(ProcessPaymentRequest request);

		void PostProcessPayment(PostProcessPaymentRequest request);

		CapturePaymentResult Capture(CapturePaymentRequest request);

		RefundPaymentResult Refund(RefundPaymentRequest request);

		VoidPaymentResult Void(VoidPaymentRequest request);

		void ProcessIpn(HttpRequestBase request);

		void DataPollingTaskProcess();

		void DataPollingTaskInit();

		void DataPollingTaskUpdate(bool enabled, int seconds);

		void DataPollingTaskDelete();
	}
}
