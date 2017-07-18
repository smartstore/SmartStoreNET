using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using SmartStore.AmazonPay.Models;
using SmartStore.Services.Authentication.External;
using SmartStore.Services.Payments;

namespace SmartStore.AmazonPay.Services
{
	public partial interface IAmazonPayService : IExternalProviderAuthorizer
	{
		void LogError(Exception exception, string shortMessage = null, string fullMessage = null, bool notify = false, IList<string> errors = null);

		void SetupConfiguration(ConfigurationModel model);

		AmazonPayViewModel CreateViewModel(
			AmazonPayRequestType type,
			TempDataDictionary tempData,
			string orderReferenceId = null,
			string accessToken = null);

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
