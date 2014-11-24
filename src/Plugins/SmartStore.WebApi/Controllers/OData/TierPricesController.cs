using System.Web.Http;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Services.Catalog;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
	[WebApiAuthenticate(Permission = "ManageCatalog")]
	public class TierPricesController : WebApiEntityController<TierPrice, IProductService>
	{
		protected override void Insert(TierPrice entity)
		{
			Service.InsertTierPrice(entity);
		}
		protected override void Update(TierPrice entity)
		{
			Service.UpdateTierPrice(entity);
		}
		protected override void Delete(TierPrice entity)
		{
			Service.DeleteTierPrice(entity);
		}

		[WebApiQueryable]
		public SingleResult<TierPrice> GetTierPrice(int key)
		{
			return GetSingleResult(key);
		}
	}
}
