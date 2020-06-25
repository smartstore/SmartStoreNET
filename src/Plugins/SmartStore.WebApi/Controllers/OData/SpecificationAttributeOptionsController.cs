using System.Linq;
using System.Web.Http;
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
        [WebApiAuthenticate(Permission = Permissions.Catalog.Attribute.EditOption)]
        protected override void Insert(SpecificationAttributeOption entity)
		{
			Service.InsertSpecificationAttributeOption(entity);
		}

        [WebApiAuthenticate(Permission = Permissions.Catalog.Attribute.EditOption)]
        protected override void Update(SpecificationAttributeOption entity)
		{
			Service.UpdateSpecificationAttributeOption(entity);
		}

        [WebApiAuthenticate(Permission = Permissions.Catalog.Attribute.EditOption)]
        protected override void Delete(SpecificationAttributeOption entity)
		{
			Service.DeleteSpecificationAttributeOption(entity);
		}

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Attribute.Read)]
        public SingleResult<SpecificationAttributeOption> GetSpecificationAttributeOption(int key)
		{
			return GetSingleResult(key);
		}

		// Navigation properties.

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
	}
}
