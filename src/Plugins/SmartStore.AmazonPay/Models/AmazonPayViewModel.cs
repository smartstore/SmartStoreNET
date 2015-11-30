using System;
using SmartStore.AmazonPay.Services;
using SmartStore.Web.Framework.Mvc;

namespace SmartStore.AmazonPay.Models
{
	public class AmazonPayViewModel : ModelBase
    {
		public AmazonPayViewModel()
		{
			IsShippable = true;
			RedirectAction = "Cart";
			RedirectController = "ShoppingCart";
			Result = AmazonPayResultType.PluginView;
			WidgetUrl = AmazonPayCore.UrlWidgetProduction.FormatWith("de");
		}

		public string SystemName { get { return AmazonPayCore.SystemName; } }

		public string SellerId { get; set; }
		public string ClientId { get; set; }

		public string WidgetUrl { get; set; }
		public string ButtonUrl { get; set; }
		public string LoginHandlerUrl { get; set; }

		public bool IsShippable { get; set; }
		public bool IsRecurring { get; set; }

		public AmazonPayRequestType Type { get; set; }
		public AmazonPayResultType Result { get; set; }
		public string RedirectAction { get; set; }
		public string RedirectController { get; set; }

		public string OrderReferenceId { get; set; }
		public string Warning { get; set; }

		public int AddressWidgetWidth { get; set; }
		public int AddressWidgetHeight { get; set; }

		public int PaymentWidgetWidth { get; set; }
		public int PaymentWidgetHeight { get; set; }

		public bool DisplayRewardPoints { get; set; }
		public int RewardPointsBalance { get; set; }
		public string RewardPointsAmount { get; set; }
		public bool UseRewardPoints { get; set; }

		public string ShippingMethod { get; set; }

		public string GetWidgetId
		{
			get
			{
				return "AmazonPay" + Type.ToString();
			}
		}
    }
}