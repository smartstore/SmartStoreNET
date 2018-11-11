using System.Web.Http;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Orders;
using SmartStore.Services.Orders;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
	[WebApiAuthenticate(Permission = "ManageReturnRequests")]
	public class ReturnRequestsController : WebApiEntityController<ReturnRequest, IOrderService>
	{
		protected override void Delete(ReturnRequest entity)
		{
			Service.DeleteReturnRequest(entity);
		}

		[WebApiQueryable]
		public SingleResult<ReturnRequest> GetReturnRequest(int key)
		{
			return GetSingleResult(key);
		}

		// navigation properties

		[WebApiQueryable]
		public SingleResult<Customer> GetCustomer(int key)
		{
			return GetRelatedEntity(key, x => x.Customer);
		}
	}
}
