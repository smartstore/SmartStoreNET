using System.Web.Http;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Security;
using SmartStore.Services.Catalog;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
    public class RelatedProductsController : WebApiEntityController<RelatedProduct, IProductService>
	{
        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.EditPromotion)]
        protected override void Insert(RelatedProduct entity)
		{
			Service.InsertRelatedProduct(entity);
		}

        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.EditPromotion)]
        protected override void Update(RelatedProduct entity)
		{
			Service.UpdateRelatedProduct(entity);
		}

        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.EditPromotion)]
        protected override void Delete(RelatedProduct entity)
		{
			Service.DeleteRelatedProduct(entity);
		}

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.Read)]
        public SingleResult<RelatedProduct> GetRelatedProduct(int key)
		{
			return GetSingleResult(key);
		}
	}
}
