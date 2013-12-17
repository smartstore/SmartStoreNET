using System.Web.Http;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Directory;
using SmartStore.Services.Common;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.Plugin.Api.WebApi.Controllers.OData
{
	[WebApiAuthenticate(Permission = "ManageCustomers")]
	public class AddressesController : WebApiEntityController<Address, IAddressService>
	{
		protected override void Insert(Address entity)
		{
			Service.InsertAddress(entity);
		}
		protected override void Update(Address entity)
		{
			Service.UpdateAddress(entity);
		}
		protected override void Delete(Address entity)
		{
			Service.DeleteAddress(entity);
		}

		[WebApiQueryable]
		public SingleResult<Address> GetAddress(int key)
		{
			return GetSingleResult(key);
		}

		// navigation properties

		public Country GetCountry(int key)
		{
			return GetExpandedProperty<Country>(key, x => x.Country);
		}

		public StateProvince GetStateProvince(int key)
		{
			return GetExpandedProperty<StateProvince>(key, x => x.StateProvince);
		}
	}
}
