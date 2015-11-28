using System.Collections.Generic;
using System.Web;
using System.Web.Routing;
using SmartStore.Web.Models.ShoppingCart;
using SmartStore.AmazonPay.Services;
using SmartStore.AmazonPay.Extensions;
using SmartStore.Services.Cms;
using SmartStore.Core.Plugins;

namespace SmartStore.AmazonPay.Widgets
{
	[SystemName("Widgets.AmazonPay")]
	[FriendlyName("Pay with Amazon")]
	public class AmazonPayWidget : IWidget
	{
		private readonly HttpContextBase _httpContext;

		public AmazonPayWidget(HttpContextBase httpContext)
		{
			_httpContext = httpContext;
		}

		public IList<string> GetWidgetZones()
		{
			return new List<string>()
			{
				"order_summary_content_before", "mobile_order_summary_content_before", 
				"mini_shopping_cart_bottom",
				"head_html_tag", "mobile_head_html_tag"
			};
		}

		public void GetDisplayWidgetRoute(string widgetZone, object model, int storeId, out string actionName, out string controllerName, out RouteValueDictionary routeValues)
		{
			bool renderAmazonPayView = true;

			if (widgetZone.IsCaseInsensitiveEqual("head_html_tag") || widgetZone.IsCaseInsensitiveEqual("mobile_head_html_tag"))
			{
				actionName = "WidgetLibrary";
			}
			else if (widgetZone.IsCaseInsensitiveEqual("mini_shopping_cart_bottom"))
			{
				actionName = "MiniShoppingCart";

				var viewModel = model as MiniShoppingCartModel;
				if (viewModel != null)
					renderAmazonPayView = viewModel.DisplayCheckoutButton;
			}
			else
			{
				actionName = "OrderReviewData";

				renderAmazonPayView = (_httpContext.HasAmazonPayState() && _httpContext.Request.RequestContext.RouteData.IsRouteEqual("Checkout", "Confirm"));

				if (renderAmazonPayView)
				{
					var viewModel = model as ShoppingCartModel;
					if (viewModel != null)
						viewModel.OrderReviewData.Display = false;
				}
			}

			controllerName = "AmazonPayShoppingCart";

			routeValues = new RouteValueDictionary()
            {
                { "Namespaces", "SmartStore.AmazonPay.Controllers" },
                { "area", AmazonPayCore.SystemName },
				{ "renderAmazonPayView", renderAmazonPayView }
            };
		}
	}
}