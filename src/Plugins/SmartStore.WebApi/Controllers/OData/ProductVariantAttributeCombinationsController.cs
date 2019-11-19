using System.Web.Http;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Security;
using SmartStore.Services.Catalog;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
    public class ProductVariantAttributeCombinationsController : WebApiEntityController<ProductVariantAttributeCombination, IProductAttributeService>
	{
        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.EditVariant)]
        protected override void Insert(ProductVariantAttributeCombination entity)
		{
			Service.InsertProductVariantAttributeCombination(entity);
		}

        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.EditVariant)]
        protected override void Update(ProductVariantAttributeCombination entity)
		{
			Service.UpdateProductVariantAttributeCombination(entity);
		}

        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.EditVariant)]
        protected override void Delete(ProductVariantAttributeCombination entity)
		{
			Service.DeleteProductVariantAttributeCombination(entity);
		}

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.Read)]
        public SingleResult<ProductVariantAttributeCombination> GetProductVariantAttributeCombination(int key)
		{
			return GetSingleResult(key);
		}

		// Navigation properties.

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Configuration.DeliveryTime.Read)]
        public SingleResult<DeliveryTime> GetDeliveryTime(int key)
		{
			return GetRelatedEntity(key, x => x.DeliveryTime);
		}
	}
}
