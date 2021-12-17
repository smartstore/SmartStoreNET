using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Security;
using SmartStore.Services.Stores;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
    [IEEE754Compatible]
    public class StoresController : WebApiEntityController<Store, IStoreService>
    {
        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Configuration.Store.Read)]
        public IHttpActionResult Get()
        {
            return Ok(GetEntitySet());
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Configuration.Store.Read)]
        public IHttpActionResult Get(int key)
        {
            return Ok(GetByKey(key));
        }

        [WebApiAuthenticate(Permission = Permissions.Configuration.Store.Read)]
        public IHttpActionResult GetProperty(int key, string propertyName)
        {
            return GetPropertyValue(key, propertyName);
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Configuration.Store.Create)]
        public IHttpActionResult Post(Store entity)
        {
            var result = Insert(entity, () => Service.InsertStore(entity));
            return result;
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Configuration.Store.Update)]
        public async Task<IHttpActionResult> Put(int key, Store entity)
        {
            var result = await UpdateAsync(entity, key, () => Service.UpdateStore(entity));
            return result;
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Configuration.Store.Update)]
        public async Task<IHttpActionResult> Patch(int key, Delta<Store> model)
        {
            var result = await PartiallyUpdateAsync(key, model, entity => Service.UpdateStore(entity));
            return result;
        }

        [WebApiAuthenticate(Permission = Permissions.Configuration.Store.Delete)]
        public async Task<IHttpActionResult> Delete(int key)
        {
            var result = await DeleteAsync(key, entity => Service.DeleteStore(entity));
            return result;
        }
    }
}
