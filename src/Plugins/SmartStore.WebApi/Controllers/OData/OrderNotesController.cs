using System.Web.Http;
using SmartStore.Core.Domain.Orders;
using SmartStore.Services.Orders;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
	[WebApiAuthenticate(Permission = "ManageOrders")]
	public class OrderNotesController : WebApiEntityController<OrderNote, IOrderService>
	{
		protected override void Delete(OrderNote entity)
		{
			Service.DeleteOrderNote(entity);
		}

		[WebApiQueryable]
		public SingleResult<OrderNote> GetOrderNote(int key)
		{
			return GetSingleResult(key);
		}

		// navigation properties

		[WebApiQueryable]
		public SingleResult<Order> GetOrder(int key)
		{
			return GetRelatedEntity(key, x => x.Order);
		}
	}
}
