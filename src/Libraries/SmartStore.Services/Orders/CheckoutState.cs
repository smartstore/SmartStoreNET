using System;
using System.Collections.Generic;
using System.Web.Routing;

namespace SmartStore.Services.Orders
{
	[Serializable]
	public partial class CheckoutState
	{
		public CheckoutState()
		{
			CustomProperties = new RouteValueDictionary();
			PaymentData = new Dictionary<string, object>();
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
		/// Indicates whether the payment method selection page was skipped
		/// </summary>
		public bool IsPaymentSelectionSkipped { get; set; }

		/// <summary>
		/// Use this dictionary for any custom data required along checkout flow
		/// </summary>
		public IDictionary<string, object> CustomProperties { get; set; }

		/// <summary>
		/// Payment data entered on payment method selection page
		/// </summary>
		public IDictionary<string, object> PaymentData { get; set; }
	}
}
