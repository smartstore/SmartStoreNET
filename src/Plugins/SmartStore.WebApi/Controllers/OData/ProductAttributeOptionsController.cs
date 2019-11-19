using System.Web.Http;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Security;
using SmartStore.Services.Catalog;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
    public class ProductAttributeOptionsController : WebApiEntityController<ProductAttributeOption, IProductAttributeService>
	{
        [WebApiAuthenticate(Permission = Permissions.Catalog.Variant.EditSet)]
		protected override void Insert(ProductAttributeOption entity)
		{
			Service.InsertProductAttributeOption(entity);
		}

        [WebApiAuthenticate(Permission = Permissions.Catalog.Variant.EditSet)]
        protected override void Update(ProductAttributeOption entity)
		{
			Service.UpdateProductAttributeOption(entity);
		}

        [WebApiAuthenticate(Permission = Permissions.Catalog.Variant.EditSet)]
        protected override void Delete(ProductAttributeOption entity)
		{
			Service.DeleteProductAttributeOption(entity);
		}

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Variant.Read)]
        public SingleResult<ProductAttributeOption> GetProductAttributeOption(int key)
		{
			return GetSingleResult(key);
		}

		// Navigation properties.

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Variant.Read)]
        public SingleResult<ProductAttributeOptionsSet> GetProductAttributeOptionsSet(int key)
		{
			return GetRelatedEntity(key, x => x.ProductAttributeOptionsSet);
		}
	}
}
