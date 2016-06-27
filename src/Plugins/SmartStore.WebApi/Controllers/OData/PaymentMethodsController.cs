using System.Web.Http;
using SmartStore.Core.Domain.Payments;
using SmartStore.Services.Payments;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
	[WebApiAuthenticate(Permission = "ManagePaymentMethods")]
	public class PaymentMethodsController : WebApiEntityController<PaymentMethod, IPaymentService>
	{
		protected override void Insert(PaymentMethod entity)
		{
			Service.InsertPaymentMethod(entity);
		}
		protected override void Update(PaymentMethod entity)
		{
			Service.UpdatePaymentMethod(entity);
		}
		protected override void Delete(PaymentMethod entity)
		{
			Service.DeletePaymentMethod(entity);
		}

		[WebApiQueryable]
		public SingleResult<PaymentMethod> GetShippingMethod(int key)
		{
			return GetSingleResult(key);
		}

		// navigation properties

	}
}
