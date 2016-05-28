using System.Web.Http;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Domain.Stores;
using SmartStore.Services.DataExchange;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
	[WebApiAuthenticate(Permission = "ManageMaintenance")] // TODO: ManageMaintenance... really?
	public class SyncMappingsController : WebApiEntityController<SyncMapping, ISyncMappingService>
	{
		protected override void Insert(SyncMapping entity)
		{
			Service.InsertSyncMapping(entity);
		}
		protected override void Update(SyncMapping entity)
		{
			Service.UpdateSyncMapping(entity);
		}
		protected override void Delete(SyncMapping entity)
		{
			Service.DeleteSyncMapping(entity);
		}

		[WebApiQueryable]
		public SingleResult<SyncMapping> GetSyncMapping(int key)
		{
			return GetSingleResult(key);
		}
	}
}
