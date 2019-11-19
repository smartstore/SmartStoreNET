using System.Web.Http;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Security;
using SmartStore.Services.Directory;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
    public class StateProvincesController : WebApiEntityController<StateProvince, IStateProvinceService>
	{
        [WebApiAuthenticate(Permission = Permissions.Configuration.Country.Create)]
        protected override void Insert(StateProvince entity)
		{
			Service.InsertStateProvince(entity);
		}

        [WebApiAuthenticate(Permission = Permissions.Configuration.Country.Update)]
        protected override void Update(StateProvince entity)
		{
			Service.UpdateStateProvince(entity);
		}

        [WebApiAuthenticate(Permission = Permissions.Configuration.Country.Delete)]
        protected override void Delete(StateProvince entity)
		{
			Service.DeleteStateProvince(entity);
		}

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Configuration.Country.Read)]
        public SingleResult<StateProvince> GetStateProvince(int key)
		{
			return GetSingleResult(key);
		}

		// Navigation properties.

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Configuration.Country.Read)]
        public SingleResult<Country> GetCountry(int key)
		{
			return GetRelatedEntity(key, x => x.Country);
		}
	}
}
