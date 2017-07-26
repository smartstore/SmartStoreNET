using System.Web;
using System.Web.Mvc;
using SmartStore.AmazonPay.Services;
using SmartStore.Core.Domain.Customers;
using SmartStore.Services.Common;

namespace SmartStore.AmazonPay.Controllers
{
	public class AmazonPayCheckoutController : AmazonPayControllerBase
	{
		private readonly HttpContextBase _httpContext;
		private readonly IAmazonPayService _apiService;
		private readonly IGenericAttributeService _genericAttributeService;
		private readonly RewardPointsSettings _rewardPointsSettings;

		public AmazonPayCheckoutController(
			HttpContextBase httpContext,
			IAmazonPayService apiService,
			IGenericAttributeService genericAttributeService,
			RewardPointsSettings rewardPointsSettings)
		{
			_httpContext = httpContext;
			_apiService = apiService;
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
				_genericAttributeService.SaveAttribute(Services.WorkContext.CurrentCustomer,
					SystemCustomerAttributeNames.UseRewardPointsDuringCheckout, UseRewardPoints ?? false, Services.StoreContext.CurrentStore.Id);
			}

			return RedirectToAction("Confirm", "Checkout", new { area = "" });
		}

		public ActionResult PaymentInfo()
		{
			return RedirectToAction("PaymentMethod", "Checkout", new { area = "" });
		}

		public ActionResult CheckoutCompleted()
		{
			var note = _httpContext.Session["AmazonPayCheckoutCompletedNote"] as string;
			if (note.HasValue())
			{
				return Content(note);
			}

			return new EmptyResult();
		}
	}
}