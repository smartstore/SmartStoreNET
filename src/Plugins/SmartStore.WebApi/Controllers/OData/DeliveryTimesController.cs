using System.Web.Http;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Security;
using SmartStore.Services.Directory;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
    public class DeliveryTimesController : WebApiEntityController<DeliveryTime, IDeliveryTimeService>
	{
        [WebApiAuthenticate(Permission = Permissions.Configuration.DeliveryTime.Create)]
		protected override void Insert(DeliveryTime entity)
		{
			Service.InsertDeliveryTime(entity);
		}

        [WebApiAuthenticate(Permission = Permissions.Configuration.DeliveryTime.Update)]
        protected override void Update(DeliveryTime entity)
		{
			Service.UpdateDeliveryTime(entity);
		}

        [WebApiAuthenticate(Permission = Permissions.Configuration.DeliveryTime.Delete)]
        protected override void Delete(DeliveryTime entity)
		{
			Service.DeleteDeliveryTime(entity);
		}

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Configuration.DeliveryTime.Read)]
        public SingleResult<DeliveryTime> GetDeliveryTime(int key)
		{
			return GetSingleResult(key);
		}
	}
}
