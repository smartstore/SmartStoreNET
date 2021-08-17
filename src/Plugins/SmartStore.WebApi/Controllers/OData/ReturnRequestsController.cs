using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Security;
using SmartStore.Services.Orders;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
    [IEEE754Compatible]
    public class ReturnRequestsController : WebApiEntityController<ReturnRequest, IOrderService>
    {
        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Order.ReturnRequest.Read)]
        public IHttpActionResult Get()
        {
            return Ok(GetEntitySet());
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Order.ReturnRequest.Read)]
        public IHttpActionResult Get(int key)
        {
            return Ok(GetByKey(key));
        }

        [WebApiAuthenticate(Permission = Permissions.Order.ReturnRequest.Read)]
        public IHttpActionResult GetProperty(int key, string propertyName)
        {
            return GetPropertyValue(key, propertyName);
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Customer.Create)]
        public IHttpActionResult Post()
        {
            return StatusCode(HttpStatusCode.NotImplemented);
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Customer.Update)]
        public IHttpActionResult Put()
        {
            return StatusCode(HttpStatusCode.NotImplemented);
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Customer.Update)]
        public IHttpActionResult Patch()
        {
            return StatusCode(HttpStatusCode.NotImplemented);
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
        public IHttpActionResult GetCustomer(int key)
        {
            return Ok(GetRelatedEntity(key, x => x.Customer));
        }

        #endregion
    }
}
