using System;
using System.Linq;
using System.Web.Http;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Discounts;
using SmartStore.Core.Security;
using SmartStore.Services.Catalog;
using SmartStore.Services.Seo;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
    public class ManufacturersController : WebApiEntityController<Manufacturer, IManufacturerService>
	{
		private readonly Lazy<IUrlRecordService> _urlRecordService;

		public ManufacturersController(Lazy<IUrlRecordService> urlRecordService)
		{
			_urlRecordService = urlRecordService;
		}

		protected override IQueryable<Manufacturer> GetEntitySet()
		{
			var query =
				from x in this.Repository.Table
				where !x.Deleted
				select x;

			return query;
		}

        [WebApiAuthenticate(Permission = Permissions.Catalog.Manufacturer.Create)]
		protected override void Insert(Manufacturer entity)
		{
			Service.InsertManufacturer(entity);

			this.ProcessEntity(() =>
			{
				_urlRecordService.Value.SaveSlug<Manufacturer>(entity, x => x.Name);
			});
		}

        [WebApiAuthenticate(Permission = Permissions.Catalog.Manufacturer.Update)]
        protected override void Update(Manufacturer entity)
		{
			Service.UpdateManufacturer(entity);

			this.ProcessEntity(() =>
			{
				_urlRecordService.Value.SaveSlug<Manufacturer>(entity, x => x.Name);
			});
		}

        [WebApiAuthenticate(Permission = Permissions.Catalog.Manufacturer.Delete)]
        protected override void Delete(Manufacturer entity)
		{
			Service.DeleteManufacturer(entity);
		}

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Manufacturer.Read)]
        public SingleResult<Manufacturer> GetManufacturer(int key)
		{
			return GetSingleResult(key);
		}

		// Navigation properties.

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Manufacturer.Read)]
        public IQueryable<Discount> GetAppliedDiscounts(int key)
		{
			return GetRelatedCollection(key, x => x.AppliedDiscounts);
		}
	}
}
