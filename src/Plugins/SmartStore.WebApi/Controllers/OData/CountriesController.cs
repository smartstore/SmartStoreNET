using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
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
		[WebApiQueryable]
		[WebApiAuthenticate(Permission = Permissions.Configuration.Country.Read)]
		public IQueryable<Country> Get()
		{
			return GetEntitySet();
		}

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Configuration.Country.Read)]
        public SingleResult<Country> Get(int key)
		{
			return GetSingleResult(key);
		}

		[WebApiAuthenticate(Permission = Permissions.Configuration.Country.Create)]
		public IHttpActionResult Post(Country entity)
		{
			var result = Insert(entity, () => Service.InsertCountry(entity));
			return result;
		}

		[WebApiAuthenticate(Permission = Permissions.Configuration.Country.Update)]
		public async Task<IHttpActionResult> Put(int key, Country entity)
		{
			var result = await UpdateAsync(entity, key, () => Service.UpdateCountry(entity));
			return result;
		}

		[WebApiAuthenticate(Permission = Permissions.Configuration.Country.Update)]
		public async Task<IHttpActionResult> Patch(int key, Delta<Country> model)
		{
			var result = await PartiallyUpdateAsync(key, model, entity => Service.UpdateCountry(entity));
			return result;
		}

		[WebApiAuthenticate(Permission = Permissions.Configuration.Country.Delete)]
		public async Task<IHttpActionResult> Delete(int key)
		{
			var result = await DeleteAsync(key, entity => Service.DeleteCountry(entity));
			return result;
		}

		#region Navigation properties

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Configuration.Country.Read)]
        public IQueryable<StateProvince> GetStateProvinces(int key)
		{
			return GetRelatedCollection(key, x => x.StateProvinces);
		}

		#endregion
	}
}
