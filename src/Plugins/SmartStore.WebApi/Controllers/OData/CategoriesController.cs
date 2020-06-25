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
    public class CategoriesController : WebApiEntityController<Category, ICategoryService>
	{
		private readonly Lazy<IUrlRecordService> _urlRecordService;

		public CategoriesController(Lazy<IUrlRecordService> urlRecordService)
		{
			_urlRecordService = urlRecordService;
		}

		protected override IQueryable<Category> GetEntitySet()
		{
			var query =
				from x in this.Repository.Table
				where !x.Deleted
				select x;

			return query;
		}

        [WebApiAuthenticate(Permission = Permissions.Catalog.Category.Create)]
		protected override void Insert(Category entity)
		{
			Service.InsertCategory(entity);

			this.ProcessEntity(() =>
			{
				_urlRecordService.Value.SaveSlug<Category>(entity, x => x.Name);
			});
		}

        [WebApiAuthenticate(Permission = Permissions.Catalog.Category.Update)]
        protected override void Update(Category entity)
		{
			Service.UpdateCategory(entity);

			this.ProcessEntity(() =>
			{
				_urlRecordService.Value.SaveSlug<Category>(entity, x => x.Name);
			});
		}

        [WebApiAuthenticate(Permission = Permissions.Catalog.Category.Delete)]
        protected override void Delete(Category entity)
		{
			Service.DeleteCategory(entity);
		}

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Category.Read)]
        public SingleResult<Category> GetCategory(int key)
		{
			return GetSingleResult(key);
		}

		// Navigation properties.

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Category.Read)]
        public IQueryable<Discount> GetAppliedDiscounts(int key)
		{
			return GetRelatedCollection(key, x => x.AppliedDiscounts);
		}
	}
}
