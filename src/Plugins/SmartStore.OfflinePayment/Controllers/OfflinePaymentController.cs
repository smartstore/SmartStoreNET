using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Autofac;
using FluentValidation;
using SmartStore.Core.Localization;
using SmartStore.OfflinePayment.Models;
using SmartStore.OfflinePayment.Settings;
using SmartStore.OfflinePayment.Validators;
using SmartStore.Services;
using SmartStore.Services.Payments;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Controllers;

namespace SmartStore.OfflinePayment.Controllers
{

    public class OfflinePaymentController : PaymentControllerBase
    {
		private readonly IComponentContext _ctx;
		private readonly ICommonServices _services;

		public OfflinePaymentController(
			ICommonServices services,
			IComponentContext ctx)
        {
			this._services = services;
			this._ctx = ctx;

			T = NullLocalizer.Instance;
        }

		public Localizer T { get; set; }

		#region Global

		[NonAction]
		private TModel ConfigureGet<TModel, TSetting>(Action<TModel, TSetting> fn = null) 
			where TModel : ConfigurationModelBase, new()
			where TSetting : PaymentSettingsBase, new()
		{
			var settings = _ctx.Resolve<TSetting>();
			var model = new TModel();

			model.DescriptionText = settings.DescriptionText;
			model.AdditionalFee = settings.AdditionalFee;
			model.AdditionalFeePercentage = settings.AdditionalFeePercentage;

			if (fn != null)
			{
				fn(model, settings);
			}

			return model;
		}

		[NonAction]
		private TSetting ConfigurePost<TModel, TSetting>(TModel model)
			where TModel : ConfigurationModelBase, new()
			where TSetting : PaymentSettingsBase, new()
		{
			var settings = _ctx.Resolve<TSetting>();
			settings.DescriptionText = model.DescriptionText;
			settings.AdditionalFee = model.AdditionalFee;
			settings.AdditionalFeePercentage = model.AdditionalFeePercentage;

			return settings;
		}

		[NonAction]
		private TModel PaymentInfoGet<TModel, TSetting>(Action<TModel, TSetting> fn = null)
			where TModel : PaymentInfoModelBase, new()
			where TSetting : PaymentSettingsBase, new()
		{
			var settings = _ctx.Resolve<TSetting>();
			var model = new TModel();
			model.DescriptionText = GetLocalizedText(settings.DescriptionText);

			if (fn != null)
			{
				fn(model, settings);
			}

			return model;
		}

		private string GetLocalizedText(string text)
		{
			if (text.EmptyNull().StartsWith("@"))
			{
				return T(text.Substring(1));
			}

			return text;
		}

		[NonAction]
		public override IList<string> ValidatePaymentForm(FormCollection form)
		{
			var warnings = new List<string>();
			IValidator validator;

			string type = form["OfflinePaymentMethodType"].NullEmpty();

			if (type.HasValue())
			{
				if (type == "Manual")
				{
					validator = new ManualPaymentInfoValidator(_services.Localization);
					var model = new ManualPaymentInfoModel
					{
						CardholderName = form["CardholderName"],
						CardNumber = form["CardNumber"],
						CardCode = form["CardCode"]
					};
					var validationResult = validator.Validate(model);
					if (!validationResult.IsValid)
					{
						validationResult.Errors.Each(x => warnings.Add(x.ErrorMessage));
					}
				}
				else if (type == "DirectDebit")
				{
					// [...]
				}
			}

			return warnings;
		}

		[NonAction]
		public override ProcessPaymentRequest GetPaymentInfo(FormCollection form)
		{
			var paymentInfo = new ProcessPaymentRequest();

			string type = form["OfflinePaymentMethodType"].NullEmpty();

			if (type.HasValue())
			{
				if (type == "Manual")
				{
					paymentInfo.CreditCardType = form["CreditCardType"];
					paymentInfo.CreditCardName = form["CardholderName"];
					paymentInfo.CreditCardNumber = form["CardNumber"];
					paymentInfo.CreditCardExpireMonth = int.Parse(form["ExpireMonth"]);
					paymentInfo.CreditCardExpireYear = int.Parse(form["ExpireYear"]);
					paymentInfo.CreditCardCvv2 = form["CardCode"];
				}
				else if (type == "DirectDebit")
				{
					// [...]
				}
			}

			return paymentInfo;
		}

		[NonAction]
		public override string GetPaymentSummary(FormCollection form)
		{
			string type = form["OfflinePaymentMethodType"].NullEmpty();

			if (type.HasValue())
			{
				if (type == "Manual")
				{
					var number = form["CardNumber"];
					var len = number.Length;
					return "{0}, {1}, {2}".FormatCurrent(
						form["CreditCardType"],
						form["CardholderName"],
						number.Substring(0, 4) + new String('*', len - 4)
					);
				}
				else if (type == "DirectDebit")
				{
					if (form["DirectDebitAccountNumber"].HasValue() && (form["DirectDebitBankCode"].HasValue()) && form["DirectDebitAccountHolder"].HasValue())
					{
						var number = form["DirectDebitAccountNumber"];
						var len = number.Length;
						return "{0}, {1}, {2}".FormatCurrent(
							form["DirectDebitAccountHolder"],
							form["DirectDebitBankName"] ?? form["DirectDebitBankCode"],
							number.Substring(0, 4) + new String('*', len - 4)
						);
					}
					else if (form["DirectDebitIban"].HasValue())
					{
						var number = form["DirectDebitIban"];
						var len = number.Length;
						return number.Substring(0, 8) + new String('*', len - 8);
					}
				}
			}

			return null;
		}

		#endregion


		#region CashOnDelivery

		[AdminAuthorize]
		[ChildActionOnly]
		public ActionResult CashOnDeliveryConfigure()
		{
			var model = ConfigureGet<CashOnDeliveryConfigurationModel, CashOnDeliveryPaymentSettings>();
			return View("GenericConfigure", model);
		}

		[HttpPost]
		[AdminAuthorize]
		[ChildActionOnly]
		[ValidateInput(false)]
		public ActionResult CashOnDeliveryConfigure(CashOnDeliveryConfigurationModel model, FormCollection form)
		{
			if (!ModelState.IsValid)
				return CashOnDeliveryConfigure();

			var settings = ConfigurePost<CashOnDeliveryConfigurationModel, CashOnDeliveryPaymentSettings>(model);
			_services.Settings.SaveSetting(settings);

			return View("GenericConfigure", model);
		}

		public ActionResult CashOnDeliveryPaymentInfo() 
		{
			var model = PaymentInfoGet<CashOnDeliveryPaymentInfoModel, CashOnDeliveryPaymentSettings>();
			return PartialView("GenericPaymentInfo", model);
		}

		#endregion


		#region Invoice

		[AdminAuthorize]
		[ChildActionOnly]
		public ActionResult InvoiceConfigure()
		{
			var model = ConfigureGet<InvoiceConfigurationModel, InvoicePaymentSettings>();
			return View("GenericConfigure", model);
		}

		[HttpPost]
		[AdminAuthorize]
		[ChildActionOnly]
		[ValidateInput(false)]
		public ActionResult InvoiceConfigure(InvoiceConfigurationModel model, FormCollection form)
		{
			if (!ModelState.IsValid)
				return InvoiceConfigure();

			var settings = ConfigurePost<InvoiceConfigurationModel, InvoicePaymentSettings>(model);
			_services.Settings.SaveSetting(settings);

			return View("GenericConfigure", model);
		}

		public ActionResult InvoicePaymentInfo()
		{
			var model = PaymentInfoGet<InvoicePaymentInfoModel, InvoicePaymentSettings>();
			return PartialView("GenericPaymentInfo", model);
		}

		#endregion


		#region PayInStore

		[AdminAuthorize]
		[ChildActionOnly]
		public ActionResult PayInStoreConfigure()
		{
			var model = ConfigureGet<PayInStoreConfigurationModel, PayInStorePaymentSettings>();
			return View("GenericConfigure", model);
		}

		[HttpPost]
		[AdminAuthorize]
		[ChildActionOnly]
		[ValidateInput(false)]
		public ActionResult PayInStoreConfigure(PayInStoreConfigurationModel model, FormCollection form)
		{
			if (!ModelState.IsValid)
				return PayInStoreConfigure();

			var settings = ConfigurePost<PayInStoreConfigurationModel, PayInStorePaymentSettings>(model);
			_services.Settings.SaveSetting(settings);

			return View("GenericConfigure", model);
		}

		public ActionResult PayInStorePaymentInfo()
		{
			var model = PaymentInfoGet<PayInStorePaymentInfoModel, PayInStorePaymentSettings>();
			return PartialView("GenericPaymentInfo", model);
		}

		#endregion


		#region Prepayment

		[AdminAuthorize]
		[ChildActionOnly]
		public ActionResult PrepaymentConfigure()
		{
			var model = ConfigureGet<PrepaymentConfigurationModel, PrepaymentPaymentSettings>();
			return View("GenericConfigure", model);
		}

		[HttpPost]
		[AdminAuthorize]
		[ChildActionOnly]
		[ValidateInput(false)]
		public ActionResult PrepaymentConfigure(PrepaymentConfigurationModel model, FormCollection form)
		{
			if (!ModelState.IsValid)
				return PrepaymentConfigure();

			var settings = ConfigurePost<PrepaymentConfigurationModel, PrepaymentPaymentSettings>(model);
			_services.Settings.SaveSetting(settings);

			return View("GenericConfigure", model);
		}

		public ActionResult PrepaymentPaymentInfo()
		{
			var model = PaymentInfoGet<PrepaymentPaymentInfoModel, PrepaymentPaymentSettings>();
			return PartialView("GenericPaymentInfo", model);
		}

		#endregion


		#region Manual

		[AdminAuthorize]
		[ChildActionOnly]
		public ActionResult ManualConfigure()
		{
			var model = ConfigureGet<ManualConfigurationModel, ManualPaymentSettings>((m, s) => 
			{
				m.TransactMode = Convert.ToInt32(s.TransactMode);
				m.TransactModeValues = s.TransactMode.ToSelectList();		
			});

			return View(model);
		}

		[HttpPost]
		[AdminAuthorize]
		[ChildActionOnly]
		[ValidateInput(false)]
		public ActionResult ManualConfigure(ManualConfigurationModel model, FormCollection form)
		{
			if (!ModelState.IsValid)
				return ManualConfigure();

			var settings = ConfigurePost<ManualConfigurationModel, ManualPaymentSettings>(model);

			settings.TransactMode = (TransactMode)model.TransactMode;
			model.TransactModeValues = settings.TransactMode.ToSelectList();

			_services.Settings.SaveSetting(settings);

			return View(model);
		}

		public ActionResult ManualPaymentInfo()
		{
			var model = PaymentInfoGet<ManualPaymentInfoModel, ManualPaymentSettings>();

			// CC types
			model.CreditCardTypes.Add(new SelectListItem()
			{
				Text = "Visa",
				Value = "Visa",
			});
			model.CreditCardTypes.Add(new SelectListItem()
			{
				Text = "Master card",
				Value = "MasterCard",
			});
			model.CreditCardTypes.Add(new SelectListItem()
			{
				Text = "Discover",
				Value = "Discover",
			});
			model.CreditCardTypes.Add(new SelectListItem()
			{
				Text = "Amex",
				Value = "Amex",
			});

			// years
			for (int i = 0; i < 15; i++)
			{
				string year = Convert.ToString(DateTime.Now.Year + i);
				model.ExpireYears.Add(new SelectListItem()
				{
					Text = year,
					Value = year,
				});
			}

			// months
			for (int i = 1; i <= 12; i++)
			{
				string text = (i < 10) ? "0" + i.ToString() : i.ToString();
				model.ExpireMonths.Add(new SelectListItem()
				{
					Text = text,
					Value = i.ToString(),
				});
			}

			// set postback values
			var form = this.GetPaymentData();
			model.CardholderName = form["CardholderName"];
			model.CardNumber = form["CardNumber"];
			model.CardCode = form["CardCode"];
			var selectedCcType = model.CreditCardTypes.Where(x => x.Value.Equals(form["CreditCardType"], StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
			if (selectedCcType != null)
				selectedCcType.Selected = true;
			var selectedMonth = model.ExpireMonths.Where(x => x.Value.Equals(form["ExpireMonth"], StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
			if (selectedMonth != null)
				selectedMonth.Selected = true;
			var selectedYear = model.ExpireYears.Where(x => x.Value.Equals(form["ExpireYear"], StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
			if (selectedYear != null)
				selectedYear.Selected = true;

			return PartialView(model);
		}

		#endregion

	}
}