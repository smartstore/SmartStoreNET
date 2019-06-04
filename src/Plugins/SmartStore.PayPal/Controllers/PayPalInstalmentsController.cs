using System;
using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.ComponentModel;
using SmartStore.PayPal.Models;
using SmartStore.PayPal.Providers;
using SmartStore.PayPal.Services;
using SmartStore.PayPal.Settings;
using SmartStore.Services.Payments;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Security;
using SmartStore.Web.Framework.Settings;
using SmartStore.Web.Framework.Theming;

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

        [ChildActionOnly, AdminAuthorize, LoadSetting, AdminThemed]
        public ActionResult Configure(PayPalInstalmentsSettings settings, int storeScope)
        {
            var model = new PayPalInstalmentsConfigModel();
            MiniMapper.Map(settings, model);
            PrepareConfigurationModel(model, storeScope);

            return View(model);
        }

        [HttpPost, ChildActionOnly, AdminAuthorize, AdminThemed]
        public ActionResult Configure(PayPalInstalmentsConfigModel model, FormCollection form)
        {
            if (!SaveConfigurationModel<PayPalInstalmentsSettings>(model, form))
            {
                var storeScope = this.GetActiveStoreScopeConfiguration(Services.StoreService, Services.WorkContext);
                var settings = Services.Settings.LoadSetting<PayPalInstalmentsSettings>(storeScope);

                return Configure(settings, storeScope);
            }

            return RedirectToConfiguration(PayPalInstalmentsProvider.SystemName, false);
        }
    }
}