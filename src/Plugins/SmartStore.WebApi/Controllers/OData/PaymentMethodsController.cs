using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using SmartStore.Core.Domain.Payments;
using SmartStore.Core.Security;
using SmartStore.Services.Payments;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
    [IEEE754Compatible]
    public class PaymentMethodsController : WebApiEntityController<PaymentMethod, IPaymentService>
    {
        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Configuration.PaymentMethod.Read)]
        public IHttpActionResult Get()
        {
            return Ok(GetEntitySet());
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Configuration.PaymentMethod.Read)]
        public IHttpActionResult Get(int key)
        {
            return Ok(GetByKey(key));
        }

        [WebApiAuthenticate(Permission = Permissions.Configuration.PaymentMethod.Read)]
        public IHttpActionResult GetProperty(int key, string propertyName)
        {
            return GetPropertyValue(key, propertyName);
        }

        // Update permission sufficient here.
        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Configuration.PaymentMethod.Update)]
        public IHttpActionResult Post(PaymentMethod entity)
        {
            var result = Insert(entity, () => Service.InsertPaymentMethod(entity));
            return result;
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Configuration.PaymentMethod.Update)]
        public async Task<IHttpActionResult> Put(int key, PaymentMethod entity)
        {
            var result = await UpdateAsync(entity, key, () => Service.UpdatePaymentMethod(entity));
            return result;
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Configuration.PaymentMethod.Update)]
        public async Task<IHttpActionResult> Patch(int key, Delta<PaymentMethod> model)
        {
            var result = await PartiallyUpdateAsync(key, model, entity => Service.UpdatePaymentMethod(entity));
            return result;
        }

        public IHttpActionResult Delete()
        {
            return StatusCode(HttpStatusCode.Forbidden);
        }
    }
}
