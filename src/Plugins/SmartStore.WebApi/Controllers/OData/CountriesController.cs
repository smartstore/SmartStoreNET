using System.Linq;
using System.Web.Http;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Security;
using SmartStore.Services.Directory;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
    public class CountriesController : WebApiEntityController<Country, ICountryService>
	{
        [WebApiAuthenticate(Permission = Permissions.Configuration.Country.Create)]
		protected override void Insert(Country entity)
		{
			Service.InsertCountry(entity);
		}

        [WebApiAuthenticate(Permission = Permissions.Configuration.Country.Update)]
        protected override void Update(Country entity)
		{
			Service.UpdateCountry(entity);
		}

        [WebApiAuthenticate(Permission = Permissions.Configuration.Country.Delete)]
        protected override void Delete(Country entity)
		{
			Service.DeleteCountry(entity);
		}

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Configuration.Country.Read)]
        public SingleResult<Country> GetCountry(int key)
		{
			return GetSingleResult(key);
		}

		// Navigation properties.

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Configuration.Country.Read)]
        public IQueryable<StateProvince> GetStateProvinces(int key)
		{
			return GetRelatedCollection(key, x => x.StateProvinces);
		}
	}
}
