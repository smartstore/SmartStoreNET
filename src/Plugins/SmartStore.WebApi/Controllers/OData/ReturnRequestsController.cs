using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
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
		[WebApiQueryable]
		[WebApiAuthenticate(Permission = Permissions.Order.ReturnRequest.Read)]
		public IQueryable<ReturnRequest> Get()
		{
			return GetEntitySet();
		}

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Order.ReturnRequest.Read)]
        public SingleResult<ReturnRequest> Get(int key)
		{
			return GetSingleResult(key);
		}

		[WebApiAuthenticate(Permission = Permissions.Customer.Create)]
		public IHttpActionResult Post(ReturnRequest entity)
		{
			throw this.ExceptionNotImplemented();
		}

		[WebApiAuthenticate(Permission = Permissions.Customer.Update)]
		public IHttpActionResult Put(int key, ReturnRequest entity)
		{
			throw this.ExceptionNotImplemented();
		}

		[WebApiAuthenticate(Permission = Permissions.Customer.Update)]
		public IHttpActionResult Patch(int key, Delta<ReturnRequest> model)
		{
			throw this.ExceptionNotImplemented();
		}

		[WebApiAuthenticate(Permission = Permissions.Order.ReturnRequest.Delete)]
		public async Task<IHttpActionResult> Delete(int key)
		{
			var result = await DeleteAsync(key, entity => Service.DeleteReturnRequest(entity));
			return result;
		}

		#region Navigation properties

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Customer.Read)]
        public SingleResult<Customer> GetCustomer(int key)
		{
			return GetRelatedEntity(key, x => x.Customer);
		}

		#endregion
	}
}
