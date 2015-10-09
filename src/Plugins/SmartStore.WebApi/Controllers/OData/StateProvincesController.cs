using System.Web.Http;
using SmartStore.Core.Domain.Directory;
using SmartStore.Services.Directory;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
	[WebApiAuthenticate(Permission = "ManageCountries")]
	public class StateProvincesController : WebApiEntityController<StateProvince, IStateProvinceService>
	{
		protected override void Insert(StateProvince entity)
		{
			Service.InsertStateProvince(entity);
		}
		protected override void Update(StateProvince entity)
		{
			Service.UpdateStateProvince(entity);
		}
		protected override void Delete(StateProvince entity)
		{
			Service.DeleteStateProvince(entity);
		}

		[WebApiQueryable]
		public SingleResult<StateProvince> GetStateProvince(int key)
		{
			return GetSingleResult(key);
		}

		// navigation properties

		[WebApiQueryable]
		public SingleResult<Country> GetCountry(int key)
		{
			return GetRelatedEntity(key, x => x.Country);
		}
	}
}
