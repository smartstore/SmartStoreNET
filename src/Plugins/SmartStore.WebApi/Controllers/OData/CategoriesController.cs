using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
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

		[WebApiQueryable]
		[WebApiAuthenticate(Permission = Permissions.Catalog.Category.Read)]
		public IQueryable<Category> Get()
		{
			return GetEntitySet();
		}

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Category.Read)]
        public SingleResult<Category> Get(int key)
		{
			return GetSingleResult(key);
		}

		[WebApiAuthenticate(Permission = Permissions.Catalog.Category.Create)]
		public IHttpActionResult Post(Category entity)
		{
			var result = Insert(entity, () =>
			{
				Service.InsertCategory(entity);

				this.ProcessEntity(() =>
				{
					_urlRecordService.Value.SaveSlug(entity, x => x.Name);
				});
			});

			return result;
		}

		[WebApiAuthenticate(Permission = Permissions.Catalog.Category.Update)]
		public async Task<IHttpActionResult> Put(int key, Category entity)
		{
			var result = await UpdateAsync(entity, key, () =>
			{
				Service.UpdateCategory(entity);

				this.ProcessEntity(() =>
				{
					_urlRecordService.Value.SaveSlug(entity, x => x.Name);
				});
			});

			return result;
		}

		[WebApiAuthenticate(Permission = Permissions.Catalog.Category.Update)]
		public async Task<IHttpActionResult> Patch(int key, Delta<Category> model)
		{
			var result = await PartiallyUpdateAsync(key, model, entity =>
			{
				Service.UpdateCategory(entity);

				this.ProcessEntity(() =>
				{
					_urlRecordService.Value.SaveSlug(entity, x => x.Name);
				});
			});

			return result;
		}

		[WebApiAuthenticate(Permission = Permissions.Catalog.Category.Delete)]
		public async Task<IHttpActionResult> Delete(int key)
		{
			var result = await DeleteAsync(key, entity =>
			{
				Service.DeleteCategory(entity);
			});

			return result;
		}

		#region Navigation properties

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Catalog.Category.Read)]
        public IQueryable<Discount> GetAppliedDiscounts(int key)
		{
			return GetRelatedCollection(key, x => x.AppliedDiscounts);
		}

		#endregion
	}
}
