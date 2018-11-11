using System.Web.Http;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Directory;
using SmartStore.Services.Catalog;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
	[WebApiAuthenticate(Permission = "ManageCatalog")]
	public class ProductVariantAttributeCombinationsController : WebApiEntityController<ProductVariantAttributeCombination, IProductAttributeService>
	{
		protected override void Insert(ProductVariantAttributeCombination entity)
		{
			Service.InsertProductVariantAttributeCombination(entity);
		}
		protected override void Update(ProductVariantAttributeCombination entity)
		{
			Service.UpdateProductVariantAttributeCombination(entity);
		}
		protected override void Delete(ProductVariantAttributeCombination entity)
		{
			Service.DeleteProductVariantAttributeCombination(entity);
		}

		[WebApiQueryable]
		public SingleResult<ProductVariantAttributeCombination> GetProductVariantAttributeCombination(int key)
		{
			return GetSingleResult(key);
		}

		// navigation properties

		[WebApiQueryable]
		public SingleResult<DeliveryTime> GetDeliveryTime(int key)
		{
			return GetRelatedEntity(key, x => x.DeliveryTime);
		}
	}
}
