using System.Collections.Generic;
using System.Web.Mvc;
using Autofac;
using SmartStore.Core.Localization;
using SmartStore.OfflinePayment.Models;
using SmartStore.OfflinePayment.Settings;
using SmartStore.Services;
using SmartStore.Services.Payments;
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
		private TModel ConfigureGet<TModel, TSetting>() 
			where TModel : ConfigurationModelBase, new()
			where TSetting : PaymentSettingsBase, new()
		{
			var settings = _ctx.Resolve<TSetting>();
			var model = new TModel();

			model.DescriptionText = settings.DescriptionText;
			model.AdditionalFee = settings.AdditionalFee;
			model.AdditionalFeePercentage = settings.AdditionalFeePercentage;

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
		private TModel PaymentInfoGet<TModel, TSetting>()
			where TModel : PaymentInfoModelBase, new()
			where TSetting : PaymentSettingsBase, new()
		{
			var settings = _ctx.Resolve<TSetting>();
			var model = new TModel();
			model.DescriptionText = GetLocalizedText(settings.DescriptionText);

			return model;
		}

		private string GetLocalizedText(string text)
		{
			if (text.StartsWith("@"))
			{
				return T(text.Substring(1));
			}

			return text;
		}

		[NonAction]
		public override IList<string> ValidatePaymentForm(FormCollection form)
		{
			var warnings = new List<string>();

			string type = form["PaymentMethodType"].NullEmpty();
			if (type.HasValue())
			{
				// [...]
			}

			return warnings;
		}

		[NonAction]
		public override ProcessPaymentRequest GetPaymentInfo(FormCollection form)
		{
			var paymentInfo = new ProcessPaymentRequest();

			string type = form["PaymentMethodType"].NullEmpty();
			if (type.HasValue())
			{
				// [...]
			}

			return paymentInfo;
		}

		#endregion


		#region CashOnDelivery

		[AdminAuthorize]
		[ChildActionOnly]
		public ActionResult CashOnDeliveryConfigure()
		{
			var model = ConfigureGet<CashOnDeliveryConfigurationModel, CashOnDeliveryPaymentSettings>();
			return View(model);
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

			return View(model);
		}

		[ChildActionOnly]
		public ActionResult CashOnDeliveryPaymentInfo()
		{
			var model = PaymentInfoGet<CashOnDeliveryPaymentInfoModel, CashOnDeliveryPaymentSettings>();
			return View(model);
		}

		#endregion

	}
}