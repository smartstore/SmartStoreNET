using System.Web;
using System.Web.Mvc;
using SmartStore.AmazonPay.Models;
using SmartStore.AmazonPay.Services;

namespace SmartStore.AmazonPay.Controllers
{
	public class AmazonPayShoppingCartController : AmazonPayControllerBase
	{
        private readonly HttpContextBase _httpContext;
        private readonly IAmazonPayService _apiService;

		public AmazonPayShoppingCartController(
            HttpContextBase httpContext,
            IAmazonPayService apiService)
		{
            _httpContext = httpContext;
			_apiService = apiService;
		}

		public ActionResult PayButtonHandler()
		{
			var model = _apiService.CreateViewModel(AmazonPayRequestType.PayButtonHandler, TempData);
			return GetActionResult(model);
		}

		[ChildActionOnly]
		public ActionResult ShoppingCart()
		{
			if (ControllerContext.ParentActionViewContext.RequestContext.RouteData.IsRouteEqual("ShoppingCart", "Cart"))
			{
				var model = _apiService.CreateViewModel(AmazonPayRequestType.ShoppingCart, TempData);

				return GetActionResult(model);
			}

			return new EmptyResult();
		}

		[ChildActionOnly]
		public ActionResult OrderReviewData(bool renderAmazonPayView)
		{
			if (renderAmazonPayView)
			{
				var model = _apiService.CreateViewModel(AmazonPayRequestType.OrderReviewData, TempData);

				return View(model);
			}
			return new EmptyResult();
		}

		[ChildActionOnly]
		public ActionResult MiniShoppingCart(bool renderAmazonPayView)
		{
			if (renderAmazonPayView)
			{
				var model = _apiService.CreateViewModel(AmazonPayRequestType.MiniShoppingCart, TempData);

				return GetActionResult(model);
			}
			return new EmptyResult();
		}

        #region Confirmation flow

        public ActionResult ConfirmationFlow()
        {
            var model = new ConfirmationFlowModel
            {
                TriggerPostOrderFlow = true
            };

            // Chrome calls this method twice!
            var confirmationFlowStarted = (TempData["ConfirmationFlowStarted"] as bool?) ?? false;
            if (!confirmationFlowStarted)
            {
                TempData["ConfirmationFlowStarted"] = true;

                _apiService.ConfirmOrderReference(out var redirectUrl);

                model.RedirectUrl = redirectUrl;
                model.TriggerPostOrderFlow = redirectUrl.IsEmpty();
            }

            var settings = Services.Settings.LoadSetting<AmazonPaySettings>(Services.StoreContext.CurrentStore.Id);
            var state = _httpContext.GetAmazonPayState(Services.Localization);

            model.WidgetUrl = AmazonPayService.GetWidgetUrl(settings);
            model.SellerId = settings.SellerId;
            model.OrderReferenceId = state.OrderReferenceId;

            return View(model);
        }

        public ActionResult ConfirmationSuccess()
        {
            "ConfirmationSuccess".Dump();
            return new EmptyResult();
        }

        public ActionResult ConfirmationFailure()
        {
            "ConfirmationFailure".Dump();
            return new EmptyResult();
        }

        #endregion
    }
}