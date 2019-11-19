using System;
using System.Linq;
using System.Web.Http;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Blogs;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Security;
using SmartStore.Services.Blogs;
using SmartStore.Services.Customers;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;

namespace SmartStore.WebApi.Controllers.OData
{
    public class BlogCommentsController : WebApiEntityController<BlogComment, ICustomerContentService>
	{
		private readonly IRepository<CustomerContent> _contentRepository;
		private readonly Lazy<IBlogService> _blogService;

		public BlogCommentsController(
			IRepository<CustomerContent> contentRepository,
			Lazy<IBlogService> blogService)
		{
			_contentRepository = contentRepository;
			_blogService = blogService;
		}

		private void FulfillCrudOperation(BlogComment entity)
		{
			this.ProcessEntity(() =>
			{
				var blogPost = _blogService.Value.GetBlogPostById(entity.BlogPostId);

				_blogService.Value.UpdateCommentTotals(blogPost);
			});
		}

		protected override IQueryable<BlogComment> GetEntitySet()
		{
			var query = _contentRepository.Table
				.OrderByDescending(c => c.CreatedOnUtc)
				.OfType<BlogComment>();

			return query;
		}

        [WebApiAuthenticate(Permission = Permissions.Cms.Blog.Create)]
		protected override void Insert(BlogComment entity)
		{
			Service.InsertCustomerContent(entity);

			FulfillCrudOperation(entity);
		}

        [WebApiAuthenticate(Permission = Permissions.Cms.Blog.Update)]
        protected override void Update(BlogComment entity)
		{
			Service.UpdateCustomerContent(entity);

			FulfillCrudOperation(entity);
		}

        [WebApiAuthenticate(Permission = Permissions.Cms.Blog.Delete)]
        protected override void Delete(BlogComment entity)
		{
			Service.DeleteCustomerContent(entity);

			FulfillCrudOperation(entity);
		}

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Cms.Blog.Read)]
        public SingleResult<BlogComment> GetBlogComment(int key)
		{
			return GetSingleResult(key);
		}

		// Navigation properties.

		[WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Cms.Blog.Read)]
        public SingleResult<BlogPost> GetBlogPost(int key)
		{
			return GetRelatedEntity(key, x => x.BlogPost);
		}
	}
}