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
    public class SpecificationAttributesController : WebApiEntityController<SpecificationAttribute, ISpecificationAttributeService>
	{
        [WebApiAuthenticate(Permission = Permissions.Catalog.Attribute.Create)]
        protected override void Insert(SpecificationAttribute entity)
		{
			Service.InsertSpecificationAttribute(entity);
		}

        [WebApiAuthenticate(Permission = Permissions.Catalog.Attribute.Update)]
        protected override void Update(SpecificationAttribute entity)
		{
			Service.UpdateSpecificationAttribute(entity);
		}

        [WebApiAuthenticate(Permission = Permissions.Catalog.Attribute.Delete)]
        protected override void Delete(SpecificationAttribute entity)
		{
			Service.DeleteSpecificationAttribute(entity);
		}

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Attribute.Read)]
        public SingleResult<SpecificationAttribute> GetSpecificationAttribute(int key)
		{
			return GetSingleResult(key);
		}

		// Navigation properties.

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Attribute.Read)]
        public IQueryable<SpecificationAttributeOption> GetSpecificationAttributeOptions(int key)
		{
			return GetRelatedCollection(key, x => x.SpecificationAttributeOptions);
		}
	}
}
