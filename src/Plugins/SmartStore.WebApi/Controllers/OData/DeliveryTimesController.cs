using System.Web.Http;
using SmartStore.Core.Domain.Directory;
using SmartStore.Services.Directory;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
	[WebApiAuthenticate(Permission = "ManageDeliveryTimes")]
	public class DeliveryTimesController : WebApiEntityController<DeliveryTime, IDeliveryTimeService>
	{
		protected override void Insert(DeliveryTime entity)
		{
			Service.InsertDeliveryTime(entity);
		}
		protected override void Update(DeliveryTime entity)
		{
			Service.UpdateDeliveryTime(entity);
		}
		protected override void Delete(DeliveryTime entity)
		{
			Service.DeleteDeliveryTime(entity);
		}

		[WebApiQueryable]
		public SingleResult<DeliveryTime> GetDeliveryTime(int key)
		{
			return GetSingleResult(key);
		}
	}
}
