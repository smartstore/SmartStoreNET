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
    public class ProductVariantAttributesController : WebApiEntityController<ProductVariantAttribute, IProductAttributeService>
	{
        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.EditVariant)]
		protected override void Insert(ProductVariantAttribute entity)
		{
			Service.InsertProductVariantAttribute(entity);
		}

        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.EditVariant)]
        protected override void Update(ProductVariantAttribute entity)
		{
			Service.UpdateProductVariantAttribute(entity);
		}

        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.EditVariant)]
        protected override void Delete(ProductVariantAttribute entity)
		{
			Service.DeleteProductVariantAttribute(entity);
		}

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.Read)]
        public SingleResult<ProductVariantAttribute> GetProductVariantAttribute(int key)
		{
			return GetSingleResult(key);
		}

		// Navigation properties.

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.Read)]
        public SingleResult<ProductAttribute> GetProductAttribute(int key)
		{
			return GetRelatedEntity(key, x => x.ProductAttribute);
		}

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.Read)]
        public IQueryable<ProductVariantAttributeValue> GetProductVariantAttributeValues(int key)
		{
			return GetRelatedCollection(key, x => x.ProductVariantAttributeValues);
		}
	}
}
