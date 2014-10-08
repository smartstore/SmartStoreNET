using System.Web.Http;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Services.Catalog;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
	[WebApiAuthenticate(Permission = "ManageCatalog")]
	public class ProductAttributesController : WebApiEntityController<ProductAttribute, IProductAttributeService>
	{
		protected override void Insert(ProductAttribute entity)
		{
			Service.InsertProductAttribute(entity);
		}
		protected override void Update(ProductAttribute entity)
		{
			Service.UpdateProductAttribute(entity);
		}
		protected override void Delete(ProductAttribute entity)
		{
			Service.DeleteProductAttribute(entity);
		}

		[WebApiQueryable]
		public SingleResult<ProductAttribute> GetProductAttribute(int key)
		{
			return GetSingleResult(key);
		}
	}
}
