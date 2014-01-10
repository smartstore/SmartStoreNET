using SmartStore.Core.Domain.Catalog;
using SmartStore.Services.Catalog;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.Security;
using System.Linq;
using SmartStore.Web.Framework.WebApi.OData;
using System.Web.Http;

namespace SmartStore.Plugin.Api.WebApi.Controllers.OData
{
	[WebApiAuthenticate(Permission = "ManageCatalog")]
	public class ManufacturersController : WebApiEntityController<Manufacturer, IManufacturerService>
	{
		protected override IQueryable<Manufacturer> GetEntitySet()
		{
			var query =
				from x in this.Repository.Table
				where !x.Deleted
				select x;

			return query;
		}
		protected override void Insert(Manufacturer entity)
		{
			Service.InsertManufacturer(entity);
		}
		protected override void Update(Manufacturer entity)
		{
			Service.UpdateManufacturer(entity);
		}
		protected override void Delete(Manufacturer entity)
		{
			Service.DeleteManufacturer(entity);
		}

		[WebApiQueryable]
		public SingleResult<Manufacturer> GetManufacturer(int key)
		{
			return GetSingleResult(key);
		}
	}
}
