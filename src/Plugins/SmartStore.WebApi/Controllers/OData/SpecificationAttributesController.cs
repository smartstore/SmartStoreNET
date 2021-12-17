using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Security;
using SmartStore.Services.Catalog;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
    [IEEE754Compatible]
    public class SpecificationAttributesController : WebApiEntityController<SpecificationAttribute, ISpecificationAttributeService>
    {
        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Attribute.Read)]
        public IHttpActionResult Get()
        {
            return Ok(GetEntitySet());
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Attribute.Read)]
        public IHttpActionResult Get(int key)
        {
            return Ok(GetByKey(key));
        }

        [WebApiAuthenticate(Permission = Permissions.Catalog.Attribute.Read)]
        public IHttpActionResult GetProperty(int key, string propertyName)
        {
            return GetPropertyValue(key, propertyName);
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Attribute.Create)]
        public IHttpActionResult Post(SpecificationAttribute entity)
        {
            var result = Insert(entity, () => Service.InsertSpecificationAttribute(entity));
            return result;
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Attribute.Update)]
        public async Task<IHttpActionResult> Put(int key, SpecificationAttribute entity)
        {
            var result = await UpdateAsync(entity, key, () => Service.UpdateSpecificationAttribute(entity));
            return result;
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Attribute.Update)]
        public async Task<IHttpActionResult> Patch(int key, Delta<SpecificationAttribute> model)
        {
            var result = await PartiallyUpdateAsync(key, model, entity => Service.UpdateSpecificationAttribute(entity));
            return result;
        }

        [WebApiAuthenticate(Permission = Permissions.Catalog.Attribute.Delete)]
        public async Task<IHttpActionResult> Delete(int key)
        {
            var result = await DeleteAsync(key, entity => Service.DeleteSpecificationAttribute(entity));
            return result;
        }

        #region Navigation properties

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Attribute.Read)]
        public IHttpActionResult GetSpecificationAttributeOptions(int key)
        {
            return Ok(GetRelatedCollection(key, x => x.SpecificationAttributeOptions));
        }

        #endregion
    }
}
