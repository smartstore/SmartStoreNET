using System.Web.Http;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Security;
using SmartStore.Services.Catalog;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
    public class TierPricesController : WebApiEntityController<TierPrice, IProductService>
	{
        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.EditTierPrice)]
        protected override void Insert(TierPrice entity)
		{
			Service.InsertTierPrice(entity);
		}

        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.EditTierPrice)]
        protected override void Update(TierPrice entity)
		{
			Service.UpdateTierPrice(entity);
		}

        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.EditTierPrice)]
        protected override void Delete(TierPrice entity)
		{
			Service.DeleteTierPrice(entity);
		}

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.Read)]
        public SingleResult<TierPrice> GetTierPrice(int key)
		{
			return GetSingleResult(key);
		}
	}
}
