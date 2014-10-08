using System.Web.Http;
using SmartStore.Core.Domain.Stores;
using SmartStore.Services.Stores;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
	[WebApiAuthenticate(Permission = "ManageStores")]
	public class StoreMappingsController : WebApiEntityController<StoreMapping, IStoreMappingService>
	{
		protected override void Insert(StoreMapping entity)
		{
			Service.InsertStoreMapping(entity);
		}
		protected override void Update(StoreMapping entity)
		{
			Service.UpdateStoreMapping(entity);
		}
		protected override void Delete(StoreMapping entity)
		{
			Service.DeleteStoreMapping(entity);
		}

		[WebApiQueryable]
		public SingleResult<StoreMapping> GetStoreMapping(int key)
		{
			return GetSingleResult(key);
		}
	}
}
