using System.Web.Http;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Security;
using SmartStore.Services.Orders;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
    public class ReturnRequestsController : WebApiEntityController<ReturnRequest, IOrderService>
	{
        [WebApiAuthenticate(Permission = Permissions.Order.ReturnRequest.Delete)]
		protected override void Delete(ReturnRequest entity)
		{
			Service.DeleteReturnRequest(entity);
		}

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Order.ReturnRequest.Read)]
        public SingleResult<ReturnRequest> GetReturnRequest(int key)
		{
			return GetSingleResult(key);
		}

		// Navigation properties.

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Customer.Read)]
        public SingleResult<Customer> GetCustomer(int key)
		{
			return GetRelatedEntity(key, x => x.Customer);
		}
	}
}
