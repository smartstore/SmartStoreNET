using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Mvc;

namespace SmartStore.Admin.Models.Orders
{
	public class CancelOrderItemModel : EntityModelBase
	{
		public CancelOrderItemModel()
		{
			AdjustInventory = true;
		}

		public string Caption { get; set; }
		public string PostUrl { get; set; }

		[SmartResourceDisplayName("Admin.Orders.OrderItem.Cancel.Fields.AdjustInventory")]
		public bool AdjustInventory { get; set; }

		[SmartResourceDisplayName("Admin.Orders.OrderItem.Cancel.Fields.ReduceRewardPoints")]
		public bool ReduceRewardPoints { get; set; }
	}
}