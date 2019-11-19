using System.Web.Http;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Security;
using SmartStore.Services.Directory;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
    public class CurrenciesController : WebApiEntityController<Currency, ICurrencyService>
	{
        [WebApiAuthenticate(Permission = Permissions.Configuration.Currency.Create)]
		protected override void Insert(Currency entity)
		{
			Service.InsertCurrency(entity);
		}

        [WebApiAuthenticate(Permission = Permissions.Configuration.Currency.Update)]
        protected override void Update(Currency entity)
		{
			Service.UpdateCurrency(entity);
		}

        [WebApiAuthenticate(Permission = Permissions.Configuration.Currency.Delete)]
        protected override void Delete(Currency entity)
		{
			Service.DeleteCurrency(entity);
		}

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Configuration.Currency.Read)]
        public SingleResult<Currency> GetCurrency(int key)
		{
			return GetSingleResult(key);
		}
	}
}
