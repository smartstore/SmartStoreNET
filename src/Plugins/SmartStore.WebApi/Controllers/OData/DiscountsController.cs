using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Discounts;
using SmartStore.Services.Discounts;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace SmartStore.WebApi.Controllers.OData
{
	[WebApiAuthenticate(Permission = "ManageDiscounts")]
	public class DiscountsController : WebApiEntityController<Discount, IDiscountService>
	{
		protected override void Insert(Discount entity)
		{
			Service.InsertDiscount(entity);
		}
		protected override void Update(Discount entity)
		{
			Service.UpdateDiscount(entity);
		}
		protected override void Delete(Discount entity)
		{
			Service.DeleteDiscount(entity);
		}

		[WebApiQueryable]
		public SingleResult<Discount> GetDiscount(int key)
		{
			return GetSingleResult(key);
		}

		// navigation properties

		[WebApiQueryable]
		public IQueryable<Category> GetAppliedToCategories(int key)
		{
			return GetRelatedCollection(key, x => x.AppliedToCategories);
		}
	}
}
