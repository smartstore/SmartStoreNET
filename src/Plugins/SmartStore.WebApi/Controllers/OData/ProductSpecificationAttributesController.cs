using System.Web.Http;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Security;
using SmartStore.Services.Catalog;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
    public class ProductSpecificationAttributesController : WebApiEntityController<ProductSpecificationAttribute, ISpecificationAttributeService>
	{
        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.EditAttribute)]
		protected override void Insert(ProductSpecificationAttribute entity)
		{
			Service.InsertProductSpecificationAttribute(entity);
		}

        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.EditAttribute)]
        protected override void Update(ProductSpecificationAttribute entity)
		{
			Service.UpdateProductSpecificationAttribute(entity);
		}

        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.EditAttribute)]
        protected override void Delete(ProductSpecificationAttribute entity)
		{
			Service.DeleteProductSpecificationAttribute(entity);
		}

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.Read)]
        public SingleResult<ProductSpecificationAttribute> GetProductSpecificationAttribute(int key)
		{
			return GetSingleResult(key);
		}

		// Navigation properties.

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.Read)]
        public SingleResult<SpecificationAttributeOption> GetSpecificationAttributeOption(int key)
		{
            return GetRelatedEntity(key, x => x.SpecificationAttributeOption);
        }
	}
}
