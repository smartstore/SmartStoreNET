using SmartStore.AmazonPay.Services;
using SmartStore.Web.Framework.Modelling;

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
			ButtonType = (Type == AmazonPayRequestType.LoginPage ? "Login" : "PwA");
			UseLoginPopupWindow = true;
		}

		public string SystemName { get { return AmazonPayPlugin.SystemName; } }

		public string SellerId { get; set; }
		public string ClientId { get; set; }

		public string WidgetUrl { get; set; }
		public string ButtonUrl { get; set; }
		public string LoginHandlerUrl { get; set; }

		public bool IsShippable { get; set; }
		public bool IsRecurring { get; set; }

		public string LanguageCode { get; set; }
		public AmazonPayRequestType Type { get; set; }
		public AmazonPayResultType Result { get; set; }
		
		public string RedirectAction { get; set; }
		public string RedirectController { get; set; }

		public string OrderReferenceId { get; set; }
		public string Warning { get; set; }

		public string ButtonType { get; set; }
		public string ButtonColor { get; set; }
		public string ButtonSize { get; set; }
		public bool UseLoginPopupWindow { get; set; }

		public int AddressWidgetWidth { get; set; }
		public int AddressWidgetHeight { get; set; }

		public int PaymentWidgetWidth { get; set; }
		public int PaymentWidgetHeight { get; set; }

		public bool DisplayRewardPoints { get; set; }
		public int RewardPointsBalance { get; set; }
		public string RewardPointsAmount { get; set; }
		public bool UseRewardPoints { get; set; }

		public string ShippingMethod { get; set; }

		public string WidgetId
		{
			get
			{
				return "AmazonPay" + Type.ToString();
			}
		}
    }
}