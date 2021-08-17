using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using SmartStore.Core.Domain.Stores;
using SmartStore.Services.Stores;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
    [WebApiAuthenticate]
    [IEEE754Compatible]
    public class StoreMappingsController : WebApiEntityController<StoreMapping, IStoreMappingService>
    {
        [WebApiQueryable]
        public IHttpActionResult Get()
        {
            return Ok(GetEntitySet());
        }

        [WebApiQueryable]
        public IHttpActionResult Get(int key)
        {
            return Ok(GetByKey(key));
        }

        public IHttpActionResult GetProperty(int key, string propertyName)
        {
            return GetPropertyValue(key, propertyName);
        }

        [WebApiQueryable]
        public IHttpActionResult Post(StoreMapping entity)
        {
            var result = Insert(entity, () => Service.InsertStoreMapping(entity));
            return result;
        }

        [WebApiQueryable]
        public async Task<IHttpActionResult> Put(int key, StoreMapping entity)
        {
            var result = await UpdateAsync(entity, key, () => Service.UpdateStoreMapping(entity));
            return result;
        }

        [WebApiQueryable]
        public async Task<IHttpActionResult> Patch(int key, Delta<StoreMapping> model)
        {
            var result = await PartiallyUpdateAsync(key, model, entity => Service.UpdateStoreMapping(entity));
            return result;
        }

        public async Task<IHttpActionResult> Delete(int key)
        {
            var result = await DeleteAsync(key, entity => Service.DeleteStoreMapping(entity));
            return result;
        }
    }
}
