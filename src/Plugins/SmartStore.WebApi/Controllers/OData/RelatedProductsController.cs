using System.Web.Http;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Services.Catalog;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
	[WebApiAuthenticate(Permission = "ManageCatalog")]
	public class RelatedProductsController : WebApiEntityController<RelatedProduct, IProductService>
	{
		protected override void Insert(RelatedProduct entity)
		{
			Service.InsertRelatedProduct(entity);
		}
		protected override void Update(RelatedProduct entity)
		{
			Service.UpdateRelatedProduct(entity);
		}
		protected override void Delete(RelatedProduct entity)
		{
			Service.DeleteRelatedProduct(entity);
		}

		[WebApiQueryable]
		public SingleResult<RelatedProduct> GetRelatedProduct(int key)
		{
			return GetSingleResult(key);
		}
	}
}
