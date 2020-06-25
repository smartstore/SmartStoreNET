using System.Web.Http;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Security;
using SmartStore.Services.Stores;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
    public class StoresController : WebApiEntityController<Store, IStoreService>
    {
        [WebApiAuthenticate(Permission = Permissions.Configuration.Store.Create)]
        protected override void Insert(Store entity)
		{
			Service.InsertStore(entity);
		}

        [WebApiAuthenticate(Permission = Permissions.Configuration.Store.Update)]
        protected override void Update(Store entity)
		{
			Service.UpdateStore(entity);
		}

        [WebApiAuthenticate(Permission = Permissions.Configuration.Store.Delete)]
        protected override void Delete(Store entity)
		{
			Service.DeleteStore(entity);
		}

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Configuration.Store.Read)]
        public SingleResult<Store> GetStore(int key)
		{
			return GetSingleResult(key);
		}
	}
}
