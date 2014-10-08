using System.Web.Http;
using SmartStore.Core.Domain.Directory;
using SmartStore.Services.Directory;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
	[WebApiAuthenticate(Permission = "ManageCurrencies")]
	public class CurrenciesController : WebApiEntityController<Currency, ICurrencyService>
	{
		protected override void Insert(Currency entity)
		{
			Service.InsertCurrency(entity);
		}
		protected override void Update(Currency entity)
		{
			Service.UpdateCurrency(entity);
		}
		protected override void Delete(Currency entity)
		{
			Service.DeleteCurrency(entity);
		}

		[WebApiQueryable]
		public SingleResult<Currency> GetCurrency(int key)
		{
			return GetSingleResult(key);
		}
	}
}
