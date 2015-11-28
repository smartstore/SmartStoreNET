using System.Web.Mvc;
using SmartStore.AmazonPay.Services;

namespace SmartStore.AmazonPay.Controllers
{
	public class AmazonPayCheckoutController : AmazonPayControllerBase
	{
		private readonly IAmazonPayService _apiService;

		public AmazonPayCheckoutController(IAmazonPayService apiService)
		{
			_apiService = apiService;
		}

		public ActionResult BillingAddress()
		{
			return RedirectToAction("ShippingAddress", "Checkout", new { area = "" });
		}

		public ActionResult ShippingAddress()
		{
			var model = _apiService.ProcessPluginRequest(AmazonPayRequestType.Address, TempData);

			return GetActionResult(model);
		}

		public ActionResult PaymentMethod()
		{
			var model = _apiService.ProcessPluginRequest(AmazonPayRequestType.Payment, TempData);

			return GetActionResult(model);
		}

		[HttpPost]
		public ActionResult PaymentMethod(bool? UseRewardPoints)
		{
			_apiService.ApplyRewardPoints(UseRewardPoints ?? false);

			return RedirectToAction("Confirm", "Checkout", new { area = "" });
		}

		public ActionResult PaymentInfo()
		{
			return RedirectToAction("PaymentMethod", "Checkout", new { area = "" });
		}
	}
}