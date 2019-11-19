using System;
using System.Linq;
using System.Web.Http;
using SmartStore.Core.Domain.Blogs;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Security;
using SmartStore.Services.Blogs;
using SmartStore.Services.Seo;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
    public class BlogPostsController : WebApiEntityController<BlogPost, IBlogService>
	{
		private readonly Lazy<IUrlRecordService> _urlRecordService;

		public BlogPostsController(Lazy<IUrlRecordService> urlRecordService)
		{
			_urlRecordService = urlRecordService;
		}

		protected override IQueryable<BlogPost> GetEntitySet()
		{
			var query =
				from x in this.Repository.Table
				orderby x.CreatedOnUtc descending
				select x;

			return query;
		}

        [WebApiAuthenticate(Permission = Permissions.Cms.Blog.Create)]
		protected override void Insert(BlogPost entity)
		{
			Service.InsertBlogPost(entity);

			this.ProcessEntity(() =>
			{
				_urlRecordService.Value.SaveSlug(entity, x => x.Title);
			});
		}

        [WebApiAuthenticate(Permission = Permissions.Cms.Blog.Update)]
        protected override void Update(BlogPost entity)
		{
			Service.UpdateBlogPost(entity);

			this.ProcessEntity(() =>
			{
				_urlRecordService.Value.SaveSlug(entity, x => x.Title);
			});
		}

        [WebApiAuthenticate(Permission = Permissions.Cms.Blog.Delete)]
        protected override void Delete(BlogPost entity)
		{
			Service.DeleteBlogPost(entity);
		}

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Cms.Blog.Read)]
        public SingleResult<BlogPost> GetBlogPost(int key)
		{
			return GetSingleResult(key);
		}

		// Navigation properties.

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Cms.Blog.Read)]
        public SingleResult<Language> GetLanguage(int key)
		{
			return GetRelatedEntity(key, x => x.Language);
		}

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Cms.Blog.Read)]
        public IQueryable<BlogComment> GetBlogComments(int key)
		{
			return GetRelatedCollection(key, x => x.BlogComments);
		}
	}
}