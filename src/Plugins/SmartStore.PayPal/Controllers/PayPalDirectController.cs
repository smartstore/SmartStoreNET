using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SmartStore.Core.Domain.Payments;
using SmartStore.PayPal.Models;
using SmartStore.PayPal.Services;
using SmartStore.PayPal.Settings;
using SmartStore.PayPal.Validators;
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
				PayPalDirectProvider.SystemName,
				paymentService,
				orderService,
				orderProcessingService)
		{
			_httpContext = httpContext;
		}

		private SelectList TransactModeValues(TransactMode selected)
		{
			return new SelectList(new List<object>
			{
				new { ID = (int)TransactMode.Authorize, Name = T("Plugins.Payments.PayPalDirect.ModeAuth") },
				new { ID = (int)TransactMode.AuthorizeAndCapture, Name = T("Plugins.Payments.PayPalDirect.ModeAuthAndCapture") }
			},
			"ID", "Name", (int)selected);
		}

		[AdminAuthorize, ChildActionOnly]
		public ActionResult Configure()
		{
            var model = new PayPalDirectConfigurationModel();
            int storeScope = this.GetActiveStoreScopeConfiguration(Services.StoreService, Services.WorkContext);
            var settings = Services.Settings.LoadSetting<PayPalDirectPaymentSettings>(storeScope);

            model.Copy(settings, true);

			model.TransactModeValues = TransactModeValues(settings.TransactMode);

			model.AvailableSecurityProtocols = PayPalService.GetSecurityProtocols()
				.Select(x => new SelectListItem { Value = ((int)x.Key).ToString(), Text = x.Value })
				.ToList();

			var storeDependingSettingHelper = new StoreDependingSettingHelper(ViewData);
			storeDependingSettingHelper.GetOverrideKeys(settings, model, storeScope, Services.Settings);

            return View(model);
		}

		[HttpPost, AdminAuthorize, ChildActionOnly]
		public ActionResult Configure(PayPalDirectConfigurationModel model, FormCollection form)
		{
            if (!ModelState.IsValid)
                return Configure();

			ModelState.Clear();

            var storeDependingSettingHelper = new StoreDependingSettingHelper(ViewData);
            int storeScope = this.GetActiveStoreScopeConfiguration(Services.StoreService, Services.WorkContext);
			var settings = Services.Settings.LoadSetting<PayPalDirectPaymentSettings>(storeScope);

            model.Copy(settings, false);

			using (Services.Settings.BeginScope())
			{
				storeDependingSettingHelper.UpdateSettings(settings, form, storeScope, Services.Settings);

				// multistore context not possible, see IPN handling
				Services.Settings.SaveSetting(settings, x => x.UseSandbox, 0, false);
			}

            NotifySuccess(T("Admin.Common.DataSuccessfullySaved"));

            return Configure();
		}

		public ActionResult PaymentInfo()
		{
			var model = new PayPalDirectPaymentInfoModel();

			//CC types
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

			//years
			for (int i = 0; i < 15; i++)
			{
				string year = Convert.ToString(DateTime.Now.Year + i);
				model.ExpireYears.Add(new SelectListItem
				{
					Text = year,
					Value = year,
				});
			}

			//months
			for (int i = 1; i <= 12; i++)
			{
				string text = (i < 10) ? "0" + i.ToString() : i.ToString();
				model.ExpireMonths.Add(new SelectListItem
				{
					Text = text,
					Value = i.ToString(),
				});
			}

			//set postback values
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
			var validator = new PaymentInfoValidator(Services.Localization);

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