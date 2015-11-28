using SmartStore.Core.Domain.Orders;
using SmartStore.Services.Orders;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;
using System.Linq;
using System.Web.Http;
using SmartStore.Core.Domain.Catalog;

namespace SmartStore.WebApi.Controllers.OData
{
	[WebApiAuthenticate(Permission = "ManageOrders")]
	public class OrderItemsController : WebApiEntityController<OrderItem, IOrderService>
	{
		protected override IQueryable<OrderItem> GetEntitySet()
		{
			var query =
				from x in this.Repository.Table
				select x;

			return query;
		}
		protected override void Insert(OrderItem entity)
		{
			throw this.ExceptionNotImplemented();
		}
		protected override void Update(OrderItem entity)
		{
			throw this.ExceptionNotImplemented();
		}
		protected override void Delete(OrderItem entity)
		{
			Service.DeleteOrderItem(entity);
		}

		[WebApiQueryable]
		public SingleResult<OrderItem> GetOrderItem(int key)
		{
			return GetSingleResult(key);
		}

		// navigation properties

		public Order GetOrder(int key)
		{
			return GetExpandedProperty<Order>(key, x => x.Order);
		}

		public Product GetProduct(int key)
		{
			return GetExpandedProperty<Product>(key, x => x.Product);
		}
	}
}
