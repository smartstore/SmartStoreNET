using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Security;
using SmartStore.Services.Catalog;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
    public class SpecificationAttributeOptionsController : WebApiEntityController<SpecificationAttributeOption, ISpecificationAttributeService>
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
			return Ok(key);
		}

		[WebApiAuthenticate(Permission = Permissions.Catalog.Attribute.Read)]
		public IHttpActionResult GetProperty(int key, string propertyName)
		{
			return GetPropertyValue(key, propertyName);
		}

		[WebApiAuthenticate(Permission = Permissions.Catalog.Attribute.EditOption)]
		public IHttpActionResult Post(SpecificationAttributeOption entity)
		{
			var result = Insert(entity, () => Service.InsertSpecificationAttributeOption(entity));
			return result;
		}

		[WebApiAuthenticate(Permission = Permissions.Catalog.Attribute.EditOption)]
		public async Task<IHttpActionResult> Put(int key, SpecificationAttributeOption entity)
		{
			var result = await UpdateAsync(entity, key, () => Service.UpdateSpecificationAttributeOption(entity));
			return result;
		}

		[WebApiAuthenticate(Permission = Permissions.Catalog.Attribute.EditOption)]
		public async Task<IHttpActionResult> Patch(int key, Delta<SpecificationAttributeOption> model)
		{
			var result = await PartiallyUpdateAsync(key, model, entity => Service.UpdateSpecificationAttributeOption(entity));
			return result;
		}

		[WebApiAuthenticate(Permission = Permissions.Catalog.Attribute.EditOption)]
		public async Task<IHttpActionResult> Delete(int key)
		{
			var result = await DeleteAsync(key, entity => Service.DeleteSpecificationAttributeOption(entity));
			return result;
		}

		#region Navigation properties

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Attribute.Read)]
        public SingleResult<SpecificationAttribute> GetSpecificationAttribute(int key)
		{
			return GetRelatedEntity(key, x => x.SpecificationAttribute);
		}

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.Read)]
        public IQueryable<ProductSpecificationAttribute> GetProductSpecificationAttributes(int key)
		{
			return GetRelatedCollection(key, x => x.ProductSpecificationAttributes);
		}

		#endregion
	}
}
