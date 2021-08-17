using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Security;
using SmartStore.Services.Catalog;
using SmartStore.Services.Seo;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
    [IEEE754Compatible]
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
                from x in Repository.Table
                where !x.Deleted
                select x;

            return query;
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Manufacturer.Read)]
        public IHttpActionResult Get()
        {
            return Ok(GetEntitySet());
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Manufacturer.Read)]
        public IHttpActionResult Get(int key)
        {
            return Ok(GetByKey(key));
        }

        [WebApiAuthenticate(Permission = Permissions.Catalog.Manufacturer.Read)]
        public IHttpActionResult GetProperty(int key, string propertyName)
        {
            return GetPropertyValue(key, propertyName);
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Manufacturer.Create)]
        public IHttpActionResult Post(Manufacturer entity)
        {
            var result = Insert(entity, () =>
            {
                Service.InsertManufacturer(entity);

                this.ProcessEntity(() =>
                {
                    _urlRecordService.Value.SaveSlug(entity, x => x.Name);
                });
            });

            return result;
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Manufacturer.Update)]
        public async Task<IHttpActionResult> Put(int key, Manufacturer entity)
        {
            var result = await UpdateAsync(entity, key, () =>
            {
                Service.UpdateManufacturer(entity);

                this.ProcessEntity(() =>
                {
                    _urlRecordService.Value.SaveSlug<Manufacturer>(entity, x => x.Name);
                });
            });

            return result;
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Manufacturer.Update)]
        public async Task<IHttpActionResult> Patch(int key, Delta<Manufacturer> model)
        {
            var result = await PartiallyUpdateAsync(key, model, entity =>
            {
                Service.UpdateManufacturer(entity);

                this.ProcessEntity(() =>
                {
                    _urlRecordService.Value.SaveSlug<Manufacturer>(entity, x => x.Name);
                });
            });

            return result;
        }

        [WebApiAuthenticate(Permission = Permissions.Catalog.Manufacturer.Delete)]
        public async Task<IHttpActionResult> Delete(int key)
        {
            var result = await DeleteAsync(key, entity => Service.DeleteManufacturer(entity));
            return result;
        }

        #region Navigation properties

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Manufacturer.Read)]
        public IHttpActionResult GetAppliedDiscounts(int key)
        {
            return Ok(GetRelatedCollection(key, x => x.AppliedDiscounts));
        }

        #endregion
    }
}
