using SmartStore.Core.Domain.Orders;
using SmartStore.Services.Localization;

namespace SmartStore.Services.Orders
{
	public class CancelOrderItemContext
	{
		/// <summary>
		/// [IN] Order item
		/// </summary>
		public OrderItem OrderItem { get; set; }

		/// <summary>
		/// [IN] Return request object
		/// </summary>
		public ReturnRequest ReturnRequest { get; set; }

		/// <summary>
		/// [IN] Whether to adjust the inventory
		/// </summary>
		public bool AdjustInventory  { get; set; }

		/// <summary>
		/// [IN] Whether to reduce reward points
		/// </summary>
		public bool ReduceRewardPoints { get; set; }

		/// <summary>
		/// [OUT] Inventory old
		/// </summary>
		public int InventoryOld { get; set; }

		/// <summary>
		/// [OUT] Inventory new
		/// </summary>
		public int InventoryNew { get; set; }

		/// <summary>
		/// [OUT] Reward points old
		/// </summary>
		public int RewardPointsOld { get; set; }

		/// <summary>
		/// [OUT] Reward points new
		/// </summary>
		public int RewardPointsNew { get; set; }

		public static string InfoKey { get { return "CancelOrderItemContextInfo"; } }

		public string ToString(ILocalizationService localizationService)
		{
			string result = localizationService.GetResource("Admin.Orders.OrderItem.Cancel.Info").FormatWith(
				InventoryOld, InventoryNew, RewardPointsOld, RewardPointsNew
			);

			return result;
		}
	}
}
