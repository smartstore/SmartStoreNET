using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
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
    [IEEE754Compatible]
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

        protected override IQueryable<BlogComment> GetEntitySet()
        {
            var query = _contentRepository.Table
                .OrderByDescending(c => c.CreatedOnUtc)
                .OfType<BlogComment>();

            return query;
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Cms.Blog.Read)]
        public IHttpActionResult Get()
        {
            return Ok(GetEntitySet());
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Cms.Blog.Read)]
        public IHttpActionResult Get(int key)
        {
            return Ok(GetByKey(key));
        }

        [WebApiAuthenticate(Permission = Permissions.Cms.Blog.Read)]
        public IHttpActionResult GetProperty(int key, string propertyName)
        {
            return GetPropertyValue(key, propertyName);
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Cms.Blog.Create)]
        public IHttpActionResult Post(BlogComment entity)
        {
            var result = Insert(entity, () =>
            {
                Service.InsertCustomerContent(entity);
                UpdateCommentTotals(entity);
            });

            return result;
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Cms.Blog.Update)]
        public async Task<IHttpActionResult> Put(int key, BlogComment entity)
        {
            var result = await UpdateAsync(entity, key, () =>
            {
                Service.UpdateCustomerContent(entity);

                // Actually not necessary, but does not hurt in terms of synchronization.
                UpdateCommentTotals(entity);
            });

            return result;
        }

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Cms.Blog.Update)]
        public async Task<IHttpActionResult> Patch(int key, Delta<BlogComment> model)
        {
            var result = await PartiallyUpdateAsync(key, model, entity =>
            {
                Service.UpdateCustomerContent(entity);

                // Actually not necessary, but does not hurt in terms of synchronization.
                UpdateCommentTotals(entity);
            });

            return result;
        }

        [WebApiAuthenticate(Permission = Permissions.Cms.Blog.Delete)]
        public async Task<IHttpActionResult> Delete(int key)
        {
            var result = await DeleteAsync(key, entity =>
            {
                Service.DeleteCustomerContent(entity);
                UpdateCommentTotals(entity);
            });

            return result;
        }

        #region Navigation properties

        [WebApiQueryable]
        [WebApiAuthenticate(Permission = Permissions.Cms.Blog.Read)]
        public IHttpActionResult GetBlogPost(int key)
        {
            return Ok(GetRelatedEntity(key, x => x.BlogPost));
        }

        #endregion

        private void UpdateCommentTotals(BlogComment entity)
        {
            this.ProcessEntity(() =>
            {
                var blogPost = _blogService.Value.GetBlogPostById(entity.BlogPostId);

                _blogService.Value.UpdateCommentTotals(blogPost);
            });
        }
    }
}