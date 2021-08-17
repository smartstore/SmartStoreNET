using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using SmartStore.Core.Domain.Discounts;
using SmartStore.Core.Security;
using SmartStore.Services.Discounts;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
    [IEEE754Compatible]
    public class DiscountsController : WebApiEntityController<Discount, IDiscountService>
    {
        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Promotion.Discount.Read)]
        public IHttpActionResult Get()
        {
            return Ok(GetEntitySet());
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Promotion.Discount.Read)]
        public IHttpActionResult Get(int key)
        {
            return Ok(GetByKey(key));
        }

        [WebApiAuthenticate(Permission = Permissions.Promotion.Discount.Read)]
        public IHttpActionResult GetProperty(int key, string propertyName)
        {
            return GetPropertyValue(key, propertyName);
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Promotion.Discount.Create)]
        public IHttpActionResult Post(Discount entity)
        {
            var result = Insert(entity, () => Service.InsertDiscount(entity));
            return result;
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Promotion.Discount.Update)]
        public async Task<IHttpActionResult> Put(int key, Discount entity)
        {
            var result = await UpdateAsync(entity, key, () => Service.UpdateDiscount(entity));
            return result;
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Promotion.Discount.Update)]
        public async Task<IHttpActionResult> Patch(int key, Delta<Discount> model)
        {
            var result = await PartiallyUpdateAsync(key, model, entity => Service.UpdateDiscount(entity));
            return result;
        }

        [WebApiAuthenticate(Permission = Permissions.Promotion.Discount.Delete)]
        public async Task<IHttpActionResult> Delete(int key)
        {
            var result = await DeleteAsync(key, entity => Service.DeleteDiscount(entity));
            return result;
        }

        #region Navigation properties

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Category.Read)]
        public IHttpActionResult GetAppliedToCategories(int key)
        {
            return Ok(GetRelatedCollection(key, x => x.AppliedToCategories));
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Manufacturer.Read)]
        public IHttpActionResult GetAppliedToManufacturers(int key)
        {
            return Ok(GetRelatedCollection(key, x => x.AppliedToManufacturers));
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.Read)]
        public IHttpActionResult GetAppliedToProducts(int key)
        {
            return Ok(GetRelatedCollection(key, x => x.AppliedToProducts));
        }

        #endregion
    }
}
