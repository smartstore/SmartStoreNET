using System.Web;
using System.Web.Mvc;
using AmazonPay;
using SmartStore.AmazonPay.Models;
using SmartStore.Core.Domain.Orders;
using SmartStore.Services.Authentication.External;
using SmartStore.Services.Payments;

namespace SmartStore.AmazonPay.Services
{
    public partial interface IAmazonPayService : IExternalProviderAuthorizer
    {
        void SetupConfiguration(ConfigurationModel model);

        AmazonPayViewModel CreateViewModel(AmazonPayRequestType type, TempDataDictionary tempData);

        void CloseOrderReference(AmazonPaySettings settings, Order order);

        void AddCustomerOrderNoteLoop(AmazonPayActionState state);

        void GetBillingAddress();

        bool SetOrderReferenceDetails(
            Client client,
            AmazonPaySettings settings,
            AmazonPayCheckoutState state,
            string sellerNote,
            out string errorMessage);

        bool ConfirmOrderReference();

        ProcessPaymentResult ProcessPayment(ProcessPaymentRequest request);

        void PostProcessPayment(PostProcessPaymentRequest request);

        CapturePaymentResult Capture(CapturePaymentRequest request);

        RefundPaymentResult Refund(RefundPaymentRequest request);

        VoidPaymentResult Void(VoidPaymentRequest request);

        void ProcessIpn(HttpRequestBase request);

        void StartDataPolling();

        void ShareKeys(string payload, int storeId);
    }
}
