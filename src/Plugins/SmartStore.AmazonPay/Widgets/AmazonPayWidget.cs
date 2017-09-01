using System.Collections.Generic;
using System.Web;
using System.Web.Routing;
using SmartStore.Core.Plugins;
using SmartStore.Services.Cms;
using SmartStore.Web.Models.ShoppingCart;

namespace SmartStore.AmazonPay.Widgets
{
	[DisplayOrder(-1)]
	[SystemName("Widgets.AmazonPay")]
	[FriendlyName("Amazon Pay")]
	public class AmazonPayWidget : IWidget
	{
		private readonly HttpContextBase _httpContext;

		public AmazonPayWidget(HttpContextBase httpContext)
		{
			_httpContext = httpContext;
		}

		public IList<string> GetWidgetZones()
		{
			return new List<string>
			{
				"order_summary_content_before",
                "offcanvas_cart_summary",
				"checkout_completed_top"
			};
		}

		public void GetDisplayWidgetRoute(string widgetZone, object model, int storeId, out string actionName, out string controllerName, out RouteValueDictionary routeValues)
		{
			var renderAmazonPayView = true;

			if (widgetZone.IsCaseInsensitiveEqual("checkout_completed_top"))
			{
				actionName = "CheckoutCompleted";
				controllerName = "AmazonPayCheckout";
			}
			else if (widgetZone.IsCaseInsensitiveEqual("offcanvas_cart_summary"))
			{
				actionName = "MiniShoppingCart";
				controllerName = "AmazonPayShoppingCart";

				var viewModel = model as MiniShoppingCartModel;
				if (viewModel != null)
					renderAmazonPayView = viewModel.DisplayCheckoutButton;
			}
			else
			{
				actionName = "OrderReviewData";
				controllerName = "AmazonPayShoppingCart";

				renderAmazonPayView = (_httpContext.HasAmazonPayState() && _httpContext.Request.RequestContext.RouteData.IsRouteEqual("Checkout", "Confirm"));

				if (renderAmazonPayView)
				{
					var viewModel = model as ShoppingCartModel;
					if (viewModel != null)
						viewModel.OrderReviewData.Display = false;
				}
			}

			routeValues = new RouteValueDictionary
            {
                { "Namespaces", "SmartStore.AmazonPay.Controllers" },
                { "area", AmazonPayPlugin.SystemName },
				{ "renderAmazonPayView", renderAmazonPayView }
            };
		}
	}
}