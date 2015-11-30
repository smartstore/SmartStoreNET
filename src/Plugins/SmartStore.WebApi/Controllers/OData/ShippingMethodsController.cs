using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Services.Shipping;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
	[WebApiAuthenticate(Permission = "ManageShippingSettings")]
	public class ShippingMethodsController : WebApiEntityController<ShippingMethod, IShippingService>
	{
		protected override void Insert(ShippingMethod entity)
		{
			Service.InsertShippingMethod(entity);
		}
		protected override void Update(ShippingMethod entity)
		{
			Service.UpdateShippingMethod(entity);
		}
		protected override void Delete(ShippingMethod entity)
		{
			Service.DeleteShippingMethod(entity);
		}

		[WebApiQueryable]
		public SingleResult<ShippingMethod> GetShippingMethod(int key)
		{
			return GetSingleResult(key);
		}

		// navigation properties

		[WebApiQueryable]
		public IQueryable<Country> GetRestrictedCountries(int key)
		{
			var entity = GetExpandedEntity<ICollection<Country>>(key, x => x.RestrictedCountries);

			return entity.RestrictedCountries.AsQueryable();
		}
	}
}
