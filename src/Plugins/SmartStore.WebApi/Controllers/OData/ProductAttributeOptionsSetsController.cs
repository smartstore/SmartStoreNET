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
    public class ProductAttributeOptionsSetsController : WebApiEntityController<ProductAttributeOptionsSet, IProductAttributeService>
	{
        [WebApiAuthenticate(Permission = Permissions.Catalog.Variant.EditSet)]
		protected override void Insert(ProductAttributeOptionsSet entity)
		{
			Service.InsertProductAttributeOptionsSet(entity);
		}

        [WebApiAuthenticate(Permission = Permissions.Catalog.Variant.EditSet)]
        protected override void Update(ProductAttributeOptionsSet entity)
		{
			Service.UpdateProductAttributeOptionsSet(entity);
		}

        [WebApiAuthenticate(Permission = Permissions.Catalog.Variant.EditSet)]
        protected override void Delete(ProductAttributeOptionsSet entity)
		{
			Service.DeleteProductAttributeOptionsSet(entity);
		}

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Variant.Read)]
        public SingleResult<ProductAttributeOptionsSet> GetProductAttributeOptionsSet(int key)
		{
			return GetSingleResult(key);
		}

		// Navigation properties.

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Variant.Read)]
        public SingleResult<ProductAttribute> GetProductAttribute(int key)
		{
			return GetRelatedEntity(key, x => x.ProductAttribute);
		}

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Variant.Read)]
        public IQueryable<ProductAttributeOption> GetProductAttributeOptions(int key)
		{
			return GetRelatedCollection(key, x => x.ProductAttributeOptions);
		}
	}
}
