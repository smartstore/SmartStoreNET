using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Services.DataExchange;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
    [WebApiAuthenticate]
    [IEEE754Compatible]
    public class SyncMappingsController : WebApiEntityController<SyncMapping, ISyncMappingService>
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
        public IHttpActionResult Post(SyncMapping entity)
        {
            var result = Insert(entity, () => Service.InsertSyncMapping(entity));
            return result;
        }

        [WebApiQueryable]
        public async Task<IHttpActionResult> Put(int key, SyncMapping entity)
        {
            var result = await UpdateAsync(entity, key, () => Service.UpdateSyncMapping(entity));
            return result;
        }

        [WebApiQueryable]
        public async Task<IHttpActionResult> Patch(int key, Delta<SyncMapping> model)
        {
            var result = await PartiallyUpdateAsync(key, model, entity => Service.UpdateSyncMapping(entity));
            return result;
        }

        public async Task<IHttpActionResult> Delete(int key)
        {
            var result = await DeleteAsync(key, entity => Service.DeleteSyncMapping(entity));
            return result;
        }
    }
}
