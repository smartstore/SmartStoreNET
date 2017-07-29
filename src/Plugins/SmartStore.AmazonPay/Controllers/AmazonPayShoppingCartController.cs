﻿using System.Web.Mvc;
using SmartStore.AmazonPay.Services;

namespace SmartStore.AmazonPay.Controllers
{
	public class AmazonPayShoppingCartController : AmazonPayControllerBase
	{
		private readonly IAmazonPayService _apiService;

		public AmazonPayShoppingCartController(IAmazonPayService apiService)
		{
			_apiService = apiService;
		}

		public ActionResult PayButtonHandler(string orderReferenceId, string accessToken)
		{
			var model = _apiService.CreateViewModel(AmazonPayRequestType.PayButtonHandler, TempData, orderReferenceId, accessToken);

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

		//[ChildActionOnly]
		//public ActionResult WidgetLibrary()
		//{
		//	// Not possible to load it asynchronously cause of document.write inside.
		//	var widgetUrl = _apiService.GetWidgetUrl();

		//	if (widgetUrl.HasValue())
		//	{
		//		return this.Content("<script src=\"{0}\" type=\"text/javascript\"></script>".FormatWith(widgetUrl));
		//	}

		//	return new EmptyResult();
		//}
	}
}