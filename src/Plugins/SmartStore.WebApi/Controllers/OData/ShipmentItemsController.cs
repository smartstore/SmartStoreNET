using System.Web.Http;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Services.Shipping;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
	[WebApiAuthenticate(Permission = "ManageOrders")]
	public class ShipmentItemsController : WebApiEntityController<ShipmentItem, IShipmentService>
	{
		protected override void Insert(ShipmentItem entity)
		{
			Service.InsertShipmentItem(entity);
		}
		protected override void Update(ShipmentItem entity)
		{
			Service.UpdateShipmentItem(entity);
		}
		protected override void Delete(ShipmentItem entity)
		{
			Service.DeleteShipmentItem(entity);
		}

		[WebApiQueryable]
		public SingleResult<ShipmentItem> GetShipmentItem(int key)
		{
			return GetSingleResult(key);
		}
	}
}
