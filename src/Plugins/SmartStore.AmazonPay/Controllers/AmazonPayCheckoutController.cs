using System.Web.Mvc;
using SmartStore.AmazonPay.Services;
using SmartStore.Core.Domain.Customers;
using SmartStore.Services;
using SmartStore.Services.Common;

namespace SmartStore.AmazonPay.Controllers
{
	public class AmazonPayCheckoutController : AmazonPayControllerBase
	{
		private readonly IAmazonPayService _apiService;
		private readonly ICommonServices _services;
		private readonly IGenericAttributeService _genericAttributeService;
		private readonly RewardPointsSettings _rewardPointsSettings;

		public AmazonPayCheckoutController(
			IAmazonPayService apiService,
			ICommonServices services,
			IGenericAttributeService genericAttributeService,
			RewardPointsSettings rewardPointsSettings)
		{
			_apiService = apiService;
			_services = services;
			_genericAttributeService = genericAttributeService;
			_rewardPointsSettings = rewardPointsSettings;
		}

		public ActionResult BillingAddress()
		{
			return RedirectToAction("ShippingAddress", "Checkout", new { area = "" });
		}

		public ActionResult ShippingAddress()
		{
			var model = _apiService.CreateViewModel(AmazonPayRequestType.Address, TempData);

			return GetActionResult(model);
		}

		public ActionResult PaymentMethod()
		{
			var model = _apiService.CreateViewModel(AmazonPayRequestType.PaymentMethod, TempData);

			return GetActionResult(model);
		}

		[HttpPost]
		public ActionResult PaymentMethod(bool? UseRewardPoints)
		{
			if (_rewardPointsSettings.Enabled)
			{
				_genericAttributeService.SaveAttribute(_services.WorkContext.CurrentCustomer,
					SystemCustomerAttributeNames.UseRewardPointsDuringCheckout, UseRewardPoints ?? false, _services.StoreContext.CurrentStore.Id);
			}

			return RedirectToAction("Confirm", "Checkout", new { area = "" });
		}

		public ActionResult PaymentInfo()
		{
			return RedirectToAction("PaymentMethod", "Checkout", new { area = "" });
		}
	}
}