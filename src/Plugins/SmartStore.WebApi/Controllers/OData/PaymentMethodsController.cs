using System.Web.Http;
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
        // Update permission sufficient here.
        [WebApiAuthenticate(Permission = Permissions.Configuration.PaymentMethod.Update)]
		protected override void Insert(PaymentMethod entity)
		{
			Service.InsertPaymentMethod(entity);
		}

        [WebApiAuthenticate(Permission = Permissions.Configuration.PaymentMethod.Update)]
        protected override void Update(PaymentMethod entity)
		{
			Service.UpdatePaymentMethod(entity);
		}

        [WebApiAuthenticate]
        protected override void Delete(PaymentMethod entity)
		{
            throw this.ExceptionForbidden();
        }

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Configuration.PaymentMethod.Read)]
        public SingleResult<PaymentMethod> GetPaymentMethod(int key)
		{
			return GetSingleResult(key);
		}
	}
}
