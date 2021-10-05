using System.Net;
using System.Web.Http;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Security;
using SmartStore.Services.Customers;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
    [IEEE754Compatible]
    public class RewardPointsHistoryController : WebApiEntityController<RewardPointsHistory, ICustomerService>
    {
        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Customer.Read)]
        public IHttpActionResult Get()
        {
            return Ok(GetEntitySet());
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Customer.Read)]
        public IHttpActionResult Get(int key)
        {
            return Ok(GetByKey(key));
        }

        [WebApiAuthenticate(Permission = Permissions.Customer.Read)]
        public IHttpActionResult GetProperty(int key, string propertyName)
        {
            return GetPropertyValue(key, propertyName);
        }

        public IHttpActionResult Post()
        {
            return StatusCode(HttpStatusCode.Forbidden);
        }

        public IHttpActionResult Put()
        {
            return StatusCode(HttpStatusCode.Forbidden);
        }

        public IHttpActionResult Patch()
        {
            return StatusCode(HttpStatusCode.Forbidden);
        }

        public IHttpActionResult Delete()
        {
            return StatusCode(HttpStatusCode.Forbidden);
        }

        #region Navigation properties

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Customer.Read)]
        public IHttpActionResult GetUsedWithOrder(int key)
        {
            return Ok(GetRelatedEntity(key, x => x.UsedWithOrder));
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Customer.Read)]
        public IHttpActionResult GetCustomer(int key)
        {
            return Ok(GetRelatedEntity(key, x => x.Customer));
        }

        #endregion
    }
}