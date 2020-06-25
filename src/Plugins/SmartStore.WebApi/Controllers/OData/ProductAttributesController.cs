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
    public class ProductAttributesController : WebApiEntityController<ProductAttribute, IProductAttributeService>
	{
        [WebApiAuthenticate(Permission = Permissions.Catalog.Variant.Create)]
        protected override void Insert(ProductAttribute entity)
		{
			Service.InsertProductAttribute(entity);
		}

        [WebApiAuthenticate(Permission = Permissions.Catalog.Variant.Update)]
        protected override void Update(ProductAttribute entity)
		{
			Service.UpdateProductAttribute(entity);
		}

        [WebApiAuthenticate(Permission = Permissions.Catalog.Variant.Delete)]
        protected override void Delete(ProductAttribute entity)
		{
			Service.DeleteProductAttribute(entity);
		}

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Variant.Read)]
        public SingleResult<ProductAttribute> GetProductAttribute(int key)
		{
			return GetSingleResult(key);
		}

		// Navigation properties.

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Variant.Read)]
        public IQueryable<ProductAttributeOptionsSet> GetProductAttributeOptionsSets(int key)
		{
			return GetRelatedCollection(key, x => x.ProductAttributeOptionsSets);
		}
	}
}
