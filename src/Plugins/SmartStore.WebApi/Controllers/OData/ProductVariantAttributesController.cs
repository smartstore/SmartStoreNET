using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Services.Catalog;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
	[WebApiAuthenticate(Permission = "ManageCatalog")]
	public class ProductVariantAttributesController : WebApiEntityController<ProductVariantAttribute, IProductAttributeService>
	{
		protected override void Insert(ProductVariantAttribute entity)
		{
			Service.InsertProductVariantAttribute(entity);
		}
		protected override void Update(ProductVariantAttribute entity)
		{
			Service.UpdateProductVariantAttribute(entity);
		}
		protected override void Delete(ProductVariantAttribute entity)
		{
			Service.DeleteProductVariantAttribute(entity);
		}

		[WebApiQueryable]
		public SingleResult<ProductVariantAttribute> GetProductVariantAttribute(int key)
		{
			return GetSingleResult(key);
		}

		// navigation properties

		public ProductAttribute GetProductAttribute(int key)
		{
			return GetExpandedProperty<ProductAttribute>(key, x => x.ProductAttribute);
		}

		[WebApiQueryable]
		public IQueryable<ProductVariantAttributeValue> GetProductVariantAttributeValues(int key)
		{
			var entity = GetExpandedEntity<ICollection<ProductVariantAttributeValue>>(key, x => x.ProductVariantAttributeValues);

			return entity.ProductVariantAttributeValues.AsQueryable();
		}
	}
}
