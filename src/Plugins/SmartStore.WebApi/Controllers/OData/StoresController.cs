using System.Web.Http;
using SmartStore.Core.Domain.Stores;
using SmartStore.Services.Stores;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
	[WebApiAuthenticate(Permission = "ManageStores")]
	public class StoresController : WebApiEntityController<Store, IStoreService>
	{
		protected override void Insert(Store entity)
		{
			Service.InsertStore(entity);
		}
		protected override void Update(Store entity)
		{
			Service.UpdateStore(entity);
		}
		protected override void Delete(Store entity)
		{
			Service.DeleteStore(entity);
		}

		[WebApiQueryable]
		public SingleResult<Store> GetStore(int key)
		{
			return GetSingleResult(key);
		}
	}
}
