using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SmartStore.Core.Domain.Payments;
using SmartStore.PayPal.Models;
using SmartStore.PayPal.Settings;
using SmartStore.Services.Orders;
using SmartStore.Services.Payments;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Security;
using SmartStore.Web.Framework.Settings;

namespace SmartStore.PayPal.Controllers
{
    public class PayPalDirectController : PayPalControllerBase<PayPalDirectPaymentSettings>
    {
        private readonly HttpContextBase _httpContext;

        public PayPalDirectController(
            IPaymentService paymentService,
            IOrderService orderService,
            IOrderProcessingService orderProcessingService,
            PaymentSettings paymentSettings,
            HttpContextBase httpContext) : base(
                paymentService,
                orderService,
                orderProcessingService)
        {
            _httpContext = httpContext;
        }

        protected override string ProviderSystemName => PayPalDirectProvider.SystemName;

        [LoadSetting, AdminAuthorize, ChildActionOnly]
        public ActionResult Configure(PayPalDirectPaymentSettings settings, int storeScope)
        {
            var model = new PayPalDirectConfigurationModel();
            model.Copy(settings, true);

            PrepareConfigurationModel(model, storeScope);

            return View(model);
        }

        [HttpPost, AdminAuthorize, ChildActionOnly]
        [ValidateAntiForgeryToken]
        public ActionResult Configure(PayPalDirectConfigurationModel model, FormCollection form)
        {
            var storeDependingSettingHelper = new StoreDependingSettingHelper(ViewData);
            var storeScope = this.GetActiveStoreScopeConfiguration(Services.StoreService, Services.WorkContext);
            var settings = Services.Settings.LoadSetting<PayPalDirectPaymentSettings>(storeScope);

            if (!ModelState.IsValid)
            {
                return Configure(settings, storeScope);
            }

            ModelState.Clear();
            model.Copy(settings, false);

            using (Services.Settings.BeginScope())
            {
                storeDependingSettingHelper.UpdateSettings(settings, form, storeScope, Services.Settings);
            }

            using (Services.Settings.BeginScope())
            {
                // Multistore context not possible, see IPN handling.
                Services.Settings.SaveSetting(settings, x => x.UseSandbox, 0, false);
            }

            NotifySuccess(T("Admin.Common.DataSuccessfullySaved"));

            return RedirectToConfiguration(PayPalDirectProvider.SystemName, false);
        }

        public ActionResult PaymentInfo()
        {
            var model = new PayPalDirectPaymentInfoModel();

            // Credit card types.
            model.CreditCardTypes.Add(new SelectListItem
            {
                Text = "Visa",
                Value = "Visa",
            });
            model.CreditCardTypes.Add(new SelectListItem
            {
                Text = "Master card",
                Value = "MasterCard",
            });
            model.CreditCardTypes.Add(new SelectListItem
            {
                Text = "Discover",
                Value = "Discover",
            });
            model.CreditCardTypes.Add(new SelectListItem
            {
                Text = "Amex",
                Value = "Amex",
            });

            // Years.
            for (int i = 0; i < 15; i++)
            {
                string year = Convert.ToString(DateTime.Now.Year + i);
                model.ExpireYears.Add(new SelectListItem
                {
                    Text = year,
                    Value = year,
                });
            }

            // Months.
            for (int i = 1; i <= 12; i++)
            {
                string text = (i < 10) ? "0" + i.ToString() : i.ToString();
                model.ExpireMonths.Add(new SelectListItem
                {
                    Text = text,
                    Value = i.ToString(),
                });
            }

            // Set postback values.
            var paymentData = _httpContext.GetCheckoutState().PaymentData;
            model.CardholderName = (string)paymentData.Get("CardholderName");
            model.CardNumber = (string)paymentData.Get("CardNumber");
            model.CardCode = (string)paymentData.Get("CardCode");

            var creditCardType = (string)paymentData.Get("CreditCardType");
            var selectedCcType = model.CreditCardTypes.Where(x => x.Value.Equals(creditCardType, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            if (selectedCcType != null)
                selectedCcType.Selected = true;

            var expireMonth = (string)paymentData.Get("ExpireMonth");
            var selectedMonth = model.ExpireMonths.Where(x => x.Value.Equals(expireMonth, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            if (selectedMonth != null)
                selectedMonth.Selected = true;

            var expireYear = (string)paymentData.Get("ExpireYear");
            var selectedYear = model.ExpireYears.Where(x => x.Value.Equals(expireYear, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            if (selectedYear != null)
                selectedYear.Selected = true;

            return PartialView(model);
        }

        [NonAction]
        public override IList<string> ValidatePaymentForm(FormCollection form)
        {
            var warnings = new List<string>();
            var validator = new PaymentInfoValidator(T);

            var model = new PayPalDirectPaymentInfoModel
            {
                CardholderName = form["CardholderName"],
                CardNumber = form["CardNumber"],
                CardCode = form["CardCode"],
                ExpireMonth = form["ExpireMonth"],
                ExpireYear = form["ExpireYear"]
            };

            var validationResult = validator.Validate(model);
            if (!validationResult.IsValid)
            {
                validationResult.Errors.Each(x => warnings.Add(x.ErrorMessage));
            }
            return warnings;
        }

        [NonAction]
        public override ProcessPaymentRequest GetPaymentInfo(FormCollection form)
        {
            var paymentInfo = new ProcessPaymentRequest();

            paymentInfo.CreditCardType = form["CreditCardType"];
            paymentInfo.CreditCardName = form["CardholderName"];
            paymentInfo.CreditCardNumber = form["CardNumber"];
            paymentInfo.CreditCardExpireMonth = int.Parse(form["ExpireMonth"]);
            paymentInfo.CreditCardExpireYear = int.Parse(form["ExpireYear"]);
            paymentInfo.CreditCardCvv2 = form["CardCode"];

            return paymentInfo;
        }

        [NonAction]
        public override string GetPaymentSummary(FormCollection form)
        {
            var number = form["CardNumber"];

            return "{0}, {1}, {2}".FormatInvariant(
                form["CreditCardType"],
                form["CardholderName"],
                number.Mask(4)
            );
        }
    }
}