using System.Web.Http;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Security;
using SmartStore.Services.Catalog;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
    public class ProductVariantAttributeValuesController : WebApiEntityController<ProductVariantAttributeValue, IProductAttributeService>
	{
        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.EditVariant)]
        protected override void Insert(ProductVariantAttributeValue entity)
		{
			Service.InsertProductVariantAttributeValue(entity);
		}

        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.EditVariant)]
        protected override void Update(ProductVariantAttributeValue entity)
		{
			Service.UpdateProductVariantAttributeValue(entity);
		}

        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.EditVariant)]
        protected override void Delete(ProductVariantAttributeValue entity)
		{
			Service.DeleteProductVariantAttributeValue(entity);
		}

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.Read)]
        public SingleResult<ProductVariantAttributeValue> GetProductVariantAttributeValue(int key)
		{
			return GetSingleResult(key);
		}

		// Navigation properties.

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.Read)]
        public SingleResult<ProductVariantAttribute> GetProductVariantAttribute(int key)
		{
			return GetRelatedEntity(key, x => x.ProductVariantAttribute);
		}
	}
}
