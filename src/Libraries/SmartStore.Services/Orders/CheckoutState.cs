using System.Collections.Generic;

namespace SmartStore.Services.Orders
{
	public partial class CheckoutState
	{
		public CheckoutState()
		{
			CustomProperties = new Dictionary<string, object>();
		}

		public static string CheckoutStateSessionKey { get { return "SmCheckoutState"; } }

		/// <summary>
		/// Whether the one page checkout is enabled for a particular session
		/// </summary>
		public bool OnePageCheckoutEnabled { get; set; }

		/// <summary>
		/// Use that dictionary for any custom data required along checkout flow
		/// </summary>
		public Dictionary<string, object> CustomProperties { get; set; }
	}
}
