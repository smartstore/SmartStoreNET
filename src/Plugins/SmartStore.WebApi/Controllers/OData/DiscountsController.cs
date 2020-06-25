using System.Linq;
using System.Web.Http;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Discounts;
using SmartStore.Core.Security;
using SmartStore.Services.Discounts;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
    public class DiscountsController : WebApiEntityController<Discount, IDiscountService>
	{
        [WebApiAuthenticate(Permission = Permissions.Promotion.Discount.Create)]
		protected override void Insert(Discount entity)
		{
			Service.InsertDiscount(entity);
		}

        [WebApiAuthenticate(Permission = Permissions.Promotion.Discount.Update)]
        protected override void Update(Discount entity)
		{
			Service.UpdateDiscount(entity);
		}

        [WebApiAuthenticate(Permission = Permissions.Promotion.Discount.Delete)]
        protected override void Delete(Discount entity)
		{
			Service.DeleteDiscount(entity);
		}

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Promotion.Discount.Read)]
        public SingleResult<Discount> GetDiscount(int key)
		{
			return GetSingleResult(key);
		}

		// Navigation properties.

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Category.Read)]
        public IQueryable<Category> GetAppliedToCategories(int key)
		{
			return GetRelatedCollection(key, x => x.AppliedToCategories);
		}

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Manufacturer.Read)]
        public IQueryable<Manufacturer> GetAppliedToManufacturers(int key)
        {
            return GetRelatedCollection(key, x => x.AppliedToManufacturers);
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Product.Read)]
        public IQueryable<Product> GetAppliedToProducts(int key)
        {
            return GetRelatedCollection(key, x => x.AppliedToProducts);
        }
    }
}
