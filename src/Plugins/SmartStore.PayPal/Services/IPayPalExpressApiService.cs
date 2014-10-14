using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Routing;
using SmartStore.Core;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Payments;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Infrastructure;
using SmartStore.PayPal.PayPalSvc;
using SmartStore.PayPal.Settings;
using SmartStore.Services;
using SmartStore.Services.Catalog;
using SmartStore.Services.Common;
using SmartStore.Services.Configuration;
using SmartStore.Services.Customers;
using SmartStore.Services.Directory;
using SmartStore.Services.Orders;
using SmartStore.Services.Payments;
using SmartStore.Services.Shipping;
using SmartStore.Services.Stores;
using SmartStore.Web.Framework.Plugins;

namespace SmartStore.PayPal.Services
{

	public partial interface IPayPalExpressApiService
	{
        ProcessPaymentResult ProcessPayment(ProcessPaymentRequest request);

        void PostProcessPayment(PostProcessPaymentRequest request);

        CapturePaymentResult Capture(CapturePaymentRequest request);

        VoidPaymentResult Void(VoidPaymentRequest request);

        RefundPaymentResult Refund(RefundPaymentRequest request);

        CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest);

        SetExpressCheckoutResponseType SetExpressCheckout(PayPalProcessPaymentRequest processPaymentRequest, IList<Core.Domain.Orders.OrganizedShoppingCartItem> cart);

        GetExpressCheckoutDetailsResponseType GetExpressCheckoutDetails(string token);

        ProcessPaymentRequest SetCheckoutDetails(ProcessPaymentRequest processPaymentRequest, GetExpressCheckoutDetailsResponseDetailsType checkoutDetails);

        bool VerifyIPN(string formString, out Dictionary<string, string> values);

        DoExpressCheckoutPaymentResponseType DoExpressCheckoutPayment(ProcessPaymentRequest processPaymentRequest);
    }
}

