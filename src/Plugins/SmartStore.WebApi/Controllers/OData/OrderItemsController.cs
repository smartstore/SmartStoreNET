using System.Linq;
using System.Web.Http;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Security;
using SmartStore.Services.Orders;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;
using SmartStore.WebApi.Models.OData;

namespace SmartStore.WebApi.Controllers.OData
{
    public class OrderItemsController : WebApiEntityController<OrderItem, IOrderService>
	{
		protected override IQueryable<OrderItem> GetEntitySet()
		{
			var query =
				from x in this.Repository.Table
				select x;

			return query;
		}
		
        [WebApiAuthenticate(Permission = Permissions.Order.EditItem)]
        protected override void Insert(OrderItem entity)
		{
			throw this.ExceptionNotImplemented();
		}

        [WebApiAuthenticate(Permission = Permissions.Order.EditItem)]
        protected override void Update(OrderItem entity)
		{
			throw this.ExceptionNotImplemented();
		}

        [WebApiAuthenticate(Permission = Permissions.Order.EditItem)]
        protected override void Delete(OrderItem entity)
		{
			Service.DeleteOrderItem(entity);
		}

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Order.Read)]
        public SingleResult<OrderItem> GetOrderItem(int key)
		{
			return GetSingleResult(key);
		}

		// Navigation properties.

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Order.Read)]
        public SingleResult<Order> GetOrder(int key)
		{
			return GetRelatedEntity(key, x => x.Order);
		}

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.Read)]
        public SingleResult<Product> GetProduct(int key)
		{
			return GetRelatedEntity(key, x => x.Product);
		}

		[HttpPost]
        [WebApiAuthenticate(Permission = Permissions.Order.Read)]
        public OrderItemInfo Infos(int key)
		{
			var result = new OrderItemInfo();
			var entity = GetEntityByKeyNotNull(key);

			this.ProcessEntity(() =>
			{
				result.ItemsCanBeAddedToShipmentCount = entity.GetItemsCanBeAddedToShipmentCount();
				result.ShipmentItemsCount = entity.GetShipmentItemsCount();
				result.DispatchedItemsCount = entity.GetDispatchedItemsCount();
				result.NotDispatchedItemsCount = entity.GetNotDispatchedItemsCount();
				result.DeliveredItemsCount = entity.GetDeliveredItemsCount();
				result.NotDeliveredItemsCount = entity.GetNotDeliveredItemsCount();
			});

			return result;
		}
	}
}
