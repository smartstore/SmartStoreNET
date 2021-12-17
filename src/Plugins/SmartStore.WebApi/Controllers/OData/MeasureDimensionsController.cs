using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Security;
using SmartStore.Services.Directory;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
    [IEEE754Compatible]
    public class MeasureDimensionsController : WebApiEntityController<MeasureDimension, IMeasureService>
    {
        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Configuration.Measure.Read)]
        public IHttpActionResult Get()
        {
            return Ok(GetEntitySet());
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Configuration.Measure.Read)]
        public IHttpActionResult Get(int key)
        {
            return Ok(GetByKey(key));
        }

        [WebApiAuthenticate(Permission = Permissions.Configuration.Measure.Read)]
        public IHttpActionResult GetProperty(int key, string propertyName)
        {
            return GetPropertyValue(key, propertyName);
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Configuration.Measure.Create)]
        public IHttpActionResult Post(MeasureDimension entity)
        {
            var result = Insert(entity, () => Service.InsertMeasureDimension(entity));
            return result;
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Configuration.Measure.Update)]
        public async Task<IHttpActionResult> Put(int key, MeasureDimension entity)
        {
            var result = await UpdateAsync(entity, key, () => Service.UpdateMeasureDimension(entity));
            return result;
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Configuration.Measure.Update)]
        public async Task<IHttpActionResult> Patch(int key, Delta<MeasureDimension> model)
        {
            var result = await PartiallyUpdateAsync(key, model, entity => Service.UpdateMeasureDimension(entity));
            return result;
        }

        [WebApiAuthenticate(Permission = Permissions.Configuration.Measure.Delete)]
        public async Task<IHttpActionResult> Delete(int key)
        {
            var result = await DeleteAsync(key, entity => Service.DeleteMeasureDimension(entity));
            return result;
        }
    }
}
