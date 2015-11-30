using System.Web.Http;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Services.Catalog;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
	[WebApiAuthenticate(Permission = "ManageCatalog")]
	public class ProductVariantAttributeValuesController : WebApiEntityController<ProductVariantAttributeValue, IProductAttributeService>
	{
		protected override void Insert(ProductVariantAttributeValue entity)
		{
			Service.InsertProductVariantAttributeValue(entity);
		}
		protected override void Update(ProductVariantAttributeValue entity)
		{
			Service.UpdateProductVariantAttributeValue(entity);
		}
		protected override void Delete(ProductVariantAttributeValue entity)
		{
			Service.DeleteProductVariantAttributeValue(entity);
		}

		[WebApiQueryable]
		public SingleResult<ProductVariantAttributeValue> GetProductVariantAttributeValue(int key)
		{
			return GetSingleResult(key);
		}

		// navigation properties

		public ProductVariantAttribute GetProductVariantAttribute(int key)
		{
			return GetExpandedProperty<ProductVariantAttribute>(key, x => x.ProductVariantAttribute);
		}
	}
}
