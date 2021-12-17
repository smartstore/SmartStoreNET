using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using SmartStore.Core.Domain.Common;
using SmartStore.Services.Common;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
    [WebApiAuthenticate]
    [IEEE754Compatible]
    public class GenericAttributesController : WebApiEntityController<GenericAttribute, IGenericAttributeService>
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
        public IHttpActionResult Post(GenericAttribute entity)
        {
            var result = Insert(entity, () => Service.InsertAttribute(entity));
            return result;
        }

        [WebApiQueryable]
        public async Task<IHttpActionResult> Put(int key, GenericAttribute entity)
        {
            var result = await UpdateAsync(entity, key, () => Service.UpdateAttribute(entity));
            return result;
        }

        [WebApiQueryable]
        public async Task<IHttpActionResult> Patch(int key, Delta<GenericAttribute> model)
        {
            var result = await PartiallyUpdateAsync(key, model, entity => Service.UpdateAttribute(entity));
            return result;
        }

        public async Task<IHttpActionResult> Delete(int key)
        {
            var result = await DeleteAsync(key, entity => Service.DeleteAttribute(entity));
            return result;
        }
    }
}
