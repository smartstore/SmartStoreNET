using System.Web.Http;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Security;
using SmartStore.Services.Shipping;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
    public class ShippingMethodsController : WebApiEntityController<ShippingMethod, IShippingService>
	{
        [WebApiAuthenticate(Permission = Permissions.Configuration.Shipping.Create)]
        protected override void Insert(ShippingMethod entity)
		{
			Service.InsertShippingMethod(entity);
		}

        [WebApiAuthenticate(Permission = Permissions.Configuration.Shipping.Update)]
        protected override void Update(ShippingMethod entity)
		{
			Service.UpdateShippingMethod(entity);
		}

        [WebApiAuthenticate(Permission = Permissions.Configuration.Shipping.Delete)]
        protected override void Delete(ShippingMethod entity)
		{
			Service.DeleteShippingMethod(entity);
		}

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Configuration.Shipping.Read)]
        public SingleResult<ShippingMethod> GetShippingMethod(int key)
		{
			return GetSingleResult(key);
		}
	}
}
