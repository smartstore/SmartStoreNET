using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Autofac;
using FluentValidation;
using FluentValidation.Results;
using SmartStore.OfflinePayment.Models;
using SmartStore.OfflinePayment.Settings;
using SmartStore.OfflinePayment.Validators;
using SmartStore.Services;
using SmartStore.Services.Payments;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Security;
using SmartStore.Web.Framework.Settings;
using SmartStore.Services.Media;
using SmartStore.Web.Framework.Theming;

namespace SmartStore.OfflinePayment.Controllers
{

	public class OfflinePaymentController : PaymentControllerBase
    {
		private readonly IComponentContext _ctx;
		private readonly HttpContextBase _httpContext;
		private readonly IPictureService _pictureService;

		public OfflinePaymentController(
			HttpContextBase httpContext,
			IComponentContext ctx,
			IPictureService pictureService)
        {
			_httpContext = httpContext;
			_ctx = ctx;
			_pictureService = pictureService;
		}

		#region Global

		private List<SelectListItem> GetTransactModes()
		{
			var list = new List<SelectListItem>
			{
				new SelectListItem { Text = T("Enums.SmartStore.Core.Domain.Payments.PaymentStatus.Pending"), Value = ((int)TransactMode.Pending).ToString() },
				new SelectListItem { Text = T("Enums.SmartStore.Core.Domain.Payments.PaymentStatus.Authorized"), Value = ((int)TransactMode.Authorize).ToString() },
				new SelectListItem { Text = T("Enums.SmartStore.Core.Domain.Payments.PaymentStatus.Paid"), Value = ((int)TransactMode.Paid).ToString() }
			};

			return list;
		}

		[NonAction]
		private TModel ConfigureGet<TModel, TSetting>(Action<TModel, TSetting> fn = null)
			where TModel : ConfigurationModelBase, new()
			where TSetting : PaymentSettingsBase, new()
		{
			var model = new TModel();

			var storeScope = this.GetActiveStoreScopeConfiguration(Services.StoreService, Services.WorkContext);
			var settings = Services.Settings.LoadSetting<TSetting>(storeScope);
			var store = storeScope == 0
				? Services.StoreContext.CurrentStore
				: Services.StoreService.GetStoreById(storeScope);

			model.PrimaryStoreCurrencyCode = store.PrimaryStoreCurrency.CurrencyCode;
			model.DescriptionText = settings.DescriptionText;
			model.PaymentMethodLogo = settings.ThumbnailPictureId;
			model.AdditionalFee = settings.AdditionalFee;
			model.AdditionalFeePercentage = settings.AdditionalFeePercentage;

			if (fn != null)
			{
				fn(model, settings);
			}

			var storeDependingSettingHelper = new StoreDependingSettingHelper(ViewData);
			storeDependingSettingHelper.GetOverrideKeys(settings, model, storeScope, Services.Settings);

			return model;
		}

		[NonAction]
		private void ConfigurePost<TModel, TSetting>(TModel model, FormCollection form, Action<TSetting> fn = null)
			where TModel : ConfigurationModelBase, new()
			where TSetting : PaymentSettingsBase, new()
		{
			ModelState.Clear();

			var storeDependingSettingHelper = new StoreDependingSettingHelper(ViewData);
			int storeScope = this.GetActiveStoreScopeConfiguration(Services.StoreService, Services.WorkContext);
			var settings = Services.Settings.LoadSetting<TSetting>(storeScope);

			settings.DescriptionText = model.DescriptionText;
			settings.ThumbnailPictureId = model.PaymentMethodLogo;
			settings.AdditionalFee = model.AdditionalFee;
			settings.AdditionalFeePercentage = model.AdditionalFeePercentage;

			if (fn != null)
			{
				fn(settings);
			}

			using (Services.Settings.BeginScope())
			{
				storeDependingSettingHelper.UpdateSettings(settings, form, storeScope, Services.Settings);
			}

			NotifySuccess(T("Admin.Common.DataSuccessfullySaved"));
		}

		[NonAction]
		private TModel PaymentInfoGet<TModel, TSetting>(Action<TModel, TSetting> fn = null)
			where TModel : PaymentInfoModelBase, new()
			where TSetting : PaymentSettingsBase, new()
		{
			var settings = _ctx.Resolve<TSetting>();
			var model = new TModel();
			model.DescriptionText = GetLocalizedText(settings.DescriptionText);
			model.ThumbnailUrl = _pictureService.GetUrl(settings.ThumbnailPictureId, 120, false); 

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
			ValidationResult validationResult = null;

			string type = form["OfflinePaymentMethodType"].NullEmpty();

			if (type.HasValue())
			{
				if (type == "Manual")
				{
					validator = new ManualPaymentInfoValidator(Services.Localization);
					var model = new ManualPaymentInfoModel
					{
						CardholderName = form["CardholderName"],
						CardNumber = form["CardNumber"],
						CardCode = form["CardCode"]
					};
					validationResult = validator.Validate(model);
				}
				else if (type == "DirectDebit")
				{
					validator = new DirectDebitPaymentInfoValidator(Services.Localization);
					var model = new DirectDebitPaymentInfoModel
					{
						EnterIBAN = form["EnterIBAN"],
						DirectDebitAccountHolder = form["DirectDebitAccountHolder"],
						DirectDebitAccountNumber = form["DirectDebitAccountNumber"],
						DirectDebitBankCode = form["DirectDebitBankCode"],
						DirectDebitCountry = form["DirectDebitCountry"],
						DirectDebitBankName = form["DirectDebitBankName"],
						DirectDebitIban = form["DirectDebitIban"],
						DirectDebitBic = form["DirectDebitBic"]
					};
					validationResult = validator.Validate(model);
				}

				if (validationResult != null && !validationResult.IsValid)
				{
					validationResult.Errors.Each(x => warnings.Add(x.ErrorMessage));
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
					paymentInfo.CreditCardExpireMonth = int.Parse(form["ExpireMonth"].SplitSafe(",")[0]);
					paymentInfo.CreditCardExpireYear = int.Parse(form["ExpireYear"].SplitSafe(",")[0]);
					paymentInfo.CreditCardCvv2 = form["CardCode"];
				}
				else if (type == "DirectDebit")
				{
					paymentInfo.DirectDebitAccountHolder = form["DirectDebitAccountHolder"];
					paymentInfo.DirectDebitAccountNumber = form["DirectDebitAccountNumber"];
					paymentInfo.DirectDebitBankCode = form["DirectDebitBankCode"];
					paymentInfo.DirectDebitBankName = form["DirectDebitBankName"];
					paymentInfo.DirectDebitBic = form["DirectDebitBic"];
					paymentInfo.DirectDebitCountry = form["DirectDebitCountry"];
					paymentInfo.DirectDebitIban = form["DirectDebitIban"];
				}
                else if (type == "PurchaseOrderNumber")
                {
                    paymentInfo.PurchaseOrderNumber = form["PurchaseOrderNumber"];
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
					return "{0}, {1}, {2}".FormatCurrent(
						form["CreditCardType"],
						form["CardholderName"],
						number.Mask(4)
					);
				}
				else if (type == "DirectDebit")
				{
					if (form["DirectDebitAccountNumber"].HasValue() && (form["DirectDebitBankCode"].HasValue()) && form["DirectDebitAccountHolder"].HasValue())
					{
						var number = form["DirectDebitAccountNumber"];
						return "{0}, {1}, {2}".FormatCurrent(
							form["DirectDebitAccountHolder"],
							form["DirectDebitBankName"].NullEmpty() ?? form["DirectDebitBankCode"],
							number.Mask(4)
						);
					}
					else if (form["DirectDebitIban"].HasValue())
					{
						var number = form["DirectDebitIban"];
						return number.Mask(8);
					}
				}
                else if (type == "PurchaseOrderNumber")
                {
                    return form["PurchaseOrderNumber"];
                }
			}

			return null;
		}

		#endregion
		
		#region CashOnDelivery

		[AdminAuthorize, AdminThemed, ChildActionOnly]
		public ActionResult CashOnDeliveryConfigure()
		{
			var model = ConfigureGet<CashOnDeliveryConfigurationModel, CashOnDeliveryPaymentSettings>();

			return View("GenericConfigure", model);
		}

		[HttpPost, AdminAuthorize, AdminThemed, ChildActionOnly, ValidateInput(false)]
		public ActionResult CashOnDeliveryConfigure(CashOnDeliveryConfigurationModel model, FormCollection form)
		{
			if (!ModelState.IsValid)
				return CashOnDeliveryConfigure();

			ConfigurePost<CashOnDeliveryConfigurationModel, CashOnDeliveryPaymentSettings>(model, form);

			return CashOnDeliveryConfigure();
		}

		public ActionResult CashOnDeliveryPaymentInfo() 
		{
			var model = PaymentInfoGet<CashOnDeliveryPaymentInfoModel, CashOnDeliveryPaymentSettings>();
			return PartialView("GenericPaymentInfo", model);
		}

		#endregion
		
		#region Invoice

		[ChildActionOnly, AdminThemed, AdminAuthorize]
		public ActionResult InvoiceConfigure()
		{
			var model = ConfigureGet<InvoiceConfigurationModel, InvoicePaymentSettings>();

			return View("GenericConfigure", model);
		}

		[HttpPost, AdminAuthorize, AdminThemed, ChildActionOnly, ValidateInput(false)]
		public ActionResult InvoiceConfigure(InvoiceConfigurationModel model, FormCollection form)
		{
			if (!ModelState.IsValid)
				return InvoiceConfigure();

			ConfigurePost<InvoiceConfigurationModel, InvoicePaymentSettings>(model, form);

			return InvoiceConfigure();
		}

		public ActionResult InvoicePaymentInfo()
		{
			var model = PaymentInfoGet<InvoicePaymentInfoModel, InvoicePaymentSettings>();
			return PartialView("GenericPaymentInfo", model);
		}

		#endregion
		
		#region PayInStore

		[ChildActionOnly, AdminThemed, AdminAuthorize]
		public ActionResult PayInStoreConfigure()
		{
			var model = ConfigureGet<PayInStoreConfigurationModel, PayInStorePaymentSettings>();

			return View("GenericConfigure", model);
		}

		[HttpPost, AdminAuthorize, AdminThemed, ChildActionOnly, ValidateInput(false)]
		public ActionResult PayInStoreConfigure(PayInStoreConfigurationModel model, FormCollection form)
		{
			if (!ModelState.IsValid)
				return PayInStoreConfigure();

			ConfigurePost<PayInStoreConfigurationModel, PayInStorePaymentSettings>(model, form);

			return PayInStoreConfigure();
		}

		public ActionResult PayInStorePaymentInfo()
		{
			var model = PaymentInfoGet<PayInStorePaymentInfoModel, PayInStorePaymentSettings>();
			return PartialView("GenericPaymentInfo", model);
		}

		#endregion
		
		#region Prepayment

		[AdminAuthorize, AdminThemed, ChildActionOnly]
		public ActionResult PrepaymentConfigure()
		{
			var model = ConfigureGet<PrepaymentConfigurationModel, PrepaymentPaymentSettings>();

			return View("GenericConfigure", model);
		}

		[HttpPost, AdminAuthorize, AdminThemed, ChildActionOnly, ValidateInput(false)]
		public ActionResult PrepaymentConfigure(PrepaymentConfigurationModel model, FormCollection form)
		{
			if (!ModelState.IsValid)
				return PrepaymentConfigure();

			ConfigurePost<PrepaymentConfigurationModel, PrepaymentPaymentSettings>(model, form);

			return PrepaymentConfigure();
		}

		public ActionResult PrepaymentPaymentInfo()
		{
			var model = PaymentInfoGet<PrepaymentPaymentInfoModel, PrepaymentPaymentSettings>();
			return PartialView("GenericPaymentInfo", model);
		}

		#endregion
		
		#region DirectDebit

		[AdminAuthorize, AdminThemed, ChildActionOnly]
		public ActionResult DirectDebitConfigure()
		{
			var model = ConfigureGet<DirectDebitConfigurationModel, DirectDebitPaymentSettings>();

			return View("GenericConfigure", model);
		}

		[HttpPost, AdminAuthorize, AdminThemed, ChildActionOnly, ValidateInput(false)]
		public ActionResult DirectDebitConfigure(DirectDebitConfigurationModel model, FormCollection form)
		{
			if (!ModelState.IsValid)
				return DirectDebitConfigure();

			ConfigurePost<DirectDebitConfigurationModel, DirectDebitPaymentSettings>(model, form);

			return DirectDebitConfigure();
		}

		public ActionResult DirectDebitPaymentInfo()
		{
			var model = PaymentInfoGet<DirectDebitPaymentInfoModel, DirectDebitPaymentSettings>();
			var paymentData = _httpContext.GetCheckoutState().PaymentData;

			model.DirectDebitAccountHolder = (string)paymentData.Get("DirectDebitAccountHolder");
			model.DirectDebitAccountNumber = (string)paymentData.Get("DirectDebitAccountNumber");
			model.DirectDebitBankCode = (string)paymentData.Get("DirectDebitBankCode");
			model.DirectDebitBankName = (string)paymentData.Get("DirectDebitBankName");
			model.DirectDebitBic = (string)paymentData.Get("DirectDebitBic");
			model.DirectDebitCountry = (string)paymentData.Get("DirectDebitCountry");
			model.DirectDebitIban = (string)paymentData.Get("DirectDebitIban");

			return PartialView(model);
		}

		#endregion
		
		#region Manual

		[AdminAuthorize, AdminThemed, ChildActionOnly]
		public ActionResult ManualConfigure()
		{
			var model = ConfigureGet<ManualConfigurationModel, ManualPaymentSettings>((m, s) => 
			{
				m.TransactMode = s.TransactMode;
				m.TransactModeValues = GetTransactModes();
				m.ExcludedCreditCards = s.ExcludedCreditCards.SplitSafe(",");		

				m.AvailableCreditCards = ManualProvider.CreditCardTypes
					.Select(x => new SelectListItem
					{
						Text = x.Text,
						Value = x.Value,
						Selected = m.ExcludedCreditCards.Contains(x.Value)
					})
					.ToList();
			});

			return View(model);
		}

		[HttpPost, AdminAuthorize, AdminThemed, ChildActionOnly, ValidateInput(false)]
		public ActionResult ManualConfigure(ManualConfigurationModel model, FormCollection form)
		{
			if (!ModelState.IsValid)
				return ManualConfigure();

			ConfigurePost<ManualConfigurationModel, ManualPaymentSettings>(model, form, s =>
			{
				s.TransactMode = model.TransactMode;
				s.ExcludedCreditCards = string.Join(",", model.ExcludedCreditCards ?? new string[0]);
			});

			return ManualConfigure();
		}

		public ActionResult ManualPaymentInfo()
		{
			var model = PaymentInfoGet<ManualPaymentInfoModel, ManualPaymentSettings>((m, s) =>
			{
				var excludedCreditCards = s.ExcludedCreditCards.SplitSafe(",");

				foreach (var creditCard in ManualProvider.CreditCardTypes)
				{
					if (!excludedCreditCards.Any(x => x.IsCaseInsensitiveEqual(creditCard.Value)))
					{
						m.CreditCardTypes.Add(new SelectListItem
						{
							Text = creditCard.Text,
							Value = creditCard.Value
						});
					}
				}
			});

			// years
			for (int i = 0; i < 15; i++)
			{
				string year = Convert.ToString(DateTime.Now.Year + i);
				model.ExpireYears.Add(new SelectListItem { Text = year,	Value = year });
			}

			// months
			for (int i = 1; i <= 12; i++)
			{
				string text = (i < 10) ? "0" + i.ToString() : i.ToString();
				model.ExpireMonths.Add(new SelectListItem { Text = text, Value = i.ToString() });
			}

			// set postback values
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

		#endregion

        #region PurchaseOrderNumber

        [AdminAuthorize, AdminThemed, ChildActionOnly]
        public ActionResult PurchaseOrderNumberConfigure()
        {
            var model = ConfigureGet<PurchaseOrderNumberConfigurationModel, PurchaseOrderNumberPaymentSettings>();

            return View("GenericConfigure", model);
        }

        [HttpPost, AdminAuthorize, AdminThemed, ChildActionOnly, ValidateInput(false)]
        public ActionResult PurchaseOrderNumberConfigure(PurchaseOrderNumberConfigurationModel model, FormCollection form)
        {
            if (!ModelState.IsValid)
				return PurchaseOrderNumberConfigure();

			ConfigurePost<PurchaseOrderNumberConfigurationModel, PurchaseOrderNumberPaymentSettings>(model, form);

            return PurchaseOrderNumberConfigure();
        }

        public ActionResult PurchaseOrderNumberPaymentInfo()
        {
            var model = PaymentInfoGet<PurchaseOrderNumberPaymentInfoModel, InvoicePaymentSettings>();
			var paymentData = _httpContext.GetCheckoutState().PaymentData;

            model.PurchaseOrderNumber = (string)paymentData.Get("PurchaseOrderNumber");

            return PartialView("PurchaseOrderNumberPaymentInfo", model);
        }

        #endregion
    }
}