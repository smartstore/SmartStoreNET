using SmartStore.Core.Domain.Directory;
using SmartStore.Services.Directory;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace SmartStore.WebApi.Controllers.OData
{
	[WebApiAuthenticate(Permission = "ManageCountries")]
	public class CountriesController : WebApiEntityController<Country, ICountryService>
	{
		protected override void Insert(Country entity)
		{
			Service.InsertCountry(entity);
		}
		protected override void Update(Country entity)
		{
			Service.UpdateCountry(entity);
		}
		protected override void Delete(Country entity)
		{
			Service.DeleteCountry(entity);
		}

		[WebApiQueryable]
		public SingleResult<Country> GetCountry(int key)
		{
			return GetSingleResult(key);
		}

		// navigation properties

		[WebApiQueryable]
		public IQueryable<StateProvince> GetStateProvinces(int key)
		{
			return GetRelatedCollection(key, x => x.StateProvinces);
		}
	}
}
