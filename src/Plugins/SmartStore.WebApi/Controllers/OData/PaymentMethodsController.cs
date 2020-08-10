using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using SmartStore.Core.Domain.Payments;
using SmartStore.Core.Security;
using SmartStore.Services.Payments;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
    public class PaymentMethodsController : WebApiEntityController<PaymentMethod, IPaymentService>
	{
		[WebApiQueryable]
		[WebApiAuthenticate(Permission = Permissions.Configuration.PaymentMethod.Read)]
		public IQueryable<PaymentMethod> Get()
		{
			return GetEntitySet();
		}

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Configuration.PaymentMethod.Read)]
        public SingleResult<PaymentMethod> Get(int key)
		{
			return GetSingleResult(key);
		}

		// Update permission sufficient here.
		[WebApiAuthenticate(Permission = Permissions.Configuration.PaymentMethod.Update)]
		public IHttpActionResult Post(PaymentMethod entity)
		{
			var result = Insert(entity, () => Service.InsertPaymentMethod(entity));
			return result;
		}

		[WebApiAuthenticate(Permission = Permissions.Configuration.PaymentMethod.Update)]
		public async Task<IHttpActionResult> Put(int key, PaymentMethod entity)
		{
			var result = await UpdateAsync(entity, key, () => Service.UpdatePaymentMethod(entity));
			return result;
		}

		[WebApiAuthenticate(Permission = Permissions.Configuration.PaymentMethod.Update)]
		public async Task<IHttpActionResult> Patch(int key, Delta<PaymentMethod> model)
		{
			var result = await PartiallyUpdateAsync(key, model, entity => Service.UpdatePaymentMethod(entity));
			return result;
		}

		[WebApiAuthenticate]
		public IHttpActionResult Delete(int key)
		{
			throw new HttpResponseException(HttpStatusCode.Forbidden);
		}
	}
}
