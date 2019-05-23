using System;
using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.PayPal.Providers;
using SmartStore.PayPal.Services;
using SmartStore.PayPal.Settings;
using SmartStore.Services.Payments;

namespace SmartStore.PayPal.Controllers
{
    public class PayPalInstalmentsController : PayPalRestApiControllerBase<PayPalInstalmentsSettings>
    {
        public PayPalInstalmentsController(
            IPayPalService payPalService) : base(
                PayPalInstalmentsProvider.SystemName,
                payPalService)
        {
        }

        public override ProcessPaymentRequest GetPaymentInfo(FormCollection form)
        {
            throw new NotImplementedException();
        }

        public override IList<string> ValidatePaymentForm(FormCollection form)
        {
            throw new NotImplementedException();
        }
    }
}