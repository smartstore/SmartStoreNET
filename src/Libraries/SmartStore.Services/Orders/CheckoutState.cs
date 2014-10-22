using System.Collections.Generic;
using System.Web.Routing;

namespace SmartStore.Services.Orders
{
	public partial class CheckoutState
	{
		public CheckoutState()
		{
			CustomProperties = new RouteValueDictionary();
		}

		public static string CheckoutStateSessionKey { get { return "SmCheckoutState"; } }

		public string PaymentSummary
		{
			get
			{
				return CustomProperties["_PaymentSummary"] as string;
			}
			set
			{
				CustomProperties["_PaymentSummary"] = value;
			}
		}

		/// <summary>
		/// Use this dictionary for any custom data required along checkout flow
		/// </summary>
		public IDictionary<string, object> CustomProperties { get; set; }
	}
}
